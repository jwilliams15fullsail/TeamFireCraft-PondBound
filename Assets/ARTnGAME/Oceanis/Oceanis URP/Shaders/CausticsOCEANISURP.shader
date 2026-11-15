Shader "ARTnGAME/Oceanis/CausticsURP"
{
    Properties
    {
		debugDepth("Debug Depth",Float) = 0
		waterHeight("Water Height",Float) = 0
        _CausticTex("Caustics Texture", 2D) = "white" {}
		_CausticDepthFade("Caustics Depth Fade", Vector) = (0,0,0,0)
		_CausticsPower("Caustics Power", Range(-1.0, 100.0)) = 0.1
		_CausticsAngle("Caustics Abberation Angle", Range(0.0, 0.5)) = 0.0
		_CausticsSize("Caustics Size", Float) = 2.0
		_CausticsVelocity("Caustics Velocity", Range(0.0, 0.5)) = 0.1
		_CausticsLightMasking("Caustics Light Masking", Range(-0.2, 0.2)) = 0.0
		_CausticsFadeSize("_Caustics Fade Size", Range(-1.0, 1.0)) = 0.5
		_CausticsFadePower("Caustics Fade Power", Range(0.5, 1.0)) = 1.0

			_BumpDirection("_Bump Direction", Vector) = (0,1,0,0)
			_BumpTiling("_Bump Tiling", Vector) = (1,1,1,1)
			_DistortParams("_DistortParams", Vector) = (1,1,1,1)
				_BumpMap("Normals ", 2D) = "bump" {}

		cameraFade("cameraFade", Vector) = (2000,1.2,1,1)

			//v1.9 - ripples
		ripplesTex("ripples Texture", 2D) = "white" {}
		ripplesPos("Decal position X-Z - rotation (deg) - scale", Float) = (0, 0, 0, 1)
			rippleWaveParams("ripple Wave Params", Vector) = (1, 1, 1, 1)
	}
		SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Transparent"
			"Queue" = "Transparent+1"
		}
		Pass
		{
			Cull Front
			ZTest Always
			ZWrite Off
			Blend One One

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
				
			//v1.9
	float4 ripplesPos;
sampler2D ripplesTex;

			//v0.2
			float4 cameraFade;

			//v0.1
			sampler2D _BumpMap;
			//TEXTURE2D(_BumpMap);
			//SAMPLER(sampler_BumpMap);
			float4 _BumpDirection; 
			float4 _DistortParams;
			float4 _BumpTiling;
			//v0.1
			half4x4 _MainLightDirection;
			TEXTURE2D(_CausticTex);
			SAMPLER(sampler_CausticTex);

			CBUFFER_START(UnityPerMaterial)
			float waterHeight;
			float2 _CausticDepthFade;
			half _CausticsSize;
			half _CausticsVelocity;
			half _CausticsAngle;
			half _CausticsLightMasking;
			half _CausticsPower;
			float _CausticsFadeSize;
			float _CausticsFadePower;
			CBUFFER_END
			float debugDepth;

			struct Attributes
			{
				float4 positionOS : POSITION;
				float4 bumpCoords : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 bumpCoords : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};	
			//v0.1
			half2 Scroller(half2 uv, half speed, half tiling)
			{
				return uv * tiling + half2(1, 0) * _Time.y * speed;
			}
			//v0.1
			half3 DoCaustics(half2 uv, half split)
			{
				half2 uv1 = uv + half2(split, split);
				half2 uv2 = uv + half2(split, -split);
				half2 uv3 = uv + half2(-split, -split);
				half r = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv1).r;
				half g = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv2).g;
				half b = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv3).b;
				return half3(r, g, b);
			}
			//v0.1
			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
				
				half2 tileableUv = mul(unity_ObjectToWorld, (IN.positionOS)).xz;
				OUT.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;
				return OUT;
			}	

			//v0.1
			half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
			{
				half3 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)) + UnpackNormal(tex2D(bumpMap, coords.zw))) * 0.5;
				half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1, 0, 1);
				return normalize(worldNormal);
			}
			float4 frag(Varyings IN) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				half3 worldNormal = PerPixelNormal(_BumpMap, IN.bumpCoords, float3(0, 1, 0), _DistortParams.x);

				//return float4(worldNormal* worldNormal * worldNormal, 1);


			



					
				half4 distortOffset = half4(worldNormal.xz * _DistortParams.y * 10.0 , 0, 0);
				
				//float2 positionNDC = (IN.positionCS.xy) / _ScaledScreenParams.xy;
				float2 positionNDC = (IN.positionCS.xy ) / _ScaledScreenParams.xy;

				//return float4(positionNDC,1, 1);

				half2 grabWithOffset = IN.positionCS.xy + distortOffset*10;// i.grabPassPos + distortOffset;

				//return float4(grabWithOffset/1011, 1, 1);

				float2 samplingPositionNDC = float2(grabWithOffset.xy / _ScaledScreenParams.xy).xy;
				positionNDC = samplingPositionNDC;

				//return float4(positionNDC / 1, 1, 1);

				#if UNITY_REVERSED_Z
					float depth = SampleSceneDepth(positionNDC);
				#else
					float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(positionNDC));
				#endif
				float3 positionWS = ComputeWorldSpacePosition(positionNDC, depth, UNITY_MATRIX_I_VP);
				//return float4(positionWS / 111, 1);
				//positionWS = positionWS / 100;


					//v1.9 - RIPPLES
				float3 CamPosAA = float3(ripplesPos.x, 0, ripplesPos.y);// _DepthCameraPos;//_WorldSpaceCameraPos;		
				float2 OriginA = float2(CamPosAA.x - ripplesPos.w / 2, CamPosAA.z - ripplesPos.w / 2);
				float2 UnscaledTexPointA = float2(positionWS.x - OriginA.x, positionWS.z - OriginA.y);
				float2 newcoordsB = float2(UnscaledTexPointA.x / (ripplesPos.w), UnscaledTexPointA.y / (ripplesPos.w));
				//float2 newcoordsB = ScaledTexPoint; //move to center		
				float4 ripplesHeight = tex2D(ripplesTex, float2((newcoordsB)));
				if (newcoordsB.x > 1 || newcoordsB.x < 0 || newcoordsB.y > 1 || newcoordsB.y < 0) {
					ripplesHeight = 0;
				}
				float heightA = clamp(430101 * pow(ripplesHeight.r, 15) * 1, 0, 25);


				
				if (debugDepth == 1) {
					float4 color = float4(frac(positionWS), 1.0);
#if UNITY_REVERSED_Z
					if (depth < 0.0001) return float4(0, 0, 0, 1);
#else
					if (depth > 0.9999) return float4(0, 0, 0, 1);
#endif
					return color ;
				}
				else {
					float3 positionOS = TransformWorldToObject(positionWS);

					//return float4(positionOS / 1, 1);

					float boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));					
					float2 uv = mul(positionWS, _MainLightDirection).xz; //positionWS.xz;// mul(positionWS, _MainLightDirection).xy;
					//half4 caustics = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv);
					//half2 moving_uv = Scroller(uv, _CausticsVelocity, 1 / _CausticsSize);
					float2 uv1 = Scroller(uv, 0.75 * _CausticsVelocity, 1 / _CausticsSize);
					float2 uv2 = Scroller(uv, 1 * _CausticsVelocity, -1 / _CausticsSize);

					//return float4(uv2 / 1111,1, 1);

					//half4 tex1 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv1);
					//half4 tex2 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv2);
					float3 tex1 = DoCaustics(uv1/1, _CausticsAngle);
					float3 tex2 = DoCaustics(uv2, _CausticsAngle);
					//tex1 = tex1 - tex2;
					//half3 rA = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv/111);
					//return float4(rA, 1);

					float3 caustics = _CausticsPower * min(tex1, tex2);


					//v1.9
					if (heightA.r > 1) {
						caustics = saturate(caustics + caustics * heightA * 1*0.4);
					}


					float3 sceneColor = SampleSceneColor(positionNDC);
					float sceneLuminance = clamp(Luminance(sceneColor),0,10);
					//half luminanceMask = lerp(1, sceneLuminance, _CausticsLightMasking);
					
					
					//float luminanceMask = smoothstep(_CausticsLightMasking, _CausticsLightMasking + 0.1, sceneLuminance);
					//float luminanceMask = (10 * smoothstep(_CausticsLightMasking, _CausticsLightMasking + 1.4, sceneLuminance));
					float luminanceMask = (11 * smoothstep(_CausticsLightMasking, _CausticsLightMasking + 1.1, sceneLuminance));

					float edgeFadeMask = 1 - saturate((distance(positionOS, 0) - _CausticsFadeSize) / (1 - _CausticsFadePower));
					
					//float depthFade = _CausticDepthFade.x * pow(depth*100, _CausticDepthFade.y);// _CausticDepthFade.x * pow(depth, _CausticDepthFade.y);
					float depthFade = (_CausticDepthFade.x * pow(1-abs(waterHeight-positionWS.y)*0.000001, _CausticDepthFade.y*10000) - 0.5);// _CausticDepthFade.x * pow(depth, _CausticDepthFade.y);

					//return float4(1, 0, 0, 1);
					float camDist = length(_WorldSpaceCameraPos - positionWS);
					float cameraFadeA = cameraFade.x/pow(camDist, cameraFade.y);

					caustics *= luminanceMask * boundingBoxMask * edgeFadeMask * depthFade * cameraFadeA;
					return saturate(float4(caustics, 1.0));
				}
			}
            ENDHLSL
        }
    }
}
//Based on 
//TUTORIAL - https://alexanderameye.github.io/notes/realtime-caustics/
//TUTORIAL - https://www.alanzucconi.com/2019/09/13/believable-caustics-reflections/

//
//Pass
//{
//	Cull Front
//	ZTest Always
//	ZWrite Off
//	Blend One One
//
//	HLSLPROGRAM
//	#pragma vertex vert
//	#pragma fragment frag
//	#pragma multi_compile_fog
//	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
//	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
//	//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
//
//	//v0.1
//	sampler2D _BumpMap;
////TEXTURE2D(_BumpMap);
////SAMPLER(sampler_BumpMap);
//float4 _BumpDirection;
//float4 _DistortParams;
//float4 _BumpTiling;
////v0.1
//half4x4 _MainLightDirection;
//TEXTURE2D(_CausticTex);
//SAMPLER(sampler_CausticTex);
//
//CBUFFER_START(UnityPerMaterial)
//float waterHeight;
//float2 _CausticDepthFade;
//half _CausticsSize;
//half _CausticsVelocity;
//half _CausticsAngle;
//half _CausticsLightMasking;
//half _CausticsPower;
//float _CausticsFadeSize;
//float _CausticsFadePower;
//CBUFFER_END
//float debugDepth;
//
//struct Attributes
//{
//	float4 positionOS : POSITION;
//	float4 bumpCoords : TEXCOORD0;
//	UNITY_VERTEX_INPUT_INSTANCE_ID
//};
//struct Varyings
//{
//	float4 positionCS : SV_POSITION;
//	float4 bumpCoords : TEXCOORD0;
//	UNITY_VERTEX_OUTPUT_STEREO
//};
////v0.1
//half2 Scroller(half2 uv, half speed, half tiling)
//{
//	return uv * tiling + half2(1, 0) * _Time.y * speed;
//}
////v0.1
//half3 DoCaustics(half2 uv, half split)
//{
//	half2 uv1 = uv + half2(split, split);
//	half2 uv2 = uv + half2(split, -split);
//	half2 uv3 = uv + half2(-split, -split);
//	half r = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv1).r;
//	half g = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv2).g;
//	half b = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv3).b;
//	return half3(r, g, b);
//}
////v0.1
//Varyings vert(Attributes IN)
//{
//	Varyings OUT;
//	UNITY_SETUP_INSTANCE_ID(IN);
//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
//	OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
//
//	half2 tileableUv = mul(unity_ObjectToWorld, (IN.positionOS)).xz;
//	OUT.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;
//	return OUT;
//}
//
////v0.1
//half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
//{
//	half3 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)) + UnpackNormal(tex2D(bumpMap, coords.zw))) * 0.5;
//	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1, 0, 1);
//	return normalize(worldNormal);
//}
//float4 frag(Varyings IN) : SV_Target
//{
//	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
//
//	half3 worldNormal = PerPixelNormal(_BumpMap, IN.bumpCoords, float3(0, 1, 0), _DistortParams.x);
//	half4 distortOffset = half4(worldNormal.xz * _DistortParams.y * 10.0 , 0, 0);
//
//	//float2 positionNDC = (IN.positionCS.xy) / _ScaledScreenParams.xy;
//	float2 positionNDC = (IN.positionCS.xy) / _ScaledScreenParams.xy;
//
//	half2 grabWithOffset = IN.positionCS.xy + distortOffset * 10;// i.grabPassPos + distortOffset;
//	float2 samplingPositionNDC = float2(grabWithOffset.xy / _ScaledScreenParams.xy).xy;
//	positionNDC = samplingPositionNDC;
//
//	#if UNITY_REVERSED_Z
//		float depth = SampleSceneDepth(positionNDC);
//	#else
//		float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(positionNDC));
//	#endif
//	float3 positionWS = ComputeWorldSpacePosition(positionNDC, depth, UNITY_MATRIX_I_VP);
//
//	if (debugDepth == 1) {
//		float4 color = float4(frac(positionWS), 1.0);
//#if UNITY_REVERSED_Z
//					if (depth < 0.0001) return float4(0, 0, 0, 1);
//#else
//					if (depth > 0.9999) return float4(0, 0, 0, 1);
//#endif
//					return color;
//				}
//				else {
//					float3 positionOS = TransformWorldToObject(positionWS);
//					float boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));
//					float2 uv = mul(positionWS, _MainLightDirection).xy;
//					//half4 caustics = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv);
//					//half2 moving_uv = Scroller(uv, _CausticsVelocity, 1 / _CausticsSize);
//					float2 uv1 = Scroller(uv, 0.75 * _CausticsVelocity, 1 / _CausticsSize);
//					float2 uv2 = Scroller(uv, 1 * _CausticsVelocity, -1 / _CausticsSize);
//					//half4 tex1 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv1);
//					//half4 tex2 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, uv2);
//					float3 tex1 = DoCaustics(uv1, _CausticsAngle);
//					float3 tex2 = DoCaustics(uv2, _CausticsAngle);
//					float3 caustics = _CausticsPower * min(tex1, tex2);
//					float3 sceneColor = SampleSceneColor(positionNDC);
//					float sceneLuminance = clamp(Luminance(sceneColor),0,10);
//					//half luminanceMask = lerp(1, sceneLuminance, _CausticsLightMasking);
//
//
//					//float luminanceMask = smoothstep(_CausticsLightMasking, _CausticsLightMasking + 0.1, sceneLuminance);
//					//float luminanceMask = (10 * smoothstep(_CausticsLightMasking, _CausticsLightMasking + 1.4, sceneLuminance));
//					float luminanceMask = (11 * smoothstep(_CausticsLightMasking, _CausticsLightMasking + 1.1, sceneLuminance));
//
//					float edgeFadeMask = 1 - saturate((distance(positionOS, 0) - _CausticsFadeSize) / (1 - _CausticsFadePower));
//
//					//float depthFade = _CausticDepthFade.x * pow(depth*100, _CausticDepthFade.y);// _CausticDepthFade.x * pow(depth, _CausticDepthFade.y);
//					float depthFade = (_CausticDepthFade.x * pow(1 - abs(waterHeight - positionWS.y) * 0.000001, _CausticDepthFade.y * 10000) - 0.5);// _CausticDepthFade.x * pow(depth, _CausticDepthFade.y);
//
//
//					caustics *= luminanceMask * boundingBoxMask * edgeFadeMask * depthFade;
//					return saturate(float4(caustics, 1.0));
//				}
//			}
//			ENDHLSL
//}