using Unity.Mathematics;

namespace Reactics.Core.Commons {
    public struct Anchor {
        private sbyte value;

        public Anchor(sbyte value) : this() {
            Value = value;
        }
        public Anchor(short value) : this() {
            Value = (sbyte)value;
        }
        public Anchor(int value) : this() {
            Value = (sbyte)value;
        }
        public Anchor(long value) : this() {
            Value = (sbyte)value;
        }
        public Anchor(float value) : this() {
            Value = (sbyte)value;
        }
        public Anchor(double value) : this() {
            Value = (sbyte)value;
        }

        public sbyte Value
        {
            get => value;
            set
            {
                this.value = (sbyte)(value > 0 ? 1 : (value < 0 ? -1 : 0));
            }
        }
        public static implicit operator Anchor(short value) => new Anchor(value);
        public float Invoke(float extent) => value * extent;

        public int Invoke(int extent) => value * extent;

        public sbyte Invoke(sbyte extent) => (sbyte)(value * extent);

        public short Invoke(short extent) => (short)(value * extent);

        public long Invoke(long extent) => value * extent;

        public double Invoke(double extent) => value * extent;
    }

    public struct Anchor2D {
        private sbyte _x, _y;
        public sbyte x
        {
            get => _x; set
            {
                _x = (sbyte)(value > 0 ? 1 : (value < 0 ? -1 : 0));
            }
        }
        public sbyte y
        {
            get => _y; set
            {
                _y = (sbyte)(value > 0 ? 1 : (value < 0 ? -1 : 0));
            }
        }
    }

    public struct Anchor3D {


        private sbyte _x, _y, _z;
        public sbyte x
        {
            get => _x; set
            {
                _x = (sbyte)(value > 0 ? 1 : (value < 0 ? -1 : 0));
            }
        }
        public sbyte y
        {
            get => _y; set
            {
                _y = (sbyte)(value > 0 ? 1 : (value < 0 ? -1 : 0));
            }
        }
        public sbyte z
        {
            get => _z; set
            {
                _z = (sbyte)(value > 0 ? 1 : (value < 0 ? -1 : 0));
            }
        }


        public Anchor3D(sbyte x, sbyte y, sbyte z) {
            _x = x;
            _y = y;
            _z = z;
        }
        public Anchor3D(int x, int y, int z) {
            _x = (sbyte)x;
            _y = (sbyte)y;
            _z = (sbyte)z;
        }
        public Anchor3D(short x, short y, short z) {
            _x = (sbyte)x;
            _y = (sbyte)y;
            _z = (sbyte)z;
        }
        public Anchor3D(long x, long y, long z) {
            _x = (sbyte)x;
            _y = (sbyte)y;
            _z = (sbyte)z;
        }
        public Anchor3D(float x, float y, float z) {
            _x = (sbyte)x;
            _y = (sbyte)y;
            _z = (sbyte)z;
        }

        public float X(float extent) => _x * extent;
        public float Y(float extent) => _y * extent;
        public float Z(float extent) => _z * extent;
        public float X(int extent) => _x * extent;
        public float Y(int extent) => _y * extent;
        public float Z(int extent) => _z * extent;

        public float3 XYZ(float3 extents) => new float3(extents.x * _x, extents.y * _y, extents.z * _z);
    }

}