using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.GhostNet
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;

    public enum PointType
    {
        WhiteSlow = 0,
        MidBlueMedium = 1,
        JitteryBlue = 2
    }

    /// <summary>
    /// Represents data/logic for an object that controls its display object
    /// </summary>
    public class DataObject
    {
        public Point Velocity { get; set; }
        public Point Position { get; private set; }
        public float Rotation { get; private set; }
        public Dim PixelSize { get; private set; }
        
        // New properties
        public float AmplitudeA { get; set; }
        public float AmplitudeB { get; set; }
        public DateTime TimeCreated { get; private set; }
        public bool Visible { get; set; } = false;
        public PointType PointType { get; set; } = PointType.WhiteSlow;

        private readonly Random _random;
        private readonly Vector2D<int> _screenSize;

        public DataObject( Point initialPosition, Point initialVelocity, Vector2D<int> screenSize, Dim pixelSize, float amplitudeA = 1.0f, float amplitudeB = 1.0f, PointType pointType = PointType.WhiteSlow)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Rotation = 0.0f;
            _random = new Random();
            _screenSize = screenSize;
            PixelSize = pixelSize;
            AmplitudeA = amplitudeA;
            AmplitudeB = amplitudeB;
            TimeCreated = DateTime.Now;
            PointType = pointType;

            // Calculate screen scale based on pixel size
            // Convert pixel size to normalized screen coordinates
            Dim screenScale = new(
                PixelSize.X / _screenSize.X,
                PixelSize.Y / _screenSize.Y
            );

            // Set initial position in display object
            SetPosition(Position);
            
        }

        public void Update(float deltaTime)
        {
            if (!Visible) return;

            // Calculate time since creation
            float timeSinceCreation = (float)(DateTime.Now - TimeCreated).TotalSeconds;
            
            // Calculate maelstrom value based on point type
            float maelstrom = 0.0f;
            switch (PointType)
            {
                case PointType.WhiteSlow:
                    maelstrom = 0.0f; // Always white
                    break;
                case PointType.MidBlueMedium:
                    maelstrom = 0.5f; // Mid blue
                    break;
                case PointType.JitteryBlue:
                    maelstrom = 1.0f; // Full blue
                    break;
            }
            
            // Add pulsing effect to maelstrom (slow pulse over 4 seconds)
            float pulse = (MathF.Sin(timeSinceCreation * 0.5f) + 1.0f) * 0.1f; // 0.0 to 0.2 pulse
            maelstrom = Math.Max(0.0f, Math.Min(1.0f, maelstrom + pulse));

            // Elliptical movement behavior with different speeds
            Point center = new Point(_screenSize.X / 2.0f, _screenSize.Y / 2.0f);
            
            // Different speeds based on point type
            float speedMultiplier = 1.0f;
            switch (PointType)
            {
                case PointType.WhiteSlow:
                    speedMultiplier = 0.5f; // Slow
                    break;
                case PointType.MidBlueMedium:
                    speedMultiplier = 1.0f; // Medium
                    break;
                case PointType.JitteryBlue:
                    speedMultiplier = 1.5f; // Fast
                    break;
            }
            
            // Calculate elliptical position
            float angle = timeSinceCreation * AmplitudeB * speedMultiplier;
            float radiusX = AmplitudeA * 100;
            float radiusY = AmplitudeA * 100;
            
            // Calculate elliptical position
            Point ellipticalPosition = new Point(
                center.X + radiusX * MathF.Cos(angle),
                center.Y + radiusY * MathF.Sin(angle)
            );
            
            // Add jittery movement for JitteryBlue type
            if (PointType == PointType.JitteryBlue)
            {
                float jitterAmount = 10.0f; // Constant jitter for this type
                ellipticalPosition.X += (float)(_random.NextDouble() - 0.5) * jitterAmount;
                ellipticalPosition.Y += (float)(_random.NextDouble() - 0.5) * jitterAmount;
            }
            
            // Keep within screen bounds
            ellipticalPosition.X = Math.Max(0, Math.Min(_screenSize.X, ellipticalPosition.X));
            ellipticalPosition.Y = Math.Max(0, Math.Min(_screenSize.Y, ellipticalPosition.Y));
            
            Position = ellipticalPosition;

            // Update display object
            SetPosition(Position);
            SetRotation(Rotation);
        }

        public void SetVelocity(Point velocity)
        {
            Velocity = velocity;
        }

        public void SetPixelSize(Dim pixelSize)
        {
            PixelSize = pixelSize;
            // Recalculate screen scale based on new pixel size
            Dim screenScale = new(
                PixelSize.X / _screenSize.X,
                PixelSize.Y / _screenSize.Y
            );
        }

        public void SetRotation(float rotation)
        {
            Rotation = rotation;
        }

        public void SetPosition(Point p)
        {
            Position = p;
            // Convert pixel position to normalized coordinates (-1 to 1)
            // Position 0 maps to -1 (screen start), Position screenSize maps to 1 (screen end)
            Point normalizedPosition = new(
                (p.X / _screenSize.X) * 2.0f - 1.0f,
                (p.Y / _screenSize.Y) * 2.0f - 1.0f
            );
        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
        }

        public bool ShouldBeDestroyed()
        {
            float age = (float)(DateTime.Now - TimeCreated).TotalSeconds;
            
            // Different lifespans based on point type for natural variation
            float maxLifespan = 0.0f;
            switch (PointType)
            {
                case PointType.WhiteSlow:
                    maxLifespan = 15.0f; // White objects live longer
                    break;
                case PointType.MidBlueMedium:
                    maxLifespan = 10.0f; // Medium lifespan
                    break;
                case PointType.JitteryBlue:
                    maxLifespan = 8.0f; // Jittery objects live shorter
                    break;
            }
            
            return age > maxLifespan;
        }
    }
}
