// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CustomRenderTexture/Scrolling_2layers 1"
{
	Properties
	{
		_Tex("Fog", 2D) = "white" {}
		_Tex2("Mask", 2D) = "white" {}
		_Tex3("Scene", 2D) = "white" {}
		_Speed("Speed", Vector) = (1,1,1,1)
	}

		SubShader
	{
		Lighting Off
		//Blend One Zero

			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
//Blend One OneMinusSrcAlpha // Premultiplied transparency
//Blend One One // Additive
//Blend OneMinusDstColor One // Soft additive
//Blend DstColor Zero // Multiplicative
//Blend DstColor SrcColor // 2x multiplicative

		Pass
		{
			CGPROGRAM
			//HLSLPROGRAM
		//#include "UnityCustomRenderTexture.cginc"
		#pragma vertex vert //CustomRenderTextureVertexShader
		#pragma fragment frag
		#pragma target 3.0

		float4		_Speed;
		sampler2D   _Tex;
		float4   _Tex_ST;
		sampler2D   _Tex2, _Tex3;
		float4   _Tex2_ST;
		float4   _Tex3_ST;

		struct appdata
		{
			float4 vertex : POSITION; // vertex position
			float2 uv : TEXCOORD0; // texture coordinate
		};

		struct v2f
		{
			float2 uv : TEXCOORD0; // texture coordinate
			float4 vertex : SV_POSITION; // clip space position
		};
		// vertex shader
		v2f vert(appdata v)
		{
			v2f o;
			// transform position to clip space
			// (multiply with model*view*projection matrix)
			o.vertex = UnityObjectToClipPos(v.vertex);
			// just pass the texture coordinate
			o.uv = v.uv;
			return o;
		}

		float4 frag(v2f IN) : COLOR//float4 frag(v2f_customrendertexture IN) : COLOR
		{
			//float4 col = tex2D(_Tex, IN.localTexcoord.xy + frac(_Time * _Speed.xy));
			//float4 col2 = tex2D(_Tex2, IN.localTexcoord.xy + frac(_Time * _Speed.zw));
			float4 col = tex2D(_Tex, IN.uv.xy);// +frac(_Time * _Speed.xy)); //FOG
			float4 col2 = tex2D(_Tex2, IN.uv.xy);// +frac(_Time * _Speed.zw)); //MASK
			float4 col3 = tex2D(_Tex3, IN.uv.xy); //SCENE
			float4 colorFinal = col3;
			if (col2.r < 0.002) {
				return col3;
				discard;
			}
			return float4(clamp(col.rgb, 0.055, 1) + clamp(col3.rgb, 0.055, 1)*0.4, 1);// float4(1, 0, 0, 0); //max(0.1f, col * col2); //float4(1, 0, 0, 0.5);/// max(0.1f, col * col2);
		}
		ENDCG
			//ENDHLSL
		}
	}
}