//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

const texture HeightMapTexture;

const float HeightMapSizeInverse;

const float GridSpacing;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

uniform sampler HeightMapSampler = sampler_state           
{
    Texture   = <HeightMapTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = WRAP;
    AddressV  = WRAP;
};


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	float2 posxy    : POSITION0;
	float2 worldPos  : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 position : POSITION;
	float2 worldPos : TEXCOORD0;
};

struct PS_INPUT
{
	float2 worldPos : TEXCOORD0;
	float2 vPos    : VPOS;
};

struct PS_OUTPUT
{
	float4 colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VS_OUTPUT UpdateElevationFromHeightMapVS(VS_INPUT input)
{
	VS_OUTPUT output;

	output.position = mul(WorldViewProjection, float4(input.posxy, 0.0f, 1.0f));
	output.worldPos = float2(input.worldPos);

	return output;
}

float LookupHeight(float2 texcoord)
{
	return tex2D(HeightMapSampler, texcoord + (0.5f * HeightMapSizeInverse)).x * 100.0f;// * 65535.0f;
}

float CalculateAverageHeight(float2 texcoord1, float2 texcoord2)
{
	float z1 = LookupHeight(texcoord1);
	float z2 = LookupHeight(texcoord2);
	return (z1 + z2) / 2.0f;
}

float CalculateAverageHeight(float2 texcoord1, float2 texcoord2, float2 texcoord3, float2 texcoord4)
{
	float z1 = LookupHeight(texcoord1);
	float z2 = LookupHeight(texcoord2);
	float z3 = LookupHeight(texcoord3);
	float z4 = LookupHeight(texcoord4);
	return (z1 + z2 + z3 + z4) / 4.0f;
}

PS_OUTPUT UpdateElevationFromHeightMapPS(PS_INPUT input)
{
	PS_OUTPUT output;
	
	float2 texcoords = round(input.worldPos);
	texcoords *= HeightMapSizeInverse;
	texcoords.y = 1.0f - texcoords.y;
	
	// offset texture coordinates, see Directly Mapping Texels to Pixels
	// in the DirectX SDK helpfile
	float zf = LookupHeight(texcoords);
	
	// TODO: currently I'm using dynamic branching here, which apparently is bad
	// for performance. instead we could use a lookup texture of 2x2 which gives
	// us a mask that can then be applied to get the correct vertices
	float zc;
	
	float x = round(input.vPos.x);
	float y = round(input.vPos.y);
	if (x % 2 == 0.0f && y % 2 == 0.0f)
	{
		zc = zf;
	}
	else if (x % 2 == 0)
	{
		// average of top and bottom
		zc = CalculateAverageHeight(
			float2(texcoords.x, texcoords.y + (GridSpacing * HeightMapSizeInverse)),
			float2(texcoords.x, texcoords.y - (GridSpacing * HeightMapSizeInverse)));
	}
	else if (y % 2 == 0)
	{
		// average of left and right
		zc = CalculateAverageHeight(
			float2(texcoords.x + (GridSpacing * HeightMapSizeInverse), texcoords.y),
			float2(texcoords.x - (GridSpacing * HeightMapSizeInverse), texcoords.y));
	}
	else
	{
		zc = CalculateAverageHeight(
			float2(texcoords.x + (GridSpacing * HeightMapSizeInverse), texcoords.y),
			float2(texcoords.x - (GridSpacing * HeightMapSizeInverse), texcoords.y),
			float2(texcoords.x, texcoords.y + (GridSpacing * HeightMapSizeInverse)),
			float2(texcoords.x, texcoords.y - (GridSpacing * HeightMapSizeInverse)));
	}
	
	//float zd = zc - zf;
	
	// pack the signed difference zd into the fractional component
	//float zf_zd = zf + ((zd + 65536) / 131072);
	
	//output.colour = float4(zf_zd, 0, 0, 1);
	output.colour = float4(zf, zc, 0, 1);
	//output.colour = float4(zf / 255.0f, 0, 0, 1);
	
	return output;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique UpdateElevationFromHeightMap
{
    pass P0
    {
				AlphaBlendEnable = false;
				
        VertexShader = compile vs_3_0 UpdateElevationFromHeightMapVS();
        PixelShader  = compile ps_3_0 UpdateElevationFromHeightMapPS();
    }
}

