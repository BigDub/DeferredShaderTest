float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

Texture DiffuseTexture;
sampler DiffuseTextureSampler = sampler_state { 
	texture = <DiffuseTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = mirror; 
	AddressV = mirror;};

Texture NormalTexture;
sampler NormalTextureSampler = sampler_state { 
	texture = <NormalTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 

	mipfilter = LINEAR; 
	AddressU = mirror; 
	AddressV = mirror;};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 DiffuseTex : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 DiffuseTex : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.DiffuseTex = input.DiffuseTex;
	output.Normal = mul(input.Normal,World);
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

	output.Color = tex2D(DiffuseTextureSampler, input.DiffuseTex);
	output.Normal = float4(0.5f * (normalize(input.Normal) + 1), 1);
	output.Depth = input.Depth.x / input.Depth.y;

    return output;
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
