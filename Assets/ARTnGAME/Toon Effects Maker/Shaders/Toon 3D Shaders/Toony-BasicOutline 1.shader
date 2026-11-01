// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Toon Effects Master/Basic Outline" {
	Properties {
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.0002, 0.03)) = .005
		_MainTex ("Base (RGB)", 2D) = "white" { }
		_ToonShade ("ToonShader Cubemap(RGB)", CUBE) = "" //{ Texgen CubeNormal }
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};
	
	uniform float _Outline;
	uniform float4 _OutlineColor;
	
	v2f vertA(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex.xyz);

		float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += offset * o.pos.z * _Outline;

		o.color = _OutlineColor;


		v.vertex.xyz = 0;
		return o;
	}
	ENDCG

	SubShader {
		Tags { "RenderType"="Opaque" }
		
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front
			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vertB
			#pragma fragment frag
			v2f vertB(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex.xyz);

				//float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);

				float3 norm = UnityObjectToViewPos(float4(v.normal, 0));//FIX OUTLINE NEW UNITY

				float2 offset = TransformViewToProjection(norm.xy);

				o.pos.xy += offset * o.pos.z * _Outline;

				o.color = _OutlineColor;


				//o.pos.xyz = float3(1,1,1);
				return o;
			}
			half4 frag(v2f i) :COLOR { return i.color; }
			ENDCG
		}
		UsePass "Toon Effects Master/Basic/BASE"
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		UsePass "Toon Effects Master/Basic/BASE"
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front
			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vertA
			#pragma exclude_renderers shaderonly
			ENDCG
			SetTexture [_MainTex] { combine primary }
		}
	}
	
	Fallback "Toon Effects Master/Basic"
}
