using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Reactics.Commons
{


    [NativeContainer]
    [StructLayout(LayoutKind.Sequential)]

    public unsafe struct NativeHeap<T> : IDisposable where T : struct, IComparable<T>
    {
        [NativeDisableUnsafePtrRestriction]
        internal void* arr;
        internal long arrSize;


        internal int allocatedLength;
        private float loadFactor;

        private Allocator allocator;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle safetyHandle;

        [NativeSetClassTypeToNullOnSchedule]
        internal DisposeSentinel disposeSentinel;
#endif

        public int Length { get; private set; }

        public NativeHeap(int initialSize, float loadFactor, Allocator allocator)
        {
            allocatedLength = nextPow2(initialSize);
            arrSize = UnsafeUtility.SizeOf<T>() * allocatedLength;
            this.loadFactor = loadFactor;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", "allocator");
            if (allocatedLength < 0)
                throw new ArgumentOutOfRangeException("initialSize", "InitialSize must be >= 0");
            if (!UnsafeUtility.IsBlittable<T>())
                throw new ArgumentException(string.Format("{0} used in NativeHeap<{0}> must be blittable", typeof(T)));
#endif

            arr = UnsafeUtility.Malloc(arrSize, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(arr, arrSize);
            Length = 0;
            this.allocator = allocator;
#if ENABLE_UNITY_COLLECTIONS_CHECKS

            DisposeSentinel.Create(out safetyHandle, out disposeSentinel, 0, allocator);
#endif

        }

        public NativeHeap(Allocator allocator = Allocator.Temp) : this(8, 0.75f, allocator)
        {

        }


        public void Add(T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(safetyHandle);
#endif

            if (Length + 1 > allocatedLength * loadFactor)
            {
                IncreaseAllocation();
            }
            UnsafeUtility.WriteArrayElement(arr, Length, value);
            int index = Length;
            Length++;

            while (index != 0 && UnsafeUtility.ReadArrayElement<T>(arr, (index - 1) / 2).CompareTo(value) > 0)
            {
                SwapParent(index);
                index = (index - 1) / 2;
            }


        }

        private void SwapParent(int index)
        {
            Swap(index, (index - 1) / 2);
        }
        private void Swap(int index1, int index2)
        {
            T parentTemp = UnsafeUtility.ReadArrayElement<T>(arr, index2);
            T temp = UnsafeUtility.ReadArrayElement<T>(arr, index1);
            UnsafeUtility.WriteArrayElement(arr, index1, parentTemp);
            UnsafeUtility.WriteArrayElement(arr, index2, temp);

        }
        public T Peek()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(safetyHandle);

#endif

            return UnsafeUtility.ReadArrayElement<T>(arr, 0);
        }
        public bool Peek(out T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(safetyHandle);

#endif
            if (IsCreated && Length > 0)
            {
                value = UnsafeUtility.ReadArrayElement<T>(arr, 0);
                return true;
            }
            else
            {
                value = default;
                return false;
            }


        }
        public T Pop()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(safetyHandle);
#endif
            T value = UnsafeUtility.ReadArrayElement<T>(arr, 0);

            UnsafeUtility.WriteArrayElement(arr, 0, UnsafeUtility.ReadArrayElement<T>(arr, Length - 1));

            Length -= 1;
            HeapSort(0);

            return value;
        }
        public bool Pop(out T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(safetyHandle);
#endif
            if (IsCreated && Length > 0)
            {
                value = UnsafeUtility.ReadArrayElement<T>(arr, 0);

                UnsafeUtility.WriteArrayElement(arr, 0, UnsafeUtility.ReadArrayElement<T>(arr, Length - 1));

                Length -= 1;
                HeapSort(0);

                return true;
            }
            else
            {
                value = default;
                return false;
            }

        }

        private void HeapSort(int index)
        {
            int l = 2 * index + 1;
            int r = 2 * index + 2;
            int smallest = index;
            if (l < Length && UnsafeUtility.ReadArrayElement<T>(arr, l).CompareTo(UnsafeUtility.ReadArrayElement<T>(arr, index)) < 0)
            {
                smallest = l;
            }
            if (r < Length && UnsafeUtility.ReadArrayElement<T>(arr, r).CompareTo(UnsafeUtility.ReadArrayElement<T>(arr, smallest)) < 0)
            {
                smallest = r;
            }
            if (smallest != index)
            {
                Swap(index, smallest);
                HeapSort(smallest);
            }
        }
        public T GetT(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(safetyHandle);
#endif
            return UnsafeUtility.ReadArrayElement<T>(arr, index);
        }
        public bool IsCreated { get => arr != null; }

        private void IncreaseAllocation()
        {
            int newAllocatedLength = allocatedLength * 2;
            int newArrSize = UnsafeUtility.SizeOf<T>() * newAllocatedLength;
            void* newArr = UnsafeUtility.Malloc(newArrSize, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(newArr, newArrSize);

            UnsafeUtility.MemMove(newArr, arr, arrSize);
            UnsafeUtility.MemClear(arr, arrSize);
            UnsafeUtility.Free(arr, allocator);
            allocatedLength = newAllocatedLength;
            arrSize = newArrSize;
            arr = newArr;

        }

        private static int nextPow2(int x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x |= x >> 32;
            return ++x;
        }

        public void Clear()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(safetyHandle);
#endif
            UnsafeUtility.MemClear(arr, arrSize);
            Length = 0;

        }

        public void Dispose()
        {
            UnsafeUtility.MemClear(arr, arrSize);
            UnsafeUtility.Free(arr, allocator);
            Length = 0;
        }
    }
}