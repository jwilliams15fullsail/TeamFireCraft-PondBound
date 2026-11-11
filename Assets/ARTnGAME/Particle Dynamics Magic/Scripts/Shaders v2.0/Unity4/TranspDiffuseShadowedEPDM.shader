Shader "Particle Dynamic Magic/Transparent/Diffuse with Shadow E" { 

	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		//Pass {
		
		Tags { 
	
		
		"Queue"="Geometry+1"
		"RenderType"="Transparent" 

		}

		
		CGPROGRAM
		#pragma surface surf Lambert decal:blend fullforwardshadows
		#pragma target 2.0
		
		

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
o.Albedo = tex.rgb * _Color.rgb;

o.Alpha = tex.a * _Color.a;


		}
		ENDCG
		
	}

Fallback "VertexLit"
}
