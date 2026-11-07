// Put this on the material used by the "clue" plane.
// The fingerprint is invisible everywhere except inside a soft circle
// around _RevealPos (set every frame by FingerprintTable.cs from the mouse).
Shader "Custom/FingerprintReveal"
{
    Properties
    {
        _BaseMap ("Fingerprint Texture", 2D) = "white" {}
        _Color ("Print Color", Color) = (0.55, 0.9, 1, 1)
        [Toggle] _AlphaFromDarkness ("Dark-on-white image (no transparency)", Float) = 1
        _RevealPos ("Reveal Position (set by script)", Vector) = (0, -9999, 0, 0)
        _RevealRadius ("Reveal Radius (world units)", Float) = 0.08
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _Color;
                float  _AlphaFromDarkness;
                float4 _RevealPos;
                float  _RevealRadius;
                float  _Softness;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // Either use the image's own alpha, or treat dark lines as the print.
                half printAlpha = lerp(tex.a, (1.0h - dot(tex.rgb, half3(0.2126h, 0.7152h, 0.0722h))) * tex.a, _AlphaFromDarkness);

                // Soft circle around the flashlight point.
                float d = distance(IN.positionWS, _RevealPos.xyz);
                float mask = 1.0 - smoothstep(_RevealRadius * (1.0 - _Softness), _RevealRadius, d);

                return half4(_Color.rgb, printAlpha * mask * _Color.a);
            }
            ENDHLSL
        }
    }
}
