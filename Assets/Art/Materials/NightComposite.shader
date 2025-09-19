Shader "UI/NightComposite"
{
    Properties{
        [HideInInspector]_MainTex("Sprite", 2D) = "white" {}
        [HideInInspector]_Color ("Unused", Color) = (0,0,0,1)
        // driven by script only (default 0 = pitch black)
        [HideInInspector]_Ambient("Ambient Darkness", Range(0,1)) = 0
        [HideInInspector]_MaxAlpha("Patch Cap", Range(0,1)) = 0
        [HideInInspector]_Gamma("Patch Gain", Range(0.5,4)) = 1
    }
    SubShader
    {
        Tags{ "Queue"="Transparent+50" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
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
                float _Ambient, _MaxAlpha, _Gamma;
                int _DarkCount;
                float4 _DarkData[32]; // (vp.x, vp.y, radiusVP, intensity)
            CBUFFER_END

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_Position; float2 uv:TEXCOORD0; float4 clip:TEXCOORD1; };
            v2f vert(appdata v){ v2f o; o.pos=TransformObjectToHClip(v.vertex.xyz); o.clip=o.pos; o.uv=TRANSFORM_TEX(v.uv,_MainTex); return o; }

            half PatchAlpha(float2 vp)
            {
                half a = 0;
                [unroll] for(int i=0;i<32;i++){
                    if(i>=_DarkCount) break;
                    float2 c=_DarkData[i].xy; float r=_DarkData[i].z; float k=_DarkData[i].w;
                    if(r<=0 || k<=0) continue;
                    float t = saturate(1.0 - distance(vp,c)/r);
                    a = max(a, pow(saturate(t*k), _Gamma));
                }
                return saturate(a)*_MaxAlpha;
            }

            half4 frag(v2f i):SV_Target
            {
                // viewport from clip
                float2 vp = (i.clip.xy / i.clip.w) * 0.5 + 0.5;

                half aAmbient = saturate(_Ambient);
                half aPatch   = PatchAlpha(vp);
                half a = saturate(aAmbient + aPatch - aAmbient*aPatch); // saturating add

                // Pure black darkness. Alpha = darkness. No tint, no texture influence.
                return half4(0,0,0,a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
