Shader "TEM/StencilLight MaskMOVING"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        
        Pass
        {
            
            Ztest Greater
            Zwrite off
            Cull Off
            Colormask 0
            Stencil
            {
                Comp Always
                Ref 1
                Pass Replace
            }
        }
        
        
    }
}