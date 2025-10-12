using Silk.NET.OpenGL;

namespace Maelstrom
{
    /// <summary>
    /// Manages shader compilation and loading
    /// </summary>
    public class ShaderManager
    {
        private readonly GL _gl;
        private readonly Dictionary<string, uint> _shaderCache;

        public ShaderManager(GL gl)
        {
            _gl = gl;
            _shaderCache = new Dictionary<string, uint>();
        }

        /// <summary>
        /// Load and compile a shader from files
        /// </summary>
        public uint LoadShader(string key, string vertexPath, string fragmentPath)
        {
            if (_shaderCache.TryGetValue(key, out uint cachedShader))
            {
                return cachedShader;
            }

            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);

            uint shaderProgram = CompileShader(vertexSource, fragmentSource);
            _shaderCache[key] = shaderProgram;

            return shaderProgram;
        }

        public uint getShaderProgram(string key)
        {
            return _shaderCache[key];
        }

        /// <summary>
        /// Compile shader from source code
        /// </summary>
        private uint CompileShader(string vertexSource, string fragmentSource)
        {
            // Compile vertex shader
            uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShader, vertexSource);
            _gl.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, "Vertex");

            // Compile fragment shader
            uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShader, fragmentSource);
            _gl.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader, "Fragment");

            // Create shader program
            uint shaderProgram = _gl.CreateProgram();
            _gl.AttachShader(shaderProgram, vertexShader);
            _gl.AttachShader(shaderProgram, fragmentShader);
            _gl.LinkProgram(shaderProgram);

            // Check for linking errors
            _gl.GetProgram(shaderProgram, ProgramPropertyARB.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = _gl.GetProgramInfoLog(shaderProgram);
                throw new Exception($"Shader program linking failed: {infoLog}");
            }

            // Clean up individual shaders
            _gl.DetachShader(shaderProgram, vertexShader);
            _gl.DetachShader(shaderProgram, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private void CheckShaderCompile(uint shader, string type)
        {
            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = _gl.GetShaderInfoLog(shader);
                throw new Exception($"{type} shader compilation failed: {infoLog}");
            }
        }

        /// <summary>
        /// Clean up all cached shaders
        /// </summary>
        public void Dispose()
        {
            foreach (var shader in _shaderCache.Values)
            {
                _gl.DeleteProgram(shader);
            }
            _shaderCache.Clear();
        }
    }
}
