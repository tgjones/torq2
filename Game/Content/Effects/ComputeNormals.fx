//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

const texture ElevationTexture;

const float ElevationTextureSizeInverse;

const float2 NormalScaleFactor;


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
	// NormalScaleFactor = 0.5/sx, 0.5/sy
	float2 normalf = float2((- z1 - z2) * NormalScaleFactor.x, (z1 - z2) * NormalScaleFactor.y);

	output.colour = float4(normalf.xy, 0, 0);

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

