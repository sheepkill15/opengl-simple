#version 430 core
#define M_PI 3.14159265

struct Light {
    vec3 color;
    vec3 position;
    vec3 direction;
    float innerCutOff;
    float outerCutOff;
    float intensity;
    float attenuationFalloff;
};

layout(std430, binding = 3) readonly buffer Lights
{
    Light lights[];
};

uniform vec3 viewPos;

uniform vec3 AmbientColor;
uniform vec3 DiffuseColor;
uniform vec3 SpecularColor;
uniform float SpecularComponent;
uniform float Dissolve;

uniform sampler2D DiffuseTexture;
uniform sampler2D BumpTexture;
uniform sampler2D AlphaTexture;
uniform sampler2D SpecularTexture;
uniform sampler2D DecalTexture;

uniform sampler2D shadowMap;

out vec4 FragColor;

in vec4 outCol;
in vec3 outNormal;
in vec3 outWorldPosition;
in vec2 outTex;
in mat3 TBN;
in vec4 FragPosLightSpace;

float DistributionGGX(vec3 normal, vec3 viewVec, float roughness)
{
    float a2   = pow(roughness, 4);
    float NdotH  = max(dot(normal, viewVec), 0.0);

    float denom = (NdotH*NdotH * (a2 - 1.0) + 1.0);
    return a2 / (M_PI * denom * denom);
}

float GeometrySchlickGGX(float dotp, float roughness)
{

    return dotp / (dotp * (1.0 - roughness) + roughness);
}
float GeometrySmith(vec3 normal, vec3 viewVec, vec3 lightVec, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;
    return GeometrySchlickGGX(max(dot(normal, viewVec), 0.0), k) * GeometrySchlickGGX(max(dot(normal, lightVec), 0.0), k);
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 calculateLightContribution(Light light, vec3 V, vec3 albedo, float metallic, vec3 N, float roughness) {
    vec3 L = normalize(light.position - outWorldPosition);
    vec3 H = normalize(V + L);

    float distance    = length(light.position - outWorldPosition);
    float attenuation = 1.0 / (1.0 + light.attenuationFalloff * distance * distance);
    attenuation = clamp(attenuation, 0.0, 1.0);
    vec3 radiance     = light.color * (attenuation * light.intensity);
    vec3 F0 = vec3(0.04);
    F0      = mix(F0, albedo, metallic);
    vec3 F  = fresnelSchlick(max(dot(H, V), 0.0), F0);
    float NDF = DistributionGGX(H, N, roughness);
    float G   = GeometrySmith(N, V, L, roughness);

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0)  + 0.0001;
    vec3 specular     = numerator / denominator;

    vec3 kD = mix(vec3(1)-F, vec3(0), metallic);
    float NdotL = max(dot(N, L), 0.0);
    vec3 Lo = (kD * albedo / M_PI + specular) * radiance * NdotL;
    float theta = dot(L, normalize(-light.direction));
    float epsilon   = light.innerCutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    return Lo * intensity;
}

float LinearizeDepth(float depth)
{
    float near_plane = 0.1;
    float far_plane = 10;
    float z = depth * 2.0 - 1.0; // Back to NDC 
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = (texture(shadowMap, projCoords.xy).r);
    float currentDepth = projCoords.z;
    // Bias to avoid shadow acne
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);

    // Simple shadow calculation
    float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
    if(projCoords.z > 1.0)
        shadow = 0.0;

    return shadow;
}

void main()
{
//    vec3 projCoords = FragPosLightSpace.xyz / FragPosLightSpace.w;
//    projCoords = projCoords * 0.5 + 0.5;
//    float closestDepth = LinearizeDepth(texture(shadowMap, projCoords.xy).r);
//    FragColor = vec4(projCoords, 1.0);
//    return;
//    float depth = texture(shadowMap, outTex).r;
//    FragColor = vec4(vec3(closestDepth), 1.0);
//    return;
    vec3 texNormal = normalize(texture(BumpTexture, outTex).rgb);
    texNormal = normalize(texNormal * 2.0 - 1.0);
    texNormal = normalize(TBN * texNormal);
    vec3 albedo = pow(texture(DiffuseTexture, outTex).rgb, vec3(2.2));
    float metallic = texture(DecalTexture, outTex).r;
    float roughness = max(texture(SpecularTexture, outTex).r, SpecularComponent / 1000);
    vec3 N = texNormal;
    float alpha = texture(AlphaTexture, outTex).r + Dissolve;
    vec3 V = normalize(outWorldPosition - viewPos);
    vec3 Lo = vec3(0.0);
    for(int i = 0; i < lights.length(); i++) {
        Lo += calculateLightContribution(lights[i], V, albedo, metallic, N, roughness);
        vec3 L = normalize(lights[i].position - outWorldPosition);
        float shadow = ShadowCalculation(FragPosLightSpace, N, L);
        Lo = (1 - shadow) * Lo;
    }
    vec3 ambient = AmbientColor * albedo * 0.03;
    vec3 color   = ambient + Lo;
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));
    
    FragColor = vec4(color, outCol.a * alpha);
}