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
        private static PointsRenderer _pointsRenderer;
        private static ShaderManager _shaderManager;
        private static Vector2D<int> _screenSize = new(1920, 1080);
        private static IInputContext _inputContext;
        private static Point _mousePosition = new(0, 0);
        private static float _alphaThreshold = 0.3f;
        private static List<DataObject> _ghostnetObjects;

        // FPS tracking
        private static int _frameCount = 0;
        private static double _fpsTimer = 0.0;
        private static double _fpsUpdateInterval = 0.5; // Update FPS every 0.5 seconds

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
            _pointsRenderer = new PointsRenderer(_gl, pointsShaderProgram, _screenSize);
            _ghostnetObjects = new List<DataObject>();

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

            SpawnGhostNetPoints();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Update all DataObjects
            foreach (var obj in _ghostnetObjects)
            {
                obj.Update((float)deltaTime);
            }

            // Update points renderer with current DataObject positions
            _pointsRenderer.Update(_ghostnetObjects);
        }

        private static void OnRender(double deltaTime)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _pointsRenderer.Render(_alphaThreshold);

            // Update FPS counter
            _frameCount++;
            _fpsTimer += deltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                double fps = _frameCount / _fpsTimer;
                _window.Title = $"MAELSTROM ! - GhostNet - FPS: {fps:F1} - DataObjects: {_ghostnetObjects.Count} - Points: {_pointsRenderer.PointCount:N0} - Threshold: {_alphaThreshold:F2} (Up/Down: threshold)";

                // Reset counters
                _frameCount = 0;
                _fpsTimer = 0.0;
            }
        }

        private static void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
        {
            _mousePosition = new Point(position.X, position.Y);

            // Make the first DataObject follow the mouse
            if (_ghostnetObjects.Count > 0)
            {
                _ghostnetObjects[0].SetPosition(_mousePosition);
            }
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
            _pointsRenderer.UpdateScreenSize(_screenSize);
        }

        private static void OnClosing()
        {
            _pointsRenderer?.Dispose();
            _shaderManager?.Dispose();
        }

        private static void SpawnGhostNetPoints()
        {
            Random random = new Random();
            _ghostnetObjects.Clear();

            // Create DataObjects with slow, linear movement
            for (int i = 0; i < 100000; i++)
            {
                // Random starting position
                Point startPos = new Point(
                    random.Next(50, _screenSize.X - 50),
                    random.Next(50, _screenSize.Y - 50)
                );

                // Slow, linear velocity (pixels per second)
                Point velocity = new Point(
                    (float)(random.NextDouble() - 0.5) * 20, // -10 to 10 px/s (slow)
                    (float)(random.NextDouble() - 0.5) * 20
                );

                // Create display object
                DisplayObject displayObj = new DisplayObject(_gl, _shaderManager.getShaderProgram("points"));

                // Create data object with 1x1 pixel size (single dot)
                DataObject dataObj = new DataObject(displayObj, startPos, velocity, _screenSize, new Dim(1, 1));

                // Add to collection
                _ghostnetObjects.Add(dataObj);
            }

            // Initialize points renderer with DataObjects
            _pointsRenderer.InitializePoints(_ghostnetObjects);
        }
    }
}
