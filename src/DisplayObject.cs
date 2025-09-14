using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace maelstrom_poc
{
    /// <summary>
    /// Represents an object that displays a shader at a specific world position
    /// </summary>
    public class DisplayObject
    {
        private readonly GL _gl;
        private readonly uint _vao;
        private readonly uint _vbo;
        private readonly uint _ebo;
        private readonly uint _shaderProgram;
        private readonly int _positionUniformLocation;
        private readonly int _timeUniformLocation;

        public Vector2D<float> Position { get; set; }
        public Vector2D<float> Scale { get; set; } = new Vector2D<float>(1.0f, 1.0f);
        public float Rotation { get; set; } = 0.0f;

        // Movement properties
        private Vector2D<float> _velocity;
        private Vector2D<float> _targetPosition;
        private float _moveTimer;
        private readonly Random _random;

        public unsafe DisplayObject(GL gl, uint shaderProgram, Vector2D<float> position)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            Position = position;
            _random = new Random();

            // Initialize movement
            _targetPosition = position;
            _velocity = Vector2D<float>.Zero;
            _moveTimer = 0.0f;

            // Get uniform locations
            _positionUniformLocation = _gl.GetUniformLocation(_shaderProgram, "uObjectPosition");
            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");

            // Create quad geometry (centered at origin, will be transformed by position)
            float[] vertices = {
                // Position     // UV Coords
                -0.5f, -0.5f,   0.0f, 0.0f,  // Bottom-left
                 0.5f, -0.5f,   1.0f, 0.0f,  // Bottom-right
                 0.5f,  0.5f,   1.0f, 1.0f,  // Top-right
                -0.5f,  0.5f,   0.0f, 1.0f   // Top-left
            };

            uint[] indices = {
                0, 1, 2,  // First triangle
                2, 3, 0   // Second triangle
            };

            // Create and bind VAO
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            // Create and bind VBO
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* ptr = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(vertices.Length * sizeof(float)),
                    ptr, BufferUsageARB.StaticDraw);
            }

            // Create and bind EBO
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* ptr = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(indices.Length * sizeof(uint)),
                    ptr, BufferUsageARB.StaticDraw);
            }

            // Set up vertex attributes
            // Position attribute (location 0)
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
                4 * sizeof(float), (void*)0);

            // UV attribute (location 1)
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                4 * sizeof(float), (void*)(2 * sizeof(float)));

            // Unbind
            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        /// <summary>
        /// Update the object's position with random movement
        /// </summary>
        public void Update(float deltaTime)
        {
            _moveTimer += deltaTime;

            // Change target position every 2-4 seconds
            if (_moveTimer >= 2.0f + (float)_random.NextDouble() * 2.0f)
            {
                _moveTimer = 0.0f;
                _targetPosition = new Vector2D<float>(
                    (float)(_random.NextDouble() * 4.0 - 2.0),  // -2 to 2
                    (float)(_random.NextDouble() * 4.0 - 2.0)   // -2 to 2
                );
            }

            // Smooth movement towards target
            float moveSpeed = 0.5f; // Adjust this to make objects move faster/slower
            Vector2D<float> direction = _targetPosition - Position;
            float distance = MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (distance > 0.01f) // Small threshold to prevent jittering
            {
                direction = new Vector2D<float>(direction.X / distance, direction.Y / distance);
                _velocity = direction * moveSpeed * deltaTime;
                Position += _velocity;
            }
        }

        /// <summary>
        /// Renders this shader object at its world position
        /// </summary>
        public unsafe void Render(float time)
        {
            _gl.UseProgram(_shaderProgram);
            _gl.BindVertexArray(_vao);

            // Pass the object's world position to the shader
            _gl.Uniform2(_positionUniformLocation, Position.X, Position.Y);

            // Pass time for animated shaders
            _gl.Uniform1(_timeUniformLocation, time);

            // Draw the quad
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }

        /// <summary>
        /// Clean up OpenGL resources
        /// </summary>
        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}
