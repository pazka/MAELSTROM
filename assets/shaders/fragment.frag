#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    // Define the border area (edge of the fragment from -1 to 1)
    float borderWidth = 0.05;
    
    // Calculate distance from edges
    float distFromLeft = uv.x - (-1.0);
    float distFromRight = 1.0 - uv.x;
    float distFromTop = uv.y - (-1.0);
    float distFromBottom = 1.0 - uv.y;
    
    // Find the closest edge
    float distToEdge = min(min(distFromLeft, distFromRight), min(distFromTop, distFromBottom));
    
    // Create border mask with smooth anti-aliasing
    float border = 1.0 - smoothstep(0.0, borderWidth, distToEdge);
    
    // Create a small circle at the center
    float centerRadius = 0.1;
    float distToCenter = distance(uv, vec2(0.0, 0.0));
    float centerCircle = 1.0 - smoothstep(centerRadius - 0.02, centerRadius + 0.02, distToCenter);
    
    // Combine border and center circle
    float finalMask = max(border, centerCircle);
    
    // Color the result (white)
    FragColor = vec4(1.0, 1.0, 1.0, finalMask);
}
