using System.Numerics;
using DataViz.Data;

namespace DataViz.Core
{
    /// <summary>
    /// Detects and manages neighboring points for wall calculations
    /// Provides useful information for shader customization
    /// </summary>
    public class NeighborDetector
    {
        private List<CausticPoint> _points;
        private float _maxNeighborDistance;

        public NeighborDetector(List<CausticPoint> points, float maxNeighborDistance = 5.0f)
        {
            _points = points;
            _maxNeighborDistance = maxNeighborDistance;
        }

        /// <summary>
        /// Get all neighbors within the maximum distance of a given point
        /// </summary>
        public List<NeighborInfo> GetNeighbors(Vector2 position, int pointIndex)
        {
            var neighbors = new List<NeighborInfo>();

            for (int i = 0; i < _points.Count; i++)
            {
                if (i == pointIndex) continue;

                var point = _points[i];
                float distance = Vector2.Distance(position, point.Position);

                if (distance <= _maxNeighborDistance)
                {
                    neighbors.Add(new NeighborInfo
                    {
                        Point = point,
                        Index = i,
                        Distance = distance,
                        Direction = Vector2.Normalize(point.Position - position),
                        WallWidth = point.WallWidth,
                        Agitation = point.Agitation,
                        Color = point.Color
                    });
                }
            }

            return neighbors.OrderBy(n => n.Distance).ToList();
        }

        /// <summary>
        /// Get the closest neighbor to a given position
        /// </summary>
        public NeighborInfo? GetClosestNeighbor(Vector2 position, int pointIndex)
        {
            var neighbors = GetNeighbors(position, pointIndex);
            return neighbors.FirstOrDefault();
        }

        /// <summary>
        /// Calculate the perpendicular bisector between two points
        /// Useful for wall calculations
        /// </summary>
        public WallInfo CalculateWall(Vector2 pos1, Vector2 pos2, float wallWidth1, float wallWidth2)
        {
            Vector2 midpoint = (pos1 + pos2) * 0.5f;
            Vector2 direction = Vector2.Normalize(pos2 - pos1);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            float wallThickness = (wallWidth1 + wallWidth2) * 0.5f;

            return new WallInfo
            {
                Midpoint = midpoint,
                Direction = direction,
                Perpendicular = perpendicular,
                Thickness = wallThickness,
                Length = Vector2.Distance(pos1, pos2)
            };
        }

        /// <summary>
        /// Get all walls between a point and its neighbors
        /// </summary>
        public List<WallInfo> GetWalls(Vector2 position, int pointIndex, float wallWidth)
        {
            var walls = new List<WallInfo>();
            var neighbors = GetNeighbors(position, pointIndex);

            foreach (var neighbor in neighbors)
            {
                walls.Add(CalculateWall(position, neighbor.Point.Position, wallWidth, neighbor.WallWidth));
            }

            return walls;
        }

        /// <summary>
        /// Update the points list (call when points change)
        /// </summary>
        public void UpdatePoints(List<CausticPoint> newPoints)
        {
            _points = newPoints;
        }
    }

    /// <summary>
    /// Information about a neighboring point
    /// </summary>
    public struct NeighborInfo
    {
        public CausticPoint Point;
        public int Index;
        public float Distance;
        public Vector2 Direction;
        public float WallWidth;
        public float Agitation;
        public Vector3 Color;
    }

    /// <summary>
    /// Information about a wall between two points
    /// </summary>
    public struct WallInfo
    {
        public Vector2 Midpoint;
        public Vector2 Direction;
        public Vector2 Perpendicular;
        public float Thickness;
        public float Length;
    }
}
