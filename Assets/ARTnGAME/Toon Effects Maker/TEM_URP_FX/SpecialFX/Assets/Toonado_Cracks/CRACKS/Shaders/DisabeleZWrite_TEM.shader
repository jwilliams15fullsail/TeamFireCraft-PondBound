Shader "TEM/DisableZWriteTEM"
{
    SubShader{
		 Tags
		{
			"RenderType" = "Geometry-1"
			"Queue" = "Geometry-1"
			"RenderPipeline" = "UniversalPipeline"
		}

        Pass{
            //ZWrite Off
			Cull Off
			ZTest Always
			ZWrite Off
			Stencil { 
				Comp Always //NotEqual
				Ref 1 
				Pass Replace
			}
			//Stencil { PassFront IncrWrap PassBack DecrWrap }
			ColorMask 0 // prevents drawing the actual object


			//Ztest Greater
			//Zwrite off
			//Cull Off
			//Colormask 0
			//Stencil
			//{
			//	Comp Greater//Always
			//	Ref 1
			//	Pass Replace
			//}
        }
    }
}