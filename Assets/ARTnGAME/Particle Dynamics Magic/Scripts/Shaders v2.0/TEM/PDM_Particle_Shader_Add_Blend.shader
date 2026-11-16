// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Particle Dynamic Magic/PDM_Particle_Shader_Add_Blend" {
	Properties {
	_Color ("Edge Color", Color) = (0.5,0.5,0.5,0.5)
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_EdgeFade ("Edge Factor", Range(0.01,20.0)) = 1.0
			brightness("brightness", Vector) = (1,1,1,1)
	}
	Category
{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off
	
	SubShader {
	
	//from ParticleAlphaBlend.shader, unity shaders
	Pass {
		
		Tags { "Queue"="Transparent-6" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Off 
			//Lighting Off 
			ZWrite Off
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			float _EdgeFade;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				//#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				//#endif
			};
			
			float4 _MainTex_ST; 

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
			//	#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				//#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			fixed4 _Color;
			float2 brightness;
			
			fixed4 frag (v2f i) : COLOR
			{
				//#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				//#endif

				float4 prevv = float4(i.color.rgb * _Color.rgb, _EdgeFade * i.color.a * tex2D(_MainTex, i.texcoord).a * _Color.a);

				half4 prev =  prevv.rgba;// tex2D(_MainTex, i.texcoord).a;
				prev.rgb *= prev.a;
				return float4(prev.rgb*brightness.x,prev.a) + 0.5*_TintColor * prev.a*_TintColor.a*brightness.y;
				
				//return fixed4 (i.color.rgb * _Color.rgb, _EdgeFade * i.color.a * tex2D(_MainTex, i.texcoord).a * _Color.a);
			}
			ENDCG 
		}
	
		//from Particle AddSmooth.shader, unity shaders
		Pass {
		
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		ColorMask RGB
		Cull Off 
				//Lighting Off
				ZWrite Off
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			//	#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
			//	#endif
			};

			float4 _MainTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
			//	#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
			//	#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;			
			
			fixed4 frag (v2f i) : COLOR
			{
			//	#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
			//	#endif
				
				half4 prev = i.color * tex2D(_MainTex, i.texcoord).a   ;
				prev.rgb *= prev.a;
				return prev * _TintColor;
			}
			ENDCG 
		}
	}
  }
	//FallBack "Diffuse"
}
