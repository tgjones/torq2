//-----------------------------------------------------------------------------
// Description: Defines the shaders necessary for geoclipmapping on the GPU.
//              This single shader is used for all level footprints.
//              Parts of these shaders are taken from Asirvatham's and Hoppe's
//              paper "Terrain Rendering Using GPU-Based Geometry Clipmaps"
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

// ScaleFactor.xy: grid spacing of current level
// ScaleFactor.zw: origin of current block within world
const float4 ScaleFactor;

// shading colour used during debugging to differentiate between
// blocks, ring fix-ups, interior trim and outer degenerate triangles
const float4 Shading;

// FineBlockOrig.xy: 1/(w, h) of texture
// FineBlockOrig.zw: origin of block in texture
const float4 FineBlockOrig;

const float2 FineBlockOrig2;

// 2D texture that stores heights at this level's resolution.
// it is a floating pointing texture, with data packed into the float
const texture ElevationTexture;

// 2D texture that stores the normal data. This texture has twice
// the resolution of the elevation texture
const texture NormalMapTexture;

const texture CoarserNormalMapTexture;

// position of viewer in world coordinates
const float2 ViewerPos;

// this is ((n - 1) / 2) - w - 1, where
// n = grid size
// w = transition width
const float2 AlphaOffset;

// this needs to be the inverse of the transition width, which we choose
// as n / 10
const float2 OneOverWidth;

// used when calculating the grid position of the vertex
const float GridSize;

// vector for the direction of sunlight
const float3 LightDirection;

const float2 CoarserNormalMapTextureOffset;

const float NormalMapTextureSize;

const float NormalMapTextureSizeInverse;

const texture GrassTexture;

// ToroidalOffsets.xy: toroidal origin in texture coordinates
// ToroidalOffsets.zw: size of grid inside texture, used for modulation
const float4 ToroidalOffsets;

const float ElevationTextureSize;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler ElevationSampler = 
sampler_state
{
	Texture = <ElevationTexture>;
	MipFilter = NONE;
	MinFilter = POINT;
	MagFilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler NormalMapSampler = 
sampler_state
{
	Texture = <NormalMapTexture>;
	MipFilter = NONE;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler CoarserNormalMapSampler = 
sampler_state
{
	Texture = <CoarserNormalMapTexture>;
	MipFilter = NONE;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler GrassSampler = 
sampler_state
{
	Texture = <GrassTexture>;
	MipFilter = LINEAR;
	MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	half2 posxy    : POSITION0;
};

struct VS_OUTPUT
{
	float4 position : POSITION;
	float2 uv       : TEXCOORD0;
	float2 alpha    : TEXCOORD1;
	float2 worldPos : TEXCOORD2;
	float4 colour   : COLOR;
};

struct PS_OUTPUT
{
	float4 colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VS_OUTPUT VS(VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	// convert from grid xy to world xy coordinates
	float2 worldPos = (IN.posxy * ScaleFactor.xy) + ScaleFactor.zw;
	
	// compute coordinates for vertex texture
	float2 uv = (IN.posxy * FineBlockOrig.xy) + FineBlockOrig.zw;
	float2 offsetUv = uv + ToroidalOffsets.xy;
	
	float4 elevationFineCoarse = tex2Dlod(ElevationSampler, float4(offsetUv + (0.5f / ElevationTextureSize), 0, 0));
	float zf = elevationFineCoarse.x;
	float zc = elevationFineCoarse.y;
	float zd = zc - zf;
	
	// compute alpha (transition parameter) and blend elevation
	float2 pos = IN.posxy + (FineBlockOrig2.xy * GridSize);
	float2 alpha = clamp((abs(pos - ViewerPos) - AlphaOffset) * OneOverWidth, 0, 1);
	alpha.x = max(alpha.x, alpha.y);
	float z = zf + (alpha.x * zd);
	
	// transform position to screen space
	OUT.position = mul(WorldViewProjection, float4(worldPos, z, 1));
	
	OUT.uv = uv;
	
	OUT.alpha = float2(alpha.x, 0);
	
	OUT.colour = float4(alpha.x, 0, 0, 1);
	
	OUT.worldPos = worldPos;
	
	return OUT;
}

PS_OUTPUT PS(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	
	// get fine normal
	float2 normalf = tex2D(NormalMapSampler, IN.uv + ToroidalOffsets.xy).xy;
	
	// get coarse normal
	float2 texcoordc = ((IN.uv * NormalMapTextureSize) / 2.0) + CoarserNormalMapTextureOffset;
	texcoordc *= NormalMapTextureSizeInverse;
	float2 normalc = tex2D(CoarserNormalMapSampler, texcoordc + ToroidalOffsets.xy).xy;
	
	float3 normal = float3(((1 - IN.alpha.x) * normalf) + (IN.alpha.x * normalc), 1.0);

	// renormalize
	normal = normalize(normal);
	
	// compute simple diffuse lighting
	float s = clamp(dot(normal, LightDirection), 0, 1);
	float4 ambient = float4(0.7f, 0.7f, 0.8f, 1);
	//float4 material = float4(0.4f, 0.5f, 0.4f, 1);
	float4 material = tex2D(GrassSampler, IN.worldPos / 4);
	OUT.colour = float4(s, s, s, 1) * 0.2f + ambient * 0.1f + material * 0.7f;
	//OUT.colour = Shading;

	return OUT;
}

PS_OUTPUT PS_2(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	
	OUT.colour = float4(1, 0, 0, 0.7);
	
	return OUT;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique
{
	pass Pass0
	{
		ZEnable = true;
		//FillMode = WIREFRAME;
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
	
	/*pass Pass1
	{
		FillMode = WIREFRAME;
		//ZEnable = false;
		
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_2_0 PS_2();
	}*/
}