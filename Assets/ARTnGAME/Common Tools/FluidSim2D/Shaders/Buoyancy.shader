// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FluidSim/Buoyancy" 
{
		Properties
	{
		_JetDirection ("Jet Direction (x = east, y = north)", Vector) = (0, 1, 0, 0)
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
			
			uniform sampler2D _Velocity;
			uniform sampler2D _Temperature;
			uniform sampler2D _Density;
			uniform float _AmbientTemperature;
			uniform float _TimeStep;
			uniform float _Sigma;
			uniform float _Kappa;

			//v0.1
			uniform float4 _Wind;
			uniform float4 _JetDirection; // which way warm water pushes the flow (it was always (0, 1))
		
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
			    float T = tex2D(_Temperature, IN.uv).x;
			    float2 V = tex2D(_Velocity, IN.uv).xy;
			    float D = tex2D(_Density, IN.uv).x;
			
				//v0.1 - add wind control
				V.x = V.x * _Wind.x + _Wind.z;
				V.y = V.y * _Wind.y + _Wind.w;

			    float2 result = V;
			
			    if(T > _AmbientTemperature) 
			    {
			        			        result += (_TimeStep * (T - _AmbientTemperature) * _Sigma - D * _Kappa ) * (dot(_JetDirection.xy, _JetDirection.xy) > 0 ? normalize(_JetDirection.xy) : float2(0, 1));
			    }
			    
			    return float4(result, 0, 1);
			    
			}
			
			ENDCG

    	}
	}
}