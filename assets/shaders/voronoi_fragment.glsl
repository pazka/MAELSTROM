#version 330 core
out vec4 FragColor;
in vec2 uv;

uniform vec2 uResolution;
uniform float iTime;

float distToLine(vec2 pt1, vec2 pt2, vec2 testPt)
{
  vec2 lineDir = pt2 - pt1;
  vec2 perpDir = vec2(lineDir.y, -lineDir.x);
  vec2 dirToPt1 = pt1 - testPt;
  return pow(abs(dot(normalize(perpDir), dirToPt1)),10);
}

void main()
{
    float dist1 = distToLine(vec2(0.0,0.0),vec2(0.0,1.0),uv);
    float dist2 = distToLine(vec2(1.0,0.0),vec2(1.0,1.0),uv);
    float dist3 = distToLine(vec2(0.0,0.0),vec2(1.0,0.0),uv);
    float dist4 = distToLine(vec2(0.0,1.0),vec2(1.0,1.0),uv);

    FragColor = vec4(dist1,dist2,dist3,1.0);
}
