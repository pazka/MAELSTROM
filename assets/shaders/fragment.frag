#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    float limit = 0.01;
    float d = distance(uv, vec2(0.5, 0.5));
    
    
    // Create the base gradient color
    vec3 baseColor = vec3(0,0,smoothstep( 1,0, d));
    
    // If near edge, make it white, otherwise use base color

    FragColor = vec4(baseColor, 1.0);
}
