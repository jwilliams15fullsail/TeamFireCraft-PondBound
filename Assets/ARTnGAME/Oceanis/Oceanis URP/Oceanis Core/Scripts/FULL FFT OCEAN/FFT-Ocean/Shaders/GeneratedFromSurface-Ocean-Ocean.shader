// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Ocean/Ocean BiRP"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _SSSColor("SSS Color", Color) = (1,1,1,1)
        _SSSStrength("SSSStrength", Range(0,1)) = 0.2
        _SSSScale("SSS Scale", Range(0.1,50)) = 4.0
        _SSSBase("SSS Base", Range(-5,1)) = 0
        _LOD_scale("LOD_scale", Range(1,10)) = 0
        _MaxGloss("Max Gloss", Range(0,1)) = 0
        _Roughness("Distant Roughness", Range(0,1)) = 0
        _RoughnessScale("Roughness Scale", Range(0, 0.01)) = 0.1
        _FoamColor("Foam Color", Color) = (1,1,1,1)
        _FoamTexture("Foam Texture", 2D) = "grey" {}
        _FoamBiasLOD0("Foam Bias LOD0", Range(0,7)) = 1
        _FoamBiasLOD1("Foam Bias LOD1", Range(0,7)) = 1
        _FoamBiasLOD2("Foam Bias LOD2", Range(0,7)) = 1
        _FoamScale("Foam Scale", Range(0,20)) = 1
        _ContactFoam("Contact Foam", Range(0,1000)) = 1


        [Header(Cascade 0)]
        [HideInInspector]_Displacement_c0("Displacement C0", 2D) = "black" {}
        [HideInInspector]_Derivatives_c0("Derivatives C0", 2D) = "black" {}
        [HideInInspector]_Turbulence_c0("Turbulence C0", 2D) = "white" {}
        [Header(Cascade 1)]
        [HideInInspector]_Displacement_c1("Displacement C1", 2D) = "black" {}
        [HideInInspector]_Derivatives_c1("Derivatives C1", 2D) = "black" {}
        [HideInInspector]_Turbulence_c1("Turbulence C1", 2D) = "white" {}
        [Header(Cascade 2)]
        [HideInInspector]_Displacement_c2("Displacement C2", 2D) = "black" {}
        [HideInInspector]_Derivatives_c2("Derivatives C2", 2D) = "black" {}
        [HideInInspector]_Turbulence_c2("Turbulence C2", 2D) = "white" {}
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Opaque" }
        LOD 200
		Cull Off
        
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		//Name "FORWARD"
		//Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile _ MID CLOSE
#pragma target 4.0
#pragma multi_compile_instancing
//#pragma multi_compile_fog
//#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_LIGHTING_COORDS(8,9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission * 2;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c * 1;
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(CLOSE) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_LIGHTING_COORDS(8,9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c * 1;
}


#endif

// -------- variant for: MID 
#if defined(MID) && !defined(CLOSE) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_LIGHTING_COORDS(8,9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: MID INSTANCING_ON 
#if defined(MID) && defined(INSTANCING_ON) && !defined(CLOSE)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_LIGHTING_COORDS(8,9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c*1;
}


#endif

// -------- variant for: CLOSE 
#if defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.01 + _Time.r).r;


            //return float4(foam, foam, foam, 1);
           
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

           // o.Emission = color * fresnel + 0.5*color;// lerp(color * (1 - fresnel), 0, jacobian);

            o.Emission = lerp(saturate(color * (1 - fresnel)), 1*color, saturate(jacobian));

            //o.Emission = color* jacobian* float4(foam, foam, foam, 1)/5;
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_LIGHTING_COORDS(8,9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c*1;
}


#endif

// -------- variant for: CLOSE INSTANCING_ON 
#if defined(CLOSE) && defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_LIGHTING_COORDS(8,9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
  float4 lmap : TEXCOORD7;
  UNITY_FOG_COORDS(8)
  UNITY_SHADOW_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif


ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile _ MID CLOSE
#pragma target 4.0
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma skip_variants INSTANCING_ON
#pragma multi_compile_fwdadd_fullshadows
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float3 tSpace0 : TEXCOORD0;
  float3 tSpace1 : TEXCOORD1;
  float3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 screenPos : TEXCOORD4;
  float2 custompack0 : TEXCOORD5; // worldUV
  float4 custompack1 : TEXCOORD6; // lodScales
  float3 custompack2 : TEXCOORD7; // viewVector
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_FOG_COORDS(10)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.screenPos = ComputeScreenPos (o.pos);

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: MID 
#if defined(MID) && !defined(CLOSE) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float3 tSpace0 : TEXCOORD0;
  float3 tSpace1 : TEXCOORD1;
  float3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 screenPos : TEXCOORD4;
  float2 custompack0 : TEXCOORD5; // worldUV
  float4 custompack1 : TEXCOORD6; // lodScales
  float3 custompack2 : TEXCOORD7; // viewVector
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_FOG_COORDS(10)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.screenPos = ComputeScreenPos (o.pos);

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: CLOSE 
#if defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float3 tSpace0 : TEXCOORD0;
  float3 tSpace1 : TEXCOORD1;
  float3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 screenPos : TEXCOORD4;
  float2 custompack0 : TEXCOORD5; // worldUV
  float4 custompack1 : TEXCOORD6; // lodScales
  float3 custompack2 : TEXCOORD7; // viewVector
  UNITY_LIGHTING_COORDS(8,9)
  UNITY_FOG_COORDS(10)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.screenPos = ComputeScreenPos (o.pos);

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif


ENDCG

}

	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile _ MID CLOSE
#pragma target 4.0
#pragma multi_compile_instancing
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
            o.Emission = lerp(saturate(color * (1 - fresnel)), 1 * color, saturate(jacobian));
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifndef DIRLIGHTMAP_OFF
  float3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.screenPos = ComputeScreenPos (o.pos);
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(CLOSE) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifndef DIRLIGHTMAP_OFF
  float3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.screenPos = ComputeScreenPos (o.pos);
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: MID 
#if defined(MID) && !defined(CLOSE) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifndef DIRLIGHTMAP_OFF
  float3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.screenPos = ComputeScreenPos (o.pos);
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: MID INSTANCING_ON 
#if defined(MID) && defined(INSTANCING_ON) && !defined(CLOSE)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifndef DIRLIGHTMAP_OFF
  float3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.screenPos = ComputeScreenPos (o.pos);
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: CLOSE 
#if defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifndef DIRLIGHTMAP_OFF
  float3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.screenPos = ComputeScreenPos (o.pos);
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: CLOSE INSTANCING_ON 
#if defined(CLOSE) && defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifndef DIRLIGHTMAP_OFF
  float3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.screenPos = ComputeScreenPos (o.pos);
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif


ENDCG

}

	// ---- shadow caster pass:
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		ZWrite On ZTest LEqual

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile _ MID CLOSE
#pragma target 4.0
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_shadowcaster
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 custompack0 : TEXCOORD2; // worldUV
  float4 custompack1 : TEXCOORD3; // lodScales
  float3 custompack2 : TEXCOORD4; // viewVector
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(CLOSE) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 custompack0 : TEXCOORD2; // worldUV
  float4 custompack1 : TEXCOORD3; // lodScales
  float3 custompack2 : TEXCOORD4; // viewVector
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: MID 
#if defined(MID) && !defined(CLOSE) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 custompack0 : TEXCOORD2; // worldUV
  float4 custompack1 : TEXCOORD3; // lodScales
  float3 custompack2 : TEXCOORD4; // viewVector
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: MID INSTANCING_ON 
#if defined(MID) && defined(INSTANCING_ON) && !defined(CLOSE)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 custompack0 : TEXCOORD2; // worldUV
  float4 custompack1 : TEXCOORD3; // lodScales
  float3 custompack2 : TEXCOORD4; // viewVector
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: CLOSE 
#if defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 custompack0 : TEXCOORD2; // worldUV
  float4 custompack1 : TEXCOORD3; // lodScales
  float3 custompack2 : TEXCOORD4; // viewVector
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: CLOSE INSTANCING_ON 
#if defined(CLOSE) && defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 custompack0 : TEXCOORD2; // worldUV
  float4 custompack1 : TEXCOORD3; // lodScales
  float3 custompack2 : TEXCOORD4; // viewVector
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif


ENDCG

}

	// ---- meta information extraction pass:
	Pass {
		Name "Meta"
		Tags { "LightMode" = "Meta" }
		Cull Off

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile _ MID CLOSE
#pragma target 4.0
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma shader_feature EDITOR_VISUALIZATION

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD7;
  float4 lightCoord : TEXCOORD8;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(CLOSE) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD7;
  float4 lightCoord : TEXCOORD8;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: MID 
#if defined(MID) && !defined(CLOSE) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD7;
  float4 lightCoord : TEXCOORD8;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: MID INSTANCING_ON 
#if defined(MID) && defined(INSTANCING_ON) && !defined(CLOSE)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD7;
  float4 lightCoord : TEXCOORD8;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: CLOSE 
#if defined(CLOSE) && !defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD7;
  float4 lightCoord : TEXCOORD8;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: CLOSE INSTANCING_ON 
#if defined(CLOSE) && defined(INSTANCING_ON) && !defined(MID)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: YES
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 39 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        //#pragma multi_compile _ MID CLOSE
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        //#pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color, _FoamColor, _SSSColor;
        float _SSSStrength;
        float _Roughness, _RoughnessScale, _MaxGloss;
        float _FoamBiasLOD0, _FoamBiasLOD1, _FoamBiasLOD2, _FoamScale, _ContactFoam;
        sampler2D _CameraDepthTexture;
        sampler2D _FoamTexture;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            
            #if defined(CLOSE)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x
                + tex2D(_Turbulence_c2, IN.worldUV / LengthScale2).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD2) * _FoamScale));
            #elif defined(MID)
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x
                + tex2D(_Turbulence_c1, IN.worldUV / LengthScale1).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD1) * _FoamScale));
            #else
            float jacobian = tex2D(_Turbulence_c0, IN.worldUV / LengthScale0).x;
            jacobian = min(1, max(0, (-jacobian + _FoamBiasLOD0) * _FoamScale));
            #endif

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
            float depthDifference = max(0, backgroundDepth - surfaceDepth - 0.1);
            float foam = tex2D(_FoamTexture, IN.worldUV * 0.5 + _Time.r).r;
            jacobian += _ContactFoam * saturate(max(0, foam - depthDifference) * 5) * 0.9;

            o.Albedo = lerp(0, _FoamColor, jacobian);
            float distanceGloss = lerp(1 - _Roughness, _MaxGloss, 1 / (1 + length(IN.viewVector) * _RoughnessScale));
            o.Smoothness = lerp(distanceGloss, 0, jacobian);
            o.Metallic = 0;

            float3 viewDir = normalize(IN.viewVector);
            float3 H = normalize(-worldNormal + _WorldSpaceLightPos0);
            float ViewDotH = pow5(saturate(dot(viewDir, -H))) * 30 * _SSSStrength;
            fixed3 color = lerp(_Color, saturate(_Color + _SSSColor.rgb * ViewDotH * IN.lodScales.w), IN.lodScales.z);

            float fresnel = dot(worldNormal, viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow5(fresnel);

            o.Emission = lerp(color * (1 - fresnel), 0, jacobian);
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 screenPos : TEXCOORD3;
  float2 custompack0 : TEXCOORD4; // worldUV
  float4 custompack1 : TEXCOORD5; // lodScales
  float3 custompack2 : TEXCOORD6; // viewVector
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD7;
  float4 lightCoord : TEXCOORD8;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xy = customInputData.worldUV;
  o.custompack1.xyzw = customInputData.lodScales;
  o.custompack2.xyz = customInputData.viewVector;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.screenPos = ComputeScreenPos (o.pos);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.worldUV.x = 1.0;
  surfIN.lodScales.x = 1.0;
  surfIN.viewVector.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.screenPos.x = 1.0;
  surfIN.worldUV = IN.custompack0.xy;
  surfIN.lodScales = IN.custompack1.xyzw;
  surfIN.viewVector = IN.custompack2.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.screenPos = IN.screenPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif


ENDCG

}

	// ---- end of surface shader generated code

#LINE 183

    }
        FallBack "Diffuse"
}