using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.Feed
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;
    /// <summary>
    /// Manages and renders multiple DataObjects
    /// </summary>
    public class Renderer
    {
        private readonly GL _gl;
        private readonly List<DataObject> _objects;
        private readonly ObjectPool _objectPool;
        private float _time;

        public Renderer(GL gl, ObjectPool objectPool)
        {
            _gl = gl;
            _objectPool = objectPool;
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
        /// Create a new DataObject using the object pool
        /// </summary>
        public DataObject CreateDataObject(Point initialPosition, Point initialVelocity, Vector2D<int> screenSize, Dim pixelSize)
        {
            var displayObject = _objectPool.GetObject();
            if (displayObject == null)
            {
                Console.WriteLine("Warning: Object pool exhausted, cannot create new DataObject");
                return null;
            }

            var dataObject = new DataObject(displayObject, initialPosition, initialVelocity, screenSize, pixelSize);
            _objects.Add(dataObject);
            return dataObject;
        }

        /// <summary>
        /// Remove and return a DataObject to the pool
        /// </summary>
        public void ReturnDataObject(DataObject dataObject)
        {
            if (_objects.Remove(dataObject))
            {
                _objectPool.ReturnObject(dataObject.DisplayObject);
            }
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

            // Render each object's display object (only if enabled)
            foreach (var obj in _objects)
            {
                if (obj.DisplayObject.IsEnabled)
                {
                    obj.DisplayObject.Render(_time);
                }
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
            // Return all objects to the pool instead of disposing them
            foreach (var obj in _objects)
            {
                _objectPool.ReturnObject(obj.DisplayObject);
            }
            _objects.Clear();
            
            // Dispose the object pool
            _objectPool.Dispose();
        }
    }
}
