#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 uv;

// Uniforms for object transformation
uniform float iTime;           // Time for animations
uniform mat4 uModel;           // Model transformation matrix

void main() 
{    
    // Pass UV coordinates to fragment shader
    uv = aTexCoord;
    
    // Apply model transformation to vertex position
    vec4 worldPos = uModel * vec4(aPos, 0.0, 1.0);
    
    // Set final position (for 2D, we keep z=0 and w=1)
    gl_Position = worldPos;
}
