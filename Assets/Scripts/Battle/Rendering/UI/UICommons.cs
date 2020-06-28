using System;
using Reactics.Commons;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.UI
{
    public struct ValueInfo
    {
        private UILength fontSize;

        public UILength FontSize
        {
            get => fontSize; set
            {
                if (!value.unit.IsAbsolute())
                {
                    throw new ArgumentException("Font Size must be an Absolute Length");
                }
                fontSize = value;
            }
        }

    }


    public struct UILength
    {
        public const float PixelsPerInch = 96;
        public float value;

        public UILengthUnit unit;

        public float RealValue(ValueInfo info)
        {
            switch (unit)
            {
                case UILengthUnit.Px:
                    return value;
                case UILengthUnit.Pt:
                    return value * (4f / 3);
                case UILengthUnit.Pc:
                    return value * (4f / 3) * 12f;
                case UILengthUnit.In:
                    return value * PixelsPerInch;
                case UILengthUnit.Cm:
                    return value * PixelsPerInch * 2.54f;
                case UILengthUnit.Mm:
                    return value * PixelsPerInch * 2.54f / 10f;
                //TODO
                case UILengthUnit.Em:
                    break;
                case UILengthUnit.Ex:
                    break;
                case UILengthUnit.Ch:
                    break;
                case UILengthUnit.Rem:
                    break;
                case UILengthUnit.Vw:
                    break;
                case UILengthUnit.Vh:
                    break;
                case UILengthUnit.Vmin:
                    break;
                case UILengthUnit.Vmax:
                    break;
                case UILengthUnit.Perc:
                    break;
                case UILengthUnit.Uniform:
                    break;
            }
            return 0f;
        }
    }

    public enum UILengthUnit
    {
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
        Uniform = 15
    }

    public static class UILengthUnitUtils
    {
        public static bool IsAbsolute(this UILengthUnit unit) => ((byte)unit) < 6;
        public static bool IsRelative(this UILengthUnit unit) => ((byte)unit) >= 6;
    }




}