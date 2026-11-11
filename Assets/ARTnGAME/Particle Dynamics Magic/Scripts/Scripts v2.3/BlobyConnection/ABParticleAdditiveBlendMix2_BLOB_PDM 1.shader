// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Particle Dynamic Magic/AdditiveBlendMix2_BLOB1" {
	Properties {
		_MainTex ("Base", 2D) = "white" {}
		_MainTex2 ("Base2", 2D) = "white" {}
		_BorderTex ("Border", 2D) = "white" {}
		_TintColor ("TintColor", Color) = (1.0, 1.0, 1.0, 1.0)


		//v2.3
		_bodyPos ("_bodyA position", Float) = (0, 0, 0, 1.0)
		_effectPower ("effectPower", Float) = 44
		_falloffPower ("falloffPower", Float) = 5

		_fadePower ("_fadePower", Float) = 22
		_fadeFalloff ("_fadeFalloff", Float) = 2

		_bodyDist ("_bodyDist", Float) = 1
		_worldBase ("_worldBase", Float) = 0

		_bodyPos2 ("_bodyB position", Float) = (0, 0, 0, 1.0)
		_bodyDist2 ("_bodyDist2", Float) = 1
		_bodyPos3 ("_bodyC position", Float) = (0, 0, 0, 1.0)
		_bodyDist3 ("_bodyDist3", Float) = 1
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

sampler2D _BorderTex;
half4 _BorderTex_ST;

sampler2D _MainTex2;
half4 _MainTex2_ST;

		sampler2D _MainTex;
		fixed4 _TintColor;
		half4 _MainTex_ST;

		float3 _bodyPos;

		float _effectPower;
		float _falloffPower;

		float _fadePower;
		float _fadeFalloff;
		float _bodyDist;
		float _worldBase;

		float3 _bodyPos2;
		float _bodyDist2;
		float3 _bodyPos3;
		float _bodyDist3;
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv1 : TEXCOORD1;
			half2 uv2 : TEXCOORD2;
			fixed4 vertexColor : COLOR;
		};

		v2f vert(appdata_full v) {
			v2f o;





			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);			
			o.uv1 = TRANSFORM_TEX(v.texcoord, _BorderTex);
			o.uv2 = TRANSFORM_TEX(v.texcoord, _MainTex2);


			//v2.3 - BLOBY
			////////// 1st BODY
			float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
			float dist = distance(worldPos.xyz, _bodyPos);
			float3 direction = _bodyPos - worldPos.xyz; 	

			float clamper = 1;
			if(_bodyDist > 9){				
				clamper=clamp(2-(_bodyDist/9),0,1);
			}

			float ratio = _effectPower/pow(dist,_falloffPower);
			float clamped = clamp(ratio,-1,0.4*10.3/dist);

			float extra =0;
			if(_bodyDist < 4){
				extra = float3(-direction.x*clamped,0,-direction.z*clamped);
			}
			worldPos.xyz = worldPos.xyz + (				
				direction*(clamped)   +
				clamp( 
				float3(0, clamp(-11421*(abs(1/(worldPos.y+_worldBase)+0))/pow(dist-0,4),-1,1)+clamper*0.9,0)  
				+ float3(0, clamp(-111421*(abs(1/(worldPos.y+_worldBase)+2))/pow(dist,6),-0.4,1)+clamper*0.2,0)
				,-2,0)
				+0.1*(cos(_Time.y*4 + dist)+0)
				+abs(extra*1)
			);
			float adjustBright = 3;
			float shiftDist = 9;
			if(_bodyDist < shiftDist){
				_fadePower = _fadePower*(_bodyDist/shiftDist);			
				adjustBright = 6-3*(pow(_bodyDist,3)/pow(shiftDist,3));
			}
			o.vertexColor = v.color * _TintColor * clamp(pow(dist+adjustBright*pow(clamper,6),_fadeFalloff)/(_fadePower) ,0.1,1);
			////////// END 1st BODY

			////////// 2ond BODY
			dist = distance(worldPos.xyz, _bodyPos2);
			direction = _bodyPos2 - worldPos.xyz; 	

			clamper = 1;
			if(_bodyDist2 > 9){				
				clamper=clamp(2-(_bodyDist2/9),0,1);
			}

			ratio = _effectPower/pow(dist,_falloffPower);
			clamped = clamp(ratio,-1,0.4*10.3/dist);

			extra =0;
			if(_bodyDist2 < 4){
				extra = float3(-direction.x*clamped,0,-direction.z*clamped);
			}
			worldPos.xyz = worldPos.xyz + (				
				direction*(clamped)   +
				clamp( 
				float3(0, clamp(-11421*(abs(1/(worldPos.y+_worldBase)+0))/pow(dist-0,4),-1,1)+clamper*0.9,0)  
				+ float3(0, clamp(-111421*(abs(1/(worldPos.y+_worldBase)+2))/pow(dist,6),-0.4,1)+clamper*0.2,0)
				,-2,0)
				+0.1*(cos(_Time.y*4 + dist)+0)
				+abs(extra*1)
			);
			adjustBright = 3;
			shiftDist = 9;
			if(_bodyDist2 < shiftDist){
				_fadePower = _fadePower*(_bodyDist2/shiftDist);			
				adjustBright = 6-3*(pow(_bodyDist2,3)/pow(shiftDist,3));
			}
			o.vertexColor = o.vertexColor * clamp(pow(dist+adjustBright*pow(clamper,6),_fadeFalloff)/(_fadePower) ,0.1,1);
			////////// END 2ond BODY

			////////// 3rd BODY
			dist = distance(worldPos.xyz, _bodyPos3);
			direction = _bodyPos3 - worldPos.xyz; 	

			clamper = 1;
			if(_bodyDist3 > 9){				
				clamper=clamp(2-(_bodyDist3/9),0,1);
			}

			ratio = _effectPower/pow(dist,_falloffPower);
			clamped = clamp(ratio,-1,0.4*10.3/dist);

			extra =0;
			if(_bodyDist3 < 4){
				extra = float3(-direction.x*clamped,0,-direction.z*clamped);
			}
			worldPos.xyz = worldPos.xyz + (				
				direction*(clamped)   +
				clamp( 
				float3(0, clamp(-11421*(abs(1/(worldPos.y+_worldBase)+0))/pow(dist-0,4),-1,1)+clamper*0.9,0)  
				+ float3(0, clamp(-111421*(abs(1/(worldPos.y+_worldBase)+2))/pow(dist,6),-0.4,1)+clamper*0.2,0)
				,-2,0)
				+0.1*(cos(_Time.y*4 + dist)+0)
				+abs(extra*1)
			);
			adjustBright = 3;
			shiftDist = 9;
			if(_bodyDist3 < shiftDist){
				_fadePower = _fadePower*(_bodyDist3/shiftDist);			
				adjustBright = 6-3*(pow(_bodyDist3,3)/pow(shiftDist,3));
			}
			o.vertexColor = o.vertexColor * clamp(pow(dist+adjustBright*pow(clamper,6),_fadeFalloff)/(_fadePower) ,0.1,1)*1.5;
			////////// END 3rd BODY

			//END BLOBY

			v.vertex = mul(unity_WorldToObject,worldPos);


			o.pos = UnityObjectToClipPos (v.vertex);	

			
			//o.vertexColor = v.color * _TintColor;

			//_fadePower = _fadePower *dist;


			//v2.3 - BLOBY
//			float3 worldPos = mul(_Object2World,v.vertex).xyz;
//			float dist=distance(o.pos.xyz, _bodyPos);
//			o.pos.xyz = o.pos.xyz - 10*(1/pow(dist,2)*(o.pos.xyz - _bodyPos)*47*abs((abs(cos(_Time.y*0.7))-0.8))); 
//
//			v.vertex = mul(_Object2World,worldPos);



			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR {	
		
		float4 ColorFinal = tex2D(_MainTex, i.uv.xy )* tex2D(_BorderTex, i.uv1.xy)* tex2D(_MainTex2, i.uv2.xy) * i.vertexColor;
			return  ColorFinal ;
		}
	
	ENDCG
	
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha One
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}
