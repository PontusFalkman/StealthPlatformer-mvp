Shader "Stealth/RadialFade2D"
{
    Properties{
        _Color("Tint", Color) = (1,1,1,1)
        _CoreAlpha("Core Alpha", Range(0,1)) = 0.9
        _InnerFrac("Inner Fraction", Range(0,0.99)) = 0.2   // inner / outer
        _Steps("Steps (0=smooth)", Float) = 3
        _MainTex("Sprite", 2D) = "white" {}
    }
    SubShader{
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass{
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _CoreAlpha, _InnerFrac, _Steps;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = TRANSFORM_TEX(v.uv,_MainTex); return o; }

            fixed4 frag(v2f i):SV_Target
            {
                float2 p = i.uv - 0.5;                 // center at 0,0
                float r = length(p)/0.5;               // 0 center .. 1 at sprite edge
                if (r>1.0) discard;                    // circular mask

                float inner = saturate(_InnerFrac);    // 0..<1
                float t = saturate((r - inner) / max(1e-4, 1.0 - inner)); // 0 in core .. 1 at edge
                float f = ( _Steps <= 0.0 ) ? (1.0 - t)
                          : ceil( (1.0 - t) * _Steps ) / _Steps;

                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                tex.a *= _CoreAlpha * f;               // 90% in core, step down outward
                return tex;
            }
            ENDHLSL
        }
    }
}
