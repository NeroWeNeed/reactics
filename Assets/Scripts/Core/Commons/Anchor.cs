using Unity.Mathematics;

namespace Reactics.Core.Commons {
    public struct Anchor {
        private byte value;
        public sbyte X
        {
            get
            {
                var b = value & 0b00000011;
                return (sbyte)((b >> 1) - (b & 0b00000001));
            }
            set
            {
                var old = (byte)(this.value & 0b11111100);
                this.value = (byte)(old | (value < 0 ? 0b00000001 : (value > 0 ? 0b00000010 : 0b00000000)));
            }
        }
        public sbyte Y
        {
            get
            {
                var b = value & 0b00001100;
                return (sbyte)((b >> 3) - (b & 0b00000100));
            }
            set
            {
                var old = (byte)(this.value & 0b11110011);
                this.value = (byte)(old | (value < 0 ? 0b00000100 : (value > 0 ? 0b00001000 : 0b00000000)));
            }
        }
        public sbyte Z
        {
            get
            {
                var b = value & 0b00110000;
                return (sbyte)((b >> 5) - (b & 0b00010000));
            }
            set
            {
                var old = (byte)(this.value & 0b11001111);
                this.value = (byte)(old | (value < 0 ? 0b00010000 : (value > 0 ? 0b00100000 : 0b00000000)));
            }
        }
        public sbyte W
        {
            get
            {
                var b = value & 0b11000000;
                return (sbyte)((b >> 7) - (b & 0b01000000));
            }
            set
            {
                var old = (byte)(this.value & 0b00111111);
                this.value = (byte)(old | (value < 0 ? 0b01000000 : (value > 0 ? 0b10000000 : 0b00000000)));
            }
        }
        public Anchor(sbyte x = 0, sbyte y = 0, sbyte z = 0, sbyte w = 0) {
            value = (byte)(
            (w < 0 ? 0b01000000 : (w > 0 ? 0b10000000 : 0b00000000)) |
            (z < 0 ? 0b00010000 : (z > 0 ? 0b00100000 : 0b00000000)) |
            (y < 0 ? 0b00000100 : (y > 0 ? 0b00001000 : 0b00000000)) |
            (x < 0 ? 0b00000001 : (x > 0 ? 0b00000010 : 0b00000000))
            );
        }

        public float XOffset(float extent) => X * extent;
        public float YOffset(float extent) => Y * extent;
        public float ZOffset(float extent) => Z * extent;
        public float WOffset(float extent) => W * extent;
        public int XOffset(int extent) => X * extent;
        public int YOffset(int extent) => Y * extent;
        public int ZOffset(int extent) => Z * extent;
        public int WOffset(int extent) => W * extent;
        public float2 XYOffset(float2 extents) => new float2(extents.x * X, extents.y * Y);
        public float3 XYZOffset(float3 extents) => new float3(extents.x * X, extents.y * Y, extents.z * Z);
        public float4 XYZWOffset(float4 extents) => new float4(extents.x * X, extents.y * Y, extents.z * Z, extents.w * W);
        public int2 XYOffset(int2 extents) => new int2(extents.x * X, extents.y * Y);
        public int3 XYZOffset(int3 extents) => new int3(extents.x * X, extents.y * Y, extents.z * Z);
        public int4 XYZWOffset(int4 extents) => new int4(extents.x * X, extents.y * Y, extents.z * Z, extents.w * W);
    }


}