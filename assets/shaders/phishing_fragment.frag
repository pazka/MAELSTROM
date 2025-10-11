#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    // Center the UV coordinates
    vec2 centeredUV = uv - 0.5;
    
    // Calculate distance from center
    float distance = length(centeredUV);
    
    // Create circle mask with smooth edges
    float circleMask = 1.0 - smoothstep(0.4, 0.5, distance);
    
    // Create animated pulsing effect
    float pulse = 0.8 + 0.2 * sin(iTime * 3.0);
    
    // Create gradient from center to edge
    float gradient = 1.0 - smoothstep(0.0, 0.5, distance);
    
    // Main color (bright blue/cyan)
    vec3 baseColor = vec3(0.2, 0.6, 1.0);
    
    // Add some color variation based on time
    baseColor.r += 0.1 * sin(iTime * 2.0);
    baseColor.g += 0.1 * cos(iTime * 1.5);
    
    // Apply gradient and pulse
    vec3 finalColor = baseColor * gradient * pulse;
    
    // Add a bright center
    float centerGlow = 1.0 - smoothstep(0.0, 0.1, distance);
    finalColor = mix(finalColor, vec3(1.0), centerGlow * 0.5);
    
    // Apply circle mask for alpha
    float alpha = circleMask;
    
    FragColor = vec4(finalColor, alpha);
}
