#version 330 core
layout(location = 0) in vec2 aPosition;

uniform float iTime;
uniform vec2 uScreenSize;

void main() 
{    
    // Set point position directly (already normalized to -1 to 1)
    gl_Position = vec4(aPosition, 0.0, 1.0);
    
    // Set point size (optional - can be controlled by gl_PointSize)
    gl_PointSize = 1.0;
}
