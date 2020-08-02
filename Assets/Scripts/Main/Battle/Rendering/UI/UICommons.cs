using System;
using System.Runtime.CompilerServices;
using Reactics.Commons;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.TextCore;

namespace Reactics.Core.UI {
    public struct ValueInfo {
        private UILength fontSize;
        public UILength FontSize
        {
            get => fontSize; set
            {
                if (!value.unit.IsAbsolute()) {
                    throw new ArgumentException("Font Size must be an Absolute Length");
                }
                fontSize = value;
            }
        }
    }

    public interface IValueProperties {

        float CalculatePercentage(float percent, UILength.Hints hints);

        float FontSize { get; }
        float RootFontSize { get; }

    }
    public struct UILength : IEquatable<UILength> {
        public const float PixelsPerInch = 96;

        public static readonly UILength Undefined = new UILength(float.NaN, UILengthUnit.Px);
        public static readonly UILength PositiveInfinity = new UILength(float.PositiveInfinity, UILengthUnit.Px);
        public static readonly UILength NegativeInfinity = new UILength(float.NegativeInfinity, UILengthUnit.Px);
        public static readonly UILength Zero = new UILength(0f, UILengthUnit.Px);
        public static readonly UILength Auto = new UILength(0f, UILengthUnit.Auto);
        public float value;
        public UILengthUnit unit;
        public UILength(float value, UILengthUnit unit) {
            this.value = value;
            this.unit = unit;
        }

        public bool Equals<TProperties>(UILength other, TProperties properties) where TProperties : struct, IValueProperties {
            return unit.IsAbsolute() && other.unit.IsAbsolute() ? RealValue(properties).Equals(other.RealValue(properties)) : (value == other.value && unit == other.unit);
        }
        public static float RealValue<TProperties>(float value, byte unit, TProperties properties = default, Hints hints = 0) where TProperties : struct, IValueProperties {
            switch (unit) {
                case 0:
                    return value;
                case 1:
                    return value * 1.3333f;
                case 2:
                    return value * 16f;
                case 3:
                    return value * UIScreenInfoSystem.ScreenDpi.Data;
                case 4:
                    return value * UIScreenInfoSystem.ScreenDpi.Data * 2.54f;
                case 5:
                    return value * UIScreenInfoSystem.ScreenDpi.Data * 0.254f;
                case 6:
                    return value * UIScreenInfoSystem.ScreenDpi.Data * 0.0635f;
                case 7:
                    return properties.FontSize * value;
                case 8:
                    return properties.RootFontSize * value;
                case 9:
                    return value * UIScreenInfoSystem.ScreenWidth.Data * 0.01f;
                case 10:
                    return value * UIScreenInfoSystem.ScreenHeight.Data * 0.01f;
                case 11:
                    return value * math.min(UIScreenInfoSystem.ScreenHeight.Data, UIScreenInfoSystem.ScreenWidth.Data) * 0.01f;
                case 12:
                    return value * math.max(UIScreenInfoSystem.ScreenHeight.Data, UIScreenInfoSystem.ScreenWidth.Data) * 0.01f;
                case 13:
                    return properties.CalculatePercentage(value, hints);
                case 14:
                    break;
            }
            return 0f;
        }
        public float RealValue<TProperties>(TProperties properties = default, Hints hints = 0) where TProperties : struct, IValueProperties => RealValue(value, (byte)unit, properties, hints);

        public bool Equals(UILength other) {
            return value == other.value && unit == other.unit;
        }
        public static implicit operator UILength(float value) => new UILength(value, UILengthUnit.Px);
        [Flags]
        public enum Hints {
            None = 0, Horizontal = 1, Vertical = 2
        }
    }

    public enum UILengthUnit {
        Px = 0,
        Pt = 1,
        Pc = 2,
        In = 3,
        Cm = 4,
        Mm = 5,
        Q = 6,
        Em = 7,
        Rem = 8,
        Vw = 9,
        Vh = 10,
        Vmin = 11,
        Vmax = 12,
        Perc = 13,
        Auto = 14


    }
    public static class UILengthUnitUtils {
        public static bool IsAbsolute(this UILengthUnit unit) => ((byte)unit) <= 6;
        public static bool IsRelative(this UILengthUnit unit) => ((byte)unit) > 6;
    }
    public interface IBox {
        UILength Top { get; }
        UILength Right { get; }
        UILength Bottom { get; }
        UILength Left { get; }
    }
    public interface IConstrainable {
        UILength Min { get; }
        UILength Max { get; }

    }
    public interface ISpan {
        float Length { get; }
    }
    public static class ISpanExtensions {
        public static NativeArray<float> GetSpacing<TItem, TList>(this Spacing spacing, float totalSpace, TList items, Allocator allocator = Allocator.Temp) where TItem : struct, ISpan where TList : struct, INativeList<TItem> {
            var output = new NativeArray<float>(items.Length + 1, allocator);
            if (items.Length == 0)
                return output;
            switch (spacing) {
                case Spacing.Start:
                    break;

                case Spacing.End:
                    float endUsedSpace = 0;
                    for (int i = 0; i < items.Length; i++) {
                        endUsedSpace += items[i].Length;
                    }
                    output[0] = math.max(0, totalSpace - endUsedSpace);
                    break;
                case Spacing.Center:
                    float centerUsedSpace = 0;
                    for (int i = 0; i < items.Length; i++) {
                        centerUsedSpace += items[i].Length;
                    }
                    var centerAvailableSpace = math.max(0, totalSpace - centerUsedSpace);
                    output[0] = centerAvailableSpace / 2f;
                    output[output.Length - 1] = totalSpace - centerUsedSpace;
                    break;
                case Spacing.SpaceBetween:
                    if (items.Length == 1)
                        return output;
                    float spaceBetweenUsedSpace = 0;
                    for (int i = 0; i < items.Length; i++) {
                        spaceBetweenUsedSpace += items[i].Length;
                    }
                    var gap = math.max(0, totalSpace - spaceBetweenUsedSpace) / (items.Length - 1);
                    for (int i = 0; i < output.Length - 2; i++) {
                        output[i + 1] = gap;
                    }
                    break;
            }
            return output;
        }
    }
    public static class IBoxExtensions {
        public static float2 GetSize<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return new float2(box.Left.RealValue(properties, UILength.Hints.Horizontal) + box.Right.RealValue(properties, UILength.Hints.Horizontal), box.Top.RealValue(properties, UILength.Hints.Vertical) + box.Bottom.RealValue(properties, UILength.Hints.Vertical));
        }
        public static float GetWidth<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return box.Left.RealValue(properties) + box.Right.RealValue(properties);
        }
        public static float GetHeight<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return box.Top.RealValue(properties) + box.Bottom.RealValue(properties);
        }
        public static float4 GetRealValues<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return new float4(box.Top.RealValue(properties, UILength.Hints.Vertical), box.Right.RealValue(properties, UILength.Hints.Horizontal), box.Bottom.RealValue(properties, UILength.Hints.Vertical), box.Left.RealValue(properties, UILength.Hints.Horizontal));
        }
    }
    public static class IConstrainableExtensions {
        public static float Clamp<TConstrainable, TProperties>(this TConstrainable self, UILength value, TProperties properties, bool favorMax = true) where TConstrainable : struct, IConstrainable where TProperties : struct, IValueProperties {
            var min = self.Min.RealValue(properties);
            var max = self.Max.RealValue(properties);
            var realValue = value.RealValue(properties);
            if (float.IsNaN(realValue)) {
                return favorMax ? max : min;
            }
            return math.clamp(realValue, min, max);
        }
        public static float Clamp<TConstrainable, TProperties>(this TConstrainable self, float value, TProperties properties, bool favorMax = true) where TConstrainable : struct, IConstrainable where TProperties : struct, IValueProperties {
            var min = self.Min.RealValue(properties);
            var max = self.Max.RealValue(properties);
            if (float.IsNaN(value)) {
                return favorMax ? max : min;
            }
            return math.clamp(value, min, max);
        }
        public static bool IsDefinite(this float self) {
            return !float.IsInfinity(self) && !float.IsNaN(self);
        }
        public static bool IsFixed<TConstrainable>(this TConstrainable self) where TConstrainable : struct, IConstrainable => self.Min.Equals(self.Max);
    }
    public static class AlignmentExtensions {
        public static float GetOffset(this Alignment alignment, float targetSpan, float containerSpan) {
            switch (alignment) {
                case Alignment.Start:
                    return 0;
                case Alignment.End:
                    return containerSpan - targetSpan;
                case Alignment.Center:
                    return (containerSpan / 2f) - (targetSpan / 2f);
                case Alignment.Baseline:
                    return 0;
                default:
                    return 0;
            }
        }
    }
    public enum Alignment {
        Start, End, Center, Baseline
    };
    public enum Spacing {
        Start, End, Center, SpaceBetween
    }

    public enum Layout {
        Horizontal, Vertical
    }
    public enum UIAnchor {
        BOTTOM_LEFT = 0b0000,
        BOTTOM_CENTER = 0b0001,
        BOTTOM_RIGHT = 0b0010,
        CENTER_LEFT = 0b0100,
        CENTER = 0b0101,
        CENTER_RIGHT = 0b0110,
        TOP_LEFT = 0b1000,
        TOP_CENTER = 0b1001,
        TOP_RIGHT = 0b1010,
    }

    public static class UIAnchorExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float X(this UIAnchor anchor, float extent) {

            return ((((sbyte)anchor) & 0b0011) - 1) * extent;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Y(this UIAnchor anchor, float extent) {

            return ((((sbyte)anchor) >> 2) - 1) * extent;
        }
    }


}