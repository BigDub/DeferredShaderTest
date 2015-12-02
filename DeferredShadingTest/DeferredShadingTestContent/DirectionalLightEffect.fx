float3 lightDirection;
float3 Color;
float3 cameraPosition;
float4x4 InvertViewProjection; 
float2 halfPixel;

Texture DiffuseMap;
sampler DiffuseMapSampler = sampler_state { 
	texture = <DiffuseMap>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = mirror; 
	AddressV = mirror;};
Texture NormalMap;
sampler NormalMapSampler = sampler_state { 
	texture = <NormalMap>; 
	magfilter = POINT; 
	minfilter = POINT; 
	mipfilter = POINT; 
	AddressU = mirror; 
	AddressV = mirror;};
Texture DepthMap;
sampler DepthMapSampler = sampler_state { 
	texture = <DepthMap>; 
	magfilter = POINT; 
	minfilter = POINT; 
	mipfilter = POINT; 
	AddressU = mirror; 
	AddressV = mirror;};

struct VertexShaderInput
{
    float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
		//get normal data from the normalMap
    float4 normalData = tex2D(NormalMapSampler,input.TexCoord);
		//tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
	float4 diffuseColor = tex2D(DiffuseMapSampler, input.TexCoord);

		//read depth
	float depthVal = tex2D(DepthMapSampler,input.TexCoord).r;
		//compute screen-space position
	float4 position;
	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
		//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

		//surface-to-light vector
    float3 lightVector = -normalize(lightDirection);
		//compute diffuse light
    float NdL = max(0,dot(normal,lightVector));
    float3 diffuseLight = NdL * Color.rgb * diffuseColor;
		//reflexion vector
    float3 reflectionVector = normalize(reflect(lightVector, normal));
		//camera-to-surface vector
    float3 directionToCamera = normalize(cameraPosition - position);
		//output the light
    return float4(diffuseLight.rgb, NdL);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
