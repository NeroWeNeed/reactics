using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Reactics.Core.Commons.Collections {
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeHeapDebugView<>))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeSortedSet<T> : IDisposable where T : struct, IEquatable<T>, IComparable<T> {
        internal IntPtr m_Buffer;
        internal int m_Capacity;
        internal float m_LoadingFactor;
        internal int m_Length;
        public bool IsCreated { get => m_Buffer != IntPtr.Zero; }
        public int Length { get => m_Length; }
        public sbyte m_Ascending;
        public bool IsAscending { get => m_Ascending > 0; }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal int m_MinIndex;
        internal int m_MaxIndex;
        internal AtomicSafetyHandle m_Safety;
        internal DisposeSentinel m_DisposeSentinel;
#endif

        internal Allocator m_Allocator;

        public NativeSortedSet(int initialCapacity, float loadFactor = 0.75f, bool ascending = true, Allocator allocator = Allocator.Temp) {
            long totalSize = UnsafeUtility.SizeOf<T>() * initialCapacity;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", "allocator");
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException("initialCapacity", "Length must be >= 0");
            if (!UnsafeUtility.IsBlittable<T>())
                throw new ArgumentException(string.Format("{0} used in NativeHeap<{0}> must be blittable", typeof(T)));
#endif
            m_Buffer = (IntPtr)UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear((void*)m_Buffer, totalSize);

            m_Length = 0;
            m_Capacity = initialCapacity;
            m_Allocator = allocator;
            m_LoadingFactor = loadFactor;
            m_Ascending = (sbyte)(ascending ? -1 : 1);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_MinIndex = 0;
            m_MaxIndex = initialCapacity - 1;
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);

#endif
        }
        [WriteAccessRequired]
        public bool Add(T value) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

            if (m_Length >= m_Capacity * 0.75f) {
                Expand(m_Capacity * 2);
            }
            IntPtr pointer;
            if (m_Length == 0) {
                pointer = m_Buffer;
            }
            else {
                BinarySearch(0, m_Length - 1, value, out pointer, out int index);
                if (pointer == IntPtr.Zero || index == -1)
                    return false;
                if (index < m_Length)
                    UnsafeUtility.MemMove((pointer + UnsafeUtility.SizeOf<T>()).ToPointer(), pointer.ToPointer(), (m_Length - index) * UnsafeUtility.SizeOf<T>());
            }
            UnsafeUtility.CopyStructureToPtr(ref value, pointer.ToPointer());
            m_Length++;
            return true;
        }
        private void BinarySearch(int lower, int upper, T value, out IntPtr address, out int index) {
            if (upper <= lower) {
                var targetAddr = m_Buffer + lower * UnsafeUtility.SizeOf<T>();

                UnsafeUtility.CopyPtrToStructure(targetAddr.ToPointer(), out T temp);
                if (temp.Equals(value)) {
                    address = IntPtr.Zero;
                    index = -1;
                }
                else if (Compare(temp, value) > 0) {
                    address = targetAddr + UnsafeUtility.SizeOf<T>();
                    index = lower + 1;
                }
                else {
                    address = targetAddr;
                    index = lower;
                }
            }
            else {
                int targetIndex = (lower + upper) / 2;
                var targetAddr = m_Buffer + targetIndex * UnsafeUtility.SizeOf<T>();
                UnsafeUtility.CopyPtrToStructure(targetAddr.ToPointer(), out T temp);
                if (temp.Equals(value)) {
                    address = IntPtr.Zero;
                    index = -1;
                }
                else if (Compare(temp, value) == 0) {
                    address = targetAddr + UnsafeUtility.SizeOf<T>();
                    index = targetIndex + 1;
                }
                else if (Compare(temp, value) > 0) {
                    BinarySearch(targetIndex + 1, upper, value, out address, out index);
                }
                else {
                    BinarySearch(lower, targetIndex - 1, value, out address, out index);
                }
            }
        }
        private int Compare(T value1, T value2) {
            return value1.CompareTo(value2) * m_Ascending;
        }
        private void Expand(int capacity) {

            if (capacity == m_Capacity)
                return;
            if (capacity < m_Capacity)
                throw new Exception("New Capacity cannot be less than current capacity");
            var newTotalSize = NextHigherPowerOfTwo(capacity * UnsafeUtility.SizeOf<T>());

            var newBuffer = (IntPtr)UnsafeUtility.Malloc(newTotalSize, JobsUtility.CacheLineSize, m_Allocator);
            UnsafeUtility.MemCpy(newBuffer.ToPointer(), m_Buffer.ToPointer(), UnsafeUtility.SizeOf<T>() * m_Length);
            UnsafeUtility.MemSet(m_Buffer.ToPointer(), 0, UnsafeUtility.SizeOf<T>() * m_Length);
            UnsafeUtility.Free(m_Buffer.ToPointer(), m_Allocator);
            m_Buffer = newBuffer;
            m_Capacity = capacity;


        }
        private static int NextHigherPowerOfTwo(int val) {
            val -= 1;
            val |= val >> 1;
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            return val + 1;
        }

        public T Pop() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (m_Length <= 0)
                throw new Exception("NativeHeap is empty");
            UnsafeUtility.CopyPtrToStructure(m_Buffer.ToPointer(), out T temp);
            UnsafeUtility.MemCpy(m_Buffer.ToPointer(), (m_Buffer + UnsafeUtility.SizeOf<T>()).ToPointer(), (m_Length - 1) * UnsafeUtility.SizeOf<T>());
            m_Length--;

            return temp;
        }

        public bool TryPop(out T value) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (m_Length <= 0) {
                value = default;
                return false;
            }
            UnsafeUtility.CopyPtrToStructure(m_Buffer.ToPointer(), out value);
            UnsafeUtility.MemCpy(m_Buffer.ToPointer(), (m_Buffer + UnsafeUtility.SizeOf<T>()).ToPointer(), (m_Length - 1) * UnsafeUtility.SizeOf<T>());
            m_Length--;
            return true;
        }
        public T Peek() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (m_Length <= 0)
                throw new Exception("NativeHeap is Empty");
            UnsafeUtility.CopyPtrToStructure(m_Buffer.ToPointer(), out T temp);
            return temp;
        }
        public bool TryPeek(out T value) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (m_Length <= 0) {
                value = default;
                return false;
            }

            UnsafeUtility.CopyPtrToStructure(m_Buffer.ToPointer(), out value);
            return true;
        }
        public T[] ToArray() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            var result = new T[m_Length];
            for (int i = 0; i < m_Length; i++) {
                UnsafeUtility.CopyPtrToStructure((m_Buffer + (i * UnsafeUtility.SizeOf<T>())).ToPointer(), out result[i]);
            }
            return result;
        }
        public void Clear() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            UnsafeUtility.MemSet(m_Buffer.ToPointer(), 0, UnsafeUtility.SizeOf<T>() * m_Length);
            m_Length = 0;
        }
        public bool Remove(T value) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (m_Length <= 0)
                return false;

            for (int i = 0; i < m_Length; i++) {
                if (UnsafeUtility.ReadArrayElement<T>(m_Buffer.ToPointer(), i).Equals(value)) {
                    if (i < m_Length - 1) {
                        UnsafeUtility.MemMove((m_Buffer + (i * UnsafeUtility.SizeOf<T>())).ToPointer(), (m_Buffer + ((i + 1) * UnsafeUtility.SizeOf<T>())).ToPointer(), (m_Length - i - 1) * UnsafeUtility.SizeOf<T>());
                    }
                    else {
                        UnsafeUtility.MemSet((m_Buffer + (i * UnsafeUtility.SizeOf<T>())).ToPointer(), 0, UnsafeUtility.SizeOf<T>());
                    }
                    m_Length--;
                    return true;
                }
            }
            return false;
        }
        public bool Contains(T value) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (m_Length <= 0)
                return false;

            for (int i = 0; i < m_Length; i++) {
                if (UnsafeUtility.ReadArrayElement<T>(m_Buffer.ToPointer(), i).Equals(value)) {

                    return true;
                }
            }
            return false;
        }
        public void Dispose() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            UnsafeUtility.Free((void*)m_Buffer, m_Allocator);
            m_Buffer = IntPtr.Zero;
            m_Length = 0;
            m_Capacity = 0;
        }
    }
    internal sealed class NativeHeapDebugView<T> where T : struct, IEquatable<T>, IComparable<T> {
        private NativeSortedSet<T> m_Heap;

        public NativeHeapDebugView(NativeSortedSet<T> heap) {
            m_Heap = heap;
        }

        public T[] Items
        {
            get { return m_Heap.ToArray(); }
        }
    }
}