using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Corals
{
    /// <summary>
    /// Manages and renders multiple ShaderObjects
    /// </summary>
    public class Renderer
    {
        private readonly GL _gl;
        private readonly List<DisplayObject> _objects;
        private float _time;

        public Renderer(GL gl)
        {
            _gl = gl;
            _objects = new List<DisplayObject>();
            _time = 0.0f;
        }

        /// <summary>
        /// Add a shader object to the renderer
        /// </summary>
        public void AddObject(DisplayObject shaderObject)
        {
            _objects.Add(shaderObject);
        }

        /// <summary>
        /// Remove a shader object from the renderer
        /// </summary>
        public void RemoveObject(DisplayObject shaderObject)
        {
            _objects.Remove(shaderObject);
        }

        /// <summary>
        /// Update the renderer (call this each frame)
        /// </summary>
        public void Update(double deltaTime)
        {
            _time += (float)deltaTime;

            // Update all objects for movement
            foreach (var obj in _objects)
            {
                obj.Update((float)deltaTime);
            }
        }

        /// <summary>
        /// Render all shader objects
        /// </summary>
        public void Render(bool clearScreen = true)
        {
            // Clear the screen only if requested (not when rendering to framebuffer)
            if (clearScreen)
            {
                _gl.Clear(ClearBufferMask.ColorBufferBit);
            }

            // Render each object
            foreach (var obj in _objects)
            {
                obj.Render(_time);
            }
        }

        /// <summary>
        /// Get all objects (for manipulation)
        /// </summary>
        public IReadOnlyList<DisplayObject> Objects => _objects.AsReadOnly();

        /// <summary>
        /// Get current time
        /// </summary>
        public float GetTime() => _time;

        /// <summary>
        /// Clean up all objects
        /// </summary>
        public void Dispose()
        {
            foreach (var obj in _objects)
            {
                obj.Dispose();
            }
            _objects.Clear();
        }
    }
}
