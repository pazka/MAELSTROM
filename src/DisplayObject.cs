using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace maelstrom_poc
{
    using Point = Vector2D<float>;
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
        private readonly int _timeUniformLocation;

        private float[] _vertices;
        private readonly uint[] _indices;
        private Point _velocity;
        private Point _targetPosition;
        private readonly Random _random;
        private readonly Perlin _noise;
        private int _activeVertexCount = 19; // Track how many vertices are actually being used
        private float _speed;
        private Point _direction;
        public Point Position { get { return new Point(_vertices[0], _vertices[1]); } }


        public unsafe DisplayObject(GL gl, uint shaderProgram, Point position)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _random = new Random();
            _noise = new Perlin();
            
            _targetPosition = position;
            _velocity = Point.Zero;
            
            // Initialize diagonal movement with random speed
            _speed = (float)(_random.NextDouble() * 0.05 + 0.1) ; // Speed between 0.1 and 0.6
            InitializeRandomDirection();

            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");

            //vertex and texture coordinates
            _vertices = new float[] {
                position.X, position.Y,     0.5f, 0.5f,  // Center vertex (index 0)
                1f, -1f,                    1.0f, 0.0f,
                1.0f, 0.0f,                 1.0f, 0.5f,
                1f, 1f,                     1.0f, 1.0f,
                0.0f, 1.0f,                 0.5f, 1.0f,
                -1f, 1f,                    0.0f, 1.0f,
                -1.0f, 0.0f,                0.0f, 0.5f,
                -1f, -1f,                   0.0f, 0.0f,
                0.0f, -1.0f,                0.5f, 0.0f,
                // Additional 10 vertices
                0.5f, -0.5f,                0.75f, 0.25f,
                0.5f, 0.5f,                 0.75f, 0.75f,
                -0.5f, 0.5f,                0.25f, 0.75f,
                -0.5f, -0.5f,               0.25f, 0.25f,
                0.8f, -0.8f,                0.9f, 0.1f,
                0.8f, 0.8f,                 0.9f, 0.9f,
                -0.8f, 0.8f,                0.1f, 0.9f,
                -0.8f, -0.8f,               0.1f, 0.1f,
                0.3f, 0.0f,                 0.65f, 0.5f,
                0.0f, 0.3f,                 0.5f, 0.65f,
            };

            _indices = new uint[] {
                // Original outer ring triangles to center
                1,2,0, 2,3,0, 3,4,0, 4,5,0,
                5,6,0, 6,7,0, 7,8,0, 8,1,0,
                // Inner ring triangles to center
                9,10,0, 10,11,0, 11,12,0, 12,9,0,
                // Additional vertices triangles to center
                13,14,0, 14,15,0, 15,16,0, 16,13,0,
                17,18,0
            };

            // VAO: Binds vertex layout to GPU state machine
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            // VBO: Uploads vertex data to GPU memory
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* ptr = _vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(_vertices.Length * sizeof(float)),
                    ptr, BufferUsageARB.DynamicDraw);
            }

            // EBO: Uploads triangle indices to GPU memory
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* ptr = _indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(_indices.Length * sizeof(uint)),
                    ptr, BufferUsageARB.StaticDraw);
            }

            // Vertex attributes: Tells GPU how to interpret vertex data
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
                4 * sizeof(float), (void*)0);

            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                4 * sizeof(float), (void*)(2 * sizeof(float)));

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        public void Update(float deltaTime)
        {
            // Move diagonally with constant speed
            Point newPosition = Position + _direction * _speed * deltaTime;
            
            // Check screen boundaries and handle wrapping
            newPosition = HandleScreenBoundaries(newPosition);
            
            SetObjectPosition(newPosition.X, newPosition.Y);
        }

        public void SetVertexPosition(int vertexIndex, float x, float y)
        {
            if (vertexIndex < 0 || vertexIndex >= 19) return;

            int arrayIndex = vertexIndex * 4;
            _vertices[arrayIndex] = x;
            _vertices[arrayIndex + 1] = y;
        }

        public void SetVertexPositions(List<Point> positions)
        {
            int nbToAssign = Math.Min(positions.Count, 18);
            _activeVertexCount = nbToAssign + 1; // +1 for the center vertex (index 0)

            for (int i = 0; i < nbToAssign; i++)
            {
                SetVertexPosition(i + 1, positions[i].X, positions[i].Y); // Start from index 1 since 0 is center
            }
            for (int i = nbToAssign; i < 18; i++)
            {
                SetVertexPosition(i + 1, positions[0].X, positions[0].Y); // Start from index 1 since 0 is center
            }
        }

        public void SetObjectPosition(float x, float y)
        {
            //the first vertex is the center
            _vertices[0] = x;
            _vertices[1] = y;
        }

        public void moveTo(float x, float y)
        {
            _targetPosition = new Point(x, y);
        }

        public Point[] GetVertexPositions()
        {
            Point[] positions = new Point[_vertices.Length / 4];

            for (int i = 0; i < _vertices.Length / 4; i++)
            {
                int arrayIndex = i * 4;
                positions[i] = new Point(_vertices[arrayIndex], _vertices[arrayIndex + 1]);
            }
            return positions;
        }


        // BufferSubData: Updates GPU buffer with modified vertex data
        private unsafe void UpdateVertexBuffer()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* ptr = _vertices)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                    (nuint)(_vertices.Length * sizeof(float)), ptr);
            }
        }

        // Render pipeline: Upload data → Bind shader → Bind VAO → Set uniforms → Draw
        public unsafe void Render(float time)
        {
            UpdateVertexBuffer();

            // UseProgram: Activates shader pipeline on GPU
            _gl.UseProgram(_shaderProgram);
            // BindVertexArray: Restores vertex layout and buffer bindings
            _gl.BindVertexArray(_vao);

            // Uniforms: Pass data to shader (executes on GPU)
            _gl.Uniform1(_timeUniformLocation, time);

            // DrawElements: Triggers GPU to process vertices and render triangles
            // Calculate number of triangles based on active vertices (excluding center vertex)
            int activeVertices = _activeVertexCount - 1; // Exclude center vertex
            int triangleCount = activeVertices * 2; // Each vertex connects to center, forming triangles
            _gl.DrawElements(GLEnum.Triangles, (uint)(triangleCount * 3), GLEnum.UnsignedInt, (void*)0);
        }

        private void InitializeRandomDirection()
        {
            // Generate random diagonal direction
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            _direction = new Point((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private Point HandleScreenBoundaries(Point position)
        {
            float screenMin = -1.0f;
            float screenMax = 1.0f;
            
            // Check if object has moved beyond screen boundaries and bounce
            if (position.X < screenMin)
            {
                // Bounce off left boundary
                position = new Point(screenMin, position.Y);
                _direction = new Point(-_direction.X, _direction.Y); // Reverse X direction
                // Add small random variation to prevent corner trapping
                AddRandomVariation();
            }
            else if (position.X > screenMax)
            {
                // Bounce off right boundary
                position = new Point(screenMax, position.Y);
                _direction = new Point(-_direction.X, _direction.Y); // Reverse X direction
                // Add small random variation to prevent corner trapping
                AddRandomVariation();
            }
            
            if (position.Y < screenMin)
            {
                // Bounce off bottom boundary
                position = new Point(position.X, screenMin);
                _direction = new Point(_direction.X, -_direction.Y); // Reverse Y direction
                // Add small random variation to prevent corner trapping
                AddRandomVariation();
            }
            else if (position.Y > screenMax)
            {
                // Bounce off top boundary
                position = new Point(position.X, screenMax);
                _direction = new Point(_direction.X, -_direction.Y); // Reverse Y direction
                // Add small random variation to prevent corner trapping
                AddRandomVariation();
            }
            
            return position;
        }

        private void AddRandomVariation()
        {
            // Add small random variation to direction to prevent corner trapping
            float variation = 0.1f;
            float randomX = (float)(_random.NextDouble() * variation * 2 - variation);
            float randomY = (float)(_random.NextDouble() * variation * 2 - variation);
            
            _direction = new Point(_direction.X + randomX, _direction.Y + randomY);
            
            // Normalize to maintain speed
            float length = (float)Math.Sqrt(_direction.X * _direction.X + _direction.Y * _direction.Y);
            if (length > 0.001f) // Avoid division by zero
            {
                _direction = new Point(_direction.X / length, _direction.Y / length);
            }
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}
