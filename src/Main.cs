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
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.SrcAlpha);

            // Initialize managers
            _shaderManager = new ShaderManager(_gl);
            _renderer = new Renderer(_gl);

            // Load shaders
            _causticShader = _shaderManager.LoadShader("assets/shaders/vertex.vert", "assets/shaders/fragment.frag");

            // Create some shader objects at different positions
            InitializeObjects();

            // Set up input
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += OnKeyDown;
            }
        }

        private static void InitializeObjects()
        {
            int objectNb = 1;
            var random = new Random();
            for (int i = 0; i < objectNb; i++)
            {
                var randomPosition = new Point((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1);
                var shaderObject = new DisplayObject(_gl, _causticShader, randomPosition);
                _renderer.AddObject(shaderObject);
            }

            // Voronoi.InitVoronoi(_renderer.Objects.ToList());
            // Voronoi.ComputeVoronoi();
        }

        private static void OnUpdate(double deltaTime)
        {
            var random = new Random();
            _renderer.Objects[0].SetObjectPosition(_renderer.Objects[0].Position.X + ((float)random.NextDouble() * 2 - 1) / 100, _renderer.Objects[0].Position.Y + ((float)random.NextDouble() * 2 - 1) / 100);

            _renderer.Update(deltaTime);
            // Voronoi.ComputeVoronoi(_renderer.GetTime());
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
    }
}
