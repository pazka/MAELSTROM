#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 uv;

void main() 
{    
    // Pass UV coordinates to fragment shader
    uv = aTexCoord ;
    
    // Set final position (full-screen quad)
    gl_Position = vec4(aPos, 0.0, 1.0);
}
