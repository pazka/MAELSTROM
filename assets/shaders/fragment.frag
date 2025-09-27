#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;

void main()
{
    float limit = 0.01;
    float d = distance(uv, vec2(0, 0));

    FragColor = vec4(mix(0.5, 0.0, d), mix(0.5, 0.0, d), mix(0.5, 0.0, d), 1.0);
}
