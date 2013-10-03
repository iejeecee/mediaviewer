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
    Filter = MIN_MAG_MIP_LINEAR;
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


float3 YUV444toRGB888(float3 yuv) {

	float3 temp;
	
    temp.r = yuv[0] - (16.0  / 256.0);
    temp.g = yuv[1] - (128.0 / 256.0);
    temp.b = yuv[2] - (128.0 / 256.0);

	float3x3 mat = {
		1.1640625, 0, 1.59765625,
		1.1640625, -0.390625, -0.8125,
		1.1640625, 0, 2.015625
	};

	float3 rgb = mul(mat, temp);

	return(rgb);
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

	 float3 pre;

    pre.r = yTexture.Sample(textureSampler, input.uv).r - (16.0  / 256.0);
    pre.g = uTexture.Sample(textureSampler, input.uv).r - (128.0 / 256.0);
    pre.b = vTexture.Sample(textureSampler, input.uv).r - (128.0 / 256.0);

    const float3 red   = float3 (0.00456621, 0.0, 0.00625893) * 255.0;
    const float3 green = float3 (0.00456621, -0.00153632, -0.00318811) * 255.0;
    const float3 blue  = float3 (0.00456621, 0.00791071, 0.0) * 255.0;

	float4 output;

    output.r = dot(blue, pre);
    output.g = dot(green, pre);
    output.b = dot(red, pre);
    output.a = 1.0;

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