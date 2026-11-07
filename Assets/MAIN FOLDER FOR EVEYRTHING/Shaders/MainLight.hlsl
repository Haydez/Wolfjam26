// MainLight.hlsl
// Shader Graph Custom Function (File mode), function name: MainLight
// Gives an Unlit Shader Graph access to URP's main directional light + shadows.

#ifndef PAINTERLY_MAINLIGHT_INCLUDED
#define PAINTERLY_MAINLIGHT_INCLUDED

void MainLight_float(float3 WorldPos,
                     out float3 Direction,
                     out float3 Color,
                     out float  ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    // Shader Graph preview has no scene lights; fake one so the preview isn't black.
    Direction   = normalize(float3(0.5, 0.5, -0.5));
    Color       = float3(1, 1, 1);
    ShadowAtten = 1.0;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light light = GetMainLight(shadowCoord);
    Direction   = light.direction;
    Color       = light.color;
    ShadowAtten = light.shadowAttenuation;
#endif
}

// Half-precision variant so the node works with either graph precision.
void MainLight_half(half3 WorldPos,
                    out half3 Direction,
                    out half3 Color,
                    out half  ShadowAtten)
{
    float3 d, c; float s;
    MainLight_float(WorldPos, d, c, s);
    Direction = d; Color = c; ShadowAtten = s;
}

#endif
