#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    // Center the UV coordinates
    vec2 centeredUV = uv - 0.5;
    
    // Create rounded rectangle mask
    float cornerRadius = 0.15;
    float roundedMask = 1.0;
    
    // Check distance to corners for rounding
    vec2 cornerDist = abs(centeredUV) - (0.5 - cornerRadius);
    if (cornerDist.x > 0.0 && cornerDist.y > 0.0) {
        float cornerDistance = length(cornerDist);
        roundedMask = 1.0 - smoothstep(0.0, cornerRadius * 0.1, cornerDistance);
    }
    
    // Create gradient border effect
    float borderWidth = 0.2;
    float distanceToEdge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
    float borderGradient = smoothstep(0.0, borderWidth, distanceToEdge);
    
    // Main color (blue gradient from center)
    float d = distance(uv, vec2(0.5, 0.5));
    
    // Border color (white gradient)
    vec3 borderColor = vec3(1.0);
    
    // Mix main color with border based on gradient
    vec3 finalColor = mix(borderColor, vec3(0), borderGradient);
    
    // Apply rounded corners using alpha
    float alpha = roundedMask * (1.0 - borderGradient * 0.3); // Slight transparency for border

    FragColor = vec4(finalColor, alpha);
}
