Shader "UI/DarknessPatches"
{
    Properties{
        _MaxAlpha("Patch Max Alpha", Range(0,1)) = 0.35
        _MainTex("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
    }
    SubShader
    {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Stencil { Ref 1 ReadMask 255 Comp NotEqual }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // matches DarknessPatchController
            // _DarkData[i] = (vx, vy, radiusVP, intensity)
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _MaxAlpha;
                int _DarkCount;
                float4 _DarkData[32];
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_Position; float2 uv:TEXCOORD0; float4 posCS:NORMAL; };

            v2f vert(appdata v){
                v2f o; float3 w = v.vertex.xyz;
                o.pos = TransformObjectToHClip(w);
                o.posCS = o.pos;
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            half patchAlpha(float2 vp)
            {
                // accumulate max of per-patch darkness
                half a = 0;
                [unroll] for(int i=0;i<32;i++){
                    if(i>=_DarkCount) break;
                    float2 c = _DarkData[i].xy;
                    float r = _DarkData[i].z;
                    float k = _DarkData[i].w; // intensity 0..1
                    if(r<=0 || k<=0) continue;
                    float d = distance(vp, c);
                    float t = saturate(1.0 - d / r);          // 1 at center -> 0 at edge
                    a = max(a, t * k);
                }
                return saturate(a);
            }

            half4 frag(v2f i):SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // compute viewport pos from clip
                float2 vp = (i.posCS.xy / i.posCS.w) * 0.5 + 0.5;

                half a = patchAlpha(vp) * _MaxAlpha;
                half3 col = lerp(base.rgb, 0, a); // darken only
                return half4(col, a * base.a);
            }
            ENDHLSL
        }
    }
}
