// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Particle Dynamic Magic/AdditiveBlendMix" {
	Properties {
		_MainTex ("Base", 2D) = "white" {}
		_BorderTex ("Border", 2D) = "white" {}
		_TintColor ("TintColor", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

sampler2D _BorderTex;
half4 _BorderTex_ST;

		sampler2D _MainTex;
		fixed4 _TintColor;
		half4 _MainTex_ST;
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
			fixed4 vertexColor : COLOR;
		};

		v2f vert(appdata_full v) {
			v2f o;
			
			o.pos = UnityObjectToClipPos (v.vertex);	
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			
			o.uv1 = TRANSFORM_TEX(v.texcoord, _BorderTex);
			
			o.vertexColor = v.color * _TintColor;
					
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR {	
		
		float4 ColorFinal = tex2D(_MainTex, i.uv.xy )* tex2D(_BorderTex, i.uv1.xy) * i.vertexColor;
			return  ColorFinal ;
		}
	
	ENDCG
	
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha One
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}
