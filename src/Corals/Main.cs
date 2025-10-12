using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Maelstrom.Corals
{
    using Point = Vector2D<float>;

    public class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static Renderer _renderer;
        private static ShaderManager _shaderManager;
        private static Vector2D<int> _screenSize = new(1920, 1080);
        private static IInputContext _inputContext;
        private static Point _mousePosition = new(0, 0);

        // FPS tracking
        private static int _frameCount = 0;
        private static double _fpsTimer = 0.0;
        private static double _fpsUpdateInterval = 0.5; // Update FPS every 0.5 seconds

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = _screenSize,
                Title = "MAELSTROM !"
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
            _gl = _window.CreateOpenGL();
            _gl.BlendFunc(BlendingFactor.OneMinusDstColor, BlendingFactor.DstColor);
            _gl.Enable(GLEnum.Blend);
            _gl.ClearColor(Color.Black);

            _shaderManager = new ShaderManager(_gl);
            _renderer = new Renderer(_gl);

            _shaderManager.LoadShader("coral", "assets/shaders/corals.vert", "assets/shaders/corals.frag");

            _inputContext = _window.CreateInput();
            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += OnKeyDown;
            }

            for (int i = 0; i < _inputContext.Mice.Count; i++)
            {
                _inputContext.Mice[i].MouseMove += OnMouseMove;
            }

            _renderer.AddObject(new DisplayObject(_gl, _shaderManager.getShaderProgram("coral"), _screenSize));
            _renderer.AddObject(new DisplayObject(_gl, _shaderManager.getShaderProgram("coral"), _screenSize));
            _renderer.AddObject(new DisplayObject(_gl, _shaderManager.getShaderProgram("coral"), _screenSize));
        }

        private static void OnUpdate(double deltaTime)
        {
            _renderer.Update(deltaTime);
        }

        private static void OnRender(double deltaTime)
        {
            _renderer.Render(false);
            _frameCount++;
            _fpsTimer += deltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                double fps = _frameCount / _fpsTimer;
                _window.Title = $"MAELSTROM ! - FPS: {fps:F1}";

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

            // Update screen size in all display objects
            foreach (var obj in _renderer.Objects)
            {
                obj.UpdateScreenSize(_screenSize);
            }
        }

        private static void OnClosing()
        {
            _renderer?.Dispose();
            _shaderManager?.Dispose();
        }
    }
}
