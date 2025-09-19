Shader "UI/DarkenCutoutUI"
{
    Properties{
        _Alpha("Dark Alpha", Range(0,1)) = 0.25
        _MainTex("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
    }
    SubShader
    {
Tags { "Queue"="Transparent+55" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Stencil { Ref 1 ReadMask 255 Comp NotEqual }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Alpha;
            CBUFFER_END

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_Position; float2 uv:TEXCOORD0; };
            v2f vert(appdata v){ v2f o; o.pos=TransformObjectToHClip(v.vertex.xyz); o.uv=TRANSFORM_TEX(v.uv,_MainTex); return o; }

            half4 frag(v2f i):SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half a = saturate(_Alpha);
                half3 col = lerp(base.rgb, 0, a);
                return half4(col, a * base.a);
            }
            ENDHLSL
        }
    }
}
