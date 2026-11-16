// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PBDFluid/VolumeFluid3D A"
{
	Properties
	{
		AbsorptionCoff("Absorption Coff", Vector) = (0.45, 0.029, 0.018)
		lightDir("lightDir", Vector) = (1, 1, 1)
		AbsorptionScale("Absorption Scale", Range(0.01, 10)) = 1.5
		RenderDensities("Render Densities", Vector) = (1,1,1,1)

		//v0.2
		_Shininess("Shininess",  Range(0, 3)) = 0.5
		_SpecularValues("Specular Intensity", Vector) = (12, 768, 0.15)
		_RefractionValues("Refraction/Reflection", Vector) = (0.3, 0.01, 1.0)
		_RefractionScale("Refraction Scale",  Float) = 0.005// Range(0, 0.03)) = 0.005
		 _InvFade("Soft Factor", Range(0.01,3.0)) = 1.0
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		GrabPass { "BackGroundTexture" }

		cull front
		ZWrite Off
		//ZWrite On
		ztest always//ZTest LEqual//ztest always
		blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc" 
	
			#include "Lighting.cginc"

			#define NUM_SAMPLES 16 //8//64 //32

			float AbsorptionScale;
			float4 AbsorptionCoff;
			float3 Translate, Scale, Size;
			sampler3D Volume;
			sampler2D BackGroundTexture;

			//v0.1
			float3 lightDir;
			sampler2D normalTex;
			sampler2D rampTex;

			//v0.3
			sampler2D_float _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;
			float _InvFade;

			//v0.2
			float4 RenderDensities;
			// lightDir, eyeDir and normal in world space
			//https://github.com/tuxalin/water-shader - MIT
			//v0.2
			uniform float _Shininess;
			// xy = Specular intensity values, z = shininess exponential factor.
			uniform float3 _SpecularValues;
			uniform float2 _RefractionValues;
			// Amount of wave refraction, of zero then no refraction. 
			uniform float _RefractionScale;
			// refractionValues, x = index of refraction constant, y = refraction strength
			// normal and eyeVec in world space
			half FresnelValue(float2 refractionValues, float3 normal, float3 eyeVec)
			{
				// R0 is a constant related to the index of refraction (IOR).
				float R0 = refractionValues.x;
				// This value modifies current fresnel term. If you want to weaken
				// reflections use bigger value.
				float refractionStrength = refractionValues.y;
#ifdef SIMPLIFIED_FRESNEL
				return R0 + (1.0f - R0) * pow(1.0f - dot(eyeVec, normal), 5.0f);
#else		
				float angle = 1.0f - saturate(dot(normal, eyeVec));
				float fresnel = angle * angle;
				fresnel *= fresnel;
				fresnel *= angle;
				return saturate(fresnel * (1.0f - saturate(R0)) + R0 - refractionStrength);
#endif // #ifdef SIMPLIFIED_FRESNEL
			}
			half3 ReflectedRadiance(float shininess, half3 specularValues, half3 lightColor, float3 lightDir, float3 eyeDir, float3 normal, float fresnel)
			{
				float shininessExp = specularValues.z;

#ifdef BLINN_PHONG
				// a variant of the blinn phong shading
				float specularIntensity = specularValues.x * 0.0075;

				float3 H = normalize(eyeDir + lightDir);
				float e = shininess * shininessExp * 800;
				float kS = saturate(dot(normal, lightDir));
				half3 specular = kS * specularIntensity * pow(saturate(dot(normal, H)), e) * sqrt((e + 1) / 2);
				specular *= lightColor;
#else
				float2 specularIntensity = specularValues.xy;
				// reflect the eye vector such that the incident and emergent angles are equal
				float3 mirrorEye = reflect(-eyeDir, normal);
				half dotSpec = saturate(dot(mirrorEye, lightDir) * 0.5f + 0.5f);
				half3 specular = (1.0f - fresnel) * saturate(lightDir.y) * pow(dotSpec, specularIntensity.y) * (shininess * shininessExp + 0.2f) * lightColor;
				specular += specular * specularIntensity.x * saturate(shininess - 0.05f) * lightColor;
#endif // #ifdef BLINN_PHONG
				return specular;
			}



			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 grabPos : TEXCOORD1;
				float4 screenPos: TEXCOORD2;
				float eyeDepth : TEXCOORD3;
				float3 camRelativeWorldPos:  TEXCOORD4;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				//OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				OUT.grabPos = ComputeGrabScreenPos(OUT.pos);
				OUT.screenPos = ComputeScreenPos(OUT.pos);
				COMPUTE_EYEDEPTH(OUT.eyeDepth);
				//https://forum.unity.com/threads/reconstructing-world-space-position-from-depth-texture.1139599/
				OUT.camRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz - _WorldSpaceCameraPos;
				return OUT;
			}

			struct Ray 
			{
				float3 origin;
				float3 dir;
			};

			struct AABB 
			{
				float3 Min;
				float3 Max;
			};

			//find intersection points of a ray with a box
			bool IntersectBox(Ray r, AABB aabb, out float t0, out float t1)
			{
				float3 invR = 1.0 / r.dir;
				float3 tbot = invR * (aabb.Min - r.origin);
				float3 ttop = invR * (aabb.Max - r.origin);
				float3 tmin = min(ttop, tbot);
				float3 tmax = max(ttop, tbot);
				float2 t = max(tmin.xx, tmin.yz);
				t0 = max(t.x, t.y);
				t = min(tmax.xx, tmax.yz);
				t1 = min(t.x, t.y);
				return t0 <= t1;
			}
			
			//v0.3
			bool depthIsNotSky(float depth)
			{
#if defined(UNITY_REVERSED_Z)
				return (depth > 0.0);
#else
				return (depth < 1.0);
#endif
			}
			float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth)
			{
				float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);

#if UNITY_UV_STARTS_AT_TOP
				// Our world space, view space, screen space and NDC space are Y-up.
				// Our clip space is flipped upside-down due to poor legacy Unity design.
				// The flip is baked into the projection matrix, so we only have to flip
				// manually when going from CS to NDC and back.
				positionCS.y = -positionCS.y;
#endif

				return positionCS;
			}
			float3 ComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)
			{
				float4 positionCS = ComputeClipSpacePosition(positionNDC, deviceDepth);
				float4 hpositionWS = mul(invViewProjMatrix, positionCS);
				return hpositionWS.xyz / hpositionWS.w;
			}
			float3 ComputeWorldSpacePosition(float4 positionCS, float4x4 invViewProjMatrix)
			{
				float4 hpositionWS = mul(invViewProjMatrix, positionCS);
				return hpositionWS.xyz / hpositionWS.w;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				float3 pos = _WorldSpaceCameraPos;
				//float3 grab = tex2Dproj(BackGroundTexture, IN.grabPos).rgb;

				Ray r;
				r.origin = pos;
				r.dir = normalize(IN.worldPos - pos);

				AABB aabb;
				aabb.Min = float3(-0.5,-0.5,-0.5) * Scale + Translate;
				aabb.Max = float3(0.5,0.5,0.5) * Scale + Translate;

				//figure out where ray from eye hit front of cube
				float tnear, tfar;
				IntersectBox(r, aabb, tnear, tfar);

				//if eye is in cube then start ray at eye
				if (tnear < 0.0) tnear = 0.0;

				float3 rayStart = r.origin + r.dir * tnear;
				float3 rayStartA = r.origin + r.dir * tnear;
				rayStartA -= Translate;
				rayStartA = (rayStartA + 0.5 * Scale) / Scale;

				float3 rayStop = r.origin + r.dir * tfar;

				//convert to texture space
				rayStart -= Translate;
				rayStop -= Translate;
				rayStart = (rayStart + 0.5 * Scale) / Scale;
				rayStop = (rayStop + 0.5 * Scale) / Scale;

				float3 start = rayStart;
				float dist = distance(rayStop, rayStart);
				float stepSize = dist / float(NUM_SAMPLES);
				float3 ds = normalize(rayStop - rayStart) * stepSize;

				//v0.3- depth
				float2 uvA = IN.screenPos.xy / IN.screenPos.w;
				float depthA = 1 - Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvA));
				float rawZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvA);// SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(uvA));
				float sceneZ = LinearEyeDepth(rawZ);
				float partZ = IN.eyeDepth;
				float fade = 1.0;
				if (rawZ > 0.0) // Make sure the depth texture exists
				{
					fade = saturate(_InvFade * (sceneZ - partZ));
				}
				///https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@11.0/manual/writing-shaders-urp-reconstruct-world-position.html
				//float3 worldPos = ComputeWorldSpacePosition(uvA, depthA, _InvViewProjMatrix);// UNITY_MATRIX_I_VP);
				//return float4(depthA, depthA, depthA, 1);
				//return float4(partZ, partZ, partZ, 0.5);
				//return float4(depthA.xxx*0.5, 1);
				//return float4(sceneZ.xxx, 1);

				//v0.3a
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				// sample depth texture
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
				// get linear depth from the depth
				float sceneZA = LinearEyeDepth(depth);
				// calculate the view plane vector
				// note: Something like normalize(i.camRelativeWorldPos.xyz) is what you'll see other
				// examples do, but that is wrong! You need a vector that at a 1 unit view depth, not
				// a1 unit magnitude.
				float3 viewPlane = IN.camRelativeWorldPos.xyz / dot(IN.camRelativeWorldPos.xyz, unity_WorldToCamera._m20_m21_m22);
				// calculate the world position
				// multiply the view plane by the linear depth to get the camera relative world space position
				// add the world space camera position to get the world space position from the depth texture
				float3 worldPos = viewPlane * sceneZA +_WorldSpaceCameraPos;
				//worldPos = mul(unity_CameraToWorld, float4(worldPos, 1.0));
				float3 worldPosA = worldPos;
				worldPosA -= Translate;
				worldPosA = (worldPosA + 0.5 * Scale) / Scale;
				//half4 colA = 0;
				// draw grid where it's not the sky
				//if (depthIsNotSky(depth)) {
					//colA.rgb = saturate(2.0 - abs(frac(worldPos) * 2.0 - 1.0) * 100.0);
				//}
				//return float4(colA.rgb,1);
				
				//accumulate density though volume along ray
				float density = 0;
				int foundNormal = 0;//v0.1
				float3 normal = 0;
				float fluidHeight = 0;
				for (int i = 0; i < NUM_SAMPLES; i++, start += ds)
				{
					float4 sampleInVolume = tex3D(Volume, start);
					//density += sampleInVolume.x * RenderDensities.x;// +fade;

					if(length(worldPosA - rayStartA) < length(start - rayStartA)){ //length(start) > (1 - depthA)*_RefractionScale) {
						break;
					}
					else {
						density += sampleInVolume.x * RenderDensities.x;// +fade;
						if (foundNormal == 0 && density > 0) {
							//found surface, calc normal
							//normal = sampleInVolume.yzw;
							normal += 0.05*sampleInVolume.yzw;
							//foundNormal = 1;

							//find max height
							if (start.y > fluidHeight) {
								fluidHeight = start.y;
							}
						}
					}

					
					/*if (density > 0) {
						density = density * 1111100;
						break;
					}*/
					//if ( length(_WorldSpaceCameraPos + start) > rawZ) {
					//	break;
					//}
					//f (sceneZ - partZ < length(start)) {
						//break;
					//}
					
					
				}

				//float3 lightDir = lightDir;
				float dotOfLight = dot(lightDir, normal);
				int normalPower =  2;
				float3 grab = tex2Dproj(BackGroundTexture, IN.grabPos 
					+ float4(density*pow(normal.x* _RefractionScale, normalPower), density*pow(normal.y* _RefractionScale, normalPower),0,0)).rgb;
				float3 grab2 = tex2Dproj(BackGroundTexture, IN.grabPos + RenderDensities.z
					* float4(density*pow(normal.x* _RefractionScale, normalPower) , density*pow(normal.y* _RefractionScale, normalPower) , 0, 0)).rgb;
				float denisty_glow = saturate(grab / (density*0.99));
				if (density <1) {
					denisty_glow = 0;
				}
				//float3 col = grab * exp(-AbsorptionCoff * density * AbsorptionScale)/2 + grab2/2 + (denisty_glow)*(denisty_glow)*1;

				//v0.2
				float3 col = grab * exp(-AbsorptionCoff * density * AbsorptionScale) / 2 + grab2 * exp(-AbsorptionCoff * density * AbsorptionScale) / 2 
					+ 0.1 * denisty_glow* denisty_glow* RenderDensities.y - 0.01*density*float3(1,1,1)*AbsorptionCoff.w* exp(-AbsorptionCoff * density * AbsorptionScale);
				//float3 col =grab * exp(-AbsorptionCoff * density * AbsorptionScale) / 2 + grab2 / 2 + 0.1 * denisty_glow* denisty_glow* RenderDensities.y;

				//return float4(col + 0.18*saturate(length(normal)*length(normal))*float3(1,1,1),1);
				//return float4(col + 2*pow(fluidHeight-0.2,2) +0.2*float3(1,1,1)* saturate(dotOfLight)*0.2, 1);

				/*float power = 0.3;
				if (fluidHeight == 0) {
					fluidHeight = 1 + 0.14;
					power = 1;
				}*/
				//return float4(col, 1);
				//return float4((saturate(col * pow(fluidHeight-0.14, power))/2 + col/2), 1);

				//v0.2
				//https://github.com/tuxalin/water-shader - MIT
				float3 eyeDir = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos);
				half3 lightColor = _LightColor0.rgb;
				float3 lightDir = normalize(_WorldSpaceLightPos0);
				half fresnel = FresnelValue(_RefractionValues, normal, eyeDir);
				half3 specularColor = ReflectedRadiance(_Shininess, _SpecularValues, lightColor, lightDir, eyeDir, normal, fresnel);
				//half3 reflectColor = fresnel * MeanSkyRadiance(_SkyTexture, eyeDir, normal) * _RadianceFactor;

				//return float4(rawZ, rawZ, rawZ, 0.5);

				float power = 0.3;
				return float4(col +saturate(dotOfLight)*0.2 + specularColor, 1);

			}
			ENDCG
		}
	}
}



////find intersection points of a ray with a box
//bool IntersectBox(Ray r, AABB aabb, out float t0, out float t1)
//{
//	float3 invR = 1.0 / r.dir;
//	float3 tbot = invR * (aabb.Min - r.origin);
//	float3 ttop = invR * (aabb.Max - r.origin);
//	float3 tmin = min(ttop, tbot);
//	float3 tmax = max(ttop, tbot);
//	float2 t = max(tmin.xx, tmin.yz);
//	t0 = max(t.x, t.y);
//	t = min(tmax.xx, tmax.yz);
//	t1 = min(t.x, t.y);
//	return t0 <= t1;
//}
//
//fixed4 frag(v2f IN) : SV_Target
//{
//	float3 pos = _WorldSpaceCameraPos;
//	float3 grab = tex2Dproj(BackGroundTexture, IN.grabPos).rgb;
//
//	Ray r;
//	r.origin = pos;
//	r.dir = normalize(IN.worldPos - pos);
//
//	AABB aabb;
//	aabb.Min = float3(-0.5,-0.5,-0.5) * Scale + Translate;
//	aabb.Max = float3(0.5,0.5,0.5) * Scale + Translate;
//
//	//figure out where ray from eye hit front of cube
//	float tnear, tfar;
//	IntersectBox(r, aabb, tnear, tfar);
//
//	//if eye is in cube then start ray at eye
//	if (tnear < 0.0) tnear = 0.0;
//
//	float3 rayStart = r.origin + r.dir * tnear;
//	float3 rayStop = r.origin + r.dir * tfar;
//
//	//convert to texture space
//	rayStart -= Translate;
//	rayStop -= Translate;
//	rayStart = (rayStart + 0.5 * Scale) / Scale;
//	rayStop = (rayStop + 0.5 * Scale) / Scale;
//
//	float3 start = rayStart;
//	float dist = distance(rayStop, rayStart);
//	float stepSize = dist / float(NUM_SAMPLES);
//	float3 ds = normalize(rayStop - rayStart) * stepSize;
//
//	//accumulate density though volume along ray
//	float density = 0;
//	for (int i = 0; i < NUM_SAMPLES; i++, start += ds)
//	{
//		density += tex3D(Volume, start).x;
//	}
//
//	float3 col = grab * exp(-AbsorptionCoff * density * AbsorptionScale);
//
//	return float4(col, 1);
//}