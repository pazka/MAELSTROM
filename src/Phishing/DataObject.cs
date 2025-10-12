using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Phishing
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;
    /// <summary>
    /// Represents data/logic for an object that controls its display object
    /// </summary>
    public class DataObject
    {
        public DisplayObject DisplayObject { get; private set; }
        public Point Velocity { get; set; }
        public Point Position { get; private set; }
        public float Rotation { get; private set; }
        public Dim PixelSize { get; private set; }

        private readonly Random _random;
        private readonly Vector2D<int> _screenSize;

        public DataObject(DisplayObject displayObject, Point initialPosition, Point initialVelocity, Vector2D<int> screenSize, Dim pixelSize)
        {
            DisplayObject = displayObject;
            Position = initialPosition;
            Velocity = initialVelocity;
            Rotation = 0.0f;
            _random = new Random();
            _screenSize = screenSize;
            PixelSize = pixelSize;

            // Calculate screen scale based on pixel size
            // Convert pixel size to normalized screen coordinates
            Dim screenScale = new(
                PixelSize.X / _screenSize.X,
                PixelSize.Y / _screenSize.Y
            );
            DisplayObject.SetScreenScale(screenScale);

            // Set initial position in display object
            SetPosition(Position);
        }

        public void Update(float deltaTime)
        {
            // Update position based on velocity
            Position += Velocity * deltaTime;

            // Wrap around screen edges
            if (Position.X < 0) Position = new Point(_screenSize.X, Position.Y);
            if (Position.X > _screenSize.X) Position = new Point(0, Position.Y);
            if (Position.Y < 0) Position = new Point(Position.X, _screenSize.Y);
            if (Position.Y > _screenSize.Y) Position = new Point(Position.X, 0);

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
            DisplayObject.SetScreenScale(screenScale);
        }

        public void SetRotation(float rotation)
        {
            Rotation = rotation;
            DisplayObject.SetRotation(rotation);
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
            DisplayObject.SetPosition(normalizedPosition);
        }
    }
}
