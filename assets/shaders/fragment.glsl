#version 330 core
out vec4 FragColor;
in vec2 uv;
uniform float iTime;
uniform vec2 uObjectPosition;  // World position of this object

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    vec2 u = f*f*(3.0-2.0*f);
    return mix(a, b, u.x) +
           (c - a)*u.y*(1.0 - u.x) +
           (d - b)*u.x*u.y;
}

void main()
{
    vec2 p = uv * 10.0;
    float n = 0.0;
    for(int i=0; i<4; i++) {
        float fi = float(i);
        n += noise(p + iTime*0.2*fi) / (fi+1.0);
    }
    n = pow(n, 3.0);
    FragColor = vec4(vec3(n), 1.0);
}
