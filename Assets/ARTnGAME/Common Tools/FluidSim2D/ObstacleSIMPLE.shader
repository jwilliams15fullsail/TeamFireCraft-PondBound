// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FluidSim/ObstacleSIMPLE" 
{

Properties
     {
         _MainTex ("Base (RGB)", 2D) = "white" {}
         //_heightFactor ("height Factor", Range(0, 5)) = 1
         //_heightCutoff ("height Cutoff", Range(0, 0.1)) = 0.1
         //_contrast ("contrast", Range(0.05, 0.15)) = 0.1
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

			sampler2D _MainTex;
		
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





				//render texture
				float3 camPos = _WorldSpaceCameraPos;
				float camStartY=0;
				//float4 col = tex2D(_MainTex, IN.uv  + float2(0.0, 0.0));
				//float4 col = tex2D(_MainTex, float2(IN.uv.x * 0.5, IN.uv.y * 0.5) +        float2(0.2,0.3)           ); //+ float2(-0.0,0.5*(camPos.x-camStartY)));
				float4 col = tex2D(_MainTex, float2(IN.uv.x * 0.35, IN.uv.y * 0.35) + float2(0.291, 0.35)); //v0.1 - adjust obstacle placement
				result = 1-col;
				if(result.r < 0.2){
					result = float4(0,0,0,0);
				}
				if(result.r > 0.3){
					result = float4(1,1,1,1);
				}





				//draw border 
				if(IN.uv.x <= _InverseSize.x) result = float4(1,1,1,1);
				if(IN.uv.x >= 1.0-_InverseSize.x) result = float4(1,1,1,1);
				if(IN.uv.y <= _InverseSize.y) result = float4(1,1,1,1);
				if(IN.uv.y >= 1.0-_InverseSize.y) result = float4(1,1,1,1);
				
				//draw point
				float d = distance(_Point, IN.uv);
			    
			    if(d < _Radius) result = float4(1,1,1,1);
			
				return result;
				

			}
			
			ENDCG

    	}
	}
}