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
    }
}