using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Core.UI {
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

}