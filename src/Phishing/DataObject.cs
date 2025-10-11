using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Phishing
{
    using Point = Vector2D<float>;
    /// <summary>
    /// Represents data/logic for an object that controls its display object
    /// </summary>
    public class DataObject
    {
        public DisplayObject DisplayObject { get; private set; }
        public Point Velocity { get; set; }
        public Point Position { get; private set; }
        public float Rotation { get; private set; }
        public float Scale { get; private set; }

        private readonly Random _random;
        private readonly Vector2D<int> _screenSize;

        public DataObject(DisplayObject displayObject, Point initialPosition, Point initialVelocity, Vector2D<int> screenSize)
        {
            DisplayObject = displayObject;
            Position = initialPosition;
            Velocity = initialVelocity;
            Rotation = 0.0f;
            Scale = 1.0f;
            _random = new Random();
            _screenSize = screenSize;

            // Set initial position in display object
            SetPosition(Position);
        }

        public void Update(float deltaTime)
        {
            return;
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
            SetScale(Scale);
        }

        public void SetVelocity(Point velocity)
        {
            Velocity = velocity;
        }

        public void SetScale(float scale)
        {
            Scale = scale;
            DisplayObject.SetScale(scale);
        }

        public void SetRotation(float rotation)
        {
            Rotation = rotation;
            DisplayObject.SetRotation(rotation);
        }

        public void SetPosition(Point p)
        {
            Position = p;
            Point normalizedPosition = new(p.X / _screenSize.X, p.Y / _screenSize.Y);
            DisplayObject.SetPosition(normalizedPosition);
        }
    }
}
