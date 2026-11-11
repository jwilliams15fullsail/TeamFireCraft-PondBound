Shader "ProjectorB" {
	Properties{
		_ShadowTex("Projected Image", 2D) = "white" {}
		Intensity("Intensity", Float) = 1
			Power("Power", Float) = 1

			UpperLimits("UpperLimits XYZ", Float) = (10,10,10,1)
			LowerLimits("LowerLimits XYZ", Float) = (-10,-10,0,1)
	}
		SubShader{
		Pass{
		Blend One One
		// add color of _ShadowTex to the color in the framebuffer 
		ZWrite Off // don't change depths
		//Offset - 1, -1 // avoid depth fighting (should be "Offset -1, -1")	

		CGPROGRAM
			#include "UnityCG.cginc" //VR1
#pragma vertex vert  
#pragma fragment frag 

		// User-specified properties
		uniform sampler2D _ShadowTex;

	// Projector-specific uniforms
	uniform float4x4 unity_Projector; // transformation matrix 
									  // from object space to projector space 
	float Intensity;
	float Power;
	float4 UpperLimits;
	float4 LowerLimits;
	struct vertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
	};
	struct vertexOutput {
		float4 pos : SV_POSITION;
		float4 posProj : TEXCOORD0;
		// position in projector space
		UNITY_VERTEX_OUTPUT_STEREO //VR1
	};

	vertexOutput vert(vertexInput input)
	{
		vertexOutput output;

		UNITY_SETUP_INSTANCE_ID(input); //v1.7.8 //VR1
		UNITY_INITIALIZE_OUTPUT(vertexOutput, output); //Insert //VR1
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); //Insert //VR1		


		output.posProj = mul(unity_Projector, input.vertex);
		output.pos = UnityObjectToClipPos(input.vertex);
		return output;
	}


	float4 frag(vertexOutput input) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);//VR1

		if (input.posProj.w > 0.0 
			&& input.posProj.x > LowerLimits.x && input.posProj.x < UpperLimits.x
			&& input.posProj.y > LowerLimits.y && input.posProj.y < UpperLimits.y
			&& input.posProj.z > LowerLimits.z && input.posProj.z < UpperLimits.z
			) // in front of projector?
		{
			float4 tex= tex2D(_ShadowTex ,
				float2(input.posProj.x, input.posProj.y) / input.posProj.w);
			// alternatively: return tex2Dproj(  
			//    _ShadowTex, input.posProj);
			if (tex.r > 0.01) {
				return float4(1,1,1,1-tex.r)*(pow(tex.r,Power)) * Intensity;
			}
			else {
				return tex * 0 ;
			}
		}
		else // behind projector
		{
			return float4(0.0, 0.0, 0.0, 0.0);
		}
	}

		ENDCG
	}
	}
		Fallback "Projector/Light"
}