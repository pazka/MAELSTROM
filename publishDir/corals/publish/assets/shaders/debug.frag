#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    float limit = 0.01;
    float d = distance(uv, vec2(.5, .5));
    
    // Check if we're near the edge of the fragment (uv coordinates from 0 to 1)
    float edgeThreshold = 0.02;
    bool nearEdge = uv.x < edgeThreshold || uv.x > abs(1.0 - edgeThreshold) || 
                    uv.y < edgeThreshold || uv.y > abs(1.0 - edgeThreshold);
    
    // Create the base gradient color
    vec3 baseColor = vec3(mix(0.5, 0.0, d));
    
    // If near edge, make it white, otherwise use base color
    vec3 finalColor = nearEdge ? vec3(1.0) : baseColor;

    FragColor = vec4(finalColor, 1.0);
}
