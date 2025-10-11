using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Phishing
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
        private readonly Random _random;
        public Point Position { get { return new Point(_vertices[0], _vertices[1]); } }


        public unsafe DisplayObject(GL gl, uint shaderProgram, Point position)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _random = new Random();

            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");

            //vertex and texture coordinates
            float[] vertices =
        {
        //       aPosition     | aTexCoords
            1f,  1f,  1.0f, 1.0f,
            1f, -1f,  1.0f, 0.0f,
            -1f, -1f,  0.0f, 0.0f,
            -1f,  1f,  0.0f, 1.0f
        };


            uint[] indices = {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

            // VAO: Binds vertex layout to GPU state machine
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            // VBO: Uploads vertex data to GPU memory
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* ptr = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(vertices.Length * sizeof(float)),
                    ptr, BufferUsageARB.StaticDraw);
            }

            // EBO: Uploads triangle indices to GPU memory
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* ptr = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(indices.Length * sizeof(uint)),
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
        }

        public void SetVertexPosition(int vertexIndex, float x, float y)
        {
            if (vertexIndex < 0 || vertexIndex >= 19) return;

            int arrayIndex = vertexIndex * 4;
            _vertices[arrayIndex] = x;
            _vertices[arrayIndex + 1] = y;
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

        // Render pipeline: Upload data → Bind shader → Bind VAO → Set uniforms → Draw
        public unsafe void Render(float time)
        {

            // UseProgram: Activates shader pipeline on GPU
            _gl.UseProgram(_shaderProgram);
            // BindVertexArray: Restores vertex layout and buffer bindings
            _gl.BindVertexArray(_vao);
            // Uniforms: Pass data to shader (executes on GPU)
            _gl.Uniform1(_timeUniformLocation, time);
            // DrawElements: Triggers GPU to process vertices and render triangles
            _gl.DrawElements(GLEnum.Triangles, (uint)(4 * 3), GLEnum.UnsignedInt, (void*)0);
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}
