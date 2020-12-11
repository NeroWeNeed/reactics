using System;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.Commons {
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ColorAttribute : Attribute {
        public Color Value { get; }

        public ColorAttribute(string value) {
            if (ColorUtility.TryParseHtmlString(value, out Color result)) {
                Value = result;
            }
            else {
                Value = Color.black;
            }
        }
        public ColorAttribute(byte red, byte green, byte blue, byte alpha = 255) : this(red / 255f, green / 255f, blue / 255f, alpha / 255f) { }
        public ColorAttribute(float red, float green, float blue, float alpha = 1f) {
            Value = new Color(math.clamp(red, 0, 1), math.clamp(green, 0, 1), math.clamp(blue, 0, 1), math.clamp(alpha, 0, 1));
        }
    }
}