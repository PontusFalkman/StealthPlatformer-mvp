Shader "Stealth/DarknessMask"
{
    Properties{
        _AmbientDarkness("Ambient Darkness [0..1]", Range(0,1)) = 0.10
    }
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass{
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define MAX_LIGHTS 32

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f      { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            float _AmbientDarkness;
            int   _LightCount;
            float4 _LightData[MAX_LIGHTS]; // xy=viewport, z=radius(vp), w=intensity [0..1]

            v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }

            float circleSmooth(float2 p, float2 c, float r){
                float d = distance(p,c);
                float inner = r*0.7;                // near-full inside 70%
                float t = saturate((d - inner)/max(r - inner, 1e-5));
                return 1.0 - smoothstep(0.0, 1.0, t); // 1 center → 0 edge+
            }

            fixed4 frag(v2f i):SV_Target{
                float darkness = saturate(_AmbientDarkness);
                [loop]
                for(int k=0;k<MAX_LIGHTS;k++){
                    if(k >= _LightCount) break;
                    float2 c = _LightData[k].xy;
                    float  r = _LightData[k].z;
                    float  inten = saturate(_LightData[k].w);
                    float  f = circleSmooth(i.uv, c, r);     // 0..1
                    darkness *= (1.0 - inten * f);           // carve
                }
                return float4(0,0,0, saturate(darkness));
            }
            ENDHLSL
        }
    }
}

