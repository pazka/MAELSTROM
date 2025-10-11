using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Numerics;

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
        private readonly int _modelMatrixUniformLocation;

        private readonly Random _random;

        // Transformation matrices
        public Matrix4x4 TranslationMatrix { get; private set; } = Matrix4x4.Identity;
        public Matrix4x4 RotationMatrix { get; private set; } = Matrix4x4.Identity;
        public Matrix4x4 ScaleMatrix { get; private set; } = Matrix4x4.Identity;
        public Matrix4x4 ModelMatrix { get; private set; } = Matrix4x4.Identity;

        public Point Position { get; private set; }
        public float Rotation { get; private set; }
        public float Scale { get; private set; } = 1.0f;


        public unsafe DisplayObject(GL gl, uint shaderProgram, Point position)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _random = new Random();
            Position = position;

            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");
            _modelMatrixUniformLocation = _gl.GetUniformLocation(_shaderProgram, "uModel");

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

            // Initialize transformation matrices
            SetPosition(position);
            UpdateModelMatrix();
        }

        public void Update(float deltaTime)
        {
            UpdateModelMatrix();
        }

        public void SetPosition(Point position)
        {
            Position = position;
            TranslationMatrix = Matrix4x4.CreateTranslation(position.X, position.Y, 0.0f);
        }

        public void SetRotation(float rotation)
        {
            Rotation = rotation;
            RotationMatrix = Matrix4x4.CreateRotationZ(rotation);
        }

        public void SetScale(float scale)
        {
            Scale = scale;
            ScaleMatrix = Matrix4x4.CreateScale(scale, scale, 1.0f);
        }

        private void UpdateModelMatrix()
        {
            // Combine transformations: Scale * Rotation * Translation
            ModelMatrix = ScaleMatrix * RotationMatrix * TranslationMatrix;
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

            // Pass model matrix to shader
            if (_modelMatrixUniformLocation != -1)
            {
                unsafe
                {
                    float[] matrixArray = new float[16];
                    matrixArray[0] = ModelMatrix.M11; matrixArray[1] = ModelMatrix.M12; matrixArray[2] = ModelMatrix.M13; matrixArray[3] = ModelMatrix.M14;
                    matrixArray[4] = ModelMatrix.M21; matrixArray[5] = ModelMatrix.M22; matrixArray[6] = ModelMatrix.M23; matrixArray[7] = ModelMatrix.M24;
                    matrixArray[8] = ModelMatrix.M31; matrixArray[9] = ModelMatrix.M32; matrixArray[10] = ModelMatrix.M33; matrixArray[11] = ModelMatrix.M34;
                    matrixArray[12] = ModelMatrix.M41; matrixArray[13] = ModelMatrix.M42; matrixArray[14] = ModelMatrix.M43; matrixArray[15] = ModelMatrix.M44;

                    fixed (float* matrixPtr = matrixArray)
                    {
                        _gl.UniformMatrix4(_modelMatrixUniformLocation, 1, false, matrixPtr);
                    }
                }
            }

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
