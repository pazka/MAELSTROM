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
        amplitude *= 0.5;
        frequency *= 2.0;
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

// Enhanced edge detection with organic randomness
float edgeDetection(vec2 uv) {
    float edge = 0.0;
    
    // Check distance to edges
    float distToEdge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
    
    // Create organic edge mask with noise-based randomness
    float edgeNoise = perlinNoise(uv * 8.0 + iTime * 0.3);
    float organicEdgeThreshold = 0.08 + edgeNoise * 0.04; // Vary edge threshold
    
    if (distToEdge < organicEdgeThreshold) {
        float smoothEdge = 1.0 - smoothstep(0.0, organicEdgeThreshold, distToEdge);
        // Add noise-based variation to edge strength
        edge = smoothEdge * (0.7 + edgeNoise * 0.6);
    }
    
    // Add corner effects for more dramatic water distortion
    vec2 cornerDist = vec2(
        min(uv.x, 1.0 - uv.x),
        min(uv.y, 1.0 - uv.y)
    );
    float cornerEffect = 1.0 - smoothstep(0.0, 0.15, min(cornerDist.x, cornerDist.y));
    edge += cornerEffect * 0.3;
    
    return clamp(edge, 0.0, 1.0);
}

void main()
{
    vec2 texCoord = uv;
    
    // Detect edges for stronger water effects
    float edgeMask = edgeDetection(texCoord);
    
    // Calculate water-like distortion with multiple layers
    vec2 waterDist = waterDistortion(texCoord, iTime);
    
    // Enhanced distortion strength with more pronounced water effects
    float baseDistortion = 0.025;
    float edgeDistortion = 0.045;
    float distortionStrength = baseDistortion + edgeMask * edgeDistortion;
    
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
    
    // Sample the distorted texture
    vec4 color = texture(screenTexture, distortedCoord);
    
    // Enhanced chromatic aberration with water-like dispersion
    float aberration = 0.005 + edgeMask * 0.008;
    vec2 aberrationOffset = vec2(aberration, 0.0);
    
    // Add water-like color separation with distortion
    vec4 r = texture(screenTexture, clamp(distortedCoord + aberrationOffset, 0.0, 1.0));
    vec4 g = texture(screenTexture, distortedCoord);
    vec4 b = texture(screenTexture, clamp(distortedCoord - aberrationOffset, 0.0, 1.0));
    
    // Mix original color with chromatic aberration
    vec4 chromaticColor = vec4(r.r, g.g, b.b, color.a);
    color = mix(color, chromaticColor, 0.7 + edgeMask * 0.4);
    
    // Enhanced water-like caustics effect with multiple layers
    float caustics1 = perlinNoise(texCoord * 8.0 + iTime * 0.6);
    float caustics2 = perlinNoise(texCoord * 15.0 + iTime * 1.2);
    float caustics3 = perlinNoise(texCoord * 25.0 + iTime * 1.8);
    
    caustics1 = smoothstep(0.2, 0.8, caustics1);
    caustics2 = smoothstep(0.3, 0.7, caustics2);
    caustics3 = smoothstep(0.4, 0.6, caustics3);
    
    float totalCaustics = (caustics1 * 0.5 + caustics2 * 0.3 + caustics3 * 0.2);
    color.rgb += totalCaustics * 0.15 * edgeMask;
    
    // Add water surface shimmer
    float shimmer = perlinNoise(texCoord * 30.0 + iTime * 2.0);
    shimmer = smoothstep(0.4, 0.6, shimmer);
    color.rgb += shimmer * 0.08 * edgeMask;
    
    // Enhanced vignette with water-like falloff and organic variation
    float centerDistance = distance(texCoord, vec2(0.5));
    float vignetteNoise = perlinNoise(texCoord * 4.0 + iTime * 0.2);
    float organicVignette = centerDistance * (0.8 + vignetteNoise * 0.4);
    float vignette = 1.0 - organicVignette;
    vignette = smoothstep(0.0, 1.0, vignette);
    
    // Enhanced water-like edge darkening with organic variation
    float edgeDarkeningNoise = perlinNoise(texCoord * 6.0 + iTime * 0.4);
    float edgeDarkening = 1.0 - edgeMask * (0.25 + edgeDarkeningNoise * 0.15);
    
    // Add water depth effect (darker towards edges)
    float depthEffect = 1.0 - edgeMask * 0.1;
    
    color *= vignette * edgeDarkening * depthEffect;
    
    FragColor = color;
}
