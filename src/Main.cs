using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;

namespace maelstrom_poc
{
    public class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static Renderer _renderer;
        private static ShaderManager _shaderManager;
        private static uint _causticShader;

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(1920, 1080),
                Title = "2D Shader Objects Demo"
            };

            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Closing += OnClosing;

            _window.Run();
        }

        private static void OnLoad()
        {
            // Initialize OpenGL
            _gl = _window.CreateOpenGL();
            _gl.ClearColor(Color.Black);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Initialize managers
            _shaderManager = new ShaderManager(_gl);
            _renderer = new Renderer(_gl);

            // Load shaders
            _causticShader = _shaderManager.LoadShader("assets/shaders/vertex.glsl", "assets/shaders/fragment.glsl");

            // Create some shader objects at different positions
            CreateDemoObjects();

            // Set up input
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += OnKeyDown;
            }
        }

        private static void CreateDemoObjects()
        {
            // Create multiple shader objects at different world positions
            var positions = new Vector2D<float>[]
            {
                new Vector2D<float>(-2.0f, 1.0f),   // Top-left
                new Vector2D<float>(0.0f, 1.0f),    // Top-center
                new Vector2D<float>(2.0f, 1.0f),    // Top-right
                new Vector2D<float>(-2.0f, -1.0f),  // Bottom-left
                new Vector2D<float>(0.0f, -1.0f),   // Bottom-center
                new Vector2D<float>(2.0f, -1.0f),   // Bottom-right
                new Vector2D<float>(-1.0f, 0.0f),   // Left-center
                new Vector2D<float>(1.0f, 0.0f),    // Right-center
            };

            foreach (var pos in positions)
            {
                var shaderObject = new DisplayObject(_gl, _causticShader, pos);
                _renderer.AddObject(shaderObject);
            }
        }

        private static void OnUpdate(double deltaTime)
        {
            _renderer.Update(deltaTime);
        }

        private static void OnRender(double deltaTime)
        {
            _renderer.Render();
        }

        private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            switch (key)
            {
                case Key.Escape:
                    _window.Close();
                    break;

                case Key.Space:
                    // Add a new random shader object
                    var random = new Random();
                    var randomPos = new Vector2D<float>(
                        (float)(random.NextDouble() * 4.0 - 2.0),  // -2 to 2
                        (float)(random.NextDouble() * 4.0 - 2.0)   // -2 to 2
                    );
                    var newObject = new DisplayObject(_gl, _causticShader, randomPos);
                    _renderer.AddObject(newObject);
                    break;

                case Key.R:
                    // Remove the last added object
                    if (_renderer.Objects.Count > 0)
                    {
                        var lastObject = _renderer.Objects[_renderer.Objects.Count - 1];
                        _renderer.RemoveObject(lastObject);
                        lastObject.Dispose();
                    }
                    break;
            }
        }

        private static void OnClosing()
        {
            _renderer?.Dispose();
            _shaderManager?.Dispose();
        }
    }
}
