sampler2D implicitInput : register(s0);
     
float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
    float4 src = tex2D(implicitInput, uv);
     
    float4 dst;
    dst.rgb = dot(src.rgb, float3(0.3, 0.59, 0.11));
    dst.a = src.a;
        
    return dst;
}
