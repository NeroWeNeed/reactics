using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.Commons {
    public static class MathUtility {
        /// <summary>
        /// Finds point of intersection given two position vectors and two direction vectors.
        /// Reference: http://wiki.unity3d.com/index.php/3d_Math_functions
        /// </summary>
        /// <param name="pos1">Position Vector 1</param>
        /// <param name="dir1">Direction Vector 1</param>
        /// <param name="pos2">Position Vector 2</param>
        /// <param name="dir2">Direction Vector 2</param>
        /// <param name="intersection">Point of intersection if vectors intersect</param>
        /// <returns>True if vectors intersect, false otherwise</returns>
        public static bool Intersects(float3 pos1, float3 dir1, float3 pos2, float3 dir2, out float3 intersection) {
            float3 dir3 = pos2 - pos1;
            float3 crossDir1And2 = math.cross(dir1, dir2);
            float3 crossDir3and2 = math.cross(dir3, dir2);
            float planarFactor = math.dot(dir3, crossDir1And2);
            var crossDir1And2SqrMagnitude = math.sqrt(math.dot(crossDir1And2, crossDir1And2));
            //is coplanar, and not parrallel
            if (math.abs(planarFactor) < 0.0001f && crossDir1And2SqrMagnitude > 0.0001f) {
                float s = math.dot(crossDir3and2, crossDir1And2) / crossDir1And2SqrMagnitude;
                intersection = pos1 + (dir1 * s);
                return true;
            }
            else {
                intersection = default;
                return false;
            }
        }
    }
    public static class MathCommons {
        public static int FloorMod(int x, int y) {
            return x - FloorDiv(x, y) * y;
        }
        public static int FloorDiv(int x, int y) {
            int r = x / y;
            if ((x ^ y) < 0 && (r * y != x))
                r--;
            return r;
        }
        public static void GetEllipseIntersection(float xr, float yr, float xPoint, float yPoint, out float xIntersection, out float yIntersection) {
            if (xPoint == 0 && yPoint == 0) {
                xIntersection = 0;
                yIntersection = 0;
            }
            else {
                var intersection = xr * yr / math.sqrt(math.pow(xr, 2) * math.pow(yPoint, 2) + math.pow(yr, 2) * math.pow(xPoint, 2));
                xIntersection = xPoint * intersection;
                yIntersection = yPoint * intersection;
            }
        }
        public static bool WithinEllipse(float xr, float yr, float xPoint, float yPoint, bool inclusive = false) {
            GetEllipseIntersection(xr, yr, xPoint, yPoint, out float xIntersection, out float yIntersection);
            var xr2 = math.abs(xIntersection);
            var yr2 = math.abs(yIntersection);
            return inclusive ? (math.abs(xPoint) <= xr2 && math.abs(yPoint) <= yr2) : (math.abs(xPoint) < xr2 && math.abs(yPoint) < yr2);
        }

    }
}