// Full-screen layer used by BrightnessOverlay.cs. It multiplies what is already on screen.
// Must stay in a Resources folder so it's included in builds.
Shader "Hidden/PondBound/BrightnessOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Amount ("Amount", Float) = 1
        _SrcBlend ("Src Blend", Float) = 0
        _DstBlend ("Dst Blend", Float) = 3
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        ColorMask RGB
        Blend [_SrcBlend] [_DstBlend]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Amount;

            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag () : SV_Target
            {
                return fixed4(_Amount, _Amount, _Amount, 1);
            }
            ENDCG
        }
    }
}
