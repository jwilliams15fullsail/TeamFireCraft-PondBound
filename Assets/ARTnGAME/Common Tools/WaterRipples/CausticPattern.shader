// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SkyMaster/Ripples/CausticPattern" {
	Properties {
		_BumpTexture ("_BumpTexture", 2D) = "white" {}
		RefractionAmount ("RefractionAmount", Float) = 0.8
		Ydistance ("Ydistance", Float) = 0.1
		Light ("Light", Vector) = (0, 1, 1, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

			Cull Off 
			ZTest Always 
			ZWrite Off 	
			Fog { Mode Off }
		
		Pass {
			CGPROGRAM
			//#define PNORMAL
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _BumpTexture;
			float4 _BumpTexture_TexelSize;
			float RefractionAmount;
			float Ydistance;
			float3 Light;
			float4 _texWidth;
			float _Displace;

			struct Input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				
			};
			struct vertx {
				float4 vertex : POSITION;
				float2 P_C : TEXCOORD0;
				float2 P_G : TEXCOORD1;
			};


			float3 refraction(float3 Norm, float Refr ,float3 L1 ) {
				float Dot = dot(Norm , L1);
				return (L1 * Refr + Norm * (-Dot * Refr  - sqrt(1.0 - pow(Refr,2) * (1.0 - pow(Dot,2)))));
			}

			vertx vert(Input IN) {
				vertx OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				float3 refractor = refraction(float3(0,0,-1), RefractionAmount, Light);
				float inverse = -1.0/refractor.z;
				float2 Displace1 = float2( Ydistance*refractor.x * inverse , Ydistance*refractor.y * inverse  );
				OUT.P_G  = IN.uv * _texWidth.zw;
				OUT.P_C = IN.uv + Displace1;
				return OUT;
			}
			
			float2 Findcollision(float2 P_C) {
				#ifdef PNORMAL
					float3 norm = UnpackNormal(tex2D(_BumpTexture, P_C));
					norm.z *= -1;
				#else
					float3 norm = tex2D(_BumpTexture, P_C).xyz;
				#endif
				float3 refractor = refract(Light, norm, RefractionAmount);
				refractor.xy = refractor.xy/refractor.z;				
				return Ydistance * refractor.xy + P_C;
			}
			float4 frag(vertx IN) : COLOR {				

				float Intensity0 = 0.0;
				float P_Gy0 = IN.P_G.y + (0 + _Displace);
				float Intensity1 = 0.0;
				float P_Gy1 = IN.P_G.y + (1 + _Displace);
				float Intensity2 = 0.0;
				float P_Gy2 = IN.P_G.y + (2 + _Displace);
				float Intensity3 = 0.0;
				float P_Gy3 = IN.P_G.y + (3 + _Displace);

				float2 P_C = IN.P_C + (0 + _Displace) * _texWidth.xy;
				float2 collision = _texWidth.zw * Findcollision(P_C);
				float ax = max(0, 1 - abs(IN.P_G.x - collision.x));
					float ay = max(0, 1 - abs(P_Gy0 - collision.y));
					Intensity0 += ax * ay;
					ay = max(0, 1 - abs(P_Gy1 - collision.y));
					Intensity1 += ax * ay;
					ay = max(0, 1 - abs(P_Gy2 - collision.y));
					Intensity2 += ax * ay;
					ay = max(0, 1 - abs(P_Gy3 - collision.y));
					Intensity3 += ax * ay;

				P_C = IN.P_C + (1 + _Displace) * _texWidth.xy;
				collision = _texWidth.zw * Findcollision(P_C);
				ax = max(0, 1 - abs(IN.P_G.x - collision.x));
					ay = max(0, 1 - abs(P_Gy0 - collision.y));
					Intensity0 += ax * ay;
					ay = max(0, 1 - abs(P_Gy1 - collision.y));
					Intensity1 += ax * ay;
					ay = max(0, 1 - abs(P_Gy2 - collision.y));
					Intensity2 += ax * ay;
					ay = max(0, 1 - abs(P_Gy3 - collision.y));
					Intensity3 += ax * ay;

				P_C = IN.P_C + (2 + _Displace) * _texWidth.xy;
				collision = _texWidth.zw * Findcollision(P_C);
				ax = max(0, 1 - abs(IN.P_G.x - collision.x));
					ay = max(0, 1 - abs(P_Gy0 - collision.y));
					Intensity0 += ax * ay;
					ay = max(0, 1 - abs(P_Gy1 - collision.y));
					Intensity1 += ax * ay;
					ay = max(0, 1 - abs(P_Gy2 - collision.y));
					Intensity2 += ax * ay;
					ay = max(0, 1 - abs(P_Gy3 - collision.y));
					Intensity3 += ax * ay;

				P_C = IN.P_C + (3 + _Displace) * _texWidth.xy;
				collision = _texWidth.zw * Findcollision(P_C);
				ax = max(0, 1 - abs(IN.P_G.x - collision.x));
					ay = max(0, 1 - abs(P_Gy0 - collision.y));
					Intensity0 += ax * ay;
					ay = max(0, 1 - abs(P_Gy1 - collision.y));
					Intensity1 += ax * ay;
					ay = max(0, 1 - abs(P_Gy2 - collision.y));
					Intensity2 += ax * ay;
					ay = max(0, 1 - abs(P_Gy3 - collision.y));
					Intensity3 += ax * ay;				
			
				return float4(Intensity0,Intensity1,Intensity2,Intensity3)*3;

			}
			ENDCG
		}
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D causticSurf1;
			float4 causticSurf1_TexelSize;			
			sampler2D causticSurf2;
			
			struct Input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vertx { 
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			  
			vertx vert(Input IN) {
				vertx OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}
			float4 frag(vertx IN) : COLOR {
				float2 Delta = causticSurf1_TexelSize.xy;
				float val = 0.0;
				val += tex2D(causticSurf1, IN.uv + Delta*float2(0, -3) ).r;
				val += tex2D(causticSurf1, IN.uv + Delta*float2(0, -2) ).g;
				val += tex2D(causticSurf1, IN.uv + Delta*float2(0, -1) ).b;
				val += tex2D(causticSurf1, IN.uv).a;
				val += tex2D(causticSurf2, IN.uv + Delta*float2(0,  1) ).r;
				val += tex2D(causticSurf2, IN.uv + Delta*float2(0,  2) ).g;
				val += tex2D(causticSurf2, IN.uv + Delta*float2(0,  3) ).b;				
				return float4(val, 0, 0, 0);
			}
			ENDCG
		}
	} 
}