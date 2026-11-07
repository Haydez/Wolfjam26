// PainterlyKuwahara.shader
// URP full-screen Kuwahara filter: flattens the image into painterly patches
// while keeping edges crisp. Use with URP's "Full Screen Pass Renderer Feature".

Shader "Hidden/Painterly/Kuwahara"
{
    Properties
    {
        _Radius   ("Brush Radius (px)", Range(1, 8)) = 4
        _Strength ("Blend With Original", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "Kuwahara"

            HLSLPROGRAM
            #pragma vertex Vert          // provided by Blit.hlsl
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Radius;
            float _Strength;

            float3 SampleScene(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv    = input.texcoord;
                float2 texel = 1.0 / _ScreenParams.xy;
                int    r     = (int)_Radius;

                // Mean and sum-of-squares for the four overlapping quadrants
                // around the pixel (top-left, top-right, bottom-left, bottom-right).
                float3 m0 = 0, m1 = 0, m2 = 0, m3 = 0;
                float3 s0 = 0, s1 = 0, s2 = 0, s3 = 0;

                [loop] for (int y = -r; y <= r; y++)
                {
                    [loop] for (int x = -r; x <= r; x++)
                    {
                        float3 c  = SampleScene(uv + float2(x, y) * texel);
                        float3 c2 = c * c;

                        if (x <= 0 && y <= 0) { m0 += c; s0 += c2; }
                        if (x >= 0 && y <= 0) { m1 += c; s1 += c2; }
                        if (x <= 0 && y >= 0) { m2 += c; s2 += c2; }
                        if (x >= 0 && y >= 0) { m3 += c; s3 += c2; }
                    }
                }

                float n = (r + 1) * (r + 1);
                m0 /= n; m1 /= n; m2 /= n; m3 /= n;

                // Variance per quadrant, summed across RGB.
                float v0 = dot(abs(s0 / n - m0 * m0), 1);
                float v1 = dot(abs(s1 / n - m1 * m1), 1);
                float v2 = dot(abs(s2 / n - m2 * m2), 1);
                float v3 = dot(abs(s3 / n - m3 * m3), 1);

                // Take the mean of the calmest quadrant -> flat "brush daubs", sharp edges.
                float3 result = m0; float minV = v0;
                if (v1 < minV) { minV = v1; result = m1; }
                if (v2 < minV) { minV = v2; result = m2; }
                if (v3 < minV) { minV = v3; result = m3; }

                float3 original = SampleScene(uv);
                return float4(lerp(original, result, _Strength), 1);
            }
            ENDHLSL
        }
    }
}
