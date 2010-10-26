//-----------------------------------------------------------------------------
// Description: Defines the shaders necessary for rendering the sky.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix World;
const matrix ViewProjection;

float4 BetaR = {0.00004182918f, 0.0000707346f, 0.000146676f, 1.0f};
float4 BetaM = {0.0000057406f, 0.00000739969f, 0.0000105143f, 1.0f};
float4 AngularBetaR = {4.16082e-005f, 7.03612e-005f, 0.000145901f, 1.0f};
float4 AngularBetaM = {0.00133379f, 0.00173466f, 0.00249762f, 1.0f};
float PI = 3.14f;
float G = 0.98f;
float4 SunVector = {0.578f, 0.578f, 0.578f, 0.0f};
float4 SunColour = {0.578f, 0.578f, 0.578f, 0.0f};


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	float3 position : POSITION0;
	float3 normal   : NORMAL0;
	float2 texcoord : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 position     : POSITION;
	float4 inscattering : COLOR0;
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
	
	// transform position to screen space
	float4 worldPos = mul(World, float4(IN.position, 1));
	OUT.position = mul(ViewProjection, worldPos);
	
	float3 eyeVector = normalize(worldPos);
	
	// distance from camera to vertex
	float s = 3000.0f;
	
	// extinction
	float4 extinction = -(BetaR + BetaM) * s;
	extinction.xyz = exp(extinction.xyz);
	extinction.w = 1.0f;
	
	// dot product is |a| * |b| * cos(theta), and since eyeVector
	// and sunVector are unit length, the dot product is simply
	// equal to cos(theta)
	float cosTheta = dot(eyeVector, -SunVector);
	
	// BetaR(theta)
	float4 betaRTheta = (3 / (16 * PI)) * AngularBetaR * (1 + (cosTheta * cosTheta));
	
	// BetaM(theta)
	float betaMThetaNumerator = (1 - G) * (1 - G);
	float betaMThetaDenominator = 1 + (G * G) - (2 * G * cosTheta);
	betaMThetaDenominator = sqrt(betaMThetaDenominator);
	betaMThetaDenominator = betaMThetaDenominator * betaMThetaDenominator * betaMThetaDenominator;
	float4 betaMTheta = (1 / (4 * PI)) * AngularBetaM * (betaMThetaNumerator / betaMThetaDenominator);
	
	OUT.inscattering = ((betaRTheta + betaMTheta) / (BetaR + BetaM)) * SunColour * (1 - extinction);
	OUT.inscattering *= 100.0f;
	
	OUT.inscattering.w = 1.0f;
	
	return OUT;
}

PS_OUTPUT PS(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	
	OUT.colour = IN.inscattering;

	return OUT;
}

PS_OUTPUT PS2(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	
	OUT.colour = float4(1, 1, 1, 1);

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
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
	
	pass Pass1
	{
		ZEnable = true;
		FillMode = WIREFRAME;
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS2();
	}
}