using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;

namespace Reactics.Util
{
    public interface IMutableExchangeable<T>
    {
        T AsMutable();

        T AsImmutable();
    }
    public static class GeneralUtils
    {
        public delegate void ApplyDelegate<T>(T target);
        public delegate R LetDelegate<T, R>(T target);
        public static T Apply<T>(this T target, ApplyDelegate<T> apply)
        {
            apply.Invoke(target);
            return target;
        }
        public static R Let<T, R>(this T target, LetDelegate<T, R> let)
        {
            return let.Invoke(target);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Find<T>(this IEnumerable<T> target, out T output, Func<T, bool> testFunc)
        {

            foreach (var item in target)
            {
                if (testFunc(item))
                {
                    output = item;
                    return true;
                }
            }
            output = default;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Find<T>(this DynamicBuffer<T> target, out T output, Func<T, bool> testFunc) where T : struct
        {
            for (int i = 0; i < target.Length; i++)
            {
                if (testFunc(target[i]))
                {
                    output = target[i];
                    return true;
                }
            }
            output = default;
            return false;
        }
    }

    public static class MathUtils
    {
        public static int FloorMod(int x, int y)
        {
            return x - FloorDiv(x, y) * y;
        }
        public static int FloorDiv(int x, int y)
        {
            int r = x / y;
            if ((x ^ y) < 0 && (r * y != x))
                r--;
            return r;
        }
    }

}