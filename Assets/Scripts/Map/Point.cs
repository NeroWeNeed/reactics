using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Reactics.Battle
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point>
    {
        public static readonly Point zero = new Point(0, 0);

        [SerializeField]
        private ushort _x, _y;
        public ushort x { get => _x; }
        public ushort y { get => _y; }

        public Point(ushort x, ushort y)
        {
            _x = x;
            _y = y;
        }
        public Point(int x, int y)
        {
            _x = Convert.ToUInt16(x);
            _y = Convert.ToUInt16(y);
        }



        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public override int GetHashCode()
        {
            int hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return "Point(" + x + "," + y + ")";
        }


        public static implicit operator Vector2Int(Point point) => new Vector2Int(Convert.ToInt32(point.x), Convert.ToInt32(point.y));
        public static explicit operator Point(Vector2Int vector)
        {
            if (vector.x >= 0 && vector.y >= 0)
                if (vector.x == 0 && vector.y == 0)
                    return zero;
                else
                    return new Point((ushort)vector.x, (ushort)vector.y);
            else
                throw new InvalidCastException("Vector must be positive");
        }

    }
}