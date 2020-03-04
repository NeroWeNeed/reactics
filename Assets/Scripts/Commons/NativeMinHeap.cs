using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Reactics.Commons
{


    [NativeContainer]

    public unsafe struct NativeHeap<T> : IDisposable where T : struct, IComparable<T>
    {
        internal void* arr;
        internal long arrSize;

        private int allocatedLength;
        private float loadFactor;

        private Allocator allocator;


        public int Length { get; private set; }

        public NativeHeap(int initialSize, float loadFactor, Allocator allocator = Allocator.Temp)
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

        }
        public void Add(T value)
        {
            if (Length + 1 > allocatedLength * loadFactor)
            {

            }
        }

        private void IncreaseAllocation()
        {
            int newAllocatedLength = allocatedLength * 2;
            int newArrSize = UnsafeUtility.SizeOf<T>() * newAllocatedLength;
            void* newArr = UnsafeUtility.Malloc(newArrSize, UnsafeUtility.AlignOf<T>(), allocator);

            
            UnsafeUtility.MemMove(newArr, arr, arrSize);
            UnsafeUtility.Free(arr, allocator);
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

        public void Remove() {
            
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}