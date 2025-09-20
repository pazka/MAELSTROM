#version 330 core
layout(location = 0) in vec2 aPos;
out vec2 uv;
out vec2 worldPos;

uniform vec2 iResolution;
uniform vec2 iWorldBounds;

void main()
{
    // Normalize to [0,1] for texture coordinates
    uv = (aPos + 1.0) * 0.5;
    
    // Convert to world coordinates for caustics calculation
    worldPos = aPos * iWorldBounds * 0.5;
    
    // Full screen rendering
    gl_Position = vec4(aPos, 0.0, 1.0);
}
