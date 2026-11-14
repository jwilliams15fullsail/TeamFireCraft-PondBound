// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "FluidSim/AdvectWideTerrainFOAM" 
{
	SubShader 
	{
		Pass // PASS 0
		{
			ZTest Always

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _Velocity;
			uniform sampler2D _Source;
			uniform sampler2D _Obstacles;
			uniform sampler2D _foamTex;

			uniform float2 _InverseSize;
			uniform float _TimeStep;
			uniform float _Dissipation;

			float fluidSpeed;
			float4 foamBrightnessA;
			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{
				//float4 SPEED = tex2D(_Velocity, IN.uv)*3;
				//float4 SPEEDF = tex2D(_Source, IN.uv) ;
				//return SPEEDF;

				//float4 foamTexA = tex2D(_foamTex, IN.uv * 6);

				//v0.4
				float2 flowTEXA = tex2D(_Velocity, IN.uv).rg;// *2 - 1;
				float4 resultC = _Dissipation * tex2D(_Source, IN.uv - 0.00005* flowTEXA* fluidSpeed)* 1;// *flowTEXA.xy);
				if (length(flowTEXA) < 0.6 * foamBrightnessA.z) {
					resultC = 0.01;
				}
				resultC = clamp(resultC * foamBrightnessA.x, 0, 0.8 * foamBrightnessA.y);// *0.005 * (length(flowTEXA) + 0.5));
				return resultC;

				float2 coordB = IN.uv - (flowTEXA * (1 / 1) * 0.7) * 0.00009003245;// *(abs(sin(_Time.y * 1.7) * 0.5) + 0.5);// _InverseSize* _TimeStep);
				float4 resultB = _Dissipation * tex2D(_Source, coordB);
				float factor = (flowTEXA * (1 / 1) * 1.5) * 0.1011 * abs(cos(_Time.y * 2));
				if (length(flowTEXA) < 10.1) {
					//resultB = 0.01;
				}
				resultB = clamp(resultB * 1.02* foamBrightnessA.x, 0, 0.78 * foamBrightnessA.y);// *length(flowTEXA) * 0.02;
				return resultB;




				//float2 flowTEX = tex2D(_Velocity, IN.uv).rg * 2 - 1;
				float2 flowTEX = tex2D(_Velocity, IN.uv).rg * 2 - 1;
				float HT = 0.0035;//*_Time.y;		
				//float2 flowmap = tex2D( _FlowMap, ps.uv ).rg * 2.0f - 1.0f;
				float PA = frac(IN.uv * 0.001 * _Time.y);//*0.0001);
				float PB = frac((IN.uv + 0.05) * 0.001 * _Time.y);//*0.0001);
				float3 FOAMA1 = tex2D(_Source, (IN.uv * 3 + flowTEX * PA * 0.03));
				float3 FOAMA2 = tex2D(_Source, (IN.uv * 3 + flowTEX * PB * 0.03));
				float flowLerp = ((HT - IN.uv) / HT);
				float	foamTex = 0.015 * pow(lerp(FOAMA1, FOAMA2, flowLerp).rgbb,2) * 1 * 1 * 1;
				//return tex2D(_WaterFlowFOAM, flowBumpMYFLOWERC*4);//
				//	foamTex.rgb = foamTex.rrr * 1320*0.1;
					//return float4(foamTex.rrr,0.4);
					//foamTex.rgb = 0.01;
					//float2 foamPosition = flowBumpMYFLOWERCA * 15 - (ddy(flowTEX)+ddx(flowTEX)) * 0.015 * flowTEX - flowTEX * PA;
				//float2 foamPosition = IN.uv - (ddy(flowTEX) - ddx(flowTEX)) * 0.00005 * flowTEX - flowTEX * PB * 0.032;
				float2 foamPosition = IN.uv - (ddy(flowTEX) - ddx(flowTEX)) * 0.00005 * flowTEX - flowTEX * 1 * 0.132 * foamTex;


				//float2 foamPosition = IN.uv - (ddy(flowTEX) - ddx(flowTEX)) * 0.000015 * flowTEX;// -flowTEX * 1 * 0.0132 * foamTex;
				float4 resultA = _Dissipation * tex2D(_Source, foamPosition);



				resultA = clamp(pow(resultA, 1) * 1, 0, 0.7) * 1;
				//resultA = clamp(pow(resultA,1)*1,0,0.87)*1;



				//float2 solidA = tex2D(_Obstacles, IN.uv).xy;

				//if(solid > 0.0) resultA = float4(0,0,0,0);
				//if(solidA.x == 0.0 || solidA.y == 0.0) resultA = float4(0,0,0,0);

				return resultA;
				//return float4( pow(resultA.rgb,3)*1.2,1);

				float2 u = tex2D(_Velocity, IN.uv).xy;

				float2 coord = saturate(IN.uv - (u * 1 * _TimeStep) * 1);

				float4 result = 1 * tex2D(_Source, coord);

				//float solid = tex2D(_Obstacles, IN.uv).x;
				float2 solid = tex2D(_Obstacles, IN.uv).xy;

				//if(solid > 0.0) result = float4(0,0,0,0);
				//if(solid.x == 0.0 || solid.y == 0.0) result = float4(0,0,0,0);

				return  result;
			}

			ENDCG

		}//END PASS 0


		Pass // PASS 1
		{
			ZTest Always

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			uniform sampler2D _Velocity;
			uniform sampler2D _VelocityFOAM;
			uniform sampler2D _SourceFOAM;
			sampler2D _foamTex;
			float foamBrightness;
			uniform float _Dissipation;
			float fluidSpeed;
			float4 foamBrightnessA;
			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{	
				
				float4 foamTexO = tex2D(_foamTex, IN.uv * 25);
				float4 foamTex = tex2D(_foamTex, IN.uv * 25); //return foamTex;
				float4 FOAM = tex2D(_VelocityFOAM, IN.uv);

				float2 flowTEXA = tex2D(_Velocity, IN.uv).rg;// *2 - 1;
				float4 resultC = _Dissipation * tex2D(_foamTex, IN.uv - 0.00005 * flowTEXA * fluidSpeed) * 1;


				float4 SOURCE = tex2D(_SourceFOAM, IN.uv);

				if (FOAM.r == 0 && FOAM.g == 0){// && SOURCE.r < 0.1) {
					foamTex = 0;
				}
				//foamTex = foamTex * FOAM.r;

				//return foamTex;

				
				//return SOURCE + FOAM;// *2.115 + SOURCE * 1 * 1;// +foamTex * 0.02;
				

				return (resultC * 2 + SOURCE * 2.115 * resultC + SOURCE * foamTexO * 2.8)* foamBrightness;// *foamTexO * 5 - SOURCE;
				//return saturate(SOURCE + float4(foamTex.rgb, 1) * 1.01)/1.3;
				//return SOURCE + SOURCE * float4(foamTex.rgb, 1) * 11111+ float4(foamTex.rgb, 1)*1;
				return SOURCE;// SOURCE + SOURCE * float4(FOAM.rgb, 0.5) * 11110.01 + float4(FOAM.rgb, 0.5) * 0.001;
				return  float4(FOAM.rgb, 1)*0.001;
			}
			ENDCG
		}//END PASS 1

	}
}