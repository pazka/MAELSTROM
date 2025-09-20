#version 330 core
out vec4 FragColor;
in vec2 uv;
in vec2 worldPos;

uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iWorldBounds;
uniform int iPointCount;

// Point data structure
struct CausticPoint {
    vec2 position;
    float wallWidth;    // Property A
    float agitation;    // Property B
    vec3 color;         // Property C
};

// Maximum number of points (adjust based on your needs)
#define MAX_POINTS 64
uniform CausticPoint iPoints[MAX_POINTS];

// Noise functions for water-like effects
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    vec2 u = f*f*(3.0-2.0*f);
    return mix(a, b, u.x) +
           (c - a)*u.y*(1.0 - u.x) +
           (d - b)*u.x*u.y;
}

float fbm(vec2 p) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for(int i = 0; i < 4; i++) {
        value += amplitude * noise(p * frequency);
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    return value;
}

// Calculate wall effect around a point (Property A)
float calculateWallEffect(vec2 pos, vec2 pointPos, float wallWidth) {
    float distance = length(pos - pointPos);
    
    if (distance < wallWidth) {
        // Inside the wall - high intensity
        return 1.0 - (distance / wallWidth) * 0.3;
    } else if (distance < wallWidth * 2.0) {
        // Wall edge - gradual falloff
        float normalizedDistance = (distance - wallWidth) / wallWidth;
        return 0.7 * (1.0 - normalizedDistance);
    }
    
    return 0.0;
}

// Calculate agitation effect (Property B)
float calculateAgitationEffect(vec2 pos, vec2 pointPos, float agitation, float time) {
    // Create wave-like agitation based on the point's agitation property
    float wave1 = sin((pos.x + time * 2.0) * 2.0 * agitation) * 0.5 + 0.5;
    float wave2 = sin((pos.y + time * 1.5) * 1.5 * agitation) * 0.5 + 0.5;
    float wave3 = sin((distance(pos, pointPos) * 3.0 - time * 3.0) * agitation) * 0.5 + 0.5;
    
    return (wave1 + wave2 + wave3) / 3.0;
}

// Calculate cell boundaries for wall rendering
float calculateCellBoundary(vec2 pos) {
    float boundary = 0.0;
    
    for(int i = 0; i < iPointCount && i < MAX_POINTS; i++) {
        for(int j = i + 1; j < iPointCount && j < MAX_POINTS; j++) {
            vec2 p1 = iPoints[i].position;
            vec2 p2 = iPoints[j].position;
            
            // Calculate perpendicular bisector
            vec2 midpoint = (p1 + p2) * 0.5;
            vec2 direction = normalize(p2 - p1);
            vec2 perpendicular = vec2(-direction.y, direction.x);
            
            // Distance to the boundary line
            float distanceToBoundary = abs(dot(pos - midpoint, perpendicular));
            
            // Wall thickness based on both points' wall widths
            float wallThickness = (iPoints[i].wallWidth + iPoints[j].wallWidth) * 0.5;
            
            if (distanceToBoundary < wallThickness) {
                float wallIntensity = 1.0 - (distanceToBoundary / wallThickness);
                boundary = max(boundary, wallIntensity);
            }
        }
    }
    
    return boundary;
}

// Calculate water caustics effect
float calculateCaustics(vec2 pos, float time) {
    float caustics = 0.0;
    
    for(int i = 0; i < iPointCount && i < MAX_POINTS; i++) {
        vec2 pointPos = iPoints[i].position;
        float agitation = iPoints[i].agitation;
        
        // Distance-based influence
        float distance = length(pos - pointPos);
        float influence = exp(-distance / (iPoints[i].wallWidth * 2.0));
        
        if (influence > 0.01) {
            // Wall effect
            float wallEffect = calculateWallEffect(pos, pointPos, iPoints[i].wallWidth);
            
            // Agitation effect
            float agitationEffect = calculateAgitationEffect(pos, pointPos, agitation, time);
            
            // Water-like caustics using multiple octaves
            vec2 causticPos = (pos - pointPos) * 0.3 + time * 0.5;
            float causticNoise = fbm(causticPos * 1.5 + time * 1.0) * agitation;
            causticNoise += fbm(causticPos * 3.0 + time * 1.5) * agitation * 0.5;
            causticNoise += fbm(causticPos * 6.0 + time * 2.0) * agitation * 0.25;
            
            // Combine effects
            float pointCaustics = (wallEffect + agitationEffect * 0.5) * causticNoise * influence;
            caustics += pointCaustics;
        }
    }
    
    return clamp(caustics, 0.0, 1.0);
}

// Calculate color based on nearby points (Property C)
vec3 calculateColor(vec2 pos, float time) {
    vec3 totalColor = vec3(0.0);
    float totalWeight = 0.0;
    
    for(int i = 0; i < iPointCount && i < MAX_POINTS; i++) {
        vec2 pointPos = iPoints[i].position;
        float distance = length(pos - pointPos);
        float influence = exp(-distance / (iPoints[i].wallWidth * 1.5));
        
        if (influence > 0.01) {
            // Add time-based color variation
            vec3 baseColor = iPoints[i].color;
            float timeVariation = sin(time * 0.8 + iPoints[i].agitation * 5.0) * 0.1 * iPoints[i].agitation;
            vec3 timeColor = baseColor + vec3(timeVariation, timeVariation * 0.5, timeVariation * 0.3);
            
            totalColor += timeColor * influence;
            totalWeight += influence;
        }
    }
    
    if (totalWeight > 0.0) {
        return totalColor / totalWeight;
    }
    
    // Default water color
    return vec3(0.1, 0.3, 0.6);
}

void main()
{
    vec2 pos = worldPos;
    
    // Calculate caustics intensity
    float caustics = calculateCaustics(pos, iTime);
    
    // Calculate cell boundaries
    float boundaries = calculateCellBoundary(pos);
    
    // Calculate base color
    vec3 baseColor = calculateColor(pos, iTime);
    
    // Add water-like depth effect
    float depth = fbm(pos * 0.2 + iTime * 0.3) * 0.4 + 0.6;
    
    // Combine all effects
    vec3 finalColor = baseColor * depth;
    finalColor += vec3(caustics * 0.8, caustics * 0.9, caustics * 1.0); // Blue-tinted caustics
    finalColor += vec3(boundaries * 0.5); // White boundaries
    
    // Add some ambient lighting
    finalColor += vec3(0.1, 0.15, 0.2);
    
    // Clamp and output
    finalColor = clamp(finalColor, 0.0, 1.0);
    FragColor = vec4(finalColor, 1.0);
}
