using Silk.NET.OpenGL;
using Silk.NET.Maths;

namespace Maelstrom.GhostNet
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;

    /// <summary>
    /// Object pool for managing DataObjects efficiently
    /// </summary>
    public class ObjectPool
    {
        private readonly List<DataObject> _pool;
        private readonly List<DataObject> _activeObjects;
        private readonly GL _gl;
        private readonly uint _shaderProgram;
        private readonly Vector2D<int> _screenSize;
        private readonly Random _random;

        public ObjectPool(GL gl, uint shaderProgram, Vector2D<int> screenSize, int initialPoolSize = 100000)
        {
            _gl = gl;
            _shaderProgram = shaderProgram;
            _screenSize = screenSize;
            _random = new Random();
            _pool = new List<DataObject>();
            _activeObjects = new List<DataObject>();

            // Pre-create objects in the pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                var dataObject = new DataObject(
                    new Point(0, 0), // Will be set when activated
                    new Point(0, 0), // Not used in elliptical movement
                    _screenSize,
                    new Dim(1, 1), // Single pixel size
                    1.0f, // Default amplitudeA
                    1.0f  // Default amplitudeB
                );
                dataObject.SetVisible(false); // Start invisible
                _pool.Add(dataObject);
            }
        }

        public DataObject GetObject()
        {
            if (_pool.Count > 0)
            {
                var obj = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                _activeObjects.Add(obj);
                return obj;
            }
            else
            {
                // Create new object if pool is empty
                var dataObject = new DataObject(
                    new Point(0, 0),
                    new Point(0, 0),
                    _screenSize,
                    new Dim(1, 1),
                    1.0f,
                    1.0f
                );
                _activeObjects.Add(dataObject);
                return dataObject;
            }
        }

        public void ReturnObject(DataObject obj)
        {
            if (_activeObjects.Contains(obj))
            {
                _activeObjects.Remove(obj);
                obj.SetVisible(false);
                _pool.Add(obj);
            }
        }

        public List<DataObject> GetActiveObjects()
        {
            return _activeObjects;
        }

        public int GetPoolSize()
        {
            return _pool.Count;
        }

        public int GetActiveCount()
        {
            return _activeObjects.Count;
        }

        public void UpdateScreenSize(Vector2D<int> newScreenSize)
        {
        }

        public void Dispose()
        {
            _pool.Clear();
            _activeObjects.Clear();
        }
    }
}
