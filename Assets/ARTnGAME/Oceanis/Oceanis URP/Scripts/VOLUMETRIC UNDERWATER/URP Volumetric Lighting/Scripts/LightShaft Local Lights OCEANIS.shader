Shader "Hidden/ARTnGAME/Oceanis/LightShaftLocalLightsOCEANIS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		//v1.9.9.5 - Ethereal v1.1.8
		_visibleLightsCount("_visible Lights Count", Int) = 1
    }
    
	HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

	//v0.1
	//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
	#define URP11

	//v0.3
	#define URP_TEXTURE_WRAP_MODE_REPEAT       0
	#define URP_TEXTURE_WRAP_MODE_CLAMP        1
	
	#pragma multi_compile _ _ADDITIONAL_LIGHTS
	#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

    TEXTURE2D_X(_MainTex);
    TEXTURE2D_X_FLOAT(_CameraDepthTexture);
    SAMPLER(sampler_CameraDepthTexture);

    TEXTURE2D_X(_LightShaftTempTex);

    half4 _MainTex_ST;
    float4 _MainTex_TexelSize;

    float4 _CamWorldSpace;
    float4x4 _CamFrustum, _CamToWorld;
    int _MaxIterations;
    float _MaxDistance;
    float _MinDistance;

    float _Intensity;

	//v0.1
	//v1.9.9.5 - Ethereal v1.1.8
	int _visibleLightsCount;

	//v0.1
	float cutoffHeigth;

	//v0.2
	TEXTURE2D_X(_CookieTex);
	half4 _CookieTex_ST;
	float4 _CookieTex_TexelSize;
	float customCookieIntensity;
	float customCookieIntensityA;
	float lightCookieIntensity;
	float customCookieScaler;
	float customCookieScalerA;

    struct RayVaryings
    {
        float4 positionCS    : SV_POSITION;
        float2 uv            : TEXCOORD0;
        float4 ray           : TEXCOORD1;
    };

	//v0.1
	struct AttributesA
	{
		float4 positionOS    : POSITION;
		float2 uv            : TEXCOORD0;
	};

    RayVaryings Vert_Ray(AttributesA input)
    {
        RayVaryings output;
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        output.uv = input.uv;

        int index = output.uv.x + 2 * output.uv.y;
        output.ray = _CamFrustum[index];
        
        return output;
    }

    float GetRandomNumber(float2 texCoord, int Seed)
    {
        return frac(sin(dot(texCoord.xy, float2(12.9898, 78.233)) + Seed) * 43758.5453);
    }


	//v0.3
	////float4x4 _MainLightWorldToLight;
	//float2 ComputeLightCookieUVDirectional(float4x4 worldToLight, float3 samplePositionWS, float4 atlasUVRect, uint2 uvWrap)
	//{
	//	// Translate and rotate 'positionWS' into the light space.
	//	// Project point to light "view" plane, i.e. discard Z.
	//	float2 positionLS = mul(worldToLight, float4(samplePositionWS, 1)).xy;

	//	// Remap [-1, 1] to [0, 1]
	//	// (implies the transform has ortho projection mapping world space box to [-1, 1])
	//	float2 positionUV = positionLS * 0.5 + 0.5;

	//	// Tile texture for cookie in repeat mode
	//	positionUV.x = (uvWrap.x == URP_TEXTURE_WRAP_MODE_REPEAT) ? frac(positionUV.x) : positionUV.x;
	//	positionUV.y = (uvWrap.y == URP_TEXTURE_WRAP_MODE_REPEAT) ? frac(positionUV.y) : positionUV.y;
	//	positionUV.x = (uvWrap.x == URP_TEXTURE_WRAP_MODE_CLAMP) ? saturate(positionUV.x) : positionUV.x;
	//	positionUV.y = (uvWrap.y == URP_TEXTURE_WRAP_MODE_CLAMP) ? saturate(positionUV.y) : positionUV.y;

	//	// Remap to atlas texture
	//	float2 positionAtlasUV = atlasUVRect.xy * float2(positionUV)+atlasUVRect.zw;

	//	return positionAtlasUV;
	//}


    half4 SimpleRaymarching(float3 rayOrigin, float3 rayDirection, float depth)
    {
		half4 result = float4(_MainLightColor.xyz, 1) *0.2;
        
        float step = _MaxDistance / _MaxIterations;
        float t = _MinDistance + step * GetRandomNumber(rayDirection, _Time.y * 100);
        // float t = _MinDistance;
        float alpha = 0;

		[loop]
        for(int i = 0; i < _MaxIterations; i++)
        {
            if(t > _MaxDistance || t >= depth)
            {
                break;
            }
            
            float3 p = rayOrigin + rayDirection * t;
            // float d = DistanceField(p);

            float4 shadowCoord = TransformWorldToShadowCoord(p);
			
			//v0.1
			//COOKIE
#ifdef URP11
			float3 cookieDirColor = SampleMainLightCookie(float3(p.x, p.y*0.2, p.z)  - 0.25*(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z));
			//float3 cookieDirColor = SampleMainLightCookie(float3(p.x, 0, p.z));// -float3(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z)); //SampleMainLightCookie(p);// shadowCoord.rgb);
#endif

            float shadow = SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord);

			//COOKIE 	//v0.1
			//shadow = shadow +  cookieDirColor;

            if(shadow >= 1)
            {
               // alpha += step * 0.005;
				//alpha = -0.1;
				alpha += step * 0.1;// -(1 - shadow);
            }

			//COOKIE 	//v0.1
			half4 colorCOOKIE = SAMPLE_TEXTURE2D_X(_CookieTex, sampler_LinearClamp, shadowCoord.xy*customCookieScaler);// shadowCoord.xy*_CookieTex_ST.xy*0.1 + _CookieTex_ST.zw);
			//float shadowCOOKIE = SAMPLE_TEXTURE2D_SHADOW(_CookieTex, sampler_MainLightShadowmapTexture, shadowCoord*customCookieScaler);
			alpha = alpha - 0.2*alpha * colorCOOKIE.rgb * customCookieIntensity;//

			if (customCookieIntensityA != 0) {

				float2 uv = ComputeLightCookieUVDirectional(_MainLightWorldToLight, float3(p.x, p.y*0.05, p.z) - 1*(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z), float4(1, 1, 0, 0), URP_TEXTURE_WRAP_MODE_REPEAT);
				float4 shadowCoordA = TransformWorldToShadowCoord(float3(p.x, p.y*0.5, p.z));// float3(p.x, p.y*0.2, p.z) - 0.25*(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z));
				//float2 coords = shadowCoordA.xy*customCookieScalerA;

				//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
				float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
				float2 coords = shadowCoordA.xy*customCookieScalerA;// -0.1* float2(forward.z*_CamWorldSpace.x + forward.x*_CamWorldSpace.z, 0);// -0.01* float2(_CamWorldSpace.x, 0);
				coords = coords - 0.1* float2(1*_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.z);
				if (coords.x > 1 || coords.x < 0) {
					coords.x = coords.x - 1 * floor(coords.x / 1);
				}
				if (coords.y > 1 || coords.y < 0) {
					coords.y = coords.y - 1 * floor(coords.y / 1);
				}
				
				half4 colorCOOKIEA = SAMPLE_TEXTURE2D_X(_CookieTex, sampler_LinearClamp, coords);
				//half4 colorCOOKIEA = SAMPLE_TEXTURE2D_X(_CookieTex, sampler_LinearClamp,uv.xy);

				alpha =( alpha - 0.2*alpha * pow(colorCOOKIEA.rgb,5) * customCookieIntensityA);//
			}

#ifdef URP11			
			alpha = alpha - 0.2*alpha * cookieDirColor * lightCookieIntensity;// *saturate(0.001*depth);
#endif

            t += step;
        }






		//v0.1 - LOCAL LIGHTS
		int farQuality = 1;
		int farDiastanceMult = 1;
		float stepA = _MaxDistance* farDiastanceMult / _MaxIterations / farQuality;
		t = _MinDistance + stepA * GetRandomNumber(rayDirection, _Time.y * 100);
		//if (lightControlA.y != 0) 
		{
			half distanceAtten = 0;
#ifdef _ADDITIONAL_LIGHTS			
			//[unroll]
			//for (uint k = 0u; k < 1; ++k);// for (int k = 0; k < 3; ++k); //for (int k = 0; k < pixelLightCount; ++k)
			{
				//float3 stepA = ray * stepLength;
				float3 pA = rayOrigin + rayDirection * t;
				float3 pos = pA;// rayStart + stepA;

				//float distToRayStart = length(rayStart - pos);
				//float steps2 = steps; //+ 10*pow(1/distToRayStart, 3)*_stepsControl.x; //v1.5

				//float powDIVIDE = (pow(distToRayStart, 0.7*_stepsControl.y)*_stepsControl.z) * lightControlA.y; //v1.9.9

				int steps2 = _MaxIterations * farDiastanceMult;// 20;

				[loop]
				for (int m = 0; m < steps2; ++m)
				{

					if (t > _MaxDistance * farDiastanceMult || t >= depth)
					{
						break;
					}

					[loop] //v1.9.9.8 - Ethereal v1.1.8h
					for (int k = 0; k < _visibleLightsCount; k++) //   _visibleLightsCount; k++) //v1.9.9.8 - Ethereal v1.1.8h
					{
						//v1.1.8f
						float lightPower3 = 1;
						//if (_stepsControl.x != 0 && length(pos - _WorldSpaceCameraPos) > length(WorldPosition - _WorldSpaceCameraPos)) { //if behind obstacle, zero intensity
						//	lightPower3 = 0;
						//}

						//LIGHT 1
						float distToRayStartA = length(_WorldSpaceCameraPos - pos);//v1.9.9.8 - Ethereal v1.1.8h
#ifdef URP10
						Light light = GetAdditionalPerObjectLight(k, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
						light.shadowAttenuation = AdditionalLightShadow(k, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
						Light light = GetAdditionalPerObjectLight(k, pos);
						light.shadowAttenuation = AdditionalLightShadow(k, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
						Light light = GetAdditionalPerObjectLight(k, pos);
#endif
#endif					
						//LIGHT 1
						float multLightPow = 0.4300346225501;// 0.6805029349687388;// 17088.109200; for 188, for 128  =0.4300346225501
						//if ((lightCount < 0 && distToRayStartA < abs(lightCount*0.001)) || (lightCount > k && _visibleLightsCount > k + 1)) {//v1.9.9.5 if (_visibleLightsCount > 1) {//v1.9.9.8
							/*if (controlByColor == 0 || (controlByColor == 1 &&
								((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
									|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
									|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
									)
									)
								)) {*/

								//result.rgb = result.rgb - 0.14*1 * light.color * light.distanceAttenuation * light.shadowAttenuation;//*15
								//result.rgb = result.rgb - result.rgb* light.color * light.shadowAttenuation* light.distanceAttenuation;
								//result.a = result.a + result.a*light.shadowAttenuation* light.distanceAttenuation;
				//		alpha = alpha - 1.2*alpha * light.shadowAttenuation* light.distanceAttenuation;
						//result.rgb = result.rgb + 1.2*result.rgb *light.color* light.shadowAttenuation* light.distanceAttenuation;

						//result.rgb = 0+ light.color*10.0001* light.shadowAttenuation* light.distanceAttenuation;// result.rgb * (1 - light.shadowAttenuation* light.distanceAttenuation) + 0.1*light.color* light.shadowAttenuation* light.distanceAttenuation;// float3(1, 0, 0);// light.color;

						if (light.shadowAttenuation* light.distanceAttenuation >= 1)
						{
							//alpha += step * 0.2;
						}

						//COOKIE
						//#if defined(_LIGHT_COOKIES)
#ifdef URP11
						float3 cookieColor = SampleAdditionalLightCookie(k, pos)* light.shadowAttenuation;
						light.color *= cookieColor;
#endif

						result.rgb += light.color*0.001* light.distanceAttenuation * light.shadowAttenuation*_Intensity*100;// *(light.shadowAttenuation)* (light.distanceAttenuation);
						alpha += light.color*0.001* (light.shadowAttenuation)* (light.distanceAttenuation);
						//result.rgb += float3(1, 1, 1) * light.shadowAttenuation* light.distanceAttenuation;
						//result.a = 1;
						//alpha = alpha -( 1-light.shadowAttenuation* light.distanceAttenuation);
						//sourceImg = sourceImg + lightPower3 * lightControlA.z * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;//*15
					//}
				//}
					}
					//float rand = volumeSamplingControl.y * 0.1 * (1 - volumeSamplingControl.x)
					//	+ volumeSamplingControl.z * UVRandom(uv + m + 1) * (volumeSamplingControl.x);

					//pos += stepA + stepA * rand * 0.8;
					t += stepA;
					pos = rayOrigin + rayDirection * t;
				}

				//alpha = alpha / _MaxIterations;
			}
#endif
		}
		//END v0.1 LOCAL LIGHTS





		result.a *= alpha; //v0.1
        //result.a *= saturate(alpha);

        return result * (_Intensity*4);
    }
	TEXTURE2D_X(_MaskTex);
    half4 Frag(RayVaryings input) : SV_Target
    {
        float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_LinearClamp, input.uv).r;
        depth = Linear01Depth(depth, _ZBufferParams);
        depth *= length(input.ray);

        float3 rayOrigin = _CamWorldSpace;
        float3 rayDir = normalize(input.ray);
        float4 result = SimpleRaymarching(rayOrigin, rayDir, depth);


		result.a = result.a -0.25*(5*input.uv.y) * clamp(customCookieIntensityA*8,0,1);
		result.rgb = result.rgb * 3 * result.a;
        
        return result * 1;
    }


		//VERT
		struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	struct AttributesB
	{
		float4 positionOS       : POSITION;
		float2 uv               : TEXCOORD0;
	};

	v2f VertA(AttributesB v) {//v2f vert(AttributesDefault v) { //appdata_img v) {
		//v2f o;
		v2f o = (v2f)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
		o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
		float2 uv = v.uv;
#if !UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		o.uv = uv;
#if !UNITY_UV_STARTS_AT_TOP
		o.uv = uv.xy;//o.uv1 = uv.xy;
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;//o.uv1.y = 1 - o.uv1.y;
#endif	
		return o;
	}

		half4 Frag_Combine(AttributesA input) : SV_Target//half4 Frag_Combine(Varyings input) : SV_Target //v0.1
    {
        half4 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp , input.uv);
        half4 shaft = SAMPLE_TEXTURE2D_X(_LightShaftTempTex, sampler_LinearClamp, input.uv);
		half4 colorA = color;

        color.rgb = saturate(color.rgb * (1 - shaft.a)) + shaft.rgb * shaft.a;

		half4 mask = SAMPLE_TEXTURE2D_X(_MaskTex, sampler_LinearClamp, input.uv);

		//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
		float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));

		if (_WorldSpaceCameraPos.y < cutoffHeigth - 30 || (dot(forward, float3(0, -1, 0)) > 0.5)) {
			mask.rgb = float3(1, 1, 1);
		}

		if (mask.r < 0.01 && mask.g < 0.01 && mask.b < 0.01) {
			//discard;
			color = colorA;
		}
        
		return color;// *_Intensity;
        //return pow(color,0.8)*0.4;
    }
ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "GradientFog"

            HLSLPROGRAM
                #pragma vertex Vert_Ray
                #pragma fragment Frag
            ENDHLSL
        }
        
        Pass
        {
            Name "Combine"
            
            HLSLPROGRAM
                #pragma vertex VertA
                #pragma fragment Frag_Combine
            ENDHLSL
        }
    }
}
