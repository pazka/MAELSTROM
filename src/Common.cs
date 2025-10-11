using Silk.NET.Maths;

namespace Maelstrom
{
    using Point = Vector2D<float>;

    public static class Utils
    {
        public static float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }

        public static Point Normalized(Point point)
        {
            return point / point.Length;
        }
    }
}