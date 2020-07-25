using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core.UI {
    public struct UIBoxConstraints : IComponentData {
        public float4 value;
        public float2 Size => new float2(value.z - value.x, value.w - value.y);
        public float2 Position => new float2(value.x, value.y);
        public float2 Center => new float2(value.x + ((value.z - value.x) / 2f), value.y + ((value.w - value.y) / 2f));
    }
    public struct UIElement : IComponentData {
        public int Version;
    }


    public struct UIChild : IBufferElementData, IEquatable<UIChild>, IEquatable<UISystemStateChild> {
        public Entity value;

        public bool Equals(UISystemStateChild other) {
            return value == other.value;
        }

        public bool Equals(UIChild other) {
            return value == other.value;
        }
        public static implicit operator UIChild(Entity entity) => new UIChild { value = entity };
        public static implicit operator UIChild(UISystemStateChild child) => new UIChild { value = child.value };
        public static implicit operator Entity(UIChild child) => child.value;
    }
    public struct UISystemStateChild : ISystemStateBufferElementData, IEquatable<UIChild>, IEquatable<UISystemStateChild> {
        public Entity value;
        public bool Equals(UISystemStateChild other) {
            return value == other.value;
        }

        public bool Equals(UIChild other) {
            return value == other.value;
        }
        public static implicit operator UISystemStateChild(Entity entity) => new UISystemStateChild { value = entity };
        public static implicit operator UISystemStateChild(UIChild child) => new UISystemStateChild { value = child.value };
        public static implicit operator Entity(UISystemStateChild child) => child.value;
    }
    public struct UIParent : IComponentData, IEquatable<UIParent>, IEquatable<UISystemStateParent> {
        public Entity value;

        public bool Equals(UISystemStateParent other) {
            return value == other.value;
        }

        public bool Equals(UIParent other) {
            return value == other.value;
        }

        public static implicit operator UIParent(Entity entity) => new UIParent { value = entity };
        public static implicit operator UIParent(UISystemStateParent child) => new UIParent { value = child.value };
        public static implicit operator Entity(UIParent parent) => parent.value;
    }
    public struct UISystemStateParent : ISystemStateComponentData, IEquatable<UIParent>, IEquatable<UISystemStateParent> {
        public Entity value;
        public bool Equals(UISystemStateParent other) {
            return value == other.value;
        }

        public bool Equals(UIParent other) {
            return value == other.value;
        }
        public static implicit operator UISystemStateParent(Entity entity) => new UISystemStateParent { value = entity };
        public static implicit operator UISystemStateParent(UIParent child) => new UISystemStateParent { value = child.value };
        public static implicit operator Entity(UISystemStateParent parent) => parent.value;
    }
    //Flex
    /*     public struct FlexProperties : IComponentData {
            public Direction direction;
            public Wrap wrap;
            public float grow, shrink;
            public FlexBasis basis;

            public enum Direction {
                Row = 0x00, Column = 0x01, RowReverse = 0x10, ColumnReverse = 0x11
            }
            public enum Wrap {
                NoWrap, Wrap, WrapReverse
            }
        }
        public static class FlexExtensions {
            public static bool IsRow(this FlexProperties.Direction self) => ((byte)self & 0x01) == 0;
            public static bool IsColumn(this FlexProperties.Direction self) => ((byte)self & 0x01) != 0;
            public static bool IsReversed(this FlexProperties.Direction self) => ((byte)self & 0x10) != 0;
        }
        public struct FlexBasis {
            public const byte CONTENT_UNIT = 17;
            public static readonly FlexBasis Content = new FlexBasis(0f, CONTENT_UNIT);
            public float value;
            public byte unit;

            public FlexBasis(float value, byte unit) {
                this.value = value;
                this.unit = unit;
            }
            public FlexBasis(float value, UILengthUnit unit) {
                this.value = value;
                this.unit = (byte)unit;
            }
            public float RealValue<TProperties>(TProperties properties = default) where TProperties : struct, IValueProperties {
                if (unit == CONTENT_UNIT) {
                    return float.NaN;
                }
                else {
                    return UILength.RealValue(value, unit, properties);
                }
            }
            public bool IsContent => unit == CONTENT_UNIT;
            public bool RequiresAvailableSpace => unit >= (byte)UILengthUnit.Auto;
        } */
    public struct UIMargin : IComponentData, IBox {
        public UILength Top { get; set; }
        public UILength Right { get; set; }
        public UILength Bottom { get; set; }
        public UILength Left { get; set; }
    }
    public struct UIPadding : IComponentData, IBox {
        public UILength Top { get; set; }
        public UILength Right { get; set; }
        public UILength Bottom { get; set; }
        public UILength Left { get; set; }
    }
    public struct UIBorderWidth : IComponentData, IBox {
        public UILength Top { get; set; }
        public UILength Right { get; set; }
        public UILength Bottom { get; set; }
        public UILength Left { get; set; }
    }
    public struct UIBorderRadius : IComponentData {
        public UILength topLeft, topRight, bottomRight, bottomLeft;
    }
    public struct UIOrder : IComponentData {
        public int value;
    }
    public struct Margin : IComponentData {
        public float4 value;
    }
    public struct AspectRatio : IComponentData {
        public float value;
    }

    public struct UIWidth : IComponentData, IConstrainable {
        public static readonly UIWidth Unbound = new UIWidth(UILength.NegativeInfinity, UILength.PositiveInfinity);
        public UILength Min { get; set; }
        public UILength Max { get; set; }

        public UILength Fixed
        {
            set
            {
                Min = value;
                Max = value;
            }
            get => IsFixed() ? Max : UILength.Undefined;
        }
        public bool IsFixed() => Min.Equals(Max);
        public UIWidth(UILength min, UILength max) {
            Min = min;
            Max = max;
        }
        public UIWidth(UILength value) {
            Min = value;
            Max = value;
        }
    }
    public struct UIHeight : IComponentData, IConstrainable {
        public static readonly UIHeight Unbound = new UIHeight(UILength.NegativeInfinity, UILength.PositiveInfinity);
        public UILength Min { get; set; }
        public UILength Max { get; set; }
        public UILength Fixed
        {
            set
            {
                Min = value;
                Max = value;
            }
            get => IsFixed() ? Max : UILength.Undefined;
        }
        public bool IsFixed() => Min.Equals(Max);
        public UIHeight(UILength min, UILength max) {
            Min = min;
            Max = max;
        }
        public UIHeight(UILength value) {
            Min = value;
            Max = value;
        }
    }

}