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
        public Point Position { get { return new Point(_vertices[18 * 4], _vertices[18 * 4 + 1]); } }


        public unsafe DisplayObject(GL gl, uint shaderProgram, Point position)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _random = new Random();

            _targetPosition = position;
            _velocity = Point.Zero;

            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");

            //vertex and texture coordinates
            _vertices = new float[] {
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
                position.X, position.Y,     0.5f, 0.5f,
            };

            _indices = new uint[] {
                // Original outer ring triangles to center
                0,1,18, 1,2,18, 2,3,18, 3,4,18,
                4,5,18, 5,6,18, 6,7,18, 7,0,18,
                // Inner ring triangles to center
                8,9,18, 9,10,18, 10,11,18, 11,8,18,
                // Additional vertices triangles to center
                12,13,18, 13,14,18, 14,15,18, 15,12,18,
                16,17,18
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
            Point newPosition = new(Utils.Lerp(Position.X, _targetPosition.X, (float)deltaTime), Utils.Lerp(Position.Y, _targetPosition.Y, (float)deltaTime));
            SetObjectPosition(newPosition.X, newPosition.Y);

            // if the distance between the position and the target position is less than 0.01, set the target position to the position
            if (Vector2D.Distance(Position, _targetPosition) < 0.01)
            {
                _targetPosition = Position;
            }

            // if the distance between the position and the target position is less than 0.01, set the target position to the position
            if (Vector2D.Distance(Position, _targetPosition) < 0.01)
            {
                var random = new Random();
                SetObjectPosition((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1);
            }
        }

        public void SetVertexPosition(int vertexIndex, float x, float y)
        {
            if (vertexIndex < 0 || vertexIndex >= 18) return;

            int arrayIndex = vertexIndex * 4;
            _vertices[arrayIndex] = x;
            _vertices[arrayIndex + 1] = y;
        }

        public void SetVertexPositions(List<Point> positions)
        {
            int nbToAssign = Math.Min(positions.Count, 18);

            for (int i = 0; i < nbToAssign; i++)
            {
                SetVertexPosition(i, positions[i].X, positions[i].Y);
            }
            for (int i = nbToAssign; i < 18; i++)
            {
                SetVertexPosition(i, positions[0].X, positions[0].Y);
            }
        }

        public void SetObjectPosition(float x, float y)
        {
            //the end vertex is the center
            int arrayIndex = 18 * 4;
            _vertices[arrayIndex] = x;
            _vertices[arrayIndex + 1] = y;
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
            _gl.DrawElements(PrimitiveType.Triangles, 33, DrawElementsType.UnsignedInt, (void*)0);
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}
