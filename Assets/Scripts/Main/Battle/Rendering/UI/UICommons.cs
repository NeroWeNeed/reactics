using System;
using Reactics.Commons;
using Unity.Entities;
using Unity.Mathematics;

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
        float ContainerLength { get; }
        float FontSize { get; }
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
        public static float RealValue<TProperties>(float value, byte unit, TProperties properties = default) where TProperties : struct, IValueProperties {
            switch (unit) {
                case 0:
                    return value;
                case 1:
                    return value * (4f / 3);
                case 2:
                    return value * (4f / 3) * 12f;
                case 3:
                    return value * PixelsPerInch;
                case 4:
                    return value * PixelsPerInch * 2.54f;
                case 5:
                    return value * PixelsPerInch * 2.54f / 10f;
                //TODO
                case 6:
                    return properties.FontSize * value;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    break;
                case 11:
                    break;
                case 12:
                    break;
                case 13:
                    break;
                case 14:
                    return properties.ContainerLength * value;
                case 15:
                    break;
            }
            return 0f;
        }
        public float RealValue<TProperties>(TProperties properties = default) where TProperties : struct, IValueProperties => RealValue(value, (byte)unit, properties);

        public bool Equals(UILength other) {
            return value == other.value && unit == other.unit;
        }
    }

    public enum UILengthUnit {
        Px = 0,
        Pt = 1,
        Pc = 2,
        In = 3,
        Cm = 4,
        Mm = 5,
        Em = 6,
        Ex = 7,
        Ch = 8,
        Rem = 9,
        Vw = 10,
        Vh = 11,
        Vmin = 12,
        Vmax = 13,
        Perc = 14,
        Inherit = 15,
        Auto = 16


    }
    public static class UILengthUnitUtils {
        public static bool IsAbsolute(this UILengthUnit unit) => ((byte)unit) < 6;
        public static bool IsRelative(this UILengthUnit unit) => ((byte)unit) >= 6;
    }
    public struct UILength2 {
        public UILength x, y;

        public UILength2(UILength x, UILength y) {
            this.x = x;
            this.y = y;
        }
    }
    public struct UILength3 {
        public UILength x, y, z;

        public UILength3(UILength x, UILength y, UILength z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    public struct UILength4 {
        public UILength x, y, z, w;

        public UILength4(UILength x, UILength y, UILength z, UILength w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
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
    public struct UISize {
        public UIWidth width;
        public UIHeight height;
        public float4 RealValue<TProperties>(TProperties properties) where TProperties : struct, IValueProperties {
            return new float4(width.Min.RealValue(properties), height.Min.RealValue(properties), width.Max.RealValue(properties), height.Max.RealValue(properties));
        }
        public float2 RealWidthValue<TProperties>(TProperties properties) where TProperties : struct, IValueProperties {
            return new float2(width.Min.RealValue(properties), width.Max.RealValue(properties));
        }
        public float2 RealHeightValue<TProperties>(TProperties properties) where TProperties : struct, IValueProperties {
            return new float2(height.Min.RealValue(properties), height.Max.RealValue(properties));
        }
    }
    public static class IBoxExtensions {
        public static float2 GetSize<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return new float2(box.Top.RealValue(properties) + box.Bottom.RealValue(properties), box.Left.RealValue(properties) + box.Right.RealValue(properties));
        }
        public static float GetWidth<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return box.Left.RealValue(properties) + box.Right.RealValue(properties);
        }
        public static float GetHeight<TBox, TProperties>(this TBox box, TProperties properties) where TBox : struct, IBox where TProperties : struct, IValueProperties {
            return box.Top.RealValue(properties) + box.Bottom.RealValue(properties);
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
    }

}