Shader "Unlit/SunShaftsWaterSRP_FORWARD_URP"
{

	Properties
	{
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
	//[HideInInspector]_ColorBuffer("Base (RGB)", 2D) = "white" {}
	 
		//[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
		//_Delta("Line Thickness", Range(0.0005, 0.0025)) = 0.001
		//[Toggle(RAW_OUTLINE)]_Raw("Outline Only", Float) = 0
		//[Toggle(POSTERIZE)]_Poseterize("Posterize", Float) = 0
		//_PosterizationCount("Count", int) = 8

		_SunThreshold("sun thres", Color) = (0.87, 0.74, 0.65,1)
		_SunColor("sun color", Color) = (1.87, 1.74, 1.65,1)
		_BlurRadius4("blur", Color) = (0.00325, 0.00325, 0,0)
		_SunPosition("sun pos", Color) = (111, 11,339, 11)
	}

		HLSLINCLUDE

		//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" //unity 2018.3
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" 
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"
		//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "PostProcessing/Shaders/StdLib.hlsl" //unity 2018.1-2
		//#include "UnityCG.cginc"




		//v4.7
//v0.2 - shadows
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/ShaderLibrary/Shadows.hlsl"
//https://forum.unity.com/threads/solved-clip-space-to-world-space-in-a-vertex-shader.531492/
#include "ClassicNoise3D.hlsl"
//float4x4 unity_CameraInvProjection; //ALREADY DEFINED in LIBS ABOVE //v4.8
//	float4x4 unity_WorldToCamera; //v4.8
//	float4x4 unity_CameraProjection; //v4.8
//	float4 unity_FogColor; //v4.8
	//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/API/D3D11.hlsl"
	//#include "UnityCG.cginc"
	//#include "Lighting.cginc"
	//#include "AutoLight.cginc" 
	//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.hlsl"
	//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.cs.hlsl"
	//HDRP 2019.3
	//#define UNITY_DECLARE_TEX2DARRAY(tex) Texture2DArray tex; SamplerState sampler##tex
	//https://forum.unity.com/threads/has-depthpyramidtexture-been-removed-from-hdrp-in-unity-2019-3.851236/#post-5615971
	//UNITY_DECLARE_TEX2DARRAY(_CameraDepthTexture);
	//	TEXTURE2D_X(_DepthTexture);
	//	SAMPLER(sampler_DepthTexture);
	//float4 _DepthPyramidScale;
#define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
	//MASK
	//	sampler2D _SourceTex, _WaterInterfaceTex;
	TEXTURE2D_X(_SourceTex);
	TEXTURE2D_X(_WaterInterfaceTex);
	SAMPLER(sampler_SourceTex);
	SAMPLER(sampler_WaterInterfaceTex);


		TEXTURE2D(_MainTex);
	TEXTURE2D(_ColorBuffer);
	TEXTURE2D(_Skybox);

	SAMPLER(sampler_MainTex);
	SAMPLER(sampler_ColorBuffer);
	SAMPLER(sampler_Skybox);
	float _Blend;





	//sampler2D _MainTex;
	//sampler2D _ColorBuffer;
	//sampler2D _Skybox;
	//sampler2D_float _CameraDepthTexture;
	TEXTURE2D(_CameraDepthTexture);
	SAMPLER(sampler_CameraDepthTexture);
	half4 _CameraDepthTexture_ST;

	half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);

	half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
	uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
	uniform half4 _SunPosition = half4(1,1,1,1);
	uniform half4 _MainTex_TexelSize;

#define SAMPLES_FLOAT 16.0f
#define SAMPLES_INT 16

	//v4.7
	TEXTURE2D(_NoiseTex);
	SAMPLER(sampler_NoiseTex);
	//URP v0.1
	//#pragma multi_compile FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_fog
#pragma multi_compile _ RADIAL_DIST
#pragma multi_compile _ USE_SKYBOX

	float _DistanceOffset;
	float _Density;
	float _LinearGrad;
	float _LinearOffs;
	float _Height;
	float _cameraRoll;
	//WORLD RECONSTRUCT	
	//float4x4 _InverseView;
	//float4x4 _camProjection;	////TO REMOVE
	// Fog/skybox information
	half4 _FogColor; //v4.7 - 4.8
	samplerCUBE _SkyCubemap;
	half4 _SkyCubemap_HDR;
	half4 _SkyTint;
	half _SkyExposure;
	float _SkyRotation;
	float4 _cameraDiff;
	float _cameraTiltSign;
	float _NoiseDensity;
	float _NoiseScale;
	float3 _NoiseSpeed;
	float _NoiseThickness;
	float _OcclusionDrop;
	float _OcclusionExp;
	int noise3D = 0;
	//END FOG URP /////////////////


	/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
	/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
	/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
	// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
	half ComputeFogFactorA(float coord) /// REDFINED, SO CHANGED NAME
	{
		float fog = 0.0;
#if FOG_LINEAR
		// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
		fog = coord * _LinearGrad + _LinearOffs;
#elif FOG_EXP
		// factor = exp(-density*z)
		fog = _Density * coord;
		fog = exp2(-fog);
#else // FOG_EXP2
		// factor = exp(-(density*z)^2)
		fog = _Density * coord;
		fog = exp2(-fog * fog);
#endif
		return saturate(fog);
	}
	// Distance-based fog
	float ComputeDistance(float3 ray, float depth)
	{
		float dist;
#if RADIAL_DIST
		dist = length(ray * depth);
#else
		dist = depth * _ProjectionParams.z;
#endif
		// Built-in fog starts at near plane, so match that by
		// subtracting the near value. Not a perfect approximation
		// if near plane is very large, but good enough.
		dist -= _ProjectionParams.y;
		return dist;
	}
	////LOCAL LIGHT
	float4 localLightColor;
	float4 localLightPos;
	/////////////////// SCATTER
	bool doDistance;
	bool doHeight;
	// Distance-based fog
	uniform float4 _CameraWS;
	uniform float4x4 _FrustumCornersWS;
	//SM v1.7
	uniform float luminance, Multiplier1, Multiplier2, Multiplier3, bias, lumFac, contrast, turbidity;
	//uniform float mieDirectionalG = 0.7,0.913; 
	float mieDirectionalG;
	float mieCoefficient;//0.054
	float reileigh;
	//SM v1.7
	uniform sampler2D _ColorRamp;
	uniform float _Close;
	uniform float _Far;
	uniform float3 v3LightDir;		// light source
	uniform float FogSky;
	float4 _TintColor; //float3(680E-8, 1550E-8, 3450E-8);
	float4 _TintColorL;
	float4 _TintColorK;
	uniform float ClearSkyFac;
	uniform float4 _HeightParams;
	// x = start distance
	uniform float4 _DistanceParams;

	int4 _SceneFogMode; // x = fog mode, y = use radial flag
	float4 _SceneFogParams;
#ifndef UNITY_APPLY_FOG
	//half4 unity_FogColor; //ALREADY DEFINED !!!!!!!!!!
	half4 unity_FogDensity;
#endif	

	uniform float e = 2.71828182845904523536028747135266249775724709369995957;
	uniform float pi = 3.141592653589793238462643383279502884197169;
	uniform float n = 1.0003;
	uniform float N = 2.545E25;
	uniform float pn = 0.035;
	uniform float3 lambda = float3(680E-9, 550E-9, 450E-9);
	uniform float3 K = float3(0.686, 0.678, 0.666);//const vec3 K = vec3(0.686, 0.678, 0.666);
	uniform float v = 4.0;
	uniform float rayleighZenithLength = 8.4E3;
	uniform float mieZenithLength = 1.25E3;
	uniform float EE = 1000.0;
	uniform float sunAngularDiameterCos = 0.999956676946448443553574619906976478926848692873900859324;
	// 66 arc seconds -> degrees, and the cosine of that
	float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;
	float steepness = 1.5;
	// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
	float ComputeHalfSpace(float3 wsDir)
	{
		//float4 _HeightParams = float4(1,1,1,1);
		//wsDir.y = wsDir.y - abs(11.2*_cameraDiff.x);// -0.4;// +abs(11.2*_cameraDiff.x);

		float3 wpos = _CameraWS.xyz + wsDir; // _CameraWS + wsDir;
		float FH = _HeightParams.x;
		float3 C = _CameraWS.xyz;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = _HeightParams.w * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y - FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1 - 2 * k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
		return g;
	}

	//SM v1.7
	float3 totalRayleigh(float3 lambda) {
		float pi = 3.141592653589793238462643383279502884197169;
		float n = 1.0003; // refraction of air
		float N = 2.545E25; //molecules per air unit volume 								
		float pn = 0.035;
		return (8.0 * pow(pi, 3.0) * pow(pow(n, 2.0) - 1.0, 2.0) * (6.0 + 3.0 * pn)) / (3.0 * N * pow(lambda, float3(4.0, 4.0, 4.0)) * (6.0 - 7.0 * pn));
	}

	float rayleighPhase(float cosTheta)
	{
		return (3.0 / 4.0) * (1.0 + pow(cosTheta, 2.0));
	}

	float3 totalMie(float3 lambda, float3 K, float T)
	{
		float pi = 3.141592653589793238462643383279502884197169;
		float v = 4.0;
		float c = (0.2 * T) * 10E-18;
		return 0.434 * c * pi * pow((2.0 * pi) / lambda, float3(v - 2.0, v - 2.0, v - 2.0)) * K;
	}

	float hgPhase(float cosTheta, float g)
	{
		float pi = 3.141592653589793238462643383279502884197169;
		return (1.0 / (4.0*pi)) * ((1.0 - pow(g, 2.0)) / pow(abs(1.0 - 2.0*g*cosTheta + pow(g, 2.0)), 1.5));
	}

	float sunIntensity(float zenithAngleCos)
	{
		float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;//pi/
		float steepness = 1.5;
		float EE = 1000.0;
		return EE * max(0.0, 1.0 - exp(-((cutoffAngle - acos(zenithAngleCos)) / steepness)));
	}

	float logLuminance(float3 c)
	{
		return log(c.r * 0.2126 + c.g * 0.7152 + c.b * 0.0722);
	}

	float3 tonemap(float3 HDR)
	{
		float Y = logLuminance(HDR);
		float low = exp(((Y*lumFac + (1.0 - lumFac))*luminance) - bias - contrast / 2.0);
		float high = exp(((Y*lumFac + (1.0 - lumFac))*luminance) - bias + contrast / 2.0);
		float3 ldr = (HDR.rgb - low) / (high - low);
		return float3(ldr);
	}
	/////////////////// END SCATTER
	half _Opacity;
	struct VaryingsBC
	{
		//float2 uv        : TEXCOORD0;
		//float4 vertex : SV_POSITION;
		//UNITY_VERTEX_OUTPUT_STEREO
		float4 position : SV_POSITION;// SV_Position;
		float2 texcoord : TEXCOORD0;
		float3 ray : TEXCOORD1;
		float2 uvFOG : TEXCOORD2;
		float4 interpolatedRay : TEXCOORD3;
		UNITY_VERTEX_OUTPUT_STEREO
	};
	/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
	/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
	/////////////////// END MORE FOG URP ////////////////////////////////////////////////////

	//v0.2 - SHADOWS
	float UVRandom(float2 uv)
	{
		float f = dot(float2(12.9898, 78.233), uv);
		return frac(43758.5453 * sin(f));
	}
	float _Brightness = 1;//
	float _Extinct = 0.5f;//
	float3 _ExtinctColor = float3(0.4, 0.6, 0.7);//
	float _Scatter = 0.5f;//
	float3 _ScatterColor = float3(0.4, 0.6, 0.7);//
	float _LightSpread = 8;//	
	float _FogHeight = 1;
	float _RaySamples = 8;
	float blendVolumeLighting = 0;

	float AddDensity(float dense, float scatter, float dist, float3 ray, float3 rayStart)
	{
		return saturate(dense * (scatter / _FogHeight)  * (1.0 - exp(dist * -ray.y * _FogHeight)) * exp(-_FogHeight * rayStart.y) / (ray.y));
	}
	float3 VolumeFog(float3 sourceImg, float dist, float3 ray, float3 rayStart, float3 WorldPosition, float2 texcoord)
	{
		int steps = _RaySamples;
		float stepLength = dist / steps;
		float3 step = ray * stepLength;
		/*
		Light directionalLight = GetMainLight();
		half shadowdirectionalLight = GetMainLightShadowStrength();
		ShadowSamplingData shadowSampleData = GetMainLightShadowSamplingData();

		float3 pos = rayStart + step;
		float ColorE = 0;
		float ColorA = 0;

		float lightINT = max(dot(normalize(directionalLight.direction), -ray), 0);
		float3 ColorFOG = lerp(_ScatterColor, directionalLight.color, _LightSpread * pow(lightINT, 7));
		float2 uv = texcoord + _Time.x;
		float attenuate = 0;
		[loop]
		for (int i = 0; i < steps; ++i)
		{
			float4 coordinates = TransformWorldToShadowCoord(pos);

			attenuate = SampleShadowmap(coordinates, TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),
				shadowSampleData, shadowdirectionalLight, false);

			sourceImg = sourceImg * 1 + 0.1*sourceImg * directionalLight.color * attenuate;
			//sourceImg = sourceImg* pow(attenuate, 0.1) + 0.1*sourceImg * directionalLight.color  * pow(attenuate,2) ;
			//sourceImg = sourceImg * pow(attenuate, 0.04) + 0.0001*directionalLight.color * attenuate;
			//sourceImg = sourceImg * pow(attenuate, 0.04);
			//sourceImg += saturate(_Brightness * (_Scatter / _FogHeight)  * (1.0 - exp(stepLength * -ray.y * _FogHeight)) * exp(-_FogHeight * pos.y) / (ray.y));

			//ColorE += 1000000 * AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r), stepLength, ray, pos);
			//ColorA += 0.005* pow(attenuate, 21);// 1000000 * AddDensity(_Brightness, _Scatter * ColorFOG.r * attenuate, stepLength, ray, pos);
			//ColorA += 0.01*saturate(_Brightness * (_Scatter* ColorFOG.r * attenuate / _FogHeight)  * (1.0 - exp(stepLength * -ray.y * _FogHeight)) * exp(-_FogHeight * pos.y) / (ray.y));
			//ColorE += 0.01*saturate(_Brightness * (_Extinct * (1 - _ExtinctColor.r) / _FogHeight)  * (1.0 - exp(stepLength * -ray.y * _FogHeight)) * exp(-_FogHeight * pos.y) / (ray.y));

			float rand = UVRandom(uv + i + 1);
			//pos += (step + step * rand * 0.8);
			pos += (step + step * rand * 0.8);
		}

		//SPOT - POINT LIGHTS
		//https://forum.unity.com/threads/how-to-make-additional-lights-cast-shadows-toon-shader-setup.757730/
		half distanceAtten = 0;
		int pixelLightCount = GetAdditionalLightsCount();

		//https://github.com/Unity-Technologies/Graphics/blob/6de5959b69ae36a22e0745974809353e03665654/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
		[loop]
		for (int k = 0; k < 3; ++k); //for (int k = 0; k < pixelLightCount; ++k)
		{
			float3 stepA = ray * stepLength;
			pos = rayStart + stepA;

			[loop]
			for (int m = 0; m < steps; ++m)
			{
				Light light = GetAdditionalLight(k, pos);
				distanceAtten = light.distanceAttenuation;
				attenuate = light.shadowAttenuation;

				sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAtten * attenuate;

				float lightINT = max(dot(normalize(light.direction), -ray), 0);
				float3  ColorFOG = lerp(_ScatterColor, light.color, _LightSpread * pow(lightINT, 7));

				ColorE += 10 * AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r), stepLength, ray, pos);
				ColorA += 110 * AddDensity(_Brightness, _Scatter * ColorFOG.r * distanceAtten * attenuate, stepLength, ray, pos);

				float rand = UVRandom(uv + m + 1);
				pos += stepA + stepA * rand * 0.8;
			}
		}
		return sourceImg - sourceImg * ColorE + ColorA;
		*/
		return sourceImg;
	}
	//END v0.2 SHADOWS

	//// END v4.7







	/////////// REFRACT LINE
	//v4.6a
	float _BumpMagnitudeRL, _BumpScaleRL, _BumpLineHeight;
	//v4.6
	float _refractLineWidth;
	float _refractLineXDisp;
	float _refractLineXDispA;
	float4 _refractLineFade;
	//v4.3
	//sampler2D _MainTex, _SourceTex, _WaterInterfaceTex;
	//TEXTURE2D_X(_BumpTex);// sampler2D _BumpTex;
	sampler2D _BumpTex;
	float4 _BumpTex_ST;
	float _BumpMagnitude, _BumpScale;
	float4 _BumpVelocity, _underWaterTint;
	float _underwaterDepthFade;
	//float2 intensity;
	//v4.6
	//half4 _MainTex_TexelSize;
	//half4 _BlurOffsets;
	half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
	{
		half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
		bump.xy = bump.wy - half2(1.0, 1.0);
		half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1, 0, 1);
		return normalize(worldNormal);
	}
	//half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
	half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength)
	{
		half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
		bump = bump * 0.5;
		half3 normal = UnpackNormal(bump);
		normal.xy *= bumpStrength;
		return normalize(normal);
	}
	////////// END REFRACT LINE








	// Vertex manipulation
	float2 TransformTriangleVertexToUV(float2 vertex)
	{
		float2 uv = (vertex + 1.0) * 0.5;
		return uv;
	}

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
#if UNITY_UV_STARTS_AT_TOP
		float2 uv1 : TEXCOORD1;
#endif		
	};

	struct v2f_radial {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 blurVector : TEXCOORD1;
	};

	struct Varyings
	{
		float2 uv        : TEXCOORD0;
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	


	float Linear01DepthA(float2 uv)
	{
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
#else
		return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
#endif
	}

	float4 FragGrey(v2f i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
		//float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
		//color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
		//return color/2 + colorB/2;
		return color ;
	}

	half4 fragScreenOLD(v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			//half4 colorA = tex2D(_MainTex, i.uv.xy);
			half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy); // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
		#if !UNITY_UV_STARTS_AT_TOP
																				 ///half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
			half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v0.2 //i.uv1.xy);//v0.2
		#else
																				 //half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
			half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v1.1
		#endif
			half4 depthMask = saturate(colorB * _SunColor);
			return  1.0f - (1.0f - colorA) * (1.0f - depthMask);//colorA * 5.6;// 1.0f - (1.0f - colorA) * (1.0f - depthMask);
	}



	half4 fragScreen(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			//half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy); 
			float2 positionSS = i.uv * _ScreenSize.xy;
			float2 positionSS1 = i.uv1 * _ScreenSize.xy;
			half4 colorA = LOAD_TEXTURE2D_X(_MainTex, positionSS);

			if (colorA.r < 0.05 && colorA.g < 0.05 && colorA.b < 0.05) {//if (tex.g == 0) {
				//colorA = float4(0.2, 0.7, 0.9, colorA.a);
				//find first color and sample it
				[loop]
				int counter = 0;
				//for (float j = 0; j < 1 * _ScreenSize.y; j = j + _MainTex_TexelSize.y* _ScreenSize.y) {
				for (float j = _ScreenSize.y; j >= 0; j = j - _MainTex_TexelSize.y* _ScreenSize.y) {
					//if (j > positionSS.y *1) {
					if (j < positionSS.y * 1) {
						float4 mainTexUP = LOAD_TEXTURE2D_X(_MainTex, float2(positionSS.x, j));
						if (mainTexUP.r < 0.05 && mainTexUP.g < 0.05 && mainTexUP.b < 0.05) {
							counter++;
						}
						else {
							//float4 mainTexUPA = LOAD_TEXTURE2D_X(_MainTex, float2(positionSS.x, j + counter * _MainTex_TexelSize.y* _ScreenSize.y * 1));
							float4 mainTexUPA = LOAD_TEXTURE2D_X(_MainTex, float2(positionSS.x, j - counter * _MainTex_TexelSize.y* _ScreenSize.y * 1));
							colorA = mainTexUPA;//mainTexUP;
							//colorA.a = 0; 
							break;
						}
					}
				}
			}

			#if !UNITY_UV_STARTS_AT_TOP																		
			//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
			half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS1);
			#else																		
				//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v1.1
				half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS);//v1.1
			#endif
			half4 depthMask = saturate(colorB * _SunColor);
			return  1.0f - (1.0f - colorA) * (1.0f - depthMask);//colorA * 5.6;// 1.0f - (1.0f - colorA) * (1.0f - depthMask);
	}

		//v4.6
	half4 _BlurOffsets;
	half4 fragAdd(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		//half4 colorA = tex2D(_MainTex, i.uv.xy);
		half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
#if !UNITY_UV_STARTS_AT_TOP
		//half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy); //v0.1 - i.uv1.xy
#else
		//half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
#endif
		half4 depthMask = saturate(colorB * _SunColor);
		return 1 * colorA + depthMask;
	}

	struct Attributes
	{
		float4 positionOS       : POSITION;
		float2 uv               : TEXCOORD0;
	};

	v2f vert(Attributes v) {//v2f vert(AttributesDefault v) { //appdata_img v) {
		//v2f o;
		v2f o = (v2f)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
		//o.pos = vertexInput.positionCS;
		//o.uv = v.uv;
		//Varyings output = (Varyings)0;
		//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
		//output.vertex = vertexInput.positionCS;
		//output.uv = input.uv;
		//return output;
		//o.pos = UnityObjectToClipPos(v.vertex);
		//	o.pos = float4(v.vertex.xy, 0.0, 1.0);
		//	float2 uv = TransformTriangleVertexToUV(v.vertex.xy);
		o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
		float2 uv = v.uv;
		//o.uv = uv;// v.texcoord.xy;
		//o.uv1 = uv.xy;
		//// NEW 1
		//o.pos = float4(v.positionOS.xy, 0.0, 1.0);
		//uv = TransformTriangleVertexToUV(v.positionOS.xy);

#if !UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
		//uv.y = 1-uv.y;
#endif
		o.uv = uv;// v.texcoord.xy;

#if !UNITY_UV_STARTS_AT_TOP
		o.uv = uv.xy;//o.uv1 = uv.xy;
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;//o.uv1.y = 1 - o.uv1.y;
#endif	
		return o;
	}


	struct v2fA {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		//#if UNITY_UV_STARTS_AT_TOP
				float2 uv1 : TEXCOORD1;
		//#else
		//#endif	

		//float2 uv1   : TEXCOORD1;
		float2 bumpUV : TEXCOORD2;
		//v4.6
		half2 taps[4] : TEXCOORD3;
		UNITY_VERTEX_OUTPUT_STEREO
	};
	v2fA Vert(Attributes v) {
		v2fA o = (v2fA)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);		
		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);	
		o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
		float2 uv = v.uv;		
#if !UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);	
#endif
		o.uv = uv;

#if !UNITY_UV_STARTS_AT_TOP
		o.uv = uv.xy;
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	

		//v4.7
		o.bumpUV = o.uv * _BumpTex_ST.xy + _BumpTex_ST.zw;

		//v4.6
//#ifdef UNITY_SINGLE_PASS_STEREO
//			// we need to keep texel size correct after the uv adjustment.
//		o.taps[0] = UnityStereoScreenSpaceUVAdjust(o.uv + _MainTex_TexelSize * _BlurOffsets.xy * (1.0f / _MainTex_ST.xy), _MainTex_ST);
//		o.taps[1] = UnityStereoScreenSpaceUVAdjust(o.uv - _MainTex_TexelSize * _BlurOffsets.xy * (1.0f / _MainTex_ST.xy), _MainTex_ST);
//		o.taps[2] = UnityStereoScreenSpaceUVAdjust(o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTex_ST.xy), _MainTex_ST);
//		o.taps[3] = UnityStereoScreenSpaceUVAdjust(o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTex_ST.xy), _MainTex_ST);
//#else
		//o.taps[0] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy;// *_ScreenSize.xy;// *scalerMask;
		//o.taps[1] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy;// *_ScreenSize.xy;
		//o.taps[2] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1);// *_ScreenSize.xy;
		//o.taps[3] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1);// *_ScreenSize.xy;
//#endif

		o.taps[0] = o.uv + _MainTex_TexelSize * 1;// *_ScreenSize.xy;// *scalerMask;
		o.taps[1] = o.uv - _MainTex_TexelSize * 1;// *_ScreenSize.xy;
		o.taps[2] = o.uv + _MainTex_TexelSize * 1 * half2(1, -1);// *_ScreenSize.xy;
		o.taps[3] = o.uv - _MainTex_TexelSize * 1 * half2(1, -1);// *_ScreenSize.xy;


		return o;
	}


//	//v4.7
//	struct VaryingsA
//	{
//		//float4 positionCS : SV_POSITION;
//		//float2 texcoord   : TEXCOORD0;
//		float4 pos : SV_POSITION;
//		float2 uv   : TEXCOORD0;
//		float2 uv1   : TEXCOORD1;
//		float2 bumpUV : TEXCOORD2;
//		//v4.6
//		half2 taps[4] : TEXCOORD3;
//
//		UNITY_VERTEX_OUTPUT_STEREO
//	};
//	VaryingsA Vert(Attributes input)
//	{
//		VaryingsA output;
//		UNITY_SETUP_INSTANCE_ID(input);
//		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//		output.pos = GetFullScreenTriangleVertexPosition(input.vertexID);
//		output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
//		output.uv1 = GetFullScreenTriangleTexCoord(input.vertexID);
//
//		output.bumpUV = output.uv * _BumpTex_ST.xy + _BumpTex_ST.zw;
//
//		//v4.6
//#ifdef UNITY_SINGLE_PASS_STEREO
//			// we need to keep texel size correct after the uv adjustment.
//		output.taps[0] = UnityStereoScreenSpaceUVAdjust(output.uv + _MainTexA_TexelSize * _BlurOffsets.xy * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
//		output.taps[1] = UnityStereoScreenSpaceUVAdjust(output.uv - _MainTexA_TexelSize * _BlurOffsets.xy * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
//		output.taps[2] = UnityStereoScreenSpaceUVAdjust(output.uv + _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
//		output.taps[3] = UnityStereoScreenSpaceUVAdjust(output.uv - _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
//#else
//		output.taps[0] = output.uv + _MainTexA_TexelSize * _BlurOffsets.xy;// *_ScreenSize.xy;// *scalerMask;
//		output.taps[1] = output.uv - _MainTexA_TexelSize * _BlurOffsets.xy;// *_ScreenSize.xy;
//		output.taps[2] = output.uv + _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1);// *_ScreenSize.xy;
//		output.taps[3] = output.uv - _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1);// *_ScreenSize.xy;
//#endif
//
//		return output;
//	}


	v2f_radial vert_radial(Attributes v) {//v2f_radial vert_radial(AttributesDefault v) { //appdata_img v) {
		//v2f_radial o;

		v2f_radial o = (v2f_radial)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		////		o.pos = UnityObjectToClipPos(v.vertex);

		//o.pos = float4(v.vertex.xyz,1);
		//o.pos = float4(v.vertex.xy, 0.0, 1.0);
		//float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
		o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
		float2 uv = v.uv;
		//output.vertex = vertexInput.positionCS;

		//uv = TransformTriangleVertexToUV(vertexInput.positionCS.xy);

		#if !UNITY_UV_STARTS_AT_TOP
				//uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
		#endif

		o.uv.xy = uv;//v.texcoord.xy;
					 //o.blurVector = (_SunPosition.xy - v.texcoord.xy) * _BlurRadius4.xy;
		//o.uv1 = uv.xy;
		//o.uv.y = 1 - o.uv.y;
		//uv.y = 1 - uv.y;
		//o.uv.y = 1 - o.uv.y;
		//_SunPosition.y = _SunPosition.y*0.5 + 0.5;
		//_SunPosition.x = _SunPosition.x*0.5 + 0.5;
		o.blurVector = (_SunPosition.xy - uv.xy) * _BlurRadius4.xy;

		return o;
	}

	half4 frag_radial(v2f_radial i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		half4 color = half4(0,0,0,0);
		for (int j = 0; j < SAMPLES_INT; j++)
		{
			//half4 tmpColor = tex2D(_MainTex, i.uv.xy);
			half4 tmpColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
			color += tmpColor;
			i.uv.xy += i.blurVector;
		}
		return color / SAMPLES_FLOAT;
	}

	half TransformColor(half4 skyboxValue) {
		return dot(max(skyboxValue.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
	}

	half4 frag_depth(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	#if !UNITY_UV_STARTS_AT_TOP
			//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv1.xy);
			float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams); //v0.1 URP i.uv1.xy
	#else
			//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
			float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams);
	#endif

		//half4 tex = tex2D(_MainTex, i.uv.xy);
		half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
		//depthSample = Linear01Depth(depthSample, _ZBufferParams);

		// consider maximum radius
	#if !UNITY_UV_STARTS_AT_TOP
		half2 vec = _SunPosition.xy - i.uv.xy; //i.uv1.xy;
	#else
		half2 vec = _SunPosition.xy - i.uv.xy;
	#endif
		half dist = saturate(_SunPosition.w - length(vec.xy));

		half4 outColor = 0;

		// consider shafts blockers
		//if (depthSample > 0.99)
		//if (depthSample > 0.103)
		if (depthSample > 1- 0.018) {//if (depthSample < 0.018) {
			//outColor = TransformColor(tex) * dist;
		}





#if !UNITY_UV_STARTS_AT_TOP
		if (depthSample < 0.018) {
			outColor = TransformColor(tex) * dist;
		}
#else
		if (depthSample > 1 - 0.018) {
			outColor = TransformColor(tex) * dist;
		}
#endif
		//return float4(1, 0, 0, 0);
		return outColor * 1;
	}

	//inline half Luminance(half3 rgb)
	//{
		//return dot(rgb, unity_ColorSpaceLuminance.rgb);
	//	return dot(rgb, rgb);
	//}

	half4 frag_nodepth(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	#if !UNITY_UV_STARTS_AT_TOP
			//float4 sky = (tex2D(_Skybox, i.uv1.xy));
			float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy);
	#else
			//float4 sky = (tex2D(_Skybox, i.uv.xy));
			float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy); //i.uv1.xy;
	#endif

			//float4 tex = (tex2D(_MainTex, i.uv.xy));
			half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
			//sky = float4(0.3, 0.05, 0.05,  1);
			/// consider maximum radius
	#if !UNITY_UV_STARTS_AT_TOP
			half2 vec = _SunPosition.xy - i.uv.xy;
	#else
			half2 vec = _SunPosition.xy - i.uv.xy;//i.uv1.xy;
	#endif
			half dist = saturate(_SunPosition.w - length(vec));

			half4 outColor = 0;

			// find unoccluded sky pixels
			// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded


			if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2) {
				outColor = TransformColor(tex) * dist;
				//outColor = TransformColor(sky) * dist;
			}

			return outColor * 1;
	}







		////////////// WATER AIR START




		/////////////////////////////////////////////////////


		//v4.7
		/////// FOG URP //////////////////////////
		/////// FOG URP //////////////////////////
		/////// FOG URP //////////////////////////
		// Vertex shader that procedurally outputs a full screen triangle
		VaryingsBC Vertex(uint vertexID : SV_VertexID) //Varyings Vertex(Attributes v)
	{
		//Varyings o = (Varyings)0;
		//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);

		//float4 vpos = float4(vertexInput.positionCS.x, vertexInput.positionCS.y, vertexInput.positionCS.z, 1.0);
		//o.position = vpos;

		////o.position.z = 0.1;

		//float2 uv = v.uv;
		//
		//o.texcoord = float2(uv.x,uv.y);
		////o.texcoord = uv.xy;

		//float far = _ProjectionParams.z ;
		//float3 rayPers = -mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
		//rayPers.y = rayPers.y + 1* abs(_cameraDiff.x * 15111);

		//o.ray = rayPers;//lerp(rayPers, rayOrtho, isOrtho);
		//o.uvFOG = uv.xy;
		//half index = vpos.z;
		//o.interpolatedRay.xyz = _FrustumCornersWS[(int)index] ;// vpos;  // _FrustumCornersWS[(int)index];
		//o.interpolatedRay.w = index;
		//return o;

		// Render settings
		float far = _ProjectionParams.z;
		float2 orthoSize = unity_OrthoParams.xy;
		float isOrtho = _ProjectionParams.w; // 0: perspective, 1: orthographic
											 // Vertex ID -> clip space vertex position
		float x = (vertexID != 1) ? -1 : 3;
		float y = (vertexID == 2) ? -3 : 1;
		float3 vpos = float3(x, y, 1.0);

		// Perspective: view space vertex position of the far plane
		float3 rayPers = mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
		//rayPers.y = rayPers.y - abs(_cameraDiff.x * 15111);

		// Orthographic: view space vertex position
		float3 rayOrtho = float3(orthoSize * vpos.xy, 0);

		VaryingsBC o;
		o.position = float4(vpos.x, -vpos.y, 1, 1);
		o.texcoord = (vpos.xy + 1) / 2;
		o.ray = lerp(rayPers, rayOrtho, isOrtho);

		//MINE
		float3 vA = vpos;
		float deg = _cameraRoll;
		float alpha = deg * 3.14 / 180.0;
		float sina, cosa;
		sincos(alpha, sina, cosa);
		float2x2 m = float2x2(cosa, -sina, sina, cosa);

		float3 tmpV = float3(mul(m, vA.xy), vA.z).xyz;
		float2 uvFOG = TransformTriangleVertexToUV(tmpV.xy);
		o.uvFOG = uvFOG.xy;

		half index = vpos.z;
		o.interpolatedRay.xyz = vpos;  // _FrustumCornersWS[(int)index];
		o.interpolatedRay.w = index;

		return o;
	}

	float3 ComputeViewSpacePosition(VaryingsBC input, float z)
	{
		// Render settings
		float near = _ProjectionParams.y;
		float far = _ProjectionParams.z;
		float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic
											 // Z buffer sample
											 // Far plane exclusion
#if !defined(EXCLUDE_FAR_PLANE)
		float mask = 1;
#elif defined(UNITY_REVERSED_Z)
		float mask = z > 0;
#else
		float mask = z < 1;
#endif

		// Perspective: view space position = ray * depth
		float lindepth = Linear01DepthA(input.texcoord.xy);
		lindepth = Linear01Depth(lindepth, _ZBufferParams);// Linear01Depth(lindepth);
		float3 vposPers = input.ray * lindepth;

		//if (Linear01DepthA(input.texcoord.xy) ==0) {
		//	vposPers = input.ray;
		//}

		// Orthographic: linear depth (with reverse-Z support)
#if defined(UNITY_REVERSED_Z)
		float depthOrtho = -lerp(far, near, z);
#else
		float depthOrtho = -lerp(near, far, z);
#endif

		// Orthographic: view space position
		float3 vposOrtho = float3(input.ray.xy, depthOrtho);

		// Result: view space position
		return lerp(vposPers, vposOrtho, isOrtho) * mask;
	}

	half4 VisualizePosition(VaryingsBC input, float3 pos)
	{
		const float grid = 5;
		const float width = 3;

		pos *= grid;

		// Detect borders with using derivatives.
		float3 fw = fwidth(pos);
		half3 bc = saturate(width - abs(1 - 2 * frac(pos)) / fw);

		// Frequency filter
		half3 f1 = smoothstep(1 / grid, 2 / grid, fw);
		half3 f2 = smoothstep(2 / grid, 4 / grid, fw);
		bc = lerp(lerp(bc, 0.5, f1), 0, f2);

		// Blend with the source color.
		//half4 c = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, input.texcoord);
		half4 c = LOAD_TEXTURE2D_X(_MainTex, input.texcoord);
		c.rgb = SRGBToLinear(lerp(LinearToSRGB(c.rgb), bc, 0.5));

		return c;
	}

	///////////////// FRAGMENT /////////////////////////////////

	float4x4 _InverseView;

	float2 WorldToScreenPos(float3 pos) {
		pos = normalize(pos - _WorldSpaceCameraPos)*(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
		float2 uv = float2(0, 0);
		float4 toCam = mul(unity_WorldToCamera, float4(pos.xyz, 1));
		float camPosZ = toCam.z;
		float height = 2 * camPosZ / unity_CameraProjection._m11;
		float width = _ScreenParams.x / _ScreenParams.y * height;
		uv.x = (toCam.x + width / 2) / width;
		uv.y = (toCam.y + height / 2) / height;
		return uv;
	}

	float2 raySphereIntersect(float3 r0, float3 rd, float3 s0, float sr) {

		float a = dot(rd, rd);
		float3 s0_r0 = r0 - s0;
		float b = 2.0 * dot(rd, s0_r0);
		float c = dot(s0_r0, s0_r0) - (sr * sr);
		float disc = b * b - 4.0 * a* c;
		if (disc < 0.0) {
			return float2(-1.0, -1.0);
		}
		else {
			return float2(-b - sqrt(disc), -b + sqrt(disc)) / (2.0 * a);
		}
	}

	float3x3 rotationMatrix(float3 axis, float angle)
	{
		axis = normalize(axis);
		float s = sin(angle);
		float c = cos(angle);
		float oc = 1.0 - c;

		return float3x3 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
			oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
			oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
	}
	float4x4 rotationMatrix4(float3 axis, float angle)
	{
		axis = normalize(axis);
		float s = sin(angle);
		float c = cos(angle);
		float oc = 1.0 - c;

		return float4x4 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
			oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
			oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
			0.0, 0.0, 0.0, 1.0);
	}

	half4 Fragment(VaryingsBC input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float3 forward = mul((float3x3)(unity_WorldToCamera), float3(0, 0, 1));
		//float zsample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
		//float zsample = Linear01Depth(input.texcoord.xy); //CORRECT WAY

		float zsample = Linear01DepthA(input.texcoord.xy);
		float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);// Linear01Depth(lindepth);
																			   //float3 vposPers = input.ray * lindepth;

		float3 vpos = ComputeViewSpacePosition(input, zsample);
		float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;
		//float depth = Linear01Depth(zsample * (zsample < 1.0));
		//	float depth =  Linear01Depth(zsample * (zsample < 1.0)); //////// CHANGE 2 URP
		//float depth = Linear01DepthA(input.texcoord.xy);


		//float depthSample = Linear01Depth(input.texcoord.xy); //CORRECT WAY
		//if (depthSample > 0.00000001) { //affect frontal
		//if (depthSample == 0) { 		//affect background
		//if (depthSample < 0.00000001) {	//affect background
		//if (depth > 0.00000001) {
		//return float4(1, 0, 0, 1);
		//}
		//else {
		//return float4(1, 1, 0, 1);
		//}

		//if (depth ==0) {
		//depth = 1; //EXPOSE BACKGROUND AS FORGROUND TO GET SCATTERING
		//}

		float4 wsDir = depth * float4(input.ray, 1); // input.interpolatedRay;	

													 //_CameraWS = float4(85.8, -102.19,-10,1);
		float4 wsPos = (_CameraWS)+wsDir;// _CameraWS + wsDir; //////// CHANGE 1 URP
										 //return wsPos;


										 //return ((wsPos) *0.1);

										 ///// SCATTER
										 //float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);
										 //float3 lightDirection = float3(v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, -v3LightDir.z);
		float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);


		float3 pointToCamera = (wpos - _WorldSpaceCameraPos);// * 0.47;
		int steps = 2;
		float stepCount = 1;
		float step = length(pointToCamera) / steps;

		//int noise3D = 0;
		half4 noise;
		half4 noise1;
		half4 noise2;
		if (noise3D == 0) {
			float fixFactor1 = 0;
			float fixFactor2 = 0;
			float dividerScale = 1; //1
			float scaler1 = 1.00; //0.05
			float scaler2 = 0.8; //0.01
			float scaler3 = 0.3; //0.01
			float signer1 = 0.004 / (dividerScale * 1.0);//0.4 !!!! (0.005 for 1) (0.4 for 0.05) //0.004
			float signer2 = 0.004 / (dividerScale * 1.0);//0.001

			if (_cameraDiff.w < 0) {
				fixFactor1 = -signer1 * 90 * 2 * 2210 / 1 * (dividerScale / 1);//2210
				fixFactor2 = -signer2 * 90 * 2 * 2210 / 1 * (dividerScale / 1);
			}
			float hor1 = -_cameraDiff.w * signer1 *_cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor1;
			float hor2 = -_cameraDiff.w * signer2 *_cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor2;
			float hor3 = -_cameraDiff.w * signer2 *_cameraDiff.y * 1210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 2 + fixFactor2;

			float vert1 = _cameraTiltSign * _cameraDiff.x * 0.77 * 1.05 * 160 + 0.0157*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
				- 2 * 0.33 * _WorldSpaceCameraPos.z * 2.13 + 50 * abs(cos(_WorldSpaceCameraPos.z * 0.01)) + 35 * abs(sin(_WorldSpaceCameraPos.z * 0.005));

			float vert2 = _cameraTiltSign * _cameraDiff.x * 0.20 * 1.05 * 160 + 0.0157*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
				- 1 * 0.33 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

			float vert3 = _cameraTiltSign * _cameraDiff.x * 0.10 * 1.05 * 70 + 0.0117*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
				- 1 * 1.03 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

			noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (float2(input.texcoord.x*scaler1 * 1, input.texcoord.y*scaler1))
				+ (-0.001*float2((0.94)*hor1, vert1)) + 3 * abs(cos(_Time.y *1.22* 0.012)))) * 2 * 9;
			noise1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy*scaler2)
				+ (-0.001*float2((0.94)*hor2, vert2) + 3 * abs(cos(_Time.y *1.22* 0.010))))) * 3 * 9;
			noise2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy*scaler3)
				+ (-0.001*float2((0.94)*hor3, vert3) + 1 * abs(cos(_Time.y *1.22* 0.006))))) * 3 * 9;
		}
		else {

			/////////// NOISE 3D //////////////
			const float epsilon = 0.0001;

			float2 uv = input.texcoord * 4.0 + float2(0.2, 1) * _Time.y * 0.01;

			/*#if defined(SNOISE_AGRAD) || defined(SNOISE_NGRAD)
			#if defined(THREED)
			float3 o = 0.5;
			#else
			float2 o = 0.5;
			#endif
			#else*/
			float o = 0.5 * 1.5;
			//#endif

			float s = 0.011;

			/*#if defined(SNOISE)
			float w = 0.25;
			#else*/
			float w = 0.02;
			//#endif

			//#ifdef FRACTAL

			for (int i = 0; i < 5; i++)
				//#endif
			{
				float3 coord = wpos + float3(_Time.y * 3 * _NoiseSpeed.x,
					_Time.y * _NoiseSpeed.y,
					_Time.y * _NoiseSpeed.z);
				float3 period = float3(s, s, 1.0) * 1111;



				//#if defined(CNOISE)
				o += cnoise(coord * 0.17 * _NoiseScale) * w;

				//float3 pointToCamera = (wpos - _WorldSpaceCameraPos);// * 0.47;
				//int steps = 2;
				//float stepCount = 1;
				//float step = length(pointToCamera) / steps;
				for (int j = 0; j < steps; j++) {
					//ray trace noise												
					float3 coordAlongRay = _WorldSpaceCameraPos + normalize(pointToCamera) * step
						+ float3(_Time.y * 6 * _NoiseSpeed.x,
							_Time.y * _NoiseSpeed.y,
							_Time.y * _NoiseSpeed.z);
					o += 1.5*cnoise(coordAlongRay * 0.17 * _NoiseScale) * w * 1;
					//stepCount++;
					if (depth < 0.99999) {
						o += depth * 45 * _NoiseThickness;
					}
					step = step + step;
				}

				s *= 2.0;
				w *= 0.5;
			}
			noise = float4(o, o, o, 1);
			noise1 = float4(o, o, o, 1);
			noise2 = float4(o, o, o, 1);
		}

		float cosTheta = dot(normalize(wsDir.xyz), lightDirection);
		cosTheta = dot(normalize(wsDir.xyz), -lightDirection);

		float lumChange = clamp(luminance * pow(abs(((1 - depth) / (_OcclusionDrop * 0.1 * 2))), _OcclusionExp), luminance, luminance * 2);
		if (depth <= _OcclusionDrop * 0.1 * 1) {
			luminance = lerp(4 * luminance, 1 * luminance, (0.001 * 1) / (_OcclusionDrop * 0.1 - depth + 0.001));
		}

		float3 up = float3(0.0, 1.0, 0.0); //float3(0.0, 0.0, 1.0);			
		float3 lambda = float3(680E-8 + _TintColorL.r * 0.000001, 550E-8 + _TintColorL.g * 0.000001, 450E-8 + _TintColorL.b * 0.000001);
		float3 K = float3(0.686 + _TintColorK.r * 0.1, 0.678 + _TintColorK.g * 0.1, 0.666 + _TintColorK.b * 0.1);
		float  rayleighZenithLength = 8.4E3;
		float  mieZenithLength = 1.25E3;
		float  pi = 3.141592653589793238462643383279502884197169;
		float3 betaR = totalRayleigh(lambda) * reileigh * 1000;
		float3 lambda1 = float3(_TintColor.r, _TintColor.g, _TintColor.b)* 0.0000001;//  680E-8, 1550E-8, 3450E-8); //0.0001//0.00001
		lambda = lambda1;
		float3 betaM = totalMie(lambda1, K, turbidity * Multiplier2) * mieCoefficient;
		float zenithAngle = acos(max(0.0, dot(up, normalize(lightDirection))));
		float sR = rayleighZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
		float sM = mieZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
		float  rPhase = rayleighPhase(cosTheta*0.5 + 0.5);
		float3 betaRTheta = betaR * rPhase;
		float  mPhase = hgPhase(cosTheta, mieDirectionalG) * Multiplier1;
		float3 betaMTheta = betaM * mPhase;
		float3 Fex = exp(-(betaR * sR + betaM * sM));
		float  sunE = sunIntensity(dot(lightDirection, up));
		float3 Lin = ((betaRTheta + betaMTheta) / (betaR + betaM)) * (1 - Fex) + sunE * Multiplier3*0.0001;
		float  sunsize = 0.0001;
		float3 L0 = 1.5 * Fex + (sunE * 1.0 * Fex)*sunsize;
		float3 FragColor = tonemap(Lin + L0);//tonemap(Lin + L0);
											 ///// END SCATTER

											 ///////////////return float4(FragColor,1);

											 //occlusion !!!!
		//float4 sceneColor = Multiplier3 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord.xy);
		float4 sceneColor = Multiplier3 * LOAD_TEXTURE2D_X(_MainTex, input.texcoord.xy); //v4.7

		//return sceneColor+ (float4(FragColor, 1));
		//return 0 + (float4(FragColor, 1));


		float3 subtractor = saturate(pow(abs(dot(normalize(input.ray), normalize(lightDirection))),36)) - (float3(1, 1, 1)*depth * 1);
		if (depth < _OcclusionDrop * 0.1) {
			FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), _OcclusionExp));
		}
		else {
			if (depth < 0.9999) {
				FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), 0.001));
			}
		}


		////return float4(FragColor, 1);


		//SCATTER
		int doHeightA = 1;
		int doDistanceA = 1;
		//float g = ComputeDistance(input.ray, depth) - _DistanceOffset/_WorldSpaceCameraPos.y;
		float g = ComputeDistance(input.ray, depth) - _DistanceOffset;
		if (doDistanceA == 1) {
			//g += ComputeDistance(input.ray, depth) - (_DistanceOffset - 100*_WorldSpaceCameraPos.y);
			g += ComputeDistance(input.ray, depth) - _DistanceOffset;
		}
		if (doHeightA == 1) {
			g += ComputeHalfSpace(wpos);
			//g += ComputeHalfSpace(wpos*0.5) - _DistanceOffset;
			//g += ComputeHalfSpace(wpos*0.05);
			//float4 wsDir1 = depth * input.interpolatedRay;
			//float3 wpos1 = _WorldSpaceCameraPos + (wsDir1);// +wsDir; // _CameraWS + wsDir;
			//float FH = _HeightParams.x;
			//float3 C = _WorldSpaceCameraPos.xyz;
			//float3 V = (wsDir1);
			//float3 P = wpos1;
			//float3 aV = _HeightParams.w * V			*		1;
			//float FdotC = _HeightParams.y;
			//float k = _HeightParams.z;
			//float FdotP = P.y - FH;
			//float FdotV = (wsDir1).y;
			//float c1 = k * (FdotP + FdotC);
			//float c2 = (1 - 2 * k) * FdotP;
			//float g1 = min(c2, 0.0);
			//g1 = -length(aV) * (c1 - g1 * g1 / abs(FdotV + 1.0e-5f));
			//g += g1 * 1;
		}

		g = g * pow(abs((noise.r + 1 * noise1.r + _NoiseDensity * noise2.r * 1)), 1.2)*0.3;

		half fogFac = ComputeFogFactorA(max(0.0, g));
		if (zsample <= 1 - 0.999995) {
			//if (zsample >= 0.999995) {
			if (FogSky <= 0) {
				fogFac = 1.0* ClearSkyFac;
			}
			else {
				if (doDistanceA) {
					fogFac = fogFac * ClearSkyFac;
				}
			}
		}

		float4 Final_fog_color = lerp(unity_FogColor + float4(FragColor * 1, 1), sceneColor, fogFac);

		//return Final_fog_color;

		float fogHeight = _Height;
		half fog = ComputeFogFactorA(max(0.0, g));

		//local light
		float3 visual = 0;// VisualizePosition(input, wpos);
		if (1 == 1) {

			float3 light1 = localLightPos.xyz;
			float dist1 = length(light1 - wpos);

			float2 screenPos = WorldToScreenPos(light1);
			float lightRadius = localLightColor.w;

			float dist2 = length(screenPos - float2(input.texcoord.x, input.texcoord.y * 0.62 + 0.23));
			if (
				length(_WorldSpaceCameraPos - wpos) < length(_WorldSpaceCameraPos - light1) - lightRadius
				&&
				dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1)) > 0.95// 0.999
				) { //occlusion
			}
			else {
				float factorOcclusionDist = length(_WorldSpaceCameraPos - wpos) - (length(_WorldSpaceCameraPos - light1) - lightRadius);
				float factorOcclusionDot = dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1));

				Final_fog_color = lerp(Final_fog_color,
					Final_fog_color  * (1 - ((11 - dist2) / 11))
					+ Final_fog_color * float4(2 * localLightColor.x, 2 * localLightColor.y, 2 * localLightColor.z, 1)*(11 - dist2) / 11,
					(localLightPos.w * saturate(1 * 0.1458 / pow(dist2, 0.95))
						+ 0.04*saturate(pow(1 - input.uvFOG.y * (1 - fogHeight), 1.0)) - 0.04)
				);
			}
		}

		//return sceneColor/2 + Final_fog_color/2;

		//#if USE_SKYBOX
		//		// Look up the skybox color.
		//		half3 skyColor = DecodeHDR(texCUBE(_SkyCubemap, i.ray), _SkyCubemap_HDR);
		//		skyColor *= _SkyTint.rgb * _SkyExposure * unity_ColorSpaceDouble;
		//		// Lerp between source color to skybox color with fog amount.
		//		return lerp(half4(skyColor, 1), sceneColor, fog);
		//#else
		// Lerp between source color to fog color with the fog amount.
		half4 skyColor = lerp(_FogColor, sceneColor, saturate(fog));

		float distToWhite = (Final_fog_color.r - 0.99) + (Final_fog_color.g - 0.99) + (Final_fog_color.b - 0.99);

		//Final_fog_color = Final_fog_color + 0.0*Final_fog_color * float4(8,2,0,1);


		//v0.2 - SHADOWS
		float4 result = Final_fog_color * _FogColor + float4(visual, 0);

		float3 posToCameraA = (wpos - _WorldSpaceCameraPos);
		float normalizeMag = length(posToCameraA);
		float4 shadows = half4(VolumeFog(result.rgb, normalizeMag, posToCameraA / normalizeMag, _WorldSpaceCameraPos, wpos, input.texcoord), result.a);
		//END v0.2 SHADOWS

		return shadows * blendVolumeLighting + result * (1 - blendVolumeLighting);// result;
																				  //return Final_fog_color;
																				  //#endif					                
	}

		half4 FragmentTWO(VaryingsBC input) : SV_Target
	{
		//v4.7
		//float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord.xy);	
		float2 positionSS = input.texcoord.xy * _ScreenSize.xy;
		float z = LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS);

		float3 vpos = ComputeViewSpacePosition(input,z);
		//vpos.z = vpos.z +11110;
		float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;

		/*	return float4(_InverseView[3][0], _InverseView[3][1], _InverseView[3][2], _InverseView[3][3]);
		return float4(_InverseView[2][0], _InverseView[2][1], _InverseView[2][2], _InverseView[2][3]);
		return float4(_InverseView[1][0], _InverseView[1][1], _InverseView[1][2], _InverseView[1][3]);
		return float4(_InverseView[0][0], _InverseView[0][1], _InverseView[0][2], _InverseView[0][3]);*/
		//return float4(_ProjectionParams.w, _ProjectionParams.w, _ProjectionParams.w, _ProjectionParams.w);//x=0, y=0.5,z=1,w=0
		//return float4(z,z,z,1);//
		//return float4(input.ray, 1);

		//return float4(vpos, 1);
		return VisualizePosition(input, wpos);
	}
		/////// END FOG URP //////////////////////
		/////// END FOG URP //////////////////////
		/////// END FOG URP //////////////////////






	//struct Attributes
	//{
	//	uint vertexID : SV_VertexID;
	//	UNITY_VERTEX_INPUT_INSTANCE_ID
	//};

	//struct Varyings
	//{
	//	float4 positionCS : SV_POSITION;
	//	float2 texcoord   : TEXCOORD0;
	//	UNITY_VERTEX_OUTPUT_STEREO
	//};



	//// List of properties to control your post process effect
	float _Intensity;
	//TEXTURE2D_X(_InputTexture);
	TEXTURE2D(_InputTexture);
	SAMPLER(sampler_InputTexture);
	float scalerMask = 1;

	float4 CustomPostProcess(Varyings input) : SV_Target
	{
		//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 positionSS = input.uv * scalerMask * _ScreenSize.xy;
		//float4 outColor = LOAD_TEXTURE2D_X(_MainTex, positionSS);

		//float4 outColor = LOAD_TEXTURE2D(_InputTexture, input.uv);
		//float4 outColor = SAMPLE_TEXTURE2D(_InputTexture, sampler_InputTexture, input.uv.xy* 1);
		float4 outColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv.xy * 1);
		//return float4(1, 0, 0, 1);


		//half4 tmpColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv.xy);
		//return tmpColor;

		return outColor;// float4(lerp(outColor, Luminance(outColor).xxx, _Intensity), 1);
	}


		/////////////////////// WATER MASK ///////////////////////

	float _CamXRot;
	float waterHeight = 0;

	//TEXTURE2D_X(_InputTexture);
	float4 _InputTexture_TexelSize;

	float4 waterMaskFrag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 positionSS = i.uv * _ScreenSize.xy;
		float4 mainTex = LOAD_TEXTURE2D_X(_InputTexture, positionSS);

		////////////////// MASK		

		//v4.4
		//half4 maskFRONT = tex2D(waterFrontRender, i.uv + float2(0, -0.015));

		//v4.4
		//half4 mainTex = tex2D(_MainTex, i.uv);
		//float4 _MainTex_TexelSize = _InputTexture_TexelSize;

		half4 newTex = float4(0, 0, 0, 0);
		half4 mainTexUP = float4(0, 0, 0, 0);
		bool whiteFound = false;

		[loop]
		for (float j = 0; j < 1 * _ScreenSize.y; j = j + _InputTexture_TexelSize.y* _ScreenSize.y) {
			if (j > positionSS.y) {
				mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x, j)); //SAMPLE PIXELS ABOVE
				if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
					newTex = float4(0, 1, 0, 1);
					whiteFound = true;
					//break;
				}
				//float offsetLR = 8*_InputTexture_TexelSize.x* _ScreenSize.x;
				//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x + offsetLR, j));
				//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
				//	newTex = float4(0, 1, 0, 1);
				//	whiteFound = true;
				//	//break;
				//}
				//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x - offsetLR, j));
				//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
				//	newTex = float4(0, 1, 0, 1);
				//	whiteFound = true;
				//	//break;
				//}
			}
			if (j < positionSS.y) {
				mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x, j)); //SAMPLE PIXELS ABOVE
				if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
					whiteFound = true;
					//break;
				}
				//float offsetLR = 8* _InputTexture_TexelSize.x* _ScreenSize.x;
				//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x + offsetLR, j));
				//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
				//	//newTex = float4(0, 1, 0, 1);
				//	whiteFound = true;
				//	//break;
				//}
				//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x - offsetLR, j));
				//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
				//	//newTex = float4(0, 1, 0, 1);
				//	whiteFound = true;
				//	//break;
				//}
			}
		}
		//float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
		//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;

		if (!whiteFound) {
			bool FoundLeftUP = false;
			bool  FoundRightUP = false;
			int foundDOWN = 0;
			[loop]
			for (float j = 0; j < 1 * _ScreenSize.x; j = j + _InputTexture_TexelSize.x* _ScreenSize.x) {
				mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(j, 0.05));	//SAMPLE UPPER LINE	
				if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
					if (j > positionSS.x) {
						FoundRightUP = true; foundDOWN = -1; break;
					}
					if (j < positionSS.x) {
						FoundLeftUP = true; foundDOWN = -1; break;
					}
				}
				mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(j, 0.95 * _ScreenSize.y)); //SAMPLE LOWER LINE		
				if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
					if (j > positionSS.x) {
						FoundRightUP = true; foundDOWN = 1; break;
					}
					if (j < positionSS.x) {
						FoundLeftUP = true; foundDOWN = 1; break;
					}
				}
			}

			if (FoundLeftUP) {
				if (foundDOWN > 0)//if (waveHeightSampler != null && middleDist > 0) //if we are above water, assume white line is below us, so make it all black
				{
					newTex = float4(0, 1, 0, 1);//float4(1, 0, 0, 1);
				}
				else if (foundDOWN < 0) {
					newTex = float4(0, 0, 0, 1);
				}
			}
			if (FoundRightUP) {
				if (foundDOWN > 0)//if (waveHeightSampler != null && middleDist > 0) //if we are above water, assume white line is below us, so make it all black
				{
					newTex = float4(0, 1, 0, 1);//float4(0, 0, 1, 1);
				}
				else if (foundDOWN < 0) {
					newTex = float4(0, 0, 0, 1);
				}
			}

			float _toWaveHeight = _WorldSpaceCameraPos.y - waterHeight;// _Intensity * 20 - 10;

			if (!FoundLeftUP) {
				if (!FoundRightUP) {
					if (_toWaveHeight > 0) {
						//newTex = float4(0, 0, 0, 1);
					}
					if (_toWaveHeight < 0) {//  && _CamXRot > 0) {

						newTex = float4(0,1,0, 1);
					}
				}
			}
		}

		//if (maskFRONT.r > 0) { //if front is white
		///	newTex = float4(0, 0, 0, 1); //make pixel black
		//}

		return half4(newTex.rgb, newTex.a);
		//return half4( mainTex.rgb, newTex.a);
		//return half4(saturate(newTex.rgb) + newTex.rgb * 0.0  + mainTex.rgb, newTex.a);

		///////////////// END MASK

		//return float4(lerp(outColor, Luminance(outColor).xxx, _Intensity), 1);
	}

		/////////////////////// END WATER MASK



		///////////// WATER AIR END







		ENDHLSL

		//		SubShader
		//	{
		//		//Cull Off ZWrite Off ZTest Always
		//
		//			Pass
		//		{
		//			HLSLPROGRAM
		//
		//#pragma vertex VertDefault
		//#pragma fragment Frag
		//
		//			ENDHLSL
		//		}
		//	}
		Subshader {
		//Tags{ "RenderType" = "Opaque" }
			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment fragScreenOLD //fragScreen

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert_radial
#pragma fragment frag_radial

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment frag_depth

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment frag_nodepth

			ENDHLSL
		}

			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment fragAdd

			ENDHLSL
		}


			//PASS5
			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment FragGrey

			ENDHLSL
		}



			//PASS6  // .6
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment CustomPostProcess

			ENDHLSL
		}
			//PASS6  // .7 - WATER MASK
			Pass{
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment waterMaskFrag

				ENDHLSL
			}
			//////////////////////////
			//v4.3
			Pass{ // 8 - blend with water interface mask
				HLSLPROGRAM//CGPROGRAM
				#pragma vertex Vert
				#pragma fragment FragmentProgram

				//v1.3
				sampler2D _InputTextureB;
		//TEXTURE2D_X(_InputTextureB);
		float _InputTextureBPower;

		//v4.8
		half4 FragmentProgram(v2fA i) : SV_Target{//half4 FragmentProgram(VaryingsA i) : SV_Target{

			    float2 _ScreenSize_xy = float2(1,1);

				//uint2 positionSS = i.uv * _ScreenSize.xy * 1;
				//uint2 positionSSS = i.uv * _ScreenSize.xy * scalerMask;
				float2 positionSS = i.uv * _ScreenSize_xy * 1;
				float2 positionSSS = i.uv * _ScreenSize_xy * scalerMask;
				float4 c = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, positionSS);
				float4 mask = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS);

				float enchanceBorder = 0;

				float2 blurUV = positionSSS; //taps0
				if (mask.g == 0) {
					half4 colorM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV); //v4.6	

					float distM = 0.003 * 1;
					half4 color1M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, -distM)* _ScreenSize_xy);
					half4 color2M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, -distM)* _ScreenSize_xy);
					half4 color3M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, distM)* _ScreenSize_xy);
					half4 color4M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, distM)*_ScreenSize_xy);
					half4 color1aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, distM)* _ScreenSize_xy);
					half4 color2aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, -distM)* _ScreenSize_xy);
					half4 color3aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, 0)* _ScreenSize_xy);
					half4 color4aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, 0)* _ScreenSize_xy);

					colorM += color1M + color2M + color3M + color4M;
					colorM /= 4;
					colorM += color1aM + color2aM + color3aM + color4aM;
					colorM /= 4;

					distM = 0.001 * 1;
					color1M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, -distM)* _ScreenSize_xy);
					color2M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, -distM)* _ScreenSize_xy);
					color3M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, distM)* _ScreenSize_xy);
					color4M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, distM)* _ScreenSize_xy);
					color1aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, distM)* _ScreenSize_xy);
					color2aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, -distM)* _ScreenSize_xy);
					color3aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, 0)* _ScreenSize_xy);
					color4aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, 0)* _ScreenSize_xy);
					colorM += color1M + color2M + color3M + color4M;
					colorM /= 4;
					colorM += color1aM + color2aM + color3aM + color4aM;
					colorM /= 4;
					mask.g = colorM.g;
				}

				//v4.6
				float blurWidth = 0.002;
				half4 maskUP1 = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS + float2(0, blurWidth)* _ScreenSize_xy.y * scalerMask);
				half4 maskUP2 = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS + float2(0, -blurWidth)* _ScreenSize_xy.y * scalerMask);

				float2 uv = i.uv * _ScreenSize_xy * 1;
				/////////////// REFRACTION LINE
				float2 addRefract = float2(_refractLineXDisp, _refractLineWidth) * 0.025; //v4.6b
				float rightBreakPoint = 0.84;
				if (uv.x > rightBreakPoint) {
					float dist = uv.x - rightBreakPoint;
					addRefract.x = addRefract.x * (1 - ((dist) / (1 - rightBreakPoint)));
				}
				addRefract.x = addRefract.x * 0.45;
				float upperBreakPoint = 0.75 + _refractLineWidth;//v5.0
				if (uv.y > upperBreakPoint) {
					float dist = uv.y - upperBreakPoint;
					addRefract.y = addRefract.y * (1 - ((dist) / (1 - upperBreakPoint)));
				}
				//v4.6
				float2 taps0 = (i.taps[0] * _ScreenSize_xy + addRefract - float2(_refractLineXDispA,0)* _ScreenSize_xy.x) * 1;
				float2 taps1 = (i.taps[1] * _ScreenSize_xy + addRefract) * 1;
				float2 taps2 = (i.taps[2] * _ScreenSize_xy + addRefract) * 1;
				float2 taps3 = (i.taps[3] * _ScreenSize_xy + addRefract) * 1;

				//FIX BLACK WHEN GO ABOVE TEXTURE in refraction
				if (taps0.y >= 0.99 * _ScreenSize_xy.y) { //v0.1 - 0.93 before
					taps0.y = 0.99 * _ScreenSize_xy.y;
				}

				float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, taps0);

				half4 maskDISP = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, uv + float2(0, _BumpLineHeight) * _ScreenSize_xy);
				if (maskDISP.g == 0) {  //if (maskDISP.r == 0) { 
					_BumpMagnitude = _BumpMagnitudeRL;
					_BumpScale = _BumpScaleRL;
					//v4.6b - gradient refract 
					float gradDiff = uv.y - (mask.g) + _BumpLineHeight;
					float _BumpGradIntensity = 1;
					_BumpMagnitude = _BumpMagnitude * pow(gradDiff, _BumpGradIntensity) * 0.11 * 115;
				}


				float2 bumpUVs = i.uv * _BumpScale
				+ float2(_BumpVelocity.x*abs((_Time.y*_BumpVelocity.z + 2.4)), _BumpVelocity.y*abs((_Time.y*_BumpVelocity.w + 1.4))) * _ScreenSize_xy;
				half3 bump = PerPixelNormal(_BumpTex, float4(bumpUVs.x, bumpUVs.y, 0, 0), half3(0, 1, 0), 1);
				half2 distortion = UnpackNormal(float4(bump,1)).rg;
				taps0.xy += distortion * _BumpMagnitude *0.4;
				half4 colDISTORT = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, taps0);

				if (mask.r > 0) {
					mask.r = 1;
				}
				if (mask.g > 0) {
					mask.g = 1;
				}
				if (mask.b > 0) {
					mask.b = 1;
				}

				float toggledUV = uv.y;// pow(uv.y, 0.8);
				if (_underwaterDepthFade >= 10 && _underwaterDepthFade <= 20) {
					_underwaterDepthFade = -5 + _underwaterDepthFade - 10;
					toggledUV = 1 - uv.y;// pow(1 - uv.y, 0.8);
				}

				half3 resultB = c.rgb *(1 - mask.g) + colDISTORT.rgb * mask.g * (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), toggledUV * _underwaterDepthFade));
				
				half3 result = c.rgb* (1 - mask.g)
					+ colDISTORT * (mask.g)* (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), toggledUV * _underwaterDepthFade) + 0.5)	;

				half4 color = half4(result.rgb,1);



				//colDISTORT = LOAD_TEXTURE2D_X(_MainTex, i.uv * _ScreenSize.xy * 1);
				//return colDISTORT;
				
				//positionSSS = i.uv * 1 * scalerMask;
				//mask = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, i.uv.xy);
				//mask =  SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS);
				//return mask;
				//mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, positionSSS);
				//return mask;



				return half4(result, c.a);
		}
		ENDHLSL//ENDCG
	}//END PASS 8


	//v4.7
	///////////// PASS FOG
	Pass{ //6 pass in URP - PASS 9 in HDRP
		ZTest Always Cull Off ZWrite Off

		HLSLPROGRAM

		#pragma vertex Vertex
		#pragma fragment Fragment

		ENDHLSL
	}
			///////////// END PASS FOG



				//v4.3
				Pass{ // 10 - reset mask
					HLSLPROGRAM//CGPROGRAM
					#pragma vertex Vert
					#pragma fragment FragmentProgram

					//v4.8
					half4 FragmentProgram(v2fA i) : SV_Target//half4 FragmentProgram(VaryingsA i) : SV_Target
					{
							return half4(1,1,1,1);
					}
					ENDHLSL//ENDCG
			}//END PASS 10

			//v4.3  //v1.2
			Pass{ // 11 - enable external mask
							HLSLPROGRAM//CGPROGRAM
							#pragma vertex Vert
							#pragma fragment FragmentProgram

							//float scalerMask = 1;

							//TEXTURE2D_X(_InputTextureA);
							//SAMPLER(sampler_InputTextureA);
							sampler2D _InputTextureA;




					//v4.8
					half4 FragmentProgram(v2fA i) : SV_Target//half4 FragmentProgram(VaryingsA i) : SV_Target
							{
								UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

							//uint2 positionSS = i.uv * 5;
							float4 mainTexA = tex2D(_InputTextureA, float2(i.uv.x, i.uv.y));
							float4 mask = mainTexA;

							float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));

							if (mask.r > 0.001 || _Height < -6 || (dot(forward, float3(0, -1, 0)) > 0.5) ) { //v1.2a//if (mask.r > 0.001 || _Height < -15) { //v1.2a
								mask = float4(1, 1, 1, 1);
							}
							else {
								mask = float4(0, 0, 0, 0);
							}

							return	saturate(mask);// float4(1, 1, 1, 1);// (mainTex);
						}
						ENDHLSL//ENDCG
				}//END PASS 11

		////////////////////////////

		///////////////////////////////////////// PASS 12 /////////////////////////////////////////
		//// GRAPH
		Pass //6 BLIT
			{
				Name "ColorBlitPass"
				HLSLPROGRAM
					// Core.hlsl includes URP basic variables needed for any shader. The Blit.hlsl provides a
					//Vert and Fragment function that abstracts platform differences when handling a full screen shader pass.
					//It also declares a _BlitTex texture that is bound by the Blitter API.
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
					//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
					// This is a simple read shader so we use the default provided Vert and FragNearest
					//functions. If you would like to do a bilinear sample you could use the FragBilinear functions instead.
					#include "BlitOCEANIS.hlsl"//v0.2

					#pragma vertex VertABC
					#pragma fragment FragNearest
				ENDHLSL
		}
		///////////////////////////////////////// PASS 13 /////////////////////////////////////////
		Pass //7 BLIT BACKGROUND
		{
				Name "ColorBlitPasss"
				HLSLPROGRAM
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
				#include "BlitOCEANIS.hlsl"//v0.2
				#pragma vertex VertABC
				#pragma fragment Frag
				//
				float4 Frag(VaryingsB input) : SV_Target0
				{
					
					return half4(1, 1, 1, 0.5);
					// this is needed so we account XR platform differences in how they handle texture arrays
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
					// sample the texture using the SAMPLE_TEXTURE2D_X_LOD
					float2 uv = input.texcoord.xy;
					half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);
					// Inverts the sampled color
					//return half4(1, 1, 1, 1) - color;
					return color*1;
				}
				ENDHLSL
		}


		///////////////////////////////////////// PASS 14 /////////////////////////////////////////
			//PASS 0-8
		//Tags{ "RenderType" = "Opaque" }
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "BlitOCEANIS.hlsl"//v0.2
			#pragma vertex VertABC
			#pragma fragment fragScreenA

			half4 fragScreenA(VaryingsB i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					//half4 colorA = tex2D(_MainTex, i.uv.xy);
					half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord.xy); // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				#if !UNITY_UV_STARTS_AT_TOP
																						 ///half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
					half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.texcoord.xy);//v0.2 //i.uv1.xy);//v0.2
				#else
																						 //half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
					half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.texcoord.xy);//v1.1
				#endif
					half4 depthMask = saturate(colorB * _SunColor);
					return  1.0f - (1.0f - colorA) * (1.0f - depthMask);//colorA * 5.6;// 1.0f - (1.0f - colorA) * (1.0f - depthMask);
			}

			ENDHLSL
		}
		///////////////////////////////////////// PASS 15 /////////////////////////////////////////
		//PASS 1-9
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			#include "BlitOCEANIS.hlsl"//v0.2
			//#pragma vertex Vert
			#pragma vertex vert_radialA
			#pragma fragment frag_radialA

				struct v2f_radialA {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 blurVector : TEXCOORD1;
				};

				struct VaryingsBA
				{
					float4 positionCS : SV_POSITION;
					float2 texcoord   : TEXCOORD0;
					float2 blurVector : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				VaryingsBA vert_radialA(AttributesB input)
				{
					VaryingsBA output;
					UNITY_SETUP_INSTANCE_ID(input);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				#if SHADER_API_GLES
					float4 pos = input.positionOS;
					float2 uv  = input.uv;
				#else
					float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
					float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
				#endif

					output.positionCS = pos;
					output.texcoord   = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;

					output.blurVector = (_SunPosition.xy - output.texcoord.xy) * _BlurRadius4.xy;

					return output;
				}
				
					/*
			v2f_radialA vert_radialA(Attributes v) {

				v2f_radialA o = (v2f_radialA)0;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
				o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
				float2 uv = v.uv;

				#if !UNITY_UV_STARTS_AT_TOP
						//uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
				#endif

				o.uv.xy = uv;
				o.blurVector = (_SunPosition.xy - uv.xy) * _BlurRadius4.xy;

				return o;
			}
			*/

			TEXTURE2D(_MainTexA);
			SAMPLER(sampler_MainTexA);
			#define SAMPLES_FLOATA 29.0f
			#define SAMPLES_INTA 29

			//v0.1
			sampler2D OceanisWaterMaskGlob;

			half4 frag_radialA(VaryingsBA i) : SV_Target//half4 frag_radialA(v2f_radialA i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half4 color = half4(0,0,0,0);

				//return SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.texcoord.xy);
				//return float4(1*i.texcoord.y,0,0,1)*i.blurVector.r;

				for (int j = 0; j < SAMPLES_INTA; j++)
				{
					half4 tmpColor = SAMPLE_TEXTURE2D(_MainTexA, sampler_MainTexA, i.texcoord.xy);
					//color += tmpColor;

					//v0.1
					//sampler2D OceanisWaterMaskGlob;
					float4 mask = tex2D(OceanisWaterMaskGlob, float2(i.texcoord.x, i.texcoord.y));
					float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
					if (mask.r > 0.001 || _Height < -6 || (dot(forward, float3(0, -1, 0)) > 0.5)) { //v1.2a//if (mask.r > 0.001 || _Height < -15) { //v1.2a
						color += tmpColor;
					}

					i.texcoord.xy += i.blurVector;
				}
				return color / SAMPLES_FLOATA;
			}
			ENDHLSL
		}
		///////////////////////////////////////// PASS 16 /////////////////////////////////////////
		//PASS 2-10
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			#include "BlitOCEANIS.hlsl"//v0.2
			#pragma vertex VertABC
			//#pragma vertex vert
			#pragma fragment frag_depthA

			half4 frag_depthA(VaryingsB i) : SV_Target{

					//return float4(0,0,0,0);

					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					#if !UNITY_UV_STARTS_AT_TOP
							//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv1.xy);
							float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord.xy), _ZBufferParams); //v0.1 URP i.uv1.xy
					#else
							//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
							float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord.xy), _ZBufferParams);
					#endif
					//return depthSample;

					//half4 tex = tex2D(_MainTex, i.uv.xy);
					half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord.xy);
					//depthSample = Linear01Depth(depthSample, _ZBufferParams);
					//return tex;

					// consider maximum radius
					#if !UNITY_UV_STARTS_AT_TOP
						half2 vec = _SunPosition.xy - i.texcoord.xy; //i.uv1.xy;
					#else
						half2 vec = _SunPosition.xy - i.texcoord.xy;
					#endif
					half dist = saturate(_SunPosition.w - length(vec.xy));

					half4 outColor = 0;

					if (depthSample > 1- 0.018) {//if (depthSample < 0.018) {
						//outColor = TransformColor(tex) * dist;
					}

					#if !UNITY_UV_STARTS_AT_TOP
							if (depthSample < 0.018) {
								outColor = TransformColor(tex) * dist;
							}
					#else
							if (depthSample > 1 - 0.018) {
								outColor = TransformColor(tex) * dist;
							}
					#endif
						return outColor * 1;
				}

			ENDHLSL
		}
		///////////////////////////////////////// PASS 17 /////////////////////////////////////////
		//PASS 3-11
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			#include "BlitOCEANIS.hlsl"//v0.2
			#pragma vertex VertABC
			//#pragma vertex vert
			#pragma fragment frag_nodepthA
			

			half4 frag_nodepthA(VaryingsB i) : SV_Target{

					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					#if !UNITY_UV_STARTS_AT_TOP
							//float4 sky = (tex2D(_Skybox, i.uv1.xy));
							float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.texcoord.xy);
					#else
							//float4 sky = (tex2D(_Skybox, i.uv.xy));
							float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.texcoord.xy); //i.uv1.xy;
					#endif

							//float4 tex = (tex2D(_MainTex, i.uv.xy));
					half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord.xy);
							//sky = float4(0.3, 0.05, 0.05,  1);
							/// consider maximum radius
					#if !UNITY_UV_STARTS_AT_TOP
							half2 vec = _SunPosition.xy - i.texcoord.xy;
					#else
							half2 vec = _SunPosition.xy - i.texcoord.xy;//i.uv1.xy;
					#endif
					half dist = saturate(_SunPosition.w - length(vec));

					half4 outColor = 0;

					// find unoccluded sky pixels
					// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded

					if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2) {
						outColor = TransformColor(tex) * dist;
						//outColor = TransformColor(sky) * dist;
					}

					return outColor * 1;
			}

			ENDHLSL
		}
		///////////////////////////////////////// PASS 18 /////////////////////////////////////////
		//PASS 4-12
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "BlitOCEANIS.hlsl"//v0.2
			#pragma vertex VertABC
			#pragma fragment fragAdd

			ENDHLSL
		}

		///////////////////////////////////////// PASS 19 /////////////////////////////////////////
		//PASS 5-13
		Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "BlitOCEANIS.hlsl"//v0.2
			#pragma vertex VertABC
			#pragma fragment FragGrey

			ENDHLSL
		}
		///////////////////////////////////////// PASS 20 /////////////////////////////////////////
			Pass //14 BLIT ONE TO ANOTHER
		{
				Name "ColorBlitPasss"
				HLSLPROGRAM
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
				#include "BlitOCEANIS.hlsl"//v0.2
				#pragma vertex VertABC
				#pragma fragment Frag
				//
				TEXTURE2D(_MainTexA);
				SAMPLER(sampler_MainTexA);

				sampler2D _skyboxOnly;

				float4 Frag(VaryingsB input) : SV_Target0
				{
					//return half4(0.1, 0, 0, 0.5);
					// this is needed so we account XR platform differences in how they handle texture arrays
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
					// sample the texture using the SAMPLE_TEXTURE2D_X_LOD
					float2 uv = input.texcoord.xy;
					half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);
					// Inverts the sampled color
					//return half4(1, 1, 1, 1) - color;

					color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord.xy);
					//color = SAMPLE_TEXTURE2D_X_LOD(_MainTexA, sampler_MainTexA, uv, _BlitMipLevel);
					color = tex2D(_skyboxOnly,input.texcoord.xy);

					return color*1;
				}
				ENDHLSL
		}

		//////////////////////////
			//v4.3
			Pass{ // 8 - blend with water interface mask //////////////////// PASS 21
				HLSLPROGRAM//CGPROGRAM

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
				#include "BlitOCEANIS.hlsl"//v0.2
				#pragma vertex VertA1
				//#pragma vertex Vert
				#pragma fragment FragmentProgram

				struct v2fA1 {
					float4 positionCS : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					//#if UNITY_UV_STARTS_AT_TOP
					float2 uv1 : TEXCOORD1;
					//#else
					//#endif	
					//float2 uv1   : TEXCOORD1;
					float2 bumpUV : TEXCOORD2;
					//v4.6
					half2 taps[4] : TEXCOORD3;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				TEXTURE2D_X(_MainTexB);
				SAMPLER(sampler_MainTexB);
				float4 _MainTexB_TexelSize;

				v2fA1 VertA1(AttributesB v) {
						v2fA1 o = (v2fA1)0;
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						

						#if SHADER_API_GLES
							float4 pos = v.positionOS;
							float2 uv  = v.uv;
						#else
							float4 pos = GetFullScreenTriangleVertexPosition(v.vertexID);
							float2 uv  = GetFullScreenTriangleTexCoord(v.vertexID);
						#endif

						o.positionCS = pos;
						o.texcoord   = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;


				//		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);	
				//		o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
				//		float2 uv = v.uv;		
				//		#if !UNITY_UV_STARTS_AT_TOP
				//				uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);	
				//		#endif
				//				o.uv = uv;

				//		#if !UNITY_UV_STARTS_AT_TOP
				//				o.uv = uv.xy;
				//				if (_MainTex_TexelSize.y < 0)
				//					o.uv.y = 1 - o.uv.y;
				//		#endif	
						//v4.7
						o.bumpUV = o.texcoord * _BumpTex_ST.xy + _BumpTex_ST.zw;
						o.taps[0] = o.texcoord + _MainTexB_TexelSize * 1;// *_ScreenSize.xy;// *scalerMask;
						o.taps[1] = o.texcoord - _MainTexB_TexelSize * 1;// *_ScreenSize.xy;
						o.taps[2] = o.texcoord + _MainTexB_TexelSize * 1 * half2(1, -1);// *_ScreenSize.xy;
						o.taps[3] = o.texcoord - _MainTexB_TexelSize * 1 * half2(1, -1);// *_ScreenSize.xy;
						return o;
				}

				//v1.3
				//sampler2D _InputTextureB;
				//TEXTURE2D_X(_InputTextureB);
				float _InputTextureBPower;


				//sampler2D OceanisWaterMaskGlob;
				sampler2D OceanisMaskBumpGlob;

				//v4.8
				half4 FragmentProgram(v2fA1 i) : SV_Target //half4 FragmentProgram(VaryingsA i) : SV_Target{
				{
						//return tex2D(_BumpTex, i.texcoord);
						//return tex2D(OceanisWaterMaskGlob, i.texcoord);
						//return SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.texcoord);
						//return SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, i.texcoord)*3;
						//return SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, i.texcoord)*3;
						//return float4(1*i.texcoord.y,0,0,1);// SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, i.texcoord);

						float2 _ScreenSize_xy = float2(1,1);
						float2 positionSS = i.texcoord * _ScreenSize_xy * 1;
						float2 positionSSS = i.texcoord * _ScreenSize_xy * scalerMask;
						float4 c = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, positionSS);
						float4 mask = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS);

						float enchanceBorder = 0;
						float2 blurUV = positionSSS; //taps0
						if (mask.g == 0) {
							half4 colorM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV); //v4.6	

							float distM = 0.003 * 1;
							half4 color1M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, -distM)* _ScreenSize_xy);
							half4 color2M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, -distM)* _ScreenSize_xy);
							half4 color3M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, distM)* _ScreenSize_xy);
							half4 color4M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, distM)*_ScreenSize_xy);
							half4 color1aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, distM)* _ScreenSize_xy);
							half4 color2aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, -distM)* _ScreenSize_xy);
							half4 color3aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, 0)* _ScreenSize_xy);
							half4 color4aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, 0)* _ScreenSize_xy);

							colorM += color1M + color2M + color3M + color4M;
							colorM /= 4;
							colorM += color1aM + color2aM + color3aM + color4aM;
							colorM /= 4;

							distM = 0.001 * 1;
							color1M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, -distM)* _ScreenSize_xy);
							color2M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, -distM)* _ScreenSize_xy);
							color3M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, distM)* _ScreenSize_xy);
							color4M = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, distM)* _ScreenSize_xy);
							color1aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, distM)* _ScreenSize_xy);
							color2aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(0, -distM)* _ScreenSize_xy);
							color3aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(distM, 0)* _ScreenSize_xy);
							color4aM = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, blurUV + float2(-distM, 0)* _ScreenSize_xy);
							colorM += color1M + color2M + color3M + color4M;
							colorM /= 4;
							colorM += color1aM + color2aM + color3aM + color4aM;
							colorM /= 4;
							mask.g = colorM.g;
						}

						//v4.6
						float blurWidth = 0.002;
						half4 maskUP1 = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS + float2(0, blurWidth)* _ScreenSize_xy.y * scalerMask);
						half4 maskUP2 = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, positionSSS + float2(0, -blurWidth)* _ScreenSize_xy.y * scalerMask);

						float2 uv = i.texcoord * _ScreenSize_xy * 1;
						/////////////// REFRACTION LINE
						float2 addRefract = float2(_refractLineXDisp, _refractLineWidth) * 0.025; //v4.6b
						float rightBreakPoint = 0.84;
						if (uv.x > rightBreakPoint) {
							float dist = uv.x - rightBreakPoint;
							addRefract.x = addRefract.x * (1 - ((dist) / (1 - rightBreakPoint)));
						}
						addRefract.x = addRefract.x * 0.45;
						float upperBreakPoint = 0.75 + _refractLineWidth;//v5.0
						if (uv.y > upperBreakPoint) {
							float dist = uv.y - upperBreakPoint;
							addRefract.y = addRefract.y * (1 - ((dist) / (1 - upperBreakPoint)));
						}
						//v4.6
						float2 taps0 = (i.taps[0] * _ScreenSize_xy + addRefract - float2(_refractLineXDispA,0)* _ScreenSize_xy.x) * 1;
						float2 taps1 = (i.taps[1] * _ScreenSize_xy + addRefract) * 1;
						float2 taps2 = (i.taps[2] * _ScreenSize_xy + addRefract) * 1;
						float2 taps3 = (i.taps[3] * _ScreenSize_xy + addRefract) * 1;

						//FIX BLACK WHEN GO ABOVE TEXTURE in refraction
						if (taps0.y >= 0.99 * _ScreenSize_xy.y) { //v0.1 - 0.93 before
							taps0.y = 0.99 * _ScreenSize_xy.y;
						}
						float4 col = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, taps0);

						half4 maskDISP = SAMPLE_TEXTURE2D(_WaterInterfaceTex, sampler_WaterInterfaceTex, uv + float2(0, _BumpLineHeight) * _ScreenSize_xy);
						if (maskDISP.g == 0) {  //if (maskDISP.r == 0) { 
							_BumpMagnitude = _BumpMagnitudeRL;
							_BumpScale = _BumpScaleRL;
							//v4.6b - gradient refract 
							float gradDiff = uv.y - (mask.g) + _BumpLineHeight;
							float _BumpGradIntensity = 1;
							_BumpMagnitude = _BumpMagnitude * pow(gradDiff, _BumpGradIntensity) * 0.11 * 115;
						}

						float2 bumpUVs = i.texcoord * _BumpScale
						+ float2(_BumpVelocity.x*abs((_Time.y*_BumpVelocity.z + 2.4)), _BumpVelocity.y*abs((_Time.y*_BumpVelocity.w + 1.4))) * _ScreenSize_xy;
						half3 bump = PerPixelNormal(OceanisMaskBumpGlob, float4(bumpUVs.x, bumpUVs.y, 0, 0), half3(0, 1, 0), 1);
						half2 distortion = UnpackNormal(float4(bump,1)).rg;
						taps0.xy += distortion * _BumpMagnitude *0.4;
						half4 colDISTORT = SAMPLE_TEXTURE2D(_MainTexB, sampler_MainTexB, taps0);

						if (mask.r > 0) {
							mask.r = 1;
						}
						if (mask.g > 0) {
							mask.g = 1;
						}
						if (mask.b > 0) {
							mask.b = 1;
						}

						float toggledUV = uv.y;// pow(uv.y, 0.8);
						if (_underwaterDepthFade >= 10 && _underwaterDepthFade <= 20) {
							_underwaterDepthFade = -5 + _underwaterDepthFade - 10;
							toggledUV = 1 - uv.y;// pow(1 - uv.y, 0.8);
						}

						half3 resultB = c.rgb *(1 - mask.g) + colDISTORT.rgb * mask.g * (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), toggledUV * _underwaterDepthFade));
				
						half3 result = c.rgb* (1 - mask.g)
							+ colDISTORT * (mask.g)* (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), toggledUV * _underwaterDepthFade) + 0.5)	;

						half4 color = half4(result.rgb,1);
						return half4(result, c.a);
				}
				ENDHLSL//ENDCG
			}//END PASS 8 -- PASS 21

			//v4.3  //v1.2
			Pass{ // 11 - enable external mask			 -- PASS 22
					HLSLPROGRAM//CGPROGRAM
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
					//#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
					#include "BlitOCEANIS.hlsl"//v0.2
					#pragma vertex VertABC
					//#pragma vertex Vert
					#pragma fragment FragmentProgramA

					//sampler2D _InputTextureA;

					TEXTURE2D_X(_InputTextureA);
					SAMPLER(sampler_InputTextureA);
					//float4 _InputTextureA_TexelSize;

					sampler2D OceanisWaterMaskGlob;

					//v4.8
					half4 FragmentProgramA(VaryingsB i) : SV_Target//half4 FragmentProgram(VaryingsA i) : SV_Target
					{		
							//return float4(1,0,0,1);

							//return tex2D(OceanisWaterMaskGlob, i.texcoord);

							//return SAMPLE_TEXTURE2D(_InputTextureA, sampler_InputTextureA, i.texcoord)*3;
							//return  tex2D(_InputTextureA, float2(i.texcoord.x, i.texcoord.y));;
							//return float4(1, 0, 1, 0);
							UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
							//uint2 positionSS = i.texcoord * 5;
							//float4 mainTexA =SAMPLE_TEXTURE2D(_InputTextureA, sampler_InputTextureA, i.texcoord);// tex2D(_InputTextureA, float2(i.texcoord.x, i.texcoord.y));
							
							float4 mainTexA = tex2D(OceanisWaterMaskGlob, float2(i.texcoord.x, i.texcoord.y));
							
							float4 mask = mainTexA;

							float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));

							if (mask.r > 0.001 || _Height < -6 || (dot(forward, float3(0, -1, 0)) > 0.5) ) { //v1.2a//if (mask.r > 0.001 || _Height < -15) { //v1.2a
								mask = float4(1, 1, 1, 1);
							}
							else {
								mask = float4(0, 0, 0, 0);
							}
							return	saturate(mask);// float4(1, 1, 1, 1);// (mainTex);
					}
					ENDHLSL//ENDCG
				}//END PASS 11  -- PASS 22

		///// END PASSES
	}
}