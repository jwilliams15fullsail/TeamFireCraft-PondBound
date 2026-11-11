// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FluidSim/Impluse" 
{
	SubShader 
	{
    	Pass 
    	{
			ZTest Always
			//Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float2 _Point;
			uniform float _Radius;
			uniform float3 _Fill;  //v0.2
			uniform sampler2D _Source;
			
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
			    float d = distance(_Point, IN.uv);
			    
				float3 impulse = float3(0,0,0);
			    
			    if(d < _Radius) 
			    {
			        float a = (_Radius - d) * 0.5;
					impulse = min(a, 1.0);
			    } 

				float3 source = tex2D(_Source, IN.uv).xyz;
			  
			  	//return max(0, lerp(source, _Fill, impulse)).xxxx;
				return float4(max(float3(0,0,0), lerp(source, _Fill, impulse.xxx)),1);  //v0.2
				//return float4(1,0,0, 1);
			}
			
			ENDCG

    	}
	}
}