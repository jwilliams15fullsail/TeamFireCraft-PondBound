// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FluidSim/Obstacle HDRP DEPTH TRANSP" 
{

Properties
     {
        // _MainTex ("Base (RGB)", 2D) = "white" {}
         //_heightFactor ("height Factor", Range(0, 5)) = 1
         //_heightCutoff ("height Cutoff", Range(0, 0.1)) = 0.1
         //_contrast ("contrast", Range(0.05, 0.15)) = 0.1

		 //TERRAIN DEPTH
		_DepthCameraPos("Depth Camera Pos", Vector) = (240 ,100, 250, 3.0)
		//_ShoreContourTex("_ShoreContourTex", 2D) = "white" {}
		_ShoreFadeFactor("_ShoreFadeFactor", Float) = 1.0
		_TerrainScale("Terrain Scale", Float) = 500.0
		_ShoreContourTex("Base (RGB)", 2D) = "white" {}

		invertDepth("invertDepth", Float) = 0
     }

	SubShader 
	{
    	Pass 
    	{
			ZTest Always

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float2 _InverseSize;
			uniform float2 _Point;
			uniform float _Radius;
			float invertDepth;
			sampler2D _ShoreContourTex;

			//TERRAIN DEPTH
			//sampler2D _ShoreContourTex;
			float3 _DepthCameraPos;
			float _ShoreFadeFactor;
			float _TerrainScale;
		
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
				float4 result = float4(0,0,0,0);



				//MINE
				//float scaler = 1126.0 / 534.0;// _ScreenParams.x / _ScreenParams.y; //width / height of screen
				float heightDepthCam = _DepthCameraPos.y;// 88;// 942.64;// 88;// 942.64;// 942.64; //URP
				_TerrainScale = heightDepthCam * 2;
				//float2 coords = v.texcoord.xy/1;		
				//coords = float2(tileableUv.x*11, tileableUv.y*10);		
				float WorldScale = _TerrainScale;
				float3 CamPos = _DepthCameraPos;//_WorldSpaceCameraPos;
				float2 Origin = float2(CamPos.x - WorldScale / 2, CamPos.z - WorldScale / 2);
				float2 UnscaledTexPoint = float2(IN.uv.x - Origin.x, IN.uv.y - Origin.y);

				

				float2 ScaledTexPoint = float2(_ShoreFadeFactor*UnscaledTexPoint.x / WorldScale, UnscaledTexPoint.y / WorldScale);
				//MINE TEXTURE FEED
				//float heightDepthCam = _DepthCameraPos.y;// 88;// 942.64;// 88;// 942.64;// 942.64; //URP
				//_TerrainScale = heightDepthCam * 2;//URP
				//half2 tileableUv = float2(positionWS.x, positionWS.z);// positionWS.xz;// mul(unity_ObjectToWorld, (v.vertex)).xz;
				//float WorldScale = 500;
				//WorldScale = _TerrainScale;
				//float3 CamPos = float3(250, 0, 250);//_WorldSpaceCameraPos;
				//CamPos = _DepthCameraPos;//_WorldSpaceCameraPos;
				//float2 Origin = float2(CamPos.x - WorldScale / 2.0, CamPos.z - WorldScale / 2.0);
				//float2 UnscaledTexPoint = float2(tileableUv.x - Origin.x, tileableUv.y - Origin.y);
				//float2 ScaledTexPoint = float2(UnscaledTexPoint.x / WorldScale, UnscaledTexPoint.y / WorldScale);
				//float4 tex = tex2D(_MainTex, float4(float2(ScaledTexPoint.x, ScaledTexPoint.y), 0.0, 0.0));



				//float4 tex = tex2Dlod(_ShoreContourTex, float4(ScaledTexPoint, 0, 0));
				float4 col = tex2D(_ShoreContourTex, float4(ScaledTexPoint.x, ScaledTexPoint.y, 0, 0)); //v0.1 - adjust obstacle placement
				col = (1 - col) * invertDepth + (col) * (1 - invertDepth);

				if (invertDepth > 0 ) {
					col = saturate(float4(1,1,1,1) * col.r *1);
				}

				//render texture
				float3 camPos = _WorldSpaceCameraPos;
				float camStartY=0;
				//float4 col = tex2D(_MainTex, IN.uv  + float2(0.0, 0.0));
				//float4 col = tex2D(_MainTex, float2(IN.uv.x * 0.5, IN.uv.y * 0.5) +        float2(0.2,0.3)           ); //+ float2(-0.0,0.5*(camPos.x-camStartY)));
				////float4 col = tex2D(_MainTex, float2(IN.uv.x * 0.35, IN.uv.y * 0.35) + float2(0.291, 0.35)); //v0.1 - adjust obstacle placement
				result = 1-col;
				

				if (invertDepth > 0) {
				
				}
				else {
					if (result.r < 0.2) {
						result = float4(0, 0, 0, 0);
					}
					if (result.r > 0.3) {
						result = float4(1, 1, 1, 1);
					}
				}



				//draw border 
				if(IN.uv.x <= _InverseSize.x) result = float4(1,1,1,1);
				if(IN.uv.x >= 1.0-_InverseSize.x) result = float4(1,1,1,1);
				if(IN.uv.y <= _InverseSize.y) result = float4(1,1,1,1);
				if(IN.uv.y >= 1.0-_InverseSize.y) result = float4(1,1,1,1);
				
				//draw point
				//float d = distance(_Point, IN.uv);			    
			    //if(d < _Radius) result = float4(1,1,1,1);


				////draw point 2
				//d = distance(_Point, IN.uv * 1.4 + float2(0.5,0.4));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				////draw point 2
				//d = distance(_Point, IN.uv * 1.6 + float2(0.6, 0.2));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				////draw point 2
				//d = distance(_Point, IN.uv * 1.7 - float2(0.1, 0.6));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				////draw point 2
				//d = distance(_Point, IN.uv * 2.1 - float2(0.4, 0.1));
				//if (d < _Radius) result = float4(1, 1, 1, 1);

				////draw point 2
				//d = distance(_Point, IN.uv * 2.4 + float2(0.55, 0.45));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				////draw point 2
				//d = distance(_Point, IN.uv * 2.6 + float2(0.65, 0.25));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				////draw point 2
				//d = distance(_Point, IN.uv * 2.7 - float2(0.15, 0.65));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				////draw point 2
				//d = distance(_Point, IN.uv * 3.1 - float2(0.45, 0.15));
				//if (d < _Radius) result = float4(1, 1, 1, 1);
				result.a = 0;
				return result;
				

			}
			
			ENDCG

    	}
	}
}