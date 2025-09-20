using System.Numerics;

namespace DataViz.Core
{
    /// <summary>
    /// Customizable shader properties for each point
    /// Override or extend this to add your own properties
    /// </summary>
    public class ShaderProperties
    {
        // Basic properties (A, B, C)
        public float WallWidth { get; set; } = 0.2f;        // Property A
        public float Agitation { get; set; } = 0.5f;         // Property B
        public Vector3 Color { get; set; } = Vector3.One;    // Property C

        // Extended properties for customization
        public float WallIntensity { get; set; } = 1.0f;
        public float WallFalloff { get; set; } = 2.0f;
        public float CausticIntensity { get; set; } = 1.0f;
        public float CausticSpeed { get; set; } = 1.0f;
        public float CausticScale { get; set; } = 1.0f;
        public Vector3 AmbientColor { get; set; } = Vector3.Zero;
        public float Opacity { get; set; } = 1.0f;
        public float GlowIntensity { get; set; } = 0.0f;
        public Vector3 GlowColor { get; set; } = Vector3.One;

        // Animation properties
        public float AnimationSpeed { get; set; } = 1.0f;
        public float AnimationPhase { get; set; } = 0.0f;
        public Vector2 AnimationDirection { get; set; } = Vector2.One;

        // Noise properties
        public float NoiseScale { get; set; } = 1.0f;
        public float NoiseIntensity { get; set; } = 1.0f;
        public float NoiseSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Create a copy of these properties
        /// </summary>
        public ShaderProperties Clone()
        {
            return new ShaderProperties
            {
                WallWidth = WallWidth,
                Agitation = Agitation,
                Color = Color,
                WallIntensity = WallIntensity,
                WallFalloff = WallFalloff,
                CausticIntensity = CausticIntensity,
                CausticSpeed = CausticSpeed,
                CausticScale = CausticScale,
                AmbientColor = AmbientColor,
                Opacity = Opacity,
                GlowIntensity = GlowIntensity,
                GlowColor = GlowColor,
                AnimationSpeed = AnimationSpeed,
                AnimationPhase = AnimationPhase,
                AnimationDirection = AnimationDirection,
                NoiseScale = NoiseScale,
                NoiseIntensity = NoiseIntensity,
                NoiseSpeed = NoiseSpeed
            };
        }

        /// <summary>
        /// Apply time-based animation to properties
        /// Override this to create custom animated properties
        /// </summary>
        public virtual ShaderProperties Animate(float time)
        {
            var animated = Clone();

            // Example: Animate color based on time
            float colorPhase = time * AnimationSpeed + AnimationPhase;
            animated.Color = new Vector3(
                0.5f + 0.5f * MathF.Sin(colorPhase),
                0.5f + 0.5f * MathF.Sin(colorPhase + 2.0f),
                0.5f + 0.5f * MathF.Sin(colorPhase + 4.0f)
            );

            // Example: Animate agitation
            animated.Agitation = Agitation * (1.0f + 0.3f * MathF.Sin(time * AnimationSpeed * 2.0f));

            return animated;
        }
    }

    /// <summary>
    /// Example: Pulsing properties that change over time
    /// </summary>
    public class PulsingShaderProperties : ShaderProperties
    {
        public float PulseSpeed { get; set; } = 2.0f;
        public float PulseIntensity { get; set; } = 0.5f;

        public override ShaderProperties Animate(float time)
        {
            var animated = base.Animate(time);

            float pulse = 1.0f + PulseIntensity * MathF.Sin(time * PulseSpeed);
            animated.WallWidth *= pulse;
            animated.CausticIntensity *= pulse;
            animated.GlowIntensity *= pulse;

            return animated;
        }
    }

    /// <summary>
    /// Example: Color-cycling properties
    /// </summary>
    public class ColorCyclingShaderProperties : ShaderProperties
    {
        public float ColorSpeed { get; set; } = 1.0f;
        public Vector3[] ColorPalette { get; set; } = {
            new Vector3(1.0f, 0.0f, 0.0f), // Red
            new Vector3(0.0f, 1.0f, 0.0f), // Green
            new Vector3(0.0f, 0.0f, 1.0f), // Blue
            new Vector3(1.0f, 1.0f, 0.0f), // Yellow
            new Vector3(1.0f, 0.0f, 1.0f), // Magenta
            new Vector3(0.0f, 1.0f, 1.0f)  // Cyan
        };

        public override ShaderProperties Animate(float time)
        {
            var animated = base.Animate(time);

            float colorIndex = (time * ColorSpeed) % ColorPalette.Length;
            int index1 = (int)colorIndex % ColorPalette.Length;
            int index2 = (index1 + 1) % ColorPalette.Length;
            float t = colorIndex - MathF.Floor(colorIndex);

            animated.Color = Vector3.Lerp(ColorPalette[index1], ColorPalette[index2], t);

            return animated;
        }
    }
}
