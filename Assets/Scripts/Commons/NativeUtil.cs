using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Util
{

    public static class NativeUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToMultiHashMap<K, V, T>(this ref DynamicBuffer<T> elements, out NativeMultiHashMap<K, V> output, int capacity, Allocator allocator, Func<T, K> keySelector, Func<T, V> valueSelector) where K : struct, IEquatable<K> where V : struct where T : struct
        {
            output = new NativeMultiHashMap<K, V>(capacity, allocator);
            for (int i = 0; i < elements.Length; i++)
            {

                output.Add(keySelector(elements[i]), valueSelector(elements[i]));
            }

        }

        public static bool AddIfMissing<K, V>(this ref NativeMultiHashMap<K, V> map, K key, V value) where K : struct, IEquatable<K> where V : struct, IEquatable<V>
        {
            var enumerator = map.GetValuesForKey(key);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Equals(value))
                    return false;
            }
            map.Add(key, value);
            return true;
        }


        public static bool ContentEquals<K, V>(this ref NativeMultiHashMap<K, V> self, ref NativeMultiHashMap<K, V> other) where K : struct, IEquatable<K> where V : struct, IEquatable<V>
        {

            if (!other.IsCreated)
                return self.IsCreated == other.IsCreated;
            var selfVA = self.GetValueArray(Allocator.TempJob);
            var otherVA = other.GetValueArray(Allocator.TempJob);
            bool result = selfVA.ArraysEqual(otherVA);
            selfVA.Dispose();
            otherVA.Dispose();
            return result;

        }
        public static bool Any<T>(this ref NativeArray<T> self, Func<T, bool> tester) where T : struct
        {
            if (!self.IsCreated)
                return false;
            for (int i = 0; i < self.Length; i++)
            {
                if (tester.Invoke(self[i]))
                    return true;
            }
            return false;
        }
        public static bool Any<T, Arg1>(this ref NativeArray<T> self, ref Arg1 arg1, RefFunc<T, Arg1, bool> tester) where T : struct where Arg1 : struct
        {
            if (!self.IsCreated)
                return false;
            T value;
            for (int i = 0; i < self.Length; i++)
            {
                value = self[i];
                if (tester.Invoke(ref value, ref arg1))
                    return true;
            }
            return false;
        }
        public static bool Any<T, Arg1, Arg2>(this ref NativeArray<T> self, ref Arg1 arg1, ref Arg2 arg2, RefFunc<T, Arg1, Arg2, bool> tester) where T : struct where Arg1 : struct where Arg2 : struct
        {
            if (!self.IsCreated)
                return false;
            T value;
            for (int i = 0; i < self.Length; i++)
            {
                value = self[i];
                if (tester.Invoke(ref value, ref arg1, ref arg2))
                    return true;
            }
            return false;
        }

        public delegate TResult RefFunc<T1, out TResult>(ref T1 arg1) where T1 : struct;
        public delegate TResult RefFunc<T1, T2, out TResult>(ref T1 arg1, ref T2 arg2) where T1 : struct where T2 : struct;
        public delegate TResult RefFunc<T1, T2, T3, out TResult>(ref T1 arg1, ref T2 arg2, ref T3 arg3) where T1 : struct where T2 : struct where T3 : struct;
    }
}