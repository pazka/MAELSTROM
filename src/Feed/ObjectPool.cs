using Silk.NET.OpenGL;

namespace Maelstrom.Feed
{
    /// <summary>
    /// Object pool for DisplayObjects to avoid frequent allocation/deallocation
    /// </summary>
    public class ObjectPool
    {
        private readonly GL _gl;
        private readonly uint _shaderProgram;
        private readonly Queue<DisplayObject> _availableObjects;
        private readonly List<DisplayObject> _allObjects;
        private readonly int _initialPoolSize;
        private readonly int _maxPoolSize;

        public ObjectPool(GL gl, uint shaderProgram, int initialPoolSize = 100000, int maxPoolSize = 1000000)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _initialPoolSize = initialPoolSize;
            _maxPoolSize = maxPoolSize;
            _availableObjects = new Queue<DisplayObject>();
            _allObjects = new List<DisplayObject>();

            // Pre-create initial pool
            for (int i = 0; i < _initialPoolSize; i++)
            {
                var displayObject = new DisplayObject(_gl, _shaderProgram);
                displayObject.SetEnabled(false); // Start disabled
                _availableObjects.Enqueue(displayObject);
                _allObjects.Add(displayObject);
            }
        }

        /// <summary>
        /// Get a DisplayObject from the pool. Creates new ones if needed up to maxPoolSize
        /// </summary>
        public DisplayObject GetObject()
        {
            if (_availableObjects.Count > 0)
            {
                var obj = _availableObjects.Dequeue();
                obj.SetEnabled(true);
                return obj;
            }

            // No available objects, create new one if under max limit
            if (_allObjects.Count < _maxPoolSize)
            {
                Console.WriteLine($"Creating new object, total count: {_allObjects.Count}");
                var newObj = new DisplayObject(_gl, _shaderProgram);
                newObj.SetEnabled(true);
                _allObjects.Add(newObj);
                return newObj;
            }

                Console.WriteLine($"Pool exhausted, total count: {_allObjects.Count}");
            // Pool exhausted, return null
            return null;
        }

        /// <summary>
        /// Return a DisplayObject to the pool
        /// </summary>
        public void ReturnObject(DisplayObject displayObject)
        {
            if (displayObject != null && _allObjects.Contains(displayObject))
            {
                displayObject.SetEnabled(false);
                _availableObjects.Enqueue(displayObject);
            }
        }

        /// <summary>
        /// Get all objects in the pool (for rendering)
        /// </summary>
        public IReadOnlyList<DisplayObject> AllObjects => _allObjects.AsReadOnly();

        /// <summary>
        /// Get count of available objects
        /// </summary>
        public int AvailableCount => _availableObjects.Count;

        /// <summary>
        /// Get total count of objects in pool
        /// </summary>
        public int TotalCount => _allObjects.Count;

        /// <summary>
        /// Dispose all objects in the pool
        /// </summary>
        public void Dispose()
        {
            foreach (var obj in _allObjects)
            {
                obj.Dispose();
            }
            _allObjects.Clear();
            _availableObjects.Clear();
        }
    }
}
