Shader "TEM/StencilLight Color"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1) 
		_Radius("_Radius",Float) = 1
		_NoiseAmplitude("_NoiseAmplitude",Float) = 0.01
		_NoiseFrequency("_NoiseFrequency",Float) = 5
		_NoiseOffset("_Radius",Vector) = (0.1,0.1,0.1)
		_NoiseSpeed("_NoiseSpeed",Float) = 1
    }
    HLSLINCLUDE
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"        

	#include "SimplexNoiseGrad3D.cginc"

	float _Radius;
	float _NoiseAmplitude;
	float _NoiseFrequency;
	float3 _NoiseOffset;
	float _NoiseSpeed;
	float3 displace(float3 p)
	{
		float3 q = normalize(cross(p, float3(0, 1, 0)) + float3(0, 1e-5, 0));
		float3 r = cross(p, q);
		float3 n = snoise_grad(p * _NoiseFrequency + _NoiseOffset) * _NoiseAmplitude;
		return p * (_Radius + n.x) + q * n.y + r * n.z;
	}


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

		_NoiseOffset += float3(0.13, 0.82, 0.11) * 0.5 * _NoiseSpeed * _Time.y;
		float3 v1 = displace(v.vertex.xyz);
		v.vertex.xyz = v1;

        o.vertex = TransformObjectToHClip(v.vertex.xyz);
        return o;
    }

    CBUFFER_START(UnityPerMaterial)
        float4 _Color;    
    CBUFFER_END
    
    float4 frag(v2f i) : SV_Target
    {
        return _Color * _Color.a;
    }
    ENDHLSL

    SubShader
    {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent"}  
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


		Pass
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

        Pass
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "LightMode" = "UniversalForward"
            }
            ZTest Greater//always
            ZWrite off
            Cull Front
            Blend DstColor One

            Stencil
            {
                Ref 1
                Comp equal
                Pass Zero 
				fail zero
				Zfail zero

            }         
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }     






    }
}