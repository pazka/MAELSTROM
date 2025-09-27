#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 uv;

// Uniforms for object transformation
uniform float iTime;           // Time for animations
 
void main() 
{    
    // Pass UV coordinates to fragment shader
    uv = aTexCoord * 2.0 - 1.0;
    
    // Set final position (for 2D, we keep z=0 and w=1)
    gl_Position = vec4(aPos, 0.0, 1.0);  
} 
