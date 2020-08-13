using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
namespace Reactics.Core.Commons {
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point> {
        public static readonly Point zero = new Point(0, 0);

        public ushort x;
        public ushort y;

        public Point(ushort x, ushort y) {
            this.x = x;
            this.y = y;
        }
        public Point(uint2 coordinates) {

            this.x = (ushort)coordinates.x;
            this.y = (ushort)coordinates.y;
        }
        public Point(int x, int y) {
            this.x = Convert.ToUInt16(x);
            this.y = Convert.ToUInt16(y);
        }

        public bool ComparePoints(Point comparePoint) {
            if (this.x == comparePoint.x && this.y == comparePoint.y)
                return true;
            return false;
        }

        public bool InRange(Point comparePoint, ushort range) {
            if (this.Distance(comparePoint) <= range)
                return true;
            else
                return false;
        }

        public void GetPathToTargetTile(ref NativeList<Point> fullPathToTargetPoint, Point targetPoint, int range, ushort maxLength, ushort maxWidth, double leniency) {
            //idk why this would happen but it could I guess
            if (this.ComparePoints(targetPoint))
                return;

            if (this.x == targetPoint.x) {
                if (targetPoint.y > this.y) //2 > 1
                {
                    for (int y = this.y; y <= targetPoint.y; y++) {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
                else {
                    for (int y = this.y; y >= targetPoint.y; y--) {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
            }
            else if (this.y == targetPoint.y) {
                if (targetPoint.x > this.x) {
                    for (int x = this.x; x <= targetPoint.x; x++) {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
                else {
                    for (int x = this.x; x >= targetPoint.x; x--) {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
            }
            else {
                int x0 = this.x;
                int y0 = this.y;
                int x1 = targetPoint.x;
                int y1 = targetPoint.y;
                bool steep = math.abs(targetPoint.y - this.y) > math.abs(targetPoint.x - this.x);

                if (steep) {
                    var temp = x0;
                    x0 = y0;
                    y0 = temp;

                    temp = x1;
                    x1 = y1;
                    y1 = temp;
                }
                if (x0 > x1) {
                    //Note that this saves the list from target -> origin, instead of origin -> target
                    var temp = x0;
                    x0 = x1;
                    x1 = temp;

                    temp = y0;
                    y0 = y1;
                    y1 = temp;
                }

                //Don't trust this at all. Debug it. gradient probably being converted to some int or something.
                double dx = x1 - x0;
                double dy = y1 - y0;
                double gradient = dy / dx;
                if (dx == 0)
                    gradient = 1.0;

                //First endpoint
                var xEnd = math.floor(x0 + 0.5);
                var yEnd = y0 + gradient * (xEnd - x0);
                var xGap = 1 - ((x0 + 0.5) - math.floor(x0 + 0.5));

                var xpxl1 = xEnd;
                var ypxl1 = math.floor(yEnd);
                var firstEndPoint = new Point(0, 0);

                if (steep) {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        firstEndPoint = new Point((int)ypxl1, (int)xpxl1);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        firstEndPoint = new Point((int)ypxl1 + 1, (int)xpxl1);
                }
                else {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        firstEndPoint = new Point((int)xpxl1, (int)ypxl1);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        firstEndPoint = new Point((int)xpxl1, (int)ypxl1 + 1);
                }
                var interY = yEnd + gradient;

                //Second endpoint
                xEnd = math.floor(x1 + 0.5);
                yEnd = y1 + gradient * (xEnd - x1);
                xGap = 1 - ((x1 + 0.5) - math.floor(x1 + 0.5));

                var xpxl2 = xEnd;
                var ypxl2 = math.floor(yEnd);
                Point secondEndPoint = new Point(0, 0);

                if (steep) {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        secondEndPoint = new Point((int)ypxl2, (int)xpxl2);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        secondEndPoint = new Point((int)ypxl2 + 1, (int)xpxl2);
                }
                else {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        secondEndPoint = new Point((int)xpxl2, (int)ypxl2);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        secondEndPoint = new Point((int)xpxl2, (int)ypxl2 + 1);
                }

                //not sure if right
                fullPathToTargetPoint.Add(firstEndPoint);

                //main loop
                if (steep) {
                    for (int x = (int)xpxl1 + 1; x < (int)xpxl2; x++) {
                        var thing = 1 - ((interY) - math.floor(interY));
                        if (thing > leniency)
                            fullPathToTargetPoint.Add(new Point((ushort)math.floor(interY), x));
                        var thing2 = interY - math.floor(interY);
                        if (thing2 > leniency)
                            fullPathToTargetPoint.Add(new Point((ushort)math.floor(interY) + 1, x));
                        interY += gradient;
                    }
                }
                else {
                    for (int x = (int)xpxl1 + 1; x < (int)xpxl2; x++) {
                        var thing = 1 - ((interY) - math.floor(interY));
                        if (thing > leniency)
                            fullPathToTargetPoint.Add(new Point(x, (ushort)math.floor(interY)));
                        var thing2 = interY - math.floor(interY);
                        if (thing2 > leniency)
                            fullPathToTargetPoint.Add(new Point(x, (ushort)math.floor(interY) + 1));
                        interY += gradient;
                    }
                }

                fullPathToTargetPoint.Add(secondEndPoint);
            }
        }

        public Point ShiftX(int amount) {
            return new Point(x + amount, y);
        }
        public bool ShiftX(int amount, int max, out Point output) {
            int newX = x + amount;
            if (newX < 0 || newX >= max) {
                output = default;
                return false;
            }
            else {
                output = new Point(newX, y);
                return true;
            }
        }
        public Point ShiftY(int amount) {
            return new Point(x, y + amount);
        }
        public bool ShiftY(int amount, int max, out Point output) {
            int newY = y + amount;
            if (newY < 0 || newY >= max) {
                output = default;
                return false;
            }
            else {
                output = new Point(x, newY);
                return true;
            }
        }

        public Point Shift(int xAmount, int yAmount) {
            return new Point(x + xAmount, y + yAmount);
        }

        public bool TryShift(int xAmount, int yAmount, int xMax, int yMax, out Point output) {
            int newY = y + yAmount;
            int newX = x + xAmount;
            if (newY < 0 || newX < 0 || newX >= xMax || newY >= yMax) {
                output = default;
                return false;
            }
            else {
                output = new Point(newX, newY);
                return true;
            }
        }
        public bool TryShift(sbyte xAmount, sbyte yAmount, int xMax, int yMax, out Point output) {
            int newY = y + yAmount;
            int newX = x + xAmount;
            if (newY < 0 || newX < 0 || newX >= xMax || newY >= yMax) {
                output = default;
                return false;
            }
            else {
                output = new Point(newX, newY);
                return true;
            }
        }

        public float Distance(Point other) {
            return math.distance(new float2(x, y), new float2(other.x, other.y));

        }
        public int ManhattanDistance(Point other) {
            return math.abs(x - other.x) + math.abs(y - other.y);
        }
        public static Point FromIndex(int index, ushort width) {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            return new Point(index % width, index / width);
        }

        public static bool Create(int x, int y, int maxX, int maxY, out Point output) {
            if (x < 0 || x >= maxX || y < 0 || y >= maxY) {
                output = default;
                return false;
            }
            else {
                output = new Point(x, y);
                return true;
            }
        }



        public bool Equals(Point other) {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj) {
            return obj is Point point && Equals(point);
        }

        public override int GetHashCode() {
            int hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            return "Point(" + x + "," + y + ")";
        }

        public static Point operator +(Point thisPoint, Point otherPoint) => new Point(thisPoint.x + otherPoint.x, thisPoint.y + otherPoint.y);

        public static Point operator -(Point thisPoint, Point otherPoint) => new Point(thisPoint.x - otherPoint.x, thisPoint.y - otherPoint.y);
        public static bool operator ==(Point thisPoint, Point otherPoint) => thisPoint.Equals(otherPoint);
        public static bool operator !=(Point thisPoint, Point otherPoint) => !thisPoint.Equals(otherPoint);
        public static implicit operator Vector2Int(Point point) => new Vector2Int(Convert.ToInt32(point.x), Convert.ToInt32(point.y));
        public static explicit operator Point(Vector2Int vector) {
            if (vector.x >= 0 && vector.y >= 0) {
                if (vector.x == 0 && vector.y == 0)
                    return zero;
                else
                    return new Point((ushort)vector.x, (ushort)vector.y);
            }
            else {
                throw new InvalidCastException("Vector must be positive");
            }
        }

        public static NativeArray<Point> CreateMapPointSet(ushort width, ushort length, Allocator allocator = Allocator.Temp) {
            var result = new NativeArray<Point>(width * length, allocator);
            for (int i = 0; i < result.Length; i++) {
                result[i] = Point.FromIndex(i, width);
            }
            return result;
        }


    }

    public class PointComparerXAxis : IComparer<Point> {
        public int Compare(Point x, Point y) {
            var comparison = x.x.CompareTo(y.x);
            return comparison != 0 ? comparison : x.y.CompareTo(y.y);
        }
    }
    public class PointComparerYAxis : IComparer<Point> {
        public int Compare(Point x, Point y) {
            var comparison = x.y.CompareTo(y.y);
            return comparison != 0 ? comparison : x.x.CompareTo(y.x);
        }
    }
}