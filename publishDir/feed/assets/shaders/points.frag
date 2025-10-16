#version 330 core
out vec4 FragColor;

// Receive per-point data from vertex shader
in float vMaelstrom;
in float vAmplitudeA;
in float vAmplitudeB;

// Global uniforms
uniform float iTime;
uniform vec2 iResolution;
uniform float iSeed;
uniform float iMaelstrom;
uniform float iAmplitudeA;
uniform float iAmplitudeB;

void main()
{
    // Interpolate from white to blue based on maelstrom value
    vec3 whiteColor = vec3(1.0, 1.0, 1.0);
    vec3 blueColor = vec3(0.0, 0.0, 1.0);
    
    // Use per-point maelstrom value for color interpolation
    vec3 color = mix(whiteColor, blueColor, vMaelstrom);
    
    // Add some variation based on amplitude values
    float intensity = 0.8 + 0.2 * sin(vAmplitudeA * 3.14159 + iTime);
    color *= intensity;
    
    FragColor = vec4(color, 1.0);
}
