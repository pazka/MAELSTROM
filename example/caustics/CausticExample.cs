using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Runtime.InteropServices;

class Program
{
    static GL? gl; // Silk.NET OpenGL binding
    static uint vao, vbo, shader;
    static IWindow? window1, window2;
    static float time = 0f;

    static void CausticMain()
    {
        var opts = WindowOptions.Default with
        {
            Size = new Silk.NET.Maths.Vector2D<int>(800, 600),
            Title = "Caustics"
        };

        window1 = Window.Create(opts with { Position = new(0, 0) });
        window2 = Window.Create(opts with { Position = new(900, 0) });

        window1.Load += () => onLoad(window1);
        window1.Render += onRender;

        window2.Load += () => onLoad(window2);
        window2.Render += onRender;

        window1.Run();
        window2.Run();
    }

    static void onLoad(IWindow w)
    {
        gl = GL.GetApi(w);

        float[] vertices = {
            -0.5f, -0.5f,
             0.5f, -0.5f,
             0.5f,  0.5f,
            -0.5f,  0.5f
        };

        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        // Convert float[] → ReadOnlySpan<float> → pointer
        ReadOnlySpan<float> data = vertices;
        gl.BufferData(BufferTargetARB.ArrayBuffer,
            (nuint)(data.Length * sizeof(float)),
            ref MemoryMarshal.GetReference(data),
            BufferUsageARB.StaticDraw);

        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);

        shader = CompileShader("assets/shaders/vertex.glsl", "assets/shaders/fragment.glsl");
    }

    static void onRender(double delta)
    {
        if (gl is null) return;

        time += (float)delta;
        gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        gl.UseProgram(shader);

        int timeLoc = gl.GetUniformLocation(shader, "iTime");
        gl.Uniform1(timeLoc, time);
        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
    }

    static uint CompileShader(string vertexPath, string fragmentPath)
    {
        string vsrc = File.ReadAllText(vertexPath);
        string fsrc = File.ReadAllText(fragmentPath);

        uint vs = gl!.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vs, vsrc);
        gl.CompileShader(vs);
        CheckShaderCompile(vs);

        uint fs = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fs, fsrc);
        gl.CompileShader(fs);
        CheckShaderCompile(fs);

        uint prog = gl.CreateProgram();
        gl.AttachShader(prog, vs);
        gl.AttachShader(prog, fs);
        gl.LinkProgram(prog);

        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
        return prog;
    }

    static void CheckShaderCompile(uint shaderObj)
    {
        gl!.GetShader(shaderObj, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string info = gl.GetShaderInfoLog(shaderObj);
            throw new Exception("Shader compile error: " + info);
        }
    }
}
