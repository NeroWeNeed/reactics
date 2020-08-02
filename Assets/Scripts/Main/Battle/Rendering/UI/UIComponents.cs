using System;
using System.Collections.Generic;
using Reactics.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Core.UI {
    public struct UIResolvedBox : IComponentData {
        public float4 value;
        public float2 Size => new float2(value.z - value.x, value.w - value.y);
        public float2 Position => new float2(value.x, value.y);
        public float2 Center => new float2(value.x + ((value.z - value.x) / 2f), value.y + ((value.w - value.y) / 2f));
    }

    public struct UIElement : IComponentData {
        public int Version;
    }

    public struct UIState : IComponentData {
        public State value;
    }
    [Flags]
    public enum State {
        None = 0, Hover = 1, Pressed = 2
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
    public struct AspectRatio : IComponentData {
        public float value;
    }
    [GenerateAuthoringComponent]
    public struct ScreenInfo : IComponentData {
        public int2 screen;
        public int2 resolution;
        public float dpi;
        public ScreenOrientation orientation;
    }
    [WriteGroup(typeof(LocalToWorld))]
    public struct LocalToScreen : IComponentData { }
    public struct UISize : IComponentData {
        public static readonly UISize Unbounded = new UISize(UILength.Zero, UILength.PositiveInfinity, UILength.Zero, UILength.PositiveInfinity);
        public UILength MinWidth, MaxWidth, MinHeight, MaxHeight;
        public UILength Width
        {
            set => MinWidth = MaxWidth = value;
        }
        public UILength Height
        {
            set => MinHeight = MaxHeight = value;
        }
        public UISize(UILength minWidth, UILength maxWidth, UILength minHeight, UILength maxHeight) {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
        }
        public UISize(UILength width, UILength height) {
            MinWidth = MaxWidth = width;
            MinHeight = MaxHeight = height;
        }
        public float4 RealValues<TProperties>(TProperties properties) where TProperties : struct, IValueProperties {
            return new float4(MinWidth.RealValue(properties, UILength.Hints.Horizontal), MaxWidth.RealValue(properties, UILength.Hints.Horizontal), MinHeight.RealValue(properties, UILength.Hints.Vertical), MaxHeight.RealValue(properties, UILength.Hints.Vertical));
        }

        public float2 Clamp<TProperties>(float2 value, TProperties properties) where TProperties : struct, IValueProperties {
            return new float2(math.clamp(value.x, MinWidth.RealValue(properties, UILength.Hints.Horizontal), MaxWidth.RealValue(properties, UILength.Hints.Horizontal)), math.clamp(value.y, MinHeight.RealValue(properties, UILength.Hints.Vertical), MaxHeight.RealValue(properties, UILength.Hints.Vertical)));
        }
    }
    public struct UILayout : IComponentData {
        public Layout value;
    }
    public struct UIWrap : IComponentData { }
    public struct UISpacing : IComponentData {
        public Spacing value;

    }

    public struct UIAlignSelf : IComponentData {
        public Alignment value;
    }
    public struct UIAlignChildren : IComponentData {
        public Alignment value;
    }

    public struct UIFont : ISharedComponentData, IEquatable<UIFont> {
        public TMP_FontAsset value;
        public float size;

        public bool Equals(UIFont other) {
            return EqualityComparer<TMP_FontAsset>.Default.Equals(value, other.value);
        }

        public override int GetHashCode() {
            return -1584136870 + EqualityComparer<TMP_FontAsset>.Default.GetHashCode(value);
        }
    }
    public struct UIRectangle : IComponentData {
        public UILength width, height;
    }

    public struct UIText : ISharedComponentData, IEquatable<UIText> {
        public string text;
        public UILength minWidth;
        public UILength maxWidth;

        public UIText(string text, UILength minWidth, UILength maxWidth) {
            this.text = text;
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
        }
        public UIText(string text) {
            this.text = text;
            this.minWidth = UILength.Undefined;
            this.maxWidth = UILength.Undefined;
        }

        public override bool Equals(object obj) {
            if (obj is UIText text) {
                return Equals(other: text);
            }
            return false;
        }

        public bool Equals(UIText other) {
            return this.text == other.text &&
                   minWidth.Equals(other.minWidth) &&
                   maxWidth.Equals(other.maxWidth);
        }

        public override int GetHashCode() {
            int hashCode = -1796917006;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(text);
            hashCode = hashCode * -1521134295 + minWidth.GetHashCode();
            hashCode = hashCode * -1521134295 + maxWidth.GetHashCode();
            return hashCode;
        }
    }
    public struct UIMeshData : IBufferElementData {
        public float3 vertex;


    }
}