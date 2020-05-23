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
    }
}