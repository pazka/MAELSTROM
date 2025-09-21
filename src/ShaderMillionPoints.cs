using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using System.Runtime.InteropServices;

namespace maelstrom_poc;

public class ShaderMillionPoints
{
    private static GL? _gl;
    private static IWindow? _window;
    private static uint _vao, _vbo, _shader;
    private static float _time = 0f;
    private static float _speedMultiplier = 1.0f;
    private static bool _paused = false;

    public static void ShaderMain(string[] args)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(1920, 1080),
            Title = "Shader Million Points - Pure GPU"
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        _window.Run();
    }

    private static unsafe void OnLoad()
    {
        _gl = _window!.CreateOpenGL();
        _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Set up input handling
        if (_window != null)
        {
            var input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += OnKeyDown;
            }
        }

        // Create fullscreen quad
        float[] vertices = {
            -1.0f, -1.0f, 0.0f, 0.0f,
             1.0f, -1.0f, 1.0f, 0.0f,
             1.0f,  1.0f, 1.0f, 1.0f,
            -1.0f,  1.0f, 0.0f, 1.0f
        };

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        ReadOnlySpan<float> data = vertices;
        _gl.BufferData(BufferTargetARB.ArrayBuffer,
            (nuint)(data.Length * sizeof(float)),
            ref MemoryMarshal.GetReference(data),
            BufferUsageARB.StaticDraw);

        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Compile shader
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
                _time = 0f; // Reset time
                break;
        }
    }

    private static void OnUpdate(double deltaTime)
    {
        if (_paused) return;
        _time += (float)deltaTime;
    }

    private static void OnRender(double deltaTime)
    {
        if (_gl == null) return;

        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.UseProgram(_shader);

        // Set uniforms
        int timeLoc = _gl.GetUniformLocation(_shader, "iTime");
        _gl.Uniform1(timeLoc, _time);

        int speedLoc = _gl.GetUniformLocation(_shader, "iSpeed");
        _gl.Uniform1(speedLoc, _speedMultiplier);

        int resolutionLoc = _gl.GetUniformLocation(_shader, "iResolution");
        _gl.Uniform2(resolutionLoc, (float)_window!.Size.X, (float)_window.Size.Y);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(GLEnum.TriangleFan, 0, 4);

        // Update window title
        _window.Title = $"Shader Million Points | Speed: {_speedMultiplier:F1}x | {(_paused ? "PAUSED" : "RUNNING")} | FPS: {1.0 / deltaTime:F1}";
    }

    private static void OnClosing()
    {
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
layout (location = 1) in vec2 aTexCoord;
out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPos, 0.0, 1.0);
    TexCoord = aTexCoord;
}";

        const string fragmentShaderSource = @"#version 330 core
out vec4 FragColor;
in vec2 TexCoord;
uniform float iTime;
uniform float iSpeed;
uniform vec2 iResolution;

// Pseudo-random number generator
uint hash(uint x) {
    x += (x << 10u);
    x ^= (x >> 6u);
    x += (x << 3u);
    x ^= (x >> 11u);
    x += (x << 15u);
    return x;
}

float random(uint seed) {
    return float(hash(seed)) / 4294967296.0;
}

// Generate a point at given index
vec2 generatePoint(uint index, float time) {
    uint seed1 = hash(index * 12345u);
    uint seed2 = hash(seed1 + 67890u);
    uint seed3 = hash(seed2 + 11111u);
    
    // Base position
    float x = random(seed1) * 2.0 - 1.0;
    float y = random(seed2) * 2.0 - 1.0;
    
    // Add time-based movement
    float speed = random(seed3) * 0.5 + 0.1;
    float phase = random(seed1 + seed2) * 6.28318;
    
    x += sin(time * speed + phase) * 0.1;
    y += cos(time * speed * 1.3 + phase) * 0.1;
    
    // Add secondary wave
    float speed2 = random(seed2 + seed3) * 0.3 + 0.05;
    float phase2 = random(seed3) * 6.28318;
    
    x += sin(time * speed2 * 2.1 + phase2) * 0.05;
    y += cos(time * speed2 * 1.7 + phase2) * 0.05;
    
    return vec2(x, y);
}

// Check if a pixel is near any of the million points
float checkPoints(vec2 pixel, float time) {
    float minDist = 1.0;
    float pointSize = 0.002; // Size of each point
    
    // Sample a subset of points for performance
    // We'll use a grid-based approach to sample points efficiently
    vec2 gridSize = vec2(1000.0, 1000.0); // 1000x1000 grid = 1M points
    vec2 gridPos = floor(pixel * gridSize);
    
    // Check points in a 3x3 grid around current pixel
    for(float dx = -1.0; dx <= 1.0; dx++) {
        for(float dy = -1.0; dy <= 1.0; dy++) {
            vec2 checkGrid = gridPos + vec2(dx, dy);
            
            // Clamp to valid grid range
            checkGrid = clamp(checkGrid, vec2(0.0), gridSize - vec2(1.0));
            
            // Convert grid position to point index
            uint pointIndex = uint(checkGrid.y * gridSize.x + checkGrid.x);
            
            // Generate point position
            vec2 pointPos = generatePoint(pointIndex, time);
            
            // Convert to screen coordinates
            vec2 screenPos = (pointPos + 1.0) * 0.5;
            
            // Calculate distance
            float dist = distance(pixel, screenPos);
            minDist = min(minDist, dist);
        }
    }
    
    return minDist;
}

void main()
{
    vec2 uv = TexCoord;
    float time = iTime * iSpeed;
    
    // Check for nearby points
    float dist = checkPoints(uv, time);
    
    // Create point with soft falloff
    float point = 1.0 - smoothstep(0.0, 0.003, dist);
    
    // Add some color variation based on position and time
    float colorVariation = sin(uv.x * 10.0 + time) * 0.1 + 
                          cos(uv.y * 8.0 + time * 1.2) * 0.1;
    
    // Create color
    vec3 color = vec3(1.0, 1.0 + colorVariation, 1.0);
    
    // Add some bloom effect
    float bloom = 0.0;
    for(float x = -2.0; x <= 2.0; x++) {
        for(float y = -2.0; y <= 2.0; y++) {
            vec2 offset = vec2(x, y) / iResolution;
            float bloomDist = checkPoints(uv + offset, time);
            bloom += (1.0 - smoothstep(0.0, 0.005, bloomDist)) * 0.1;
        }
    }
    
    color += vec3(bloom * 0.3);
    
    // Final color with alpha
    FragColor = vec4(color, point);
}";

        uint vertexShader = _gl!.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            string info = _gl.GetShaderInfoLog(vertexShader);
            throw new Exception("Vertex shader compilation failed: " + info);
        }

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

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

        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            string info = _gl.GetProgramInfoLog(program);
            throw new Exception("Program linking failed: " + info);
        }

        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        return program;
    }
}
