// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SkyMaster/Ripples/RippleGeneratorSM" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_FlowTex ("Flow Texture", 2D) = "white" {}
		_PrevTex ("Flow Texture", 2D) = "white" {}
		Amplitude("Amplitude", Float) = 1
		thickness("thickness", Float) = 14.0
		delta("delta", Float) = 0.01
		TimeDelta("TimeDelta", Float) = 0.01
		distrubanceSize("distrubanceSize", Float) = 0.5
		extraRipples("extraRipples", Float) = 0
		dumper("dumper", Vector) = (0.2, 1.0, 1.0, 1.0)
		diffCamPos("diffCamPos", Vector) = (0, 0, 0, 1.0)
	}
		SubShader{

				ZWrite Off
				Cull Off
				Fog { Mode Off }
				ZTest Always

				CGINCLUDE
				#include "UnityCG.cginc"

			//v0.2
			float4 diffCamPos;

			sampler2D _MainTex;
			sampler2D _FlowTex;
			sampler2D _PrevTex;
			float4 _MainTex_TexelSize;
			float Amplitude;
			float thickness;
			float delta;
			float TimeDelta;
			float4 dumper;
			float distrubanceSize;
			float extraRipples ;
			
			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vetx {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			vetx vert(appdata IN) {
				vetx OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}
			ENDCG

			Pass{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					float4 frag(vetx IN) : COLOR {

						//v0.2
						float2 diffUV = IN.uv - float2(diffCamPos.x, diffCamPos.z) * diffCamPos.w;
						float2 diffUV2 = IN.uv - float2(diffCamPos.x, diffCamPos.z) * diffCamPos.w*1;
						//dumper = 2;
						float2 Tex = _MainTex_TexelSize.xy;
						float4 A0 = tex2D(_MainTex, diffUV);
						float4 X1 = tex2D(_MainTex, diffUV2 + Tex * float2(-dumper.w,  0)).r;
						float4 X0 = tex2D(_MainTex, diffUV2 + Tex * float2(dumper.z,  0)).r;
						float4 Y1 = tex2D(_MainTex, diffUV2 + Tex * float2(0,  -dumper.w)).r;
						float4 Y0 = tex2D(_MainTex, diffUV2 + Tex * float2(0,   dumper.z)).r;

						float Delta = (1 / delta) * (X0 + X1 + Y0 + Y1 - 4.0 * A0.x);
						float reduce = dumper.y - dumper.x * TimeDelta;
						A0.y = (A0.y + thickness * Delta * TimeDelta) * reduce;
						A0.x = (A0.x + A0.y * TimeDelta * reduce);
						A0.x = A0.x * reduce;
						return float4(A0.x, A0.y, 0, 0);
					}
			ENDCG
			}

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 frag(vetx IN) : COLOR {

				float2 Tex = _MainTex_TexelSize.xy;			
				float3 X0 = tex2D(_MainTex, IN.uv + Tex * float2( distrubanceSize, 0   ) );
				float3 X1 = tex2D(_MainTex, IN.uv + Tex * float2(-distrubanceSize, 0   ) );
				float3 Y0 = tex2D(_MainTex, IN.uv + Tex * float2(   0, distrubanceSize ) );
				float3 Y1 = tex2D(_MainTex, IN.uv + Tex * float2(   0, -distrubanceSize) );				
				float Delta1 = Amplitude * ((X0.x - X1.x) + extraRipples * (X0.y - X1.y)) / delta;
				float Delta2 = Amplitude * ((Y0.x - Y1.x) + extraRipples * (Y0.y - Y1.y)) / delta;
				float3 norm = normalize(float3(Delta1, Delta2, 1.0));
				return float4(norm, 1.0);
			}
			ENDCG
		}

		//v0.1
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 frag(vetx IN) : COLOR {
				float2 Tex = _MainTex_TexelSize.xy;			
				float3 PrevTexTexture = tex2D(_PrevTex, IN.uv);
				float3 flowTexture = tex2D(_FlowTex, IN.uv);

				float multiplyRipples = 0.3;
				float prevTextGrey = (PrevTexTexture.r + (PrevTexTexture.r + PrevTexTexture.g + PrevTexTexture.b)/3) * multiplyRipples;

				float multiplyFlow = 0.3;//1.5;
				return clamp(float4(prevTextGrey + flowTexture.r*multiplyFlow,prevTextGrey + flowTexture.g*multiplyFlow, prevTextGrey + flowTexture.b *multiplyFlow , 2)*0.5,0,1);
			}
			ENDCG
		}

	} 
	FallBack "Diffuse"
}