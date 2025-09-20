using System.Numerics;

namespace DataViz.Configuration
{
    /// <summary>
    /// Configuration settings for the caustics visualization
    /// Easy to adjust visual parameters
    /// </summary>
    public static class VisualizationConfig
    {
        // World bounds
        public static Vector2 WorldBounds = new Vector2(20.0f, 15.0f);

        // Window settings
        public static int WindowWidth = 800;
        public static int WindowHeight = 600;
        public static int WindowSpacing = 20; // Space between windows

        // Visual parameters
        public static class Visuals
        {
            // Caustics intensity
            public static float CausticsIntensity = 0.8f;
            public static float CausticsBlueTint = 1.0f;

            // Wall effects
            public static float WallIntensity = 0.5f;
            public static float WallFalloff = 2.0f;

            // Water depth effect
            public static float DepthVariation = 0.3f;
            public static float DepthBase = 0.7f;

            // Ambient lighting
            public static Vector3 AmbientColor = new Vector3(0.1f, 0.15f, 0.2f);

            // Default water color
            public static Vector3 DefaultWaterColor = new Vector3(0.1f, 0.3f, 0.6f);
        }

        // Animation parameters
        public static class Animation
        {
            public static float TimeScale = 1.0f;
            public static float WaveSpeed = 0.5f;
            public static float AgitationSpeed = 0.8f;
            public static float ColorVariationSpeed = 0.8f;
        }

        // Point generation parameters
        public static class PointGeneration
        {
            public static int DefaultGridRows = 6;
            public static int DefaultGridCols = 8;
            public static Vector2 DefaultGridSpacing = new Vector2(2.5f, 2.0f);
            public static int DefaultRandomPoints = 10;

            // Property ranges
            public static float MinWallWidth = 0.1f;
            public static float MaxWallWidth = 0.4f;
            public static float MinAgitation = 0.3f;
            public static float MaxAgitation = 1.0f;
        }

        // Shader parameters
        public static class Shader
        {
            public static int MaxPoints = 64;
            public static int NoiseOctaves = 4;
            public static float NoiseAmplitude = 0.5f;
        }
    }
}
