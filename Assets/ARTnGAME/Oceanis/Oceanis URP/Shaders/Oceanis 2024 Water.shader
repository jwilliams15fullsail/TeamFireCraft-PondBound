// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SkyMaster/Oceanis 2024 Water" { 	//v2.0
Properties {
	_ReflectionTex ("Internal reflection", 2D) = "white" {}
	
	//TERRAIN DEPTH
	_DepthCameraPos ("Depth Camera Pos", Vector) = (240 ,100, 250, 3.0)	
	_ShoreContourTex ("_ShoreContourTex", 2D) = "white" {}
	_ShoreFadeFactor ("_ShoreFadeFactor", Float) = 1.0
	_TerrainScale ("Terrain Scale", Vector) = (1.0 ,1.0, 1.0, 1.0)
		_WorldScale("_WorldScale", Float) = 2210
		cameraOffset("cameraOffset", Float) = -1
		offsetScale("offsetScale", Vector) = (0,0,0,0)
		offsetRflect("offsetRflect", Vector) = (0,0,0,0)
	
	_MainTex ("Fallback texture", 2D) = "black" {}
	_ShoreTex ("Shore & Foam texture ", 2D) = "black" {}
	_BumpMap ("Normals ", 2D) = "bump" {}
	
	_DistortParams ("Distortions (Bump waves, Reflection, Fresnel power, Fresnel bias)", Vector) = (1.0 ,1.0, 2.0, 1.15)
	_InvFadeParemeter ("Auto blend parameter (Edge, Shore, Distance scale)", Vector) = (0.15 ,0.15, 0.5, 1.0)
	
	_AnimationTiling ("Animation Tiling (Displacement)", Vector) = (2.2 ,2.2, -1.1, -1.1)
	_AnimationDirection ("Animation Direction (displacement)", Vector) = (1.0 ,1.0, 1.0, 1.0)

	_BumpTiling ("Bump Tiling", Vector) = (1.0 ,1.0, -2.0, 3.0)
	_BumpDirection ("Bump Direction & Speed", Vector) = (1.0 ,1.0, -1.0, 1.0)
	
	_FresnelScale ("FresnelScale", Range (0.15, 14.0)) = 0.75
	FresnelFactor("FresnelFactor", Range(0.01, 1.5)) = 0.25 //v0.6

	_BaseColor ("Base color", COLOR)  = ( .54, .95, .99, 0.5)
	_ReflectionColor ("Reflection color", COLOR)  = ( .54, .95, .99, 0.5)
	_SpecularColor ("Specular color", COLOR)  = ( .72, .72, .72, 1)
	
	_WorldLightDir ("Specular light direction", Vector) = (0.0, 0.1, -0.5, 0.0)
	_Shininess ("Shininess", Range (2.0, 500.0)) = 200.0
	
	_Foam ("Foam (intensity, cutoff)", Vector) = (0.1, 0.375, 1.0, 0.0)
	
	_GerstnerIntensity("Per vertex displacement", Float) = 1.0
	_GAmplitude ("Wave Amplitude", Vector) = (0.3 ,0.35, 0.25, 0.25)
	_GFrequency ("Wave Frequency", Vector) = (1.3, 1.35, 1.25, 1.25)
	_GSteepness ("Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
	_GSpeed ("Wave Speed", Vector) = (1.2, 1.375, 1.1, 1.5)
	_GDirectionAB ("Wave Direction", Vector) = (0.3 ,0.85, 0.85, 0.25)
	_GDirectionCD ("Wave Direction", Vector) = (0.1 ,0.9, 0.5, 0.5)
	
	_MultiplyEffect  ("Multiply Effect", Float) = 1
	
	_GerstnerIntensity1("Extra_GerstnerIntensity1", Float) = 0.0
	_GerstnerIntensity2("Extra_GerstnerIntensity2", Float) = 0.0
	
	_GerstnerIntensities ("Extra Wave Inclusion controls", Vector) = (0 ,0, 0, 0)
	_Gerstnerfactors ("Extra Wave Frequency controls", Vector) = (1 ,1, 1, 1)
	_Gerstnerfactors2 ("Extra Wave Amplitude controls", Vector) = (1 ,1, 1, 1)
	_GerstnerfactorsDir ("Extra Wave Direction controls", Vector) = (1 ,1, 1, 1)
	_GerstnerfactorsDirA("Extra Wave Direction A controls", Vector) = (1 ,1, 1, 1)
	_GerstnerfactorsSteep ("Extra Wave Steepness controls", Vector) = (1 ,1, 1, 1)
	_GerstnerfactorsSpeed("Extra Wave Speed controls", Vector) = (1 ,1, 1, 1)
	
	_LocalWaveVelocity ("Local Wave Velocity", Vector) = (0.1 ,0.9, 0.5, 0.5)
	_LocalWavePosition ("Local Wave Position", Vector) = (0.0 ,0.0, 0.0, 0.0)
	_LocalWaveParams ("Local Wave Height cutoff, Amp,Freq,Unaffected region", Vector) = (6 ,270, 10, 1)

	fogColor  ("Fog color", COLOR)  = ( .7, .7, .82, 1)
	fogDepth ("Water fog depth", Float) = 0.0
	fogBias ("Water fog bias", Float) = 2.0
	fogThres ("Water fog start distance", Float) = 100

	isUnderwater("Turn on when Underwater", Float) = 0 //1 == underwater
	depthFade("Fade when over water", Float) = 0 //1 == underwater
	waterHeight("WaterPlaneHeight", Float) = 0 //1 == underwater
		waterHeightFogOffset("water Height Fog Offset", Float) = 0

											   //v3.4.4
		foamContrast("Lower Foam Contrast",  Range(-0.5, 0.5)) = 0
		_foamColor("Foam Color", COLOR) = (0.15, 0.03, 0.01, 0)

		//v0.5
		sssFactor("Translucency Factor", Float) = 1
		sssBackSideFactor("Opposite Sun Side Brightness", Float) = 1
		_sssColor("SSS Color", COLOR) = (1, 1, 1, 0)
		_sssParams("SSS Params", Vector) = (1, 1, 1, 1)

		//HDRP
		_controlReflect("Control Reflection (Power,Distort,Downscale)", Vector) = (20, 5, 0.5, 0) //(1 ,1, 0, 0) v0.4a
		_localLightAPos("Local Light A pos and power", Vector) = (0 ,0, 0, 0) //pos.xyz and power
		_localLightAProps("Local Light A color", Vector) = (0 ,0, 0, 0) //color and range
		offsetTopWaterLight("Offset Top Water Light", Float) = 0 //
		_regulateShoreWavesHeight("Regulate Shore Waves Height", Float) = 0 //
		_regulateShoreWavesHeightA("Regulate Shore Waves Height A", Float) = 0 //
		_regulateShoreWavesHeightB("Regulate Shore Waves Height B", Float) = 0 //
		_regulateShoreWavesHeightC("Regulate Shore Waves Height B", Float) = 0 //
		//_localLightRange("Local Light range", Float) = 0.0 //

		//v4.7 Rain ripples
		_Lux_RainRipples("_Lux_RainRipples", 2D) = "white" {}
		_Lux_RippleWindSpeed("_Lux_RippleWindSpeed", Float) = (0, 0, 0, 0)
		_Lux_WaterFloodlevel("_Lux_WaterFloodlevel", Float) = (0, 0, 0, 0)
		_Lux_RainIntensity("_Lux_RainIntensity", Float) = 0.75
		_Lux_RippleAnimSpeed("_Lux_RippleAnimSpeed", Float) = 0.75
		_Lux_RippleTiling("_Lux_RippleTiling", Float) = 0.75
		_Lux_WaterBumpDistance("_Lux_WaterBumpDistance", Float) = 0.75
		//END v4.7 Rain ripples


		//FFT
		_RampTex("_RampTex", 2D) = "white" {}
		_TilingFFT("FFT Waves Tiling", Float) = 141
		_PowerFFT("FFT Waves Power", Float) = 21
		_GlobalPowerFFT("Global FFT Power", Float) = 1
		_PowerFFT1Y("FFT 1 Y Waves Power", Float) = -14
		_PowerFFT2Y("FFT 2 Y Waves Power", Float) = 2
		_NormalTex ("FFT Normals texture", 2D) = "black" {}

		//v0.7
		shoreWaveControl("ShorePower, Slide, EdgeLow, EdgeHigh", Float) = (0, 1, 1, 1)
		shoreWaveControlB("Shore Speed, Steep, Amplitude, Freq", Float) = (1, 1, 1, 1)
		shoreGlow("Shore lighting, Height, Divider, Power", Float) = (1, 1, 1, 1)
		foamHighSea("High sea foam control", Float) = (0, 0, 0, 0)
		shoreWaves("Shore extra waves and FFT (w)", Float) = (1, 1, 0, 0)
		//shoreWaves("Shore waves FFT", Float) = (1, 1, 0, 1)
		depthBelowZero("Depth rendered Below Zero", Float) = 8

		//v0.8
		_fluidTexture("Fluid Texture", 2D) = "white" {}
		_fluidTextureADVECT("Fluid Texture", 2D) = "white" {}
		dynamicFoamControls("dynamicFoamControls", Float) = (1, 1, 1, 1)
		dynamicFoamControlsA("dynamicFoamControlsA", Float) = (1000, 1, 1, 1)

			//PROJECTED GRID
			//_Map0("_Map0", 2D) = "white" {}
	//	_Map1("_Map1", 2D) = "white" {}
	//	_Map2("_Map2", 2D) = "white" {}
		//_Map3("_Map3", 2D) = "white" {}
		//_Map4("_Map4", 2D) = "white" {}
		controlProjection("Control Projection", Float) = (1, 1, 1, 1)

		//v1.1 - Bruneton sky radiance
		_controlSkyRadiance("Control Sky Radiance, base, sky, normal, tiling", Float) = (1, 0, 1, 1)
		_controlSkyRadianceA("Control Sky Radiance, base, sky, normal, tiling", Float) = (1, 1, 1, 1)
			_controlSkyColor("Control Sun color", COLOR) = (1, 1, 1, 0)
			cameraNearClip("Camera Near Clip Plane", Float) = 0.1
			cameraNearColor("Control air line color", COLOR) = (0.2, 0.4, 0.6, 1)
			cameraNearControl("Control black line", Float) = (1, 1, 1, 1)

			DepthPyramidScale("Depth Pyramid Scale", Float) = (1, 0.67, 0, 0)

			//v1.2 - local water - no projected grid
			enableProjectedGrid("Enable projected grid mode", Int) = 1 //enabled by default

			//v1.9 - ripples
			ripplesTex("ripples Texture", 2D) = "white" {}
			ripplesPos("ripples position X-Z - rotation (deg) - scale", Vector) = (0, 0, 0, 1)
			rippleWaveParams("ripple Wave Params", Vector) = (1, 1, 1, 1)

			//v1.9a
			RippleFoamAdjust("Ripples Foam Adjust", Vector) = (1, 1, 1, 1)
			RippleSpecularAdjust("Ripples Specular Adjust", Vector) = (1, 1, 1, 1)

			//v1.3 - decals
			decalPosRot("Decal position X-Z - rotation (deg) - scale", Float) = (0, 0, 0, 1)
			decalEdgeTweak("Decal Edge tweak", Float) = (-0.5, 0.5, 0.5, 0.5)
			decalPower("Decal power", Float) = (1, 1, 1, 1)

			//v1.4 - Vortex
			vortexPosScale("Vortex position - scale", Float) = (0, 0, 0, 1)
			_InteractAmpFreqRad("_InteractAmpFreqRadial", Vector) = (1, 1, 1,1)

			//v1.5 - PRO - FULL FFT
			_Color("Color", Color) = (1, 1, 1, 1)
			_SSSColor("SSS Color", Color) = (1, 1, 1, 1)
			_SSSStrength("SSSStrength", Range(0, 1)) = 0.2
			_SSSScale("SSS Scale", Range(0.1, 50)) = 4.0
			_SSSBase("SSS Base", Range(-5, 1)) = 0
			_LOD_scale("LOD_scale", Range(1, 10)) = 0
			_MaxGloss("Max Gloss", Range(0, 1)) = 0
			_Roughness("Distant Roughness", Range(0, 1)) = 0
			_RoughnessScale("Roughness Scale", Range(0, 0.01)) = 0.1
			_FoamColor("Foam Color", Color) = (1, 1, 1, 1)
			_FoamTexture("Foam Texture", 2D) = "grey" {}
			_FoamBiasLOD0("Foam Bias LOD0", Range(0, 7)) = 1
			_FoamBiasLOD1("Foam Bias LOD1", Range(0, 7)) = 1
			_FoamBiasLOD2("Foam Bias LOD2", Range(0, 7)) = 1
			_FoamScale("Foam Scale", Range(0, 20)) = 1
			_ContactFoam("Contact Foam", Range(0, 1)) = 1
			[Header(Cascade 0)]
			_Displacement_c0("Displacement C0", 2D) = "black" {}
			_Derivatives_c0("Derivatives C0", 2D) = "black" {}
			_Turbulence_c0("Turbulence C0", 2D) = "white" {}
			[Header(Cascade 1)]
			_Displacement_c1("Displacement C1", 2D) = "black" {}
			_Derivatives_c1("Derivatives C1", 2D) = "black" {}
			_Turbulence_c1("Turbulence C1", 2D) = "white" {}
			[Header(Cascade 2)]
			_Displacement_c2("Displacement C2", 2D) = "black" {}
			_Derivatives_c2("Derivatives C2", 2D) = "black" {}
			_Turbulence_c2("Turbulence C2", 2D) = "white" {}

			//lengthScales("length Scales", Vector) = (1, 1, 1, 1)

			//v1.6
			depthFadePower("depth Fade Power", Float) = -1

			//v1.7
			_Displace_T3D("Displace 3D Texture", 3D) = "" {}
			_Normals_T3D("Normals 3D Texture", 3D) = "" {}
			_NormalsOffset3DControls("_NormalsOffset3DControls", Vector) = (1, 1, 1, 1)

			//v1.8
			waterAirMode("water Air Line Mode", Float) = 0
			NearClipAdjust("Near Clip Adjust", Vector) = (2, 0.8, 3, 1)

			//v1.9b
			autoLOD("auto LOD waves 0 is off, 1 is on", Float) = 0
}

HLSLINCLUDE
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/ShaderLibrary/TextureXR.hlsl"

//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/API/D3D11.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//ENDHLSL

//CGINCLUDE

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "./WaterIncludeSM30.cginc"

//v1.1
//#include "Assets/ARTnGAME/Oceanis/Oceanis HDRP/Scripts/Sky Module/BrunetonsAtmosphere/Shaders/Atmosphere.cginc"
#include "Atmosphere.cginc"

//FFT
//#include "./WaterIncludeSM31FFT.cginc"

//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl" 
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/API/GLCore.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderConfig.cs.hlsl"  
//	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/TextureXR.hlsl"
	//#define LOAD_TEXTURE2D_X_LOD(textureName, unCoord2, lod)  LOAD_TEXTURE2D_ARRAY_LOD(textureName, unCoord2, SLICE_ARRAY_INDEX, lod)	
	//#define LOAD_TEXTURE2D_ARRAY_LOD(textureName, unCoord2, index, lod) textureName.Load(int4(unCoord2, index, lod))
	//#define LOAD_TEXTURE2D_X_LOD(textureName, unCoord2, index, lod) textureName.Load(int4(unCoord2, index, lod))
	//#define TEXTURE2D_ARRAY(textureName)            Texture2DArray textureName //com.unity.render-pipelines.core/ShaderLibrary/API/GLCore.hlsl
//	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.hlsl"
	//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.cs.hlsl" //v0.4a


	//v1.9b
	float autoLOD;

	//v1.9
	float4 ripplesPos;
sampler2D ripplesTex;
float4 rippleWaveParams;
float4 RippleSpecularAdjust;
float4 RippleFoamAdjust;

		//v1.8
		float waterAirMode;
		float4 NearClipAdjust;

		//v1.7
		// Unpack from normal map
		float3 UnpackNormalRGB(float4 packedNormal, float scale = 1.0)
		{
			float3 normal;
			normal.xyz = packedNormal.rgb * 2.0 - 1.0;
			normal.xy *= scale;
			return normal;
		}

		float3 UnpackNormalRGBNoScale(float4 packedNormal)
		{
			return packedNormal.rgb * 2.0 - 1.0;
		}

		float3 UnpackNormalAG(float4 packedNormal, float scale = 1.0)
		{
			float3 normal;
			normal.xy = packedNormal.ag * 2.0 - 1.0;
			normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));

			// must scale after reconstruction of normal.z which also
			// mirrors UnpackNormalRGB(). This does imply normal is not returned
			// as a unit length vector but doesn't need it since it will get normalized after TBN transformation.
			// If we ever need to blend contributions with built-in shaders for URP
			// then we should consider using UnpackDerivativeNormalAG() instead like
			// HDRP does since derivatives do not use renormalization and unlike tangent space
			// normals allow you to blend, accumulate and scale contributions correctly.
			normal.xy *= scale;
			return normal;
		}

		// Unpack normal as DXT5nm (1, y, 0, x) or BC5 (x, y, 0, 1)
		float3 UnpackNormalmapRGorAG(float4 packedNormal, float scale = 1.0)
		{
			// Convert to (?, y, 0, x)
			packedNormal.a *= packedNormal.r;
			return UnpackNormalAG(packedNormal, scale);
		}

		float3 UnpackNormalAA(float4 packedNormal)//float3 UnpackNormal(float4 packedNormal)
		{
			#if defined(UNITY_NO_DXT5nm)
				return UnpackNormalRGBNoScale(packedNormal);
			#else
				// Compiler will optimize the scale away
				return UnpackNormalmapRGorAG(packedNormal, 1.0);
			#endif
		}

		float3 UnpackNormalScale(float4 packedNormal, float bumpScale)
		{
			#if defined(UNITY_NO_DXT5nm)
				return UnpackNormalRGB(packedNormal, bumpScale);
			#else
				return UnpackNormalmapRGorAG(packedNormal, bumpScale);
			#endif
		}
		//END v1.7



	//URP v0.1
//#define REQUIRE_OPAQUE_TEXTURE
//#if defined(REQUIRE_OPAQUE_TEXTURE)
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
//#endif
TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);

float3 SampleSceneColor(float2 uv)
{
	return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UnityStereoTransformScreenSpaceTex(uv)).rgb;
}
//	float shadergraph_LWSampleSceneDepth(float2 uv)
//{
//#if defined(REQUIRE_DEPTH_TEXTURE)
//	return SampleSceneDepth(uv);
//#else
//	return 0;
//#endif
//}
//float3 shadergraph_LWSampleSceneColor(float2 uv)
//{
//#if defined(REQUIRE_OPAQUE_TEXTURE)
//	return SampleSceneColor(uv);
//#else
//	return 0;
//#endif
//}






	//CETO
	//#define CETO_OCEAN_TOPSIDE
	//#define CETO_TRANSPARENT_QUEUE

	//#include "./Shaders/OceanShaderHeader.cginc"
	//#include "./Shaders/OceanDisplacement.cginc"
	//#include "./Shaders/OceanBRDF.cginc"
	//#include "./Shaders/OceanUnderWater.cginc"
	//	#include "./Shaders/OceanSurfaceShaderBody.cginc"
	//END CETO
#pragma multi_compile _ MID CLOSE
#pragma target 4.0
	//float3 SUN_DIR;

	float4x4 _Interpolation;


	//sampler2D _Map0, _Map1, _Map2, _Map3, _Map4;
//	UNITY_DECLARE_TEX2D(_Map1);
//	UNITY_DECLARE_TEX2D_NOSAMPLER(_Map2);
	

	float4 _GridSizes, _Choppyness;
	float4 controlProjection;

	//v1.1 - START BRUNETON SKY
	sampler2D _SkyMap;
	sampler3D _Variance;	
	float3 _SeaColor;
	float _InscatterScale;

	float meanFresnel(float cosThetaV, float sigmaV)
	{
		return pow(1.0 - cosThetaV, 5.0 * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(sigmaV, 1.5));
	}

	// V, N in world space
	float MeanFresnel(float3 V, float3 N, float2 sigmaSq)
	{
		float2 v = V.xz; // view direction in wind space
		float2 t = v * v / (1.0 - V.y * V.y); // cos^2 and sin^2 of view direction
		float sigmaV2 = dot(t, sigmaSq); // slope variance in view direction
		return meanFresnel(dot(V, N), sqrt(sigmaV2));
	}

	// assumes x>0
	float erfc(float x)
	{
		return 2.0 * exp(-x * x) / (2.319 * x + sqrt(4.0 + 1.52 * x * x));
	}

	float Lambda(float cosTheta, float sigmaSq)
	{
		float v = cosTheta / sqrt((1.0 - cosTheta * cosTheta) * (2.0 * sigmaSq));
		return max(0.0, (exp(-v * v) - v * sqrt(M_PI) * erfc(v)) / (2.0 * v * sqrt(M_PI)));
		//return (exp(-v * v)) / (2.0 * v * sqrt(M_PI)); // approximate, faster formula
	}

	// L, V, N, Tx, Ty in world space
	float ReflectedSunRadiance(float3 L, float3 V, float3 N, float3 Tx, float3 Ty, float2 sigmaSq)
	{
		float3 H = normalize(L + V);
		float zetax = dot(H, Tx) / dot(H, N);
		float zetay = dot(H, Ty) / dot(H, N);

		float zL = dot(L, N); // cos of source zenith angle
		float zV = dot(V, N); // cos of receiver zenith angle
		float zH = dot(H, N); // cos of facet normal zenith angle
		float zH2 = zH * zH;

		float p = exp(-0.5 * (zetax * zetax / sigmaSq.x + zetay * zetay / sigmaSq.y)) / (2.0 * M_PI * sqrt(sigmaSq.x * sigmaSq.y));

		float tanV = atan2(dot(V, Ty), dot(V, Tx));
		float cosV2 = 1.0 / (1.0 + tanV * tanV);
		float sigmaV2 = sigmaSq.x * cosV2 + sigmaSq.y * (1.0 - cosV2);

		float tanL = atan2(dot(L, Ty), dot(L, Tx));
		float cosL2 = 1.0 / (1.0 + tanL * tanL);
		float sigmaL2 = sigmaSq.x * cosL2 + sigmaSq.y * (1.0 - cosL2);

		float fresnel = 0.02 + 0.98 * pow(1.0 - dot(V, H), 5.0);

		zL = max(zL, 0.01);
		zV = max(zV, 0.01);

		return fresnel * p / ((1.0 + Lambda(zL, sigmaL2) + Lambda(zV, sigmaV2)) * zV * zH2 * zH2 * 4.0);

	}

	// V, N, Tx, Ty in world space
	float2 U(float2 zeta, float3 V, float3 N, float3 Tx, float3 Ty)
	{
		float3 f = normalize(float3(-zeta, 1.0)); // tangent space
		float3 F = f.x * Tx + f.y * Ty + f.z * N; // world space
		float3 R = 2.0 * dot(F, V) * F - V;
		return R.xz / (1.0 + R.y);
	}

	// V, N, Tx, Ty in world space;
	float3 MeanSkyRadiance(float3 V, float3 N, float3 Tx, float3 Ty, float2 sigmaSq)
	{
		float4 result;

		const float eps = 0.001;
		float2 u0 = U(float2(0, 0), V, N, Tx, Ty);
		float2 dux = 2.0 * (U(float2(eps, 0.0), V, N, Tx, Ty) - u0) / eps * sqrt(sigmaSq.x);
		float2 duy = 2.0 * (U(float2(0.0, eps), V, N, Tx, Ty) - u0) / eps * sqrt(sigmaSq.y);

		result = tex2D(_SkyMap, u0 * (0.5 / 1.1) + 0.5, dux * (0.5 / 1.1), duy * (0.5 / 1.1));

		//if texture2DLod and texture2DGrad are not defined, you can use this (no filtering):
		//result = tex2D(_SkyMap, u0 * (0.5 / 1.1) + 0.5);

		return result.rgb;
	}

	//END BRUNETON SKY


	// define FragInputs structure
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

		//ScriptableRenderPipeline / com.unity.render - pipelines.core / ShaderLibrary / SpaceTransforms.hlsl
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

// Transform to homogenous clip space
float4x4 GetWorldToHClipMatrix()
	{
		return UNITY_MATRIX_VP;
	}
//// Tranforms position from world space to homogenous space
float4 TransformWorldToHClip(float3 positionWS)
	{
		return mul(GetWorldToHClipMatrix(), float4(positionWS, 1.0));
	}
float4 ComputeScreenPosB(float4 pos, float projectionSign)
{
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * projectionSign) + o.w;
	o.zw = pos.zw;
	return o;
}

//v3.4.4
float CalculateSSS(float3 normalL, float3 light, float3 camera) {
	
	float3 Normaly = normalize(0.15*camera + 11 * normalL);
	float flip = dot(light, -Normaly);
	float cap = max(0.0, flip);
	float cap2 = max(0.0, dot(light, -camera)*0.04 + 1.45);//v0.5 float cap2 = max(0.0, dot(light, -camera)*0.54 + 1.45);
	float cap3 = clamp(dot(Normaly, camera)*0.45 + 0.56, 0, 1);
	float cap4 = flip * cap*cap2*cap3*cap3;
	//return saturate(pow(cap4, 2) * 3 + cap2 * 0.05);
	//return saturate(pow(cap4, 2) * 3 + cap2 * 0.05) *  (dot(light, Normaly) + 0.3) ;
	//return saturate(pow(cap4, 2) * 3 + cap2 * 0.15) *  (dot(light, Normaly) + 0.1);
	return saturate(pow(cap4, 4) * 1 + cap2 * 0.05) *  (dot(light, Normaly)*1 + 0.16);
}

//v0.8
float4 _sssParams;
//v3.4.4
float CalculateSSSA(float3 worldNormal, float3 normalL, float3 light, float3 camera) {

	float3 Normaly = normalize(0.15*camera + 11 * normalL) * saturate(worldNormal);
	float flip = dot(light, -Normaly);
	float cap = max(0.0, flip);
	float cap2 = max(0.0, dot(light, -camera)*0.04*_sssParams.x + 1.45*_sssParams.y);//v0.5 float cap2 = max(0.0, dot(light, -camera)*0.54 + 1.45);
	float cap3 = clamp(dot(Normaly, camera)*0.45 + 0.56, 0, 1);
	float cap4 = flip * cap*cap2*cap3*cap3;
	//return saturate(pow(cap4, 2) * 3 + cap2 * 0.05);
	//return saturate(pow(cap4, 2) * 3 + cap2 * 0.05) *  (dot(light, Normaly) + 0.3) ;
	//return saturate(pow(cap4, 2) * 3 + cap2 * 0.15) *  (dot(light, Normaly) + 0.1);
	return saturate(pow(cap4, 1) * 11  + cap2 * 0.15) *  (dot(light, Normaly) * 11 * _sssParams.z + 0.06*_sssParams.w) + dot(light, Normaly) * 2;
}

//v3.4.4
float foamContrast;
float4 _foamColor;
float4 _sssColor;

//v0.5
float sssFactor;
float sssBackSideFactor;


	/*struct appdata
	{
		float4 vertex : POSITION;
		float4 normal : NORMAL;
	};*/

	// interpolator structs


	float _cameraTilt;

	//HDRP
	float offsetTopWaterLight;
	float _regulateShoreWavesHeight;
	float _regulateShoreWavesHeightA;
	float _regulateShoreWavesHeightB;
	float _regulateShoreWavesHeightC;
	float4 dynamicFoamControls;
	float4 dynamicFoamControlsA;

	//FFT
	sampler2D _RampTex;
	sampler2D _NormalTex;
	float _TilingFFT;
	float _PowerFFT;
	float _GlobalPowerFFT;
	float _PowerFFT1Y;
	float _PowerFFT2Y;

	//v0.7
	float4 shoreWaveControl;
	float4 shoreWaveControlB;
	float4 shoreGlow; 
	float4 foamHighSea;
	float4 shoreWaves;
	float depthBelowZero;

	//v0.8 HDRP
	sampler2D _fluidTexture;
	sampler2D _fluidTextureADVECT;

	//v1.1 - Bruneton sky radiance
	float4 _controlSkyRadiance;
	float4 _controlSkyRadianceA;
	float4 _controlSkyColor;
	float cameraNearClip;
	float4 cameraNearColor;
	float4 cameraNearControl;

	float4 DepthPyramidScale;

	//v1.2 - 1.3
	int enableProjectedGrid = 1;
	float4 decalPosRot;
	float4 decalEdgeTweak;
	float4 decalPower;

	//v1.4
	float4 vortexPosScale;
	float4 _InteractAmpFreqRad;


	//v1.5 - PRO - FULL FFT
	sampler2D _Displacement_c0;
	//sampler2D _Derivatives_c0;
	//sampler2D _Turbulence_c0;
	sampler2D _Displacement_c1;
	//sampler2D _Derivatives_c1;
	//sampler2D _Turbulence_c1;
	sampler2D _Displacement_c2;
	//sampler2D _Derivatives_c2;
	//sampler2D _Turbulence_c2;
	UNITY_DECLARE_TEX2D(_Derivatives_c0);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Derivatives_c1);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Derivatives_c2);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Turbulence_c0);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Turbulence_c1);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Turbulence_c2);
	
	float4 lengthScales;
	float LengthScale0;
	float LengthScale1;
	float LengthScale2;
	float _LOD_scale;
	float _SSSBase;
	float _SSSScale;
	float4 _Color, _FoamColor, _SSSColor;
	float _SSSStrength;
	float _Roughness, _RoughnessScale, _MaxGloss;
	float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
	//sampler2D _CameraDepthTexture;
	sampler2D _FoamTexture;
	
	//v1.6
	float depthFadePower;

	//v1.7
	sampler3D _Displace_T3D;
	sampler3D _Normals_T3D;
	float4 _NormalsOffset3DControls;

	struct Input
	{
		float2 worldUV;
		float4 lodScales;
		float3 viewVector;
		float3 worldNormal;
		float4 screenPos;
		//INTERNAL_DATA
	};
	float3 WorldToTangentNormalVector(Input IN, float3 normal) {
		float3 t2w0 = 1;// WorldNormalVector(IN, float3(1, 0, 0));
		float3 t2w1 = 1;// WorldNormalVector(IN, float3(0, 1, 0));
		float3 t2w2 = 1;// WorldNormalVector(IN, float3(0, 0, 1));
		float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
		return normalize(mul(t2w, normal));
	}
	float pow5(float f)
	{
		return f * f * f * f * f;
	}



	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float4 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float4 grabPassPos : TEXCOORD4;
		float4 normal : NORMAL;
		float4 lodScales : TEXCOORD5; //v1.5 - PRO - FULL FFT
		UNITY_FOG_COORDS(6)
		
		//FFT	
		//float3 lightDir: TEXCOORD6;
	
		//float3 vertPos: TEXCOORD7;
		//	float3 lightPos: TEXCOORD8;
	
		float4 lightDirPosx: TEXCOORD7;
		float4 lightPosYZvertXY: TEXCOORD8;//vertex z encoded in normalInterpolator
	
		LIGHTING_COORDS(9,10)
		UNITY_VERTEX_OUTPUT_STEREO //VR1
	};

	struct v2f_noGrab
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float3 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		UNITY_FOG_COORDS(4)
	};
	
	struct v2f_simple
	{
		float4 pos : SV_POSITION;
		float4 viewInterpolator : TEXCOORD0;
		float4 bumpCoords : TEXCOORD1;
		UNITY_FOG_COORDS(2)
	};

	// textures
	sampler2D _BumpMap;
	sampler2D _ReflectionTex;
	sampler2D _RefractionTex;
	sampler2D _ShoreTex;

	//FFT
	sampler2D _MainTex;


	//HDRP 2019.3
	//https://forum.unity.com/threads/has-depthpyramidtexture-been-removed-from-hdrp-in-unity-2019-3.851236/#post-5615971

	//UNITY_DECLARE_TEX2DARRAY(_CameraDepthTexture);
	TEXTURE2D(_CameraDepthTexture);//URP v0.1
	SAMPLER(sampler_CameraDepthTexture); //v0.4a HDRP 10.2

//	float4 _DepthPyramidScale;
//	UNITY_DECLARE_TEX2DARRAY(_ColorPyramidTexture);
	//vertex
	//o.projPos = ComputeScreenPos(o.vertex);
	//o.projPos.xy *= _DepthPyramidScale.xy;
	//COMPUTE_EYEDEPTH(o.projPos.z);
	//o.uvgrab = ComputeGrabScreenPos(o.vertex);
	//frag
	//float sceneZ = LinearEyeDepth(UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(i.projPos.xy / i.projPos.w, 0, 0), 0));
	//half4 grabColor = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorPyramidTexture, float4(i.uvgrab.xy / i.uvgrab.w, 0, 0), 0);

	//TEXTURE2D_X(_CameraDepthTexture);
	//SAMPLER(sampler_CameraDepthTexture);

	//HDRP 2019.2
	//sampler2D_float _CameraDepthTexture;
//	TEXTURE2D(_CameraDepthTexture);
//	SAMPLER(sampler_CameraDepthTexture);
	//TEXTURE2D_X(_CameraDepthTexture);
	//SAMPLER(sampler_CameraDepthTexture);
	//float _localLightRange = 2.0;

	///TEXTURE2D_X(_DepthPyramidTexture);
	//SAMPLER(sampler_DepthPyramidTexture);
	TEXTURE2D(_DepthTexture); //URP v0.1
	SAMPLER(sampler_DepthTexture);

	// colors in use
	uniform float4 _RefrColorDepth;
	uniform float4 _SpecularColor;
	uniform float4 _BaseColor;
	uniform float4 _ReflectionColor;
	
	// edge & shore fading
	uniform float4 _InvFadeParemeter;

	// specularity
	uniform float _Shininess;
	uniform float4 _WorldLightDir;

	// fresnel, vertex & bump displacements & strength
	uniform float4 _DistortParams;
	uniform float _FresnelScale;
	uniform float FresnelFactor; //v0.6
	uniform float4 _BumpTiling;
	uniform float4 _BumpDirection;

	uniform float4 _GAmplitude;
	uniform float4 _GFrequency;
	uniform float4 _GSteepness;
	uniform float4 _GSpeed;
	uniform float4 _GDirectionAB;
	uniform float4 _GDirectionCD;
	
	//local
	uniform float4 _LocalWavePosition;
	uniform float4 _LocalWaveVelocity;
	uniform float4 _LocalWaveParams;
	
	// foam
	uniform float4 _Foam;
	
	//SM30
	uniform float _MultiplyEffect;
	
	//TERRAIN DEPTH
	sampler2D _ShoreContourTex;
	float3 _DepthCameraPos;
	float _ShoreFadeFactor;
	float2 _TerrainScale;
	float _WorldScale;
	float cameraOffset;
	float2 offsetScale;
	float2 offsetRflect;
	
	// shortcuts
	#define PER_PIXEL_DISPLACE _DistortParams.x
	#define REALTIME_DISTORTION _DistortParams.y
	#define FRESNEL_POWER _DistortParams.z
	#define VERTEX_WORLD_NORMAL i.normalInterpolator.xyz
	#define FRESNEL_BIAS _DistortParams.w
	#define NORMAL_DISPLACEMENT_PER_VERTEX _InvFadeParemeter.z
	

	//v3.4.3
	uniform float4 fogColor;
	uniform float fogDepth;
	uniform float fogBias;
	uniform float fogThres;

	uniform float isUnderwater;
	uniform float waterHeight;
	float waterHeightFogOffset;
	float depthFade = 130; //v0.8

	//
	// UNDERWATER VERSION
	//
	
	uniform float4x4 _LightMatix0;

	//HDRP
	//sampler2D _ColorPyramidTexture;
	TEXTURE2D(_ColorPyramidTexture);// TEXTURE2D_X URP v0.1
	SAMPLER(sampler_ColorPyramidTexture);
	float4 _ScreenSize;                 // { w, h, 1 / w, 1 / h }
	float4 _controlReflect;
	float4 _localLightAPos;
	float4 _localLightAProps;
	
	v2f vert600(appdata_full v)
	{
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v); //v1.7.8 //VR1 UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
		UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert //VR1  UNITY_VERTEX_OUTPUT_STEREO //VR1
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert //VR1	
		
		half4 worldSpaceVertex = mul(unity_ObjectToWorld,(v.vertex));
		half3 vtxForAni = worldSpaceVertex.xzz;

		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
			_GAmplitude,												// amplitude
			_GFrequency,												// frequency
			_GSteepness,												// steepness
			_GSpeed,													// speed
			_GDirectionAB,												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);
		
		v.vertex.xyz += offsets;
		
		// one can also use worldSpaceVertex.xz here (speed!), albeit it'll end up a little skewed
		half2 tileableUv = mul(unity_ObjectToWorld,(v.vertex)).xz;
		
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex.xyz - _WorldSpaceCameraPos;

		o.pos = UnityObjectToClipPos(v.vertex);

		ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);
		
		o.normalInterpolator.xyz = nrml;
		
		o.viewInterpolator.w = saturate(offsets.y);
		o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		
		o.normal.xyz = v.normal.xyz;

		//if (tex.r > 0) {
			//if (tex.r > 0.0083) {
			//	o.normal.w = pow((1.1 + tex.r - 0.15), 3);// 0;// pow((1.1 + tex.r - 0.15), 2) + 0; //v0.1
			//}
			//else {
				o.normal.w = 0;//o.normal.w = pow((1.1 + tex.r - 0.15), 1) * pow(0.007 - tex.r,12);
			//}
		//}
		//else {
			//o.normal.w = 0;
		//}
		
		
		//o.lightDir = ObjSpaceLightDir(v.vertex);
		
	//	o.lightPos = mul(_LightMatix0,worldSpaceVertex);
		//o.vertPos = worldSpaceVertex;
		
		o.lightDirPosx.xyz = ObjSpaceLightDir(v.vertex);
		
		o.lightPosYZvertXY.zw = worldSpaceVertex.xy;
		o.normalInterpolator.w = worldSpaceVertex.z;
		
		float4 LightPos = mul(_LightMatix0,worldSpaceVertex);
		
		o.lightPosYZvertXY.xy = LightPos.yz;
		o.lightDirPosx.w = LightPos.x;
		
		
		UNITY_TRANSFER_FOG(o,o.pos);
		return o;
	}

	half4 frag600( v2f i ) : SV_Target
	{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, PER_PIXEL_DISPLACE);
		half3 viewVector = -normalize(i.viewInterpolator.xyz);

		half4 distortOffset = half4(worldNormal.xz * REALTIME_DISTORTION * 10.0, 0, 0);
		half4 screenWithOffset = i.screenPos + distortOffset;
		half4 grabWithOffset = i.grabPassPos + distortOffset;
		
		half4 rtRefractionsNoDistort = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(i.grabPassPos));
	//	half refrFix = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset));
		//half refrFix = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset));
		half refrFix = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset).xy,0).r; //LOAD_TEXTURE2D_X_LOD URP v0.1

		half4 rtRefractions = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(grabWithOffset));
		
		#ifdef WATER_REFLECTIVE
			float4 uvsRFefl = UNITY_PROJ_COORD(screenWithOffset);
			half4 rtReflections = tex2Dproj(_ReflectionTex, float4(uvsRFefl.x, uvsRFefl.y,1,1));
		#endif

		#ifdef WATER_EDGEBLEND_ON
		if (LinearEyeDepth(refrFix) < i.screenPos.z)
			rtRefractions = rtRefractionsNoDistort;
		#endif
		
		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize ((_WorldLightDir.xyz) + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0,pow (nh, _Shininess));
		
		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
		
		#ifdef WATER_EDGEBLEND_ON
			//half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			half depth = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos).xy, 0).r; //LOAD_TEXTURE2D_X_LOD URP v0.1

			depth = LinearEyeDepth(depth);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.w));
			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		
		// shading for fresnel term
		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		// base, depth & reflection colors
		half4 baseColor = ExtinctColor (_BaseColor, i.viewInterpolator.w * _InvFadeParemeter.w);
		#ifdef WATER_REFLECTIVE
			half4 reflectionColor = lerp (rtReflections,_ReflectionColor,_ReflectionColor.a);
		#else
			half4 reflectionColor = _ReflectionColor;
		#endif
		
		baseColor = lerp (lerp (rtRefractions, baseColor, baseColor.a), reflectionColor, refl2Refr);
		baseColor = baseColor + spec * _SpecularColor;
		
		// handle foam
		half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
		baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y));
		
		baseColor.a = edgeBlendFactors.x;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}
	
	//
	// HQ VERSION
	//

	
		//https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Normal-From-Height-Node.html
		//v1.9
	/*	void NormalFromHeight_World(float In, float3 Position,  out float3 Out)
	{
		float3 worldDirivativeX = ddx(Position * 100);
		float3 worldDirivativeY = ddy(Position * 100);
		float3 crossX = cross(TangentMatrix[2].xyz, worldDirivativeX);
		float3 crossY = cross(TangentMatrix[2].xyz, worldDirivativeY);
		float3 d = abs(dot(crossY, worldDirivativeX));
		float3 inToNormal = ((((In + ddx(In)) - In) * crossY) + (((In + ddy(In)) - In) * crossX)) * sign(d);
		inToNormal.y *= -1.0;
		Out = normalize((d * TangentMatrix[2].xyz) - inToNormal);
	}*/
	//float3 HeightToNormal(float height, float3 normal, float3 pos)
	//{
	//	float3 worldDirivativeX = ddx(pos);
	//	float3 worldDirivativeY = ddy(pos);
	//	float3 crossX = cross(normal, worldDirivativeX);
	//	float3 crossY = cross(normal, worldDirivativeY);
	//	float3 d = abs(dot(crossY, worldDirivativeX));
	//	float3 inToNormal = ((((height + ddx(height)) - height) * crossY) + (((height + ddy(height)) - height) * crossX)) * sign(d);
	//	inToNormal.y *= -1.0;
	//	return normalize((d * normal) - inToNormal);
	//}
	
	
	
	v2f vert(appdata_full v)
	{
		v2f o;		
		UNITY_SETUP_INSTANCE_ID(v); //v1.7.8 //VR1 UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
		UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert //VR1  UNITY_VERTEX_OUTPUT_STEREO //VR1
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert //VR1	
		//v1.0
		//PROJECTED GRID

		//float2 uv = v.texcoord.xy;
		float2 uv = v.texcoord.xy * 0.99; //v1.1 - fix flicker issue and back projection

		//v1.1
		float3 scale = float3(
			length(unity_ObjectToWorld._m00_m10_m20),
			length(unity_ObjectToWorld._m01_m11_m21),
			length(unity_ObjectToWorld._m02_m12_m22)
			);
		float scaler = controlProjection.x;
		float uvx = uv.x * 1;
		float uvx2 = uv.x * 1.126-0.052; //v0.4a
		if (scale.x > 1) {
			//scaler = controlProjection.y;
			//uvx = uv.x / 200 + 0.494;// 0.494;
			//uvx2 = uv.x * 1.2 - 0.05;// 0.494;
			uvx = uv.x / 200 + 0.494;// 0.494;
			uvx2 = uv.x * 1.2 - 0.07;// 0.494; //v0.4a

			float diff = (_WorldSpaceCameraPos.y);

			if (diff < 100) {
				controlProjection.w = controlProjection.w / (diff / 20);
				scaler = controlProjection.y + 0.1;
			}
			else {
				scaler = controlProjection.y;
			}
		}
		
		if (enableProjectedGrid == 1) { //v1.2

		//Interpolate between frustums world space projection points. p is in world space.
		//float4 p = lerp(lerp(_Interpolation[0], _Interpolation[1], uv.x), lerp(_Interpolation[3], _Interpolation[2], uv.x), pow(uv.y, controlProjection.x));
			float4 p = lerp(lerp(_Interpolation[0], _Interpolation[1], uvx), lerp(_Interpolation[3], _Interpolation[2], uvx2),
				pow(uv.y + controlProjection.w, scaler));
			p = p / p.w;


			//p.z += p.z * 2;

			//displacement
			float4 dp = float4(0, 0, 0, 0);

			/*dp.y += 25 * tex2Dlod(_Map0, float4(p.xz / _GridSizes.x, 0, 0)).x;
			dp.y += tex2Dlod(_Map0, float4(p.xz / _GridSizes.y, 0, 0)).y;
			dp.y += tex2Dlod(_Map0, float4(p.xz / _GridSizes.z, 0, 0)).z;
			dp.y += tex2Dlod(_Map0, float4(p.xz / _GridSizes.w, 0, 0)).w;

			dp.xz += _Choppyness.x * tex2Dlod(_Map3, float4(p.xz / _GridSizes.x, 0, 0)).xy;
			dp.xz += _Choppyness.y * tex2Dlod(_Map3, float4(p.xz / _GridSizes.y, 0, 0)).zw;
			dp.xz += _Choppyness.z * tex2Dlod(_Map4, float4(p.xz / _GridSizes.z, 0, 0)).xy;
			dp.xz += _Choppyness.w * tex2Dlod(_Map4, float4(p.xz / _GridSizes.w, 0, 0)).zw;*/
			//v.vertex = mul(unity_WorldToObject, p + 75 * clamp(dp, -1, 25));// mul(UNITY_MATRIX_VP, p); //+ dp
			v.vertex = mul(unity_WorldToObject, p);
			//v.vertex = mul(unity_WorldToObject, p);
			//v.vertex = mul(unity_WorldToObject, p);

			//o.pos = mul(UNITY_MATRIX_VP, p + dp);
			//o.worldPos = p + dp;
		}
		//END PROJECT GRID

		//v1.1
		if (scale.x > 1) {
			v.vertex.y = v.vertex.y - 25* controlProjection.z;
			//v.vertex.z = v.vertex.z + 10;
			//v.vertex.x = v.vertex.x - 25 * controlProjection.w*sign(v.vertex.x);
			//v.vertex.z = v.vertex.z - 25 * controlProjection.w*sign(v.vertex.z);
		}


		half4 worldSpaceVertex = mul(unity_ObjectToWorld,(v.vertex));
		half3 vtxForAni = worldSpaceVertex.xzz;

		//worldSpaceVertex.x = worldSpaceVertex.x + 21 * cos(0.002*_Time.y*worldSpaceVertex.z);//v3.4.4


		//DEPTH - HDRP
		half2 tileableUv = mul(unity_ObjectToWorld, (v.vertex)).xz;// +cameraOffset * _WorldSpaceCameraPos.xz;

		//MINE
		//float2 coords = v.texcoord.xy/1;		
		//coords = float2(tileableUv.x*11, tileableUv.y*10);		
		float WorldScale = _WorldScale;
		float3 CamPos = _DepthCameraPos;//_WorldSpaceCameraPos;
		/*float2 Origin = float2(CamPos.x - WorldScale / 2, CamPos.z - WorldScale / 2);
		float2 UnscaledTexPoint = float2(tileableUv.x - Origin.x + offsetScale.x, tileableUv.y - Origin.y + offsetScale.y);
		float2 ScaledTexPoint = float2(UnscaledTexPoint.x / (_TerrainScale.x*4), UnscaledTexPoint.y / (_TerrainScale.y * 4));*/
		float2 Origin = float2(CamPos.x - WorldScale * _TerrainScale.x / 2, CamPos.z - WorldScale * _TerrainScale.y / 2);
		float2 UnscaledTexPoint = float2(tileableUv.x - Origin.x, tileableUv.y - Origin.y);
		float2 ScaledTexPoint = float2(UnscaledTexPoint.x / (WorldScale*_TerrainScale.x), UnscaledTexPoint.y / (WorldScale*_TerrainScale.y));

		float4 tex = tex2Dlod(_ShoreContourTex, float4(ScaledTexPoint, 0, 0));
		//END DEPTH

		//v1.0
		//if (length(CamPos.xz - tileableUv) > (WorldScale) / 2)
		//if(  length(CamPos.xz - tileableUv) > (WorldScale* _TerrainScale.x) /2)// _TerrainScale.x/2)
		if(ScaledTexPoint.x < 0 || ScaledTexPoint.y < 0 || ScaledTexPoint.x > 1 || ScaledTexPoint.y > 1)
		{
			//if (ScaledTexPoint.x > 1 || ScaledTexPoint.y > 1) {
			tex = float4(0, 10.99, 0, 0);
		}

	

		//v1.6
		////v1.2
		////MASK
		//float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));		
		//float distToPlane = dot(camPlaneNormal, worldSpaceVertex - _WorldSpaceCameraPos);	
		//if (distToPlane > cameraNearClip+5) {
		//	if (scale.x < 1) {
		//		v.vertex.y = v.vertex.y - cameraNearControl.w;
		//	}
		//}
		




		//HDRP - affect waves by depth
		//float depthBelowZero = 8;
		float range = CamPos.y + depthBelowZero;
		float realHeight = ((tex.r) * range) - depthBelowZero;

		float4 _GFrequencyA = _GFrequency;
		float4 _GAmplitudeA = _GAmplitude;
		float4 _GSpeedA = _GSpeed;
		float4 _GDirectionABA = _GDirectionAB;
		float4 _GDirectionCDA = _GDirectionCD;
		float factorTime = 1; 
		float factorTimeA = 0.2;
		float _GSteepnessA = _GSteepness;//v0.6
		if (tex.r > 0.00008 ) {
			//_GFrequency.y = pow(1-tex.r-0.1, 12)*_GFrequency.y*factorTime;
			//_GFrequency.x = pow(1-tex.r-0.1, 12)*_GFrequency.x*factorTime;
			//_GSpeed.x = pow(1-tex.r +0.1, 11)*_GSpeed.x*factorTime;
			//_GSpeed.y = pow(1-tex.r + 0.1, 13)*_GSpeed.y*factorTime;
			//_GSpeed.z = pow(1-tex.r + 0.1, 11)*_GSpeed.z*factorTime;
			//float factorTime = 0.1;

		//	_GFrequencyA.y = pow(1 - tex.r - 0.1, 12) * _GFrequency.y*factorTimeA * 0.8;
		//	_GFrequencyA.x = pow(1 - tex.r - 0.1, 12) * _GFrequency.x * factorTimeA * 0.8;
			//_GFrequencyA.z = pow(1 - tex.r - 0.1, 12) * _GFrequency.z*factorTimeA * 0.8;
			//_GFrequencyA.w = pow(1 - tex.r - 0.1, 12) * _GFrequency.w * factorTimeA * 0.8;
			//_GFrequencyA.z = pow(1 - tex.r - 0.1, 12) * _GFrequency.z * factorTimeA * 1.0;
//			_GSpeedA.x = pow(1 - tex.r + 0.1, 11) * _GSpeed.x * factorTimeA * 2;
//			_GSpeedA.y = pow(1 - tex.r + 0.1, 13) * _GSpeed.y * factorTimeA * 2;
//			_GSpeedA.z = pow(1 - tex.r + 0.1, 11) * _GSpeed.z * factorTimeA * 2; //3
//			_GSpeedA.w = pow(1 - tex.r + 0.1, 11) * _GSpeed.w * factorTimeA * 2;

		//	_GAmplitudeA.x = pow(1 - tex.r + 0.1, 11) * _GAmplitude.x * factorTimeA * 2;
//			_GAmplitudeA.y = pow(1 - tex.r + 0.1, 11) * _GAmplitude.y * factorTimeA * 0.2;
		//	_GAmplitudeA.w = pow(1 - tex.r + 0.1, 11) * _GAmplitude.w * factorTimeA * 2;
			
			
//			_GAmplitudeA.z = pow(1 - tex.r + 0.1, 11) * _GAmplitude.z * factorTimeA * 0.01;


			//_GAmplitudeA.y = pow(1 - tex.r + 0.1, 11) * _GAmplitude.y * factorTimeA * 2;
			/*_GDirectionABA.x = pow(1 - tex.r + 0.1, 11)*_GDirectionABA.x * 1;
			_GDirectionABA.y = pow(1 - tex.r + 0.1, 11)*_GDirectionABA.y * 1;
			_GDirectionABA.z = pow(1 - tex.r + 0.1, 11)*_GDirectionABA.z * 1;
			_GDirectionABA.w = pow(1 - tex.r + 0.1, 11)*_GDirectionABA.w * 1;

			_GDirectionCDA.x = pow(1 - tex.r + 0.1, 11)*_GDirectionCDA.x * 0.1;
			_GDirectionCDA.y = pow(1 - tex.r + 0.1, 11)*_GDirectionCDA.y * 0.1;
			_GDirectionCDA.z = pow(1 - tex.r + 0.1, 11)*_GDirectionCDA.z * 0.1;
			_GDirectionCDA.w = pow(1 - tex.r + 0.1, 11)*_GDirectionCDA.w * 0.1;*/


			//_GSpeedA.x = pow(1 - tex.r + 0.1, 11) * 1 * factorTimeA * 2 / (0.003*_Time.y);
			//_GSpeedA.y = pow(1 - tex.r + 0.1, 13) * 7 * factorTimeA * 2;
			//_GSpeedA.z = pow(1 - tex.r + 0.1, 11) * 1 * factorTimeA * 2; //3
			//_GSpeedA.w = pow(1 - tex.r + 0.1, 11) * 1 * factorTimeA * 1;
			//_GAmplitudeA.x = pow(1 - tex.r + 0.1, 11) * 5 * factorTimeA * 2;

			//_GSpeedA.x = pow(1 -tex.r + 0.12 - (0.000134*_Time.y), 12) * 1 * factorTimeA * 2 ;
			//_GSpeedA.x = pow(1 - tex.r + 0.12, 12 * _GSpeed.x)   * factorTimeA * 2; //_GSpeed.x = 0.4
			//_GSpeedA.x = pow(1 - tex.r + 0.12, 12 - (0.009*_Time.y))   * factorTimeA * 2;

			//_GSpeedA.x = pow(1 - tex.r + 0.12, 12 * (_GSpeed.x - (0.000234*_Time.y)) )   * factorTimeA * 2 + 1*(0.00018*_Time.y); //_GSpeed.x = 0.4
			//_GSpeedA.x = pow(1 - tex.r + 0.12, 12 * (_GSpeed.x - (0.000434*_Time.y)))   * factorTimeA * 2 + 1 * (0.00018*_Time.y); //_GSpeed.x = 0.6
			//_GSpeedA.x = pow(1 - tex.r + 0.12, 12 * (0.5 - (0.000434 * 1)))   * factorTimeA * 2 + 1 * (0.00018 * 1) / (_Time.y/3600); //v0.5

			_GSpeedA.x = (pow(1 - tex.r + 0.12, 12 * (0.5 - (0.0001934 * _Time.y)))   * factorTimeA * 2 + 1 * (0.00018 * 1) / 1 + _GSpeed.x)*(shoreWaveControlB.x)
				+(1-shoreWaveControlB.x)*_GSpeed.x;//v0.5

			//v0.6
			//_GSteepnessA = _GSteepness / 100; //v0.6
			//_GAmplitudeA = float4(12,3,2.5,0.69);
			//_GFrequencyA = float4(0.2, 0.06, 0.14, 0.5);		
			//_GSteepnessA = float4(0.2,1.13,1.1,-1.26);
			_GSteepnessA.x = 0.2*(shoreWaveControlB.y);
			_GAmplitudeA.x = (12 * (tex.r*tex.r +0.55 + shoreWaveControl.x))*(shoreWaveControlB.z) + (1-shoreWaveControlB.z)*_GAmplitude.x; //0.35 //0.55
			_GFrequencyA.x = 0.2*(shoreWaveControlB.w);
			_GerstnerIntensities = shoreWaves;// float4(1, 1, 0, 1) * 1;

			//v1.2 - reduce flicker
			/*_GSpeedA.x = tex.r * _GSpeedA.x + (1 - tex.r) * _GSpeed.x;
			_GSteepnessA.x = tex.r * _GSteepnessA.x + (1 - tex.r) * _GSteepness.x;
			_GFrequencyA.x = tex.r * _GFrequencyA.x + (1 - tex.r) * _GFrequency.x;
			_GAmplitudeA.x = tex.r * _GAmplitudeA.x + (1 - tex.r) * _GAmplitude.x;
			_GerstnerIntensities = tex.r * _GerstnerIntensities + (1 - tex.r) * shoreWaves;*/
		}
		//END EFFECT WAVES BY DEPTH


		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
			_GAmplitudeA,												// amplitude
			_GFrequencyA,												// frequency
			_GSteepnessA,												// steepness
			_GSpeedA,													// speed
			_GDirectionABA,												// direction # 1, 2
			_GDirectionCDA												// direction # 3, 4
		);
		
		//half2 tileableUv = mul(unity_ObjectToWorld,(v.vertex)).xz;		
		//
		////MINE
		////float2 coords = v.texcoord.xy/1;		
		////coords = float2(tileableUv.x*11, tileableUv.y*10);		
		//float WorldScale=500;
		//WorldScale = _TerrainScale;
		//float3 CamPos = float3(250,0,250);//_WorldSpaceCameraPos;
		//CamPos =_DepthCameraPos;//_WorldSpaceCameraPos;
		//float2 Origin = float2(CamPos.x - WorldScale/2 , CamPos.z - WorldScale/2);
		//float2 UnscaledTexPoint = float2(tileableUv.x - Origin.x , tileableUv.y - Origin.y);
		//float2 ScaledTexPoint = float2(UnscaledTexPoint.x/WorldScale , UnscaledTexPoint.y/WorldScale);
		//
		//float4 tex = tex2Dlod(_ShoreContourTex,float4(ScaledTexPoint,0,0));
		
		//-8 is bottom depth=0, CamPos is top, depth = 1


		
		
		//FFT		
		float3 worldPos = worldSpaceVertex.xyz;
		float4 dd = tex2Dlod(_MainTex, float4(worldPos.xz / _TilingFFT, 0, 0) - 6.2*float4(2 * abs(cos(_Time.y * 0.008)), 2 * abs(cos(_Time.y * 0.008)), 0, 0));
		float4 dd2 = tex2Dlod(_MainTex, float4(worldPos.xz / _TilingFFT / 2, 0, 0) - 5.2*float4(2 * abs(cos(_Time.y * 0.008)), 2 * abs(cos(_Time.y * 0.008)), 0, 0));
		float4 dd3 = tex2Dlod(_MainTex, float4(worldPos.xz / _TilingFFT / 2, 0, 0) + 5.2*float4(2 * abs(cos(_Time.y * 0.008)), 2 * abs(cos(_Time.y * 0.008)), 0, 0));
		//v.vertex.x += dd.x * 331111 + dd.z * 111111;
		//v.vertex.y += dd.y*_PowerFFT;//21
		//v.vertex.z += dd.z * 331111 + dd.x * 111111;

		if (tex.r > 0 ) {
			_PowerFFT = _PowerFFT * shoreWaves.w;
		}

		if (tex.r > 0 && shoreWaves.w == 0 ) {//v0.6
											 //v.vertex.x += dd.x * 2 / (1 + tex.r) + dd.z * 2 / (1+tex.r);
											 //v.vertex.y += dd.y*_PowerFFT;//21
											 //v.vertex.z += dd.z * 2 / (1 + tex.r) + dd.x * 2 / (1 + tex.r);

											 //v.vertex.x += (dd2.x + dd2.z) * (2 / (1 + tex.r));
											 //v.vertex.y += (dd2.y * _PowerFFT * 2 - 14) * (2 / (1 + tex.r));// +8;//21
											 //v.vertex.z += (dd2.z + dd2.x) * (2 / (1 + tex.r));
											 //			v.vertex.x += dd.x*331111+dd.z*111111;
											 //			v.vertex.y += dd.y*1;
											 //			v.vertex.z += dd.z*331111+dd.x*111111;
											 //float4 ddN = tex2Dlod(_NormalTex, float4(worldPos.xz / _TilingFFT, 0, 0));
											 //float4 ddN2 = tex2Dlod(_NormalTex, float4(worldPos.xz / _TilingFFT / 2, 0, 0));
											 //ddN.xyz = half3(2,1,2) * ddN.rbg - half3(1,0,1);
											 //nrml = normalize(nrml + ddN + ddN2);
		}
		else {
			v.vertex.x += (dd.x * 1 + dd.z * 1)*_GlobalPowerFFT;// +55 * abs(cos(_Time.y * 1.13));
			v.vertex.y += (dd.y * 1 * _PowerFFT)*_GlobalPowerFFT;//21
			v.vertex.z += (dd.z * 1 + dd.x * 1)*_GlobalPowerFFT;// +55 * abs(cos(_Time.y * 1.13));

			v.vertex.x += (dd2.x * 1 + dd2.z * 1)*_GlobalPowerFFT;// +45 * abs(cos(_Time.y * 0.4));
			v.vertex.y += (dd2.y * 1 * _PowerFFT * 2 + _PowerFFT1Y)*_GlobalPowerFFT;// +8;//21 //v.vertex.y += (dd2.y * 1 * _PowerFFT * 2 - 14)*_GlobalPowerFFT;
			v.vertex.z += (dd2.z * 1 + dd2.x * 1)*_GlobalPowerFFT;// +45 * abs(cos(_Time.y * 0.4));

			v.vertex.x += ((dd3.x * 1 + dd3.z * 1))*_GlobalPowerFFT;// +235 * abs(cos(_Time.y * 0.3));
			v.vertex.y -= (dd3.y * 1 * dd3.y * _PowerFFT * 1 + _PowerFFT2Y)*_GlobalPowerFFT;// +8;//21 //v.vertex.y -= (dd3.y * 1 * dd3.y * _PowerFFT * 1 + 2)*_GlobalPowerFFT;
			v.vertex.z += ((dd3.z * 1 + dd3.x * 1))*_GlobalPowerFFT;//  +235 * abs(cos(_Time.y * 0.3));

												  //			v.vertex.x += dd.x*331111+dd.z*111111;
												  //			v.vertex.y += dd.y*1;
												  //			v.vertex.z += dd.z*331111+dd.x*111111;
			float4 ddN = tex2Dlod(_NormalTex, float4(worldPos.xz / _TilingFFT, 0, 0) - 6.2*float4(2 * abs(cos(_Time.y * 0.008)), 2 * abs(cos(_Time.y * 0.008)), 0, 0));
			float4 ddN2 = tex2Dlod(_NormalTex, float4(worldPos.xz / _TilingFFT / 2, 0, 0) - 5.2*float4(2 * abs(cos(_Time.y * 0.008)), 2 * abs(cos(_Time.y * 0.008)), 0, 0));
			float4 ddN3 = tex2Dlod(_NormalTex, float4(worldPos.xz / _TilingFFT / 2, 0, 0) + 5.2*float4(2 * abs(cos(_Time.y * 0.008)), 2 * abs(cos(_Time.y * 0.008)), 0, 0));
			//ddN.xyz = half3(2,1,2) * ddN.rbg - half3(1,0,1);
			nrml = normalize(nrml + _GlobalPowerFFT * (float3(ddN.x, 0, ddN.z) + float3(ddN2.x, 0, ddN2.z) + float3(ddN3.x, 0, ddN3.z)));
		}

		if (tex.r > 0 && 1==1) {

			


			v.vertex.xyz += float3(offsets.x, 1 *(pow(tex.r + 0.00000001, _ShoreFadeFactor)), offsets.z);
			//v.vertex.xyz += float3(offsets.x, (offsets.y - (tex.r)*(211418*pow(0.2-tex.r , 3))), offsets.z);
			v.vertex.xyz += _regulateShoreWavesHeight * float3(offsets.x, 0.6*offsets.y * pow(0.45 - tex.r, 1) + offsets.y * pow(0.45 - tex.r,3) + (pow(tex.r + 0.00000001, _ShoreFadeFactor)), offsets.z);
			
			if (tex.r > 0.0076*shoreWaveControl.z) {//if (tex.r  > 0.009) {//bigger takes less top ground //0.02 //0.0076
				//v.vertex.xyz += float3(0, 1 * pow((1-tex.r), 1), 0); //v0.1
				//v.vertex.xyz += float3(0, 15 * (1 - tex.r)*cos(offsets.x*_Time.y*0.00035)*cos(offsets.z*_Time.y*0.0003), 0);				

				//float diff = v.vertex.y - 1111.5 * (tex.r - 0.008);

				//v0.7
				//float diff = v.vertex.y - realHeight;
				//if (diff < 0) {
				//	diff = 0;
				//}

				//v.vertex.y = 1111.5 * tex.r + 1 * abs(diff);
				//v.vertex.y = 3 * realHeight + 5;
				//v.vertex.y += 1.4 * realHeight + 9;
				//v.vertex.y += 1.2 * realHeight + 3;

				if (tex.r < 0.01*shoreWaveControl.w + (0.0002*cos(_Time.y*0.75)*shoreWaveControl.y) ) { //0.014 //v0.7
					v.vertex.y += (1.4 * realHeight + 3);// *(0.013 - tex.r) * 3;
					//if (v.vertex.y < 4 * realHeight + 2.1) {//if (v.vertex.y < 3.3 * realHeight + 4.5) {
						//v.vertex.y = 3 * realHeight + 5;
						//v.vertex.y = 3.3 * realHeight + 4.5;
				//		v.vertex.y = 3.3 * realHeight + 3.48;// -v.vertex.y * 0.01;
						//v.vertex.y+= 3.4 * realHeight + 4.2;// -1 * abs(cos(0.2*v.vertex.y));
					//	v.vertex.y = 3.4 * realHeight + 4.2;
					//}

					/*if (v.vertex.y < 4 * realHeight + 2.1) {
						v.vertex.y = 3.4 * realHeight + 4.2;
					}*/
					if (v.vertex.y < 4 * realHeight + 2.1) {
						v.vertex.y = 3.3 * realHeight + 3.2 +0.4*abs(sin(_Time.y * 0.25)) + 0.55*abs(cos(_Time.y * 0.55 + 0.1));
						// +(abs(cos(_Time.y * 0.1)) + 0.3*abs(sin(_Time.y * 0.05)))*(v.vertex.y*0.3);// +1.48*abs(sin(_Time.y * 0.6)) + 1 * abs(cos(_Time.y * 1));// 1.5*abs(cos(_Time.y * 0.1) + sin(_Time.y * 0.2) + cos(_Time.y * 0.05));
					}
					//if (v.vertex.y < 4 * realHeight + 3.1) {
					//	v.vertex.y = 3 * realHeight + 2.2;
					//}
					//v.vertex.y +=  (abs(v.vertex.y + 4 * realHeight +2));

					//v0.7
					float diff = (0.22*v.vertex.y) - realHeight - depthBelowZero;// 8;
					if (diff <= 0) {
						v.vertex.y = (1 / 0.22)*realHeight + depthBelowZero + 3 + 0.85*(diff);// 11 + 0.85*(diff);
						//v.vertex.y = (1 / 0.22)*realHeight + depthBelowZero + 3 + 0.85*(diff)   -(tex.r-0.0076*shoreWaveControl.z) * 1111 * _regulateShoreWavesHeightC; //v0.8
						//v.vertex.y = (1 / 0.22)*realHeight + depthBelowZero + (3 + 0.85*(diff) - tex.r - 0.0076*shoreWaveControl.z * 1111)  * _regulateShoreWavesHeightC; //v0.8
					}
					v.vertex.y += 0.001* _regulateShoreWavesHeightC * 1/(tex.r - 0.0076*shoreWaveControl.z+0.001); //v0.8
				}
				else {
					//v0.7
					//float diff = (0.22*v.vertex.y) - realHeight - 8;
					//if (diff <= 0) {
					//	v.vertex.y = (1 / 0.22)*realHeight + 8 + 1.5*(diff);
					//}

				}
			}
			else {
				//v.vertex.xyz += float3(0, 5 * (1 - tex.r)*cos(offsets.x*_Time.y*0.00035)*cos(offsets.z*_Time.y*0.0003), 0);//v0.1 - 0.00234
				//////v.vertex.xyz += float3(0, 5 * (1 - tex.r)*(cos(offsets.x*_Time.y*0.00035)*cos(offsets.z*_Time.y*0.0003)), 0);

				v.vertex.xyz += float3(0,//225 * (1 - tex.r)*(cos(offsets.x*_Time.y*0.00035)*cos(offsets.z*_Time.y*0.0003)), 
					1 * (cos(offsets.x*_Time.y*0.00035 + (1 - tex.r))*cos(offsets.z*_Time.y*0.0003 + (1 - tex.r))),//(1 - tex.r)*(cos(offsets.x*_Time.y*0.00035)*cos(offsets.z*_Time.y*0.0003)),
					0);//225 * (1 - tex.r)*(cos(offsets.x*_Time.y*0.00035)*cos(offsets.z*_Time.y*0.0003)));

				v.vertex.xyz += float3(0, _regulateShoreWavesHeightA*15 * (1 - tex.r)*cos( 1 - tex.r -1), 0);
				//tex.r <= 0.0076*shoreWaveControl.z
				//v.vertex.xyz += float3(0, _regulateShoreWavesHeightA * 15 * (1 - tex.r)*cos(1 - tex.r - 1) + (0.0076*shoreWaveControl.z - tex.r)*3111, 0);//v0.3

				//v0.7
				float diff = (0.22*v.vertex.y) - realHeight ;
				if (diff <= 1) {
					//v.vertex.y = (1 / 0.22)*realHeight + depthBelowZero;// 8;
					v.vertex.y = ((1 / 0.22)*realHeight + depthBelowZero )-(0.0076*shoreWaveControl.z - tex.r) * 1111 * _regulateShoreWavesHeightB;// v0.8
				}
				//v.vertex.y += (0.0076*shoreWaveControl.z - tex.r) * 1111 * _regulateShoreWavesHeightB; //v0.8
			}
			//v.vertex.y = 4 * realHeight + 2.2;
			//v.vertex.y = 3.3 * realHeight + 4.5;

			//v1.1
			//v.vertex.y = -24 + (0.2*cos(_Time.y*0.75)*shoreWaveControl.y);
			//v.vertex.y += -24 + (0.2*cos(_Time.y*0.75)*shoreWaveControl.y);
			v.vertex.y -= 24;
		}
		else {
			v.vertex.xyz += float3(offsets.x, offsets.y , offsets.z);

			//v.vertex.xyz += float3(offsets.x, 6 * (1 - tex.r), offsets.z);
			
		}


		//v0.8 HDRP FLUID
		float4 fluid = tex2Dlod(_fluidTexture, float4(-worldSpaceVertex.xz / _TilingFFT, 0, 0));
		//v.vertex.y = v.vertex.y + abs(v.vertex.y* 1110 * pow(fluid.r, 3));
		//v.vertex.x = v.vertex.x + 0.5*abs(v.vertex.y * 1110 * pow(fluid.r, 3));
		//v.vertex.z = v.vertex.z + 0.5*abs(v.vertex.y * 1110 * pow(fluid.r, 3));
		//v.vertex.y = v.vertex.y + 5.2*abs(1* 8210 * pow(fluid.r, 4));

		//CETO
		//float4 uv = float4(v.vertex.xy, v.texcoord.xy);
		//float4 oceanPos;
		//float3 displacement;
		//float3 SampleDisplacement(float2 uv, float2 dux, float2 duy)
		//displacement = SampleDisplacement(v.vertex.xz, nrml.x, nrml.z);
		//OceanPositionAndDisplacement(uv, oceanPos, displacement);
		//v.vertex.xyz = v.vertex.xyz + float3(displacement.x*0,4*displacement.y, 0 * displacement.z); //oceanPos.xyz + ;
		//END CETO

		
		//LOCAL WAVES
		//_LocalWaveParams ("Local Wave Height cutoff, Amp,Freq,"
		
		//if(LocalWaves == 1){
		v.vertex = mul(unity_ObjectToWorld,(v.vertex));
		float3 BoatPos = _LocalWavePosition.xyz + _LocalWaveVelocity.xyz*0.02;//float3(10,0,10);
		float3 BoatToVertex = v.vertex - BoatPos;
		float dist = length(BoatToVertex);
		
		float SpeedFactor = dot(normalize(BoatToVertex),normalize(_LocalWaveVelocity))*1.0+0.7;
		
		if(SpeedFactor<0){
			SpeedFactor=0;
		}
		
		float Yoffset =   (_LocalWaveParams.y*SpeedFactor-0) * sin(_Time.y*(_LocalWaveParams.z)+v.vertex.x+v.vertex.z)/(pow(dist,1.9)+0.1) 
							+ (_LocalWaveParams.y*SpeedFactor)* cos(_Time.y*(_LocalWaveParams.z)+v.vertex.z+1.14f+v.vertex.x)/(pow(dist,1.8)+0.1);
							
						//+ (_LocalWaveParams.y-20)* sin(_Time.y*(_LocalWaveParams.z)+v.vertex.z+1.14f)/(pow(dist,1.8)+0.1) ;
						//+ (_LocalWaveParams.y-40)* cos(_Time.y*(_LocalWaveParams.z)+v.vertex.z)/(pow(dist,2)+0.1);
						
						//_LocalWaveVelocity
		
		if(abs(Yoffset) > _LocalWaveParams.x){
			Yoffset = sign(Yoffset)*(_LocalWaveParams.x + 0.08*(abs(Yoffset) - _LocalWaveParams.x));
		}

		//HDRP
		if (dist < _LocalWaveParams.w) {//if(dist > _LocalWaveParams.w){

			if (dist > _LocalWaveParams.w	/ 2) {
				v.vertex.y = v.vertex.y + 0.00001*abs(dot(_LocalWaveVelocity, BoatToVertex)) * (1 / pow(dist, 0.4)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));
			}
			//v.vertex.y += 2*Yoffset;
			//v.vertex.y = 1* Yoffset;
			//Yoffset = 3;	
			//Yoffset = 
			//v.vertex.y = v.vertex.y + (1/(dist* dist)) *_LocalWaveParams.y*0.006* (SpeedFactor * 10 * (0.5+1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));
			//v.vertex.y = min(v.vertex.y, _LocalWaveParams.x);
			
			v.vertex.y = v.vertex.y + 0.001*abs(dot(_LocalWaveVelocity, BoatToVertex)) * (1 / pow(dist,2)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));

			

			//v.vertex.y = v.vertex.y + SpeedFactor*0.000004*pow(0.05*abs(dot(_LocalWaveVelocity, BoatToVertex)),18);
			//v.vertex.y = v.vertex.y + 2.5*abs(cos(0.1*(v.vertex.x + v.vertex.z))) * SpeedFactor * 0.000004*pow(0.05*abs(dot(_LocalWaveVelocity, BoatToVertex)), 2);

			v.vertex.y = min(v.vertex.y, _LocalWaveParams.x);
		} 
		if (dist > _LocalWaveParams.w && dist < _LocalWaveParams.w * 1.5) {
			v.vertex.y = v.vertex.y + 0.05*abs(0.002*pow(dot(_LocalWaveVelocity, BoatToVertex),2)) * (1 / pow(dist, 3)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));

			//backtrail
			float3 boatBack = v.vertex - (_LocalWavePosition.xyz - _LocalWaveVelocity.xyz*0.04);
			v.vertex.y = v.vertex.y - 0.014*abs(0.000012*pow(dot(-_LocalWaveVelocity, boatBack), 3)) * (1 / pow(dist, 3)) *_LocalWaveParams.y*0.086* (1 * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));
		}
//		float _ScaleLocal = 6;
//		float _SpeedLocal = 0.0005;
//		float _FreqLocal = 0.0005;
//		half offsetVert = ((v.vertex.x*v.vertex.x)*(v.vertex.z*v.vertex.z));
//		half ValueOffset = _ScaleLocal * sin(_Time.w * _SpeedLocal + offsetVert * _FreqLocal);
		//v.vertex.y += ValueOffset;
		//}

		//MASK
	//	float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)); //v1.2
		//if (finalCol.r == 0) {
		//	finalCol.r = 0.01;
		//}
	//	float distToPlane = dot(camPlaneNormal, worldSpaceVertex - _WorldSpaceCameraPos); //v1.2
		//float distToNormal = dot(float3(camPlaneNormal.x, 0, camPlaneNormal.z), i.normalInterpolator);		
		
		//v1.6
		//if (distToPlane < cameraNearClip) {//if (distToPlane < cameraNearClip + cameraNearControl.x*54*abs(pow(distToNormal, cameraNearControl.y))) {
		//								   //return cameraNearColor;// float4(1, 1, 1, 1);
		//	v.vertex.y = v.vertex.y - cameraNearControl.x*pow(cameraNearClip - distToPlane, cameraNearControl.y);
		//}

	

		//v1.4
		//float4 vortexPosScale; - _InteractAmpFreqRad
		float3 SpeedFac = float3(0, 0, 0);
		float distB = distance(vortexPosScale.xz, v.vertex.xz);
		float distA = (distB + vortexPosScale.w) / (_InteractAmpFreqRad.w * 1);
		float distA1 = distB* distB / (_InteractAmpFreqRad.w * 1);
		if (length(v.vertex.xz - vortexPosScale.xz) < vortexPosScale.w) {
			//SpeedFac  =  3*(o.posWorld-_InteractPos) *_WaveControl1.w 
			//+ _InteractAmpFreqRad.z*(1.1*cross(float3(0,1,0.5),o.posWorld-_InteractPos) 
			//- 2.71*cross(float3(0,1,0),_InteractPos-o.posWorld))  
			//+ _InteractAmpFreqRad.x*(o.posWorld-_InteractPos) *_WaveControl1.w *sin(o.posWorld.z+_InteractAmpFreqRad.y*_Time.y);
			//_WaveXFactor = _WaveXFactor - (1-distA)*(1-distA)*SpeedFac.z;
			//_WaveYFactor = _WaveYFactor - (1 - distA)*(1 - distA)*SpeedFac.x;

			SpeedFac = 3 * (v.vertex - vortexPosScale.xyz) *1
				+ _InteractAmpFreqRad.z*(1.1*cross(float3(0,1,0), v.vertex - vortexPosScale.xyz)
				- 2.71*cross(float3(0,1,0), vortexPosScale.xyz - v.vertex))
				+ _InteractAmpFreqRad.x*(v.vertex - vortexPosScale.xyz) * 1 *sin(v.vertex.z+_InteractAmpFreqRad.y*_Time.y);

			v.vertex.x = v.vertex.x - 0.000005*(1 - distA1)*(1 - distA1)*SpeedFac.x;
			v.vertex.z = v.vertex.z - 0.000005*(1 - distA1)*(1 - distA1)*SpeedFac.z;
			//v.vertex.x = v.vertex.x - (1 - distA)*(1 - distA)*SpeedFac.z;
			//v.vertex.y = v.vertex.y - 100 * distA;// 1 / (distA + 0.1);
			v.vertex.y = v.vertex.y + vortexPosScale.y;
			v.vertex.y = v.vertex.y - 1500 / (distA1 +0.01);
			//v.vertex.z = v.vertex.z - (1 - distA)*(1 - distA)*SpeedFac.x;
		}

		//UPDATE vertex global space
		//worldSpaceVertex = v.vertex;

		//v1.3 - sample based on decalPosRot
		CamPos = float3(decalPosRot.x, 0, decalPosRot.y);// _DepthCameraPos;//_WorldSpaceCameraPos;		
		Origin = float2(CamPos.x - decalPosRot.w / 2, CamPos.z - decalPosRot.w / 2);
		UnscaledTexPoint = float2(worldPos.x - Origin.x, worldPos.z - Origin.y);
		ScaledTexPoint = float2(UnscaledTexPoint.x / (decalPosRot.w), UnscaledTexPoint.y / (decalPosRot.w));
		float sinA = sin(decalPosRot.z * 3.14 / 180);
		float cosA = cos(decalPosRot.z * 3.14 / 180);
		float2 newcoordsA = ScaledTexPoint; //move to center
		float2 newcoords = newcoordsA;
		newcoords.x = (newcoordsA.x * cosA) + (newcoordsA.y * (-sinA));
		newcoords.y = (newcoordsA.x * sinA) + (newcoordsA.y * cosA);

		float2 checkRot = float2(0.5, 0.5);
		float2 checkRot2 = checkRot;
		checkRot2.x = (checkRot.x * cosA) + (checkRot.y * (-sinA));
		checkRot2.y = (checkRot.x * sinA) + (checkRot.y * cosA);
		//if (abs(ScaledTexPoint.x) <= checkRot2.y && abs(ScaledTexPoint.y) <= checkRot2.x) {
		if (abs(ScaledTexPoint.x) <= 1 && abs(ScaledTexPoint.y) <= 1) {
			float4 fluidDecal = tex2Dlod(_fluidTexture, float4(clamp(newcoords, decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			float4 fluidDecalADV = tex2Dlod(_fluidTextureADVECT, float4(clamp(newcoords, decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			//baseColor.rgb += fluidDecal;
			float offset = decalPower.y;
			float4 fluidDecalUP = tex2Dlod(_fluidTexture, float4(clamp(newcoords + float2(0, offset), decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			float4 fluidDecalDN = tex2Dlod(_fluidTexture, float4(clamp(newcoords + float2(0, -offset), decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			float4 fluidDecalLE = tex2Dlod(_fluidTexture, float4(clamp(newcoords + float2(-offset,0), decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			float4 fluidDecalRI = tex2Dlod(_fluidTexture, float4(clamp(newcoords + float2(offset,0), decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			fluidDecal = (fluidDecalUP + fluidDecalDN + fluidDecalLE + fluidDecalRI + fluidDecal) / 2;
			if (fluidDecal.r < 1 && newcoords.x > decalEdgeTweak.x && newcoords.x < decalEdgeTweak.y &&  newcoords.y > decalEdgeTweak.x && newcoords.y < decalEdgeTweak.y) {
				//float fadeFactor = length(CamPos - worldPos) / (decalPosRot.w);
				//baseColor.rgb += (float3(1, 1, 1)*fluidDecal.r * 1.2 + saturate(float3(1, 1, 1)* fluidDecalADV.r*1.5) ) / pow(fadeFactor,4);
				//float3 glitA = tex2D(_ShoreTex, i.bumpCoords * 0.2).rgb;
				//baseColor.rgb += (float3(1, 1, 1)*fluidDecal.r*fluidDecal.r * 2.2 + saturate(float3(1, 1, 1)* fluidDecalADV.r*2.5)) * (glitA + 0.4);
				//_Foam.z = 0;
				//v.vertex.y = v.vertex.y + (float3(1, 1, 1)*fluidDecal.r*fluidDecal.r * 0.2 * decalPower.x + saturate(float3(1, 1, 1)* fluidDecalADV.r*2.5* decalPower.y));
				v.vertex.y = v.vertex.y + clamp(decalPower.z*pow(fluidDecal.r+ fluidDecalADV.r, decalPower.x)*2.5, -5, decalPower.w);
			}
		}



		//v1.9
		float heightA = 0;
		if (ripplesPos.w > 1) {
			CamPos = float3(ripplesPos.x, 0, ripplesPos.y);// _DepthCameraPos;//_WorldSpaceCameraPos;		
			Origin = float2(CamPos.x - ripplesPos.w / 2, CamPos.z - ripplesPos.w / 2);
			UnscaledTexPoint = float2(worldPos.x - Origin.x, worldPos.z - Origin.y);
			ScaledTexPoint = float2(UnscaledTexPoint.x / (ripplesPos.w), UnscaledTexPoint.y / (ripplesPos.w));
			float2 newcoordsB = ScaledTexPoint; //move to center		
			float4 ripplesHeight = tex2Dlod(ripplesTex, float4((newcoordsB), 0, 0));
			//v.vertex.y = v.vertex.y + clamp(30101*pow(ripplesHeight.r,3.5)* 1,0, 54);
			//v.vertex.y = v.vertex.y + clamp(12101 * pow(ripplesHeight.r, 3.2) * 1, 0, 6.6);

			 //heightA = clamp(130101 * pow(ripplesHeight.r, 3.5) * 1, -10, 55);
			 heightA = clamp(10101 * pow(ripplesHeight.r, 2.1) * rippleWaveParams.x, 0, 12* rippleWaveParams.y);
		}


		//v1.5 - PRO - FULL FFT
		//UNITY_INITIALIZE_OUTPUT(Input, o);
		//float3 worldPos = v.vertex;// mul(unity_ObjectToWorld, v.vertex);
		float4 worldUV = float4(worldPos.xz, 0, 0);
	//	o.worldUV = worldUV.xy;
	//	o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
		float3 view_vec = -(v.vertex.xyz - _WorldSpaceCameraPos.xyz);// _WorldSpaceCameraPos.xyz - v.vertex.xyz; //v0.1	//v2.0
		float viewDist = length(view_vec);// length(o.viewVector);

		float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
		float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
		float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
		
		float3 displacement = 0;
		float largeWavesBias = 0;

		//autoLOD = 1;

		float diffA = 800 - length(view_vec.xz);

		if (autoLOD == 1) {
			if (diffA>0) {
				displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0 * lengthScales.x; //v0.1 1/2

				lod_c0 = min(_LOD_scale * (LengthScale0 * 3) / viewDist, 1);
				displacement += 1 * tex2Dlod(_Displacement_c0, worldUV / (LengthScale0 * 3)) * lod_c0 * lengthScales.x; //v0.1 1/2
			}
			else {
				lod_c0 =  min(_LOD_scale* (LengthScale0 * 3) / viewDist, 1);
				displacement += 1*tex2Dlod(_Displacement_c0, worldUV / (LengthScale0 * 3)) * lod_c0 * lengthScales.x; //v0.1 1/2
			}
		}
		else if (autoLOD == 0) {
			displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0 * lengthScales.x; //v0.1 1/2
		}
		largeWavesBias = displacement.y;
//#if defined(MID) || defined(CLOSE)
		if (autoLOD == 1 && diffA>0) {
			displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1 * lengthScales.y; //v0.1 4/2
		}
		else if (autoLOD == 0){
			displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1 * lengthScales.y; //v0.1 4/2

			
		}
//#endif
//#if defined(CLOSE)
		if (autoLOD == 1 && length(view_vec.xz) < 300) {
			displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2 * lengthScales.z; //v0.1 15/15

			
		}
		else if (autoLOD == 0) {
			displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2 * lengthScales.z; //v0.1 15/15
		}
//#endif
	///	v.vertex.xyz += mul(unity_WorldToObject, displacement);


		//v.vertex.xyz = v.vertex.xyz + 0.75*displacement * lengthScales.w; //v0.1 *2
		//v1.8
		if (tex.r > 0) {//realHeight
			//v.vertex.xyz = v.vertex.xyz + 0.75*displacement * lengthScales.w; //v0.1 *2
			//v.vertex.y = -100;
		}
		else {
			v.vertex.xyz = v.vertex.xyz + 0.75*displacement * lengthScales.w; //v0.1 *2
		}


		//v.vertex.y = v.vertex.y + _LOD_scale * 1;
		o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
		//END v1.5 - PRO - FULL FFT


		//UPDATE vertex global space
		worldSpaceVertex = v.vertex;

		//v1.9		
		//v.vertex.y = v.vertex.y - 0.75*heightA;
		v.vertex.y = v.vertex.y + 2.35 * heightA * rippleWaveParams.z;
		v.vertex.y = v.vertex.y - 0.07 * pow(heightA,2*rippleWaveParams.w);

		//worldSpaceVertex = v.vertex;

		//v1.6
		//v1.2
		//MASK 
		//float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
		float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
		float3 camPlaneNormalA = float3(camPlaneNormal.x, 0, camPlaneNormal.z);
		float distToPlane = abs(dot(camPlaneNormalA, worldSpaceVertex - _WorldSpaceCameraPos)); // length(worldSpaceVertex - _WorldSpaceCameraPos);// dot(camPlaneNormal, worldSpaceVertex - _WorldSpaceCameraPos);
		//if (distToPlane > cameraNearClip + 0.45 || distToPlane < cameraNearClip - 0.45) {
		//if (distToPlane < cameraNearClip + 0.2){// && distToPlane > cameraNearClip - 0.2) {		
		if (waterAirMode == 1) {
			if (distToPlane < cameraNearClip - NearClipAdjust.x) {
				if (scale.x < 1) {
					v.vertex.y = -cameraNearControl.w;
					v.vertex.x = camPlaneNormal.x;
					v.vertex.z = camPlaneNormal.z;
					worldSpaceVertex = v.vertex;
				}
			}
		}
		else {
			if (distToPlane > cameraNearClip + 0.1) {
				if (scale.x < 1) {
					v.vertex.y = -cameraNearControl.w;
					worldSpaceVertex = v.vertex;
				}
			}
		}


		v.vertex = mul(unity_WorldToObject,(v.vertex));
		
		//END LOCAL WAVES
		
		
		tileableUv = mul(unity_ObjectToWorld,(v.vertex)).xz;
		
//		v.vertex.xyz += offsets;		
		// one can also use worldSpaceVertex.xz here (speed!), albeit it'll end up a little skewed
//		half2 tileableUv = mul(_Object2World,(v.vertex)).xz;
		
		if (tex.r > 0) {
			_BumpDirection.z = _BumpDirection.z + realHeight*0.2 * _BumpDirection.z;
			_BumpDirection.w = _BumpDirection.w + realHeight * 0.01 * _BumpDirection.w;
		}


		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex.xyz - _WorldSpaceCameraPos;

		o.pos = UnityObjectToClipPos(v.vertex);

		ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);

		
		//output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionRWS), _ProjectionParams.x);
		//o.grabPassPos = ComputeScreenPosB(TransformWorldToHClip(worldSpaceVertex.xyz), _ProjectionParams.x);
		//o.screenPos = ComputeScreenPosB(TransformWorldToHClip(worldSpaceVertex.xyz), _ProjectionParams.x);

		
		o.normalInterpolator.xyz = nrml;


		//FFT
		o.viewInterpolator.w = saturate(offsets.y + dd.y * 2);
		//	o.viewInterpolator.w = saturate(offsets.y + dd.y*21);
		//o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		//MULTI LIGHTS
		//o.lightDir = ObjSpaceLightDir(v.vertex);
	//	///////o.lightDir = normalize(WorldSpaceLightDir(v.vertex));
				
		//o.viewInterpolator.w = saturate(offsets.y);
		o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		
		

		//MULTI LIGHTS
		//o.lightDir = ObjSpaceLightDir(v.vertex);
		
	//	o.lightPos = mul(_LightMatix0,worldSpaceVertex);
		//o.vertPos = mul(_Object2World,v.vertex);
		
		
		o.lightDirPosx.xyz = ObjSpaceLightDir(v.vertex);
		
		o.lightPosYZvertXY.zw = worldSpaceVertex.xy;
		o.normalInterpolator.w = worldSpaceVertex.z;
		
		float3 LightPos = mul(_LightMatix0,worldSpaceVertex);
		o.lightDirPosx.w = LightPos.x;
		o.lightPosYZvertXY.xy = LightPos.yz;
		
		
		o.normal.xyz = v.normal.xyz;

		if (tex.r > 0) {
			if (tex.r > 0.0083) {
				//o.normal.w =  pow((1.1 + tex.r - 0.15), 3);// 0;// pow((1.1 + tex.r - 0.15), 2) + 0; //v0.1
				//o.normal.w = lerp(   0.000001, pow((1.1 + tex.r - 0.15), 3), saturate(pow(tex.r - 0.0083,2)*pow(10,6.1))) ;
				o.normal.w = tex.r * lerp(0.000001, pow((1.1 + tex.r - 0.15), 3), saturate(pow(tex.r - 0.0083, 2)*pow(10, 6.1)));//v1.2
			}			
			else {
				//o.normal.w = 0.000001;// 0.000001;//o.normal.w = pow((1.1 + tex.r - 0.15), 1) * pow(0.007 - tex.r,12);
				//o.normal.w = lerp(0.000001, pow((1.1 + tex.r - 0.15), 3), saturate(pow(tex.r - 0.0083, 2)*pow(10, 6.1))); //v1.1
				o.normal.w = tex.r * lerp(0.000001, pow((1.1 + tex.r - 0.15), 3), saturate(pow(tex.r - 0.0083, 2)*pow(10, 6.1))); //v1.2
			}
			//o.normal.w = lerp(0.000001, pow((1.1 + tex.r - 0.15), 3), saturate(pow(tex.r - 0.0083, 2)*pow(10, 6.1)));
			//o.normal.w = pow((0.000001 * tex.r), 2);
			//o.normal.w = pow((0.000001 * (1+tex.r)), 2);
		}
		else {
			o.normal.w = 0;
		}


		

		//multilights
		TRANSFER_VERTEX_TO_FRAGMENT(o);
		
		UNITY_TRANSFER_FOG(o,o.pos);
		return o;
	}

	//fixed4 _LightColor0;

	//v4.7 Rain ripples
	// Global Rain Properties passed in by Script
	float2 _Lux_WaterFloodlevel;
	float _Lux_RainIntensity;
	sampler2D _Lux_RainRipples;
	float4 _Lux_RippleWindSpeed;
	float _Lux_RippleAnimSpeed;
	float _Lux_RippleTiling;
	float _Lux_WaterBumpDistance;
	float2 ComputeRipple(float4 UV, float CurrentTime, float Weight)
	{
		float4 Ripple = tex2Dlod(_Lux_RainRipples, UV);
		// We use multi sampling here in order to improve Sharpness due to the lack of Anisotropic Filtering when using tex2Dlod
		Ripple += tex2Dlod(_Lux_RainRipples, float4(UV.xy, UV.zw * 0.5));
		Ripple *= 0.5;

		Ripple.yz = Ripple.yz * 2 - 1; // Decompress Normal
		float DropFrac = frac(Ripple.w + CurrentTime); // Apply time shift
		float TimeFrac = DropFrac - 1.0f + Ripple.x;
		float DropFactor = saturate(0.2f + Weight * 0.8f - DropFrac);
		float FinalFactor = DropFactor * Ripple.x * sin(clamp(TimeFrac * 9.0f, 0.0f, 3.0f) * 3.141592653589793);
		return Ripple.yz * FinalFactor * 0.35f;
	}
	//  Add Water Ripples to Waterflow
	float3 AddWaterFlowRipples(float2 i_wetFactor, float3 i_worldPos, float2 lambda, float i_worldNormalFaceY, float fadeOutWaterBumps)
	{
		float4 Weights = _Lux_RainIntensity - float4(0, 0.25, 0.5, 0.75);
		Weights = saturate(Weights * 4);
		float animSpeed = _Time.y * _Lux_RippleAnimSpeed;
		float2 Ripple1 = ComputeRipple(float4(i_worldPos.xz * _Lux_RippleTiling + float2(0.25f, 0.0f), lambda), animSpeed, Weights.x);
		float2 Ripple2 = ComputeRipple(float4(i_worldPos.xz * _Lux_RippleTiling + float2(-0.55f, 0.3f), lambda), animSpeed * 0.71, Weights.y);
		float3 rippleNormal = float3(Weights.x * Ripple1.xy + Weights.y * Ripple2.xy, 1);
		// Blend and fade out Ripples 
		return lerp(float3(0, 0, 1), rippleNormal, i_wetFactor.y * i_wetFactor.y * fadeOutWaterBumps * i_worldNormalFaceY*i_worldNormalFaceY);
	}
	//END v4.7 Rain ripples





	//float4 _ColorPyramidScale;
	SAMPLER(s_trilinear_clamp_sampler);
	float4 _RTHandleScaleHistory;

	half4 frag( v2f i, fixed facing : VFACE) : SV_Target
	{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		//0.3 - -0.35 , 3.0 map to 1.4,3,1.4
		//#define PER_PIXEL_DISPLACE _DistortParams.x
		//#define REALTIME_DISTORTION _DistortParams.y
		//#define FRESNEL_POWER _DistortParams.z

		float3 worldPos = float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w);

		float3 scale = float3(
			length(unity_ObjectToWorld._m00_m10_m20),
			length(unity_ObjectToWorld._m01_m11_m21),
			length(unity_ObjectToWorld._m02_m12_m22)
			);
		if (waterAirMode == 1) {
			
			float3 camPlaneNormalo = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
			float3 camPlaneNormalAo = float3(camPlaneNormalo.x, 0, camPlaneNormalo.z);
			float distToPlaneo = (dot(camPlaneNormalAo, worldPos - _WorldSpaceCameraPos));
			if (distToPlaneo > cameraNearClip + NearClipAdjust.y) {
				if (scale.x < 1) {
					discard;
				}
			}
		}

		if (scale.x < 1) {
			return float4(1, 1, 1, 1);
		}

		//v1.5 - PRO - FULL FFT
		//half depthRaw = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, 
		//	sampler_CameraDepthTexture, float2(i.screenPos.xy / i.screenPos.w) * float2(DepthPyramidScale.x, DepthPyramidScale.y) 
		//	+ float2(DepthPyramidScale.z, DepthPyramidScale.w), 0).r; //v0.4a HDRP 10.2

		//autoLOD = 1;

		//SAMPLE_TEXTURE2D_X_LOD URP v0.1
		half depthRaw = SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture,
			sampler_CameraDepthTexture, float2(i.screenPos.xy / i.screenPos.w) * float2(DepthPyramidScale.x, DepthPyramidScale.y)
			+ float2(DepthPyramidScale.z, DepthPyramidScale.w), 0).r; //v0.4a HDRP 10.2

		half depthFFT = 1.0 / (_ZBufferParams.x * depthRaw + _ZBufferParams.y);
		float2 IN_worldUV = i.viewInterpolator.xz + _WorldSpaceCameraPos.xz;
		float4 IN_screenPos = i.screenPos;
		float3 IN_viewVector = (i.viewInterpolator.xyz) - 0*_WorldSpaceCameraPos.xyz; //ALSO DEFINED BELOW - MERGE
		float4 derivatives = UNITY_SAMPLE_TEX2D(_Derivatives_c0, IN_worldUV / LengthScale0);//        IN.worldUV / LengthScale0);

		float diffA = 750 - length(IN_viewVector.xz);

		if (autoLOD == 1) {
			if (diffA > 0) {
				//displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0 * lengthScales.x; //v0.1 1/2
				//derivatives = UNITY_SAMPLE_TEX2D(_Derivatives_c0, IN_worldUV / (LengthScale0 * 1));
			}
			else {
				derivatives = UNITY_SAMPLE_TEX2D(_Derivatives_c0, IN_worldUV / (LengthScale0 * 3));
				//derivatives = UNITY_SAMPLE_TEX2D(_Derivatives_c0, IN_worldUV / (LengthScale0 * 3 * (diffA*100) ));
				//derivatives = abs(diffA*0.001) * UNITY_SAMPLE_TEX2D(_Derivatives_c0, IN_worldUV / (LengthScale0*3));
			}
		}


//#if defined(MID) || defined(CLOSE)
		if (autoLOD == 1 && diffA > 0) {
			derivatives += UNITY_SAMPLE_TEX2D_SAMPLER(_Derivatives_c1, _Derivatives_c0, IN_worldUV / LengthScale1) * i.lodScales.y;
		}
		else if (autoLOD == 0) {
			derivatives += UNITY_SAMPLE_TEX2D_SAMPLER(_Derivatives_c1, _Derivatives_c0, IN_worldUV / LengthScale1) * i.lodScales.y;
		}
//#endif
//#if defined(CLOSE)
		if (autoLOD == 1 && length(IN_viewVector.xz) < 300) {
			derivatives += UNITY_SAMPLE_TEX2D_SAMPLER(_Derivatives_c2, _Derivatives_c0, IN_worldUV / LengthScale2 * 7) * i.lodScales.z;
		}
		else if (autoLOD == 0) {
			derivatives += UNITY_SAMPLE_TEX2D_SAMPLER(_Derivatives_c2, _Derivatives_c0, IN_worldUV / LengthScale2 * 7) * i.lodScales.z;
		}
//#endif
		float2 slopeFFT = float2(derivatives.x / (1 + derivatives.z), derivatives.y / (1 + derivatives.w));	//MERGE
		float3 worldNormalFFT = normalize(float3(-slopeFFT.x, 1, -slopeFFT.y));		
//#if defined(CLOSE)
		float jacobian = UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c0, _Derivatives_c0, IN_worldUV / LengthScale0).x
			+ UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c1, _Derivatives_c0, IN_worldUV / LengthScale1).x
			+ UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c2, _Derivatives_c0, IN_worldUV / LengthScale2 * 7).x;
		jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));

		if (autoLOD == 1) {
			if (diffA > 0) {
				//displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0 * lengthScales.x; //v0.1 1/2
			}
			else {
				float factorA = 3;
				float factorB = 7;
				/*if (diffA > -50) {
					factorA = 1;
					factorB = 1;
				}else
				if (diffA > -100) {
					factorA = 1.5;
					factorB = 3.5;
				}
				else
				if (diffA > -150) {
						factorA = 2.2;
						factorB = 5;
				}*/
				//if (diffA > -300) {
				//	factorA = 3 * (abs(diffA)/300)+1;
				//	factorB = 7 * (abs(diffA)/300)+1;
				//}
				//factorA = lerp(1, 3, saturate(abs(diffA)/1001));
				//factorB = lerp(1, 7, saturate(abs(diffA) / 1010));
				jacobian = UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c0, _Derivatives_c0, IN_worldUV / (LengthScale0* factorA)).x
					+ UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c1, _Derivatives_c0, IN_worldUV / LengthScale1).x
					+ UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c2, _Derivatives_c0, IN_worldUV / LengthScale2 * factorB).x;
				jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
			}
		}

//#elif defined(MID)
//		float jacobian = UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c0, _Derivatives_c0, IN_worldUV / LengthScale0).x
//			+ UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c1, _Derivatives_c0, IN_worldUV / LengthScale1).x;
//		jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
//#else
//		float jacobian = UNITY_SAMPLE_TEX2D_SAMPLER(_Turbulence_c0, _Derivatives_c0, IN_worldUV / LengthScale0).x;
//		jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
//#endif
		//////////////////
		float2 screenUV = IN_screenPos.xy;// / IN_screenPos.w; ////////////
		float backgroundDepth = depthFFT;// LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV)); ////////////////
		float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN_screenPos.z);
		float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
		float foamFFT = tex2D(_FoamTexture, IN_worldUV * 0.04 + _Time.r*3).r;
		//jacobian += _ContactFoam * saturate(max(0, foamFFT - 1) * 5) * 0.9;	
		//jacobian += _ContactFoam * saturate(max(0, foamFFT - 1) * 5) * 0.9;
		jacobian = jacobian * 2;
		float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN_viewVector) * _RoughnessScale));		
		float3 viewDir = normalize(IN_viewVector);
		float3 H = normalize(worldNormalFFT - SUN_DIR);// _WorldSpaceLightPos0); //v1.1.1a, was -worldNormalFFT
		float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
		//float3 color =  lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * i.lodScales.w), i.lodScales.z);
		float3 color = lerp(_Color, (_Color + _SSSColor.rgb * ViewDotH * i.lodScales.w), i.lodScales.z+0.35); //v1.1.1a, i.lodScales.z+0.35 was 1, plus saturate removed 
		float fresnelFFT = dot(worldNormalFFT, viewDir); //MERGE
		fresnelFFT = saturate(1 - fresnelFFT);
		fresnelFFT = pow5(fresnelFFT);
		//o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
		//o.Albedo = lerp(0, _FoamColor, jacobian);
		//o.Smoothness = lerp(distanceGloss, 0, jacobian);
		//o.Metallic = 0;
		//o.Normal = WorldToTangentNormalVector(IN, worldNormalFFT);
		//i.normal.xyz = i.normal.xyz + worldNormalFFT;
		float3 colorFFT = (color) + _ContactFoam*jacobian* (_FoamColor*10 +foamFFT* _FoamColor);// 315 * lerp(color * (1 - fresnelFFT), 0, jacobian);
		//colorFFT = colorFFT + colorFFT*pow(jacobian*2+0.4,3) * (_FoamColor);
		//float fresnelFFT2 = dot(worldNormalFFT, float3(jacobian, jacobian,2* jacobian));
		//colorFFT = saturate(colorFFT * fresnelFFT2 * jacobian * ViewDotH*10);
		//UNITY_APPLY_FOG(i.fogCoord, baseColor); 
		//float3 finalColFFT = lerp(0, _FoamColor, jacobian) + 10*lerp(color * (1 - fresnelFFT), 0, jacobian);
		//return float4(finalColFFT, 0.5);//v0.5







		//v4.7 Rain ripples
		// Add Water Ripples
		float3 rippleNormal = float3(0, 0, 1);
		float2 wetFactor = float2(0.1, 0.1);
		//	Calculate miplevel (needed by tex2Dlod)
		// Calculate componentwise max derivatives
		//float2 dx1 = ddx(IN.LuxUV_MainAOTex.xy * _TextureSize * _MipBias);
		//float2 dy1 = ddy(IN.LuxUV_MainAOTex.xy * _TextureSize * _MipBias);
		//float d = max(dot(dx1, dx1), dot(dy1, dy1));
		//float d = 1;
		//float2 lambda = 0.5 * log2(d);
		float2 lambda = float2(1, 1);
		//float _Lux_WaterBumpDistance = 1;
		float fadeOutWaterBumps = saturate((_Lux_WaterBumpDistance - distance(_WorldSpaceCameraPos, worldPos)) / 5);
		if (_Lux_RainIntensity > 0) {
			//float wetFactor = 1;
			//rippleNormal = AddWaterFlowRipples(wetFactor, IN.worldPos, lambda, saturate(worldNormalFace.y), fadeOutWaterBumps);
			//rippleNormal = AddWaterFlowRipples(wetFactor, worldPos, lambda, saturate(n_pixel.y), fadeOutWaterBumps);
			rippleNormal = AddWaterFlowRipples(wetFactor, worldPos, lambda, 1, fadeOutWaterBumps);
		}
		//#ifdef Lux_WaterFlow
		// Refraction of flowing Water should be damped 
		//IN.LuxUV_MainAOTex.xy += offset + flowNormal.xy * _FlowRefraction + rippleNormal.xy;
		//#else
		// Ripples may fully effect Refraction
		//IN.LuxUV_MainAOTex.xy += offset + rippleNormal.xy;
		//#endif
		//o.Normal = lerp(o.Normal, normalize(flowNormal + rippleNormal), wetFactor.x);
		//n_pixel = n_pixel + 0.5 * rippleNormal;
		//n_pixel = lerp(n_pixel, normalize(rippleNormal), wetFactor.x);  
		//END v4.7 Rain ripples



		if (i.normal.w == 0) { //if not shore
			/*_DistortParams.x = _DistortParams.x * 4.5;
			_DistortParams.y = _DistortParams.y + 4.5;
			_DistortParams.z = _DistortParams.z / 3;*/
			_DistortParams.x = _DistortParams.x * 1.5;
			_DistortParams.y = _DistortParams.y + 1.5;
			_DistortParams.z = _DistortParams.z / 2.4;//4;
		}
		else {
			/*_DistortParams.x = lerp(_DistortParams.x, _DistortParams.x * 1.5, i.normal.w * 1000000);
			_DistortParams.y = lerp(_DistortParams.y, _DistortParams.y + 1.5, i.normal.w * 1000000);
			_DistortParams.z = lerp(_DistortParams.z, _DistortParams.z / 4.0, i.normal.w * 1000000);*/
			//float fadeFactor = pow(i.normal.w, 1) * pow(10,6);
			//float fadeFactor = pow(i.normal.w, 2) * pow(10, 10);
			float fadeFactor = saturate(1 / (pow(i.normal.w, 2)* pow(10, 0.2)*1));
			//_DistortParams.x = lerp(_DistortParams.x * 1.5, _DistortParams.x, 1);
			//_DistortParams.y = lerp(_DistortParams.y + 1.5, _DistortParams.y, 1);
			//_DistortParams.z = lerp(_DistortParams.z / 4.0, _DistortParams.z, 1);

			_DistortParams.x = lerp(_DistortParams.x, _DistortParams.x * 1.5, fadeFactor);
			_DistortParams.y = lerp(_DistortParams.y, _DistortParams.y + 1.5, fadeFactor);
			_DistortParams.z = lerp(_DistortParams.z, _DistortParams.z / 2, fadeFactor);
		}
		//_DistortParams.x = _DistortParams.x - i.normal.w*15;
		//_DistortParams.y = _DistortParams.y - i.normal.w*16500000;
		//_DistortParams.z = _DistortParams.z + i.normal.w*11111111;//i.normal.w = 2 * 10-7

		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, _DistortParams.x);//PER_PIXEL_DISPLACE

		//v1.7
		//sampler3D _Displace_T3D;
		//sampler3D _Normals_T3D;
		float4 tex3Da = tex3D(_Displace_T3D, float3(worldPos.xz*0.1, _Time.y * 1));
		//float4 tex3Db = tex3D(_Normals_T3D, float3(uv.xy*0.2, _Time.y*1));
		float3 tex3Db = UnpackNormalScale(tex3D(_Normals_T3D, float3(worldPos.xz*0.006*_NormalsOffset3DControls.w, _Time.y * 0.5)), 0.1);
		//finalCol = finalCol + finalCol * tex3Da*1.65 * tex3Db;
		//worldNormal = worldNormal * (1 - tex3Db*5) + tex3Db * 1;
		//worldNormal = worldNormal * tex3Db + tex3Db * 22;
		//worldNormal = worldNormal * (_NormalsOffset3DControls.x - tex3Db * 5 * _NormalsOffset3DControls.y) +_NormalsOffset3DControls.z * tex3Db;
		worldNormal = worldNormal * (_NormalsOffset3DControls.x - tex3Db * 5 * _NormalsOffset3DControls.y) + _NormalsOffset3DControls.z * tex3Db;

		//v1.5
		worldNormal = worldNormalFFT * worldNormal;



		//v4.7
		//worldNormal.xz = worldNormal.xz + 0.5*saturate(1110.5 * rippleNormal.xz);
		//worldNormal.xz = worldNormal.xz + saturate(fadeOutWaterBumps*_Lux_RainIntensity*0.5*saturate(1110.5 * rippleNormal.xz));
		worldNormal.xyz = worldNormal.xyz + 0.2*saturate(fadeOutWaterBumps*_Lux_RainIntensity*0.5*saturate(1110.5 * rippleNormal.xzy));
		//worldNormal.xz = lerp(worldNormal.xz, normalize(rippleNormal), fadeOutWaterBumps*_Lux_RainIntensity*wetFactor.x);



		//v1.9 - RIPPLES
		float4 ripplesFOAM = 0;
		float3 outNormal = float3(0, 0, 0);
		float3 outNormalA = float3(0, 0, 0);
		if (ripplesPos.w > 1) {
			float3 CamPosAA = float3(ripplesPos.x, 0, ripplesPos.y);// _DepthCameraPos;//_WorldSpaceCameraPos;		
			float2 OriginA = float2(CamPosAA.x - ripplesPos.w / 2, CamPosAA.z - ripplesPos.w / 2);
			float2 UnscaledTexPointA = float2(worldPos.x - OriginA.x, worldPos.z - OriginA.y);
			float2 newcoordsB = float2(UnscaledTexPointA.x / (ripplesPos.w), UnscaledTexPointA.y / (ripplesPos.w));
			//float2 newcoordsB = ScaledTexPoint; //move to center		
			float4 ripplesHeight = tex2D(ripplesTex, float2((newcoordsB)));
			float heightA = clamp(130101 * pow(ripplesHeight.r, 3) * 1, 0, 45);

			
			//NormalFromHeight_World(130101 * pow(ripplesHeight.r, 3), worldSpaceVertex,outNormal);
			//outNormal = HeightToNormal(130101 * pow(ripplesHeight.r, 3), nrml.xyz, worldSpaceVertex);		
			float3 worldDirivativeX = ddx(worldPos * 100);
			float3 worldDirivativeY = ddy(worldPos * 100);
			float3 crossX = cross(worldNormal, worldDirivativeX);
			float3 crossY = cross(worldNormal, worldDirivativeY);
			float3 d = abs(dot(crossY, worldDirivativeX));
			float3 inToNormal = ((((heightA + ddx(heightA)) - heightA) * crossY) + (((heightA + ddy(heightA)) - heightA) * crossX)) * sign(d);
			inToNormal.y *= -1.0;

			outNormalA = ripplesHeight.rgb;

			outNormal = normalize((d * worldNormal) - inToNormal);
			//nrml = nrml + outNormal;//
			//worldNormal.xyz = worldNormal.xyz + clamp(0.5 * outNormal,-0.15,0.15);
			//nrml = nrml + float3(0,0,130101 * pow(ripplesHeight.r, 3));
			//worldNormal.xyz = worldNormal.xyz  + float3(0, 0, 130101 * pow(ripplesHeight.r, 3));
			//return float4(outNormal, 1);
			worldNormal.xyz += 0.4 * outNormal / length(outNormal);//
			//worldNormal = worldNormal / length(worldNormal);
			ripplesFOAM = float4(heightA.xxx, 1) * 0.055; //worldNormal.xyzz*1;
			//ripplesFOAM = float4(heightA.xxx, 1) * 0.005 * clamp(worldDirivativeX.x * worldDirivativeY.x * worldDirivativeY.z * worldDirivativeY.z * 1110, -29, 29); //worldNormal.xyzz*1;
			//return float4(ripplesFOAM.xyz,1);
		}




		half3 viewVector = normalize(i.viewInterpolator.xyz);

		//half4 distortOffset = half4(worldNormal.xz * _DistortParams.y * 10.0, 0, 0);//REALTIME_DISTORTION //v0.9
		//half4 distortOffset = half4(worldNormal.xz * _DistortParams.y * 10.0, 0, 0) + 0*half4(i.normal.xz,0,0);//REALTIME_DISTORTION //v0.9 - Distort on waves
		half4 distortOffset = half4(worldNormal.xz * _DistortParams.y * 10.0, 0, 0);// +11 * half4(i.normalInterpolator.xz, 0, 0);//REALTIME_DISTORTION //v0.9 - Distort on waves
		half4 screenWithOffset = i.screenPos + distortOffset;
		half4 grabWithOffset = i.grabPassPos + distortOffset;

		float3 vertexPos = float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w);
		
		//half4 rtRefractionsNoDistort = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(i.grabPassPos));
		//half4 rtRefractionsNoDistort = 0;// LOAD_TEXTURE2D_X_LOD(_ColorPyramidTexture, UNITY_PROJ_COORD(i.grabPassPos).xy, 0); //v1.1 //v1.2

		//DEPTH
		//half refrFix = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset));
		//half refrFix = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset).xy, 0).r;
		float2 samplingPositionNDC = float4(grabWithOffset.xy / grabWithOffset.w, 0, 0).xy;
		float2 samplingPositionNDC_DEPTH = float4(i.screenPos.xy / i.screenPos.w, 0, 0).xy;
		//float refrFix = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, s_trilinear_clamp_sampler,
			//samplingPositionNDC_DEPTH, 0).r; //v1.1
		//half depth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos.xy/ i.screenPos.w), 0).r;
		//half depthRaw = LOAD_TEXTURE2D_X_LOD(_DepthTexture, samplingPositionNDC_DEPTH*_ScreenSize.xy, 0).r;
		//half depthRaw = LOAD_TEXTURE2D_X_LOD(_DepthTexture, samplingPositionNDC_DEPTH*_ScreenSize.xy, 0).r; ///--- HDRP unity 2019.3 v0.1
		//half depthRaw = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, samplingPositionNDC_DEPTH, 0).r; ///--- HDRP unity 2019.3 v0.1

		//frag
		//float sceneZ = LinearEyeDepth(UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(i.projPos.xy / i.projPos.w, 0, 0), 0));
		//half4 grabColor = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorPyramidTexture, float4(i.uvgrab.xy / i.uvgrab.w, 0, 0), 0);
		
		//half depthRaw = UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(i.screenPos.xy * _DepthPyramidScale / i.screenPos.w, 0, 0), 0).r; ///--- HDRP unity 2019.3 v0.1
		//v1.5
		//half depthRaw = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, float2(i.screenPos.xy / i.screenPos.w) * float2(DepthPyramidScale.x, DepthPyramidScale.y) + float2(DepthPyramidScale.z, DepthPyramidScale.w), 0).r; //v0.4a HDRP 10.2

		//HDRP 2019.3 v0.1
		//_DepthPyramidScale

		//depth = LinearEyeDepth(depth);
		//depth = Linear01Depth(depth);

		//v1.2
		//half4 rtRefractionsNoDistort = SAMPLE_TEXTURE2D_X_LOD(_ColorPyramidTexture, s_trilinear_clamp_sampler,
		//	float2(samplingPositionNDC_DEPTH.x, clamp(samplingPositionNDC_DEPTH.y, 0, 1)), 0);

		// Z buffer to linear 0..1 depth (0 at camera position, 1 at far plane).
		// Does NOT work with orthographic projections.
		// Does NOT correctly handle oblique view frustums.
		// zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
		//float Linear01Depth(float depth, float4 zBufferParam)
		//{
			//return 1.0 / (zBufferParam.x * depth + zBufferParam.y); //_ZBufferParams
			half depth = 1.0 / (_ZBufferParams.x * depthRaw + _ZBufferParams.y); 
		//}
			//depth = 1 - depth; //2019.3 -- v0.1
		//return float4(float3(1,1,1)* i.screenPos.y*0.01, 1);
		//return float4(float3(1, 1, 1)* i.pos.y*0.0000000001* i.pos.y*depth, 1);
		//return float4(float3(depth, depth, depth)* 1, 1);

		//float depthRefrFix = 1-LinearEyeDepth(refrFix);	
		//return float4(depthRefrFix, depthRefrFix, depthRefrFix, 1);
		//return float4(refrFix, refrFix, refrFix, 1);


		//float4 grabPassTex = LOAD_TEXTURE2D_X_LOD(_ColorPyramidTexture, UNITY_PROJ_COORD(float4(grabWithOffset.x, grabWithOffset.y, grabWithOffset.z, grabWithOffset.w)).xy, 0);
		//half4 rtRefractions = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(grabWithOffset));
			half4 rtRefractions = 0;// LOAD_TEXTURE2D_X_LOD(_ColorPyramidTexture, UNITY_PROJ_COORD(float4(grabWithOffset.x, grabWithOffset.y, grabWithOffset.z, grabWithOffset.w)).xy, 0);//v1.1
		
		//return float4(rtRefractions.r, rtRefractions.g, rtRefractions.b,1);

	//	#ifdef WATER_REFLECTIVE
			//float4 uvsRFefl = UNITY_PROJ_COORD(screenWithOffset);
			//half4 rtReflections = tex2Dproj(_ReflectionTex, float4(uvsRFefl.x,  uvsRFefl.y, 1, 1));
			//half4 rtReflections = 2*tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(screenWithOffset));
		//HDRP
		float correctorY = (0.12 * _cameraTilt);//(0.14 * _cameraTilt);
		//if (correctorY > 0) {
			//correctorY = 0;	
		//}

		float correctorY2 = (0.0085 * _cameraTilt);
		//half4 rtReflections = 1 * tex2Dproj(_ReflectionTex, 
		//	UNITY_PROJ_COORD(half4(screenWithOffset.x, 1.4+screenWithOffset.y - correctorY, screenWithOffset.z, screenWithOffset.w)));
		//float2 samplingPositionNDC_DEPTH_REFR = float4(screenWithOffset.xy / screenWithOffset.w, 0, 0).xy;
		//half4 rtReflections = 1 * tex2Dproj(_ReflectionTex,
			//UNITY_PROJ_COORD(half4(samplingPositionNDC_DEPTH_REFR.x * 1, 1*(samplingPositionNDC_DEPTH_REFR.y * 1 - 1 * correctorY2), 1, 1)));
			//UNITY_PROJ_COORD(half4((0.25+samplingPositionNDC_DEPTH_REFR.x*0.5),(0.5*1 + 0.5*(samplingPositionNDC_DEPTH_REFR.y*1) - (30/ _cameraTilt)*correctorY2) * 1, 1, 1))); //1:1 screen
		//	UNITY_PROJ_COORD(half4(
		//	(0.5 - 0.25*(_ScreenSize.x / _ScreenSize.y) + samplingPositionNDC_DEPTH_REFR.x*0.5*(_ScreenSize.x/ _ScreenSize.y)),
				//(0.25 + 0.5*(samplingPositionNDC_DEPTH_REFR.y) ) , 
		//		(0.25 * _controlReflect.x + 0.5*(samplingPositionNDC_DEPTH_REFR.y)* _controlReflect.y + _controlReflect.z),
		//		1, 
		//		1)));//2:1 screen

		//v0.4a
		half4 distortOffsetB = half4(worldNormal.xz * _DistortParams.y * 10.0 * _controlReflect.y, 0, 0);// +11 * half4(i.normalInterpolator.xz, 0, 0);//REALTIME_DISTORTION //v0.9 - Distort on waves
		half4 screenWithOffsetB = i.screenPos + distortOffsetB;
		float2 samplingPositionNDC_DEPTH_REFR = float4(screenWithOffsetB.xy / screenWithOffsetB.w, 0, 0).xy;
		float2 coordsRefl = samplingPositionNDC_DEPTH_REFR.xy;
		float scaleWRefl = 1;		

		//Unity 2019.3.5 - HDRP 7.2.1
		half4 rtReflections = _controlReflect.x * tex2Dproj(_ReflectionTex, //v0.4a added reflection power
			//UNITY_PROJ_COORD(half4(samplingPositionNDC_DEPTH_REFR.x * 1, 1*(samplingPositionNDC_DEPTH_REFR.y * 1 - 1 * correctorY2), 1, 1)));
			//UNITY_PROJ_COORD(half4((0.25+samplingPositionNDC_DEPTH_REFR.x*0.5),(0.5*1 + 0.5*(samplingPositionNDC_DEPTH_REFR.y*1) - (30/ _cameraTilt)*correctorY2) * 1, 1, 1))); //1:1 screen
			UNITY_PROJ_COORD(half4(
				coordsRefl * _controlReflect.z + float2(saturate(offsetRflect.x), saturate(offsetRflect.y)), //v1.1a //v0.4a
				1,
				scaleWRefl)));//2:1 screen



		//half4 rtReflections = tex2Dproj(_ReflectionTex,
		//	UNITY_PROJ_COORD(half4(screenWithOffset.x/ screenWithOffset.w, screenWithOffset.y/ screenWithOffset.w, 0,0)));

		//half4 rtReflections = 1 * tex2Dproj(_ReflectionTex,
		//	UNITY_PROJ_COORD(half4(screenWithOffset.xy / screenWithOffset.w, 0,0)));



		//half4 rtRefractions1 = 2*LOAD_TEXTURE2D_X_LOD(_ColorPyramidTexture, UNITY_PROJ_COORD(half4(screenWithOffset)).xy, 0);
		//float2 samplingPositionNDC = UNITY_PROJ_COORD(half4(screenWithOffset.x/ 1, screenWithOffset.y/ 1, screenWithOffset.z, screenWithOffset.w)).xy;// lerp(posInput.positionNDC, hit.positionNDC, refractionOffsetMultiplier);
		//float2 samplingPositionNDC = UNITY_PROJ_COORD(half4(grabWithOffset.x / 1, grabWithOffset.y / 1, grabWithOffset.z, grabWithOffset.w)).xy;

		//float2 samplingPositionNDC = float4(grabWithOffset.xy / grabWithOffset.w, 0, 0).xy;

		//float3 rtRefractions1 = SAMPLE_TEXTURE2D_X_LOD(_ColorPyramidTexture, sampler_ColorPyramidTexture,
			// Offset by half a texel to properly interpolate between this pixel and its mips
		//	samplingPositionNDC * _ColorPyramidScale.xy, 0).rgb;;// preLightData.transparentSSMipLevel).rgb;

		///float3 rtRefractions1 = SAMPLE_TEXTURE2D_X_LOD(_ColorPyramidTexture, s_trilinear_clamp_sampler,			
		//	samplingPositionNDC * _RTHandleScaleHistory.xy*0.1, 0).rgb;  
		float camPosY = _WorldSpaceCameraPos.y;
		//float3 rtRefractions1 = SAMPLE_TEXTURE2D_X_LOD(_ColorPyramidTexture, sampler_ColorPyramidTexture,
		//	samplingPositionNDC * 1 * ( 0.24 * 4 / camPosY) *float2(_ColorPyramidScale.x, _ColorPyramidScale.y), 0).rgb;//camera height 4 = 0.24, 8 = 0.12, 12 = 0.08, 16 = 0.06
		//float3 rtRefractions1 = SAMPLE_TEXTURE2D_X_LOD(_ColorPyramidTexture, s_trilinear_clamp_sampler,
		//	samplingPositionNDC *1, 0).rgb ;
			//float2(samplingPositionNDC.x*0.5*(_ScreenSize.x / _ScreenSize.y), samplingPositionNDC.y*0.5), 0).rgb;

		//v1.1
		float3 rtRefractions1 = SAMPLE_TEXTURE2D_LOD(_ColorPyramidTexture, s_trilinear_clamp_sampler, //URP v0.1
			float2(samplingPositionNDC.x, clamp(samplingPositionNDC.y,0,1)) * 1 + float4(0,0,0,0), 0).rgb;


		//v0.1 URP -- shadergraph_LWSampleSceneColor(uv)
		rtRefractions1 = SampleSceneColor(float2(samplingPositionNDC.x, clamp(samplingPositionNDC.y, 0, 1)));

		
		// if (1-depth == 0) { depth = 0.999; } //depth = 0; } 
		// if (depth == 1) { depth = 0; } //depth = 0; } //unity 2019.3 v0.1 
		//if (1 - depth == 1) { depth = 0.001; } 

		

		//WATER HEIGHT
		if (camPosY < waterHeight) {
			isUnderwater = 100 - 100*(waterHeight - camPosY) / 20; //drop to 1 when camera is in -20, start from 100
			if (isUnderwater < 1) {
				isUnderwater = 1;
			}
			//_ReflectionColor.a = 1; v0.2
		}

		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);

		//v1.6
		half4 edgeBlendFactorsC = half4(1.0, 0.0, 0.0, 0.0);

		//#ifdef WATER_EDGEBLEND_ON
		//float4 screenPos = i.screenPos;
		//screenPos.y = 1 - screenPos.y;
		//	screenPos.x = screenPos.x / screenPos.w;
		//	screenPos.y = screenPos.y / screenPos.w;
		//screenPos.x = 1 - screenPos.x;
		//screenPos.y = 1 - screenPos.y;
		//half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos));
		//	half depth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos).xy, 0).r;
		//float2 samplingPositionNDC_DEPTH = float4(i.screenPos.xy / i.screenPos.w, 0, 0).xy;
		//half depth = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, s_trilinear_clamp_sampler,
		//	samplingPositionNDC_DEPTH, 0).r;

		//v1.2 - URP v0.1
		half depthRawD = SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, float2(grabWithOffset.xy / grabWithOffset.w) * float2(DepthPyramidScale.x, DepthPyramidScale.y) + float2(DepthPyramidScale.z, DepthPyramidScale.w), 0).r; //v0.4a HDRP 10.2

		//depth = LinearEyeDepth(1-depth);// *LinearEyeDepth(1 - depth)*LinearEyeDepth(1 - depth) * 11;
		//depth = LinearEyeDepth(depth);
		half depth2 = LinearEyeDepth(depthRawD); //LinearEyeDepth(depthRaw); //v1.2
		edgeBlendFactors = saturate(_InvFadeParemeter * (i.screenPos.w - depth2));//i.screenPos.w
		edgeBlendFactors.y = 1.0 - edgeBlendFactors.y;


		//v2.1
		half4 foamAAA = Foam(_ShoreTex, i.bumpCoords * 12.0 * 0.001 * _BumpTiling.xyzz + float4(1, 1, 0, 0) * _Time.y * 0.05 * _BumpDirection.xyzz / 100
			+ (1 - _Foam.w) * float4(12 * cos(0.12 * worldPos.x), 11 * sin(0.11 * worldPos.z), 0.26 * sin(0.4 * worldPos.z) * cos(1 * _Time.y + 0.1 * worldPos.z), 0.06 * sin(1 * worldPos.z + worldPos.x))
		);
		float4 FOAMAAAA = foamAAA * 0.01 * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _InvFadeParemeter.z)) * _InvFadeParemeter.w;


		//v1.6
		edgeBlendFactorsC = saturate(float4(_InvFadeParemeter.x, depthFadePower*0.005, _InvFadeParemeter.z, _InvFadeParemeter.w)  * (i.screenPos.w - depth2));
		edgeBlendFactorsC.y = 1.0 - edgeBlendFactorsC.y;
		edgeBlendFactors = edgeBlendFactorsC;

		if (isUnderwater == 0) {
			//v0.9
			float2 samplingPositionNDC = float4(grabWithOffset.xy / grabWithOffset.w, 0, 0).xy;					
			//half depthRawD = LOAD_TEXTURE2D_X_LOD(_DepthTexture, samplingPositionNDC*_ScreenSize.xy, 0).r;
			
			//half depthRawD = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, samplingPositionNDC, 0).r; //LOAD_TEXTURE2D_X_LOD(_DepthTexture, samplingPositionNDC*1, 0).r;	//2019.3 -- v0.1

			//URP v0.1
			//half depthRawD = UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(grabWithOffset.xy * _DepthPyramidScale / grabWithOffset.w, 0, 0), 0).r;//HDRP 2019.3 v0.1
			half depthRawD = SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, float2(grabWithOffset.xy / grabWithOffset.w) * float2(DepthPyramidScale.x, DepthPyramidScale.y) + float2(DepthPyramidScale.z, DepthPyramidScale.w), 0).r; //v0.4a HDRP 10.2

			//half depthD = 1.0 / (_ZBufferParams.x * depthRawD + _ZBufferParams.y);
			half depthD = 1.0 / (_ZBufferParams.x * depthRawD + _ZBufferParams.y);

			//depthD = 1 - depthD; //2019.3 -- v0.1
			//if (1 - depthD == 1) { depth = 0.999; }

			//depthRawD = LoadCameraDepth(samplingPositionNDC * 1, 0).r;
			//depthRawD = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, samplingPositionNDC, 0).r;

			//rtRefractions1 = float3(depthD, depthD, depthD);

			//rtRefractions1 = saturate(rtRefractions1 *pow((1 - depthD), depthFade)); //v0.8, was 130 //v0.9 //v1.1
			//rtRefractions1 = saturate(rtRefractions1 *pow((1 - depth), depthFade)); //v0.8, was 130 //v0.9
			//rtRefractions1 = (rtRefractions1 *pow((1 - depthD), depthFade) + rtRefractions1*3.5* _WorldSpaceCameraPos.y * i.normal.w); //v0.8, was 130 //v1.1
			//rtRefractions1 = 0.3*rtRefractions1 * pow((1 - depthD), depthFade) +(rtRefractions1 *pow((1 - depthD), depthFade)  * i.normal.w + abs(rtRefractions1 * 0.0005* _WorldSpaceCameraPos.y* i.normal.w)); //v0.8, was 130 //v1.1
			
			//rtRefractions1 = rtRefractions1 * pow((1 - depthD), depthFade);//HDRP 2019.3 v0.1
			rtRefractions1 = rtRefractions1 * pow((edgeBlendFactors.y), depthFade);//HDRP 2019.3 v0.2

			//HDRP 2019.3 v0.1
			//rtRefractions1 = 0.3*rtRefractions1 * pow((1-depth), depthFade) + (rtRefractions1 *pow((1-depth), depthFade)  * i.normal.w + abs(rtRefractions1 * 0.0005* _WorldSpaceCameraPos.y* i.normal.w)); //v0.8, was 130 //v1.1
			//rtRefractions1 = rtRefractions1 * pow((depthD), depthFade);
			//rtRefractions1 = saturate(rtRefractions1 *pow((1 - depthD), depthFade)); //v0.8, was 130 //v0.9 //v1.1
			//rtRefractions1 = 0;
			//rtRefractions1 = saturate(rtRefractions1 *pow((1 - depth), 400));
			//rtRefractions1 = saturate(rtRefractions1 *pow((depth)+0.4, 2));
			//rtRefractions1 = saturate(rtRefractions1 *pow((waterHeight - i.lightPosYZvertXY.w),1	)); //v0.8 - o.lightPosYZvertXY.zw = worldSpaceVertex.xy;
			float4 edgeBlendFactorsB = saturate(16 * _InvFadeParemeter * (i.screenPos.w - (depth)));//i.screenPos.w //v0.8
			//edgeBlendFactorsB.y = 1.0 - edgeBlendFactorsB.y;
			//rtRefractions1 = saturate(rtRefractions1 *pow((edgeBlendFactorsB.x)+0.5, 2.5));//v0.8 //v0.9
			//rtRefractions1 = saturate(rtRefractions1 *pow((edgeBlendFactorsB.x) + 0.2, 1.5));//v0.8 //v0.9 //v1.1
			
			//rtRefractions1 = saturate(rtRefractions1 *pow((edgeBlendFactorsB.x) + 0.2, 1.5));//v0.8 //v0.9 //v1.1 //HDRP 2019.3 v0.1
			//rtRefractions1 = saturate(rtRefractions1 *pow((1-edgeBlendFactors.x) , 2));
		}
		else {
			shoreGlow = 0; //v0.8
			if (camPosY > waterHeight) {
				isUnderwater =  (camPosY - waterHeight)/2 + 1;
				//rtRefractions1 = saturate(rtRefractions1 *pow((1 - depth), isUnderwater));
				rtRefractions1 = saturate(rtRefractions1 *pow((1 - depth), isUnderwater)); //v0.8
			}
		}

		//return float4(float3(depth, depth, depth) * 1, 1);
		//output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionRWS), _ProjectionParams.x);
		rtRefractions.rgb = rtRefractions1;

		//half4 rtRefractions1 = tex2Dproj(_ColorPyramidTexture,
		//	UNITY_PROJ_COORD(half4(screenWithOffset.x, 1.4 + screenWithOffset.y - correctorY, screenWithOffset.z, screenWithOffset.w)));
	//	#endif

		//float hitDeviceDepth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, screenWithOffset.xy, 0).r;// LoadCameraDepth(hit.positionSS);
		/*float LoadCameraDepth(uint2 pixelCoords)
		{
			return LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, pixelCoords, 0).r;
		}*/
		//float hitLinearDepth = LinearEyeDepth(1-0.1*hitDeviceDepth);// , _ZBufferParams);

		//return float4(hitLinearDepth, hitLinearDepth, hitLinearDepth, 1);
		//return float4(rtRefractions1.rgb,1);

		//v1.2
		if (i.screenPos.w - depth2 > 1) {
			rtRefractions = 0;
		}

		//#ifdef WATER_EDGEBLEND_ON
		//if (LinearEyeDepth(refrFix) < i.screenPos.z)
			//rtRefractions = rtRefractionsNoDistort;
		//#endif
		
		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize ((_WorldLightDir.xyz) + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0,pow (nh, _Shininess));
		
		

			//depth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos), 0).r; 
			//uint2 uvss = uint2(1,1);
			//depth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos).xy, 0).r;
		//#endif

			//return float4(depth, depth, depth, 1);
		
		// shading for fresnel term
		worldNormal.xz *= _FresnelScale;
		//half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, _DistortParams.z);//FRESNEL_POWER
		//half refl2Refr = FresnelFactor *Fresnel(viewVector, 0 + 1.1*i.normalInterpolator.xyz, FRESNEL_BIAS, _DistortParams.z)*i.normalInterpolator.z*i.normalInterpolator.x*72;//FRESNEL_POWER //v0.6
		//half refl2Refr = FresnelFactor * Fresnel(viewVector, worldNormal*i.normalInterpolator.xyz, FRESNEL_BIAS, _DistortParams.z);
		half refl2Refr = FresnelFactor * Fresnel(viewVector , worldNormal + i.normalInterpolator.xyz*worldNormal, FRESNEL_BIAS, _DistortParams.z);
		
		//FFT
		//float3 LightDir = i.lightDirPosx.xyz;
		float3 vertexPosA = float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w);
		float3 vertexToLightD = SUN_DIR - vertexPosA; //_WorldSpaceLightPos0.xyz - vertexPosA; v0.2
		float3 LightDir2D = normalize(vertexToLightD);

		//v0.4
		//float3 Lamb = saturate(dot(_WorldLightDir, i.normal));//  i.lightDir, i.normal));
		float3 Lamb = saturate(dot(i.lightDirPosx.xyz, i.normal));

		//o.lightPosYZvertXY.zw = worldSpaceVertex.xy;
		//o.normalInterpolator.w = worldSpaceVertex.z;
		//o.viewInterpolator.xyz = worldSpaceVertex.xyz - _WorldSpaceCameraPos;
		//float3 In1 = worldNormal;
		float3 In1 = i.normal;
		float3 In11 = worldNormal;
		float3 In2 = normalize(_WorldSpaceCameraPos - float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w)) * 1;
		float3 In22 = (_WorldSpaceCameraPos - float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w)) * 1;
		//In1 = -i.viewInterpolator.xyz;
		//float Out1 = RampSpec(In1,In2);
		float Doty = abs(dot(In2, In1));
		float Out1 = tex2D(_RampTex, float2(Doty, 0)).a;
		float Doty1 = abs(dot(In22, In11));
		float Out11 = tex2D(_RampTex, float2(Doty1, 0)).a;
		//FFT
		float3 Rampy = lerp(float3(0.1, 0.1, 0.1) * 1, 1 * float3(0.7, 0.7, 0.8), (Out1));
		//*Lamb*Rampy
		float3 Rampy2 = lerp(float3(0.1, 0.1, 0.1) * 1, 1 * float3(0.9, 0.9, 0.9), (Out11));
		//FFT
		float3 RampyF = lerp(Rampy, Rampy2, 0.8);



		// base, depth & reflection colors
		//half4 baseColor = ExtinctColor (_BaseColor, i.viewInterpolator.w * _InvFadeParemeter.w + _BaseColor*11);////////// Sky Master 3.0
		
		//float3 worldPos = float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w);

		//v3.4.4
		//half4 baseColor = _BaseColor - ( i.viewInterpolator.w * _InvFadeParemeter.w + _BaseColor*_MultiplyEffect) * half4(0.15, 0.03, 0.01, 0);		
		//v3.4.4
		//worldNormal.xz *= _FresnelScale;
		//half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);	
		//half4 baseColor = _BaseColor - 0.5*(saturate(i.viewInterpolator.w) * _InvFadeParemeter.w + _BaseColor * _MultiplyEffect) * float4(_foamColor.r, _foamColor.g, _foamColor.b, 0);
		//half4 baseColor = _BaseColor - 0.5*(saturate( lerp(i.viewInterpolator.w,0,1 ) ) * _InvFadeParemeter.w * 1 + _BaseColor * _MultiplyEffect) * float4(_foamColor.r, _foamColor.g, _foamColor.b, 0);
		half4 baseColor = _BaseColor 
			- 0.5*(saturate(i.viewInterpolator.w*(worldPos.y - waterHeight - offsetTopWaterLight)) * _InvFadeParemeter.w + _BaseColor * _MultiplyEffect) 
			* float4(_foamColor.r, _foamColor.g, _foamColor.b, 0);
		
		//float SSSFactor = CalculateSSS(normalize(i.normalInterpolator.xyz) * 101011, (float3(_WorldLightDir.x, 0, _WorldLightDir.z)), pow(i.viewInterpolator.xyz, 1));
		//baseColor = float4(baseColor.rgb * (SSSFactor*2.7*sssFactor + sssBackSideFactor*(1 - 20*SSSFactor)), baseColor.a) - float4(0.01*baseColor.xyz*(i.normalInterpolator.xyz), 0);
		
		//v0.8
		float Yfactor = 0;// _WorldLightDir.y - 0.2;//v0.9
		if ((_WorldLightDir.y) <= -0.1) {
			//Yfactor = 1 - 1 * (0.1 / abs(pow(_WorldLightDir.y,1)));// (0.1 - abs(_WorldLightDir.y));
			Yfactor = 0.2 - 0.2 * (0.1 / abs(pow(_WorldLightDir.y, 1)));
		}
		float darken = 1;
		if ((_WorldLightDir.y) > -0.05){//0.01) {
			//darken =1.15 -1 + 1.2 * (0.01*0.01*0.01 / abs(pow(_WorldLightDir.y,3)));// (0.1 - abs(_WorldLightDir.y));
			//darken = 1.15 - 1 + 1.2 * (0.05*0.05*0.05 / (pow(_WorldLightDir.y, 3)+ 0.001));
			darken = 1.35 - 1 + 1.2 * (0.05*0.05*0.05 / (pow(_WorldLightDir.y, 3) + 0.005));
		}
		//float SSSFactor = CalculateSSSA((i.normalInterpolator.xyz) * 101111, (float3(_WorldLightDir.x, _WorldLightDir.y, _WorldLightDir.z)), pow(i.viewInterpolator.xyz, 1));
		float SSSFactor = CalculateSSSA(worldNormal, (i.normalInterpolator.xyz) * 101111, (float3(_WorldLightDir.x, _WorldLightDir.y+0.3*Yfactor, _WorldLightDir.z)), pow(i.viewInterpolator.xyz, 1)); //v0.9
		float SSSFactorA = CalculateSSSA(worldNormal,(i.normalInterpolator.xyz) * 101111, (float3(_WorldLightDir.x, _WorldLightDir.y+0.2*(Yfactor+0.3), _WorldLightDir.z)), pow(i.viewInterpolator.xyz, 1));
		//baseColor = float4(baseColor.rgb * (SSSFactor*2.7*pow(sssFactor,2.5)*(i.normalInterpolator.y)*(i.normalInterpolator.y) + sssBackSideFactor * (1 - 20 * SSSFactor)), baseColor.a)
		//	- float4(0.01*baseColor.xyz*(i.normalInterpolator.xyz), 0) - float4(1,1,1,1)*0.1;
		//baseColor = float4(baseColor.rgb * (SSSFactor*0.02*sssFactor + SSSFactor*1111*sssFactor*pow(i.normalInterpolator.x,3) + sssBackSideFactor * (20 - 20 * SSSFactor)), baseColor.a) ;
		//baseColor = float4(baseColor.rgb * (SSSFactor*0.02*sssFactor + SSSFactor * 1111 * sssFactor*(pow(i.normalInterpolator.x, 3)+pow(i.normalInterpolator.z, 3)) + sssBackSideFactor * (20 - 20 * SSSFactor)), baseColor.a);
		//baseColor = float4(baseColor.rgb * (SSSFactor*4.7*sssFactor + sssBackSideFactor * (1 - 20 * SSSFactor)), baseColor.a) - float4(0.01*baseColor.xyz*(i.normalInterpolator.xyz), 0);
		//baseColor = float4(baseColor.rgb * (saturate(SSSFactor*4.7*sssFactor) + sssBackSideFactor * (1 - 20 * SSSFactor)), baseColor.a) - float4(0.01*baseColor.xyz*(i.normalInterpolator.xyz), 0);//v0.9
		SSSFactorA = clamp(SSSFactorA, 0.46, 2)*1; //v0.9
		//baseColor = float4(baseColor.rgb * (darken* saturate(SSSFactorA)*4.7*sssFactor*_sssColor.rgb + darken * sssBackSideFactor * (1 - 20 * SSSFactor)), baseColor.a) - float4(0.01*baseColor.xyz*(i.normalInterpolator.xyz), 0);//v0.9
		baseColor = float4(baseColor.rgb * (1* saturate(SSSFactorA)*4.7*sssFactor*_sssColor.rgb + 1 * sssBackSideFactor * (1 - 20 * SSSFactor)), baseColor.a) - float4(0.01*baseColor.xyz*(i.normalInterpolator.xyz), 0);//v0.9
		//baseColor = clamp(baseColor,0.05,2);


		//FFT
		Lamb = Lamb * 0.25; //v0.4
		baseColor.rgb = baseColor.rgb + 0.4*(Lamb*Lamb * 1 * min(0.9, _LightColor0) + Lamb * 3.2*baseColor.rgb*min(0.9, _LightColor0));


	//	#ifdef WATER_REFLECTIVE
			half4 reflectionColor = lerp (rtReflections*_ReflectionColor,_ReflectionColor,_ReflectionColor.a);
	//	#else
			//half4 reflectionColor = _ReflectionColor;
	//	#endif
		
		//baseColor = lerp (lerp (rtRefractions, baseColor, baseColor.a), reflectionColor / 50, refl2Refr);
		//v3.4.4
		//v3.4.4
		float4 bumpCycle = i.bumpCoords;
		float3 normaL1 = worldNormal;
		//half4 foam1 = Foam(_ShoreTex, bumpCycle * 2.0 + 0.1*normaL1.y) * normaL1.y* normaL1.y* normaL1.y;
		half4 foam1 = Foam(_ShoreTex, bumpCycle * 2.0 + 0.001*(i.normalInterpolator.xyzw)*(abs(cos(_Time.y*0.5)+1) - 1)) * 2 / (1);//v0.6
		//half4 foam2 = Foam(_ShoreTex, bumpCycle * -1.5) * normaL1.y* normaL1.y* normaL1.y;
		half4 foam2 = Foam(_ShoreTex, bumpCycle * -0.7) * normaL1.y* normaL1.y;//v0.4
		//baseColor.rgb += (clamp(foam1.rgb, foamContrast, 1) + edgeBlendFactors.y*0.9*clamp(cos(_Time.y*0.2), 0.5, 1)) * _Foam.x * (edgeBlendFactors.y*0.9 + saturate(saturate(pow(i.viewInterpolator.w, 3)*0.2*_Foam.x) - _Foam.y));
		//baseColor.rgb += (clamp(foam1.rgb, foamContrast, 1) + edgeBlendFactors.y*0.9*clamp(cos(_Time.y*0.2), 0.5, 1)) * 0 * (edgeBlendFactors.y*0.9 + saturate(saturate(pow(i.viewInterpolator.w, 3)*0.2*_Foam.x) - _Foam.y));
		//baseColor = (baseColor + edgeBlendFactors.x*spec * _SpecularColor * 2);
		//baseColor = lerp(lerp(rtRefractions, lerp(baseColor, _foamColor, edgeBlendFactors.y * 1), 0.3*edgeBlendFactors.x + 0.7*baseColor.a),
		//	reflectionColor + 0.1*foam1* _Foam.x* (edgeBlendFactors.y * 9 + saturate(saturate(i.viewInterpolator.w) - _Foam.y)), refl2Refr);
		//baseColor = lerp(lerp(rtRefractions, baseColor, baseColor.a), reflectionColor / 50, refl2Refr);//v0.9
		baseColor = lerp(lerp(rtRefractions, baseColor, _SpecularColor.w), reflectionColor / 50, refl2Refr);//v0.9
		if (baseColor.r < 0.1 && baseColor.g < 0.1 && baseColor.b < 0.1) {
			if (camPosY > waterHeight && camPosY - waterHeight < 1.5) {
				//baseColor = baseColor * 2;
			}
		}

		//HDRP v0.1
		baseColor = clamp(baseColor, 0, 1); //fix black spot artifacts

		/*
		float3 LightDirN = normalize(i.lightDirPosx.xyz);
		float3 LightDir = i.lightDirPosx.xyz;	
		float diff = saturate(dot(worldNormal, LightDirN));
		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize((_WorldLightDir.xyz) + viewVector.xyz);
		float nh = max(0, dot(worldNormal, -h));
		float spec = max(0.0, pow(nh, _Shininess));*/
		//baseColor = baseColor + spec * _SpecularColor * (0.02+(0.1*dot(-viewVector, i.normalInterpolator.x)) +saturate(-i.normalInterpolator.x*15)) *7*foam1;
		//baseColor = baseColor + spec * _SpecularColor * (0.05 + (0.1*dot(-viewVector, normaL1.x)) + 0.2*saturate(-normaL1.x * 11)) * 6 * 1;
		//baseColor = baseColor + spec * _SpecularColor * ((0.0006*( abs(dot(-viewVector, -i.lightDirPosx.xyz)) + abs(dot(-viewVector, i.lightDirPosx.xyz)))) + 0.02*saturate(-normaL1.x * 11)) * 1 * 1;
		//baseColor = baseColor + spec * _SpecularColor * (8 - 130*(i.normalInterpolator.x));
		//baseColor = baseColor + spec * _SpecularColor * (8);
		//baseColor = baseColor + spec * _SpecularColor * (0.01 + (0.01*dot(-viewVector, i.normalInterpolator.x)) + saturate(-i.normalInterpolator.x * 35  * ((normaL1.x*1 + normaL1.z) * 2))) * 22 * 1;
		//baseColor = baseColor + spec * _SpecularColor * (0.01 + (0.01*dot(-viewVector, i.normalInterpolator.x)) + saturate(-i.normalInterpolator.x * 235 * ((-normaL1.x* normaL1.z) * 2))) * 22 * 1;

		float4 _Glitter = float4(0.1, -0.2, 0.3, 0.02)*0.1;
		float glit1 = tex2D(_ShoreTex, i.bumpCoords + 0.1*float2(0, _Glitter.x*_Time.z*0.01*sin(_Time.x * 3))).b;
		float glit2 = tex2D(_ShoreTex, i.bumpCoords + 0.1*float2(0, _Glitter.y*_Time.z*0.01*cos(_Time.x * 1))).g;
		float glit3 = tex2D(_ShoreTex, i.bumpCoords + 0.1*float2(_Glitter.z*_Time.z*0.01*cos(_Time.x * 3), 0)).r;
		float Allglit = (glit1 + glit2 + glit3) / 3;
		float glitterFactor = 1;
		if (Allglit > 0.61 && Allglit < 0.6515) {
			//baseColor.rgb = baseColor.rgb + 10 * float3(1, 0.5, 0);
			//_SpecularColor.rgb = _SpecularColor.rgb + 1110 * float3(1, 0.5, 0);
			glitterFactor = glitterFactor * 1110;
		}

		//baseColor = baseColor + spec * _SpecularColor * (0.01 + (0.01*dot(-viewVector, i.normalInterpolator.x)) + saturate(-i.normalInterpolator.x * 435 * 
		//	((-dot(-viewVector, normaL1.x)* dot(-viewVector, normaL1.z)) * 2))) * 32 ;
		
		//baseColor = baseColor + spec * _SpecularColor * (0.003 + (0.01*dot(-viewVector * sign(_GerstnerIntensity), i.normalInterpolator.x)) + 
		//	saturate(-i.normalInterpolator.x* sign(-_GerstnerIntensity) * 435 * glitterFactor *
		//	((-dot(-viewVector * sign(_GerstnerIntensity), normaL1.x)* dot(-viewVector * sign(_GerstnerIntensity), normaL1.z)) * 21 ))) * 32;// +0.15*abs(cos(2 * dot(normaL1.x, normaL1.y)));

		//v0.6
		float3 forwardA = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));//UNITY_MATRIX_IT_MV[2].xyz
		//int signer = sign(cross(float3(forwardA.xz,0), float3( _WorldSpaceCameraPos.xz + i.lightDirPosx.xz,0)).y);// +i.lightDirPosx.xyz).y);
		//int signer = -sign(forwardA.z)*sign(cross(float3(forwardA.z, 0, forwardA.z), abs(-sign(i.lightDirPosx.z-_WorldSpaceCameraPos.z)*float3(  i.lightDirPosx.x, 0,  i.lightDirPosx.z))).y);
		
		//signer = dot(float3(normaL1.z, 0, normaL1.z), float3(i.lightDirPosx.z, 0, i.lightDirPosx.z)  );
		//int signer =sign(normalize(i.lightDirPosx.x));

		//baseColor.rgb = baseColor.rgb + spec * _SpecularColor * (0.003 + (0*dot(-viewVector * sign(_GerstnerIntensity), i.normalInterpolator.x)) +
		//saturate(-i.normalInterpolator.x * 1*435 * glitterFactor *
		//	((-11 * 1*dot(normalize(_WorldSpaceLightPos0), (normaL1.z * normaL1.x))) * 5))) * 125* saturate(dot (i.normalInterpolator.xz, _WorldSpaceLightPos0.xz	));

		//LightDirN
		//baseColor = baseColor + saturate(spec * _SpecularColor * -1* (dot(i.lightDirPosx, normaL1.x) *dot(_WorldSpaceLightPos0, normaL1.z)) * 10)*12;
		baseColor.rgb = baseColor.rgb + spec * _SpecularColor * (10.113 + (10 * dot(-viewVector * sign(_GerstnerIntensity), i.normalInterpolator.x)) +
			saturate(-1 * 1 * 435 * glitterFactor *
				//((-11 * 1 * 1) * 5))) * 125 * saturate(dot(i.normalInterpolator.xz + normaL1.xz,-_WorldSpaceLightPos0.xz)-0.1);
			//((-11 * 1 * 1) * 5))) * 1125 * saturate(dot(-normaL1.xz, _WorldSpaceLightPos0.xz)) * saturate(worldNormal.x*1001+ worldNormal.z*100);// saturate(normaL1.x + normaL1.z);
			//((-1 * 1 * 1) * 5))) * 125 * saturate(dot(-normaL1.xz, -_WorldSpaceLightPos0.xz));// *1 * 10.1* saturate(dot(viewVector.xz, _WorldSpaceLightPos0.xz));
		((-1 * 1 * 1) * 5))) * 125 * saturate(dot(-normaL1.xz, -SUN_DIR.xz)); //v0.2

		// handle foam
		//float3 worldPos = float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w);
		//half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
		half4 foam = 21*Foam(_ShoreTex, i.bumpCoords * 2.0 
			+ float4(12 * cos(0.12*worldPos.x), 11 * sin(0.11*worldPos.z), 0.26 * sin(0.4*worldPos.z)*cos(1 * _Time.y + 0.1*worldPos.z), 0.06 * sin(1 * worldPos.z + worldPos.x)));

		//foam = foam * (1 - i.normal.w * 1);// *i.normal.w*i.normal.w*i.normal.w);
		//foam = i.normal.w * foam * 10 + foam;
		foam = i.normal.w*i.normal.w * 2 * (30 + foam) + 1 + foam - foamHighSea.x; //v0.7

		//v0.3
		//foam = foam * _GDirectionAB.x*i.normal.y*i.normal.y;
		foam = foam * 0.02 * foamHighSea.y + foam * dot(float3(worldNormal.x, 0, worldNormal.z), float3(_GDirectionCD.x, 0, _GDirectionCD.y)) * 2;// *cos(0.3*_Time.y + worldNormal.x);
			//+
			//dot(float3(worldNormal.x, 0, worldNormal.z), float3(_GDirectionAB.x, 0, _GDirectionAB.y))*2
			//+ dot(float3(worldNormal.x, 0, worldNormal.z), float3(_GDirectionAB.z, 0, _GDirectionAB.w)) * 2;
		foam = foam * cos(0.3*_Time.y + worldPos.x*0.1 +  worldPos.z*0.1	) + foam * dot(float3(worldNormal.x, worldNormal.y, worldNormal.z), float3(_WorldLightDir.x, _WorldLightDir.y, _WorldLightDir.z))*11;

		foam = foam - foamHighSea.z;//v0.7

		if (i.normal.w>0) {
			//foam = 0;
			foam = i.normal.w;//v0.4
		}

		foam = foam - foamHighSea.w;//v0.7

		if (i.screenPos.w - depth2 > 1) {
			foam1 = 0; foam2 = 0;
			foam = 0;
		}

		


		//v1.4 - vertexPosA, vertexPos, worldPos
		//o.lightPosYZvertXY.zw = worldSpaceVertex.xy;
		//o.normalInterpolator.w = worldSpaceVertex.z;
		//float4 vortexPosScale; - _InteractAmpFreqRad
		float3 SpeedFac = float3(0, 0, 0);
		float distB = distance(vortexPosScale.xz, vertexPosA.xz);
		float distA = (distB + vortexPosScale.w) / (_InteractAmpFreqRad.w * 1);
		float distA1 = (distB * distB) / (_InteractAmpFreqRad.w * 1);
		if (length(vertexPosA.xz - vortexPosScale.xz) < vortexPosScale.w) {

			SpeedFac = 3 * (vertexPosA - vortexPosScale.xyz) * 1
				+ _InteractAmpFreqRad.z*(1.1*cross(float3(0, 1, 0), vertexPosA - vortexPosScale.xyz)
					- 2.71*cross(float3(0, 1, 0), vortexPosScale.xyz - vertexPosA))
				+ _InteractAmpFreqRad.x*(vertexPosA - vortexPosScale.xyz) * 1 * sin(vertexPosA.z + _InteractAmpFreqRad.y*_Time.y);

			//v.vertex.x = v.vertex.x - 0.000005*(1 - distA1)*(1 - distA1)*SpeedFac.x;
			//v.vertex.z = v.vertex.z - 0.000005*(1 - distA1)*(1 - distA1)*SpeedFac.z;
			//float foamy = 0.1 * (foam + foam2 + foam1 + 1) - 0.000005*(1 - distA1)*(1 - distA1)*SpeedFac.x  - 0.000005*(1 - distA1)*(1 - distA1)*SpeedFac.z;
			//v.vertex.y = v.vertex.y + vortexPosScale.y;
			//v.vertex.y = v.vertex.y - 1500 / (distA1 + 0.01);	
			
			/*half4 foamC =  Foam(_ShoreTex, i.bumpCoords * 2.0*(2.6 - distA1)*0.000002 * 1 + 0.0005*SpeedFac.xzzz - 0.000002*SpeedFac.yyzz*distA1 + float4(0, 0.04 * _Time.y, 0, 0))* 0.64;
			foamC *= Foam(_ShoreTex, i.bumpCoords * 2.0*(2.6 - distA1)*0.0002 * 1 + 0.0007*SpeedFac.xzzz + 0.0002*SpeedFac.xyzz + float4(0, 0.3*_Time.y,0,0)) * 1.54;
			float foamy = 0.001*(1 - distA1)*(1 - distA1);			
			baseColor.rgb = baseColor.rgb + saturate(( 0.5*(foamC) +(foamC*baseColor.rgb*0.06*(foam))) / foamy);*/

			half4 foamC = Foam(_ShoreTex, i.bumpCoords * 2.0*(2.6 - distA1)*0.000002 * 1 + 0.0005*SpeedFac.xzzz - 0.000002*SpeedFac.yyzz*distA1 + float4(0, 0.04 * _Time.y, 0, 0))* 1.64;
			//foamC *= Foam(_ShoreTex, i.bumpCoords * 2.0*(2.6 - distA1)*0.0002 * 1 + 0.0007*SpeedFac.xzzz + 0.0002*SpeedFac.xyzz + float4(0, 0.3*_Time.y, 0, 0)) * 1.54;
			float foamy = 0.001*(1 - distA1)*(1 - distA1);
			baseColor.rgb = baseColor.rgb + saturate((0.5*(foamC)+(foamC*baseColor.rgb*0.06*(foam))) / foamy);

		}
		



		//v0.8 HDRP FLUID
		//float4 fluid = tex2D(_fluidTexture, float4(worldPos.xz / (_TilingFFT*4), 0, 0));
		//float4 fluid = tex2D(_fluidTexture, float4(_GDirectionAB.x*worldPos.x / (_TilingFFT * 1), _GDirectionAB.y*worldPos.z / (_TilingFFT * 1), 0, 0) + float4(1111,1,0,0));
		float4 fluid = tex2D(_fluidTexture, float4(_GDirectionAB.x*worldPos.x / (_TilingFFT * dynamicFoamControls.x), _GDirectionAB.y*worldPos.z / (_TilingFFT * dynamicFoamControls.y), 0, 0)
			+ float4(dynamicFoamControls.z, dynamicFoamControls.w, 0, 0));
		//v.vertex.y = v.vertex.y + abs(v.vertex.y* 1110 * pow(fluid.r, 3));
		//v.vertex.x = v.vertex.x + 0.5*abs(v.vertex.y * 1110 * pow(fluid.r, 3));
		//v.vertex.z = v.vertex.z + 0.5*abs(v.vertex.y * 1110 * pow(fluid.r, 3));
		//v.vertex.y = v.vertex.y + 5.2*abs(1 * 8210 * pow(fluid.r, 4));
		//_Foam.z = _Foam.z-_Foam.z * 0.01*(1 * 314210 * pow(fluid.r, 3))*110.5-0.1;
		//_Foam.z = _Foam.z * pow(1-fluid.r, 3) * 2011 -2212* (1-fluid.r);
		//_Foam.z = (_Foam.z - 2.5)*0.6;



		//HDRP - BOAT FOAM
		float3 BoatPos = _LocalWavePosition.xyz + _LocalWaveVelocity.xyz*0.02;//float3(10,0,10);
		float3 BoatToVertex = vertexPos - BoatPos;
		float dist = length(BoatToVertex);

		float SpeedFactor = dot(normalize(BoatToVertex), normalize(_LocalWaveVelocity))*1.0 + 0.7;
		if (dist < _LocalWaveParams.w*1) {//if(dist > _LocalWaveParams.w){

			if (dist > _LocalWaveParams.w / 2) {
				//v.vertex.y = v.vertex.y + 0.00001*abs(dot(_LocalWaveVelocity, BoatToVertex)) * (1 / pow(dist, 0.4)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));
			}			
			//v.vertex.y = v.vertex.y + 0.001*abs(dot(_LocalWaveVelocity, BoatToVertex)) * (1 / pow(dist, 2)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));
			//foam = foam+ (1/ pow(dist, 1))*0.0115*abs(dot(_LocalWaveVelocity, BoatToVertex)) * (1 / pow(dist, 2)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((vertexPos.x + vertexPos.z)*_LocalWaveParams.z))));
			foam = foam + (1 / pow(dist, 4))*0.0115*abs(pow((dot(_LocalWaveVelocity, BoatToVertex)),2)) * (1 / pow(dist, 2)) *_LocalWaveParams.y*0.086* (1 * 10 * (0.5 + 1.5*1));
			baseColor.rgb += clamp(foam/ pow(dist, 2),0,0.3);
			//v.vertex.y = min(v.vertex.y, _LocalWaveParams.x);
		}
		if (dist < _LocalWaveParams.w * 2.5) {
			//v.vertex.y = v.vertex.y + 0.05*abs(0.002*pow(dot(_LocalWaveVelocity, BoatToVertex), 2)) * (1 / pow(dist, 3)) *_LocalWaveParams.y*0.086* (SpeedFactor * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));

			//backtrail
			float3 boatBack = vertexPos - (_LocalWavePosition.xyz - _LocalWaveVelocity.xyz*0.18);
			//v.vertex.y = v.vertex.y - 0.014*abs(0.000012*pow(dot(-_LocalWaveVelocity, boatBack), 3)) * (1 / pow(dist, 3)) *_LocalWaveParams.y*0.086* (1 * 10 * (0.5 + 1.5*abs(cos((v.vertex.x + v.vertex.z)*_LocalWaveParams.z))));
			float3 foamB = foam + 0.124*abs(1.2*pow(abs(dot(-_LocalWaveVelocity, boatBack)), 1)) * (1 / pow(dist, 2)) *_LocalWaveParams.y*0.086;
			//baseColor.rgb += worldNormal.y*saturate(foamB/ pow(dist,3));
		}
		

		//foam = 0;// lerp(foam, 0, normaL1.y / 1.4 + saturate((worldPos.y - waterHeight / 138)));//v3.4.4
		float factorH =  clamp(0.18*(worldPos.y - waterHeight) + 0.1 * pow((worldPos.y-0.25 - waterHeight),7), 0, 6)*abs(0.7+cos(0.000002*_Time.y * worldPos.z)*cos(0.00001*_Time.y * worldPos.x));// (1);// / (worldPos.y * 4 - waterHeight)*0.2;
		//if (factorH < 0) {
		//	factorH = 1;
		//}

		//foam = foam *0.02* pow(worldPos.y - waterHeight - 1.5, 12) / factorH;

		factorH = factorH * (1 - i.normal.w);

		///foam = saturate(foam / factorH);
		
		//v1.2
		if (i.screenPos.w - depth2 > 1) {
			edgeBlendFactors.x = 1;
		}

	//	baseColor.rgb += factorH *saturate(foam.rgb) * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y* normaL1.x));//v3.4.4
	//	baseColor.rgb += saturate(1 * (foam.rgb) * _Foam.x * (edgeBlendFactors.y + 0));//v3.4.4

		//baseColor.rgb += 1 * saturate(1 * (edgeBlendFactors.y) * foam2.rgb * (_Foam.x));
		//baseColor.rgb += factorH * saturate(foam2.rgb) * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y* normaL1.x));
		//baseColor.rgb += factorH * saturate(foam.rgb) * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y* normaL1.x))*0.1 + 5*foam2.rgb* saturate(foam.rgb); //v0.5
		//baseColor.rgb += factorH * saturate(foam.rgb) * _Foam.x * (edgeBlendFactors.y*3 + saturate(i.viewInterpolator.w - _Foam.y* normaL1.x))*foam2.rgb * 1; //v0.5
		baseColor.rgb += factorH * saturate(foam.rgb) * _Foam.x * (edgeBlendFactors.y * 3 + saturate(i.viewInterpolator.w - _Foam.y* normaL1.x))*saturate(foam2.rgb) * 1; //v0.6
		//baseColor.rgb += saturate(1 * (foam.rgb) * _Foam.x * (edgeBlendFactors.y)) * 0.4;//v3.4.4

		//EDGE FOAM
		float4 edgeBlendFactorsA = saturate(6*_InvFadeParemeter * (i.screenPos.w - (depth2)));//i.screenPos.w
		edgeBlendFactorsA.y = 1.0 - edgeBlendFactorsA.y;
		//baseColor.rgb += 1*saturate(_Foam.z * 0.4*foam2 * (14*edgeBlendFactorsA.y + 0*saturate(i.viewInterpolator.w - _Foam.y* normaL1.x)));
		//baseColor.rgb +=  2*saturate( -_Foam.z * 0.5*foam1 * (5 * edgeBlendFactorsA.y)* (35 * normaL1.x*0.5+0.6)); //v0.4 put minus in Foam.z
	//	baseColor.rgb += 2 * saturate(-_Foam.z * 0.5*foam1 * (5 * edgeBlendFactorsA.y)* (35 * normaL1.x*0.5*(abs(cos(_Time.y*1*0.0001)) + abs(cos(_Time.y*0.0002))-0.8) + 0.6)); //v0.4
		//baseColor.rgb += 2 * saturate(-_Foam.z * 0.5*foam1 * (5 * pow(edgeBlendFactorsA.y,6))* (35 * normaL1.x*0.5*(abs(cos(_Time.y * 1 * 0.0001)) + abs(cos(_Time.y*0.0002)) - 0.8) + 0.6)); //v0.5

		//baseColor.rgb += 2 * saturate(-_Foam.z * 0.5*1 * (5 * pow(edgeBlendFactorsA.y, 6))
		///	* 1); //v0.5a

		//baseColor.rgb += 4*saturate(_Foam.z * 1* foam2.rgb * edgeBlendFactorsA.y);//v0.4
	//	baseColor.rgb += 1 * saturate((normaL1.y + 0.0)*_Foam.z * 1 * foam2.rgb * edgeBlendFactorsA.y);//v0.4
		//baseColor.rgb += 2* saturate((normaL1.y + 0.0)*_Foam.z * 1 * foam2.rgb * edgeBlendFactorsA.y 
		//	- 0.1*abs(cos(0.00005*(worldPos.x + worldPos.z)*_Time.y+_Time.y*0.2))
		//	- 0.1*abs(sin(0.00003*(worldPos.x + worldPos.z)*_Time.y + _Time.y*0.4))
		//);//v0.4

		//baseColor.rgb += 2 * saturate((normaL1.y + 0.0)*_Foam.z * 1 * foam2.rgb * edgeBlendFactorsA.y
		//	- 0.1*abs(cos(0.00005*(worldPos.x + worldPos.z)*cos(_Time.y) + cos(_Time.y)*0.2))
		//	- 0.1*abs(sin(0.00003*(worldPos.x + worldPos.z)*cos(_Time.y) + cos(_Time.y)*0.4))
		//);//v0.6	



		

		//dynamicFoamControls
		float _Foam_z = _Foam.z * pow(1 - fluid.r, 3) * 2011 - 2212 * (1 - fluid.r);
		_Foam_z = (_Foam_z - 2.5)*0.6;
		_Foam.z = _Foam_z * dynamicFoamControlsA.x + _Foam.z* (1 - dynamicFoamControlsA.x);

		

		//v1.3 - sample based on decalPosRot
		float3 CamPos = float3(decalPosRot.x, 0, decalPosRot.y);// _DepthCameraPos;//_WorldSpaceCameraPos;		
		float2 Origin = float2(CamPos.x - decalPosRot.w / 2, CamPos.z - decalPosRot.w / 2);
		float2 UnscaledTexPoint = float2(worldPos.x - Origin.x, worldPos.z - Origin.y);
		float2 ScaledTexPoint = float2(UnscaledTexPoint.x / (decalPosRot.w), UnscaledTexPoint.y / (decalPosRot.w));
		float sinA = sin(decalPosRot.z * 3.14 / 180);
		float cosA = cos(decalPosRot.z * 3.14 / 180);
		float2 newcoordsA = ScaledTexPoint; //move to center
		float2 newcoords = newcoordsA;
		newcoords.x = (newcoordsA.x * cosA) + (newcoordsA.y * (-sinA));
		newcoords.y = (newcoordsA.x * sinA) + (newcoordsA.y * cosA);

		float2 checkRot = float2(0.5, 0.5);
		float2 checkRot2 = checkRot;
		checkRot2.x = (checkRot.x * cosA) + (checkRot.y * (-sinA));
		checkRot2.y = (checkRot.x * sinA) + (checkRot.y * cosA);
		//if (abs(ScaledTexPoint.x) <= checkRot2.y && abs(ScaledTexPoint.y) <= checkRot2.x) {
		if (abs(ScaledTexPoint.x) <= 1 && abs(ScaledTexPoint.y) <= 1) {
			float4 fluidDecal = tex2D(_fluidTexture, float4(clamp(newcoords, decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			float4 fluidDecalADV = tex2D(_fluidTextureADVECT, float4(clamp(newcoords, decalEdgeTweak.x, decalEdgeTweak.y) + float2(decalEdgeTweak.z, decalEdgeTweak.w), 0, 0));
			//baseColor.rgb += fluidDecal;
			if (fluidDecal.r < 1  && newcoords.x > decalEdgeTweak.x && newcoords.x < decalEdgeTweak.y &&  newcoords.y > decalEdgeTweak.x && newcoords.y < decalEdgeTweak.y) {
				float fadeFactor = length(CamPos - worldPos) / (decalPosRot.w);
				//baseColor.rgb += (float3(1, 1, 1)*fluidDecal.r * 1.2 + saturate(float3(1, 1, 1)* fluidDecalADV.r*1.5) ) / pow(fadeFactor,4);
				float3 glitA = tex2D(_ShoreTex, i.bumpCoords * 0.2).rgb;
				baseColor.rgb += (float3(1, 1, 1)*fluidDecal.r*fluidDecal.r * 2.2 + saturate(float3(1, 1, 1)* fluidDecalADV.r*2.5)) * (glitA+0.4);
				_Foam.z = 0;
			}
		}


		//v1.5
		//o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
		//o.Albedo = lerp(0, _FoamColor, jacobian);
		baseColor.rgb = baseColor.rgb + baseColor.rgb * (lerp(0, _FoamColor, jacobian) + lerp(distanceGloss, 0, jacobian) );		
		baseColor.rgb = baseColor.rgb + colorFFT;//
		//o.Smoothness = lerp(distanceGloss, 0, jacobian);
		//o.Metallic = 0;
		//o.Normal = WorldToTangentNormalVector(IN, worldNormalFFT);
		//i.normal.xyz = i.normal.xyz + worldNormalFFT;


		//STATIC FOAM
		baseColor.rgb += 2 * saturate((normaL1.y + 0.0)*_Foam.z * 1 * foam2.rgb * edgeBlendFactorsA.y
			- 0.001*abs(cos(0.00005*(worldPos.x + worldPos.z)*cos(_Time.y*0.000001) + cos(_Time.y*0.000001)*0.2))
			//- 0.1*abs(cos(0.00005*(worldPos.x + worldPos.z)*cos(_Time.y) + cos(_Time.y)*0.2)) * (cos(_Time.y) + 10*pow(i.normal.w,1))
			//- 0.1*abs(sin(0.00003*(worldPos.x + worldPos.z)*cos(_Time.y) + cos(_Time.y)*0.4))
		);//v0.6a



		  //v1.9 - float foamFFT = tex2D(_FoamTexture, IN_worldUV * 0.04 + _Time.r*3).r;
		//if (ripplesPos.w > 1) {
		//	//baseColor.rgb += saturate(pow(ripplesFOAM.r, 4) * 1 * 1 * 1 * foam) * 223 * (edgeBlendFactorsA.y + 0.016);
		//	//if (heightA.r > 44.99) {
		//		//baseColor.rgb += saturate(pow(ripplesFOAM.r, 9) * 1 * 1) * 1 * (1);
		//	//}
		//	baseColor.rgb += saturate((pow(ripplesFOAM.r, 0.2) - 0.54)  * 1 * (1.01) * (1-foamFFT))*12.6+ ripplesFOAM.r*foamFFT * foamFFT * foamFFT * foamFFT;
		//}

		//baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y));
		//baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y)) * (clamp(i.viewInterpolator.x/25,0,1));
		
		//baseColor.rgb +=  0.2*foam.rgb * _Foam.x * pow((1-depth), 11);

		//baseColor.a = (edgeBlendFactors.x + 1); //v0.7
		//baseColor.a = (edgeBlendFactors.x - 0.06) / depth; //v0.6
		//baseColor.a = edgeBlendFactors.x + 1 + abs(worldPos.y - 0);
		baseColor.a = (edgeBlendFactors.x + 1); //v0.8

		//v3.4.3 - fogging
		//#ifdef WATER_EDGEBLEND_ON
//		if(depth > fogThres){
//			baseColor = lerp(baseColor,fogColor,clamp(0.001*fogBias*pow(depth-fogThres,fogDepth),0,1));
//		}
		//if(i.screenPos.z > fogThres){
			//baseColor = lerp(baseColor,fogColor,clamp(0.001*fogBias*pow(i.screenPos.z-fogThres,fogDepth),0,1));
		//}
		// 
		
		//v2.0aa
		//v4.1
		//if(i.screenPos.z > fogThres){
//		if ((1 - i.screenPos.z) * 50 > (fogThres*0.05 + 105)*0.3) {
			//baseColor = lerp(baseColor, fogColor, clamp(0.001*fogBias*pow((1 - i.screenPos.z) * 50 - (fogThres*0.05 + 105)*0.3, fogDepth * 9 * 0.8), 0, 1)); //v1.1
			//baseColor = baseColor+lerp(baseColor, fogColor, clamp(0.001*fogBias*pow((1 - i.screenPos.z) * 50 - (fogThres*0.05 + 105)*0.3, fogDepth * 9 * 0.8), 0, 1)); //v1.1
//			baseColor = lerp(baseColor, clamp(fogColor*2 +  0.5*fogColor * baseColor,0,1), clamp(0.001*fogBias*pow((1 - i.screenPos.z) * 50 - (fogThres*0.05 + 105)*0.3, fogDepth * 9 * 0.8) + 0.2*cos(worldPos.y)*1, 0, 1)); //v1.1
//		}
		//#endif

		//MULTI LIGHTS
//		fixed atten = LIGHT_ATTENUATION(i);
//		i.lightDir = normalize(i.lightDir);
//		fixed diff = saturate(dot(i.normal,i.lightDir));
//		
//		float4 lighter = float4(_LightColor0.rgb*atten*2*diff*diff*1*diff*(1-refl2Refr)*1,1);
		
		UNITY_APPLY_FOG(i.fogCoord, baseColor);

		half4 distortOffsetA = float4(0,0,0,0);// half4(worldNormal.xz * REALTIME_DISTORTION * 10.0, 0, 0);
		half4 screenWithOffsetA = i.screenPos + distortOffsetA;
		half4 grabWithOffsetA = i.grabPassPos + distortOffsetA;
		float4 uvsRFefl = UNITY_PROJ_COORD(grabWithOffset);
		
		//float4 grabPassTex = SAMPLE_DEPTH_TEXTURE_PROJ(_ColorPyramidTexture,  UNITY_PROJ_COORD(float4(grabWithOffset.x, grabWithOffset.y, grabWithOffset.z, grabWithOffset.w)));
		//float4 grabPassTex = LOAD_TEXTURE2D_X_LOD(_ColorPyramidTexture, UNITY_PROJ_COORD(float4(grabWithOffset.x, grabWithOffset.y, grabWithOffset.z, grabWithOffset.w)).xy, 0);
		
		//return float4(depth, depth, depth,1);
		//return float4(baseColor.rgb*1 + baseColor.rgb *grabPassTex.rgb*depth*1, baseColor.a);//baseColor * 1;
		
		//return float4(baseColor.rgb * (1/ depth) + baseColor.rgb *rtRefractions1.rgb*(1-depth) * 1, baseColor.a);

		//return float4(i.screenPos.w*0.01, i.screenPos.w*0.01, i.screenPos.w*0.01, saturate(pow((1 - depth),0.3) ));
		//return float4(i.screenPos.x*0.05, i.screenPos.y*0.05, i.screenPos.w*0.05, 1);

		//HDRP ---------------------------------------- LOCAL LIGHTS
		//_localLightAPos - _localLightAProps		
		//UNITY_LIGHT_ATTENUATION(atten, i, float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w));

		float atten = _localLightAPos.w;//LIGHT POWER
		float3 LightDirN = normalize(i.lightDirPosx.xyz);
		float3 LightDir = i.lightDirPosx.xyz;
	
		float diff = saturate(dot(worldNormal, LightDirN));

		float3 DotDir = dot(i.normal, LightDir);
		float diff2 = max(0.0, dot(worldNormal, LightDir));
		float diff3 = max(0.0, dot(i.normal, LightDir));

		//float3 vertexPos = float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w);
		//float3 vertexToLight = pow(float3(_localLightAPos.xyz) - vertexPos, 2);  // _WorldSpaceLightPos0.xyz - vertexPos;
		float3 vertexToLight = float3(_localLightAPos.xyz) - vertexPos;

		//vertexToLight = pow(vertexToLight, _localLightAProps.w);
		
		//int power = _localLightRange;
		//float3 vertexToLight = pow(float3(_localLightAPos.xyz) - vertexPos.xyz, _localLightRange);

		//float4 lighter = float4(_localLightAProps.rgb*atten*((diff*diff2 / 5) + diff3) / pow(length(vertexToLight.xyz), _localLightAProps.w), 1);
		float4 lighter = float4(_localLightAProps.rgb*atten*(diff+0.06) / pow(length(vertexToLight.xyz), 1/_localLightAProps.w*15), 1);
		
		half3 viewVector1 = normalize(_WorldSpaceCameraPos - vertexPos); //normalize(-i.viewInterpolator.xyz);				
		//half3 reflectVector = normalize(reflect(viewVector1, worldNormal));			
		//half3 h = normalize((LightDir.xyz) + viewVector1.xyz);
		//float nh = max(0, dot(reflectVector, -h));		
		float3 LightDir2 = normalize(vertexToLight);
		//reflectVector = reflect(-LightDir2, worldNormal);
		//nh = max(0.0, dot(reflectVector, viewVector1));
		//float specA = max(0.0, pow(nh, _Shininess*0.01));
		//baseColor.rgb = baseColor.rgb + (lighter.xyz*1 * 1*abs(LightDirN.x)) +(float3(_localLightAProps.rgb) *specA * _SpecularColor*worldNormal.x);
		//baseColor.rgb = baseColor.rgb + (lighter.xyz * 1 * 1 * abs(LightDirN.x)) + (float3(_localLightAProps.rgb) *specA * 1);

		half3 reflectVectorB = normalize(reflect(viewVector1, worldNormal));		
		half3 hB = normalize((LightDir.xyz) + viewVector1.xyz);
		float nhB = max(0, dot(reflectVectorB, -hB));
		float3 LightDir2B = normalize(vertexToLight);
		reflectVectorB = reflect(-LightDir2, worldNormal);
		nhB = max(0.0, dot(reflectVectorB, viewVector1));
		float specB = max(0.0, pow(nhB, _Shininess * 0.01));
		baseColor.rgb = baseColor.rgb + baseColor.rgb * specB * lighter.rgb;

		//baseColor.rgb = baseColor.rgb + (lighter.xyz);
		//return float4(lighter.xyz + (_LightColor0.rgb *spec * _SpecularColor), 1);
		//END HDRP LOCAL LIGHTS

		//v0.6
		float alpha = saturate(baseColor.a - 0.8*i.normal.w);
		//float alpha = 1;

		//v0.6
		//return float4(baseColor.rgb, baseColor.a);
		float3 refractTexture = baseColor.rgb *rtRefractions.rgb*(1 - isUnderwater) + baseColor.rgb *rtRefractions* isUnderwater;
		//refractTexture = saturate(refractTexture / abs(worldPos.y - 0) + refractTexture);
		//refractTexture = saturate(refractTexture / abs(worldPos.y*worldPos.y - 0)) + saturate(refractTexture/2);
		//refractTexture = saturate(refractTexture / abs((worldPos.y-4)*(worldPos.y-4)*0.04 - 0)) + 0;
		//refractTexture = saturate(refractTexture / ((worldPos.y - 0)*0.06*1 )) ; //v0.7
		refractTexture = saturate(shoreGlow.x*2*refractTexture / abs((worldPos.y - shoreGlow.y*11)*(worldPos.y - shoreGlow.y*11)*0.01*shoreGlow.z))*shoreGlow.w; //v0.7
		float3 finalCol = baseColor.rgb * 1 + refractTexture;


		//v1.1 - Bruneton SKY --------------------------------------
		float2 uv = worldPos.xz * _controlSkyRadiance.w;// IN.worldPos.xz;

		float2 slope = float2(0, 0);
		/*slope += tex2D(_Map1, uv / _GridSizes.x).xy;
		slope += tex2D(_Map1, uv / _GridSizes.y).zw;
		slope += tex2D(_Map2, uv / _GridSizes.z).xy;
		slope += tex2D(_Map2, uv / _GridSizes.w).zw;*/
	//	slope += UNITY_SAMPLE_TEX2D(_Map1, uv / _GridSizes.x).xy;//v1.5
	//	slope += UNITY_SAMPLE_TEX2D(_Map1, uv / _GridSizes.y).zw;
	//	slope += UNITY_SAMPLE_TEX2D_SAMPLER(_Map2, _Map1, uv / _GridSizes.z).xy;
	//	slope += UNITY_SAMPLE_TEX2D_SAMPLER(_Map2, _Map1, uv / _GridSizes.w).zw;
		slope += slopeFFT; //v1.5



		//v1.9 -- outNormal
		//N = N - N* outNormal/1;
		//N = outNormal*100111;
		//N = N-100000*outNormalA;
		float heightA1 = clamp(2 * pow(outNormalA.r, 1) * RippleSpecularAdjust.y, 0, 20 * RippleSpecularAdjust.w);
		float3 worldDirivativeX = ddx(worldPos * 10);
		float3 worldDirivativeY = ddy(worldPos * 10);
		float3 crossX = cross(worldNormal, worldDirivativeX);
		float3 crossY = cross(worldNormal, worldDirivativeY);
		float3 d = abs(dot(crossY, worldDirivativeX));
		float3 inToNormal = ((((heightA1 + ddx(heightA1)) - heightA1) * crossY) + (((heightA1 + ddy(heightA1)) - heightA1) * crossX)) * sign(d);
		inToNormal.y *= -1.0;
		//N += float3(N.x, inToNormal.y, N.y);//  normalize((d* N) - inToNormal);
		//slope = slope-0.1*normalize(sign(d) * slope - inToNormal);// ((d* slope) - inToNormal);// 0.4 * clamp(inToNormal, -0.1, 5);
		slope = slope - RippleSpecularAdjust.x * 3*clamp(inToNormal,-5 * RippleSpecularAdjust.z,5* RippleSpecularAdjust.z);

		float3 V = normalize(_WorldSpaceCameraPos - worldPos); //IN.worldPos);

		//float3 N = normalize(float3(-slope.x, 1.0, -slope.y));
		//float3 N = i.normalInterpolator;// worldNormal; i.normal;
		float3 N0 = normalize(float3(-slope.x, 1.0, -slope.y));

		//v1.9 -- outNormal
		//N0 = N0 * 1*outNormal/1;

		//i.normalInterpolator.xyz += -0.00001 * N0 * _controlSkyRadiance.z;
		float3 N = i.normalInterpolator -0.00001 * N0 * _controlSkyRadiance.z;
		//float3 N = i.normalInterpolator;

	


		if (dot(V, N) < 0.0) {
			N = reflect(N, V); // reflects backfacing normals
		}

		float Jxx = ddx(uv.x);
		float Jxy = ddy(uv.x);
		float Jyx = ddx(uv.y);
		float Jyy = ddy(uv.y);
		float A = Jxx * Jxx + Jyx * Jyx;
		float B = Jxx * Jxy + Jyx * Jyy;
		float C = Jxy * Jxy + Jyy * Jyy;
		const float SCALE = 10.0 * _controlSkyRadianceA.x;
		float ua = pow(A / SCALE, 0.25);
		float ub = 0.5 + 0.5 * B / sqrt(A * C);
		float uc = pow(C / SCALE, 0.25);
		float2 sigmaSq = float2(0,0);// tex3D(_Variance, float3(ua, ub, uc)).xy; //FFT

		sigmaSq = max(sigmaSq, 2e-5)* _controlSkyRadianceA.y;

		float3 Ty = normalize(float3(0.0, N.z, -N.y));
		float3 Tx = cross(Ty, N);

		float fresnel = 0.02 + 0.98 * MeanFresnel(V, N, sigmaSq);

		float3 Lsun = SunRadiance(_WorldSpaceCameraPos)* _controlSkyRadianceA.z;
		float3 Esky = SkyIrradiance(_WorldSpaceCameraPos);

		float3 col = float3(0, 0, 0);

		col += ReflectedSunRadiance(SUN_DIR, V, N, Tx, Ty, sigmaSq) * Lsun * 0.2 * _controlSkyColor;

		col += MeanSkyRadiance(V, N, Tx, Ty, sigmaSq) * fresnel;

		float3 Lsea = _SeaColor * Esky / M_PI;
		col += Lsea * (1.0 - fresnel);

		finalCol = finalCol * _controlSkyRadiance.x + _controlSkyRadiance.y * saturate(hdr(col));

		//return float4(hdr(col), 1);
		//END BRUNETON SKY --------------------------------------

		float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
		if (finalCol.r == 0) {
			finalCol.r = 0.01;
		}
		float distToPlane = dot(camPlaneNormal, worldPos - _WorldSpaceCameraPos);
		//float distToNormal = dot(float3(camPlaneNormal.x,0, camPlaneNormal.z), float3(i.normalInterpolator.x,0, i.normalInterpolator.z));
		//float distToNormal = dot(float3(camPlaneNormal.x, 0, camPlaneNormal.z), i.normalInterpolator);
		float distToNormal = dot(camPlaneNormal, i.normalInterpolator);
		//if (distToPlane < cameraNearClip + 14*abs(distToNormal)*abs(distToNormal)) {//if (length(worldPos- _WorldSpaceCameraPos) < 15) {
		//	return cameraNearColor;// float4(1, 1, 1, 1);
		//}

		//v1.6
		//if (distToPlane < cameraNearClip + cameraNearControl.z * abs(distToNormal)){//if (distToPlane < cameraNearClip + cameraNearControl.x*54*abs(pow(distToNormal, cameraNearControl.y))) {//if (length(worldPos- _WorldSpaceCameraPos) < 15) {
		//	return cameraNearColor;// float4(1, 1, 1, 1);
		//}


		//v2.0aa
		if ((i.screenPos.z) * 50 < (fogThres * 0.05 + 105) * 0.3 && _WorldSpaceCameraPos.y < waterHeight + waterHeightFogOffset) {   //if ((1 - i.screenPos.z) * 50 > (fogThres * 0.05 + 105) * 0.3) {
			//baseColor = lerp(baseColor, fogColor, clamp(0.001*fogBias*pow((1 - i.screenPos.z) * 50 - (fogThres*0.05 + 105)*0.3, fogDepth * 9 * 0.8), 0, 1)); //v1.1
			//baseColor = baseColor+lerp(baseColor, fogColor, clamp(0.001*fogBias*pow((1 - i.screenPos.z) * 50 - (fogThres*0.05 + 105)*0.3, fogDepth * 9 * 0.8), 0, 1)); //v1.1
			//finalCol =  lerp(finalCol, clamp(fogColor * 2 + 0.5 * fogColor * finalCol, 0, 1), clamp(0.001 * fogBias * pow((1 - i.screenPos.z) * 50 - (fogThres * 0.05 + 105) * 0.3, fogDepth * 9 * 0.8) + 0.2 * cos(worldPos.y) * 1, 0, 1)); //v1.1
			finalCol = lerp(
				finalCol,
				clamp((fogColor * 2 + 0.5 * fogColor * finalCol), 0, 1),
				clamp(0.001 * fogBias * pow((fogThres * 0.05 + 105) * 0.3 - (i.screenPos.z) * 50, fogDepth * 9 * 0.8) + 0.2 * cos(worldPos.y) * (1 - i.screenPos.z), 0, 1) //
			); //v1.1
		}


		if (scale.x >= 1 && _WorldSpaceCameraPos.y < waterHeight-4* NearClipAdjust.z){// && facing >= 0) {
			return float4(finalCol, saturate(alpha - (1 - 0.000015 * NearClipAdjust.w * pow(dot(worldPos - _WorldSpaceCameraPos, camPlaneNormal), 2.5) * 1)));
		}



		//v1.9 - float foamFFT = tex2D(_FoamTexture, IN_worldUV * 0.04 + _Time.r*3).r; -- outNormal
		if (ripplesPos.w > 1) {
			//float foamFFTA = tex2D(_FoamTexture, IN_worldUV *0.0004 * 0.12*_Time.y*float2(N.x , N.z )).r;
			float foamFFTA = tex2D(_FoamTexture, IN_worldUV * 0.08 * 0.22 * RippleFoamAdjust.x * float2(N.x, N.z)).r*0.88 * RippleFoamAdjust.y;
			//baseColor.rgb += saturate(pow(ripplesFOAM.r, 4) * 1 * 1 * 1 * foam) * 223 * (edgeBlendFactorsA.y + 0.016);
			//if (heightA.r > 44.99) {
				//baseColor.rgb += saturate(pow(ripplesFOAM.r, 9) * 1 * 1) * 1 * (1);
			//}
			//baseColor.rgb += saturate((pow(ripplesFOAM.r, 0.2) - 0.54) * 1 * (1.01) * (1 - foamFFT)) * 12.6 + ripplesFOAM.r * foamFFT * foamFFT * foamFFT * foamFFT;
			finalCol.rgb += 0.1 * saturate((pow(ripplesFOAM.r, 0.2) - 0.24 * RippleFoamAdjust.z) * 1 * (1.01) * (1 - foamFFTA)) * 2 + RippleFoamAdjust.w*ripplesFOAM.r * pow(foamFFTA,3);
			//finalCol.rgb += saturate(dot(SUN_DIR, N0 * ripplesFOAM.r))* ripplesFOAM.r*2;
		}

		//v2.1
		finalCol.rgb += saturate(1 * FOAMAAAA.rgb * 10);

		//return float4(finalCol, saturate(alpha - (1 - 0.000015 * pow(dot(worldPos - _WorldSpaceCameraPos, camPlaneNormal), 2.2) * 1)));
		//return float4(finalCol, saturate(alpha - (1 - 0.000025* pow(dot(worldPos - _WorldSpaceCameraPos, camPlaneNormal), 2)*1)));

		//return float4(baseColor.rgb * 1 + baseColor.rgb *rtRefractions.rgb*(1 - isUnderwater) + baseColor.rgb *rtRefractions* isUnderwater, saturate(baseColor.a - 0.8*i.normal.w));//v0.5
		return float4(finalCol * 1, alpha);//v0.5
		//return float4(baseColor.rgb * (1 / (depth)) + baseColor.rgb *rtRefractions1.rgb*(1 - depth) * (1 / (depth)), saturate(baseColor.a / ((1-depth) * i.screenPos.z)));
	}





	///////////////// ADD







	half4 fragADD( v2f i ) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, PER_PIXEL_DISPLACE);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half4 distortOffset = half4(worldNormal.xz * REALTIME_DISTORTION * 10.0, 0, 0);
		half4 screenWithOffset = i.screenPos + distortOffset;
		half4 grabWithOffset = i.grabPassPos + distortOffset;
		
		half4 rtRefractionsNoDistort = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(i.grabPassPos));
		//half refrFix = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset));
		half refrFix = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset).xy, 0).r; //URP v0.1

		half4 rtRefractions = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(grabWithOffset));
		
		#ifdef WATER_REFLECTIVE
		float4 uvsRFefl = UNITY_PROJ_COORD(screenWithOffset);
		half4 rtReflections = tex2Dproj(_ReflectionTex, float4(uvsRFefl.x,  uvsRFefl.y, 1, 1));
			//half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(screenWithOffset));
		#endif

		#ifdef WATER_EDGEBLEND_ON
		if (LinearEyeDepth(refrFix) < i.screenPos.z)
			rtRefractions = rtRefractionsNoDistort;
		#endif
		
		
		
		
			
		
		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
		
		#ifdef WATER_EDGEBLEND_ON
			//half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			half depth = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos).xy, 0).r; //v0.1 URP
			depth = LinearEyeDepth(depth);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.w));
			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		
		// shading for fresnel term
		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		// base, depth & reflection colors
		//half4 baseColor = ExtinctColor (_BaseColor, i.viewInterpolator.w * _InvFadeParemeter.w + _BaseColor*11);////////// Sky Master 3.0
		
		half4 baseColor = _BaseColor - ( i.viewInterpolator.w * _InvFadeParemeter.w + _BaseColor*_MultiplyEffect) * half4(0.15, 0.03, 0.01, 0);
		
		#ifdef WATER_REFLECTIVE
			half4 reflectionColor = lerp (rtReflections,_ReflectionColor,_ReflectionColor.a);
		#else
			half4 reflectionColor = _ReflectionColor;
		#endif
		
		baseColor = lerp (lerp (rtRefractions, baseColor, baseColor.a), reflectionColor, refl2Refr);
		//baseColor = baseColor + spec * _SpecularColor;
		
		// handle foam
		half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
		baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y)) ;
		
		baseColor.a = edgeBlendFactors.x;
		
		
		//MULTI LIGHTS
		
		//fixed atten = LIGHT_ATTENUATION(i); //v4.1
		//UNITY_LIGHT_ATTENUATION(attenuation, i, i.posWorld.xyz); //v4.1 //o.lightPosYZvertXY.zw = worldSpaceVertex.xy; o.normalInterpolator.w = worldSpaceVertex.z;
		UNITY_LIGHT_ATTENUATION(atten, i, float3(i.lightPosYZvertXY.zw, i.normalInterpolator.w));

		//i.lightDir = normalize(i.lightDir);
		
		float3 LightDirN = normalize(i.lightDirPosx.xyz);
		float3 LightDir = i.lightDirPosx.xyz;
		
		//fixed diff = saturate(dot(i.normal,i.lightDir));	
		//fixed diff = saturate(dot(worldNormal,i.lightDir));
		fixed diff = saturate(dot(worldNormal,LightDirN));
		
		float3 DotDir = dot(i.normal,LightDir);
		fixed diff2 = max(0.0,dot(worldNormal,LightDir));
		fixed diff3 = max(0.0,dot(i.normal,LightDir));
			
		//fixed diff = saturate(dot(worldNormal/2 + i.normal/2,i.lightDir));			
					
	//	float4 lighter = float4(_LightColor0.rgb*1*4*diff*diff*diff*diff*(1-refl2Refr),1);
		
		//lightPos
		//float4 lighter = float4(_LightColor0.rgb*diff*1/length(i.vertPos -  mul(_LightMatix0,i.vertPos)  ),1);
		//float4 lighter = float4(_LightColor0.rgb*diff*1/length(i.vertPos -  mul(_LightMatix0,i.vertPos)  ),1);
		//float4 lighter = float4(_LightColor0.rgb*diff,1);		
		
		float3 vertexPos = float3(i.lightPosYZvertXY.zw,i.normalInterpolator.w);		
		float3 vertexToLight = SUN_DIR - vertexPos;// _WorldSpaceLightPos0.xyz - vertexPos; //v0.2
		//float4 lighter = float4(_LightColor0.rgb*diff*1/length(vertexPos -  mul(_LightMatix0,vertexPos)  ),1);
		//float4 lighter = float4(_LightColor0.rgb*atten*diff3/length(_WorldSpaceLightPos0.xyz - vertexPos ),1);	
		float4 lighter = float4(_LightColor0.rgb*atten*((diff*diff2/5)+diff3)/length( vertexToLight ),1);
		
		
		
	
		half3 viewVector1 = normalize(_WorldSpaceCameraPos - vertexPos); //normalize(-i.viewInterpolator.xyz);				
		half3 reflectVector = normalize(reflect(viewVector1, worldNormal));
		//half3 h = normalize ((_WorldLightDir.xyz) + viewVector.xyz);		
		half3 h  = normalize ((LightDir.xyz) + viewVector1.xyz);		
		float nh = max (0, dot (reflectVector, -h));
		
		
		float3 LightDir2 = normalize(vertexToLight);
//		if(DotDir < 0){
//		
//		}
		reflectVector = reflect(-LightDir2, worldNormal);
		nh = max (0.0, dot (reflectVector, viewVector1));
		
		float spec = max(0.0,pow (nh, _Shininess));
		
		
//		float3 SpecularPoint;
//		if(DotDir < 0.0){
//			SpecularPoint = float3(0,0,0);
//		}else{
		//	float3 SpecularPoint = atten * _LightColor0.rgb * pow(max(0.0,dot(reflect(-LightDir,i.normal),normalize(i.viewInterpolator.xyz))),  _Shininess*1 )/length(_WorldSpaceLightPos0.xyz - vertexPos );
		//}
		
		//UNITY_APPLY_FOG(i.fogCoord, baseColor);
		//return baseColor * lighter;
		return float4(lighter.xyz + (_LightColor0.rgb *spec * _SpecularColor),1);
	}
	//
	// MQ VERSION
	//
	
	v2f_noGrab vert300(appdata_full v)
	{
		v2f_noGrab o;
		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld,(v.vertex)).xyz;
		half3 vtxForAni = (worldSpaceVertex).xzz;

		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
			_GAmplitude,												// amplitude
			_GFrequency,												// frequency
			_GSteepness,												// steepness
			_GSpeed,													// speed
			_GDirectionAB,												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);
		
		v.vertex.xyz += offsets;
		
		// one can also use worldSpaceVertex.xz here (speed!), albeit it'll end up a little skewed
		half2 tileableUv = mul(unity_ObjectToWorld,v.vertex).xz;
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;

		o.pos = UnityObjectToClipPos(v.vertex);

		o.screenPos = ComputeScreenPos(o.pos);
		
		o.normalInterpolator.xyz = nrml;
		o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		
		UNITY_TRANSFER_FOG(o,o.pos);
		return o;
	}

	half4 frag300( v2f_noGrab i ) : SV_Target
	{
		//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, normalize(VERTEX_WORLD_NORMAL), PER_PIXEL_DISPLACE);

		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half4 distortOffset = half4(worldNormal.xz * REALTIME_DISTORTION * 10.0, 0, 0);
		half4 screenWithOffset = i.screenPos + distortOffset;
		
		#ifdef WATER_REFLECTIVE
			//float4 uvsRFefl = UNITY_PROJ_COORD(screenWithOffset);
			//half4 rtReflections = tex2Dproj(_ReflectionTex, float4(uvsRFefl.x,  uvsRFefl.y, 1, 1));
			half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(screenWithOffset));
		#endif
		
		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize (_WorldLightDir.xyz + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0,pow (nh, _Shininess));
		
		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
		
		#ifdef WATER_EDGEBLEND_ON
			//half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			half depth = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos).xy, 0).r;
			depth = LinearEyeDepth(depth);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.z));
			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		
		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		half4 baseColor = _BaseColor;
		#ifdef WATER_REFLECTIVE
			baseColor = lerp (baseColor, lerp (rtReflections,_ReflectionColor,_ReflectionColor.a), saturate(refl2Refr * 2.0));
		#else
			baseColor = lerp (baseColor, _ReflectionColor, saturate(refl2Refr * 2.0));
		#endif
		
		baseColor = baseColor + spec * _SpecularColor;
		
		baseColor.a = edgeBlendFactors.x * saturate(0.5 + refl2Refr * 1.0);
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}
	
	//
	// LQ VERSION
	//
	
	v2f_simple vert200(appdata_full v)
	{
		v2f_simple o;
		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
		half2 tileableUv = worldSpaceVertex.xz;

		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex-_WorldSpaceCameraPos;
		
		o.pos = UnityObjectToClipPos(v.vertex);
		
		o.viewInterpolator.w = 1;//GetDistanceFadeout(ComputeScreenPos(o.pos).w, DISTANCE_SCALE);
		
		UNITY_TRANSFER_FOG(o,o.pos);
		return o;

	}

	half4 frag200( v2f_simple i ) : SV_Target
	{
		///UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, half3(0,1,0), PER_PIXEL_DISPLACE);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize ((_WorldLightDir.xyz) + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0,pow (nh, _Shininess));

		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);

		half4 baseColor = _BaseColor;
		baseColor = lerp(baseColor, _ReflectionColor, saturate(refl2Refr * 2.0));
		baseColor.a = saturate(2.0 * refl2Refr + 0.5);

		baseColor.rgb += spec * _SpecularColor.rgb;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}

		half4 fragDEPTH(v2f_simple i) : SV_Target
	{

		//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, half3(0,1,0), PER_PIXEL_DISPLACE);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize((_WorldLightDir.xyz) + viewVector.xyz);
		float nh = max(0, dot(worldNormal, -h));
		float spec = max(0.0,pow(nh, _Shininess));

		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);

		half4 baseColor = _BaseColor;
		baseColor = lerp(baseColor, _ReflectionColor, saturate(refl2Refr * 2.0));
		baseColor.a = saturate(2.0 * refl2Refr + 0.5);

		baseColor.rgb += spec * _SpecularColor.rgb;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return float4(1,1,1,1);// baseColor;
	}
	
//ENDCG
ENDHLSL

Subshader
{
	Tags {"RenderType"="Transparent-1" "Queue"="Transparent-1" "IgnoreProjector"="True"}
	
	Lod 600
	ColorMask RGB
	
	//GrabPass { "_RefractionTex" }
	
	Pass {
	
	//Tags {"LightMode"="ForwardBase"}
	
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			//ZWrite Off
			Cull Off
		
		HLSLPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert600
			#pragma fragment frag600
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
		ENDHLSL
	}
	
		Pass {
		
		Tags {"LightMode"="ForwardAdd"}
		
			Blend One One
			ZTest LEqual
			//ZWrite Off
			Cull Off
		
		HLSLPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert600
			#pragma fragment frag600
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
		ENDHLSL
	}
	
}


Subshader
{
	Tags {"RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
	
	Lod 500
	ColorMask RGB
	
	//GrabPass { "_RefractionTex" }

	//HDRP - Metal - https://docs.unity3d.com/Manual/SL-CullAndDepth.html
	// extra pass that renders to depth buffer only
	//Pass{

		//ZWrite On
		//ZTest LEqual
		//ColorMask 0
		//Blend SrcAlpha OneMinusSrcAlpha
		
		//HLSLPROGRAM
		//#pragma target 3.0
		//#pragma vertex vert
		//#pragma fragment fragDEPTH
		//ENDHLSL
	//}
	
	Pass {
	
	//Tags {"LightMode"="ForwardBase"}
	
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			//ZWrite Off
			Cull Off
		
	HLSLPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			//#pragma multi_compile_fwdadd
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
	ENDHLSL
	}
	
	Pass {
	
	Tags {"LightMode"="ForwardAdd"}
	
			Blend One One
			ZTest LEqual
			//ZWrite Off
			Cull Off
		
	HLSLPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert
			#pragma fragment fragADD
			#pragma multi_compile_fog
			#pragma multi_compile_fwdadd
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
	ENDHLSL
	}
}

Subshader
{
	Tags {"RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
	
	Lod 300
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			//ZWrite Off
			Cull Off
		
	HLSLPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert300
			#pragma fragment frag300
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
	ENDHLSL
	}
}

Subshader
{
	Tags {"RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
	
	Lod 200
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			//ZWrite Off
			Cull Off
		
	HLSLPROGRAM
		
			#pragma vertex vert200
			#pragma fragment frag200
			#pragma multi_compile_fog
		
	ENDHLSL
	}
}

Fallback "Transparent/Diffuse"
}
