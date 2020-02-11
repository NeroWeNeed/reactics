using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

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
    }
}