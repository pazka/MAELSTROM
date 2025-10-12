using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Corals
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
        private readonly int _seedUniformLocation;
        private readonly int _resolutionUniformLocation;
        private readonly float seed;
        private Vector2D<int> _screenSize;


        public unsafe DisplayObject(GL gl, uint shaderProgram, Vector2D<int> screenSize)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _screenSize = screenSize;
            seed = (float)new Random().NextDouble();

            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");
            _seedUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iSeed");
            _resolutionUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iResolution");

            //vertex and texture coordinates
            var vertices = new float[] {
                -1f, -1f,                   0.0f, 0.0f,
                1f, -1f,                    1.0f, 0.0f,
                1.0f, 1.0f,                 1.0f, 1.0f,
                -1f, 1f,                     0.0f, 1.0f,
            };

            var indices = new uint[] {
                // Original outer ring triangles to center
                0,1,3, 1,2,3,
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

        public void UpdateScreenSize(Vector2D<int> newScreenSize)
        {
            _screenSize = newScreenSize;
        }


        // Render pipeline: Upload data → Bind shader → Bind VAO → Set uniforms → Draw
        public unsafe void Render(float time)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            // UseProgram: Activates shader pipeline on GPU
            _gl.UseProgram(_shaderProgram);
            // BindVertexArray: Restores vertex layout and buffer bindings
            _gl.BindVertexArray(_vao);

            // Uniforms: Pass data to shader (executes on GPU)
            _gl.Uniform1(_timeUniformLocation, time);
            _gl.Uniform1(_seedUniformLocation, seed);
            _gl.Uniform2(_resolutionUniformLocation, (float)_screenSize.X, (float)_screenSize.Y);

            _gl.DrawElements(GLEnum.Triangles, 4 * 3, GLEnum.UnsignedInt, (void*)0);
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}
