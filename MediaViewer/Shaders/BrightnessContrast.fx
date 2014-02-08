sampler2D implicitInput : register(s0);

float Brightness : register(C0);
float Contrast : register(C1);

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
	float4 pixelColor = tex2D(implicitInput, uv);
	
	//pixelColor.rgb /= pixelColor.a;

	// Apply contrast.
	pixelColor.rgb = ((pixelColor.rgb - 0.5f) * max(Contrast,0)) + 0.5f;

	// Apply brightness.
	pixelColor.rgb += Brightness;

	// Return final pixel color.
	//pixelColor.rgb *= pixelColor.a;

	return pixelColor;
}
