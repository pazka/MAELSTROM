#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform sampler2D screenTexture;
uniform float iTime;
uniform vec2 screenSize;

// Enhanced Perlin noise functions for water-like effects
float noise(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

float smoothNoise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    
    float a = noise(i);
    float b = noise(i + vec2(1.0, 0.0));
    float c = noise(i + vec2(0.0, 1.0));
    float d = noise(i + vec2(1.0, 1.0));
    
    vec2 u = f * f * (3.0 - 2.0 * f);
    
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// Multi-octave Perlin noise for organic water effects
float perlinNoise(vec2 st) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < 4; i++) {
        value += amplitude * smoothNoise(st * frequency);
        amplitude *= 1;
        frequency *= 1.0;
    }
    
    return value;
}

// Enhanced water-like distortion with organic curves and flow patterns
vec2 waterDistortion(vec2 uv, float time) {
    // Large scale water movement with flow patterns
    vec2 flowDirection = vec2(
        perlinNoise(uv * 0.5 + time * 0.1),
        perlinNoise(uv * 0.5 + time * 0.1 + 50.0)
    );
    flowDirection = normalize(flowDirection - 0.5);
    
    vec2 distortion1 = vec2(
        perlinNoise(uv * 2.5 + time * 0.2 + flowDirection.x * 0.3),
        perlinNoise(uv * 2.5 + time * 0.2 + flowDirection.y * 0.3 + 100.0)
    );
    
    // Medium scale ripples with wave patterns
    vec2 waveCenter = vec2(0.5, 0.5);
    float waveDistance = distance(uv, waveCenter);
    vec2 waveDirection = normalize(uv - waveCenter);
    
    vec2 distortion2 = vec2(
        perlinNoise(uv * 6.0 + time * 0.5 + waveDirection.x * waveDistance * 2.0),
        perlinNoise(uv * 6.0 + time * 0.5 + waveDirection.y * waveDistance * 2.0 + 200.0)
    );
    
    // Small scale surface tension with organic curves
    vec2 distortion3 = vec2(
        perlinNoise(uv * 12.0 + time * 0.8 + sin(uv.x * 10.0 + time) * 0.2),
        perlinNoise(uv * 12.0 + time * 0.8 + cos(uv.y * 10.0 + time) * 0.2 + 300.0)
    );
    
    // Micro-scale turbulence for organic randomness
    vec2 distortion4 = vec2(
        perlinNoise(uv * 25.0 + time * 1.5),
        perlinNoise(uv * 25.0 + time * 1.5 + 400.0)
    );
    
    // Combine all layers with enhanced weights for more organic feel
    vec2 totalDistortion = distortion1 * 0.35 + distortion2 * 0.25 + distortion3 * 0.25 + distortion4 * 0.15;
    
    // Convert from 0-1 to -1 to 1 range
    return (totalDistortion - 0.5) * 2.0;
}


void main()
{
    vec2 texCoord = uv;
    
    // Calculate water-like distortion with multiple layers
    vec2 waterDist = waterDistortion(texCoord, iTime);
    
    // Enhanced distortion strength with more pronounced water effects
    float baseDistortion = 0.025;
    float edgeDistortion = 0.045;
    float distortionStrength = baseDistortion ;
    
    // Add time-based variation to distortion strength
    float timeVariation = 0.5 + 0.5 * sin(iTime * 0.7);
    distortionStrength *= timeVariation;
    
    vec2 distortion = waterDist * distortionStrength;
    
    // Add secondary distortion layer for more complex water movement
    vec2 secondaryDist = waterDistortion(texCoord * 1.3 + vec2(iTime * 0.1), iTime * 0.8);
    distortion += secondaryDist * 0.008;
    
    // Apply water distortion to texture coordinates
    vec2 distortedCoord = texCoord + distortion;
    distortedCoord = clamp(distortedCoord, 0.0, 1.0);
    
    // Enhanced multi-sample anti-aliasing for smoother edges
    vec4 color = vec4(0.0);
    float sampleCount = 1.0;
    vec2 sampleOffset = vec2(1.0) / screenSize * 0.5;
    
    // Multi-sample the texture for smoother edges
    for (float i = 0.0; i < sampleCount; i += 1.0) {
        float angle = (i / sampleCount) * 6.28318; // 2*PI
        vec2 offset = vec2(cos(angle), sin(angle)) * sampleOffset;
        color += texture(screenTexture, distortedCoord + offset);
    }
    color /= sampleCount;
    
    // Edge smoothing and anti-aliasing
    vec2 edgeSmoothOffset = vec2(1.0) / screenSize * 0.3;
    vec4 edgeSmooth = vec4(0.0);
    float edgeSampleCount = 1.0;

    
    // Enhanced water-like caustics effect with multiple layers
    float caustics1 = perlinNoise(texCoord * 8.0 + iTime * 0.6);
    float caustics2 = perlinNoise(texCoord * 15.0 + iTime * 1.2);
    float caustics3 = perlinNoise(texCoord * 25.0 + iTime * 1.8);
    
    caustics1 = smoothstep(0.2, 0.8, caustics1);
    caustics2 = smoothstep(0.3, 0.7, caustics2);
    caustics3 = smoothstep(0.4, 0.6, caustics3);
    
    float totalCaustics = (caustics1 * 0.5);
    color.rgb += totalCaustics * 0.15 ;
    
    // Add water surface shimmer
    float shimmer = perlinNoise(texCoord * 30.0 + iTime * 2.0);
    shimmer = smoothstep(0.4, 0.6, shimmer);
    color.rgb += shimmer * 0.08 ;
    
    FragColor = color;
}
