using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using DataViz.Data;
using DataViz.Rendering;

namespace DataViz
{
    /// <summary>
    /// Main application class for the caustics visualization
    /// Handles dual window rendering without cuts
    /// </summary>
    public class CausticApplication
    {
        private GL? _gl;
        private IWindow? _window1, _window2;
        private CausticPointCollection _points;
        private CausticRenderer? _renderer;
        private Vector2 _worldBounds;
        private bool _isRunning = false;

        public CausticApplication()
        {
            _points = new CausticPointCollection();
            _worldBounds = new Vector2(20.0f, 15.0f); // World coordinate bounds
        }

        /// <summary>
        /// Initialize and run the application
        /// </summary>
        public void Run()
        {
            // Generate some test points
            GenerateTestPoints();

            // Create windows
            CreateWindows();

            // Start the main loop
            _isRunning = true;
            RunMainLoop();
        }

        /// <summary>
        /// Generate test points for visualization
        /// </summary>
        private void GenerateTestPoints()
        {
            // Generate a grid of points with varying properties
            _points.GenerateGridPoints(6, 8, new Vector2(2.5f, 2.0f));

            // Add some random points for more interesting patterns
            _points.GenerateRandomPoints(10, _worldBounds);
        }

        /// <summary>
        /// Create the dual windows
        /// </summary>
        private void CreateWindows()
        {
            var opts = WindowOptions.Default with
            {
                Size = new Silk.NET.Maths.Vector2D<int>(800, 600),
                Title = "Caustics Visualization - Window 1"
            };

            // Create first window
            _window1 = Window.Create(opts with { Position = new(0, 0) });
            _window1.Load += OnWindow1Load;
            _window1.Render += OnWindow1Render;
            _window1.Update += OnUpdate;
            _window1.Closing += OnClosing;

            // Create second window positioned next to the first
            _window2 = Window.Create(opts with
            {
                Position = new(820, 0),
                Title = "Caustics Visualization - Window 2"
            });
            _window2.Load += OnWindow2Load;
            _window2.Render += OnWindow2Render;
            _window2.Update += OnUpdate;
            _window2.Closing += OnClosing;
        }

        /// <summary>
        /// Run the main application loop
        /// </summary>
        private void RunMainLoop()
        {
            Console.WriteLine("Starting windows...");

            // Start the first window on a separate thread
            var task1 = Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("Starting window 1...");
                    _window1!.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Window 1 error: {ex.Message}");
                }
            });

            // Start the second window on a separate thread
            var task2 = Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("Starting window 2...");
                    _window2!.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Window 2 error: {ex.Message}");
                }
            });

            // Wait for either window to close
            Task.WaitAny(task1, task2);

            Console.WriteLine("Closing application...");
            _isRunning = false;
        }

        /// <summary>
        /// Handle window 1 load event
        /// </summary>
        private void OnWindow1Load()
        {
            try
            {
                Console.WriteLine("Loading window 1...");
                _gl = GL.GetApi(_window1!);
                _renderer = new CausticRenderer(_points, _worldBounds);
                _renderer.Initialize(_gl, _window1);
                Console.WriteLine("Window 1 loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Window 1 load error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle window 2 load event
        /// </summary>
        private void OnWindow2Load()
        {
            try
            {
                Console.WriteLine("Loading window 2...");
                // Use the same GL context and renderer for consistency
                if (_renderer != null)
                {
                    _renderer.UpdateWindowSize(new Vector2(_window2!.Size.X, _window2.Size.Y));
                    Console.WriteLine("Window 2 loaded successfully");
                }
                else
                {
                    Console.WriteLine("Warning: Renderer not initialized for window 2");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Window 2 load error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle window 1 render event
        /// </summary>
        private void OnWindow1Render(double deltaTime)
        {
            if (_renderer != null)
            {
                // Set viewport for seamless rendering across windows
                _gl!.Viewport(0, 0, (uint)_window1!.Size.X, (uint)_window1.Size.Y);
                _renderer.Render(deltaTime);
            }
        }

        /// <summary>
        /// Handle window 2 render event
        /// </summary>
        private void OnWindow2Render(double deltaTime)
        {
            if (_renderer != null)
            {
                // Set viewport for seamless rendering across windows
                _gl!.Viewport(0, 0, (uint)_window2!.Size.X, (uint)_window2.Size.Y);
                _renderer.Render(deltaTime);
            }
        }

        /// <summary>
        /// Handle update events
        /// </summary>
        private void OnUpdate(double deltaTime)
        {
            // Update logic can go here if needed
        }

        /// <summary>
        /// Handle window closing events
        /// </summary>
        private void OnClosing()
        {
            _isRunning = false;
            _renderer?.Dispose();
        }

        /// <summary>
        /// Add a new point to the visualization
        /// </summary>
        public void AddPoint(Vector2 position, float wallWidth, float agitation, Vector3 color)
        {
            _points.AddPoint(position, wallWidth, agitation, color);
        }

        /// <summary>
        /// Clear all points
        /// </summary>
        public void ClearPoints()
        {
            _points.Clear();
        }

        /// <summary>
        /// Generate new random points
        /// </summary>
        public void GenerateNewPoints(int count)
        {
            _points.GenerateRandomPoints(count, _worldBounds);
        }

        /// <summary>
        /// Update world bounds
        /// </summary>
        public void UpdateWorldBounds(Vector2 newBounds)
        {
            _worldBounds = newBounds;
            _renderer?.UpdateWindowSize(newBounds);
        }
    }
}
