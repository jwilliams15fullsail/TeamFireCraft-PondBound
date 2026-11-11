// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Custom/Wiggle URP"
{
	Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGBA)", 2D) = "white" {}
		_Gloss ("_MetallicGloss (RGB)", 2D) = "white" {}
		_Tints ("Tints (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Amount ("Wave1 Frequency", float) = 1
		_TimeScale ("Wave1 Speed", float) = 1.0
		_Distance ("Distance", float) = 0.1
	}

    SubShader
		{
			Tags { "RenderType" = "Opaque" }
			Cull Off
			LOD 200
				Pass{
			CGPROGRAM

			
			//#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
			#include "UnityCG.cginc"
			//#include "UnityInstancing.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			//#pragma target 5.0
			#pragma multi_compile_instancing
			//#define UNITY_PROCEDURAL_INSTANCING_ENABLED 1

			

			// https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
			#pragma instancing_options procedural:setup

			sampler2D _MainTex;
			sampler2D _Tints;
			sampler2D _Gloss;

			//struct Input
			//{
			//	float2 uv_MainTex;
			//};
			float4 _MainTex_ST;

			struct Boid
			{
				float3 pos;
				float3 fwd;
			};

			half4 _Direction;
			half _Glossiness;
			half _Metallic;
			float4 _Color;
			float _TimeScale;
			float _Amount;
			float _Distance;

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<Boid> boidBuffer;
	#endif

			// look at matrix https://i.stack.imgur.com/LV0gi.png (column major)
			float4x4 lookAtMatrix(float3 target, float3 eye, float3 up)
			{
				float3 fwd = normalize(target - eye);
				float3 side = normalize(cross(fwd, up));
				float3 up2 = normalize(cross(side, fwd));

				return float4x4(
					side.x, up2.x, fwd.x, 0,
					side.y, up2.y, fwd.y, 0,
					side.z, up2.z, fwd.z, 0,
					-dot(side, eye), -dot(up2, eye), -dot(fwd, eye), 1
				);
			}

			// this belongs to the pragma instancing_options setup function defined
			void setup()
			{

			}

			//https://docs.unity3d.com/540/Documentation/Manual/GPUInstancing.html
			struct appdata
			{
				float4 vertex : POSITION;//SV_POSITION//POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID//UNITY_VERTEX_INPUT_INSTANCE_ID //UNITY_INSTANCE_ID
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID//UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert( appdata  v)
			{
				//v0.1
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 offs = float4(0.0, 0.0, 0.0, 0.0);

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				//UNITY_GET_INSTANCE_ID(v)
				offs = sin((unity_InstanceID + _Time.y) * _TimeScale + v.vertex.z * _Amount) * _Distance;

				// from where +
				float3 pos = boidBuffer[unity_InstanceID].pos;

				// + what to face to
				float3 target = pos + boidBuffer[unity_InstanceID].fwd;

				float4x4 lookAt = lookAtMatrix(target, pos, float3(0.0, 1.0, 0.0));
				v.vertex = mul(lookAt, v.vertex);
				v.vertex.xyz += pos.xyz;
	#else
				offs = sin((_Time.y) * _TimeScale + v.vertex.z * _Amount) * _Distance;
	#endif

				v.vertex.x += offs;

				//v0.1
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			float4 frag(v2f IN) : SV_Target//void surf(Input IN, inout SurfaceOutputStandard o)
			{
				 UNITY_SETUP_INSTANCE_ID(IN);

				float4 c = tex2D(_MainTex, IN.uv);
				float4 g = tex2D(_Gloss, IN.uv);

				float4 tintColour = tex2D(_Tints, float2(0.0, 0.0));

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				int id = unity_InstanceID;
				while (id >= 1.0)
					id /= 10.0;

				tintColour = tex2D(_Tints, float2(id, 0.0));
	#endif
				//o.Albedo = lerp(c.rgb, c.rgb * tintColour, c.a) * _Color;
				//o.Metallic = _Metallic;
				//o.Smoothness = g.a * _Glossiness;

				//UNITY_APPLY_FOG(IN.fogCoord, col);
				return float4(lerp(c.rgb, c.rgb * tintColour, c.a) * _Color * g.a * _Glossiness*2,1);
			}
			

			ENDCG
				}
		}
		
	///FallBack "Diffuse"
}