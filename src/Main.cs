using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;

namespace maelstrom_poc
{
    using Point = Vector2D<float>;
    public class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static Renderer _renderer;
        private static ShaderManager _shaderManager;
        private static uint _causticShader;
        private static Vector2D<int> _screenSize = new Vector2D<int>(1080, 720);
        private static Point _mousePosition = new Point(0, 0);
        private static IInputContext _inputContext;

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
            // Initialize OpenGL
            _gl = _window.CreateOpenGL();
            _gl.ClearColor(Color.Black);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrc1Alpha);

            // Initialize managers
            _shaderManager = new ShaderManager(_gl);
            _renderer = new Renderer(_gl);

            // Load shaders
            _causticShader = _shaderManager.LoadShader("assets/shaders/vertex.vert", "assets/shaders/fragment.frag");

            // Create some shader objects at different positions
            InitializeObjects();

            // Set up input
            _inputContext = _window.CreateInput();
            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += OnKeyDown;
            }

            // Set up mouse input
            for (int i = 0; i < _inputContext.Mice.Count; i++)
            {
                _inputContext.Mice[i].MouseDown += OnMouseDown;
                _inputContext.Mice[i].MouseUp += OnMouseUp;
            }
        }

        private static void InitializeObjects()
        {
            int objectNb = 5;
            var random = new Random();
            for (int i = 0; i < objectNb; i++)
            {
                var randomPosition = new Point((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1);
                var shaderObject = new DisplayObject(_gl, _causticShader, randomPosition);
                _renderer.AddObject(shaderObject);
            }

            Voronoi.InitVoronoi(_renderer.Objects.ToList());
            // Voronoi.ComputeVoronoi();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Get current mouse position
            if (_inputContext.Mice.Count > 0)
            {
                var mouse = _inputContext.Mice[0];
                var rawPosition = mouse.Position;

                // Convert screen coordinates to normalized coordinates (-1 to 1)
                _mousePosition = new Point(
                    (float)(rawPosition.X / _screenSize.X) * 2.0f - 1.0f,
                    1.0f - (float)(rawPosition.Y / _screenSize.Y) * 2.0f
                );
            }


            // You can access the current mouse position here using _mousePosition
            // For example, to make an object follow the mouse:
            //_renderer.Objects[0].SetObjectPosition(_mousePosition.X, _mousePosition.Y);

            _renderer.Update(deltaTime);
            Voronoi.ComputeVoronoi(_renderer.GetTime());
        }

        private static void OnRender(double deltaTime)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _renderer.Render();
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


        private static void OnMouseDown(IMouse mouse, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                Console.WriteLine($"Mouse clicked at: {_mousePosition}");
                // You can use _mousePosition here for your application logic
            }
        }

        private static void OnMouseUp(IMouse mouse, MouseButton button)
        {
            // Handle mouse up events if needed
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

        // Public method to get current mouse position (normalized coordinates -1 to 1)
        public static Point GetMousePosition()
        {
            return _mousePosition;
        }

        // Public method to get raw mouse position (screen coordinates)
        public static Point GetRawMousePosition()
        {
            if (_inputContext?.Mice.Count > 0)
            {
                var mouse = _inputContext.Mice[0];
                return new Point(mouse.Position.X, mouse.Position.Y);
            }
            return new Point(0, 0);
        }
    }
}
