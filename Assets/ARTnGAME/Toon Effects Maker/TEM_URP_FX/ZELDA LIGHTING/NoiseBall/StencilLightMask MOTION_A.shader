Shader "TEM/StencilLight Mask"
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
            
            //Ztest Lequal//Greater
            //Zwrite Off
            //Cull Off//Front//Off
            //Colormask 0
            //Stencil
            //{
            //    Comp Always //Lequal//Greater//Always//Greater              //Always
            //    Ref 1
            //    Pass Replace
            //}

			Ztest Greater
            Zwrite off
            Cull Off
            Colormask 0
            Stencil
            {
                Comp Greater//Always
                Ref 1
                Pass Replace
            }
        }
        
        
    }
}