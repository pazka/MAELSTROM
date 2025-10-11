using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Phishing
{
    /// <summary>
    /// Manages and renders multiple DataObjects
    /// </summary>
    public class Renderer
    {
        private readonly GL _gl;
        private readonly List<DataObject> _objects;
        private float _time;

        public Renderer(GL gl)
        {
            _gl = gl;
            _objects = new List<DataObject>();
            _time = 0.0f;
        }

        /// <summary>
        /// Add a data object to the renderer
        /// </summary>
        public void AddObject(DataObject dataObject)
        {
            _objects.Add(dataObject);
        }

        /// <summary>
        /// Remove a data object from the renderer
        /// </summary>
        public void RemoveObject(DataObject dataObject)
        {
            _objects.Remove(dataObject);
        }

        /// <summary>
        /// Update the renderer (call this each frame)
        /// </summary>
        public void Update(double deltaTime)
        {
            _time += (float)deltaTime;

            // Update all data objects
            foreach (var obj in _objects)
            {
                obj.Update((float)deltaTime);
            }
        }

        /// <summary>
        /// Render all data objects
        /// </summary>
        public void Render(bool clearScreen = true)
        {
            // Clear the screen only if requested (not when rendering to framebuffer)
            if (clearScreen)
            {
                _gl.Clear(ClearBufferMask.ColorBufferBit);
            }

            // Render each object's display object
            foreach (var obj in _objects)
            {
                obj.DisplayObject.Render(_time);
            }
        }

        /// <summary>
        /// Get all objects (for manipulation)
        /// </summary>
        public IReadOnlyList<DataObject> Objects => _objects.AsReadOnly();

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
                obj.DisplayObject.Dispose();
            }
            _objects.Clear();
        }
    }
}
