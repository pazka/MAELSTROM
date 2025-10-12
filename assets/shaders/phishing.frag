#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    vec2 center = vec2(0.5, 0.5);
    float radius = 0.5;

    float dist = distance(uv, center);
    float circle = smoothstep(radius, 0.0, dist); // smooth gradient from center to edge

    vec3 color = vec3(circle); // black (0) at center to white (1) at edge
    FragColor = vec4(vec3(1), circle);
}
