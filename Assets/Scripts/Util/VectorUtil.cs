using UnityEngine;

namespace Reactics.Util
{
    public static class VectorUtils
    {
        public static Vector2 InvertY(this Vector2 vector, float height) => new Vector2(vector.x, height - vector.y);
        public static Vector2 InvertX(this Vector2 vector, float width) => new Vector2(width - vector.x, vector.y);
    }
}