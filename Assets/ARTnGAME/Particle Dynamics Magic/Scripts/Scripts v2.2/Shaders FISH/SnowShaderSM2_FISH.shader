// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "SkyMaster/SnowShaderSM2_FISH" {
    Properties {      
        //_SnowCoverage ("Snow Coverage", Range(0, 1)) = 0   
        _SnowBlend ("Snow Blend", Range(0, 50)) = 0.4     
        _LightIntensity ("Light Intensity", Range(0.5, 50)) = 1
        _SnowBumpDepth ("Snow bump depth", Range(0, 5)) = 1          
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Bump ("Bump", 2D) = "bump" {}        
        _SnowTexture ("Snow texture", 2D) = "white" {}
        _Depth ("Depth of Snow", Range(0, 0.02)) = 0.01        
        _SnowBump ("Snow Bump", 2D) ="bump" {}        
        _Direction ("Direction of snow", Vector) = (0, 1, 0)
        _Power ("Snow,Main,Blend Factors", Vector) = (0.5, 0.5, 1,1)
      //  worldPos ("Pos", Vector) = (0.5, 0.5, 1,1)        
        _Shininess ("Shininess", Range (0.01, 1)) = 0.078125

        //v - BEND
        	[LM_Specular] [LM_Glossiness] _SpecGlossMap("Specular 0", 2D) = "white" {}
		AttractForce ("AttractForce", Float) = 0
		WavingForce ("WavingForce", Float) = 1
		_AttractPosition1 ("_AttractPosition1", Vector) = (0, 0, 0) 
		_AttractPosition2 ("_AttractPosition1", Vector) = (0, 0, 0) 
		_Color ("Tint", Color) = (1,1,1,1)
			Snow_Cover_offset ("Snow coverage offset", Float) = 0
			water_level ("Water level", Float) = 1
		water_spec ("Water Spec Focus", Float) = 1.2
    }
   
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        ZWrite On
        Cull Off
       
        CGPROGRAM
        #pragma surface surf StandardSpecular vertex:vert
        // #pragma target 3.0
         #include "UnityCG.cginc"
 		#include "AutoLight.cginc"
 		#include "Lighting.cginc"
         
        float _SnowCoverage; 
        float _SnowBlend;  
        float _LightIntensity;
        float _SnowBumpDepth;
                  
        sampler2D _MainTex;
        sampler2D _Bump;        
        sampler2D _SnowTexture;
        float _Depth;        
        sampler2D _SnowBump;        
        float3 _Direction;
        float4 _Power;
       // float3 worldPos; 
        half _Shininess; 



        //v - BEND
		float AttractForce;
		float3 _AttractPosition1;
		float WavingForce;
			float4 _Color;
        float Snow_Cover_offset;
        sampler2D _SpecGlossMap;

        uniform float water_level;
		
		uniform float water_spec;


        struct Input {        	
            float2 uv_MainTex;
            float2 uv_Bump;
            float2 uv_SnowTexture;
            float2 uv_SnowBump;
            float3 worldNormal;
            //float3 worldPos;
            INTERNAL_DATA         
        };        
       
        void vert (inout appdata_full v) {         	


        //UNITY_INITIALIZE_OUTPUT(Input,o);
			
			v.vertex = mul(unity_ObjectToWorld, v.vertex);

			float distx = distance(_AttractPosition1.x, v.vertex.x);
			float distz = distance(_AttractPosition1.z, v.vertex.z);

			float dist = distance(_AttractPosition1, v.vertex);


			v.vertex.xyz = v.vertex.xyz + (v.vertex.xyz-_AttractPosition1)*AttractForce/(pow(dist,1.4)); //AVOIDING

			v.vertex = mul(unity_WorldToObject, v.vertex);

		
			v.vertex.z += (0.1*cos(_Time.y*0.1*v.vertex.x)*v.vertex.x*sin(_Time.y*4 - v.vertex.x) + v.vertex.x*cos(_Time.y*3)*0.4) * WavingForce   ;




//            float3 Snow = normalize(_Direction.xyz);
//           
//            if (dot(v.normal, Snow) >= lerp(1, -1, (_SnowCoverage * 2) / 3))
//            {
//                v.vertex.xyz += normalize(v.normal + Snow)  * _SnowCoverage * _Depth;
//            }           
        }
 
        void surf (Input IN, inout SurfaceOutputStandardSpecular o) {       
   			
   			float4 SnowTexColor = tex2D(_SnowTexture, IN.uv_SnowTexture);
            float4 MainTexColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(tex2D(_Bump, IN.uv_Bump));           


            fixed4 spec1	= tex2D(_SpecGlossMap, IN.uv_MainTex);
            half4 flow1 = tex2D(_SpecGlossMap, float2(IN.uv_MainTex.x*1,IN.uv_MainTex.y+(_Time.x*0.5)) );

            o.Alpha = MainTexColor.a;  
            float DirN = dot(WorldNormalVector(IN, o.Normal), _Direction.xyz)  ; 
            float Check = lerp(1,-1,(_SnowCoverage+Snow_Cover_offset)/7); 
            if(DirN >= Check)
            {              
                o.Albedo = lerp (  MainTexColor.rgb , SnowTexColor.rgb*_LightIntensity,pow((1-(Check/(DirN))),_SnowBlend)) ;                
                o.Normal = normalize(o.Normal + UnpackNormal(tex2D(_SnowBump, IN.uv_SnowBump))*_SnowBumpDepth);                                
            }
            else{
            	o.Albedo = MainTexColor*_Color;
            	//o.Specular 		= spec1.rgb ;
            	o.Smoothness 	= water_spec*spec1.a+ water_level*flow1*spec1.a;
            } 
			o.Specular = _Shininess;			
        }
        ENDCG
    }
    FallBack "Diffuse"
}