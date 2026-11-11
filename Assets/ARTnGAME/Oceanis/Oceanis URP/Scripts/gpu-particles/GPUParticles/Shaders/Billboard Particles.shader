Shader "Custom/Billboard Particles"
{
	Properties
	{
		_MainTex("Particle Sprite", 2D) = "white" {}
		_SizeMul("Size Multiplier", Float) = 1
		oceanTex("oceanTex Sprite", 2D) = "white" {}
		scaleY("scaleY", Float) = 1
	}

		SubShader
		{
			Pass
			{
				Cull Off//Back
				Lighting Off
				Zwrite Off

			//Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha
			Blend One One
			//Blend OneMinusDstColor One

			LOD 200

			Tags
			{
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
			}

			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "./Common.cginc"

			uniform sampler2D _MainTex;	
			float4 _MainTex_ST;	

			//v0.1
			sampler2D oceanTex;
			float scaleY;

			float _SizeMul;

			StructuredBuffer<Particle> particles;
			StructuredBuffer<float3> quad;

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 col : COLOR;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 q = quad[id];

				//o.uv = q + 0.5f;
				o.uv = float2(q.x*_MainTex_ST.x +_MainTex_ST.z, q.y*_MainTex_ST.y + _MainTex_ST.w);// +0.5f;
				

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(particles[inst].position, 1.0f)) + float4(q, 0.0f) * _SizeMul * particles[inst].size);

				//v0.1
				//float4 oceanTexA = tex2D(oceanTex, float2(o.uv.xy));
				//float4 oceanTexA = tex2Dlod(oceanTex, float4(o.uv.xy, 0, 0));
				//float4 oceanTexA = tex2Dlod(oceanTex, float4(particles[inst].position.xy*100111, 0, 0));
				//o.pos.y += oceanTexA.r*10*scaleY;
				//o.pos.x *= 5;
				//o.pos.z *= 5;
				o.col = particles[inst].alive * particles[inst].color;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return tex2D(_MainTex, i.uv) * i.col * i.col.a;
			}

			ENDCG
		}
		}
}