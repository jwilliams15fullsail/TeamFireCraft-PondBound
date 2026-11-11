Shader "Test/Ocean"
{
	Properties
	{
		_Anim ("Displacement Map", 2D) = "black" {}
		_Height ("Height Map", 2D) = "black" {}
		_Bump ("Normal Map",2D) = "bump" {}
		_White ("White Cap Map", 2D) = "black" {}
		_LightWrap ("Light Wrapping Value", Float) = 1 
		_Tint ("Color Tint", Color) = (0.5, 0.65, 0.75, 1)
		_SpecColor ("Specular Color", Color) = (1, 0.25, 0, 1)
		_Glossiness ("Glossiness", Float) = 64
		_RimColor ("Rim Color", Color) = (0, 0, 1, 1)

			//v0.1
			sssPower("sssPower", Float) = 1.5
			sssEmit("sssEmit", Float) = 1
			sssScatter("sssScatter", Color) = (0, 0.6, 0.3, 1)
			wavesHeight("wavesHeight", Float) = 1
			
	}

	SubShader
	{
		Pass
		{
			Cull Back
			ZWrite On
			ZTest LEqual
			ColorMask RGB

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _Anim;
			uniform sampler2D _Height;
			uniform sampler2D _Bump;
			uniform sampler2D _White;

			uniform float4 _Tint;
			uniform float4 _SpecColor;
			uniform float _Glossiness;
			uniform float _LightWrap;
			uniform fixed4 _RimColor;

			uniform float4 _LightColor0;

			//v0.1
			float sssPower;
			float sssEmit;
			float4 sssScatter; float wavesHeight;

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				float4 color : TEXCOORD2;
				float3 lightDir : TEXCOORD3;
				float3 viewDir : TEXCOORD4;
				UNITY_VERTEX_OUTPUT_STEREO //VR1
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;

				UNITY_SETUP_INSTANCE_ID(v); //v1.7.8 //VR1
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o); //Insert //VR1
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert //VR1		

				v.vertex.y += tex2Dlod(_Height, v.texcoord).r / 8;
				v.vertex.xz += tex2Dlod(_Anim, v.texcoord).rb / 8;

				o.pos = UnityObjectToClipPos(v.vertex);

				o.normal = UnityObjectToWorldNormal(tex2Dlod(_Bump, v.texcoord).rgb);

				o.color = tex2Dlod(_White, v.texcoord).r;

				o.lightDir = normalize(WorldSpaceLightDir(v.vertex));
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				o.texcoord = v.texcoord;

				return o;
			}

			//TUTORIAL
			float4 sssColor(float3 lightDir, float3 normal, float3 view, float height)
			{
				float HA = saturate(height / wavesHeight);
				float light = sqrt(saturate(lightDir.y));
				float sss = pow(saturate(dot(lightDir,view)) + saturate(dot(-lightDir,normal)), sssPower) * light * sssEmit;
				return sssScatter * (sss + HA * 0.62);
			}


			float4 frag(VertexOutput i) : COLOR
			{
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

				i.normal = tex2D(_Bump, i.texcoord).rgb;
				i.normal = normalize(i.normal);
				i.normal = UnityObjectToWorldNormal(i.normal);
				float4 diffuse = saturate(dot(i.normal, i.lightDir));
				diffuse = pow(saturate(diffuse * (1 - _LightWrap) + _LightWrap), 2 * _LightWrap + 1) * _Tint * _LightColor0;
				i.viewDir = normalize(i.viewDir);
				i.lightDir = normalize(i.lightDir);
				float3 H = normalize(i.viewDir + i.lightDir);
				float NdotH = saturate(dot(i.normal, H));
				float4 specular = _SpecColor * saturate(pow(NdotH, _Glossiness)) * _LightColor0;
				float4 rim = _RimColor * pow(max(0, 1 - saturate(dot(i.normal, i.viewDir))), 1.5);
				//return saturate(pow(NdotH, _Glossiness));
				//return diffuse + specular + pow(i.color / 2, 2) + rim; //v0.1
				return diffuse + specular + pow(i.color / 2, 2) + rim + sssColor(i.lightDir, i.normal, -i.viewDir, i.pos.y);
			}

			ENDCG
		}
	}
}