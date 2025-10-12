using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Numerics;

namespace Maelstrom.Phishing
{
    using Point = Vector2D<float>;

    /// <summary>
    /// Efficiently renders millions of points using a single VBO and glDrawArrays(GL_POINTS)
    /// </summary>
    public class PointsRenderer
    {
        private readonly GL _gl;
        private readonly uint _vao;
        private readonly uint _vbo;
        private readonly uint _shaderProgram;
        private readonly int _timeUniformLocation;
        
        private Point[] _points;
        private Vector2D<int> _screenSize;
        private int _pointCount;
        private float _time;

        public unsafe PointsRenderer(GL gl, uint shaderProgram, Vector2D<int> screenSize)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _screenSize = screenSize;
            _time = 0.0f;

            // Get uniform locations
            _timeUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iTime");

            // Create VAO and VBO
            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            // Set up vertex attribute for point positions (2 floats per point)
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), (void*)0);

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        /// <summary>
        /// Initialize the points array with one point per DataObject
        /// </summary>
        public void InitializePoints(List<DataObject> dataObjects)
        {
            _pointCount = dataObjects.Count;
            _points = new Point[_pointCount];

            // Copy positions from DataObjects (one point per DataObject)
            for (int i = 0; i < _pointCount; i++)
            {
                _points[i] = dataObjects[i].Position;
            }

            // Upload initial data to GPU
            UploadPointsToGPU();
        }

        /// <summary>
        /// Update point positions from DataObjects
        /// </summary>
        public void Update(List<DataObject> dataObjects)
        {
            _time += 0.016f; // Approximate frame time

            // Update positions from DataObjects (one point per DataObject)
            for (int i = 0; i < Math.Min(_pointCount, dataObjects.Count); i++)
            {
                _points[i] = dataObjects[i].Position;
            }

            // Upload updated positions to GPU
            UploadPointsToGPU();
        }

        /// <summary>
        /// Upload point positions to GPU buffer
        /// </summary>
        private unsafe void UploadPointsToGPU()
        {
            // Convert points to normalized coordinates (-1 to 1)
            float[] normalizedPoints = new float[_pointCount * 2];
            for (int i = 0; i < _pointCount; i++)
            {
                normalizedPoints[i * 2] = (_points[i].X / _screenSize.X) * 2.0f - 1.0f;
                normalizedPoints[i * 2 + 1] = (_points[i].Y / _screenSize.Y) * 2.0f - 1.0f;
            }

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* ptr = normalizedPoints)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(normalizedPoints.Length * sizeof(float)),
                    ptr, BufferUsageARB.DynamicDraw);
            }
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        /// <summary>
        /// Render all points using glDrawArrays(GL_POINTS)
        /// </summary>
        public void Render(float threshold = 0.5f)
        {
            _gl.UseProgram(_shaderProgram);
            _gl.BindVertexArray(_vao);

            // Set uniforms (only iTime exists in default shaders)
            _gl.Uniform1(_timeUniformLocation, _time);

            // Draw all points as GL_POINTS
            _gl.DrawArrays(GLEnum.Points, 0, (uint)_pointCount);

            _gl.BindVertexArray(0);
        }

        /// <summary>
        /// Update screen size (for window resizing)
        /// </summary>
        public void UpdateScreenSize(Vector2D<int> newScreenSize)
        {
            _screenSize = newScreenSize;
        }

        /// <summary>
        /// Get current point count
        /// </summary>
        public int PointCount => _pointCount;


        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
        }
    }
}
