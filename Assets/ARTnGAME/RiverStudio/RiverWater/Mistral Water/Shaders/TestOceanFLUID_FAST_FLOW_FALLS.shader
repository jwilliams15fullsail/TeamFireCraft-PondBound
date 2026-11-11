// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Test/OceanFLUID_FAST_FLOW_FALLS"
{
	Properties
	{
		_Anim("Displacement Map", 2D) = "black" {}
	_Height("Height Map", 2D) = "black" {}
	_Bump("Normal Map",2D) = "bump" {}
	_White("White Cap Map", 2D) = "black" {}
	_LightWrap("Light Wrapping Value", Float) = 1
		_Tint("Color Tint", Color) = (0.5, 0.65, 0.75, 1)
		_SpecColor("Specular Color", Color) = (1, 0.25, 0, 1)
		_Glossiness("Glossiness", Float) = 64
		_RimColor("Rim Color", Color) = (0, 0, 1, 1)

		//v0.1
		sssPower("sssPower", Float) = 1.5
		sssEmit("sssEmit", Float) = 1
		sssScatter("sssScatter", Color) = (0, 0.6, 0.3, 1)
		wavesHeight("wavesHeight", Float) = 1

		//v0.2
		_WaterFlowRT("Water Flow Real Time", 2D) = "white" {} //JACOBI
	_WaterFlowBUMP("Flow bump", Float) = 1
		fluidSize("fluid Size", Float) = (0, 0, 1, 1)
		_FlowHeight("_Flow Height", Float) = (1, 1, 1, 1)

		//v0.3
		_Distortion("Refract Distortion", Float) = 1
		_ReflectionStrength("_Reflection Strength", Float) = 1
		_CubeTint("Cube Color Tint", Color) = (1, 1, 1, 1)
		[NoScaleOffset]
	_CubeMap("Cube Map", Cube) = "white" {}
	_FresnelAngle("Fresnel Angle", Range(1, 45)) = 5
		refractPower("refract Power", Float) = (1, 1, 1, 1)

		//v0.3
		_WaterFlowADVECT("Water Flow ADVECT", 2D) = "white" {}
	_WaterFlowFOAM("Water Flow FOAM", 2D) = "white" {}

	//v0.4 - FOAM controls
	foamDirection("Foam Direction", Float) = (0, 1, 1, 1)
		foamPower("Foam Power", Float) = (1, 1, 1, 1)

		//v0.5
		_ShallowColor("Color in the Shallow Area", Color) = (1, 1, 1, 1)
		_DeepColor("Color in the Deep Area", Color) = (0, 0, 0, 0)
		_DepthAmount("The maximum of depth", Float) = 1
		_DepthFade("The clarity of depth", Float) = 1
		shallowPower("shallowPower", Float) = 1

		//v0.6
		_clearColorDepth("_clearColorDepth", Float) = 0.4
		_clearColorPower("_clearColorPower", Float) = 1
		_clearColor("_clearColor", Color) = (1, 1, 1, 1)

		//v0.7
		_WaterFlowRTA("Water Waves Real Time", 2D) = "white" {}
		_FlowHeightA("_Flow Height Local Waves", Float) = 1
		_LocalWavesTiling("_Local Waves Tiling", Float) = (1, 1, 0, 0)

		//v0.8 rain drops
		_Lux_RainRipples("_Lux_RainRipples", 2D) = "white" {}
		_Lux_RippleWindSpeed("_Lux_RippleWindSpeed", Float) = (0,0,0,0)
		_Lux_WaterFloodlevel("_Lux_WaterFloodlevel", Float) = (1,1,1,1)
		_Lux_RainIntensity("_Lux_RainIntensity", Float) = 0.0
		_Lux_RippleAnimSpeed("_Lux_RippleAnimSpeed", Float) = 0.75
		_Lux_RippleTiling("_Lux_RippleTiling", Float) = 0.75
		_Lux_WaterBumpDistance("_Lux_WaterBumpDistance", Float) = 0.75

			//v0.1 - RIVER
			_DepthCameraPos("_DepthCameraPos", Float) = (0,0,0,0)
			_TerrainScale("_TerrainScale", Float) = 0
			_MeshFlowDisplacement("_MeshFlowDisplacement", Float) = (1,1,1,0)
			_clearTextureOffset("Clear around texture if beyond UVs in World Space mode", Float) = 0.01

			//v0.9 - RIVER1
			_foamTexture("Foam Texture", 2D) = "white" {}
			foamControlsA("foamControlsA", Float) = (1, 1, 1, 1)
				_NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5

				_StateTex("_StateTex", 2D) = "white" {} //_StateTex

			//v1.1
			_HeightMap("Height Map", 2D) = "black" {}
			flowControl("flow Controls", Float) = (1, 1, 1, 1)
	}

		SubShader
	{

		//v0.3 - refraction
		GrabPass
	{
		Tags{ "LightMode" = "Always" }
	}

		Pass
	{

			Tags{

			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"

		}


		Cull Back
		//ZWrite On
		//ZTest LEqual
		//ColorMask RGB
		//Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

#include "UnityCG.cginc"

#pragma vertex vert
#pragma fragment frag

			//v1.0 - FAST_FLOW
			sampler2D _StateTex;
			float2 _StateTex_TexelSize;
			#define WATER_HEIGHT(s) (s.g)
			#define TERRAIN_HEIGHT(s) (s.r)
			#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))
			float _NormalStrength;

			//RIVER
		float4 _DepthCameraPos;
	float _TerrainScale;
	float4 _MeshFlowDisplacement;
	float _clearTextureOffset;

		uniform sampler2D _Anim;
	uniform sampler2D _Height;
	uniform sampler2D _Bump;
	uniform sampler2D _White;

	uniform float4 _Tint;
	uniform float4 _SpecColor;
	uniform float _Glossiness;
	uniform float _LightWrap;
	uniform fixed4 _RimColor;

	uniform float4 _LightColor0;

	//v0.1
	float sssPower;
	float sssEmit;
	float4 sssScatter; float wavesHeight;

	//v0.2
	//FLOW
	sampler2D _WaterFlowRT;
	float _WaterFlowBUMP;
	float4 fluidSize;
	float4 _FlowHeight;

	//v0.3
	float _Distortion;
	uniform sampler2D _GrabTexture;
	uniform half4 _GrabTexture_TexelSize;
	uniform float _ReflectionStrength;
	uniform half _FresnelAngle;
	uniform samplerCUBE _CubeMap;
	uniform fixed3 _CubeTint;
	float4 refractPower;

	//v0.3
	sampler2D _WaterFlowADVECT;
	sampler2D _WaterFlowFOAM;
	float4 _WaterFlowFOAM_ST;

	//v0.4
	float4 foamDirection;
	float4 foamPower;

	//v0.5
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture); //uniform sampler2D_float _CameraDepthTexture; //VR1
	uniform fixed4 _ShallowColor;
	uniform fixed4 _DeepColor;
	uniform half _DepthAmount;
	uniform half _EdgeFade;
	uniform half _DepthFade;
	uniform float shallowPower;

	//v0.6
	float _clearColorDepth;
	float _clearColorPower;
	float4 _clearColor;

	//v0.7
	sampler2D _WaterFlowRTA;
	float _FlowHeightA;
	float4 _LocalWavesTiling;

	//v0.8
	// Global Rain Properties passed in by Script
	float4 _Lux_WaterFloodlevel;
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
	//	Add animated Ripples to areas where the Water Accumulation is high enough
	//  Returns the tweaked and adjusted Ripple Normal
	float3 AddWaterRipples(float2 i_wetFactor, float3 i_worldPos, float2 lambda, float fadeOutWaterBumps)
	{
		float4 Weights = _Lux_RainIntensity - float4(0, 0.25, 0.5, 0.75);
		Weights = saturate(Weights * 4);
		float animSpeed = _Time.y * _Lux_RippleAnimSpeed;
		float2 Ripple1 = ComputeRipple(float4(i_worldPos.xz * _Lux_RippleTiling + float2(0.25f, 0.0f) + _Lux_RippleWindSpeed.xy * _Time.y, lambda), animSpeed, Weights.x);
		float2 Ripple2 = ComputeRipple(float4(i_worldPos.xz * _Lux_RippleTiling + float2(-0.55f, 0.3f) + _Lux_RippleWindSpeed.zw * _Time.y, lambda), animSpeed * 0.71, Weights.y);
		float3 rippleNormal = float3(Weights.x * Ripple1.xy + Weights.y * Ripple2.xy, 1);
		// Blend and fade out Ripples
		return lerp(float3(0, 0, 1), rippleNormal, i_wetFactor.y * i_wetFactor.y * fadeOutWaterBumps);
	}
	//  Add Water Ripples to Waterflow
	float3 AddWaterFlowRipples(float2 i_wetFactor, float3 i_worldPos, float2 lambda, float i_worldNormalFaceY, float fadeOutWaterBumps)
	{
		float4 Weights = _Lux_RainIntensity - float4(0, 0.25, 0.5, 0.75);
		Weights = saturate(Weights * 4);
		float animSpeed = _Time.y * _Lux_RippleAnimSpeed;
		float2 Ripple1 = ComputeRipple(float4(i_worldPos.xz * _Lux_RippleTiling + float2(0.25f, 0.0f) + _Lux_RippleWindSpeed.xy * _Time.y, lambda), animSpeed, Weights.x);
		float2 Ripple2 = ComputeRipple(float4(i_worldPos.xz * _Lux_RippleTiling + float2(-0.55f, 0.3f) + _Lux_RippleWindSpeed.zw * _Time.y, lambda), animSpeed * 0.71, Weights.y);
		float3 rippleNormal = float3(Weights.x * Ripple1.xy + Weights.y * Ripple2.xy, 1);
		// Blend and fade out Ripples
		return lerp(float3(0, 0, 1), rippleNormal, i_wetFactor.y * i_wetFactor.y * fadeOutWaterBumps * i_worldNormalFaceY*i_worldNormalFaceY);
	}


	//v0.9
	sampler2D _foamTexture;
	float4 foamControlsA;

	//v1.1
	sampler2D _HeightMap;
	float4 _HeightMap_ST;
	float4 flowControl;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
		float3 normal: NORMAL; //v1.1
		float4 tangent: TANGENT; //v1.1
		UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
	};

	struct VertexOutput
	{
		float4 pos : SV_POSITION;
		float3 normal : TEXCOORD1;
		float4 texcoord : TEXCOORD0;
		float4 color : TEXCOORD2;
		float3 lightDir : TEXCOORD3;
		float3 viewDir : TEXCOORD4;
		//v0.3
		float4 grabUV : TEXCOORD5;
		//v0.5
		float4 screenPos: TEXCOORD6;
		//v0.8
		float4 worldPos:  TEXCOORD7;

		float3 TtoW0 : TEXCOORD8;//v1.1
		float3 TtoW1 : TEXCOORD9;//v1.1
		float3 TtoW2 : TEXCOORD10;//v1.1
		float3 normalORIGIN: TEXCOORD11;//v1.1

		UNITY_VERTEX_OUTPUT_STEREO //VR1
	};

	VertexOutput vert(VertexInput v)
	{
		VertexOutput o;

		UNITY_SETUP_INSTANCE_ID(v); //v1.7.8 //VR1
		UNITY_INITIALIZE_OUTPUT(VertexOutput, o); //Insert //VR1
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert //VR1

		//v1.0 - FAST_FLOW
		float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
		v.vertex.y += FULL_HEIGHT(state);




		//v.vertex.y += tex2Dlod(_Height, v.texcoord).r / 8;
		//v.vertex.xz += tex2Dlod(_Anim, v.texcoord).rb / 8;

		v.vertex.y += _MeshFlowDisplacement.y * clamp(tex2Dlod(_Height, v.texcoord).r, -24, 24) / 8;
		v.vertex.xz += _MeshFlowDisplacement.xz * clamp(tex2Dlod(_Anim, v.texcoord).rb,-24, 24) / 8;

		o.normal = UnityObjectToWorldNormal(tex2Dlod(_Bump, v.texcoord).rgb);
		o.normalORIGIN = mul(unity_ObjectToWorld, v.normal);//v.normal;


		///v0.2
		//FLOW MYFLOWER
		//float4 tangent = float4(0, 1, 0, 1);
		//float3 normal = o.normal;// float3(info.b, sqrt(1.0 - dot(info.ba, info.ba)), info.a);
		//float3 binormal = cross(normal, tangent.xyz) * tangent.w;
		//float3x3 rotation = float3x3(tangent.xyz, binormal, normal.xyz);
		//		// Store FlowDirection
		//float2 flowDirection = (mul(rotation, mul(unity_WorldToObject, float4(0, 1, 0, 0)).xyz)).xy;
		//float _FlowHeight = 1;
		float4 flowBumpMYFLOWER = tex2Dlod(_WaterFlowRT, float4(v.texcoord.xy * float2(fluidSize.z, fluidSize.w) + float2(fluidSize.x, fluidSize.y), 0, 0))  * _FlowHeight.x;
		//flowDirection.x = pow(flowBumpMYFLOWER.x, 14);
		//flowDirection.y = pow(flowBumpMYFLOWER.y, 14);
		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		//if ((flowBumpMYFLOWER.r) == 0	) {
		//	worldPos.y = 30;				
		//}
		//else {
		worldPos.y = worldPos.y + flowBumpMYFLOWER.r * _FlowHeight.y;// _FlowHeight;fluidSize
																	 //}
																	 //worldPos.xz += worldPos.xz * flowBumpMYFLOWER.r * _FlowHeight.z;
		worldPos.xz += (pow(saturate(flowBumpMYFLOWER.r),0.4)* _FlowHeight.z);



		//v0.7
		//float2 flowDirection = float2(0,0);
		float4 localWaves = tex2Dlod(_WaterFlowRTA, float4(v.texcoord.x * _LocalWavesTiling.x+_LocalWavesTiling.z, v.texcoord.y* _LocalWavesTiling.y+ _LocalWavesTiling.w, 0, 0))  * 3.5;
		//flowDirection.x = pow(localWaves.x, 1);
		//flowDirection.y = pow(localWaves.y, 1);
		//float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		//worldPos.y = worldPos.y + clamp(pow(localWaves.r*0.5,4) * _FlowHeightA,0,1.8);
		worldPos.y = worldPos.y + clamp(pow(localWaves.r*0.5, 4) * _FlowHeightA, 0, 1.65);
		//v.vertex = mul(unity_WorldToObject, worldPos);
		//flowDirection.x = pow(localWaves.x, 2);
		//flowDirection.y = pow(localWaves.y, 2);
		//flowNormal = AddWaterFlow(IN.LuxUV_MainAOTex.xy, flowDirection, worldNormalFace.y, wetFactor.x, lambda, fadeOutWaterBumps);

		//v0.8
		o.worldPos = worldPos;

		v.vertex = mul(unity_WorldToObject, worldPos);
		//o.normal += worldPos.xyz * flowBumpMYFLOWER.r * _FlowHeight.z;


		o.pos = UnityObjectToClipPos(v.vertex);

		//v0.3 - refract
		o.grabUV = ComputeGrabScreenPos(o.pos);

		o.color = tex2Dlod(_White, v.texcoord).r;
		o.lightDir = normalize(WorldSpaceLightDir(v.vertex));
		o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
		o.texcoord = v.texcoord;

		//v0.5
		o.screenPos = ComputeScreenPos(o.pos);
		COMPUTE_EYEDEPTH(o.screenPos.z);

		//v1.1
		TANGENT_SPACE_ROTATION;
		o.TtoW0 = mul(rotation, unity_ObjectToWorld[0].xyz * 1.0);
		o.TtoW1 = mul(rotation, unity_ObjectToWorld[1].xyz * 1.0);
		o.TtoW2 = mul(rotation, unity_ObjectToWorld[2].xyz * 1.0);

		return o;
	}

	//TUTORIAL
	float4 sssColor(float3 lightDir, float3 normal, float3 view, float height)
	{
		float HA = saturate(height / wavesHeight);
		float light = sqrt(saturate(lightDir.y));
		float sss = pow(saturate(dot(lightDir,view)) + saturate(dot(-lightDir,normal)), sssPower) * light * sssEmit;
		return sssScatter * (sss + HA * 0.62);
	}

	//v0.6
	float4 _CameraDepthTexture_TexelSize;
	float2 AlignWithGrabTexel(float2 uv) {
		#if UNITY_UV_STARTS_AT_TOP
			if (_CameraDepthTexture_TexelSize.y < 0) {
				uv.y = 1 - uv.y;
			}
		#endif
		return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
	}
	
	float4 frag(VertexOutput i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

		//v1.0 - FAST_FLOW
		float4 state = tex2D(_StateTex, i.texcoord);
		clip(WATER_HEIGHT(state) - 0.48);


		//RIVER
		float2 flowCOORD = i.texcoord.xy;

		//_DepthCameraPos
		float offsetPos = 0;
		float offsetScale =1;
		if (_TerrainScale > 0) {
			flowCOORD = (i.worldPos.xz - _DepthCameraPos.xz)*0.015*1.12 * _TerrainScale;//  texcoord.xy;
			offsetPos = 0.5;
			offsetScale = 0.5;
		}
		//v0.2
		float2 flowBumpMYFLOWERC = flowCOORD.xy * float2(fluidSize.z, fluidSize.w) * offsetScale + float2(fluidSize.x + offsetPos, fluidSize.y + offsetPos);

		float4 flowBumpMYFLOWER = tex2D(_WaterFlowRT, float4(flowBumpMYFLOWERC, 0, 0))  * _FlowHeight.x;
		//v0.3
		float4 flowBumpADVECT = tex2D(_WaterFlowADVECT, float4(flowBumpMYFLOWERC, 0, 0))  * _FlowHeight.x;
		if (_TerrainScale > 0 && (flowBumpMYFLOWERC.x < 0 + _clearTextureOffset || flowBumpMYFLOWERC.x > 1 - _clearTextureOffset ||
			flowBumpMYFLOWERC.y < 0 + _clearTextureOffset || flowBumpMYFLOWERC.y > 1 - _clearTextureOffset)
			) {
			flowBumpMYFLOWER = float4(0, 0, 0, 0);
			flowBumpADVECT = float4(0, 0, 0, 0);
		}
		////v0.2
		//float4 flowBumpMYFLOWER = tex2D(_WaterFlowRT, float4(i.texcoord.xy * float2(fluidSize.z, fluidSize.w) + float2(fluidSize.x, fluidSize.y), 0, 0))  * _FlowHeight.x;
		////v0.3
		//float4 flowBumpADVECT = tex2D(_WaterFlowADVECT, float4(i.texcoord.xy * float2(fluidSize.z, fluidSize.w) + float2(fluidSize.x, fluidSize.y), 0, 0))  * _FlowHeight.x;
		//
		
		
		
		
		//flowBumpMYFLOWER.r = flowBumpMYFLOWER.r *0.3 + 0.055 * (flowBumpADVECT.g + flowBumpADVECT.r);
		if (foamDirection.z > 0) {
			flowBumpMYFLOWER.r = flowBumpMYFLOWER.r *0.3 + 0.045 * (flowBumpADVECT.g + flowBumpADVECT.r) * foamDirection.z;
		}
		//flowBumpMYFLOWER.r = pow(flowBumpMYFLOWER.r, 1.2);
		//flowBumpMYFLOWER.r = pow(flowBumpMYFLOWER.r, 2) * 5 + saturate(pow(flowBumpMYFLOWER.r, 1.6)) * 0.14 ;
		//flowBumpMYFLOWER.r = pow(flowBumpMYFLOWER.r, 2.4) * 7 + saturate(pow(flowBumpMYFLOWER.r, 1.6)) * 0.14 ;
		if (foamDirection.w > 0) {
			flowBumpMYFLOWER.r = flowBumpMYFLOWER.r* 0.2 + saturate(pow(flowBumpMYFLOWER.r, 2.4* foamDirection.w) * 5 + saturate(pow(flowBumpMYFLOWER.r, 1.6)) * 0.04)*0.75;
		}

		//v0.9
		float4 foamTexA = 1.4*tex2D(_foamTexture, flowCOORD * _WaterFlowFOAM_ST.xy + _WaterFlowFOAM_ST.zw
			+ float2(-foamDirection.x * 2 * _Time.y + foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r * 0.2,
				-foamDirection.y * 2 * _Time.y + foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r * 0.2)
		);
		/*float4 foamTexB = 0.4 * tex2D(_foamTexture, flowCOORD * _WaterFlowFOAM_ST.xy + _WaterFlowFOAM_ST.zw
			+ float2(-foamDirection.x * 2 * _Time.y + 0.2*foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r * 0,
				-foamDirection.y * 2 * _Time.y + 0.2*foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r * 0)
		);*/

		

		foamTexA = foamTexA * flowBumpADVECT.r*foamControlsA.x;


		//FOAM v0.3a
		//float4 foamTex = tex2D(_WaterFlowFOAM, i.texcoord * _WaterFlowFOAM_ST.xy + _WaterFlowFOAM_ST.zw + float2(0, -2*_Time.y+flowBumpADVECT.r+ flowBumpMYFLOWER.r));
		float4 foamTex = tex2D(_WaterFlowFOAM, flowCOORD * _WaterFlowFOAM_ST.xy + _WaterFlowFOAM_ST.zw
			+ float2(-foamDirection.x * 2 * _Time.y + foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r,
				-foamDirection.y * 2 * _Time.y + foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r)
		);

		//v1.1
		float4 foamTexAAA = tex2D(_WaterFlowFOAM, 1 * _WaterFlowFOAM_ST.xy + _WaterFlowFOAM_ST.zw);
		if ( (abs(i.normalORIGIN.y) < abs(i.normalORIGIN.x) || abs(i.normalORIGIN.y) < abs(i.normalORIGIN.z))) {
		//if (flowBumpADVECT.r ==0 &&  flowBumpMYFLOWER.r == 0 && ( abs(i.normalORIGIN.y) < abs(i.normalORIGIN.x) || abs(i.normalORIGIN.y) < abs(i.normalORIGIN.z))) {
			//foamTex = saturate(foamTex * 5 * abs(i.normal.y)*abs(i.normal.z)*abs(i.normal.x) * 5 * abs(i.normalORIGIN.y)*abs(i.normalORIGIN.x)*abs(i.normalORIGIN.z)) * 4;
			//foamTex = clamp(foamTex * 8 +  flowControl.w*pow(foamTex*abs(i.normal.y), 2), 0, 5);
			if (flowBumpADVECT.r < 0.11 && flowBumpMYFLOWER.r <= 0.11) {
				foamTex = clamp(foamTex * 2 / (flowBumpADVECT.r + 0.01) + 2 * flowControl.w*pow(foamTex*abs(i.normal.y), 2), 0, 5) + pow(foamTexAAA, 2);
			}
			//_Distortion = _Distortion/2;
			_Distortion = _Distortion / 2;
			_DepthFade = _DepthFade / 3;
		}


		if (foamPower.w > 0) {
			flowBumpMYFLOWER.r *= foamPower.w * pow(foamTex.r, 4 * foamPower.x);
		}
		//v0.4
		//float4 foamDirection;
		//float4 foamPower;		

		i.normal = tex2D(_Bump, i.texcoord).rgb;

		//1.0 - FAST_FLOW
		float2 du = float2(_StateTex_TexelSize.x * 0.5, 0);
		float2 dv = float2(0, _StateTex_TexelSize.y * 0.5);
		float4 state_l = tex2D(_StateTex, i.texcoord + du);
		float4 state_r = tex2D(_StateTex, i.texcoord - du);
		float4 state_t = tex2D(_StateTex, i.texcoord + dv);
		float4 state_b = tex2D(_StateTex, i.texcoord - dv);
		half dhdu = _NormalStrength * 0.5 * (FULL_HEIGHT(state_r) - FULL_HEIGHT(state_l));
		half dhdv = _NormalStrength * 0.5 * (FULL_HEIGHT(state_b) - FULL_HEIGHT(state_t));
		float3 normalFASTFLOW = float3(dhdu, 1, dhdv);
		i.normal = i.normal * normalFASTFLOW + normalFASTFLOW;

		i.normal = normalize(i.normal);
		i.normal = UnityObjectToWorldNormal(i.normal);

		//v0.7		
		float4 localWaves = tex2D(_WaterFlowRTA, float4(flowCOORD.x * _LocalWavesTiling.x + _LocalWavesTiling.z, flowCOORD.y* _LocalWavesTiling.y + _LocalWavesTiling.w, 0, 0)) ;
		//i.normal += pow(localWaves.r * 3, 5);
		i.normal += clamp(pow(localWaves.r * 3, 5),-5,6);


		//v1.1
		// calculate world normal
		half3 normalB = ((UnpackNormal(tex2D(_HeightMap, i.texcoord*float2(_HeightMap_ST.x, _HeightMap_ST.y)* (flowBumpADVECT.r+0.99-flowControl.z) +float2(_Time.y*(_HeightMap_ST.z),_Time.y*(_HeightMap_ST.w))))));
		//normalB = normalB * flowBumpADVECT.r + flowBumpMYFLOWER.r;
		half3 worldNormal;
		worldNormal.x = dot(i.TtoW0, normalB.xyz);
		worldNormal.y = dot(i.TtoW1, normalB.xyz);
		worldNormal.z = dot(i.TtoW2, normalB.xyz);
		worldNormal = normalize(worldNormal);
		i.normal = i.normal + worldNormal * flowControl.x;
		//i.normal = i.normal + flowCOORD.x * worldNormal * 10.85* flowBumpADVECT.r + flowBumpMYFLOWER.r;//i.normal = i.normal+worldNormal*1;
		//foamTexA = foamTexA * 1000;
		////v1.1
		if (abs(i.normalORIGIN.y) < abs(i.normalORIGIN.x)  || abs(i.normalORIGIN.y) < abs(i.normalORIGIN.z)) {
			i.normal = i.normal * 5 * flowControl.y;
		}

		//v0.8
		// Add Water Ripples
		float fadeOutWaterBumps = saturate((_Lux_WaterBumpDistance - distance(_WorldSpaceCameraPos, i.worldPos)) / 5);
		//float3 worldNormalFace = WorldNormalVector(i, float3(0, 0, 1));
		float3 worldNormalFace = mul(unity_ObjectToWorld, float4(i.normal, 0.0)).xyz;
		if (_Lux_RainIntensity > 0) {
			//rippleNormal = AddWaterFlowRipples(wetFactor, IN.worldPos, lambda, saturate(worldNormalFace.y), fadeOutWaterBumps);
			float wetFactor = 11 * _Lux_WaterFloodlevel.x;
			float2 lambda = float2(_Lux_WaterFloodlevel.y, _Lux_WaterFloodlevel.z);
			//i.normal += AddWaterFlowRipples(wetFactor, i.worldPos, lambda, saturate(worldNormalFace.y), fadeOutWaterBumps);
			i.normal += _Lux_WaterFloodlevel.w * AddWaterFlowRipples(wetFactor, i.worldPos, lambda, saturate(worldNormalFace.y), fadeOutWaterBumps);
		}

		//v0.7	
		//i.normal = normalize(i.normal);
		flowBumpMYFLOWER.r = saturate(flowBumpMYFLOWER.r - saturate(pow(localWaves.r*2,3)));
		//flowBumpMYFLOWER.r = flowBumpMYFLOWER.r*0.1 + pow(localWaves.r * 1 * i.normal.y, 8)*0.1;
		flowBumpMYFLOWER.r = saturate(flowBumpMYFLOWER.r*1 + saturate(pow(localWaves.r * 0.6 * i.normal.y, 5)*0.0002));
		//_RimColor += pow(localWaves.r * 1, 5)*0.1;
		//i.normal = UnityObjectToWorldNormal(i.normal);


		float4 foamTex1 = tex2D(_WaterFlowFOAM, 10 * flowCOORD * _WaterFlowFOAM_ST.xy + _WaterFlowFOAM_ST.zw
			+ 0.05*float2(-foamDirection.x * 4 * _Time.y + foamPower.z * flowBumpADVECT.r + flowBumpMYFLOWER.r,
				-foamDirection.y * 174 * _Time.y + foamPower.z * 1 + flowBumpMYFLOWER.r* i.normal.y)
		);

		//foamTex = foamTex * foamControlsA.z + 0.25*foamTexA*foamControlsA.y;//v0.9
		foamTex1 = saturate(foamTex1 * foamControlsA.z + 0.25*foamTexA*(foamControlsA.y-1));//v0.9

		flowBumpMYFLOWER.r += pow(saturate(flowBumpADVECT.r - localWaves.r * foamTex1.r), refractPower.w) * 10 * refractPower.z * foamControlsA.w;
		flowBumpMYFLOWER.r += pow(saturate(flowBumpADVECT.r - localWaves.r + foamTex1.r), refractPower.w) * 10 * refractPower.z * foamControlsA.w;


		float4 diffuse = saturate(dot(i.normal, i.lightDir));
		diffuse = pow(saturate(diffuse * (1 - _LightWrap) + _LightWrap), 2 * _LightWrap + 1) * _Tint * _LightColor0;
		i.viewDir = normalize(i.viewDir);
		i.lightDir = normalize(i.lightDir);
		float3 H = normalize(i.viewDir + i.lightDir);
		float NdotH = saturate(dot(i.normal, H));
		float4 specular = _SpecColor * saturate(pow(NdotH, _Glossiness)) * _LightColor0;
		float4 rim = _RimColor * pow(max(0, 1 - saturate(dot(i.normal, i.viewDir))), 1.5);
		//return saturate(pow(NdotH, _Glossiness));
		//return diffuse + specular + pow(i.color / 2, 2) + rim; //v0.1

		//v0.2
		float normalEffect = (dot(float3(0, 1, 0), (i.normal)))*1.55;
		float4 fluid = float4(1, 1, 1, 1)*saturate(flowBumpMYFLOWER.r)*0.75*(pow(saturate(normalEffect), 9));
		if (saturate(flowBumpMYFLOWER.r) < 0.35) {
			fluid = 0;
		}
		float4 foam = pow(i.color / 2, 2);
		float4 sssColorA = sssColor(i.lightDir, i.normal, -i.viewDir, i.pos.y);
		float4 finalA = diffuse + specular + foam + rim
			+ sssColorA// sssColor(i.lightDir, i.normal, -i.viewDir, i.pos.y)
			+ saturate(flowBumpMYFLOWER.r)*0.75;
		//+ saturate(flowBumpMYFLOWER.r)*0.75*(0.15 + 1.5*saturate(i.normal.x) + 0.2*saturate(i.normal.y));
		//+ saturate(flowBumpMYFLOWER.r)*0.75*(0.01 +pow(saturate(normalEffect),2))*saturate(1/foam);
		//+fluid;
		if ((flowBumpMYFLOWER.r) == 0) {
			//finalA = float4(0.6, 0.3, 0.4, 1);
			finalA = float4(0.96, 0.93, 0.94, 1);
		}

		////v1.1
		//if (abs(i.normalORIGIN.y) > 0.99) {
		//	foamTex = foamTex * 5;
		//}
		//if(abs(i.normal.y)	 < 0.9){
		////if (abs(i.normal.x) > 0.999 || abs(i.normal.z) > 0.999) {
		//	//foamTex = foamTex * 1011;
		//	if (abs(i.normal.x) > 0.99 || abs(i.normal.z) > 0.99) {
		//		foamTex = foamTex * 1011;
		//	}
		//}
		//else {
		//	//foamTex = foamTex * 1011;
		//}
		////v1.1
		//if (abs(i.normalORIGIN.y) < abs(i.normalORIGIN.x) || abs(i.normalORIGIN.y) < abs(i.normalORIGIN.z)) {
			//foamTex = foamTex * 5 *abs( i.normal.y)/ abs(i.normal.z);
		//}
		//if (abs(i.normalORIGIN.y) < abs(i.normalORIGIN.x) || abs(i.normalORIGIN.y) < abs(i.normalORIGIN.z)) {
		//	//foamTex = saturate(foamTex * 5 * abs(i.normal.y)*abs(i.normal.z)*abs(i.normal.x) * 5 * abs(i.normalORIGIN.y)*abs(i.normalORIGIN.x)*abs(i.normalORIGIN.z)) * 4;
		//	foamTex = foamTex + flowControl.w*pow(foamTex*abs(i.normal.y),2);
		//}
		//finalA = finalA -9*foamTex;

		//v0.3 - refract
		half2 offset = i.normal.xz  * _GrabTexture_TexelSize.xy * _Distortion * 1000;// worldNormal.xz * _GrabTexture_TexelSize.xy * _Distortion;
		half4 grabUV = half4(offset * i.grabUV.z + i.grabUV.xy, i.grabUV.zw);

		//v0.6 - correct refractions for objects above water
		//https://catlikecoding.com/unity/tutorials/flow/looking-through-water/
		//float2 uvOffset = tangentSpaceNormal.xy * _RefractionStrength;
		/*float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.screenPos.z);
		grabUV.xy = AlignWithGrabTexel((i.screenPos.xy + offset) / i.screenPos.w);
		float depthDifference = backgroundDepth - surfaceDepth;
		if (depthDifference < 0) {
			grabUV.xy = AlignWithGrabTexel(i.screenPos.xy / i.screenPos.w);
		}*/


		float4 refraction = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(grabUV));

		//v0.5
		half4 screenUV = half4(offset * i.screenPos.z + i.screenPos.xy, i.screenPos.zw);
		float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
		float waterDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabUV)));// screenUV)));
		refraction.a = saturate(_DepthAmount / abs(waterDepth - screenUV.z));//refraction.a = saturate(_DepthAmount / abs(waterDepth - screenUV.z));
		half4 cleanRefraction = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.grabUV)); //v0.6 - fix refract artifacts on above water objects
		cleanRefraction.a = saturate(_DepthAmount / abs(sceneDepth - i.screenPos.z));
		refraction = lerp(cleanRefraction, refraction, step(screenUV.z, waterDepth));
																			 //#if _DEPTHFOG_ON
																			 //refraction.rgb = lerp(_ShallowColor.rgb * refraction.rgb, _DeepColor.rgb, 1 - refraction.a);
																			 //#else
																			 //fixed3 finalColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, 1 - refraction.a) * refraction.rgb;
																			 //refraction.rgb = lerp(_ShallowColor.rgb, _DeepColor.rgb, 1 - refraction.a) * refraction.rgb;
																			 //#endif


		//finalA = finalA * 0.75*refractPower.x + finalA * refraction*0.5*refractPower.y;
		finalA = finalA * 0.75*refractPower.x + saturate(finalA) * saturate(refraction)*0.5*refractPower.y;

		//v0.5
		//finalA.rgb = lerp(_ShallowColor.rgb * finalA.rgb*2, _DeepColor.rgb *finalA.rgb, 1 - refraction.a);//FOG
		_ShallowColor.rgb = lerp(_clearColor, _ShallowColor.rgb, pow(1 - refraction.a +_clearColorDepth, _clearColorPower)); //v0.6 - make water more clear near surface
		finalA.rgb = lerp(_ShallowColor.rgb * finalA.rgb * shallowPower, (1-_DepthFade) * (_DeepColor.rgb+sssColorA) + _DepthFade * finalA.rgb, 1 - refraction.a);
		//finalA.rgb = lerp(_ShallowColor.rgb, _DeepColor.rgb, 1 - refraction.a) * finalA.rgb;//NO FOG
		_ShallowColor = saturate(_ShallowColor);

		//reflect
		half3 worldReflect = reflect(-i.viewDir, i.normal);
		half3 cubeMap = texCUBE(_CubeMap, worldReflect).rgb * _CubeTint.rgb;
		half3 finalReflection = cubeMap;
		half NdotV = saturate(1 - dot(i.normal, i.viewDir));
		half fresnel = pow(NdotV, _FresnelAngle);
		//finalColor = lerp(finalColor, finalReflection + finalColor, fresnel * _ReflectionStrength);
		finalA.rgb = lerp(finalA.rgb, finalReflection + finalA.rgb, saturate(fresnel * _ReflectionStrength));

		//v0.3a
		//_WaterFlowFOAM
		finalA.rgb += foamTex.rgb * flowBumpMYFLOWER.r * 1.4 * foamPower.y;

		finalA = saturate(finalA);
		return finalA; //foamTexA;// finalA;
	}

		ENDCG
	}
	}
}