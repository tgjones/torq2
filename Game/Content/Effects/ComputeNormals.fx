//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

const texture ElevationTexture;

const float ElevationTextureSizeInverse;

const texture CoarserNormalMapTexture;

const float GridSpacing;

const float2 NormalScaleFactor;

// used when calculating the grid position of the vertex
const float GridSize;

const float2 CoarserNormalMapTextureOffset;

const float NormalMapTextureSizeInverse;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

uniform sampler ElevationSampler = sampler_state           
{
    Texture   = <ElevationTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

uniform sampler CoarserNormalMapSampler = sampler_state           
{
    Texture   = <CoarserNormalMapTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	float2 posxy     : POSITION0;
	float2 texcoords : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 position  : POSITION;
	float2 texcoords : TEXCOORD0;
};

struct PS_INPUT
{
	float2 texcoords : TEXCOORD0;
	float2 vPos      : VPOS;
};

struct PS_OUTPUT
{
	float4 colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VS_OUTPUT ComputeNormalsVS(VS_INPUT input)
{
	VS_OUTPUT output;

	output.position  = mul(WorldViewProjection, float4(input.posxy, 0.0f, 1.0f));
	output.texcoords = input.texcoords;

	return output;
}

PS_OUTPUT ComputeNormalsPS(PS_INPUT input)
{
	PS_OUTPUT output;
	
	// sample four points around quad face	
	input.texcoords = input.vPos + 0.5f;
	
	float2 texcoordTopLeft     = float2(input.texcoords.x - 0.5, input.texcoords.y - 0.5) * ElevationTextureSizeInverse;
	float2 texcoordTopRight    = float2(input.texcoords.x + 0.5, input.texcoords.y - 0.5) * ElevationTextureSizeInverse;
	float2 texcoordBottomLeft  = float2(input.texcoords.x - 0.5, input.texcoords.y + 0.5) * ElevationTextureSizeInverse;
	float2 texcoordBottomRight = float2(input.texcoords.x + 0.5, input.texcoords.y + 0.5) * ElevationTextureSizeInverse;
	
	//float factor = 100.0f;
	float zTopLeft     = tex2D(ElevationSampler, texcoordTopLeft     + (0.5f * ElevationTextureSizeInverse)).x;
	float zTopRight    = tex2D(ElevationSampler, texcoordTopRight    + (0.5f * ElevationTextureSizeInverse)).x;
	float zBottomLeft  = tex2D(ElevationSampler, texcoordBottomLeft  + (0.5f * ElevationTextureSizeInverse)).x;
	float zBottomRight = tex2D(ElevationSampler, texcoordBottomRight + (0.5f * ElevationTextureSizeInverse)).x;
	
	float z1 = zBottomRight - zTopLeft;
	float z2 = zTopRight - zBottomLeft;
	
	// WIKI - show the maths to pack the cross product into two components
	// The normal is now the cross product of the two tangent vectors
	// normal = (sqrt(sx^2 + sy^2), 0, zx) x (0, sqrt(sx^2 + sy^2), zy), where sx, sy = gridspacing in x, y
	// the normal below has n_z = 1
	// ScaleFac = 1/sx, 1/sy
	float2 normalf = float2((- z1 - z2) * NormalScaleFactor.x, (z1 - z2) * NormalScaleFactor.y);

	// pack coordinates in [-1, +1] range to [0, 1] range
	//normalf = (normalf / 2) + 0.5;

	// lookup the normals at the coarser level and pack it in normal map for current level
	//float2 texcoordc = ((input.texcoords / GridSize) / 2.0) + (CoarserNormalMapTextureOffset * NormalMapTextureSizeInverse);
	//float2 texcoordc = (((input.vPos + 0.5f) / (GridSize - 1)) / 2.0) + (CoarserNormalMapTextureOffset * NormalMapTextureSizeInverse);
	float2 texcoordc = ((input.vPos + 0.5f) / 2.0) + CoarserNormalMapTextureOffset;
	texcoordc = floor(texcoordc);
	texcoordc *= NormalMapTextureSizeInverse;
	//float2 texcoordc2 = (((input.vPos + 0.5f)) / 2.0) + (CoarserNormalMapTextureOffset);
	float2 normalc = tex2D(CoarserNormalMapSampler, texcoordc + (0.5 * NormalMapTextureSizeInverse)).xy;
	
	//float2 tccoarser  = (fmod((floor(tc) - ToroidalOrigin + Size),Size) * 0.5) + CoarseOffset;

	output.colour = float4(normalf.xy, normalc.xy);
	//output.colour = float4(floor(texcoordc) / 255, 0, 0);
	//output.colour = float4(input.vPos / 255, 0, 0);
	//output.colour = float4(zTopLeft / 100.0f, 0, 0, 1);
	//output.colour = float4(0, 0, 0, 1);

	return output;
}

technique ComputeNormals
{
    pass P0
    {
			AlphaTestEnable = false;
				AlphaBlendEnable = false;
				
        VertexShader = compile vs_3_0 ComputeNormalsVS();
        PixelShader  = compile ps_3_0 ComputeNormalsPS();
    }
}

