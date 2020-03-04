using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Reactics.Battle
{
    public static class MapUtils
    {

        public static Mesh GenerateMesh(this ref MapBlob map, float tileSize = 1f,float elevationStep = 0.25f)
        {

            int vertexCount = map.Width * map.Length * 4;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[map.Width * map.Length * 6];
            int x, y, index;
            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    index = (y * map.Width + x) * 4;

                    vertices[index] = new Vector3(x * tileSize, map[x, y].Elevation * elevationStep, y * tileSize);
                    uv[index] = new Vector2((float)x / map.Width, (float)y / map.Length);
                    normals[index] = Vector3.up;

                    vertices[index + 1] = new Vector3((x + 1) * tileSize, map[x, y].Elevation * elevationStep, y * tileSize);
                    uv[index + 1] = new Vector2(((float)x + 1) / map.Width, (float)y / map.Length);
                    normals[index + 1] = Vector3.up;

                    vertices[index + 2] = new Vector3(x * tileSize, map[x, y].Elevation * elevationStep, (y + 1) * tileSize);
                    uv[index + 2] = new Vector2((float)x / map.Width, ((float)y + 1) / map.Length);
                    normals[index + 2] = Vector3.up;

                    vertices[index + 3] = new Vector3((x + 1) * tileSize, map[x, y].Elevation * elevationStep, (y + 1) * tileSize);
                    uv[index + 3] = new Vector2(((float)x + 1) / map.Width, ((float)y + 1) / map.Length);
                    normals[index + 3] = Vector3.up;
                }
            }
            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    GenerateMeshTileTriangles(triangles, (y * map.Width + x) * 6, x, y, map.Width);
                }
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;
            mesh.normals = normals;
            return mesh;
        }


        public static void GenerateMeshTileTriangles(int[] triangles, int index, int x, int y, int stride)
        {
            triangles[index] = (y * stride + x) * 4;
            triangles[index + 1] = ((y * stride + x) * 4) + 2;
            triangles[index + 2] = ((y * stride + x) * 4) + 1;
            triangles[index + 3] = ((y * stride + x) * 4) + 2;
            triangles[index + 4] = ((y * stride + x) * 4) + 3;
            triangles[index + 5] = ((y * stride + x) * 4) + 1;
        }
        public static void GenerateMeshTileTriangles(ref NativeArray<int> triangles, int index, int x, int y, int stride)
        {
            triangles[index] = (y * stride + x) * 4;
            triangles[index + 1] = ((y * stride + x) * 4) + 2;
            triangles[index + 2] = ((y * stride + x) * 4) + 1;
            triangles[index + 3] = ((y * stride + x) * 4) + 2;
            triangles[index + 4] = ((y * stride + x) * 4) + 3;
            triangles[index + 5] = ((y * stride + x) * 4) + 1;
        }

        public static void GenerateMeshTileTriangles(int[] triangles, int index, int stride, params Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                GenerateMeshTileTriangles(triangles, index + (i * 6), points[i].x, points[i].y, stride);
            }
        }
        public static void GenerateMeshTileTriangles(int[] triangles, int index, int stride, IEnumerator<Point> points)
        {
            for (int i = 0; points.MoveNext(); i++)
            {
                GenerateMeshTileTriangles(triangles, index + (i * 6), points.Current.x, points.Current.y, stride);
            }

        }
        public static void GenerateMeshTileTriangles(ref NativeArray<int> triangles, int index, int stride, IEnumerator<Point> points)
        {
            for (int i = 0; points.MoveNext(); i++)
            {
                GenerateMeshTileTriangles(ref triangles, index + (i * 6), points.Current.x, points.Current.y, stride);
            }

        }

        public static NativeList<Point> Expand(this ref Point point, ref MapBlob map, ushort amount, PathExpandPattern pattern, Allocator allocator)
        {
            NativeList<Point> output = new NativeList<Point>(allocator);
            Point p;
            for (int y = -amount; y <= amount; y++)
            {
                for (int x = -amount; x <= amount; x++)
                {
                    switch (pattern)
                    {
                        case PathExpandPattern.DIAMOND:
                            if ((abs(x) + abs(y)) > amount)
                                continue;
                            if (Point.SafeCreate(point.x + x, point.y + y, map.width, map.length, out p) && !map[p].Inaccessible)
                            {
                                output.Add(p);
                            }
                            break;
                        case PathExpandPattern.SQUARE:
                            if (Point.SafeCreate(point.x + x, point.y + y, map.width, map.length, out p) && !map[p].Inaccessible)
                            {
                                output.Add(p);
                            }
                            break;
                    }


                }
            }

            return output;
        }

        /* 
                public static void Expand(this Point point, ref MapHeader mapHeader, ref DynamicBuffer<MapTile> mapTiles, ushort amount, PathExpandPattern pattern, ref NativeList<Point> output)
                {

                    output.Clear();
                    Point p;
                    for (int y = -amount; y <= amount; y++)
                    {
                        for (int x = -amount; x <= amount; x++)
                        {
                            switch (pattern)
                            {
                                case PathExpandPattern.DIAMOND:
                                    if ((abs(x) + abs(y)) > amount)
                                        continue;
                                    if (Point.SafeCreate(point.x + x, point.y + y, mapHeader.width, mapHeader.length, out p) && !mapTiles.GetTile(mapHeader, p).Value.inaccessible)
                                    {
                                        output.Add(p);
                                    }
                                    break;
                                case PathExpandPattern.SQUARE:
                                    if (Point.SafeCreate(point.x + x, point.y + y, mapHeader.width, mapHeader.length, out p) && !mapTiles.GetTile(mapHeader, p).Value.inaccessible)
                                    {
                                        output.Add(p);
                                    }
                                    break;
                            }


                        }
                    }

                }
         */




    }

    public enum PathExpandPattern
    {
        DIAMOND, SQUARE
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

        public Point ShiftX(int amount)
        {
            return new Point(x + amount, y);
        }
        public bool SafeShiftX(int amount, int max, out Point output)
        {
            int newX = x + amount;
            if (newX < 0 || newX >= max)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX, y);
                return true;
            }
        }
        public Point ShiftY(int amount)
        {
            return new Point(x, y + amount);
        }
        public bool SafeShiftY(int amount, int max, out Point output)
        {
            int newY = y + amount;
            if (newY < 0 || newY >= max)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(x, newY);
                return true;
            }
        }

        public Point Shift(int xAmount,int yAmount) {
            return new Point(x+xAmount,y+yAmount);
        }

        public bool SafeShift(int xAmount,int yAmount, int xMax,int yMax, out Point output)
        {
            int newY = y + yAmount;
            int newX = y + xAmount;
            if (newY < 0 || newX < 0 || newX >= xMax || newY >= yMax)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX,newY);
                return true;
            }
        }

        public uint Distance(Point other) {
            return (uint) (abs(x-other.x)+ abs(y-other.y));
        }

        public static bool SafeCreate(int x, int y, int maxX, int maxY, out Point output)
        {
            if (x < 0 || x >= maxX || y < 0 || y >= maxY)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(x, y);
                return true;
            }
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

        public static Point operator +(Point thisPoint, Point otherPoint) => new Point(thisPoint.x + otherPoint.x, thisPoint.y + otherPoint.y);

        public static Point operator -(Point thisPoint, Point otherPoint) => new Point(thisPoint.x - otherPoint.x, thisPoint.y - otherPoint.y);
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

        private PointComparerByX() { }
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
        private PointComparerByY() { }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SpawnGroup : IMapSpawnGroup
    {
        [SerializeField]
        public Point[] points;

        public Point this[int index] => points[index];

        public int Count => points.Length;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Tile : IMapTile
    {
        public int elevation;


        public bool inaccessible;

        public int Elevation => elevation;

        public bool Inaccessible => inaccessible;
    }

    public struct BlittableTile : IMapTile
    {

        public int elevation;

        public BlittableBool inaccessible;
        public int Elevation { get => elevation; }

        public bool Inaccessible { get => inaccessible; }

        public static implicit operator Tile(BlittableTile tile) => new Tile
        {
            elevation = tile.elevation,
            inaccessible = tile.inaccessible
        };


        public static explicit operator BlittableTile(Tile tile) => new BlittableTile
        {
            elevation = tile.elevation,
            inaccessible = tile.inaccessible
        };

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