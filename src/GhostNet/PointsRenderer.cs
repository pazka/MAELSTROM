using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Numerics;

namespace Maelstrom.GhostNet
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
        private readonly int _seedUniformLocation;
        private readonly int _resolutionUniformLocation;
        private readonly int _maelstromUniformLocation;
        private readonly int _amplitudeAUniformLocation;
        private readonly int _amplitudeBUniformLocation;

        private Point[] _points;
        private float[] _maelstromValues;
        private float[] _amplitudeAValues;
        private float[] _amplitudeBValues;
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
            _seedUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iSeed");
            _resolutionUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iResolution");
            _maelstromUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iMaelstrom");
            _amplitudeAUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iAmplitudeA");
            _amplitudeBUniformLocation = _gl.GetUniformLocation(_shaderProgram, "iAmplitudeB");

            // Create VAO and VBO
            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            // Set up vertex attributes for interleaved data (5 floats per point)
            // Position (2 floats)
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
            
            // Maelstrom (1 float)
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(2 * sizeof(float)));
            
            // AmplitudeA (1 float)
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
            
            // AmplitudeB (1 float)
            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(4 * sizeof(float)));

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        /// <summary>
        /// Update point positions from active DataObjects
        /// </summary>
        public void Update(List<DataObject> activeObjects)
        {
            _time += 0.016f; // Approximate frame time

            // Only update visible objects
            var visibleObjects = activeObjects.Where(obj => obj.Visible).ToList();
            _pointCount = visibleObjects.Count;

            // Resize arrays if needed
            if (_points == null || _points.Length < _pointCount)
            {
                _points = new Point[_pointCount];
                _maelstromValues = new float[_pointCount];
                _amplitudeAValues = new float[_pointCount];
                _amplitudeBValues = new float[_pointCount];
            }

            // Update positions and uniform values from visible DataObjects
            for (int i = 0; i < _pointCount; i++)
            {
                _points[i] = visibleObjects[i].Position;
                
                // Calculate maelstrom value for this object
                float timeSinceCreation = (float)(DateTime.Now - visibleObjects[i].TimeCreated).TotalSeconds;
                _maelstromValues[i] = Math.Min(1.0f, timeSinceCreation / 20.0f);
                
                _amplitudeAValues[i] = visibleObjects[i].AmplitudeA;
                _amplitudeBValues[i] = visibleObjects[i].AmplitudeB;
            }

            // Upload updated data to GPU
            UploadPointsToGPU();
        }

        /// <summary>
        /// Upload point positions and attributes to GPU buffer
        /// </summary>
        private unsafe void UploadPointsToGPU()
        {
            // Create interleaved vertex data: position (2) + maelstrom (1) + amplitudeA (1) + amplitudeB (1) = 5 floats per point
            float[] vertexData = new float[_pointCount * 5];
            
            for (int i = 0; i < _pointCount; i++)
            {
                int baseIndex = i * 5;
                
                // Position (normalized coordinates)
                vertexData[baseIndex] = (_points[i].X / _screenSize.X) * 2.0f - 1.0f;
                vertexData[baseIndex + 1] = (_points[i].Y / _screenSize.Y) * 2.0f - 1.0f;
                
                // Per-point attributes
                vertexData[baseIndex + 2] = _maelstromValues[i];
                vertexData[baseIndex + 3] = _amplitudeAValues[i];
                vertexData[baseIndex + 4] = _amplitudeBValues[i];
            }

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* ptr = vertexData)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(vertexData.Length * sizeof(float)),
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

            // Set global uniforms
            _gl.Uniform1(_timeUniformLocation, _time);
            _gl.Uniform2(_resolutionUniformLocation, (float)_screenSize.X, (float)_screenSize.Y);
            
            // Set default values for uniforms that might be used
            if (_seedUniformLocation != -1)
                _gl.Uniform1(_seedUniformLocation, 0.5f); // Default seed
            if (_maelstromUniformLocation != -1)
                _gl.Uniform1(_maelstromUniformLocation, 0.0f); // Global maelstrom (per-point is in attributes)
            if (_amplitudeAUniformLocation != -1)
                _gl.Uniform1(_amplitudeAUniformLocation, 1.0f); // Global amplitudeA (per-point is in attributes)
            if (_amplitudeBUniformLocation != -1)
                _gl.Uniform1(_amplitudeBUniformLocation, 1.0f); // Global amplitudeB (per-point is in attributes)

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
