using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Runtime.CompilerServices;

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
        private static uint _postProcessShader;
        private static PostProcessor _postProcessor;
        private static Vector2D<int> _screenSize = new(1280, 720);
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
            // Initialize OpenGL
            _gl = _window.CreateOpenGL();
            _gl.ClearColor(Color.Black);

            // Initialize managers
            _shaderManager = new ShaderManager(_gl);
            _renderer = new Renderer(_gl);

            // Load shaders
            _causticShader = _shaderManager.LoadShader("assets/shaders/vertex.vert", "assets/shaders/fragment.frag");
            _postProcessShader = _shaderManager.LoadShader("assets/shaders/postprocess_vertex.vert", "assets/shaders/postprocess_fragment.frag");

            // Create some shader objects at different positions
            InitializeObjects();

            // Initialize post-processor
            _postProcessor = new PostProcessor(_gl, _postProcessShader, _screenSize);

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
        }

        private static void InitializeObjects()
        {
            int objectNb = 200;
            var random = new Random();
            for (int i = 0; i < objectNb; i++)
            {
                var randomPosition = new Point((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1);
                var shaderObject = new DisplayObject(_gl, _causticShader, randomPosition);
                _renderer.AddObject(shaderObject);
            }

            Voronoi.InitVoronoi(_renderer.Objects.ToList(), 1.1f); // 10% margin for screen bounds
            Voronoi.ComputeVoronoi();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Update object position to follow mouse
            _renderer.Update(deltaTime);
            Voronoi.ComputeVoronoi(_renderer.GetTime());
        }

        private static void OnRender(double deltaTime)
        {
            // Begin rendering to framebuffer
            //_postProcessor.BeginRender();
            
            // Render the scene to framebuffer (don't clear screen)
            _renderer.Render(false);
            
            // End rendering and apply post-processing
           // _postProcessor.EndRender(_renderer.GetTime());

            // Update FPS counter
            _frameCount++;
            _fpsTimer += deltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                double fps = _frameCount / _fpsTimer;
                _window.Title = $"MAELSTROM ! - FPS: {fps:F1}";

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
            
            // Update post-processor with new screen size
        //    _postProcessor?.UpdateScreenSize(_screenSize);
        }

        private static void OnClosing()
        {
            _postProcessor?.Dispose();
            _renderer?.Dispose();
            _shaderManager?.Dispose();
        }
    }
}
