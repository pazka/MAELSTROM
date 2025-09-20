using System.Numerics;
using DataViz.Core;
using DataViz.Data;

namespace DataViz.Examples
{
    /// <summary>
    /// Example of how to customize the skeleton application
    /// This shows various ways to create custom point behaviors
    /// </summary>
    public class CustomSkeletonExample : SkeletonApp
    {
        protected override void SetupPoints()
        {
            Console.WriteLine("Setting up custom points...");

            // Example 1: Create a spiral pattern
            CreateSpiralPattern();

            // Example 2: Create a wave pattern
            CreateWavePattern();

            // Example 3: Create some custom animated points
            CreateCustomAnimatedPoints();

            // Example 4: Create a grid with different behaviors
            CreateBehavioralGrid();

            Console.WriteLine($"Added {GetPointManager().Count} custom points");
        }

        private void CreateSpiralPattern()
        {
            Console.WriteLine("Creating spiral pattern...");

            int spiralPoints = 8;
            float spiralRadius = 3.0f;
            float spiralSpeed = 0.3f;

            for (int i = 0; i < spiralPoints; i++)
            {
                float angle = i * MathF.PI * 2.0f / spiralPoints;
                Vector2 position = new Vector2(
                    10.0f + MathF.Cos(angle) * spiralRadius,
                    8.0f + MathF.Sin(angle) * spiralRadius
                );

                var point = new CausticPoint(
                    position,
                    0.2f + i * 0.05f, // Increasing wall width
                    0.5f + i * 0.1f,   // Increasing agitation
                    GetSpiralColor(i, spiralPoints)
                );

                // Each point moves in a smaller circle
                GetPointManager().AddCircularPoint(point,
                    radius: 0.5f + i * 0.1f,
                    speed: spiralSpeed + i * 0.05f,
                    phase: i * 0.5f);
            }
        }

        private void CreateWavePattern()
        {
            Console.WriteLine("Creating wave pattern...");

            int wavePoints = 6;
            float waveAmplitude = 1.5f;
            float waveFrequency = 0.8f;

            for (int i = 0; i < wavePoints; i++)
            {
                Vector2 position = new Vector2(3.0f + i * 1.5f, 12.0f);

                var point = new CausticPoint(
                    position,
                    0.3f,
                    0.7f,
                    GetWaveColor(i, wavePoints)
                );

                // Different wave directions for variety
                Vector2 direction = i % 2 == 0 ? Vector2.UnitX : Vector2.UnitY;

                GetPointManager().AddWavePoint(point,
                    amplitude: waveAmplitude,
                    frequency: waveFrequency + i * 0.2f,
                    direction: direction);
            }
        }

        private void CreateCustomAnimatedPoints()
        {
            Console.WriteLine("Creating custom animated points...");

            // Create some points with custom movement patterns
            for (int i = 0; i < 4; i++)
            {
                Vector2 position = new Vector2(16.0f + i * 0.8f, 3.0f + i * 0.5f);

                var point = new CausticPoint(
                    position,
                    0.25f,
                    0.8f,
                    new Vector3(1.0f, 0.3f, 0.6f)
                );

                // Create a custom controller for figure-8 movement
                var customController = new FigureEightController(point, i, GetWorldBounds());
                GetPointManager().AddPoint(point, customController);
            }
        }

        private void CreateBehavioralGrid()
        {
            Console.WriteLine("Creating behavioral grid...");

            int rows = 3;
            int cols = 4;
            Vector2 spacing = new Vector2(2.0f, 2.0f);

            GetPointManager().GenerateGrid(rows, cols, spacing, (row, col) =>
            {
                Vector2 position = new Vector2(col * spacing.X, row * spacing.Y);

                var point = new CausticPoint(
                    position,
                    0.2f + (row + col) * 0.05f,
                    0.4f + (row + col) * 0.1f,
                    GetGridColor(row, col)
                );

                // Different behaviors based on position
                if (row == 0) // Top row - circular movement
                {
                    return new CircularMovementController(point, row * cols + col, GetWorldBounds(),
                        radius: 0.8f, speed: 0.6f, phase: (row + col) * 0.3f);
                }
                else if (row == 1) // Middle row - wave movement
                {
                    return new WaveMovementController(point, row * cols + col, GetWorldBounds(),
                        amplitude: 0.6f, frequency: 1.0f, direction: Vector2.UnitX);
                }
                else // Bottom row - random walk
                {
                    return new RandomWalkController(point, row * cols + col, GetWorldBounds(),
                        maxSpeed: 0.4f);
                }
            });
        }

        private Vector3 GetSpiralColor(int index, int total)
        {
            float t = (float)index / total;
            return new Vector3(
                0.5f + 0.5f * MathF.Sin(t * MathF.PI * 2.0f),
                0.5f + 0.5f * MathF.Sin(t * MathF.PI * 2.0f + 2.0f),
                0.5f + 0.5f * MathF.Sin(t * MathF.PI * 2.0f + 4.0f)
            );
        }

        private Vector3 GetWaveColor(int index, int total)
        {
            float t = (float)index / total;
            return new Vector3(
                0.2f + 0.8f * t,
                0.8f - 0.6f * t,
                0.4f + 0.6f * t
            );
        }

        private Vector3 GetGridColor(int row, int col)
        {
            return new Vector3(
                0.3f + 0.4f * (row / 3.0f),
                0.3f + 0.4f * (col / 4.0f),
                0.6f + 0.4f * ((row + col) / 7.0f)
            );
        }

        private Vector2 GetWorldBounds()
        {
            return new Vector2(20.0f, 15.0f);
        }
    }

    /// <summary>
    /// Custom point controller that creates figure-8 movement
    /// </summary>
    public class FigureEightController : PointController
    {
        private float _amplitude;
        private float _speed;
        private float _phase;

        public FigureEightController(CausticPoint originalPoint, int pointIndex, Vector2 worldBounds,
            float amplitude = 1.0f, float speed = 0.5f, float phase = 0.0f)
            : base(originalPoint, pointIndex, worldBounds)
        {
            _amplitude = amplitude;
            _speed = speed;
            _phase = phase;
        }

        public override CausticPoint UpdatePoint(float time)
        {
            _time = time;

            float t = time * _speed + _phase + _pointIndex * 0.2f;

            // Figure-8 parametric equations
            float x = _amplitude * MathF.Sin(t);
            float y = _amplitude * MathF.Sin(t) * MathF.Cos(t);

            Vector2 offset = new Vector2(x, y);
            Vector2 newPosition = _originalPoint.Position + offset;

            return new CausticPoint(
                newPosition,
                _originalPoint.WallWidth,
                _originalPoint.Agitation,
                _originalPoint.Color
            );
        }
    }
}
