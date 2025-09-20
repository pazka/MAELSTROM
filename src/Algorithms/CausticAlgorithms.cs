using System.Numerics;
using DataViz.Data;

namespace DataViz.Algorithms
{
    /// <summary>
    /// Procedural generation algorithms for caustics visualization
    /// Separated from display logic for better maintainability
    /// </summary>
    public static class CausticAlgorithms
    {
        /// <summary>
        /// Calculate the caustic intensity at a given position based on nearby points
        /// </summary>
        public static float CalculateCausticIntensity(Vector2 position, IReadOnlyList<CausticPoint> points, float time)
        {
            float totalIntensity = 0.0f;

            foreach (var point in points)
            {
                float distance = Vector2.Distance(position, point.Position);

                // Calculate wall effect (Property A)
                float wallEffect = CalculateWallEffect(distance, point.WallWidth);

                // Calculate agitation effect (Property B)
                float agitationEffect = CalculateAgitationEffect(position, point, time);

                // Combine effects
                float pointIntensity = wallEffect * agitationEffect;
                totalIntensity += pointIntensity;
            }

            return Math.Clamp(totalIntensity, 0.0f, 1.0f);
        }

        /// <summary>
        /// Calculate wall effect around a point (Property A)
        /// </summary>
        private static float CalculateWallEffect(float distance, float wallWidth)
        {
            if (distance < wallWidth)
            {
                // Inside the wall - high intensity
                return 1.0f - (distance / wallWidth) * 0.3f;
            }
            else if (distance < wallWidth * 2.0f)
            {
                // Wall edge - gradual falloff
                float normalizedDistance = (distance - wallWidth) / wallWidth;
                return 0.7f * (1.0f - normalizedDistance);
            }

            return 0.0f;
        }

        /// <summary>
        /// Calculate agitation effect (Property B)
        /// </summary>
        private static float CalculateAgitationEffect(Vector2 position, CausticPoint point, float time)
        {
            // Create wave-like agitation based on the point's agitation property
            float wave1 = MathF.Sin((position.X + time * 0.5f) * 3.0f * point.Agitation) * 0.5f + 0.5f;
            float wave2 = MathF.Sin((position.Y + time * 0.3f) * 2.0f * point.Agitation) * 0.5f + 0.5f;
            float wave3 = MathF.Sin((Vector2.Distance(position, point.Position) * 4.0f - time * 2.0f) * point.Agitation) * 0.5f + 0.5f;

            return (wave1 + wave2 + wave3) / 3.0f;
        }

        /// <summary>
        /// Calculate the color at a given position based on nearby points
        /// </summary>
        public static Vector3 CalculateColor(Vector2 position, IReadOnlyList<CausticPoint> points, float time)
        {
            Vector3 totalColor = Vector3.Zero;
            float totalWeight = 0.0f;

            foreach (var point in points)
            {
                float distance = Vector2.Distance(position, point.Position);
                float influence = CalculateInfluence(distance, point.WallWidth);

                if (influence > 0.0f)
                {
                    // Add time-based color variation
                    Vector3 timeColor = AddTimeVariation(point.Color, time, point.Agitation);
                    totalColor += timeColor * influence;
                    totalWeight += influence;
                }
            }

            if (totalWeight > 0.0f)
            {
                return totalColor / totalWeight;
            }

            // Default water color
            return new Vector3(0.1f, 0.3f, 0.6f);
        }

        /// <summary>
        /// Calculate influence of a point at a given distance
        /// </summary>
        private static float CalculateInfluence(float distance, float wallWidth)
        {
            if (distance < wallWidth * 2.0f)
            {
                return MathF.Exp(-distance / (wallWidth * 0.5f));
            }
            return 0.0f;
        }

        /// <summary>
        /// Add time-based color variation
        /// </summary>
        private static Vector3 AddTimeVariation(Vector3 baseColor, float time, float agitation)
        {
            float variation = MathF.Sin(time * 0.8f + agitation * 5.0f) * 0.1f * agitation;
            return baseColor + new Vector3(variation, variation * 0.5f, variation * 0.3f);
        }

        /// <summary>
        /// Generate Voronoi-like cell boundaries for wall effects
        /// </summary>
        public static float CalculateVoronoiDistance(Vector2 position, IReadOnlyList<CausticPoint> points)
        {
            float minDistance = float.MaxValue;

            foreach (var point in points)
            {
                float distance = Vector2.Distance(position, point.Position);
                minDistance = MathF.Min(minDistance, distance);
            }

            return minDistance;
        }

        /// <summary>
        /// Calculate cell boundaries for wall rendering
        /// </summary>
        public static float CalculateCellBoundary(Vector2 position, IReadOnlyList<CausticPoint> points, float time)
        {
            float boundary = 0.0f;

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    var p1 = points[i];
                    var p2 = points[j];

                    // Calculate perpendicular bisector
                    Vector2 midpoint = (p1.Position + p2.Position) * 0.5f;
                    Vector2 direction = Vector2.Normalize(p2.Position - p1.Position);
                    Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

                    // Distance to the boundary line
                    float distanceToBoundary = MathF.Abs(Vector2.Dot(position - midpoint, perpendicular));

                    // Wall thickness based on both points' wall widths
                    float wallThickness = (p1.WallWidth + p2.WallWidth) * 0.5f;

                    if (distanceToBoundary < wallThickness)
                    {
                        float wallIntensity = 1.0f - (distanceToBoundary / wallThickness);
                        boundary = MathF.Max(boundary, wallIntensity);
                    }
                }
            }

            return boundary;
        }
    }
}
