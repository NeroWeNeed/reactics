using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Reactics.Battle
{
    public static class MapUtils
    {
        public static void Expand(this Point point, ref MapHeader mapHeader, ref DynamicBuffer<MapTile> mapTiles, ushort amount, ref NativeList<Point> output)
        {
            output.Clear();

            for (int y = -amount; y <= amount; y++)
            {
                for (int x = -amount; x <= amount; x++)
                {
                    if ((abs(x) + abs(y)) > amount)
                        continue;
                    if (Point.SafeCreate(point.x + x, point.y + y, mapHeader.width, mapHeader.length, out Point p) && !mapTiles.GetTile(mapHeader, p).Value.inaccessible)
                    {
                        output.Add(p);
                    }

                }
            }

        }
        public static MapTile GetTile(this DynamicBuffer<MapTile> tiles, MapHeader header, Point point)
        {
            return tiles[point.y * header.width + point.x];
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
                output = new Point(x + amount, y);
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
                output = new Point(x, y + amount);
                return true;
            }
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
        public bool inaccessible;

    }

    public struct BlittableTile
    {
        public int elevation;
        public BlittableBool inaccessible;

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