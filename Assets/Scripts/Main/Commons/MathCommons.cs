using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Commons
{
    public static class MathCommons
    {
        public static int FloorMod(int x, int y)
        {
            return x - FloorDiv(x, y) * y;
        }
        public static int FloorDiv(int x, int y)
        {
            int r = x / y;
            if ((x ^ y) < 0 && (r * y != x))
                r--;
            return r;
        }
        public static void GetEllipseIntersection(float xr, float yr, float xPoint, float yPoint, out float xIntersection, out float yIntersection)
        {
            if (xPoint == 0 && yPoint == 0)
            {
                xIntersection = 0;
                yIntersection = 0;
            }
            else
            {
                var intersection = xr * yr / math.sqrt(math.pow(xr, 2) * math.pow(yPoint, 2) + math.pow(yr, 2) * math.pow(xPoint, 2));
                xIntersection = xPoint * intersection;
                yIntersection = yPoint * intersection;
            }
        }
        public static bool WithinEllipse(float xr, float yr, float xPoint, float yPoint, bool inclusive = false)
        {
            GetEllipseIntersection(xr, yr, xPoint, yPoint, out float xIntersection, out float yIntersection);
            var xr2 = math.abs(xIntersection);
            var yr2 = math.abs(yIntersection);
            return inclusive ? (math.abs(xPoint) <= xr2 && math.abs(yPoint) <= yr2) : (math.abs(xPoint) < xr2 && math.abs(yPoint) < yr2);
        }
    }
}