using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace maelstrom_poc
{
    /// <summary>
    /// Handles post-processing effects using framebuffers
    /// </summary>
    public class PostProcessor : IDisposable
    {
        private readonly GL _gl;
        private readonly uint _framebuffer;
        private readonly uint _renderTexture;
        private readonly uint _postProcessShader;
        private readonly uint _vao;
        private readonly uint _vbo;
        private readonly uint _ebo;
        private readonly int _screenTextureLocation;
        private readonly int _iTimeLocation;
        private readonly int _screenSizeLocation;
        private Vector2D<int> _screenSize;

        // Fullscreen quad vertices (position + texture coordinates)
        private readonly float[] _quadVertices = {
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f,  // top-left
            -1.0f, -1.0f,  0.0f, 0.0f,  // bottom-left
             1.0f, -1.0f,  1.0f, 0.0f,  // bottom-right
             1.0f,  1.0f,  1.0f, 1.0f   // top-right
        };

        private readonly uint[] _quadIndices = {
            0, 1, 2,
            0, 2, 3
        };

        public PostProcessor(GL gl, uint postProcessShader, Vector2D<int> screenSize)
        {
            _gl = gl;
            _postProcessShader = postProcessShader;
            _screenSize = screenSize;

            // Get uniform locations
            _screenTextureLocation = _gl.GetUniformLocation(_postProcessShader, "screenTexture");
            _iTimeLocation = _gl.GetUniformLocation(_postProcessShader, "iTime");
            _screenSizeLocation = _gl.GetUniformLocation(_postProcessShader, "screenSize");

            // Create framebuffer
            _framebuffer = _gl.GenFramebuffer();
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

            // Create render texture
            _renderTexture = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, _renderTexture);
            unsafe
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 
                    (uint)_screenSize.X, (uint)_screenSize.Y, 0, 
                    PixelFormat.Rgb, PixelType.UnsignedByte, (void*)null);
            }
            
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

            // Attach texture to framebuffer
            _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, 
                FramebufferAttachment.ColorAttachment0, 
                TextureTarget.Texture2D, _renderTexture, 0);

            // Check framebuffer completeness
            if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
            {
                throw new Exception("Framebuffer is not complete!");
            }

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Create fullscreen quad VAO
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            unsafe
            {
                fixed (float* ptr = _quadVertices)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer,
                        (nuint)(_quadVertices.Length * sizeof(float)),
                        ptr, BufferUsageARB.StaticDraw);
                }
            }

            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            unsafe
            {
                fixed (uint* ptr = _quadIndices)
                {
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                        (nuint)(_quadIndices.Length * sizeof(uint)),
                        ptr, BufferUsageARB.StaticDraw);
                }
            }

            // Vertex attributes
            _gl.EnableVertexAttribArray(0);
            unsafe
            {
                _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
                    4 * sizeof(float), (void*)0);
            }

            _gl.EnableVertexAttribArray(1);
            unsafe
            {
                _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                    4 * sizeof(float), (void*)(2 * sizeof(float)));
            }

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        /// <summary>
        /// Begin rendering to framebuffer
        /// </summary>
        public void BeginRender()
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            _gl.Viewport(0, 0, (uint)_screenSize.X, (uint)_screenSize.Y);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
        }

        /// <summary>
        /// End rendering to framebuffer and apply post-processing
        /// </summary>
        public void EndRender(float time)
        {
            // Switch back to default framebuffer
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)_screenSize.X, (uint)_screenSize.Y);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            // Disable depth testing for post-processing
            _gl.Disable(EnableCap.DepthTest);

            // Use post-processing shader
            _gl.UseProgram(_postProcessShader);

            // Bind the rendered texture
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _renderTexture);
            _gl.Uniform1(_screenTextureLocation, 0);

            // Set uniforms
            _gl.Uniform1(_iTimeLocation, time);
            _gl.Uniform2(_screenSizeLocation, (float)_screenSize.X, (float)_screenSize.Y);

            // Render fullscreen quad
            _gl.BindVertexArray(_vao);
            unsafe
            {
                _gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            }
            _gl.BindVertexArray(0);

            // Re-enable depth testing
            _gl.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Update screen size when window is resized
        /// </summary>
        public void UpdateScreenSize(Vector2D<int> newSize)
        {
            _screenSize = newSize;

            // Recreate render texture with new size
            _gl.BindTexture(TextureTarget.Texture2D, _renderTexture);
            unsafe
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb,
                    (uint)_screenSize.X, (uint)_screenSize.Y, 0,
                    PixelFormat.Rgb, PixelType.UnsignedByte, (void*)null);
            }
        }

        public void Dispose()
        {
            _gl.DeleteFramebuffer(_framebuffer);
            _gl.DeleteTexture(_renderTexture);
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}
