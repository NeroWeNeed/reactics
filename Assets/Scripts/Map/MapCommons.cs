using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reactics.Util;
using UnityEngine;

namespace Reactics.Battle
{
    public static class MapUtil
    {
        public static Mesh GenerateMesh(Mesh mesh, ushort width, ushort length, float tileSize)
        {
            int vertexCount = (width + 1) * (length + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[width * length * 6];
            int x, y, index;
            for (y = 0; y <= length; y++)
            {
                for (x = 0; x <= width; x++)
                {
                    index = y * (width + 1) + x;
                    vertices[index] = new Vector3(x * tileSize, 0, y * tileSize);
                    uv[index] = new Vector2((float)x / (width), (float)y / (length));
                    normals[index] = Vector3.up;
                }
            }
            for (y = 0; y < length; y++)
            {
                for (x = 0; x < width; x++)
                {
                    index = (y * width + x) * 6;
                    triangles[index] = y * (width + 1) + x;
                    triangles[index + 1] = y * (width + 1) + x + width + 1;
                    triangles[index + 2] = y * (width + 1) + x + width + 2;
                    triangles[index + 3] = y * (width + 1) + x;
                    triangles[index + 4] = y * (width + 1) + x + width + 2;
                    triangles[index + 5] = y * (width + 1) + x + 1;
                }
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;
            mesh.normals = normals;
            return mesh;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point>
    {
        public static readonly Point zero = new Point(0, 0);

        public ushort x;
        public ushort y;

        public Point(ushort x, ushort y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(int x, int y)
        {
            this.x = Convert.ToUInt16(x);
            this.y = Convert.ToUInt16(y);
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


    public class PointComparerByX : IComparer<Point>
    {
        private static PointComparerByX INSTANCE;

        public static PointComparerByX GetInstance()
        {
            if (INSTANCE == null)
                INSTANCE = new PointComparerByX();
            return INSTANCE;
        }
        public int Compare(Point a, Point b)
        {
            int yCompare = a.y.CompareTo(b.y);
            return yCompare != 0 ? yCompare : a.x.CompareTo(b.x);
        }
    }

    public class PointComparerByY : IComparer<Point>
    {
        private static PointComparerByY INSTANCE;

        public static PointComparerByY GetInstance()
        {
            if (INSTANCE == null)
                INSTANCE = new PointComparerByY();
            return INSTANCE;
        }
        public int Compare(Point a, Point b)
        {
            int xCompare = a.x.CompareTo(b.x);
            return xCompare != 0 ? xCompare : a.y.CompareTo(b.y);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SpawnGroup
    {
        [SerializeField]
        public Point[] points;

    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Tile
    {
        public int elevation;
        public BlittableBool inaccessible;
        public bool Accessible() => !inaccessible;
    }
    public struct TileInfo : IEquatable<TileInfo>
    {
        public readonly Point point;
        public readonly Tile tile;
        public TileInfo(Point point, Tile tile)
        {
            this.point = point;
            this.tile = tile;
        }

        bool IEquatable<TileInfo>.Equals(TileInfo other)
        {
            return other.point.Equals(point) && other.tile.Equals(tile);
        }
    }

    public enum MapLayer
    {
        BASE,
        HOVER,
        PLAYER_MOVE,
        PLAYER_ATTACK,
        PLAYER_SUPPORT,
        PLAYER_ALL,
        ENEMY_MOVE,
        ENEMY_ATTACK,
        ENEMY_SUPPORT,
        ENEMY_ALL,
        ALLY_MOVE,
        ALLY_ATTACK,
        ALLY_SUPPORT,
        ALLY_ALL,
        UTILITY
    }

}