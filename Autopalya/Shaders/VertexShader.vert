#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec4 vCol;
layout (location = 2) in vec3 vNorm;
layout (location = 3) in vec2 vTex;
layout (location = 4) in vec3 tangent;

uniform mat4 uModel;
uniform mat3 uNormal;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 lightView;
uniform mat4 lightProj;

uniform vec2 TextureScale;

out vec4 outCol;
out vec3 outNormal;
out vec3 outWorldPosition;
out vec2 outTex;
out mat3 TBN;
out vec4 FragPosLightSpace;


void main()
{
    outCol = vCol;
    outTex = TextureScale * vTex;
    outWorldPosition = vec3(uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0));
    gl_Position = uProjection*uView*vec4(outWorldPosition, 1.0);
    outNormal = uNormal*vNorm;
    FragPosLightSpace = lightProj * lightView * vec4(outWorldPosition, 1.0);
    vec3 T = normalize(vec3(uModel * vec4(tangent, 0.0)));
    vec3 N = normalize(vec3(uModel * vec4(outNormal, 0.0)));
    // re-orthogonalize T with respect to N
    T = normalize(T - dot(T, N) * N);
    // then retrieve perpendicular vector B with the cross product of T and N
    vec3 B = cross(N, T);

    TBN = mat3(T, B, N);
}