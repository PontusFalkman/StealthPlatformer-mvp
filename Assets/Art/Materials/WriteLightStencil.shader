Shader "Hidden/WriteLightStencil"
{
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" }
        ZTest Always ZWrite Off Cull Off
        ColorMask RBG
        Stencil{ Ref 1 Comp Always Pass Replace }
        Pass{}
    }
}
