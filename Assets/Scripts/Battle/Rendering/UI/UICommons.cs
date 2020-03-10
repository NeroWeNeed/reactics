using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace Reactics.UI
{
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


    public interface IUIConfigurator : IEquatable<IUIConfigurator>
    {
        void Configure(Entity entity, EntityCommandBuffer entityCommandBuffer, World world);
    }

}