using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AOT;
using Reactics.Commons;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Reactics.UI
{

    public delegate float ValueConverter(float value, IntPtr info, ValueConverterHint hint);

    public struct ValueInfo
    {
        public float parentWidth;

        public float parentHeight;

        public float deadWidth, deadHeight;

        public int siblingCount;


    }
    [Flags]
    public enum ValueConverterHint
    {
        NONE = 0, USE_WIDTH = 1, USE_HEIGHT = 2, IGNORE_DEADSPACE = 4
    }
    [Serializable]
    public struct ValueRef
    {
        public float value;
        public string unit;

        public static implicit operator Value(ValueRef reference) => new Value(reference.value, ValueUtils.GetConverterAsPointer(reference.unit));
    }

    public struct Value
    {
        public static readonly Value Uniform = ValueUtils.Uniform();
        public static readonly Value Inherit = ValueUtils.Inherit();
        public unsafe float Get(ValueInfo info, ValueConverterHint hint = ValueConverterHint.NONE)
        {
            if (converter.IsCreated)
                return converter.Invoke(value, (IntPtr)(&info), hint);
            else
                return 0;
        }



        public float value;



        private readonly FunctionPointer<ValueConverter> converter;

        public Value(float value, FunctionPointer<ValueConverter> converter)
        {
            this.converter = converter;
            this.value = value;


        }


    }

    public abstract class ValueConverterReferences
    {
        public static readonly SharedStatic<float> ScreenDPI = SharedStatic<float>.GetOrCreate<ValueConverterReferences, ScreenDPIKey>();

        public static readonly SharedStatic<float> ScreenWidth = SharedStatic<float>.GetOrCreate<ValueConverterReferences, ScreenWidthKey>();

        public static readonly SharedStatic<float> ScreenHeight = SharedStatic<float>.GetOrCreate<ValueConverterReferences, ScreenHeightKey>();

        public static readonly SharedStatic<float> WindowWidth = SharedStatic<float>.GetOrCreate<ValueConverterReferences, WindowWidthKey>();

        public static readonly SharedStatic<float> WindowHeight = SharedStatic<float>.GetOrCreate<ValueConverterReferences, WindowHeightKey>();




        public static readonly SharedStatic<float> PercentageReference = SharedStatic<float>.GetOrCreate<ValueConverterReferences, PercentageReferenceKey>();

        private class ScreenDPIKey { }
        private class ScreenWidthKey { }
        private class ScreenHeightKey { }

        private class WindowWidthKey { }
        private class WindowHeightKey { }
        private class PercentageReferenceKey { }
        public static void UpdateReferenceValues(Camera uiCamera)
        {

            ScreenDPI.Data = Screen.dpi;
            ScreenWidth.Data = Screen.currentResolution.width;
            ScreenHeight.Data = Screen.currentResolution.height;
            WindowWidth.Data = Screen.width;
            WindowHeight.Data = Screen.height;
            uiCamera.orthographicSize = Screen.currentResolution.height / 2f;

        }
    }



    [BurstCompile]
    public static class ValueConverters
    {



        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Pixel(float value, IntPtr info, ValueConverterHint hint) => value;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Inch(float value, IntPtr info, ValueConverterHint hint) => value * ValueConverterReferences.ScreenDPI.Data;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Centimeter(float value, IntPtr info, ValueConverterHint hint) => value * ValueConverterReferences.ScreenDPI.Data / 2.54f;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Millimeter(float value, IntPtr info, ValueConverterHint hint) => value * ValueConverterReferences.ScreenDPI.Data / 25.4f;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Point(float value, IntPtr info, ValueConverterHint hint) => value / 72f * ValueConverterReferences.ScreenDPI.Data;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Pica(float value, IntPtr info, ValueConverterHint hint) => value * 12f / 72f * ValueConverterReferences.ScreenDPI.Data;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Vw(float value, IntPtr info, ValueConverterHint hint) => value * ValueConverterReferences.ScreenWidth.Data * 0.01f;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Vh(float value, IntPtr info, ValueConverterHint hint) => value * ValueConverterReferences.ScreenHeight.Data * 0.01f;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Vmin(float value, IntPtr info, ValueConverterHint hint) => value * (ValueConverterReferences.ScreenWidth.Data < ValueConverterReferences.ScreenHeight.Data ? ValueConverterReferences.ScreenWidth.Data : ValueConverterReferences.ScreenHeight.Data) * 0.01f;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static float Vmax(float value, IntPtr info, ValueConverterHint hint) => value * (ValueConverterReferences.ScreenWidth.Data > ValueConverterReferences.ScreenHeight.Data ? ValueConverterReferences.ScreenWidth.Data : ValueConverterReferences.ScreenHeight.Data) * 0.01f;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static unsafe float Percentage(float value, IntPtr info, ValueConverterHint hint)
        {


            UnsafeUtility.CopyPtrToStructure((void*)info, out ValueInfo _info);
            switch (hint)
            {
                case ValueConverterHint.NONE:
                    return 0f;
                case ValueConverterHint.USE_WIDTH:
                    return _info.parentWidth * value;
                case ValueConverterHint.USE_HEIGHT:

                    return _info.parentHeight * value;
                default:
                    return 0f;
            }
        }
        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static unsafe float Uniform(float value, IntPtr ptr, ValueConverterHint hint)
        {
            UnsafeUtility.CopyPtrToStructure((void*)ptr, out ValueInfo _info);

            switch (hint)
            {
                case ValueConverterHint.NONE:
                    return 0f;
                case ValueConverterHint.USE_WIDTH:
                    return (_info.parentWidth - _info.deadWidth) / _info.siblingCount;
                case ValueConverterHint.USE_HEIGHT:
                    return (_info.parentHeight - _info.deadHeight) / _info.siblingCount;
                default:
                    return 0f;
            }
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(ValueConverter))]
        public static unsafe float Inherit(float value, IntPtr info, ValueConverterHint hint)
        {
            UnsafeUtility.CopyPtrToStructure((void*)info, out ValueInfo _info);
            switch (hint)
            {
                case ValueConverterHint.NONE:
                    return 0f;
                case ValueConverterHint.USE_WIDTH:
                    return _info.parentWidth;
                case ValueConverterHint.USE_HEIGHT:
                    return _info.parentHeight;
                default:
                    return 0f;
            }
        }


    }
    public enum ValueUnit
    {
        Pixel, Point, Inch, Pica, ViewportWidth, ViewportHeight, ViewportMinimum, ViewportMaximum, Percentage, Centimeter, Millimeter, Uniform, Inherit
    }
    public static class ValueUtils
    {
        private static Dictionary<ValueConverter, FunctionPointer<ValueConverter>> cache = new Dictionary<ValueConverter, FunctionPointer<ValueConverter>>();
        public static Value Px(this float value) => new Value(value, GetConverter(ValueConverters.Pixel));
        public static Value Px(this int value) => new Value(value, GetConverter(ValueConverters.Pixel));
        public static Value Pt(this float value) => new Value(value, GetConverter(ValueConverters.Point));
        public static Value Pt(this int value) => new Value(value, GetConverter(ValueConverters.Point));

        public static Value In(this float value) => new Value(value, GetConverter(ValueConverters.Inch));
        public static Value In(this int value) => new Value(value, GetConverter(ValueConverters.Inch));


        public static Value Cm(this float value) => new Value(value, GetConverter(ValueConverters.Centimeter));
        public static Value Cm(this int value) => new Value(value, GetConverter(ValueConverters.Centimeter));

        public static Value Mm(this float value) => new Value(value, GetConverter(ValueConverters.Millimeter));
        public static Value Mm(this int value) => new Value(value, GetConverter(ValueConverters.Millimeter));

        public static Value Pc(this float value) => new Value(value, GetConverter(ValueConverters.Pica));
        public static Value Pc(this int value) => new Value(value, GetConverter(ValueConverters.Pica));

        public static Value Vw(this float value) => new Value(value, GetConverter(ValueConverters.Vw));
        public static Value Vw(this int value) => new Value(value, GetConverter(ValueConverters.Vw));

        public static Value Vh(this float value) => new Value(value, GetConverter(ValueConverters.Vh));
        public static Value Vh(this int value) => new Value(value, GetConverter(ValueConverters.Vh));

        public static Value Vmin(this float value) => new Value(value, GetConverter(ValueConverters.Vmin));
        public static Value Vmin(this int value) => new Value(value, GetConverter(ValueConverters.Vmin));

        public static Value Vmax(this float value) => new Value(value, GetConverter(ValueConverters.Vmax));
        public static Value Vmax(this int value) => new Value(value, GetConverter(ValueConverters.Vmax));

        public static Value Perc(this float value) => new Value(value, GetConverter(ValueConverters.Percentage));

        public static Value Uniform() => new Value(0f, GetConverter(ValueConverters.Uniform));

        public static Value Inherit() => new Value(0f, GetConverter(ValueConverters.Inherit));


        private static FunctionPointer<ValueConverter> GetConverter(ValueConverter converterMethod)
        {

            if (!cache.TryGetValue(converterMethod, out FunctionPointer<ValueConverter> converter))
            {
                converter = BurstCompiler.CompileFunctionPointer<ValueConverter>(converterMethod);
                cache[converterMethod] = converter;
            }
            return converter;

        }
        public static ValueConverter GetConverter(FixedString32 name)
        {
            MonoPInvokeCallbackAttribute attr;
            foreach (var method in typeof(ValueConverters).GetMethods())
            {
                attr = method.GetCustomAttribute<MonoPInvokeCallbackAttribute>();

                if (attr != null && method.Name == name)
                {
                    return method.CreateDelegate(typeof(ValueConverter)) as ValueConverter;
                }
            }
            throw new ArgumentException("Invalid ValueConverter: " + name);
        }
        public static FunctionPointer<ValueConverter> GetConverterAsPointer(FixedString32 name)
        {
            return GetConverter(GetConverter(name));
        }
        public static List<ValueConverter> GetConverters(List<ValueConverter> converters = null)
        {
            if (converters == null)
                converters = new List<ValueConverter>();
            else
                converters.Clear();
            MonoPInvokeCallbackAttribute attr;
            foreach (var method in typeof(ValueConverters).GetMethods())
            {

                attr = method.GetCustomAttribute<MonoPInvokeCallbackAttribute>();

                if (attr != null)
                {
                    converters.Add(method.CreateDelegate(typeof(ValueConverter)) as ValueConverter);

                }
            }
            return converters;
        }

    }
    public enum UIAnchor
    {
        TOP_LEFT = 0b1000,
        TOP_CENTER = 0b1001,
        TOP_RIGHT = 0b1010,
        CENTER_LEFT = 0b0100,
        CENTER = 0b0101,
        CENTER_RIGHT = 0b0110,
        BOTTOM_LEFT = 0b0000,
        BOTTOM_CENTER = 0b0001,
        BOTTOM_RIGHT = 0b0010,
    }

    public static class UIUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float X(this UIAnchor anchor, float extent)
        {

            return ((((sbyte)anchor) & 0b0011) - 1) * extent;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Y(this UIAnchor anchor, float extent)
        {

            return ((((sbyte)anchor) >> 2) - 1) * extent;
        }
    }


}