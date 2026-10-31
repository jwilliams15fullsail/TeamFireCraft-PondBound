Shader "TEM/StencilLight Color CRACKS"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1) 
    }
    HLSLINCLUDE
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"        
    struct appdata
    {
        float4 vertex : POSITION;
    };
    
    struct v2f
    {
        float4 vertex : SV_POSITION;
    };
       
    
    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex.xyz);
        return o;
    }

    CBUFFER_START(UnityPerMaterial)
        float4 _Color;    
    CBUFFER_END
    
    float4 frag(v2f i) : SV_Target
    {
		return _Color;// *_Color.a;
    }
    ENDHLSL

    SubShader
    {
        Tags{"Queue" = "Geometry" "RenderType" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        //Pass
        //{
        //    Tags
        //    {
        //        "RenderType" = "Transparent"          
        //        "RenderPipeline" = "UniversalPipeline"            
        //    }         
        //    Zwrite off
        //    Ztest Greater//Lequal
        //    Cull Back
        //    Blend DstColor One
        //    
        //    Stencil
        //    {
        //        comp equal
        //        ref 1
        //        pass zero
        //        fail zero
        //        Zfail zero
        //    }         
        //    HLSLPROGRAM
        //    #pragma vertex vert
        //    #pragma fragment frag         
        //    ENDHLSL
        //}  


	/*	Pass
		{
		    Tags
		    {
		        "RenderType" = "Transparent"          
		        "RenderPipeline" = "UniversalPipeline"            
		    }         
		    Zwrite off
		    Ztest Lequal
		    Cull Back
		    Blend DstColor One
		    
		    Stencil
		    {
		        comp equal
		        ref 1
		        pass zero
		        fail zero
		        zfail zero
		    }         
		    HLSLPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag         
		    ENDHLSL
		}  
*/
        Pass
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "LightMode" = "UniversalForward"
            }
            //ZTest LEqual//always// LEqual//Greater//always
            ZWrite on//off
            Cull Back
            //Blend DstColor One

            Stencil
            {
                Ref 0
                Comp equal
                //Pass Zero 
				//fail zero
				//Zfail zero

            }    
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }     






    }
}