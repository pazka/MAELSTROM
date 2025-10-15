#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in float aMaelstrom;
layout(location = 2) in float aAmplitudeA;
layout(location = 3) in float aAmplitudeB;

uniform float iTime;
uniform vec2 iResolution;

// Pass per-point data to fragment shader
out float vMaelstrom;
out float vAmplitudeA;
out float vAmplitudeB;

void main() 
{    
    // Set point position directly (already normalized to -1 to 1)
    gl_Position = vec4(aPosition.x , aPosition.y  , 0.0, 1.0);
    
    // Set point size (optional - can be controlled by gl_PointSize)
    gl_PointSize = 10.0;
    
    // Pass per-point data to fragment shader
    vMaelstrom = aMaelstrom;
    vAmplitudeA = aAmplitudeA;
    vAmplitudeB = aAmplitudeB;
}
