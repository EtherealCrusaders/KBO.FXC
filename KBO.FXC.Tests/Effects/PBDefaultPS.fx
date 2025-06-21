
#include "common.fxh"

sampler2D uImage0 : register(s0);
float4 PSMain(float4 color : COLOR0, float2 texCoords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, texCoords) * color;
}

technique t0
{
    pass p0
    {
        PixelShader = compile ps_2_0 PSMain();
    }
}