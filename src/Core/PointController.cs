using System.Numerics;
using DataViz.Data;

namespace DataViz.Core
{
    /// <summary>
    /// Controller for individual point behavior and movement
    /// Override this class to create custom point movement patterns
    /// </summary>
    public class PointController
    {
        protected CausticPoint _originalPoint;
        protected float _time;
        protected int _pointIndex;
        protected Vector2 _worldBounds;

        public PointController(CausticPoint originalPoint, int pointIndex, Vector2 worldBounds)
        {
            _originalPoint = originalPoint;
            _pointIndex = pointIndex;
            _worldBounds = worldBounds;
        }

        /// <summary>
        /// Update the point position based on time and custom logic
        /// Override this method to implement custom movement patterns
        /// </summary>
        public virtual CausticPoint UpdatePoint(float time)
        {
            _time = time;

            // Default: return original point (no movement)
            return _originalPoint;
        }

        /// <summary>
        /// Get the current position of the point
        /// </summary>
        public Vector2 GetCurrentPosition()
        {
            return UpdatePoint(_time).Position;
        }

        /// <summary>
        /// Get the original point data
        /// </summary>
        public CausticPoint GetOriginalPoint()
        {
            return _originalPoint;
        }

        /// <summary>
        /// Get point index for identification
        /// </summary>
        public int GetPointIndex()
        {
            return _pointIndex;
        }
    }

    /// <summary>
    /// Example: Circular movement controller
    /// </summary>
    public class CircularMovementController : PointController
    {
        private float _radius;
        private float _speed;
        private float _phase;

        public CircularMovementController(CausticPoint originalPoint, int pointIndex, Vector2 worldBounds,
            float radius = 1.0f, float speed = 1.0f, float phase = 0.0f)
            : base(originalPoint, pointIndex, worldBounds)
        {
            _radius = radius;
            _speed = speed;
            _phase = phase;
        }

        public override CausticPoint UpdatePoint(float time)
        {
            _time = time;

            float angle = time * _speed + _phase + _pointIndex * 0.5f;
            Vector2 offset = new Vector2(
                MathF.Cos(angle) * _radius,
                MathF.Sin(angle) * _radius
            );

            Vector2 newPosition = _originalPoint.Position + offset;

            return new CausticPoint(
                newPosition,
                _originalPoint.WallWidth,
                _originalPoint.Agitation,
                _originalPoint.Color
            );
        }
    }

    /// <summary>
    /// Example: Wave movement controller
    /// </summary>
    public class WaveMovementController : PointController
    {
        private float _amplitude;
        private float _frequency;
        private Vector2 _direction;

        public WaveMovementController(CausticPoint originalPoint, int pointIndex, Vector2 worldBounds,
            float amplitude = 0.5f, float frequency = 1.0f, Vector2? direction = null)
            : base(originalPoint, pointIndex, worldBounds)
        {
            _amplitude = amplitude;
            _frequency = frequency;
            _direction = direction ?? Vector2.UnitX;
        }

        public override CausticPoint UpdatePoint(float time)
        {
            _time = time;

            float wave = MathF.Sin(time * _frequency + _pointIndex * 0.3f) * _amplitude;
            Vector2 offset = _direction * wave;

            Vector2 newPosition = _originalPoint.Position + offset;

            return new CausticPoint(
                newPosition,
                _originalPoint.WallWidth,
                _originalPoint.Agitation,
                _originalPoint.Color
            );
        }
    }

    /// <summary>
    /// Example: Random walk controller
    /// </summary>
    public class RandomWalkController : PointController
    {
        private Vector2 _velocity;
        private float _maxSpeed;
        private Random _random;

        public RandomWalkController(CausticPoint originalPoint, int pointIndex, Vector2 worldBounds,
            float maxSpeed = 0.5f) : base(originalPoint, pointIndex, worldBounds)
        {
            _maxSpeed = maxSpeed;
            _random = new Random(pointIndex); // Use point index as seed for consistent randomness
            _velocity = new Vector2(
                (float)(_random.NextDouble() - 0.5) * 2.0f,
                (float)(_random.NextDouble() - 0.5) * 2.0f
            ) * _maxSpeed;
        }

        public override CausticPoint UpdatePoint(float time)
        {
            _time = time;

            // Randomly change direction occasionally
            if (_random.NextDouble() < 0.01f) // 1% chance per frame
            {
                _velocity = new Vector2(
                    (float)(_random.NextDouble() - 0.5) * 2.0f,
                    (float)(_random.NextDouble() - 0.5) * 2.0f
                ) * _maxSpeed;
            }

            Vector2 newPosition = _originalPoint.Position + _velocity * 0.016f; // Assuming ~60fps

            // Keep within world bounds
            newPosition.X = Math.Clamp(newPosition.X, 0, _worldBounds.X);
            newPosition.Y = Math.Clamp(newPosition.Y, 0, _worldBounds.Y);

            return new CausticPoint(
                newPosition,
                _originalPoint.WallWidth,
                _originalPoint.Agitation,
                _originalPoint.Color
            );
        }
    }
}
