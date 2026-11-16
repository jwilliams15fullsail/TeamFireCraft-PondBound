// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Particle Dynamic Magic/AdditiveBlendMix2_BLOB" {
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
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv1 : TEXCOORD1;
			half2 uv2 : TEXCOORD2;
			fixed4 vertexColor : COLOR;
		};

		v2f vert(appdata_full v) {
			v2f o;



			//v2.3 - BLOBY
			float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
			float dist = distance(worldPos.xyz, _bodyPos);
			float3 direction = _bodyPos - worldPos.xyz; 
			float phase = 1;
			//worldPos.xyz = worldPos.xyz * abs(11/(pow(dist,1)*( - )*2)) ;
			//worldPos.xyz = worldPos.xyz + clamp((direction*(44/pow(dist,5))) ,-1,1) * 1*(cos(_Time.y*1*pow(2,2) + phase)+3) + float3(0, clamp(-414/pow(dist,5),-1,1),0)  ;
			//worldPos.xyz = worldPos.xyz + clamp((direction*(44/pow(dist,5)))  * 1*(cos(_Time.y*1*pow(2,2) + phase)+3) + float3(0, clamp(-414/pow(dist,5),-1,1),0),-1,1)  ;

//			float clamper = 10.3;
//			if(dist < 3){
//				clamper=0;
//			}
//			worldPos.xyz = worldPos.xyz + clamp((direction*(_effectPower/pow(dist,_falloffPower)))  * 1*(cos(_Time.y*1*pow(2,2) + phase)+0) + 1*float3(0, clamp(-814/pow(dist,5),-1,1),0),-1,1)  ;


			//_effectPower = _effectPower * (dist*5);

//			worldPos.xyz = worldPos.xyz + clamp(
//			(direction*(_effectPower/pow(dist,_falloffPower)))  *0.1*(cos(_Time.y*1*pow(2,2) + phase)+1) + float3(0, clamp(-11414/pow(dist,5),-11.1*pow(dist,2),11.1*pow(dist,2)),0)    + direction*(_effectPower/pow(dist,_falloffPower)),
//			-0.1*pow(dist,2),0.1*pow(dist,2));


//			worldPos.xyz = worldPos.xyz + clamp(
//			direction*(_effectPower/pow(dist,_falloffPower))  +  float3(0, clamp(-211414/pow(dist,5),-2,111.1),0),
//			-0.1*pow(dist,2),0.1*pow(dist,2));


			float clamper = 1;
			//if(pow(_bodyDist,2) > 33){
			if(_bodyDist > 9){
				//clamper = clamp(1/(1+pow(_bodyDist,2)-33),0,11);
				clamper=clamp(2-(_bodyDist/9),0,1);
			}
			//if(dist < 6){
				//_effectPower=_effectPower / (dist*0.05);
				//float clamper = 0.1*(clamp(6-dist,-2.1,0));// * dist;// 0.3;
				//_effectPower = 0.1*_effectPower / (111111*dist);
			//}

			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);			
			o.uv1 = TRANSFORM_TEX(v.texcoord, _BorderTex);
			o.uv2 = TRANSFORM_TEX(v.texcoord, _MainTex2);

			float ratio = _effectPower/pow(dist,_falloffPower);
			float clamped = clamp(ratio,-1,0.4*10.3/dist);

			float extra =0;
			if(_bodyDist < 4){
				extra = float3(-direction.x*clamped,0,-direction.z*clamped);
			}
			worldPos.xyz = worldPos.xyz + (
				//direction*(clamp(_effectPower/pow(dist,_falloffPower),-1,0.4*10.3/dist))  +  clamper*float3(0, clamp(11112414/pow(dist,8),-11,111.1),0)  + float3(0, clamp(-121/pow(dist,2)+1,-1,0),0)
				direction*(clamped)   +
				clamp( 
				float3(0, clamp(-11421*(abs(1/(worldPos.y+_worldBase)+0))/pow(dist-0,4),-1,1)+clamper*0.9,0)  
				+ float3(0, clamp(-111421*(abs(1/(worldPos.y+_worldBase)+2))/pow(dist,6),-0.4,1)+clamper*0.2,0)
				,-2,0)
				+0.1*(cos(_Time.y*4 + dist)+0)
				+abs(extra*1)
			);


			v.vertex = mul(unity_WorldToObject,worldPos);


			o.pos = UnityObjectToClipPos (v.vertex);	

			
			//o.vertexColor = v.color * _TintColor;

			//_fadePower = _fadePower *dist;
			float adjustBright = 3;
			float shiftDist = 9;
			if(_bodyDist < shiftDist){
				_fadePower = _fadePower*(_bodyDist/shiftDist);//8-_bodyDist;
				//adjustBright = 6 - 3*(_bodyDist/8);
				adjustBright = 6-3*(pow(_bodyDist,3)/pow(shiftDist,3));
			}

			//o.vertexColor = v.color * _TintColor * clamp(1/(22/pow(dist,2)) , 0.1,1);
			o.vertexColor = v.color * _TintColor * clamp(pow(dist+adjustBright*pow(clamper,6),_fadeFalloff)/(_fadePower) ,0.1,1);

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
