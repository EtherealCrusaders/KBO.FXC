
#include "common.fxh"

float4x4 uWorldViewProjection;

void VSMain(inout float4 position : POSITION0, inout float4 color : COLOR0, inout float2 texCoords : TEXCOORD0) 
{
    position = mul(position, uWorldViewProjection) ;
}

technique t0
{
    pass p0
    {
        VertexShader = compile vs_2_0 VSMain();
    }
}