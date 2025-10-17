using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using Npgsql;
using System.Linq;

namespace Maelstrom.Feed
{
    using Point = Vector2D<float>;
    using Dim = Vector2D<float>;

    public class Program
    {
        private static IWindow _window;
        private static IWindow _window2;
        private static GL _gl;
        private static Renderer _renderer;
        private static ShaderManager _shaderManager;
        private static ObjectPool _objectPool;
        private static Vector2D<int> _screenSize = new(1920, 1080);
        private static IInputContext _inputContext;
        private static float _loopDuration = 600.0f; // seconds
        // FPS tracking
        private static int _frameCount = 0;
        private static double _fpsTimer = 0.0;
        private static double _fpsUpdateInterval = 0.5; // Update FPS every 0.5 seconds
        private static float _internalMaelstrom = 0f;
        private static float _externalMaelstrom = 0f;

        // Data-driven display management
        private static DataPoint[] _data;
        private static int _currentDataIndex = 0;
        private static float _normalizedDisplayDuration; // One week in normalized data space
        private static List<DataObject> _activeObjects = new List<DataObject>();
        private static Random _random = new Random();
        private static DateTime _currentDisplayedDate = DateTime.MinValue;

        private static string connString = "Host=localhost;Username=postgres;Password=postgres;Database=maelstrom";

        private static NpgsqlConnection _connection;

        private static void ConnectToDatabase()
        {
            _connection = new NpgsqlConnection(connString);
            _connection.Open();
        }

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = _screenSize,
                Title = "MAELSTROM ! - Feed",
                WindowState = WindowState.Fullscreen,
            };

            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;
            Console.WriteLine("Loading Data...");

            WindowOptions options2 = WindowOptions.Default with
            {
                Size = _screenSize,
                Title = "MAELSTROM ! - Feed",
                WindowState = WindowState.Fullscreen,
                Position = new(1920, 0),
            };
            _window2 = Window.Create(options);
            _window2.Load += OnLoad;
            _window2.Update += OnUpdate;
            _window2.Render += OnRender;
            _window2.Resize += OnResize;
            _window2.Closing += OnClosing;

            DataLoader.LoadData();
            _data = DataLoader.GetData();
            _normalizedDisplayDuration = DataLoader.GetNormalizedDuration(TimeSpan.FromDays(7));

            Console.WriteLine($"Loaded Data: {_data.Length} data points");
            Console.WriteLine($"One week in normalized data space: {_normalizedDisplayDuration:F6}");

            _window.Run();
            _window2.Run();
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

            // Load shaders
            _shaderManager.LoadShader("default", "assets/shaders/feed.vert", "assets/shaders/feed.frag");

            // Initialize object pool and renderer
            _objectPool = new ObjectPool(_gl, _shaderManager.getShaderProgram("default"));
            _renderer = new Renderer(_gl, _objectPool);

            // Set up input
            _inputContext = _window.CreateInput();
            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += OnKeyDown;
            }
        }

        private static void OnUpdate(double deltaTime)
        {
            _renderer.Update(deltaTime);
            
            // Process data and manage display objects
            ProcessDataAndManageObjects();
        }

        private static void ProcessDataAndManageObjects()
        {
            float currentTime = _renderer.GetTime();
            float normalizedCurrentTime = currentTime / _loopDuration;

            // Debug output every 5 seconds
            if ((int)currentTime % 5 == 0 && (int)currentTime != (int)(currentTime - 0.016)) // ~60fps
            {
                Console.WriteLine($"Time: {currentTime:F1}s, Normalized: {normalizedCurrentTime:F6}, Active Objects: {_activeObjects.Count}, Data Index: {_currentDataIndex}/{_data.Length}");
                if (_currentDataIndex < _data.Length)
                {
                    var currentDataPoint = _data[_currentDataIndex];
                    Console.WriteLine($"  Next data point: {currentDataPoint.date:yyyy-MM-dd HH:mm:ss}, Retweets: {currentDataPoint.retweetCount}, Normalized: {currentDataPoint.normalizedDate:F6}");
                }
                
                // Log current date being displayed
                if (_activeObjects.Count > 0)
                {
                    // Find the most recent data point that's currently being displayed
                    var mostRecentObject = _activeObjects.OrderByDescending(obj => obj.CreationTime).FirstOrDefault();
                    if (mostRecentObject != null)
                    {
                        // Find the data point that corresponds to this object's creation time
                        var correspondingDataPoint = _data.FirstOrDefault(dp => 
                            Math.Abs(dp.normalizedDate - mostRecentObject.CreationTime) < 0.001f);
                        if (correspondingDataPoint.date != default(DateTime))
                        {
                            Console.WriteLine($"  CURRENT DATE DISPLAYED: {correspondingDataPoint.date:yyyy-MM-dd HH:mm:ss}");
                        }
                    }
                }
            }

            // Remove objects that have been displayed for one normalized week
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                var obj = _activeObjects[i];
                float objectAge = normalizedCurrentTime - obj.CreationTime;
                if (objectAge >= _normalizedDisplayDuration)
                {
                    _renderer.ReturnDataObject(obj);
                    _activeObjects.RemoveAt(i);
                }
            }

            // Process data points and create display objects
            while (_currentDataIndex < _data.Length)
            {
                var dataPoint = _data[_currentDataIndex];
                
                // Check if this data point should be displayed at current time
                if (dataPoint.normalizedDate <= normalizedCurrentTime)
                {
                    var dataObject = CreateRandomDataObject(dataPoint);
                    if (dataObject != null)
                    {
                        _activeObjects.Add(dataObject);
                        // Log when a new date is displayed
                        if(_currentDataIndex%1000 == 0) Console.WriteLine($"DISPLAYING DATE: {dataPoint.date:yyyy-MM-dd HH:mm:ss} (Retweets: {dataPoint.retweetCount})");
                        
                        // Update current displayed date
                        _currentDisplayedDate = dataPoint.date;
                    }
                    
                    _currentDataIndex++;
                }
                else
                {
                    // Data point is in the future, wait
                    break;
                }
            }

            // If we've reached the end of data, loop back to start
            if (_currentDataIndex >= _data.Length)
            {
                _currentDataIndex = 0;
                Console.WriteLine("Looping back to start of data");
            }
        }

        private static DataObject? CreateRandomDataObject(DataPoint dataPoint)
        {
            // Random position on screen
            Point position = new Point(
                _random.Next(0, _screenSize.X),
                _random.Next(0, _screenSize.Y)
            );

            // Velocity based on retweet count (normalized)
            // Higher retweet count = higher velocity
            float velocityScale = 150 - dataPoint.normalizedRetweetCount * 120; // 20 to 100 pixels per second
            Point velocity = new Point(
                (_random.NextSingle() - 0.5f) * velocityScale,
                (_random.NextSingle() - 0.5f) * velocityScale
            );

            // Size based on retweet count (normalized)
            // Higher retweet count = larger size
            float sizeScale = 25 + dataPoint.normalizedRetweetCount * 150; // 5 to 50 pixels
            Dim pixelSize = new Dim(sizeScale, sizeScale);

            var dataObject = _renderer.CreateDataObject(position, velocity, _screenSize, pixelSize);
            if (dataObject != null)
            {
                dataObject.CreationTime = _renderer.GetTime() / _loopDuration; // Store normalized creation time
            }

            return dataObject;
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
                // Update window title with current date
                if (_currentDisplayedDate != DateTime.MinValue)
                {
                    _window.Title = $"MAELSTROM ! - Feed - {_currentDisplayedDate:yyyy-MM-dd HH:mm:ss} - FPS: {fps:F1}";
                }
                else
                {
                    _window.Title = "MAELSTROM ! - Feed - Loading...";
                }

                // Reset counters
                _frameCount = 0;
                _fpsTimer = 0.0;
            }
        }


        private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            switch (key)
            {
                case Key.Escape:
                    _window.Close();
                    break;
                case Key.F:
                    // Toggle fullscreen
                    if (_window.WindowState == WindowState.Fullscreen)
                    {
                        _window.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        _window.WindowState = WindowState.Fullscreen;
                    }
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
