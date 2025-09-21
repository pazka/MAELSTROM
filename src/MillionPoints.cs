using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using System.Runtime.InteropServices;

namespace maelstrom_poc;

public class MillionPoints
{
    private static GL? _gl;
    private static IWindow? _window;
    private static uint _vao, _vbo, _shader;
    private static float _time = 0f;
    private static int _pointCount = 1_000_000;
    private static float[] _points = new float[1_000_000 * 2]; // x, y for each point
    private static float[] _velocities = new float[1_000_000 * 2]; // velocity for each point
    private static float[] _seeds = new float[1_000_000 * 4]; // unique seeds for each point
    private static int _updateBatchSize = 1000; // Smaller batches for smoother updates
    private static int _currentBatch = 0;
    private static int _framesSinceLastUpdate = 0;
    private static float _speedMultiplier = 1.0f;
    private static bool _paused = false;
    private static bool _performanceMode = false;

    public static void Main(string[] args)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(1920, 1080),
            Title = "Million Points - Random Paths"
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        _window.Run();
    }

    private static void OnLoad()
    {
        _gl = _window!.CreateOpenGL();
        _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Dark background
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Set up input handling after window is loaded
        if (_window != null)
        {
            var input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += OnKeyDown;
            }
        }

        // Initialize points with random starting positions and velocities
        var random = new Random(42); // Fixed seed for reproducible results
        for (int i = 0; i < _pointCount; i++)
        {
            _points[i * 2] = (float)(random.NextDouble() * 2.0 - 1.0); // x: -1 to 1
            _points[i * 2 + 1] = (float)(random.NextDouble() * 2.0 - 1.0); // y: -1 to 1

            // Initialize velocities
            _velocities[i * 2] = (float)(random.NextDouble() * 0.002 - 0.001); // small random velocity
            _velocities[i * 2 + 1] = (float)(random.NextDouble() * 0.002 - 0.001);

            // Initialize unique seeds for each point
            _seeds[i * 4] = (float)random.NextDouble() * 1000f; // seed1
            _seeds[i * 4 + 1] = (float)random.NextDouble() * 1000f; // seed2
            _seeds[i * 4 + 2] = (float)random.NextDouble() * 1000f; // seed3
            _seeds[i * 4 + 3] = (float)random.NextDouble() * 1000f; // seed4
        }

        // Create VAO and VBO
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Upload point data
        ReadOnlySpan<float> data = _points;
        _gl.BufferData(BufferTargetARB.ArrayBuffer,
            (nuint)(data.Length * sizeof(float)),
            ref MemoryMarshal.GetReference(data),
            BufferUsageARB.DynamicDraw);

        // Set up vertex attributes
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);

        // Compile shaders
        _shader = CompileShader();
    }

    private static void OnKeyDown(IKeyboard keyboard, Silk.NET.Input.Key key, int keyCode)
    {
        switch (key)
        {
            case Silk.NET.Input.Key.Escape:
                _window?.Close();
                break;
            case Silk.NET.Input.Key.Space:
                _paused = !_paused;
                break;
            case Silk.NET.Input.Key.Up:
                _speedMultiplier = Math.Min(_speedMultiplier * 1.2f, 5.0f);
                break;
            case Silk.NET.Input.Key.Down:
                _speedMultiplier = Math.Max(_speedMultiplier / 1.2f, 0.1f);
                break;
            case Silk.NET.Input.Key.R:
                // Reset points
                var random = new Random(42);
                for (int i = 0; i < _pointCount; i++)
                {
                    _points[i * 2] = (float)(random.NextDouble() * 2.0 - 1.0);
                    _points[i * 2 + 1] = (float)(random.NextDouble() * 2.0 - 1.0);
                    _velocities[i * 2] = (float)(random.NextDouble() * 0.002 - 0.001);
                    _velocities[i * 2 + 1] = (float)(random.NextDouble() * 0.002 - 0.001);
                }
                break;
            case Silk.NET.Input.Key.Number1:
                _pointCount = 100_000;
                break;
            case Silk.NET.Input.Key.Number2:
                _pointCount = 500_000;
                break;
            case Silk.NET.Input.Key.Number3:
                _pointCount = 1_000_000;
                break;
            case Silk.NET.Input.Key.P:
                _performanceMode = !_performanceMode;
                break;
        }
    }

    private static void OnUpdate(double deltaTime)
    {
        if (_paused) return;

        _time += (float)deltaTime;
        float dt = (float)deltaTime * _speedMultiplier * 0.2f; // Slightly faster movement
        _framesSinceLastUpdate++;

        if (_performanceMode)
        {
            // Performance mode: Update points in smaller batches for better FPS
            int startIndex = _currentBatch * _updateBatchSize;
            int endIndex = Math.Min(startIndex + _updateBatchSize, _pointCount);

            for (int i = startIndex; i < endIndex; i++)
            {
                UpdatePoint(i, dt);
            }

            _currentBatch++;
            if (_currentBatch * _updateBatchSize >= _pointCount)
            {
                _currentBatch = 0;
            }
        }
        else
        {
            // Smooth mode: Update ALL points every frame for smooth movement
            for (int i = 0; i < _pointCount; i++)
            {
                UpdatePoint(i, dt);
            }
        }

        // Update VBO every frame for smooth display
        ReadOnlySpan<float> data = _points;
        _gl!.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
            (nuint)(data.Length * sizeof(float)),
            ref MemoryMarshal.GetReference(data));
    }

    private static void UpdatePoint(int i, float dt)
    {
        // Use pre-computed seeds for each point
        float seed1 = _seeds[i * 4];
        float seed2 = _seeds[i * 4 + 1];
        float seed3 = _seeds[i * 4 + 2];
        float seed4 = _seeds[i * 4 + 3];

        // Optimized force calculation with fewer operations
        float time1 = _time * 0.1f;
        float time2 = _time * 0.2f;

        float forceX = (float)(Math.Sin(seed1 + time1) * 0.0002 +
                              Math.Sin(seed2 + time2) * 0.0001);
        float forceY = (float)(Math.Cos(seed2 + time1 * 1.5f) * 0.0002 +
                              Math.Cos(seed3 + time2 * 1.5f) * 0.0001);

        // Update velocities with forces
        _velocities[i * 2] += forceX * dt;
        _velocities[i * 2 + 1] += forceY * dt;

        // Apply damping
        _velocities[i * 2] *= 0.995f;
        _velocities[i * 2 + 1] *= 0.995f;

        // Update positions
        _points[i * 2] += _velocities[i * 2] * dt * 200f;
        _points[i * 2 + 1] += _velocities[i * 2 + 1] * dt * 200f;

        // Wrap around screen edges
        if (_points[i * 2] > 1.0f) _points[i * 2] -= 2.0f;
        if (_points[i * 2] < -1.0f) _points[i * 2] += 2.0f;
        if (_points[i * 2 + 1] > 1.0f) _points[i * 2 + 1] -= 2.0f;
        if (_points[i * 2 + 1] < -1.0f) _points[i * 2 + 1] += 2.0f;
    }

    private static void OnRender(double deltaTime)
    {
        if (_gl == null) return;

        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.UseProgram(_shader);

        // Set time uniform
        int timeLoc = _gl.GetUniformLocation(_shader, "iTime");
        _gl.Uniform1(timeLoc, _time);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(GLEnum.Points, 0, (uint)_pointCount);

        // Update window title with current parameters
        _window!.Title = $"Million Points - Random Paths | Points: {_pointCount:N0} | Speed: {_speedMultiplier:F1}x | {(_paused ? "PAUSED" : "RUNNING")} | Mode: {(_performanceMode ? "PERF" : "SMOOTH")} | FPS: {1.0 / deltaTime:F1}";
    }

    private static void OnClosing()
    {
        // Cleanup
        if (_gl != null)
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteProgram(_shader);
        }
    }

    private static uint CompileShader()
    {
        const string vertexShaderSource = @"#version 330 core
layout (location = 0) in vec2 aPos;
uniform float iTime;

void main()
{
    gl_Position = vec4(aPos, 0.0, 1.0);
    gl_PointSize = 0.3; // Very small point size for performance
}";

        const string fragmentShaderSource = @"#version 330 core
out vec4 FragColor;
uniform float iTime;

void main()
{
    // Create a soft circular point with distance-based alpha
    vec2 center = gl_PointCoord - vec2(0.5);
    float dist = length(center);
    
    // Soft falloff for better visual quality
    float alpha = 1.0 - smoothstep(0.0, 0.5, dist);
    
    // Slight color variation based on time for subtle animation
    float colorVariation = sin(iTime * 2.0 + gl_FragCoord.x * 0.01) * 0.1;
    
    FragColor = vec4(1.0, 1.0 + colorVariation, 1.0, alpha * 0.9);
}";

        uint vertexShader = _gl!.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);

        // Check compilation
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            string info = _gl.GetShaderInfoLog(vertexShader);
            throw new Exception("Vertex shader compilation failed: " + info);
        }

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        // Check compilation
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
        {
            string info = _gl.GetShaderInfoLog(fragmentShader);
            throw new Exception("Fragment shader compilation failed: " + info);
        }

        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);

        // Check linking
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            string info = _gl.GetProgramInfoLog(program);
            throw new Exception("Program linking failed: " + info);
        }

        // Clean up shaders
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        return program;
    }
}
