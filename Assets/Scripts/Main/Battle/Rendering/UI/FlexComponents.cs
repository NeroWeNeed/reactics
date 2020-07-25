using Unity.Entities;

namespace Reactics.Core.UI {
    public struct Flex : IComponentData {
        public static readonly Flex Initial = new Flex
        {
            direction = Flex.Direction.Row,
            wrap = Flex.Wrap.NoWrap,
            grow = 0f,
            shrink = 1f,
            basis = Basis.AutoBasis
        };
        public Direction direction;
        public Wrap wrap;
        public float grow, shrink;
        public Basis basis;
        public Align alignContent;
        public Align alignItems;
        public JustifyContent justifyContent;

        public enum Direction {
            Row = 0x00, Column = 0x01, RowReverse = 0x10, ColumnReverse = 0x11
        }
        public enum Wrap {
            NoWrap = 0x00, Wrap = 0x01, WrapReverse = 0x10
        }
        public enum Align {
            FlexStart,
            FlexEnd,
            Center,
            Stretch
        }
        public enum JustifyContent {
            FlexStart,
            FlexEnd,
            Center,
            SpaceBetween,
            SpaceAround
        }
        public struct Basis {
            public const byte CONTENT = 17;
            public static readonly Basis ContentBasis = new Basis
            {
                value = 0f,
                unit = CONTENT
            };
            public static readonly Basis AutoBasis = new Basis
            {
                value = 0f,
                unit = (byte)UILengthUnit.Auto
            };
            public float value;
            public byte unit;
            public float RealValue<TProperties>(TProperties props) where TProperties : struct, IValueProperties {
                if (unit == CONTENT)
                    return 0f;
                else
                    return UILength.RealValue(value, unit, props);
            }
            public bool IsLength => unit < 16;
            public bool IsDefinite<TProperties>(TProperties props) where TProperties : struct, IValueProperties => IsLength && RealValue(props).IsDefinite();

            public bool TryGetDefiniteValue<TProperties>(TProperties props, out float value) where TProperties : struct, IValueProperties {
                if (IsLength) {
                    value = RealValue(props);
                    return value.IsDefinite();
                }
                else {
                    value = default;
                    return false;
                }
            }
            public static implicit operator Basis(UILength length) => new Basis
            {
                value = length.value,
                unit = (byte)length.unit
            };

        }

    }
    public static class FlexExtensions {
        public static bool IsRow(this Flex.Direction direction) => (((byte)direction) & 0x01) == 0;
        public static bool IsColumn(this Flex.Direction direction) => (((byte)direction) & 0x01) != 0;
    }
}