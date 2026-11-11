// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SkyMaster/Ripples/CausticShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_BumpTexture ("_BumpTexture", 2D) = "white" {}
		causticSurfMain ("causticSurfMain", 2D) = "black" {}
		_ViewDirection ("_ViewDirection", Vector) = (0, 0, 1, 0)
		Amplitude ("Amplitude", Vector) = (0.7, 0.5, 0,0)
		RefractionAmount ("RefractionAmount", Float) = 0.8
		Ydistance ("Ydistance", Float) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			//#define PNORMAL
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			float4 _MainTex_ST;
			float4 _BumpTexture_ST;
			float4 causticSurfMain_ST;
			float4 _MainTex_TexelSize;
			float4 _BumpTexture_TexelSize;
			float4 causticSurfMain_TexelSize;
			
			sampler2D _MainTex;
			sampler2D _BumpTexture;
			sampler2D causticSurfMain;
			float3 _ViewDirection;
			float4 Amplitude;
			float RefractionAmount;
			float Ydistance;

			struct Input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vs2ps {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			vs2ps vert(Input IN) {
				vs2ps OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}
			
			float4 frag(vs2ps IN) : COLOR {
				#ifdef PNORMAL
				float3 norm = UnpackNormal(tex2D(_BumpTexture, IN.uv, _BumpTexture));
				norm.z *= -1;
				#else
				float3 norm = tex2D(_BumpTexture, IN.uv).xyz;
				#endif
				float3 refractor = refract(_ViewDirection, norm, RefractionAmount);
				refractor.xy = refractor.xy/refractor.z;	
				
				float2 Reft = Ydistance * refractor.xy + IN.uv;
				float4 tex = tex2D(_MainTex, Reft);
				float causticsTex = tex2D(causticSurfMain, Reft).r;
				float4 finalResult = tex * dot(float2(4*Amplitude.x,1*Amplitude.y), float2(1, causticsTex));
				return finalResult;
			}
			ENDCG
		}
	} 
}