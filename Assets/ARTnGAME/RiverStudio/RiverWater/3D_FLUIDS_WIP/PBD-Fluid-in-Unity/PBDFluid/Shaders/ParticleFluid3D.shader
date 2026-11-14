
Shader "PBDFluid/ParticleFluid3D" 
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Tranparency("_Tranparency",float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent-10" }//Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model
		#pragma surface surf Standard addshadow fullforwardshadows
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup

		sampler2D _MainTex;
		float4 color;
		float diameter;
		float _Tranparency;

		struct Input 
		{
			float2 uv_MainTex;
			float4 color;
		};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<float3> positions;
#endif

		void setup()
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			float3 pos = positions[unity_InstanceID];
			float d = diameter/2;

			unity_ObjectToWorld._11_21_31_41 = float4(d, 0, 0, 0);
			unity_ObjectToWorld._12_22_32_42 = float4(0, d, 0, 0);
			unity_ObjectToWorld._13_23_33_43 = float4(0, 0, d, 0);
			unity_ObjectToWorld._14_24_34_44 = float4(pos, 1);

			unity_WorldToObject = unity_ObjectToWorld;
			unity_WorldToObject._14_24_34 *= -1;
			unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
#endif
		}

		half _Glossiness;
		half _Metallic;

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float4 texMe = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = texMe.rgb* _Tranparency;// (color.rgb * texMe.rgb) * _Tranparency;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = texMe.a;// _Tranparency;// 0.5; //1
		}
	ENDCG
	}

}