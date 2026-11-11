Shader "Lux/Wetness/Puddles Bumped Specular FLOW FOAM SM" {

Properties {
	_Color ("Diffuse Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_SpecTex ("Specular Color (RGB) Roughness (A)", 2D) = "black" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	
	// Special Properties needed by Heightmap based wetness shaders
	_HeightWetness("Heightmap(A) Puddle Noise(R)", 2D) = "white" {}
    _Parallax ("Height", Range (0.005, 0.08)) = 0.02

    // Special Properties needed by wetness shaders using tex2Dlod
    _TextureSize ("Texture Size", Float) = 1024
	_MipBias ("Mip Bias", Float) = 0.75


	//FLOW
	// Special Properties needed by Flow based wetness shaders
    _WaterBumpMap ("Water Normalmap", 2D) = "bump" {}
    _WaterBumpScale ("Water Normalmap Scale", Float) = 1

	_FlowSpeed ("Water Flow Speed", Float) = 1
	_FlowHeightScale ("Water Flow Height Scale", Float) = 1
	_FlowRefraction ("Water Flow Refraction", Float) = 0.01
	//MY FLOWER
	_WaterFlowRT ("Water Flow Real Time", 2D) = "white" {}
	_WaterCaustics ("Water Flow Caustics", 2D) = "white" {}
	_FlowHeight ("Water Flow Heigth", Float) = 0

	// Special Properties needed by all wetness shaders
	_WetnessWorldNormalDamp ("Wetness WorldNormal Damp", Range(0,1)) = 0.5
	_WetnessHeightMapInfluence  ("Wetness HeightMap Influence", Range(0,1)) = 1

	// General Lux Properties
	_DiffCubeIBL ("Custom Diffuse Cube", Cube) = "black" {}
	_SpecCubeIBL ("Custom Specular Cube", Cube) = "black" {}

	// Properties needed if you used Boxprojection
	// _CubemapSize ("Cube Size", Vector) = (1,1,1,0)

	// _Shininess property is needed by the lightmapper - otherwise it throws errors
	[HideInInspector] _Shininess ("Shininess (only for Lightmapper)", Float) = 0.5
	[HideInInspector] _AO ("Ambient Occlusion Alpha (A)", 2D) = "white" {}


	_Lux_RainRipples ("_Lux_RainRipples", 2D) = "white" {}
	_Lux_RippleWindSpeed ("_Lux_RippleWindSpeed", Float) = (0,0,0,0) 

	_Lux_WaterFloodlevel ("_Lux_WaterFloodlevel", Float) = (0,0,0,0)

	_Lux_RainIntensity ("_Lux_RainIntensity", Float) = 0.75
	_Lux_RippleAnimSpeed ("_Lux_RippleAnimSpeed", Float) = 0.75
	_Lux_RippleTiling ("_Lux_RippleTiling", Float) = 0.75
	_Lux_WaterBumpDistance ("_Lux_WaterBumpDistance", Float) = 0.75

	//FOAM
	_FoamTex ("_FoamTex", 2D) = "white" {}

	//CAUSTICS
	_ViewDirection ("_ViewDirection", Vector) = (0, 0, 1, 0)
	Amplitude ("Amplitude", Vector) = (0.7, 0.5, 0,0)
	RefractionAmount ("RefractionAmount", Float) = 0.8
	Ydistance ("Ydistance", Float) = 0.1

}

SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
	CGPROGRAM
	#pragma surface surf LuxDirect vertex:LuxVertWetFlow fullforwardshadows noambient
	#pragma glsl
	#pragma target 3.0


	// #pragma debug

	#pragma multi_compile LUX_LIGHTING_BP LUX_LIGHTING_CT
	#pragma multi_compile LUX_LINEAR LUX_GAMMA
	#pragma multi_compile DIFFCUBE_ON DIFFCUBE_OFF
	#pragma multi_compile SPECCUBE_ON SPECCUBE_OFF
	#pragma multi_compile LUX_AO_OFF LUX_AO_ON

	//#define LUX_LIGHTING_BP
	//#define LUX_LINEAR
	//#define DIFFCUBE_ON
	//#define SPECCUBE_ON
//	#define LUX_AO_ON

//	///////////////////////////////////////
//	Config Wetness
	#define WetnessMaskInputVertexColors // PuddleMask is stored in vertex.color.g / comment this if you want to use a texture instead
    
    #ifdef WetnessMaskInputVertexColors
        #define PuddleMask IN.color.g
        #define WetMask IN.color.b
    #else
        // If You do not want to use vertex colors to define wet vs. dry or puddles you could use a texture instead:
        // Do not forget that you have to declare the texture using sampler2D and sample it in the surface function
        // In this example we take both values from the ambient occlusion texture
        #define PuddleMask ambientOcclusion.g
        #define WetMask ambientOcclusion.b
    #endif

    //	Use this define to enable water ripples (needed by LuxWetness.cginc )
    //#define Lux_WaterRipples

    //FLOW
    #define Lux_WaterFlow
//	///////////////////////////////////////	



	//REPLACE INCLUDES

	// Material specific Wetness Properties
float _WetnessWorldNormalDamp;
float _WetnessHeightMapInfluence;


// Properties needed by heightmap based wetness shaders
#if defined (Lux_WaterRipples) || defined (Lux_WaterFlow)
    sampler2D _HeightWetness;
    float4 _MainTex_ST;
#endif
#if defined (LUX_AO_ON) || !defined (WetnessMaskInputVertexColors)
    #if defined (Lux_WaterRipples) || defined (Lux_WaterFlow)
        float4 _AO_ST;
    #endif
#endif

// Properties only needed by flow based wetness shaders
#ifdef Lux_WaterFlow

	//FLOW
	sampler2D _WaterFlowRT;
	sampler2D _WaterCaustics;
	float _FlowHeight;

    sampler2D _WaterBumpMap;
    float _WaterBumpScale;
    float _FlowSpeed;
    float _FlowHeightScale;
    float _FlowRefraction;
#endif


// Global Rain Properties passed in by Script
float2 _Lux_WaterFloodlevel;
float _Lux_RainIntensity;
sampler2D _Lux_RainRipples;
float4 _Lux_RippleWindSpeed;
float _Lux_RippleAnimSpeed;
float _Lux_RippleTiling;
float _Lux_WaterBumpDistance;



//	Based on the work of Sébastien Lagarde
//  http://seblagarde.wordpress.com/2013/04/14/water-drop-3b-physically-based-wet-surfaces/


 float2 ComputeRipple(float4 UV, float CurrentTime, float Weight)
{
    float4 Ripple = tex2Dlod(_Lux_RainRipples, UV);
    // We use multi sampling here in order to improve Sharpness due to the lack of Anisotropic Filtering when using tex2Dlod
    Ripple += tex2Dlod(_Lux_RainRipples, float4(UV.xy, UV.zw * 0.5) );
    Ripple *= 0.5;

    Ripple.yz = Ripple.yz * 2 - 1; // Decompress Normal
    float DropFrac = frac(Ripple.w + CurrentTime); // Apply time shift
    float TimeFrac = DropFrac - 1.0f + Ripple.x;
    float DropFactor = saturate(0.2f + Weight * 0.8f - DropFrac);
    float FinalFactor = DropFactor * Ripple.x * sin( clamp(TimeFrac * 9.0f, 0.0f, 3.0f) * 3.141592653589793);
    return Ripple.yz * FinalFactor * 0.35f;
}

//	Accumulate water in cracks(x) and puddles(y)
 float2 ComputeWaterAccumulation(float i_puddlemask, float2 i_heightmask, float i_worlnormalface_y )
{
	float2 AccumulatedWaters;
	// Get the "size" of the accumulated water in cracks
    AccumulatedWaters.x = min(_Lux_WaterFloodlevel.x, 1.0 - i_heightmask.x * _WetnessHeightMapInfluence ); // = Heightmap);
	// Get the size of the accumlated water in puddles taking into account the marging (0.4 constant here).
    // AccumulatedWaters.y = saturate((_Wetness.y - PuddleMask ) / 0.4);
    // First we shrink the Puddle by i_worlnormalface_y
    // In order to improve the edges we also take the Heightmap and PuddleNoise into account.
    AccumulatedWaters.y = saturate((_Lux_WaterFloodlevel.y * i_worlnormalface_y  - i_puddlemask - i_heightmask.x - i_heightmask.y * (1-_Lux_WaterFloodlevel.y) ) / 0.4)  ;
    // Damp overall WaterAccumulation according to the worldNormal.y Component
    float worldNormalDamp = saturate( saturate(i_worlnormalface_y) + _WetnessWorldNormalDamp);    
	// return float2( max(AccumulatedWaters.x, AccumulatedWaters.y) );
    return float2(max(AccumulatedWaters.x, AccumulatedWaters.y), max(AccumulatedWaters.x, AccumulatedWaters.y * 2) * 0.5) * worldNormalDamp.xx;
}

//	Add animated Ripples to areas where the Water Accumulation is high enough
//  Returns the tweaked and adjusted Ripple Normal
 float3 AddWaterRipples(float2 i_wetFactor, float3 i_worldPos, float2 lambda, float fadeOutWaterBumps )
{
    float4 Weights = _Lux_RainIntensity - float4(0, 0.25, 0.5, 0.75);
    Weights = saturate(Weights * 4);
    float animSpeed =  _Time.y * _Lux_RippleAnimSpeed;
    float2 Ripple1 = ComputeRipple( float4(i_worldPos.xz * _Lux_RippleTiling + float2( 0.25f,0.0f) + _Lux_RippleWindSpeed.xy * _Time.y, lambda ), animSpeed, Weights.x);
    float2 Ripple2 = ComputeRipple( float4(i_worldPos.xz * _Lux_RippleTiling + float2(-0.55f,0.3f) + _Lux_RippleWindSpeed.zw * _Time.y, lambda ), animSpeed * 0.71, Weights.y); 
    float3 rippleNormal = float3( Weights.x * Ripple1.xy + Weights.y * Ripple2.xy, 1);
    // Blend and fade out Ripples
    return lerp( float3(0,0,1), rippleNormal, i_wetFactor.y * i_wetFactor.y * fadeOutWaterBumps );
}


//  Water flow specific functions
#ifdef Lux_WaterFlow
    // Add water flow based on slope

    // http://www.heathershrewsbury.com/dreu2010/wp-content/uploads/2010/07/FlowVisualizationUsingMovingTextures.pdf
    // During the animation of the texture advection, this distortion continues to build up, so that eventually the visualization will become useless.
    // Therefore we periodically restart the texture coordinates back at their original positions in the regular grid. --> frac
    // To avoid the sudden jump this would cause in the animation, we gradually fade up the new texture and fade down the old one

     fixed3 AddWaterFlow(float2 MainUV, float2 flowDirection, float worlNormalY, float overallWetness, float2 lambda, float fadeOutWaterBumps)
    {
        float2 flowUV = MainUV * _WaterBumpScale;
        float2 fracTime = frac(float2(_Time.y, _Time.y + 0.5));
        float fade = abs(fracTime.x * 2 - 1);
        flowDirection *= _FlowSpeed;
        float4 flowBump = tex2Dlod(_WaterBumpMap, float4(flowUV + fracTime.xx * flowDirection, lambda)  );
        flowBump = lerp(flowBump, tex2Dlod(_WaterBumpMap, float4(flowUV + fracTime.yy * flowDirection, lambda) ), fade);
        fixed3 finalflowBump = UnpackNormal(flowBump);


       

        // Mask water flow according to overallWetness and scale down flow normal according to speed (worldNormalFace.y)
        worlNormalY = saturate(worlNormalY);
        return lerp(float3(0,0,1), finalflowBump, (1 - worlNormalY * worlNormalY) * overallWetness ) * float3( _FlowHeightScale, _FlowHeightScale, 1) * fadeOutWaterBumps;
    } 

    //  Add Water Ripples to Waterflow
     float3 AddWaterFlowRipples(float2 i_wetFactor, float3 i_worldPos, float2 lambda, float i_worldNormalFaceY, float fadeOutWaterBumps)
    {
        float4 Weights = _Lux_RainIntensity - float4(0, 0.25, 0.5, 0.75);
        Weights = saturate(Weights * 4);
        float animSpeed =  _Time.y * _Lux_RippleAnimSpeed;
        float2 Ripple1 = ComputeRipple( float4(i_worldPos.xz * _Lux_RippleTiling + float2( 0.25f,0.0f), lambda ), animSpeed, Weights.x);
        float2 Ripple2 = ComputeRipple( float4(i_worldPos.xz * _Lux_RippleTiling + float2(-0.55f,0.3f), lambda ), animSpeed * 0.71, Weights.y); 
        float3 rippleNormal = float3( Weights.x * Ripple1.xy + Weights.y * Ripple2.xy, 1);
        // Blend and fade out Ripples
        return lerp( float3(0,0,1), rippleNormal, i_wetFactor.y * i_wetFactor.y * fadeOutWaterBumps * i_worldNormalFaceY*i_worldNormalFaceY );
    } 
#endif

//	Tweak all BRDF Inputs according to calculated Wetness/Porosity
 fixed WaterBRDF (inout fixed3 Albedo, fixed Specular, fixed3 SpecularColor, fixed wetFactor) {
	float porosity = saturate((( (1 - Specular) - 0.5)) / 0.4 );
    // Materials (like metal) which are not porose should not be darkened, so we have to find the metal parts:
    // As metals have high specular color values (>0.33) we can use SpecularColor to identify those
    float metalness = saturate((dot(SpecularColor, 0.33) * 1000 - 500) );
    float factor = lerp(1, 0.2, (1 - metalness) * porosity);
	// Attenuate Albedo
	Albedo *= lerp(1.0, factor, wetFactor);	
	// Tweak SpecularColor towards Water / Water F0 specular is 0.02 (based on IOR of 1.33)
	SpecularColor = lerp(SpecularColor, 0.02, wetFactor);
	// Calc Roughness
	return lerp(1.0, Specular, lerp(1.0, factor, wetFactor) );
}

	//END REPLACE INCLUDES






	
	// include should be called after all defines
	//#include "../LuxCore/LuxLightingDirect.cginc"


	//REPLACE LIGHTING INCLUDES

	// Lux Lighting Functions

#define OneOnLN2_x6 8.656170
#define Pi 3.14159265358979323846

struct SurfaceOutputLux {
	half3 Albedo;
	half3 Normal;
	half3 Emission;
	half Specular;
	half3 SpecularColor;
	half Alpha;
	half DeferredFresnel;
};


// for Cook Torrence spec or roughness has to be in linear space
half LuxAdjustSpecular(half spec) {
	#if defined(LUX_LIGHTING_CT)
		return clamp(pow(spec, 1/2.2), 0.0, 0.996);
	#else
		return spec;
	#endif
}


/////////////////////////////// deferred lighting / uses faked fresnel

inline fixed4 LightingLuxDirect_PrePass (SurfaceOutputLux s, half4 light)
{
	// light.a is "compressed" to fit into the 0-1 range using log2(x + 1) which is the best compromise i have found
	fixed spec = exp2(light.a) - 1;
	fixed4 c;
//	Diffuse
	c.rgb = s.Albedo * light.rgb;
//	Specular
	//s.DeferredFresnel based on dot N V (faked fresnel as it should be dot H V)
	#if !defined (LUX_DIFFUSE)
		c.rgb += (s.SpecularColor.rgb + ( 1.0 - s.SpecularColor.rgb) * s.DeferredFresnel.x) * spec * light.rgb;
//	#else
//		c.rgb += s.SpecularColor.rgb * spec * light.rgb;
	#endif
	// this here is not really worth it:
	// do not use light.rgb but only the cromatic part of it as we have stores luninance already in the lighting pass
	// half3 luminanceSensivity = half3(0.299,0.587, 0.114); 
	// half crominanceSpecLight = spec / (dot(light.rgb, luminanceSensivity) + 0.0001);
	// see: http://www.realtimerendering.com/blog/deferred-lighting-approaches/
	// c.rgb += (s.SpecularColor.rgb + ( 1.0 - s.SpecularColor.rgb) * s.DeferredFresnel) * s.SpecularColor.rgb * light.rgb * crominanceSpecLight;
	c.a = s.Alpha; // + spec;
	return c;
}

/////////////////////////////// forward lighting

float4 LightingLuxDirect (SurfaceOutputLux s, fixed3 lightDir, half3 viewDir, fixed atten){
  	// get base variables

  	// normalizing lightDir makes fresnel smoother
	lightDir = normalize(lightDir);
	// normalizing viewDir does not help here, so we skip it
	half3 h = normalize (lightDir + viewDir);
	// dotNL has to have max
	float dotNL = max (0, dot (s.Normal, lightDir));
	float dotNH = max (0, dot (s.Normal, h));
	
	#if !defined (LUX_LIGHTING_BP) && !defined (LUX_LIGHTING_CT)
		#define LUX_LIGHTING_BP
	#endif

//	////////////////////////////////////////////////////////////
//	Blinn Phong	
	#if defined (LUX_LIGHTING_BP)
	// bring specPower into a range of 0.25 – 2048
	float specPower = exp2(10 * s.Specular + 1) - 1.75;

//	Normalized Lighting Model:
	// L = (c_diff * dotNL + F_Schlick(c_spec, l_c, h) * ( (spec + 2)/8) * dotNH˄spec * dotNL) * c_light
	
//	Specular: Phong lobe normal distribution function
	//float spec = ((specPower + 2.0) * 0.125 ) * pow(dotNH, specPower) * dotNL; // would be the correct term
	// we use late * dotNL to get rid of any artifacts on the backsides
	float spec = specPower * 0.125 * pow(dotNH, specPower);

//	Visibility: Schlick-Smith
	float alpha = 2.0 / sqrt( Pi * (specPower + 2) );
	float visibility = 1.0 / ( (dotNL * (1 - alpha) + alpha) * ( saturate(dot(s.Normal, viewDir)) * (1 - alpha) + alpha) ); 
	spec *= visibility;
	#endif
	
//	////////////////////////////////////////////////////////////
//	Cook Torrrence like
//	from The Order 1886 // http://blog.selfshadow.com/publications/s2013-shading-course/rad/s2013_pbs_rad_notes.pdf

	#if defined (LUX_LIGHTING_CT)
	float dotNV = max(0, dot(s.Normal, normalize(viewDir) ) );

//	Please note: s.Specular must be linear
	float alpha = (1.0 - s.Specular); // alpha is roughness
	alpha *= alpha;
	float alpha2 = alpha * alpha;

//	Specular Normal Distribution Function: GGX Trowbridge Reitz
	float denominator = (dotNH * dotNH) * (alpha2 - 1) + 1;
	denominator = Pi * denominator * denominator;
	float spec = alpha2 / denominator;

//	Geometric Shadowing: Smith
	float V_ONE = dotNL + sqrt(alpha2 + (1 - alpha2) * dotNL * dotNL );
	float V_TWO = dotNV + sqrt(alpha2 + (1 - alpha2) * dotNV * dotNV );
	spec /= V_ONE * V_TWO;
	#endif

//	Fresnel: Schlick
	// fast fresnel approximation:
	fixed3 fresnel = s.SpecularColor.rgb + ( 1.0 - s.SpecularColor.rgb) * exp2(-OneOnLN2_x6 * dot(h, lightDir));
	// from here on we use fresnel instead of spec as it is fixed3 = color
	fresnel *= spec;
	
// Final Composition
	fixed4 c;
	// we only use fresnel here / and apply late dotNL
	c.rgb = (s.Albedo + fresnel) * _LightColor0.rgb * dotNL * (atten * 2);
	c.a = s.Alpha; // + _LightColor0.a * fresnel * atten;
	return c;
}

//////////////////////////////// directional lightmaps

inline half4 LightingLuxDirect_DirLightmap (SurfaceOutputLux s, fixed4 color, fixed4 scale, half3 viewDir, bool surfFuncWritesNormal, out half3 specColor)
{
	UNITY_DIRBASIS
	half3 scalePerBasisVector;
	
	half3 lm = DirLightmapDiffuse (unity_DirBasis, color, scale, s.Normal, surfFuncWritesNormal, scalePerBasisVector);
	
	half3 lightDir = normalize (scalePerBasisVector.x * unity_DirBasis[0] + scalePerBasisVector.y * unity_DirBasis[1] + scalePerBasisVector.z * unity_DirBasis[2]);
	half3 h = normalize (lightDir + viewDir);
	
	float dotNL = max (0, dot (s.Normal, lightDir));
	float dotNH = max (0, dot (s.Normal, h));
	
	
	#if !defined (LUX_LIGHTING_BP) && !defined (LUX_LIGHTING_CT)
		#define LUX_LIGHTING_BP
	#endif
	
//	////////////////////////////////////////////////////////////
//	Blinn Phong	
	#if defined (LUX_LIGHTING_BP)
	// bring specPower into a range of 0.25 – 2048
	float specPower = exp2(10 * s.Specular + 1) - 1.75;

//	Normalized Lighting Model:
	// L = (c_diff * dotNL + F_Schlick(c_spec, l_c, h) * ( (spec + 2)/8) * dotNH˄spec * dotNL) * c_light
	
//	Specular: Phong lobe normal distribution function
	//float spec = ((specPower + 2.0) * 0.125 ) * pow(dotNH, specPower) * dotNL; // would be the correct term
	// we use late * dotNL to get rid of any artifacts on the backsides
	float spec = specPower * 0.125 * pow(dotNH, specPower);

//	Visibility: Schlick-Smith
	float alpha = 2.0 / sqrt( Pi * (specPower + 2) );
	float visibility = 1.0 / ( (dotNL * (1 - alpha) + alpha) * ( saturate(dot(s.Normal, viewDir)) * (1 - alpha) + alpha) ); 
	spec *= visibility;
	#endif	
	
//	////////////////////////////////////////////////////////////
//	Cook Torrrence like
//	from The Order 1886 // http://blog.selfshadow.com/publications/s2013-shading-course/rad/s2013_pbs_rad_notes.pdf

	#if defined (LUX_LIGHTING_CT)	
	float dotNV = max(0, dot(s.Normal, normalize(viewDir) ) );

//	Please note: s.Specular must be linear
	float alpha = (1.0 - s.Specular); // alpha is roughness
	alpha *= alpha;
	float alpha2 = alpha * alpha;
		
//	Specular Normal Distribution Function: GGX Trowbridge Reitz
	float denominator = (dotNH * dotNH) * (alpha2 - 1) + 1;
	denominator = Pi * denominator * denominator;
	float spec = alpha2 / denominator;

//	Geometric Shadowing: Smith
	// in order to make deferred fit forward lighting better we have to tweak roughness here
	// roughness = pow(roughness, .25);
	float V_ONE = dotNL + sqrt(alpha2 + (1 - alpha2) * dotNL * dotNL );
	float V_TWO = dotNV + sqrt(alpha2 + (1 - alpha2) * dotNV * dotNV );
	spec /= V_ONE * V_TWO;
	#endif
	
//	Fresnel: Schlick
	// fixed3 fresnel = s.SpecularColor.rgb + ( 1.0 - s.SpecularColor.rgb) * pow(1.0f - saturate(dot(h, lightDir)), 5);
	// fast fresnel approximation:
	fixed3 fresnel = s.SpecularColor.rgb + ( 1.0 - s.SpecularColor.rgb) * exp2(-OneOnLN2_x6 * dot(h, lightDir));

	// from here on we use fresnel (in forward) instead of spec as it is fixed3 = color
	fresnel *= spec;
	// or spec for deferred (no fresnel term applied)

	// specColor used outside in the forward path, compiled out in prepass
	// here we drop spec and go with fresnel instead as it is float3
//	forward
	//specColor = lm * _SpecColor.rgb * s.Gloss * spec;
	specColor = lm * fresnel;
//	deferred	
	// spec from the alpha component is used to calculate specular
	// in the Lighting*_Prepass function, it's not used in forward
	// we have to compress spec like we do in the "Intrenal-PrepassLighting" shader
	return half4(lm, log2(spec + 1));
}

	//END REPLACE LIGHTING INCLUDES


	// include wetness specific functions and properties
	//#include "../LuxCore/Wetness/LuxWetness.cginc"

	float4 _Color;
	sampler2D _MainTex;
	sampler2D _SpecTex;
	sampler2D _BumpMap;
	#ifdef DIFFCUBE_ON
		samplerCUBE _DiffCubeIBL;
	#endif
	#ifdef SPECCUBE_ON
		samplerCUBE _SpecCubeIBL;
	#endif
	#if defined (LUX_AO_ON) || !defined (WetnessMaskInputVertexColors)
		sampler2D _AO;
	#endif
	
	// Is set by script
	float4 ExposureIBL;

	float _Parallax;

	float _TextureSize;
	float _MipBias;





	//FOAM
	sampler2D _FoamTex;
	float4 _FoamTex_ST;

	//CAUSTICS
	float3 _ViewDirection;
	float4 Amplitude;
	float  RefractionAmount;
	float  Ydistance;


//	Wetness specific Properties are defined in the include file

	//FLOW
//	float3 _CubemapSize;
//	float4x4 _CubeMatrix_Trans;
//	float4x4 _CubeMatrix_Inv;

//	Nevertheless we have to slightly adjust the Input Structure
	struct Input {
		// We can not use uv_MainTex and uv_BumpMap here as we might run out of Texture Interpolators
		// We can not use uv_AO here as we might run out of Texture Interpolators
		// So we combine uv_MainTex and uv_AO into LuxUV_MainAOTex
		float4 LuxUV_MainAOTex;
		float3 viewDir;
		float3 worldNormal;
		float3 worldRefl;
		float3 worldPos;		// Needed by Wetness
		float4 myworldPos;						// Needed by Wetness and Boxprojection, W: flowDirection.y //FLOW
		#ifdef WetnessMaskInputVertexColors
            fixed4 color : COLOR;
        #endif
		INTERNAL_DATA
	};

	void LuxVertWet (inout appdata_full v, out Input o) {
    	UNITY_INITIALIZE_OUTPUT(Input,o);
    	// We have to combine the texture coords
    	o.LuxUV_MainAOTex.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    	#if defined (LUX_AO_ON) || !defined (WetnessMaskInputVertexColors)
			o.LuxUV_MainAOTex.zw = TRANSFORM_TEX(v.texcoord1, _AO);
		#endif
	}

	//FLOW
	void LuxVertWetFlow (inout appdata_full v, out Input o) {
    	UNITY_INITIALIZE_OUTPUT(Input,o);
    	// We have to combine the texture coords
    	o.LuxUV_MainAOTex.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    	// If AO is enabled it is mapped to the 2nd UV Channel
    	#if defined (LUX_AO_ON) || !defined (WetnessMaskInputVertexColors)
			o.LuxUV_MainAOTex.zw = TRANSFORM_TEX(v.texcoord1, _AO);
		#endif
		// Store WorldPos
		o.myworldPos.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
		//o.myworldPos.xyz = mul(_Object2World, v.vertex).xyz;
		// Calc FlowDirection
		float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;  
		float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal.xyz );
//		// Store FlowDirection
		float2 flowDirection = ( mul(rotation, mul(unity_WorldToObject, float4(0,1,0,0)).xyz) ).xy;
//		float2 flowDirection = ( mul(rotation, mul(_World2Object, float4(0,1,0,0)).xyz) ).xy;
		


		//FLOW MYFLOWER
        //float4 flowBumpMYFLOWER = tex2Dlod(_WaterFlowRT, float4(MainUV*1,0,0)  );
        //worlNormalY = worlNormalY * pow(flowBumpMYFLOWER.y,4)* pow(flowBumpMYFLOWER.x,4);
        float4 flowBumpMYFLOWER = tex2Dlod(_WaterFlowRT, float4(v.texcoord.xy,0,0)  )  * 3.5;
        //worlNormalY = worlNormalY * pow(flowBumpMYFLOWER.y,4)* pow(flowBumpMYFLOWER.x,4);
        //flowDirection.x =  flowDirection.x +  pow(flowBumpMYFLOWER.x,2);
        //flowDirection.y =  flowDirection.y +  pow(flowBumpMYFLOWER.y,2);
        flowDirection.x =  pow(flowBumpMYFLOWER.x,1);
        flowDirection.y =  pow(flowBumpMYFLOWER.y,1);
        // v.vertex.y = v.vertex.y * flowBumpMYFLOWER.r;
        float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		//worldPos.y += 1.0;
		worldPos.y = worldPos.y + flowBumpMYFLOWER.r * _FlowHeight;
		v.vertex = mul(unity_WorldToObject, worldPos);
		//o.vertex = UnityObjectToClipPos(v.vertex);
        


        v.color.a = flowDirection.x;
		o.myworldPos.w = flowDirection.y;		
	}
	 
	void surf (Input IN, inout SurfaceOutputLux o) {

		//FLOW CAUSTICS
		float3 flowBumpMYFLOWER = tex2D(_WaterFlowRT, float2(IN.LuxUV_MainAOTex.xy )) * 3;



		//	Sample combined Height and Wetness Map first as we need both to calculate Wetness: A = height / R = Puddle Noise
        fixed4 HeightWetness = tex2D(_HeightWetness, IN.LuxUV_MainAOTex.xy) * 3;

        //FLOW CAUSTICS
        //HeightWetness.rgb = HeightWetness.rgb + 1*flowBumpMYFLOWER.rgb;

    //	If we do not use vertex colors to control puddle distribution and overall wetness we have to sample the ao tex already here
        #if !defined (WetnessMaskInputVertexColors)
        	fixed4 ambientOcclusion = tex2D(_AO, IN.LuxUV_MainAOTex.zw);
        	#define LUX_AO_SAMPLED // Do not let LuxLightingAmbient.cginc sample AO twice 
        #endif

	//	Calculate miplevel (needed by tex2Dlod)
		// Calculate componentwise max derivatives
		float2 dx1 = ddx( IN.LuxUV_MainAOTex.xy * _TextureSize * _MipBias );
		float2 dy1 = ddy( IN.LuxUV_MainAOTex.xy * _TextureSize * _MipBias );
		float d = max( dot( dx1, dx1 ), dot( dy1, dy1 ) );
		float2 lambda = 0.5 * log2(d);

	//	//////////////////
	//	Wetness specific
		
//		float3 rippleNormal = float3(0,0,1);
//		float2 wetFactor = float2(0,0);
//
//		if (_Lux_WaterFloodlevel.x + _Lux_WaterFloodlevel.y > 0 && WetMask > 0 ) {
//	        // Calculate worldNormal of Face
//	        float3 worldNormalFace = WorldNormalVector(IN, float3(0,0,1));
//	        // Claculate Wetness / wetFactor.x = overall Wetness Factor / wetFactor.y = special Wetness Factor for Raindrops
//	    	wetFactor = ComputeWaterAccumulation(PuddleMask, HeightWetness.ar, worldNormalFace.y ) * WetMask;
//	    	// Add Water Ripples
//	    	// Calc Ripple Distance
//    		float fadeOutWaterBumps = saturate( ( _Lux_WaterBumpDistance - distance(_WorldSpaceCameraPos, IN.worldPos)) / 5);
//    		// Limit Ripples to Distance
//    		if (fadeOutWaterBumps > 0 && _Lux_RainIntensity > 0) {
//	    		rippleNormal = AddWaterRipples(wetFactor, IN.worldPos, lambda, fadeOutWaterBumps);
//	    	}
//	    }
//
//    	//	Now we can apply the Parallax Extrusion
//    	float2 offset = ParallaxOffset (HeightWetness.a, _Parallax, IN.viewDir);
//    	// Add Height and Refraction
//    	IN.LuxUV_MainAOTex.xy += offset + rippleNormal.xy;

//	//////////////////
	//	Wetness specific

		//	Remap worldPos, needed by Boxprojection
		//#define worldPos myworldPos.xyz

		float3 flowNormal = float3(0,0,1);
		float3 rippleNormal = float3(0,0,1);
		float2 wetFactor = float2(0,0);

		float addToSpecular = 0;

		if (_Lux_WaterFloodlevel.x + _Lux_WaterFloodlevel.y > 0 && WetMask > 0 ) {
	        // Calculate worldNormal of Face
	        float3 worldNormalFace = WorldNormalVector(IN, float3(0,0,1));
	        // Claculate Wetness / wetFactor.x = overall Wetness Factor / wetFactor.y = special Wetness Factor for Puddles
	    	wetFactor = ComputeWaterAccumulation(PuddleMask, HeightWetness.ar, worldNormalFace.y ) * WetMask;
			// Calc WaterBumps Distance
        	float fadeOutWaterBumps = saturate( ( _Lux_WaterBumpDistance - distance(_WorldSpaceCameraPos, IN.worldPos)) / 5);
        	if (fadeOutWaterBumps > 0) {
	    		// Add Water Flow
	    		float2 flowDirection = float2(IN.color.a, IN.myworldPos.w) / 4;


	    		//FLOW MYFLOWER
		        //float4 flowBumpMYFLOWER = tex2Dlod(_WaterFlowRT, float4(MainUV*1,0,0)  );
		        //worlNormalY = worlNormalY * pow(flowBumpMYFLOWER.y,4)* pow(flowBumpMYFLOWER.x,4);
		        float2 flowUV = IN.LuxUV_MainAOTex.xy * _WaterBumpScale;
		        float2 fracTime = frac(float2(_Time.y, _Time.y + 0.5));
		        float fade = abs(fracTime.x * 2 - 1);
		        //flowDirection *= _FlowSpeed;
		        flowDirection = _FlowSpeed;		       
		       // float2 flowBumpMYFLOWER = tex2D(_WaterFlowRT, float2(IN.LuxUV_MainAOTex.xy )); //fracTime.xx * flowDirection)  );
		        //worlNormalY = worlNormalY * pow(flowBumpMYFLOWER.y,4)* pow(flowBumpMYFLOWER.x,4);
		        // flowDirection.x =  flowDirection.x +  pow(flowBumpMYFLOWER.x,2);
		        //flowDirection.y =  flowDirection.y +  pow(flowBumpMYFLOWER.y,2);
		        flowDirection.x =  pow(flowBumpMYFLOWER.x,2); //flowDirection.x * pow(flowBumpMYFLOWER.x,1) +  pow(flowBumpMYFLOWER.x,1);
		        flowDirection.y =  pow(flowBumpMYFLOWER.y,2); //flowDirection.y * pow(flowBumpMYFLOWER.y,1) +  pow(flowBumpMYFLOWER.y,1);
//		        float2 flowUV = MainUV * _WaterBumpScale;
//		        float2 fracTime = frac(float2(_Time.y, _Time.y + 0.5));
//		        float fade = abs(fracTime.x * 2 - 1);
//		        flowDirection *= _FlowSpeed;
//		        float4 flowBump = tex2Dlod(_WaterBumpMap, float4(flowUV + fracTime.xx * flowDirection, lambda)  );
//		        flowBump = lerp(flowBump, tex2Dlod(_WaterBumpMap, float4(flowUV + fracTime.yy * flowDirection, lambda) ), fade);
//		        fixed3 finalflowBump = UnpackNormal(flowBump);
	
				

				flowNormal = AddWaterFlow(IN.LuxUV_MainAOTex.xy, flowDirection, worldNormalFace.y, wetFactor.x, lambda, fadeOutWaterBumps);

				//FOAM
				addToSpecular =  flowDirection.x + flowDirection.y;


				// Add Water Ripples
				if ( _Lux_RainIntensity > 0) {
					rippleNormal = AddWaterFlowRipples(wetFactor, IN.worldPos, lambda, saturate(worldNormalFace.y), fadeOutWaterBumps );
	    		}
			}
	    }
    	
    //	Now we can apply the Parallax Extrusion
    	float2 offset = ParallaxOffset (HeightWetness.a, _Parallax, IN.viewDir);
    	// Add Height and Refraction
    	#ifdef Lux_WaterFlow
    		// Refraction of flowing Water should be damped 
			IN.LuxUV_MainAOTex.xy += offset + flowNormal.xy * _FlowRefraction + rippleNormal.xy;
		#else
			// Ripples may fully effect Refraction
			IN.LuxUV_MainAOTex.xy += offset + rippleNormal.xy;
		#endif




    //	//////////////////
    //	Standard functions
    	// Sample the Base Textures
		fixed4 diff_albedo = tex2D(_MainTex, IN.LuxUV_MainAOTex.xy);



        //ADD FLOW CAUSTICS
        #ifdef PNORMAL
		float3 norm = UnpackNormal(tex2D(_BumpMap, IN.LuxUV_MainAOTex.xy, _BumpMap));
		norm.z *= -1;
		#else
		float3 norm = tex2D(_BumpMap, IN.LuxUV_MainAOTex.xy).xyz;
		#endif

		//float3 _ViewDirection = float3(0,1,0);
		float3 refractor = refract(_ViewDirection, norm, RefractionAmount);
		refractor.xy = refractor.xy/refractor.z;	
		
		float2 Reft = Ydistance * refractor.xy + IN.LuxUV_MainAOTex.xy;//IN.uv;
		//float3 flowCaustics = tex2D(_WaterCaustics, float2(IN.LuxUV_MainAOTex.xy ));
		//float4 tex = tex2D(_MainTex, Reft);
		//float causticsTex = tex2D(causticSurfMain, Reft).r;
		float flowCaustics = tex2D(_WaterCaustics, Reft ).r;
		float4 finalResult = diff_albedo +  0.1*float4(1,1,1,1)*(dot(float2(4*Amplitude.x,1*Amplitude.y), float2(1, Ydistance*flowCaustics))) ;
      	diff_albedo = finalResult;


      	//FLOW
		//FLOW CAUSTICS
        //diff_albedo.rgb = 0.7*diff_albedo.rgb + 1.2*(flowBumpMYFLOWER.rgb*flowBumpMYFLOWER.rgb);
        diff_albedo.rgb = clamp(0.6*diff_albedo.rgb + 25*(pow(flowBumpMYFLOWER.rrr,4)) , 0 ,1);;


		fixed4 spec_albedo = tex2D(_SpecTex, IN.LuxUV_MainAOTex.xy);

		float addFoam = 1;
		if(addToSpecular > 0.12){
			if(addToSpecular < 2){

				//FOAM
				float2 fracTime1 = frac(float2(_Time.y, _Time.y ));
				float2 flowDirection1 = float2(IN.color.a, IN.myworldPos.w) / 4;
				//fixed3 FoamTex = tex2D(_FoamTex, IN.LuxUV_MainAOTex.xy * _FoamTex_ST.xy + _Time.y * flowDirection1 + _FoamTex_ST.zw);
				//fixed3 FoamTex = tex2D(_FoamTex, IN.LuxUV_MainAOTex.xy * _FoamTex_ST.xy * _Time.y * flowDirection1 + _FoamTex_ST.zw);

				//fixed3 FoamTex = tex2D(_FoamTex, (IN.LuxUV_MainAOTex.xy * _FoamTex_ST.xy  + _FoamTex_ST.zw)* _Time.y * flowDirection1);
				fixed3 FoamTex = tex2D(_FoamTex, (IN.LuxUV_MainAOTex.xy * _FoamTex_ST.xy  + _FoamTex_ST.zw* _Time.y) * flowDirection1);
				//diff_albedo.rgb = 0.2 * diff_albedo.rgb * (FoamTex+0.2) + 0.8 * (FoamTex+0.2);

				//diff_albedo.rgb = diff_albedo.rgb * (FoamTex+0.2);
				//addFoam = 2 * addToSpecular + 1.1;

			//	//diff_albedo.rgb = diff_albedo.rgb * (FoamTex+0.1);

				if(addToSpecular > 0.45){
					//addFoam = 1.8  + 1.1*FoamTex;
					//addFoam = 1.8  *FoamTex;
					addFoam = 1.6 + FoamTex;
				}else{
					//addFoam = 2 * addToSpecular + 1.1*FoamTex;
					addFoam = 2 * addToSpecular + 1.05*FoamTex;
				}

			}
		}
		//if(diff_albedo.r < 0.15){
			//diff_albedo = clamp(diff_albedo,0.15,1);
		//}

		  

		// Diffuse Albedo
		o.Albedo = clamp(diff_albedo.rgb * _Color.rgb * addFoam  , 0 ,1);;
		o.Alpha = diff_albedo.a * _Color.a;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.LuxUV_MainAOTex.xy));
		// Specular Color
		o.SpecularColor = spec_albedo.rgb ;
		// Roughness
		o.Specular = spec_albedo.a;
		
	//	//////////////////
	//	Wetness specific
		if (_Lux_WaterFloodlevel.x + _Lux_WaterFloodlevel.y > 0 && WetMask > 0 ) {
			// Calling "o.Specular = WaterBRDF()" will tweak o.Albedo, o.Specular and o.SpecularColor according to the overall wetness (wetFactor.x)
			o.Specular = WaterBRDF(o.Albedo, o.Specular, o.SpecularColor, wetFactor.x);
			// Finally tweak o.Normal based on the overall Wetness Factor
			//o.Normal = lerp(o.Normal, rippleNormal, wetFactor.x );

			//FLOW
			o.Normal = lerp(o.Normal, normalize(flowNormal + rippleNormal), wetFactor.x) ;
		}
		// We have to remap the _AO uv coords before we can call the include
		#define uv_AO LuxUV_MainAOTex.zw
	//	//////////////////

		// Convert Roughness for BlinnPhong / CookTorrence as in other Lux shaders
		o.Specular = LuxAdjustSpecular(o.Specular) ;

		//#include "../LuxCore/LuxLightingAmbient.cginc"


		//AMBIEBT INCLUDE
		//		///////////////////////////////////////
// 		Further functions to keep the surf function rather simple
		
		#ifdef SPECCUBE_ON
			float NdotV = max(0, dot(o.Normal, normalize(IN.viewDir.xyz)));
		#endif

		// Fake Fresnel effect using N dot V / only needed by deferred lighting	
		#if defined(UNITY_PASS_PREPASSFINAL) && defined (SPECCUBE_ON)
			#ifdef NO_DEFERREDFRESNEL
				o.DeferredFresnel = 0;
			#else
				o.DeferredFresnel = exp2(-OneOnLN2_x6 * NdotV);
			#endif
		#endif

//		///////////////////////////////////////
// 		Lux IBL / ambient lighting
		
		// set o.Emission = 0.0 to make diffuse shaders work correctly
		o.Emission = 0.0;

		#ifdef NORMAL_IS_WORLDNORMAL
			float3 worldNormal = IN.normal;
		#else
			#ifdef USE_BLURREDNORMAL
				float3 worldNormal = WorldNormalVector(IN, o.NormalBlur);
			#else
				float3 worldNormal = WorldNormalVector(IN, o.Normal);
			#endif
		#endif

		#if defined(USE_GLOBAL_DIFFIBL_SETTINGS) && defined(GLDIFFCUBE_ON)
			#define DIFFCUBE_ON
		#endif

//		add diffuse IBL
		#ifdef DIFFCUBE_ON
			fixed4	diff_ibl = texCUBE (_DiffCubeIBL, worldNormal);
			#ifdef LUX_LINEAR
				// if colorspace = linear alpha has to be brought to linear too (rgb already is): alpha = pow(alpha,2.233333333).
				// approximation taken from http://chilliant.blogspot.de/2012/08/srgb-approximations-for-hlsl.html
				diff_ibl.a *= diff_ibl.a * (diff_ibl.a * 0.305306011 + 0.682171111) + 0.012522878;
			#endif
			diff_ibl.rgb = diff_ibl.rgb * diff_ibl.a;
			o.Emission = diff_ibl.rgb * ExposureIBL.x * o.Albedo;
		#else
			#if defined (LIGHTMAP_OFF) && defined (DIRLIGHTMAP_OFF)
//		otherwise add ambient light from Spherical Harmonics
				o.Emission = ShadeSH9 ( float4(worldNormal.xyz, 1.0)) * o.Albedo;
			#endif
			#if defined (LUX_LIGHTMAP_OFF)
//		otherwise add ambient light from Spherical Harmonics
				o.Emission = ShadeSH9 ( float4(worldNormal.xyz, 1.0)) * o.Albedo;
			#endif
		#endif
		
//		add specular IBL		
		#ifdef SPECCUBE_ON
			//#ifdef LUX_BOXPROJECTION
			//	half3 worldRefl;
			//#else
				half3 worldRefl = WorldReflectionVector (IN, o.Normal);	
			//#endif

		//	Boxprojection / Rotation
			#ifdef LUX_BOXPROJECTION
				// Bring worldRefl and worldPos into Cube Map Space
				worldRefl = mul(_CubeMatrix_Trans, float4(worldRefl,1)).xyz;
				float3 PosCS = mul(_CubeMatrix_Inv,float4(IN.worldPos,1)).xyz;
				float3 FirstPlaneIntersect = _CubemapSize - PosCS;
				float3 SecondPlaneIntersect = -_CubemapSize - PosCS;
				float3 FurthestPlane = (worldRefl > 0.0) ? FirstPlaneIntersect : SecondPlaneIntersect;
				FurthestPlane /= worldRefl;
				float Distance = min(FurthestPlane.x, min(FurthestPlane.y, FurthestPlane.z));
				worldRefl = PosCS + worldRefl * Distance;
			#endif

			#if defined (LUX_LIGHTING_CT)
				o.Specular *= o.Specular * (o.Specular * 0.305306011 + 0.682171111) + 0.012522878;
			#endif

			float mipSelect = 1.0f - o.Specular;
			mipSelect = mipSelect * mipSelect * 7; // but * 6 would look better...
			fixed4 spec_ibl = texCUBElod (_SpecCubeIBL, float4(worldRefl, mipSelect));
			#ifdef LUX_LINEAR
				// if colorspace = linear alpha has to be brought to linear too (rgb already is): alpha = pow(alpha,2.233333333) / approximation taken from http://chilliant.blogspot.de/2012/08/srgb-approximations-for-hlsl.html
				spec_ibl.a *= spec_ibl.a * (spec_ibl.a * 0.305306011 + 0.682171111) + 0.012522878;
			#endif
			spec_ibl.rgb = spec_ibl.rgb * spec_ibl.a;
			// fresnel based on spec_albedo.rgb and roughness (spec_albedo.a) / taken from: http://seblagarde.wordpress.com/2011/08/17/hello-world/
			float3 FresnelSchlickWithRoughness = o.SpecularColor + ( max(o.Specular, o.SpecularColor ) - o.SpecularColor) * exp2(-OneOnLN2_x6 * NdotV);
			// colorize fresnel highlights and make it look like marmoset:
			// float3 FresnelSchlickWithRoughness = o.SpecularColor + o.Specular.xxx * o.SpecularColor * exp2(-OneOnLN2_x6 * NdotV);	
			spec_ibl.rgb *= FresnelSchlickWithRoughness * ExposureIBL.y;
			// add diffuse and specular and conserve energy
			o.Emission = (1 - spec_ibl.rgb) * o.Emission + spec_ibl.rgb;

		#endif
		
		#ifdef LUX_AO_ON
			#if !defined(LUX_AO_SAMPLED)
				half ambientOcclusion = tex2D(_AO,IN.uv_AO).a;
				o.Emission *= ambientOcclusion;
			#else
				o.Emission *= ambientOcclusion.a;
			#endif
		#endif

		#ifdef LUX_METALNESS
			o.Emission *= spec_albedo.g;
		#endif

		//END AMBIENT INCLUDE

	}
ENDCG
}
FallBack "Specular"
//CustomEditor "LuxMaterialInspector"
}

