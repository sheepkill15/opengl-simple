#version 330 core
        
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform float shininess;

uniform vec3 AmbientColor;
uniform vec3 DiffuseColor;
uniform vec3 SpecularColor;
uniform float SpecularComponent;

uniform sampler2D DiffuseTexture;

out vec4 FragColor;

in vec4 outCol;
in vec3 outNormal;
in vec3 outWorldPosition;
in vec2 outTex;

void main()
{
    vec3 ambient = AmbientColor * lightColor;

    vec3 norm = normalize(outNormal);
    vec3 lightDir = normalize(lightPos - outWorldPosition);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor * DiffuseColor;

    vec3 viewDir = normalize(viewPos - outWorldPosition);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), SpecularComponent) / max(dot(norm,viewDir), -dot(norm,lightDir));
    vec3 specular = SpecularColor * spec * lightColor;  

    vec4 textColor = texture(DiffuseTexture, outTex);
    vec4 result = vec4((ambient + diffuse + specular) * (outCol.xyz * textColor.xyz), textColor.w * outCol.w);
    

    FragColor = result;
}