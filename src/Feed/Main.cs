using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;

namespace Maelstrom.Feed
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;

    public class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static Renderer _renderer;
        private static ShaderManager _shaderManager;
        private static Vector2D<int> _screenSize = new(1920, 1080);
        private static IInputContext _inputContext;
        private static Point _mousePosition = new(0, 0);
        private static List<DataObject> _FeedObjects;

        // FPS tracking
        private static int _frameCount = 0;
        private static double _fpsTimer = 0.0;
        private static double _fpsUpdateInterval = 0.5; // Update FPS every 0.5 seconds

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = _screenSize,
                Title = "MAELSTROM ! - Feed",
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
            _renderer = new Renderer(_gl);
            _FeedObjects = new List<DataObject>();

            // Load shaders
            _shaderManager.LoadShader("default", "assets/shaders/Feed.vert", "assets/shaders/Feed.frag");

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

            SpawnFeedObjects();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Update object position to follow mouse
            _renderer.Update(deltaTime);
        }

        private static void OnRender(double deltaTime)
        {
            _renderer.Render(true);
            // Update FPS counter
            _frameCount++;
            _fpsTimer += deltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                double fps = _frameCount / _fpsTimer;
                _window.Title = $"MAELSTROM ! - Feed - FPS: {fps:F1}";

                // Reset counters
                _frameCount = 0;
                _fpsTimer = 0.0;
            }
        }

        private static void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
        {
            _mousePosition = new Point(position.X, position.Y);
        }

        private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            switch (key)
            {
                case Key.Escape:
                    _window.Close();
                    break;
            }
        }

        private static void OnResize(Vector2D<int> size)
        {
            _screenSize = size;
            _window.Size = _screenSize;
            _gl.Viewport(0, 0, (uint)_screenSize.X, (uint)_screenSize.Y);
        }

        private static void OnClosing()
        {
            _renderer?.Dispose();
            _shaderManager?.Dispose();
        }

        private static void SpawnFeedObjects()
        {
            Random random = new Random();

            for (int i = 0; i < 1000; i++)
            {
                // Random starting position
                Point startPos = new Point(
                    random.Next(50, _screenSize.X - 50),
                    random.Next(50, _screenSize.Y - 50)
                );

                // Random velocity (pixels per second)
                Point velocity = new Point(
                    (float)(random.NextDouble() - 0.5) * 200, // -100 to 100 px/s
                    (float)(random.NextDouble() - 0.5) * 200
                );

                // Create display object
                DisplayObject displayObj = new DisplayObject(_gl, _shaderManager.getShaderProgram("default"));

                // Create data object with 10x10 pixel size
                DataObject dataObj = new DataObject(displayObj, startPos, velocity, _screenSize, new Dim(10, 10));

                // Add to collections
                _FeedObjects.Add(dataObj);
                _renderer.AddObject(dataObj);
            }
        }
    }
}
