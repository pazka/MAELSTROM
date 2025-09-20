using DataViz;

namespace DataViz
{
    /// <summary>
    /// Main entry point for the Caustics Skeleton Application
    /// Customize the SkeletonApp class to create your own visualizations
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Caustics Visualization Skeleton ===");
            Console.WriteLine();
            Console.WriteLine("This is a customizable skeleton application for creating");
            Console.WriteLine("water-like caustics visualizations with moving points.");
            Console.WriteLine();
            Console.WriteLine("Key Features:");
            Console.WriteLine("- Customizable point movement patterns");
            Console.WriteLine("- Flexible shader properties per point");
            Console.WriteLine("- Neighbor detection for wall calculations");
            Console.WriteLine("- World position tracking");
            Console.WriteLine("- Easy-to-extend architecture");
            Console.WriteLine();
            Console.WriteLine("To customize:");
            Console.WriteLine("1. Modify the SetupPoints() method in SkeletonApp.cs");
            Console.WriteLine("2. Create custom PointController classes");
            Console.WriteLine("3. Create custom ShaderProperties classes");
            Console.WriteLine("4. Override the SkeletonApp class for advanced features");
            Console.WriteLine();

            try
            {
                var app = new SkeletonApp();
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}