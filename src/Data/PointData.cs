using System.Numerics;

namespace DataViz.Data
{
    /// <summary>
    /// Represents a point in the caustics visualization with properties:
    /// A - Wall width around the point
    /// B - Agitation level of the cell
    /// C - Color of the cell
    /// </summary>
    public struct CausticPoint
    {
        public Vector2 Position;
        public float WallWidth;    // Property A
        public float Agitation;    // Property B
        public Vector3 Color;      // Property C (RGB)

        public CausticPoint(Vector2 position, float wallWidth, float agitation, Vector3 color)
        {
            Position = position;
            WallWidth = wallWidth;
            Agitation = agitation;
            Color = color;
        }
    }

    /// <summary>
    /// Collection of caustic points with utility methods
    /// </summary>
    public class CausticPointCollection
    {
        private readonly List<CausticPoint> _points = new();
        private readonly Random _random = new();

        public IReadOnlyList<CausticPoint> Points => _points.AsReadOnly();

        public void AddPoint(CausticPoint point)
        {
            _points.Add(point);
        }

        public void AddPoint(Vector2 position, float wallWidth, float agitation, Vector3 color)
        {
            _points.Add(new CausticPoint(position, wallWidth, agitation, color));
        }

        public void Clear()
        {
            _points.Clear();
        }

        /// <summary>
        /// Generate random points for testing
        /// </summary>
        public void GenerateRandomPoints(int count, Vector2 bounds)
        {
            _points.Clear();
            for (int i = 0; i < count; i++)
            {
                var position = new Vector2(
                    (float)_random.NextDouble() * bounds.X,
                    (float)_random.NextDouble() * bounds.Y
                );
                var wallWidth = 0.1f + (float)_random.NextDouble() * 0.3f;
                var agitation = (float)_random.NextDouble();
                var color = new Vector3(
                    (float)_random.NextDouble(),
                    (float)_random.NextDouble(),
                    (float)_random.NextDouble()
                );

                _points.Add(new CausticPoint(position, wallWidth, agitation, color));
            }
        }

        /// <summary>
        /// Generate a grid pattern of points
        /// </summary>
        public void GenerateGridPoints(int rows, int cols, Vector2 spacing)
        {
            _points.Clear();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var position = new Vector2(col * spacing.X, row * spacing.Y);
                    var wallWidth = 0.15f + (float)_random.NextDouble() * 0.2f;
                    var agitation = 0.3f + (float)_random.NextDouble() * 0.7f;
                    var color = new Vector3(
                        0.2f + (float)_random.NextDouble() * 0.6f,
                        0.4f + (float)_random.NextDouble() * 0.4f,
                        0.6f + (float)_random.NextDouble() * 0.4f
                    );

                    _points.Add(new CausticPoint(position, wallWidth, agitation, color));
                }
            }
        }
    }
}
