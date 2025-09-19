Shader "Stealth/StencilWriteURP"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry+450" "RenderType"="Opaque" }
        Cull Off
        ZTest Always
        ZWrite Off
        Blend One Zero
        ColorMask 0
        AlphaToMask Off
        Stencil { Ref 1 ReadMask 255 WriteMask 255 Comp Always Pass Replace Fail Keep ZFail Keep }

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings  { float4 positionHCS : SV_Position; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target { return 0; } // writes stencil only
            ENDHLSL
        }
    }
    FallBack Off
}
