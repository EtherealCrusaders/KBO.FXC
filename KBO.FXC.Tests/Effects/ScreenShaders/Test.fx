
#include "common.fxh"
int param1;
int param2,param3;
float4 PSMain(float4 color : COLOR0) : COLOR0
{
    return color * param1;
}

technique t0
{
    pass p0
    {
        PixelShader = compile ps_2_0 PSMain();
    }
}