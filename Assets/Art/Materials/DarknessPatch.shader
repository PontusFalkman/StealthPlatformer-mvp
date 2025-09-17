Shader "Stealth/DarknessPatch"
{
    Properties{ _MaxAlpha("Max Darkness Alpha", Range(0,1)) = 0.25 }
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass{
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define MAX_DARK 32

            struct appdata{ float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f{ float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            float _MaxAlpha;
            int _DarkCount;
            float4 _DarkData[MAX_DARK]; // xy=vp pos, z=radius, w=intensity

            v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }

            float falloff(float2 p, float2 c, float r){
                float d = distance(p,c);
                if (d >= r) return 0;
                float t = saturate(1 - d/r);
                return smoothstep(0,1,t);
            }

            fixed4 frag(v2f i):SV_Target{
                float a = 0;
                [loop]
                for(int k=0;k<MAX_DARK;k++){
                    if(k>=_DarkCount) break;
                    float2 c=_DarkData[k].xy;
                    float r=_DarkData[k].z;
                    float inten=_DarkData[k].w;
                    a = max(a, falloff(i.uv,c,r) * inten);
                }
                return float4(0,0,0, saturate(a * _MaxAlpha));
            }
            ENDHLSL
        }
    }
}
