using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using DataViz.Core;
using DataViz.Rendering;

namespace DataViz
{
    /// <summary>
    /// Skeleton application for caustics visualization
    /// Easy to customize point movement and shader properties
    /// </summary>
    public class SkeletonApp
    {
        private GL? _gl;
        private IWindow? _window;
        private PointManager _pointManager;
        private CausticRenderer? _renderer;
        private Vector2 _worldBounds;
        private float _time;

        public SkeletonApp()
        {
            _worldBounds = new Vector2(20.0f, 15.0f);
            _pointManager = new PointManager(_worldBounds);
        }

        /// <summary>
        /// Run the skeleton application
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Starting Caustics Skeleton Application...");
            Console.WriteLine("This is a customizable skeleton for point movement and shader properties.");
            Console.WriteLine();

            // Setup your points here - customize this section!
            SetupPoints();

            // Create window
            CreateWindow();

            // Run the application
            _window!.Run();
        }

        /// <summary>
        /// Setup your custom points and movement patterns
        /// Override this method to create your own point configurations
        /// </summary>
        protected virtual void SetupPoints()
        {
            Console.WriteLine("Setting up points...");

            // Example 1: Add some circular moving points
            for (int i = 0; i < 5; i++)
            {
                var point = new DataViz.Data.CausticPoint(
                    new Vector2(5.0f + i * 2.0f, 5.0f),
                    0.3f, // Wall width
                    0.8f, // Agitation
                    new Vector3(1.0f, 0.5f, 0.2f) // Orange color
                );

                _pointManager.AddCircularPoint(point,
                    radius: 1.0f + i * 0.2f,
                    speed: 0.5f + i * 0.1f,
                    phase: i * 1.2f);
            }

            // Example 2: Add some wave moving points
            for (int i = 0; i < 3; i++)
            {
                var point = new DataViz.Data.CausticPoint(
                    new Vector2(2.0f, 8.0f + i * 2.0f),
                    0.25f, // Wall width
                    0.6f, // Agitation
                    new Vector3(0.2f, 0.8f, 1.0f) // Blue color
                );

                _pointManager.AddWavePoint(point,
                    amplitude: 0.8f,
                    frequency: 1.0f + i * 0.3f,
                    direction: Vector2.UnitX);
            }

            // Example 3: Add some random walk points
            for (int i = 0; i < 4; i++)
            {
                var point = new DataViz.Data.CausticPoint(
                    new Vector2(15.0f + i * 1.0f, 10.0f + i * 0.5f),
                    0.2f, // Wall width
                    1.0f, // High agitation
                    new Vector3(1.0f, 0.2f, 0.8f) // Pink color
                );

                _pointManager.AddRandomWalkPoint(point, maxSpeed: 0.3f);
            }

            // Example 4: Add some static points
            for (int i = 0; i < 3; i++)
            {
                var point = new DataViz.Data.CausticPoint(
                    new Vector2(10.0f + i * 1.5f, 2.0f),
                    0.4f, // Large wall width
                    0.3f, // Low agitation
                    new Vector3(0.8f, 1.0f, 0.2f) // Green color
                );

                _pointManager.AddPoint(point); // Static point
            }

            Console.WriteLine($"Added {_pointManager.Count} points");
        }

        /// <summary>
        /// Create the window
        /// </summary>
        private void CreateWindow()
        {
            var opts = WindowOptions.Default with
            {
                Size = new Silk.NET.Maths.Vector2D<int>(1200, 800),
                Title = "Caustics Skeleton - Customize Me!",
                WindowState = WindowState.Normal,
                WindowBorder = WindowBorder.Resizable
            };

            _window = Window.Create(opts);
            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Update += OnUpdate;
            _window.Closing += OnClosing;

            Console.WriteLine("Window created. Close the window to exit.");
        }

        /// <summary>
        /// Handle window load
        /// </summary>
        private void OnLoad()
        {
            try
            {
                Console.WriteLine("Loading OpenGL context...");
                _gl = GL.GetApi(_window!);

                Console.WriteLine("Initializing renderer...");
                _renderer = new CausticRenderer(new DataViz.Data.CausticPointCollection(), _worldBounds);
                _renderer.Initialize(_gl, _window);

                Console.WriteLine("Application loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle rendering
        /// </summary>
        private void OnRender(double deltaTime)
        {
            if (_renderer != null)
            {
                _time += (float)deltaTime;

                // Update points with current time
                var currentPoints = _pointManager.UpdatePoints(_time);

                // Update renderer with current points
                var pointCollection = new DataViz.Data.CausticPointCollection();
                foreach (var point in currentPoints)
                {
                    pointCollection.AddPoint(point);
                }

                _renderer.UpdatePoints(pointCollection);
                _renderer.Render(deltaTime);
            }
        }

        /// <summary>
        /// Handle updates
        /// </summary>
        private void OnUpdate(double deltaTime)
        {
            // Add any custom update logic here
            // For example, you could modify point properties over time
        }

        /// <summary>
        /// Handle window closing
        /// </summary>
        private void OnClosing()
        {
            Console.WriteLine("Closing application...");
            _renderer?.Dispose();
        }

        /// <summary>
        /// Get the point manager for external customization
        /// </summary>
        public PointManager GetPointManager()
        {
            return _pointManager;
        }

        /// <summary>
        /// Get current time for external use
        /// </summary>
        public float GetTime()
        {
            return _time;
        }
    }
}
