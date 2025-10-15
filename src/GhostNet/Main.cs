using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;

namespace Maelstrom.GhostNet
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;

    public class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static PointsRenderer _whiteSlowRenderer;
        private static PointsRenderer _midBlueRenderer;
        private static PointsRenderer _jitteryBlueRenderer;
        private static ShaderManager _shaderManager;
        private static Vector2D<int> _screenSize = new(1920, 1080);
        private static IInputContext _inputContext;
        private static Point _mousePosition = new(0, 0);
        private static float _alphaThreshold = 0.3f;
        private static ObjectPool _whiteSlowPool;
        private static ObjectPool _midBluePool;
        private static ObjectPool _jitteryBluePool;

        // FPS tracking
        private static int _frameCount = 0;
        private static double _fpsTimer = 0.0;
        private static double _fpsUpdateInterval = 0.5; // Update FPS every 0.5 seconds

        // Timing for object creation and destruction
        private static double _lastObjectCreationTime = 0.0;
        private static double _objectCreationInterval = 0.2; // Create objects every 0.2 seconds (slower)
        private static double _lastDestructionTime = 0.0;
        private static double _destructionInterval = 0.3; // Check for old objects every 0.3 seconds

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = _screenSize,
                Title = "MAELSTROM ! - GhostNet",
            };

            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;

            _window.Run();
        }

        private static void OnLoad()
        {
            // Initialize OpenGL
            _gl = _window.CreateOpenGL();
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);
            _gl.Enable(GLEnum.Blend);
            _gl.ClearColor(Color.Black);

            // Initialize managers
            _shaderManager = new ShaderManager(_gl);

            // Load shaders for points rendering
            _shaderManager.LoadShader("points", "assets/shaders/points.vert", "assets/shaders/points.frag");

            // Initialize points renderer
            uint pointsShaderProgram = _shaderManager.getShaderProgram("points");
            
            // Initialize 3 separate renderers
            _whiteSlowRenderer = new PointsRenderer(_gl, pointsShaderProgram, _screenSize);
            _midBlueRenderer = new PointsRenderer(_gl, pointsShaderProgram, _screenSize);
            _jitteryBlueRenderer = new PointsRenderer(_gl, pointsShaderProgram, _screenSize);
            
            // Initialize 3 separate object pools with much larger capacity
            _whiteSlowPool = new ObjectPool(_gl, pointsShaderProgram, _screenSize, 50000);
            _midBluePool = new ObjectPool(_gl, pointsShaderProgram, _screenSize, 50000);
            _jitteryBluePool = new ObjectPool(_gl, pointsShaderProgram, _screenSize, 50000);

            // Set up input
            _inputContext = _window.CreateInput();
            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += OnKeyDown;
            }

            for (int i = 0; i < _inputContext.Mice.Count; i++)
            {
                _inputContext.Mice[i].MouseMove += OnMouseMove;
            }

            // Initialize with some objects
            CreateRandomObjects();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Update all active DataObjects for each pool
            var whiteSlowObjects = _whiteSlowPool.GetActiveObjects();
            var midBlueObjects = _midBluePool.GetActiveObjects();
            var jitteryBlueObjects = _jitteryBluePool.GetActiveObjects();
            
            foreach (var obj in whiteSlowObjects)
            {
                obj.Update((float)deltaTime);
            }
            foreach (var obj in midBlueObjects)
            {
                obj.Update((float)deltaTime);
            }
            foreach (var obj in jitteryBlueObjects)
            {
                obj.Update((float)deltaTime);
            }

            // Natural lifecycle management - remove old objects
            _lastDestructionTime += deltaTime;
            if (_lastDestructionTime >= _destructionInterval)
            {
                RemoveOldObjects();
                _lastDestructionTime = 0.0;
            }

            // Create new objects every 0.2 seconds (more balanced)
            _lastObjectCreationTime += deltaTime;
            if (_lastObjectCreationTime >= _objectCreationInterval)
            {
                CreateRandomObjects();
                _lastObjectCreationTime = 0.0;
            }

            // Update all renderers
            _whiteSlowRenderer.Update(whiteSlowObjects);
            _midBlueRenderer.Update(midBlueObjects);
            _jitteryBlueRenderer.Update(jitteryBlueObjects);
        }

        private static void OnRender(double deltaTime)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            
            // Render all 3 layers
            _whiteSlowRenderer.Render(_alphaThreshold);
            _midBlueRenderer.Render(_alphaThreshold);
            _jitteryBlueRenderer.Render(_alphaThreshold);

            // Update FPS counter
            _frameCount++;
            _fpsTimer += deltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                double fps = _frameCount / _fpsTimer;
                var whiteCount = _whiteSlowPool.GetActiveCount();
                var midBlueCount = _midBluePool.GetActiveCount();
                var jitteryCount = _jitteryBluePool.GetActiveCount();
                var totalActive = whiteCount + midBlueCount + jitteryCount;
                
                _window.Title = $"MAELSTROM ! - GhostNet - FPS: {fps:F1} - White: {whiteCount} - MidBlue: {midBlueCount} - Jittery: {jitteryCount} - Total: {totalActive} - Threshold: {_alphaThreshold:F2}";

                // Reset counters
                _frameCount = 0;
                _fpsTimer = 0.0;
            }
        }

        private static void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
        {
            _mousePosition = new Point(position.X, position.Y);

            // Make the first active object from each pool follow the mouse
            var whiteSlowObjects = _whiteSlowPool.GetActiveObjects();
            var midBlueObjects = _midBluePool.GetActiveObjects();
            var jitteryBlueObjects = _jitteryBluePool.GetActiveObjects();
            
            if (whiteSlowObjects.Count > 0)
                whiteSlowObjects[0].SetPosition(_mousePosition);
            if (midBlueObjects.Count > 0)
                midBlueObjects[0].SetPosition(_mousePosition);
            if (jitteryBlueObjects.Count > 0)
                jitteryBlueObjects[0].SetPosition(_mousePosition);
        }

        private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            switch (key)
            {
                case Key.Escape:
                    _window.Close();
                    break;
                case Key.Up:
                    _alphaThreshold = Math.Min(1.0f, _alphaThreshold + 0.1f);
                    break;
                case Key.Down:
                    _alphaThreshold = Math.Max(0.0f, _alphaThreshold - 0.1f);
                    break;
            }
        }


        private static void OnResize(Vector2D<int> size)
        {
            _screenSize = size;
            _window.Size = _screenSize;
            _gl.Viewport(0, 0, (uint)_screenSize.X, (uint)_screenSize.Y);
            
            // Update all renderers and pools
            _whiteSlowRenderer.UpdateScreenSize(_screenSize);
            _midBlueRenderer.UpdateScreenSize(_screenSize);
            _jitteryBlueRenderer.UpdateScreenSize(_screenSize);
            
            _whiteSlowPool.UpdateScreenSize(_screenSize);
            _midBluePool.UpdateScreenSize(_screenSize);
            _jitteryBluePool.UpdateScreenSize(_screenSize);
        }

        private static void OnClosing()
        {
            _whiteSlowRenderer?.Dispose();
            _midBlueRenderer?.Dispose();
            _jitteryBlueRenderer?.Dispose();
            _shaderManager?.Dispose();
            _whiteSlowPool?.Dispose();
            _midBluePool?.Dispose();
            _jitteryBluePool?.Dispose();
        }

        private static void CreateRandomObjects()
        {
            Random random = new Random();
            
            // Create smaller batches for more natural variation
            int whiteCount = random.Next(20, 80);   // 20-80 white objects
            int midBlueCount = random.Next(20, 80); // 20-80 mid blue objects
            int jitteryCount = random.Next(20, 80); // 20-80 jittery objects
            
            // Create white slow objects
            for (int i = 0; i < whiteCount; i++)
            {
                var dataObj = _whiteSlowPool.GetObject();
                SetupDataObject(dataObj, PointType.WhiteSlow, random);
            }
            
            // Create mid blue objects
            for (int i = 0; i < midBlueCount; i++)
            {
                var dataObj = _midBluePool.GetObject();
                SetupDataObject(dataObj, PointType.MidBlueMedium, random);
            }
            
            // Create jittery blue objects
            for (int i = 0; i < jitteryCount; i++)
            {
                var dataObj = _jitteryBluePool.GetObject();
                SetupDataObject(dataObj, PointType.JitteryBlue, random);
            }
        }

        private static void SetupDataObject(DataObject dataObj, PointType pointType, Random random)
        {
            // Set random amplitudes with more variation for thousands of objects
            float amplitudeA = (float)(random.NextDouble() * 3.0 + 0.2); // 0.2 to 3.2
            float amplitudeB = (float)(random.NextDouble() * 4.0 + 0.3); // 0.3 to 4.3
            
            dataObj.AmplitudeA = amplitudeA;
            dataObj.AmplitudeB = amplitudeB;
            dataObj.PointType = pointType;
            
            // Set initial position to center with slight random offset for variety
            Point centerPosition = new Point(
                _screenSize.X / 2.0f + (float)(random.NextDouble() - 0.5) * 100.0f,
                _screenSize.Y / 2.0f + (float)(random.NextDouble() - 0.5) * 100.0f
            );
            dataObj.SetPosition(centerPosition);
            
            // Make visible
            dataObj.SetVisible(true);
        }

        private static void RemoveOldObjects()
        {
            // Remove objects that have exceeded their natural lifespan
            var whiteObjects = _whiteSlowPool.GetActiveObjects();
            var midBlueObjects = _midBluePool.GetActiveObjects();
            var jitteryObjects = _jitteryBluePool.GetActiveObjects();
            
            // Remove old white objects
            var oldWhiteObjects = whiteObjects.Where(obj => obj.ShouldBeDestroyed()).ToList();
            foreach (var obj in oldWhiteObjects)
            {
                _whiteSlowPool.ReturnObject(obj);
            }
            
            // Remove old mid blue objects
            var oldMidBlueObjects = midBlueObjects.Where(obj => obj.ShouldBeDestroyed()).ToList();
            foreach (var obj in oldMidBlueObjects)
            {
                _midBluePool.ReturnObject(obj);
            }
            
            // Remove old jittery objects
            var oldJitteryObjects = jitteryObjects.Where(obj => obj.ShouldBeDestroyed()).ToList();
            foreach (var obj in oldJitteryObjects)
            {
                _jitteryBluePool.ReturnObject(obj);
            }
        }
    }
}
