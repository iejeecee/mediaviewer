// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

Texture2D<float> yTexture;
Texture2D<float> uTexture;
Texture2D<float> vTexture;

SamplerState textureSampler
{
    //Filter = MIN_MAG_MIP_LINEAR;
	Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VS_IN
{
	float4 pos : POSITION;
	float4 uv : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = input.pos;
	output.uv = input.uv;
	
	return output;
}


float4 YUV444toRGB888(float3 yuv) {

	float3 pre;

	pre.r = yuv[0] - (16.0  / 255.0);
    pre.g = yuv[1] - (128.0 / 255.0);
    pre.b = yuv[2] - (128.0 / 255.0);

	const float3 red   = float3 (1.164, 0.0, 2.018);
    const float3 green = float3 (1.164, -0.813, -0.391);
    const float3 blue  = float3 (1.164, 1.596, 0.0);

	float4 output;

    output.r = dot(blue, pre);
    output.g = dot(green, pre);
    output.b = dot(red, pre);
    output.a = 1.0;

	return(output);
}

float4 YCrCbtoRGB(float3 yuv) {

	float y = 1.1643 * (yuv[0] - 0.0625);
	float u = yuv[1] - 0.5;
	float v = yuv[2] - 0.5;

	float r = y + 1.5958 * v;
	float g = y - 0.39173 * u - 0.81290 * v;
	float b = y + 2.017 * u;

	return(float4(b,g,r,1.0));

}

float4 PS( PS_IN input ) : SV_Target
{
/*
	float3 yuv;

	yuv[0] = yTexture.Sample(textureSampler, input.uv);
	yuv[1] = uTexture.Sample(textureSampler, input.uv);
	yuv[2] = vTexture.Sample(textureSampler, input.uv);

	float3 rgb = YUV444toRGB888(yuv);
	
	return float4(rgb.gbr, 1);
*/

	float3 yuv;
	yuv[0] = yTexture.Sample(textureSampler, input.uv).r;
    yuv[1] = uTexture.Sample(textureSampler, input.uv).r;
    yuv[2] = vTexture.Sample(textureSampler, input.uv).r;
   
	float4 output = YUV444toRGB888(yuv);
	//float4 output =	YCrCbtoRGB(yuv);

	return(output);

}

technique10 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}