using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using DataViz.Data;
using DataViz.Algorithms;

namespace DataViz.Rendering
{
    /// <summary>
    /// Handles the rendering logic for caustics visualization
    /// Separated from algorithmic logic for better maintainability
    /// </summary>
    public class CausticRenderer
    {
        private GL? _gl;
        private uint _vao, _vbo, _shader;
        private CausticPointCollection _points;
        private float _time = 0.0f;
        private Vector2 _windowSize;
        private Vector2 _worldBounds;

        public CausticRenderer(CausticPointCollection points, Vector2 worldBounds)
        {
            _points = points;
            _worldBounds = worldBounds;
        }

        /// <summary>
        /// Initialize the renderer with OpenGL context
        /// </summary>
        public void Initialize(GL gl, IWindow window)
        {
            _gl = gl;
            _windowSize = new Vector2(window.Size.X, window.Size.Y);

            // Create full-screen quad
            CreateFullScreenQuad();

            // Compile shaders
            _shader = CompileShader("assets/shaders/vertex.glsl", "assets/shaders/fragment.glsl");
        }

        /// <summary>
        /// Render the caustics visualization
        /// </summary>
        public void Render(double deltaTime)
        {
            if (_gl == null) return;

            _time += (float)deltaTime;

            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
            _gl.UseProgram(_shader);

            // Set uniforms
            SetUniforms();

            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        /// <summary>
        /// Update window size for proper scaling
        /// </summary>
        public void UpdateWindowSize(Vector2 newSize)
        {
            _windowSize = newSize;
        }

        /// <summary>
        /// Update the points collection for animation
        /// </summary>
        public void UpdatePoints(CausticPointCollection newPoints)
        {
            _points = newPoints;
        }

        /// <summary>
        /// Create a full-screen quad for rendering
        /// </summary>
        private void CreateFullScreenQuad()
        {
            float[] vertices = {
                -1.0f, -1.0f,  // Bottom-left
                 1.0f, -1.0f,  // Bottom-right
                 1.0f,  1.0f,  // Top-right
                -1.0f,  1.0f   // Top-left
            };

            _vao = _gl!.GenVertexArray();
            _vbo = _gl.GenBuffer();
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            unsafe
            {
                fixed (float* ptr = vertices)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer,
                        (nuint)(vertices.Length * sizeof(float)),
                        ptr,
                        BufferUsageARB.StaticDraw);
                }
            }

            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            _gl.EnableVertexAttribArray(0);
        }

        /// <summary>
        /// Set shader uniforms
        /// </summary>
        private void SetUniforms()
        {
            // Time uniform
            int timeLoc = _gl!.GetUniformLocation(_shader, "iTime");
            if (timeLoc != -1)
            {
                _gl.Uniform1(timeLoc, _time);
            }

            // Window size uniform
            int sizeLoc = _gl.GetUniformLocation(_shader, "iResolution");
            if (sizeLoc != -1)
                _gl.Uniform2(sizeLoc, _windowSize.X, _windowSize.Y);

            // World bounds uniform
            int boundsLoc = _gl.GetUniformLocation(_shader, "iWorldBounds");
            if (boundsLoc != -1)
                _gl.Uniform2(boundsLoc, _worldBounds.X, _worldBounds.Y);

            // Point count uniform
            int pointCountLoc = _gl.GetUniformLocation(_shader, "iPointCount");
            if (pointCountLoc != -1)
                _gl.Uniform1(pointCountLoc, _points.Points.Count);

            // Set point data as uniforms (we'll need to modify this for many points)
            SetPointDataUniforms();
        }

        /// <summary>
        /// Set point data as shader uniforms
        /// Note: This is a simplified approach. For many points, consider using SSBOs or textures
        /// </summary>
        private void SetPointDataUniforms()
        {
            var points = _points.Points;
            int maxPoints = Math.Min(points.Count, 64); // Limit for uniform arrays

            // Set positions
            for (int i = 0; i < maxPoints; i++)
            {
                int posLoc = _gl!.GetUniformLocation(_shader, $"iPoints[{i}].position");
                if (posLoc != -1)
                    _gl.Uniform2(posLoc, points[i].Position.X, points[i].Position.Y);
            }

            // Set properties
            for (int i = 0; i < maxPoints; i++)
            {
                int wallLoc = _gl.GetUniformLocation(_shader, $"iPoints[{i}].wallWidth");
                int agitationLoc = _gl.GetUniformLocation(_shader, $"iPoints[{i}].agitation");
                int colorLoc = _gl.GetUniformLocation(_shader, $"iPoints[{i}].color");

                if (wallLoc != -1)
                    _gl.Uniform1(wallLoc, points[i].WallWidth);
                if (agitationLoc != -1)
                    _gl.Uniform1(agitationLoc, points[i].Agitation);
                if (colorLoc != -1)
                    _gl.Uniform3(colorLoc, points[i].Color.X, points[i].Color.Y, points[i].Color.Z);
            }
        }

        /// <summary>
        /// Compile shader program
        /// </summary>
        private uint CompileShader(string vertexPath, string fragmentPath)
        {
            Console.WriteLine($"Loading vertex shader from: {vertexPath}");
            string vsrc = File.ReadAllText(vertexPath);
            Console.WriteLine($"Loading fragment shader from: {fragmentPath}");
            string fsrc = File.ReadAllText(fragmentPath);

            Console.WriteLine("Compiling vertex shader...");
            uint vs = _gl!.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vs, vsrc);
            _gl.CompileShader(vs);
            CheckShaderCompile(vs, "Vertex");

            Console.WriteLine("Compiling fragment shader...");
            uint fs = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fs, fsrc);
            _gl.CompileShader(fs);
            CheckShaderCompile(fs, "Fragment");

            Console.WriteLine("Linking shader program...");
            uint prog = _gl.CreateProgram();
            _gl.AttachShader(prog, vs);
            _gl.AttachShader(prog, fs);
            _gl.LinkProgram(prog);

            // Check program linking
            _gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string info = _gl.GetProgramInfoLog(prog);
                throw new Exception($"Shader program link error: {info}");
            }

            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);
            Console.WriteLine("Shader compilation successful!");
            return prog;
        }

        /// <summary>
        /// Check shader compilation status
        /// </summary>
        private void CheckShaderCompile(uint shaderObj, string shaderType)
        {
            _gl!.GetShader(shaderObj, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string info = _gl.GetShaderInfoLog(shaderObj);
                Console.WriteLine($"{shaderType} shader compilation failed:");
                Console.WriteLine(info);
                throw new Exception($"{shaderType} shader compile error: " + info);
            }
            else
            {
                Console.WriteLine($"{shaderType} shader compiled successfully");
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (_gl != null)
            {
                _gl.DeleteVertexArray(_vao);
                _gl.DeleteBuffer(_vbo);
                _gl.DeleteProgram(_shader);
            }
        }
    }
}
