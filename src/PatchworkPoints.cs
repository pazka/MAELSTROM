using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using System.Runtime.InteropServices;

namespace maelstrom_poc;

public class PatchworkPoints
{
    private static GL? _gl;
    private static IWindow? _window;
    private static uint _vao, _vbo, _shader, _computeShader;
    private static uint[] _textures = new uint[16]; // 4x4 grid of overlapping textures
    private static uint[] _framebuffers = new uint[16];
    private static float _time = 0f;
    private static int _pointCount = 50_000_000; // 50 million points!
    private static float _speedMultiplier = 1.0f;
    private static bool _paused = false;
    private static int _textureSize = 2048; // 2K texture per patch
    private static int _patchCount = 4; // 4x4 grid
    private static float _overlap = 0.1f; // 10% overlap between patches

    public static void PatchworkMain(string[] args)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(1920, 1080),
            Title = "Patchwork Million Points - GPU Accelerated"
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

        // Create textures and framebuffers for each patch
        for (int i = 0; i < 16; i++)
        {
            _textures[i] = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, _textures[i]);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)_textureSize, (uint)_textureSize, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (void*)0);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Linear);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Linear);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            _framebuffers[i] = _gl.GenFramebuffer();
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffers[i]);
            _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _textures[i], 0);
        }

        // Compile shaders
        _shader = CompileShader();
        _computeShader = CompileComputeShader();
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
            case Silk.NET.Input.Key.Number1:
                _pointCount = 10_000_000;
                break;
            case Silk.NET.Input.Key.Number2:
                _pointCount = 25_000_000;
                break;
            case Silk.NET.Input.Key.Number3:
                _pointCount = 50_000_000;
                break;
            case Silk.NET.Input.Key.O:
                _overlap = Math.Min(_overlap + 0.05f, 0.3f);
                break;
            case Silk.NET.Input.Key.L:
                _overlap = Math.Max(_overlap - 0.05f, 0.0f);
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

        // Render points to each texture patch
        RenderPointsToPatches();

        // Render all patches to screen with blending
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.UseProgram(_shader);

        // Set uniforms
        int timeLoc = _gl.GetUniformLocation(_shader, "iTime");
        _gl.Uniform1(timeLoc, _time);

        int pointCountLoc = _gl.GetUniformLocation(_shader, "iPointCount");
        _gl.Uniform1(pointCountLoc, _pointCount);

        int textureSizeLoc = _gl.GetUniformLocation(_shader, "iTextureSize");
        _gl.Uniform1(textureSizeLoc, _textureSize);

        int overlapLoc = _gl.GetUniformLocation(_shader, "iOverlap");
        _gl.Uniform1(overlapLoc, _overlap);

        _gl.BindVertexArray(_vao);

        // Render each patch with different blending modes for depth
        for (int i = 0; i < 16; i++)
        {
            int patchX = i % 4;
            int patchY = i / 4;

            // Calculate patch position and scale
            float patchScale = 1.0f + _overlap;
            float patchXPos = (patchX - 1.5f) * 0.5f * patchScale;
            float patchYPos = (patchY - 1.5f) * 0.5f * patchScale;

            // Set patch-specific uniforms
            int patchPosLoc = _gl.GetUniformLocation(_shader, "iPatchPos");
            _gl.Uniform2(patchPosLoc, patchXPos, patchYPos);

            int patchScaleLoc = _gl.GetUniformLocation(_shader, "iPatchScale");
            _gl.Uniform1(patchScaleLoc, patchScale);

            int patchIndexLoc = _gl.GetUniformLocation(_shader, "iPatchIndex");
            _gl.Uniform1(patchIndexLoc, i);

            // Bind texture
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _textures[i]);
            int textureLoc = _gl.GetUniformLocation(_shader, "uTexture");
            _gl.Uniform1(textureLoc, 0);

            // Render patch
            _gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
        }

        // Update window title
        _window!.Title = $"Patchwork Points | Points: {_pointCount:N0} | Patches: 16 | Overlap: {_overlap:F2} | Speed: {_speedMultiplier:F1}x | {(_paused ? "PAUSED" : "RUNNING")} | FPS: {1.0 / deltaTime:F1}";
    }

    private static void RenderPointsToPatches()
    {
        // Render to each patch
        for (int patch = 0; patch < 16; patch++)
        {
            _gl!.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffers[patch]);
            _gl.Viewport(0, 0, (uint)_textureSize, (uint)_textureSize);

            // Clear with transparent background
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            // Use compute shader to render points for this patch
            _gl.UseProgram(_computeShader);

            // Set uniforms
            int timeLoc = _gl.GetUniformLocation(_computeShader, "iTime");
            _gl.Uniform1(timeLoc, _time);

            int pointCountLoc = _gl.GetUniformLocation(_computeShader, "iPointCount");
            _gl.Uniform1(pointCountLoc, _pointCount / 16); // Distribute points across patches

            int speedLoc = _gl.GetUniformLocation(_computeShader, "iSpeed");
            _gl.Uniform1(speedLoc, _speedMultiplier);

            int patchLoc = _gl.GetUniformLocation(_computeShader, "iPatchIndex");
            _gl.Uniform1(patchLoc, patch);

            // Bind texture as image for compute shader
            _gl.BindImageTexture(0, _textures[patch], 0, false, 0, BufferAccessARB.WriteOnly, InternalFormat.Rgba8);

            // Dispatch compute shader
            int workGroups = (int)Math.Ceiling((_pointCount / 16) / 1024.0);
            _gl.DispatchCompute((uint)workGroups, 1, 1);
            _gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
        }

        // Unbind framebuffer
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        if (_window != null)
        {
            _gl.Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);
        }
    }

    private static void OnClosing()
    {
        if (_gl != null)
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteProgram(_shader);
            _gl.DeleteProgram(_computeShader);

            for (int i = 0; i < 16; i++)
            {
                _gl.DeleteTexture(_textures[i]);
                _gl.DeleteFramebuffer(_framebuffers[i]);
            }
        }
    }

    private static uint CompileShader()
    {
        const string vertexShaderSource = @"#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;
out vec2 TexCoord;
uniform float iTime;
uniform int iPointCount;
uniform int iTextureSize;
uniform float iOverlap;
uniform vec2 iPatchPos;
uniform float iPatchScale;
uniform int iPatchIndex;

void main()
{
    // Transform position based on patch
    vec2 pos = aPos * iPatchScale + iPatchPos;
    gl_Position = vec4(pos, 0.0, 1.0);
    TexCoord = aTexCoord;
}";

        const string fragmentShaderSource = @"#version 330 core
out vec4 FragColor;
in vec2 TexCoord;
uniform sampler2D uTexture;
uniform float iTime;
uniform int iPointCount;
uniform int iTextureSize;
uniform float iOverlap;
uniform vec2 iPatchPos;
uniform float iPatchScale;
uniform int iPatchIndex;

void main()
{
    // Sample the texture with the points
    vec4 color = texture(uTexture, TexCoord);
    
    // Add patch-specific color variation
    float patchVariation = sin(iTime * 0.5 + float(iPatchIndex) * 0.5) * 0.2 + 0.8;
    color.rgb *= patchVariation;
    
    // Add distance-based fade for overlap effect
    vec2 center = TexCoord - 0.5;
    float dist = length(center);
    float fade = 1.0 - smoothstep(0.3, 0.5, dist);
    color.a *= fade;
    
    // Add some bloom effect
    vec2 texelSize = 1.0 / float(iTextureSize);
    vec4 bloom = vec4(0.0);
    for(int x = -1; x <= 1; x++) {
        for(int y = -1; y <= 1; y++) {
            bloom += texture(uTexture, TexCoord + vec2(x, y) * texelSize) * 0.1;
        }
    }
    color += bloom * 0.2;
    
    FragColor = color;
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

    private static uint CompileComputeShader()
    {
        const string computeShaderSource = @"#version 430 core
layout(local_size_x = 1024) in;
layout(rgba8, binding = 0) uniform image2D uTexture;

uniform float iTime;
uniform int iPointCount;
uniform float iSpeed;
uniform int iPatchIndex;

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

void main() {
    uint index = gl_GlobalInvocationID.x;
    if (index >= uint(iPointCount)) return;
    
    // Add patch offset to create unique points per patch
    uint globalIndex = index + uint(iPatchIndex) * uint(iPointCount);
    
    // Generate pseudo-random position based on index and time
    uint seed1 = hash(globalIndex * 12345u);
    uint seed2 = hash(seed1 + 67890u);
    
    float time = iTime * iSpeed * 0.1;
    
    // Create unique path for each point with patch-specific variation
    float patchVariation = float(iPatchIndex) * 0.1;
    float x = (random(seed1) * 2.0 - 1.0) + sin(time + random(seed2) * 6.28 + patchVariation) * 0.15;
    float y = (random(seed2) * 2.0 - 1.0) + cos(time + random(seed1) * 6.28 + patchVariation) * 0.15;
    
    // Convert to texture coordinates
    ivec2 texCoord = ivec2((x + 1.0) * 0.5 * float(imageSize(uTexture).x), 
                          (y + 1.0) * 0.5 * float(imageSize(uTexture).y));
    
    // Clamp to texture bounds
    texCoord = clamp(texCoord, ivec2(0), imageSize(uTexture) - ivec2(1));
    
    // Create patch-specific color
    vec3 baseColor = vec3(1.0, 1.0, 1.0);
    if (iPatchIndex % 4 == 0) baseColor = vec3(1.0, 0.8, 0.8); // Reddish
    else if (iPatchIndex % 4 == 1) baseColor = vec3(0.8, 1.0, 0.8); // Greenish
    else if (iPatchIndex % 4 == 2) baseColor = vec3(0.8, 0.8, 1.0); // Bluish
    else baseColor = vec3(1.0, 1.0, 0.8); // Yellowish
    
    // Write colored point to texture
    vec4 color = vec4(baseColor, 0.6);
    imageStore(uTexture, texCoord, color);
    
    // Add some bloom around the point
    for(int dx = -1; dx <= 1; dx++) {
        for(int dy = -1; dy <= 1; dy++) {
            ivec2 bloomCoord = texCoord + ivec2(dx, dy);
            if(bloomCoord.x >= 0 && bloomCoord.x < imageSize(uTexture).x &&
               bloomCoord.y >= 0 && bloomCoord.y < imageSize(uTexture).y) {
                vec4 bloomColor = vec4(baseColor * 0.3, 0.1);
                imageStore(uTexture, bloomCoord, bloomColor);
            }
        }
    }
}";

        uint computeShader = _gl!.CreateShader(ShaderType.ComputeShader);
        _gl.ShaderSource(computeShader, computeShaderSource);
        _gl.CompileShader(computeShader);

        _gl.GetShader(computeShader, ShaderParameterName.CompileStatus, out int cStatus);
        if (cStatus != (int)GLEnum.True)
        {
            string info = _gl.GetShaderInfoLog(computeShader);
            throw new Exception("Compute shader compilation failed: " + info);
        }

        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, computeShader);
        _gl.LinkProgram(program);

        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            string info = _gl.GetProgramInfoLog(program);
            throw new Exception("Compute program linking failed: " + info);
        }

        _gl.DeleteShader(computeShader);
        return program;
    }
}
