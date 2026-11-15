// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ParallaxOcclusion_VERT_FRAG_URP" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal map (RGB)", 2D) = "bump" {}
		_OcclusionMap("_Occlusion Map (R)", 2D) = "white" {}
		_BumpScale ("Bump scale", Range(0,1)) = 1
		_ParallaxMap ("Height map (R)", 2D) = "white" {}
		_Parallax ("Height scale", Range(-1,1)) = 0.05
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_ParallaxMinSamples ("Parallax min samples", Range(2,100)) = 4
		_ParallaxMaxSamples ("Parallax max samples", Range(2,100)) = 20

		//LIGHT
		skyPower("Sky Power", Float) = 1
		occlusionPower("occlusion Power", Float) = 1
		lightPower("light Power", Float) = 1

		scaleTexture("scale Texture", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass{
		CGPROGRAM
		//#pragma surface surf Standard fullforwardshadows vertex:vert
		#include "UnityCG.cginc" //VR1
		#pragma vertex vert  
		#pragma fragment frag 
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _ParallaxMap;
		sampler2D _OcclusionMap;

		/*struct Input {
			float2 texcoord;
			float3 eye;
			float sampleRatio;
		};*/

		half _Glossiness;
		half _Metallic;
		half _BumpScale;
		half _Parallax;
		fixed4 _Color;
		uint _ParallaxMinSamples;
		uint _ParallaxMaxSamples;

		//LIGHT
		float skyPower;
		float occlusionPower;
		float lightPower;
		float scaleTexture;

#include "UnityLightingCommon.cginc"
		#include<ParallaxOcclusion.cginc>

		/*void vert(inout appdata_full IN, out Input OUT) {
			parallax_vert(IN.vertex, IN.normal, IN.tangent, OUT.eye, OUT.sampleRatio);
			OUT.texcoord = IN.texcoord;
		}*/
		struct vertexInput {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float2 texcoord: TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
		};
		struct vertexOutput {
			float4 pos : SV_POSITION;
			float2 texcoord: TEXCOORD0;
			float3 eye: TEXCOORD1;
			float sampleRatio : TEXCOORD2;
			//float4 posProj : TEXCOORD0; 
			// position in projector space

			//LIGHT
			//half3 worldRefl : TEXCOORD3;
			float3 worldPos : TEXCOORD4;
			half3 worldNormal : TEXCOORD5;

			// these three vectors will hold a 3x3 rotation matrix
			   // that transforms from tangent to world space
			half3 tspace0 : TEXCOORD6; // tangent.x, bitangent.x, normal.x
			half3 tspace1 : TEXCOORD7; // tangent.y, bitangent.y, normal.y
			half3 tspace2 : TEXCOORD8; // tangent.z, bitangent.z, normal.z
			// texture coordinate for the normal map
			//float2 uv : TEXCOORD4;

			UNITY_VERTEX_OUTPUT_STEREO //VR1
		};
		vertexOutput vert(vertexInput input)
		{
			vertexOutput output;

			UNITY_SETUP_INSTANCE_ID(input); //v1.7.8 //VR1
			UNITY_INITIALIZE_OUTPUT(vertexOutput, output); //Insert //VR1
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); //Insert //VR1		

			parallax_vert(input.vertex, input.normal, input.tangent, output.eye, output.sampleRatio);
			output.texcoord = input.texcoord;
			output.pos = UnityObjectToClipPos(input.vertex);

			//LIGHT
			//https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
			float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
			// compute world space view direction
			float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
			// world space normal
			//float3 worldNormal = UnityObjectToWorldNormal(input.normal);
			// world space reflection vector
			//output.worldRefl = reflect(-worldViewDir, worldNormal);

			//LIGHT2
			half3 wNormal = UnityObjectToWorldNormal(input.normal);
			output.worldPos = worldPos;
			output.worldNormal = wNormal;

			//LIGHT3
			half3 wTangent = UnityObjectToWorldDir(input.tangent.xyz);
			// compute bitangent from cross product of normal and tangent
			half tangentSign = input.tangent.w * unity_WorldTransformParams.w;
			half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
			// output the tangent space matrix
			output.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
			output.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
			output.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

			//output.posProj = mul(unity_Projector, input.vertex);
			//output.pos = UnityObjectToClipPos(input.vertex);
			return output;
		}

		float4 frag(vertexOutput IN) : COLOR//void surf(Input IN, inout SurfaceOutputStandard o) {
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);//VR1

			float2 offset = parallax_offset(_Parallax * 0.001, IN.eye, IN.sampleRatio, IN.texcoord,
			_ParallaxMap, _ParallaxMinSamples, _ParallaxMaxSamples);
			float2 uv = IN.texcoord * scaleTexture + offset;
			float4 c = tex2D(_MainTex, uv) * _Color;


			//LIGHT2
			// compute view direction and reflection vector
			// per-pixel here
			//half3 worldViewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));
			//half3 worldRefl = reflect(-worldViewDir, IN.worldNormal);

			//LIGHT3
			 // sample the normal map, and decode from the Unity encoding
			half3 tnormal = UnpackNormal(tex2D(_BumpMap, uv));// *_BumpScale;
			tnormal.xy *= _BumpScale;
			// transform normal from tangent to world space
			half3 worldNormal;
			worldNormal.x = dot(IN.tspace0, tnormal);
			worldNormal.y = dot(IN.tspace1, tnormal);
			worldNormal.z = dot(IN.tspace2, tnormal);
			// rest the same as in previous shader
			half3 worldViewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));
			half3 worldRefl = reflect(-worldViewDir, worldNormal);

			//LIGHT
			//https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
			// sample the default reflection cubemap, using the reflection vector
			half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// IN.worldRefl);
			// decode cubemap data into actual color
			half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
			// output it!			
			c.rgb = c.rgb+c.rgb*skyColor*skyPower;

			//OCCLUSION
			float occlusion = tex2D(_OcclusionMap, uv).r;
			c.rgb =  c.rgb*occlusionPower*occlusion;

			//LIGHT4
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			float4 diff = nl * _LightColor0;
			//diff.rgb += ShadeSH9(half4(worldNormal, 1));
			c.rgb = c.rgb*diff*lightPower;

			//o.Albedo = c.rgb;
			//o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			//o.Alpha = c.a;
			return c;
		}

		ENDCG
			}
	}
	//FallBack "Diffuse"
}
