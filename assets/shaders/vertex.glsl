#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 uv;

// Uniforms for object transformation
uniform vec2 uObjectPosition;  // World position of this object
uniform float iTime;           // Time for animations

void main()
{
    // Transform the vertex position by the object's world position
    vec2 worldPos = aPos + uObjectPosition;
    
    // Pass UV coordinates to fragment shader
    uv = aTexCoord;
    
    // Set final position (for 2D, we keep z=0 and w=1)
    gl_Position = vec4(worldPos, 0.0, 1.0);
}
