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
    
    // Create border effect
    float borderWidth = 0.02;
    float border = 0.0;
    
    // Check if we're near any edge
    if (uv.x < borderWidth || uv.x > 1.0 - borderWidth || 
        uv.y < borderWidth || uv.y > 1.0 - borderWidth) {
        border = 1.0;
    }
    
    // Mix base color with white border
    vec3 finalColor = mix(baseColor, vec3(1.0), border);

    FragColor = vec4(finalColor, 1.0);
}
