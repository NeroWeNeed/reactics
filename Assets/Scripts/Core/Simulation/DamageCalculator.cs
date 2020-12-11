using System;
using Unity.Burst;

namespace Reactics.Core {
    public enum DamageType : byte {

        Magic_Resistance = 1, Magic_Defense = 2, Strength_Resistance = 3, Strength_Defense = 4
    }
    [Flags]
    public enum ElementalAttribute : byte {
        None = 0,
        Fire = 1,
        Water = 2,
        Earth = 4,
        Wind = 8,
        Ice = 16,
        Lightning = 32,
        Light = 64,
        Dark = 128,

    }
    [BurstCompile]
    public static class ElementalAttributeExtensions {
        [BurstCompile]
        public static ElementalAttribute Invert(this ElementalAttribute attribute) {
            byte result = 0;
            byte buffer;
            for (byte offset = 0; offset < 4; offset++) {
                buffer = (byte)((((byte)attribute) & ((byte)(0b11 << offset * 2))) >> (offset * 2));
                result |= (byte)(((byte)(buffer & 0b00000001)) << ((byte)(offset * 2)));
                result |= (byte)(((byte)(buffer & 0b00000010)) << ((byte)((offset * 2) + 1)));
            }
            return (ElementalAttribute)result;
        }
    }
    [Flags]
    public enum PhysicalAttribute : byte {
        None = 0,
        Strike = 1,
        Slash = 2,
        Pierce = 4
    }

}