Shader "Hidden/ARTnGAME/Oceanis/GradientFogOCEANIS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

    TEXTURE2D_X(_MainTex);
    TEXTURE2D_X(_CameraDepthTexture);

    TEXTURE2D_X(_RampTex);
    float _Intensity;
	
	//v0.1
	float cutoffHeigth;
	float depthFade;
	float4 fogTint;

	TEXTURE2D_X(_MaskTex);
	

	//v0.1
	struct VaryingsA
	{
		float4 positionOS    : POSITION;
		float2 uv            : TEXCOORD0;
	};

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

	half4 Frag(VaryingsA input) : SV_Target///half4 Frag(Varyings input) : SV_Target
    {
        half4 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp , input.uv);
		half4 colorA = color;
        float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_LinearClamp, input.uv).r;
		float depthEye = LinearEyeDepth(depth, _ZBufferParams);
        depth = Linear01Depth(depth, _ZBufferParams);

        half4 ramp = SAMPLE_TEXTURE2D_X(_RampTex, sampler_LinearClamp, float2(depth, 0));
		float close = 0.385;// 0.985;

		if (depthFade > 0) {
			if (depth < 0.99999) {
				color.rgb = lerp(color.rgb, ramp.rgb * 0.5, saturate(_Intensity * pow(depthEye / 30, 2) * 0.01* depthFade));
			}
		}
		else {
			if (depth < 1 - close) { //0.92 pushes further back
				//color.rgb = lerp(color.rgb, ramp.rgb*0.5, pow(1.52 - input.uv.y, 2)*_Intensity * ramp.a*(  0.3-0.3*pow(depth - (1 - close),3)));
				color.rgb = lerp(color.rgb, ramp.rgb * 0.5, pow(1.92 - input.uv.y, 2) * _Intensity * ramp.a * (0.3 - 0.3 * pow(depth - (1 - close), 3)));
			}
			else {
				//color.rgb = lerp(color.rgb, ramp.rgb, _Intensity * ramp.a);
				color.rgb = lerp(color.rgb, ramp.rgb * 0.5, pow(1.52 - input.uv.y, 2) * _Intensity * ramp.a);// *(1 - depth);
			}
			//color.rgb = lerp(color.rgb, ramp.rgb, _Intensity * ramp.a);// *(1 - depth);
		}
		

		half4 mask = SAMPLE_TEXTURE2D_X(_MaskTex, sampler_LinearClamp, input.uv);

		//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
		float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));

		if (_WorldSpaceCameraPos.y < cutoffHeigth - 30 || (dot(forward, float3(0, -1, 0)) > 0.5)) {
			mask.rgb = float3(1,1,1);
		}

		if (mask.r < 0.01 && mask.g < 0.01 && mask.b < 0.01) {
			//discard;
			color = colorA;
		}
		else {
			//if (depth > 0.59) {
				//color.rgb = ramp.rgb*float3(1,1,2)*0.2*color.rgb;
				color.rgb = color.rgb*float3(0.5, 1.8, 2)*0.4* fogTint.rgb;
			//}
		}

		//if (depth > 0.57) {
		//	color.rgb = float3(1, 1, 1);// lerp(color.rgb, ramp.rgb*10.5, pow(1.52 - input.uv.y, 2)*_Intensity * ramp.a*(0.3 - 0.3*pow(depth - (1 - close), 3)));
		//}

        return color;
    }
ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "GradientFogOCEANIS"

            HLSLPROGRAM
                #pragma vertex VertA
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
