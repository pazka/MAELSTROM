using System.Numerics;
using DataViz.Data;

namespace DataViz.Core
{
    /// <summary>
    /// Manages all points and their controllers
    /// Main interface for customizing point behavior
    /// </summary>
    public class PointManager
    {
        private List<PointController> _controllers;
        private NeighborDetector _neighborDetector;
        private Vector2 _worldBounds;
        private float _time;

        public PointManager(Vector2 worldBounds)
        {
            _worldBounds = worldBounds;
            _controllers = new List<PointController>();
            _neighborDetector = new NeighborDetector(new List<CausticPoint>(), 5.0f);
        }

        /// <summary>
        /// Add a point with a custom controller
        /// </summary>
        public void AddPoint(CausticPoint point, PointController controller)
        {
            _controllers.Add(controller);
            UpdateNeighborDetector();
        }

        /// <summary>
        /// Add a point with default controller (no movement)
        /// </summary>
        public void AddPoint(CausticPoint point)
        {
            var controller = new PointController(point, _controllers.Count, _worldBounds);
            AddPoint(point, controller);
        }

        /// <summary>
        /// Add a point with circular movement
        /// </summary>
        public void AddCircularPoint(CausticPoint point, float radius = 1.0f, float speed = 1.0f, float phase = 0.0f)
        {
            var controller = new CircularMovementController(point, _controllers.Count, _worldBounds, radius, speed, phase);
            AddPoint(point, controller);
        }

        /// <summary>
        /// Add a point with wave movement
        /// </summary>
        public void AddWavePoint(CausticPoint point, float amplitude = 0.5f, float frequency = 1.0f, Vector2? direction = null)
        {
            var controller = new WaveMovementController(point, _controllers.Count, _worldBounds, amplitude, frequency, direction);
            AddPoint(point, controller);
        }

        /// <summary>
        /// Add a point with random walk movement
        /// </summary>
        public void AddRandomWalkPoint(CausticPoint point, float maxSpeed = 0.5f)
        {
            var controller = new RandomWalkController(point, _controllers.Count, _worldBounds, maxSpeed);
            AddPoint(point, controller);
        }

        /// <summary>
        /// Get a specific point controller by index
        /// </summary>
        public PointController? GetController(int index)
        {
            if (index >= 0 && index < _controllers.Count)
                return _controllers[index];
            return null;
        }

        /// <summary>
        /// Replace a point controller
        /// </summary>
        public void SetController(int index, PointController controller)
        {
            if (index >= 0 && index < _controllers.Count)
            {
                _controllers[index] = controller;
            }
        }

        /// <summary>
        /// Update all points based on current time
        /// </summary>
        public List<CausticPoint> UpdatePoints(float time)
        {
            _time = time;
            var updatedPoints = new List<CausticPoint>();

            for (int i = 0; i < _controllers.Count; i++)
            {
                var updatedPoint = _controllers[i].UpdatePoint(time);
                updatedPoints.Add(updatedPoint);
            }

            _neighborDetector.UpdatePoints(updatedPoints);
            return updatedPoints;
        }

        /// <summary>
        /// Get current points (updated positions)
        /// </summary>
        public List<CausticPoint> GetCurrentPoints()
        {
            return UpdatePoints(_time);
        }

        /// <summary>
        /// Get neighbors for a specific point
        /// </summary>
        public List<NeighborInfo> GetNeighbors(int pointIndex)
        {
            var points = GetCurrentPoints();
            if (pointIndex >= 0 && pointIndex < points.Count)
            {
                return _neighborDetector.GetNeighbors(points[pointIndex].Position, pointIndex);
            }
            return new List<NeighborInfo>();
        }

        /// <summary>
        /// Get walls for a specific point
        /// </summary>
        public List<WallInfo> GetWalls(int pointIndex)
        {
            var points = GetCurrentPoints();
            if (pointIndex >= 0 && pointIndex < points.Count)
            {
                var point = points[pointIndex];
                return _neighborDetector.GetWalls(point.Position, pointIndex, point.WallWidth);
            }
            return new List<WallInfo>();
        }

        /// <summary>
        /// Get all walls in the system
        /// </summary>
        public List<WallInfo> GetAllWalls()
        {
            var allWalls = new List<WallInfo>();
            var points = GetCurrentPoints();

            for (int i = 0; i < points.Count; i++)
            {
                allWalls.AddRange(GetWalls(i));
            }

            return allWalls;
        }

        /// <summary>
        /// Clear all points
        /// </summary>
        public void Clear()
        {
            _controllers.Clear();
            _neighborDetector.UpdatePoints(new List<CausticPoint>());
        }

        /// <summary>
        /// Get the number of points
        /// </summary>
        public int Count => _controllers.Count;

        private void UpdateNeighborDetector()
        {
            var points = GetCurrentPoints();
            _neighborDetector.UpdatePoints(points);
        }

        /// <summary>
        /// Generate a grid of points with custom controllers
        /// </summary>
        public void GenerateGrid(int rows, int cols, Vector2 spacing, Func<int, int, PointController> controllerFactory)
        {
            Clear();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector2 position = new Vector2(col * spacing.X, row * spacing.Y);
                    var point = new CausticPoint(
                        position,
                        0.2f + (float)(new Random().NextDouble()) * 0.3f, // Random wall width
                        0.3f + (float)(new Random().NextDouble()) * 0.7f, // Random agitation
                        new Vector3(
                            0.2f + (float)(new Random().NextDouble()) * 0.6f,
                            0.4f + (float)(new Random().NextDouble()) * 0.4f,
                            0.6f + (float)(new Random().NextDouble()) * 0.4f
                        )
                    );

                    var controller = controllerFactory(row, col);
                    AddPoint(point, controller);
                }
            }
        }
    }
}
