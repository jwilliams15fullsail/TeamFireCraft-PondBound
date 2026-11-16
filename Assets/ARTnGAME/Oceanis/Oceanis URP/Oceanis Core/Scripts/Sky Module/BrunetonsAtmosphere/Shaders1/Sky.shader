// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BrunetonsAtmosphere/Sky" 
{
	Properties{
		_SampleCount0("Sample Count (min)", Float) = 30
		_SampleCount1("Sample Count (max)", Float) = 90
		_SampleCountL("Sample Count (light)", Int) = 16

		[Space]
	_NoiseTex1("Noise Volume", 3D) = ""{}
	_NoiseTex2("Noise Volume", 3D) = ""{}
	_NoiseFreq1("Frequency 1", Float) = 3.1
		_NoiseFreq2("Frequency 2", Float) = 35.1
		_NoiseAmp1("Amplitude 1", Float) = 5
		_NoiseAmp2("Amplitude 2", Float) = 1
		_NoiseBias("Bias", Float) = -0.2

		[Space]
	_Scroll1("Scroll Speed 1", Vector) = (0.01, 0.08, 0.06, 0)
		_Scroll2("Scroll Speed 2", Vector) = (0.01, 0.05, 0.03, 0)

		[Space]
	_Altitude0("Altitude (bottom)", Float) = 1500
		_Altitude1("Altitude (top)", Float) = 3500
		_FarDist("Far Distance", Float) = 30000

		[Space]
	_Scatter("Scattering Coeff", Float) = 0.008
		_HGCoeff("Henyey-Greenstein", Float) = 0.5
		_Extinct("Extinction Coeff", Float) = 0.01

		[Space]
	_SunSize("Sun Size", Range(0,1)) = 0.04
		_AtmosphereThickness("Atmoshpere Thickness", Range(0,5)) = 1.0
		_SkyTint("Sky Tint", Color) = (.5, .5, .5, 1)
		_GroundColor("Ground", Color) = (.369, .349, .341, 1)
		_Exposure("Exposure", Range(0, 8)) = 1.3

		_CloudBaseColor("_CloudBaseColor", Color) = (.369, .349, .341, 1)
		_CloudTopColor("_CloudTopColor", Color) = (.769, .749, .741, 1)

		//v3.5 clouds
		_BackShade("Back shade of cloud top", Float) = 1
		_UndersideCurveFactor("Underside Curve Factor", Float) = 0

		//v3.5.3
		_InteractTexture("_Interact Texture", 2D) = "white" {}
		_InteractTexturePos("Interact Texture Pos", Vector) = (1 ,1, 0, 0)
		_InteractTextureAtr("Interact Texture Attributes - 2multi 2offset", Vector) = (1 ,1, 0, 0)
		_InteractTextureOffset("Interact Texture offsets", Vector) = (0 ,0, 0, 0) //v4.0

																				  //v2.1.19
		_fastest("Fastest mode", Int) = 0
		_LocalLightPos("Local Light Pos & Intensity", Vector) = (0 ,0, 0, 0) //local light position (x,y,z) and intensity (w)			 
		_LocalLightColor("Local Light Color & Falloff", Vector) = (0 , 0, 0, 2) //w = _LocalLightFalloff

		
	
		_invertX("Mirror X", Float) = 0
		_invertRay("Mirror Ray", Float) = 1
	
		//HDRP v0.3
		_SunTexture("Sun Texture", 2D) = "white" {}
		_MoonTexture("Moon Texture", 2D) = "white" {}
		_MoonBumpTexture("Moon Bump Texture", 2D) = "white" {}
		_GalaxyTexture("Galaxy Texture", 2D) = "white" {}
		_JitterTexture("Jitter Texture", 2D) = "white" {}

		//v0.3 - sun rays
		raysResolution("Sun Rays resolution", Vector) = (1 , 1, 1, 1)
		_rayColor("Ground", Color) = (.969, 0.549, .041, 1)
		rayShadowing("Ray Shadows", Vector) = (1 , 1, 1, 1)
		_underDomeColor("_underDomeColor", Color) = (1, 1, 1, 1)


		//	float _Angle = 20;
		_Angle("_Angle", Float) = 20
		_moonPos("Moon effects and size (w)", Vector) = (1 , 1, 0, 0)
		_sunShading("Sun effects and size (w)", Vector) = (1 , 1, 0, -0.45)
		_galaxyShading("Galaxy effects and size (w)", Vector) = (1 , 1, 0, -0.45)
		_galaxyPos("Galaxy effects and size (w)", Vector) = (0,0,0,0)

			_HorizonYAdjust("Adjust horizon Height", Float) = 0
	}

	SubShader 
	{
    	Pass 
    	{
    		//ZWrite Off
			//Cull Front
			Cull Off
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "Atmosphere.cginc"

			//VOLUMETRIC CLOUD
			/*struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};*/
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 rayDir : TEXCOORD1;
				float3 groundColor : TEXCOORD2;
				float3 skyColor : TEXCOORD3;
				float3 sunColor : TEXCOORD4;
				float4 worldPos : TEXCOORD5;
				UNITY_VERTEX_OUTPUT_STEREO //VR1
			};
			#include "ProceduralSky.cginc"
		
			

			v2f vert(appdata_base v)
			{
    			v2f OUT;

				UNITY_SETUP_INSTANCE_ID(v); //v1.7.8 //VR1 UNITY_VERTEX_INPUT_INSTANCE_ID //v1.7.8 //VR1
				UNITY_INITIALIZE_OUTPUT(v2f, OUT); //Insert //VR1  UNITY_VERTEX_OUTPUT_STEREO //VR1
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); //Insert //VR1	

    			OUT.vertex = UnityObjectToClipPos(v.vertex);
    			OUT.worldPos = mul(unity_ObjectToWorld, v.vertex).xyzw;
				OUT.uv = v.texcoord.xy; //(OUT.vertex.xy / OUT.vertex.w + 1) * 0.5;// v.texcoord.xy;

				//VOLUMETRIC CLOUDS
				vert_sky(v.vertex.xyz, OUT);

    			return OUT;
			}



			//VOLUMETRIC CLOUDS
			float _SampleCount0;
			float _SampleCount1;
			int _SampleCountL;

			sampler3D _NoiseTex1;
			sampler3D _NoiseTex2;
			float _NoiseFreq1;
			float _NoiseFreq2;
			float _NoiseAmp1;
			float _NoiseAmp2;
			float _NoiseBias;

			float3 _Scroll1;
			float3 _Scroll2;

			float _Altitude0;
			float _Altitude1;
			float _FarDist;

			float _Scatter;
			float _HGCoeff;
			float _Extinct;

			//v0.2
			//float3 _SkyTint;
			//float _SunSize;
			//float3 _GroundColor; //v4.0
			//float _Exposure; //v4.0
			//v2.1.19
			int _fastest;
			float4 _LocalLightPos;
			float4 _LocalLightColor;
			//v3.5.3
			sampler2D _InteractTexture;
			float4 _InteractTexturePos;
			float4 _InteractTextureAtr;
			float4 _InteractTextureOffset; //v4.0
										   //v4.8
			float _invertX = 0;
			float _invertRay = 1;
			float _BackShade;
			float _UndersideCurveFactor;

			//HDRP v0.3
			sampler2D _SunTexture;
			sampler2D _MoonTexture;
			sampler2D _MoonBumpTexture;
			sampler2D _GalaxyTexture;
			sampler2D _JitterTexture;

			//v0.3 - rays
			float4 raysResolution;
			float4 _rayColor;
			float4 rayShadowing;
			float4 _underDomeColor;

			//v0.4
			float3 SUN_DIR_EULER;

			//v0.5
			float4 _sunShading;
			float4 _galaxyShading;
			float4 _galaxyPos;
			float _HorizonYAdjust;

			float UVRandom(float2 uv)
			{
				float f = dot(float2(12.9898, 78.233), uv);
				return frac(43758.5453 * sin(f));
			}

			//v0.2
			float SampleNoiseA(float3 uvw, float _Altitude1, float _NoiseAmp1, float Alpha)//v3.5.3
			{

				float AlphaFactor = clamp(Alpha*_InteractTextureAtr.w, _InteractTextureAtr.x, 1);

				const float baseFreq = 1e-5;

				float4 uvw1 = float4(uvw * _NoiseFreq1 * baseFreq, 0);
				float4 uvw2 = float4(uvw * _NoiseFreq2 * baseFreq, 0);

				uvw1.xyz += _Scroll1.xyz * _Time.x;
				uvw2.xyz += _Scroll2.xyz * _Time.x;

				float n1 = tex3Dlod(_NoiseTex1, uvw1).a;
				float n2 = tex3Dlod(_NoiseTex2, uvw2).a;
				float n = n1 * _NoiseAmp1*AlphaFactor + n2 * _NoiseAmp2;//v3.5.3

				n = saturate(n + _NoiseBias);

				float y = uvw.y - _Altitude0;
				float h = _Altitude1 * 1 - _Altitude0;//v3.5.3
				n *= smoothstep(0, h * (0.1 + _UndersideCurveFactor), y);
				n *= smoothstep(0, h * 0.4, h - y);

				return n;
			}

			float SampleNoise(float3 uvw)
			{
				const float baseFreq = 1e-5;

				float4 uvw1 = float4(uvw * _NoiseFreq1 * baseFreq, 0);
				float4 uvw2 = float4(uvw * _NoiseFreq2 * baseFreq, 0);

				uvw1.xyz += _Scroll1.xyz * _Time.x;
				uvw2.xyz += _Scroll2.xyz * _Time.x;

				float n1 = tex3Dlod(_NoiseTex1, uvw1).a;
				float n2 = tex3Dlod(_NoiseTex2, uvw2).a;
				float n = n1 * _NoiseAmp1 + n2 * _NoiseAmp2;

				n = saturate(n + _NoiseBias);

				float y = uvw.y - _Altitude0;
				float h = _Altitude1 - _Altitude0;
				n *= smoothstep(0, h * 0.1, y);
				n *= smoothstep(0, h * 0.4, h - y);

				return n;
			}

			float HenyeyGreenstein(float cosine)
			{
				float g2 = _HGCoeff * _HGCoeff;
				return 0.5 * (1 - g2) / pow(1 + g2 - 2 * _HGCoeff * cosine, 1.5);
			}

			float Beer(float depth)
			{
				return exp(-_Extinct * depth * _BackShade);
			}

			float BeerPowder(float depth)
			{
				return exp(-_Extinct * depth) * (1 - exp(-_Extinct * 2 * depth));
			}

			float MarchLight(float3 pos, float rand)
			{
				float3 light = _WorldSpaceLightPos0.xyz;
				float stride = (_Altitude1 - pos.y) / (light.y * _SampleCountL);

				pos += light * stride * rand;

				float depth = 0;
				UNITY_LOOP for (int s = 0; s < _SampleCountL; s++)
				{
					depth += SampleNoise(pos) * stride;
					pos += light * stride;
				}

				return BeerPowder(depth);
			}

			//v0.2
			float MarchLightA(float3 pos, float rand, float _Altitude1, float _NoiseAmp1, float Alpha)
			{
				float3 light = float3(SUN_DIR.x, _invertRay * SUN_DIR.y, SUN_DIR.z);//v3LightDir;// _WorldSpaceLightPos0.xyz; //v4.8
				float stride = (_Altitude1 - pos.y) / (light.y * _SampleCountL);

				//v3.5.2
				if (_invertRay * SUN_DIR.y < 0) {//if(_WorldSpaceLightPos0.y < 0){  //v4.8
													//if(_WorldSpaceLightPos0.y > -0.01){         
					stride = (_Altitude0 - pos.y + _WorldSpaceCameraPos.y) / (light.y * _SampleCountL * 15); //higher helps frame rate A LOT
																											  //}
				}

				pos += light * stride * rand;

				float depth = 0;
				UNITY_LOOP for (int s = 0; s < _SampleCountL; s++)
				{
					depth += SampleNoiseA(pos, _Altitude1, _NoiseAmp1, Alpha) * stride;
					pos += light * stride;
				}

				return BeerPowder(depth);
			}

			float3 _CloudTopColor;
			float3 _CloudBaseColor;
			//END VOLUMETRIC CLOUDS


			float _Angle = 20;
			float4 _moonPos;

			//FRAGMENT
			float4 frag(v2f i) : COLOR
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);//VR1

			    float3 dir = normalize(i.worldPos-_WorldSpaceCameraPos);


				//ADD SUN HDRP v0.3		
				float3 forwardCamera = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)); //
				float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
				float enchanceOpaque = 0; //make sun hide behind clouds
				float enchanceOpaqueB = 0;
				float enchanceOpaqueG = 0;
				float4 worldPs = (i.worldPos) + float4(512 / 2 - 0, 512 + 150, 0, _moonPos.w) - float4(_WorldSpaceCameraPos, 0);// +);// mul(i.worldPos, dir);// i.worldPos;// mul(i.worldPos, forwardCamera);
				//float4 worldPs = (i.worldPos) + float4(-512, 512, 0, _moonPos.w) - float4(_WorldSpaceCameraPos, 0);
										

				//ADD MOON
				//3D rotate world position around UP Y axis
				float horizontalAngle = _moonPos.y;
				horizontalAngle = horizontalAngle * 3.14159 / 180;
				float3x3 matRot3 = float3x3(cos(horizontalAngle), 0, sin(horizontalAngle),
					0, 1, 0,
					-sin(horizontalAngle), 0, cos(horizontalAngle));
				float3 worldPsA = mul(worldPs.xyz, matRot3);
				float verticalAngle = _moonPos.x;
				verticalAngle = verticalAngle * 3.14159 / 180;
				float3x3 matRot4 = float3x3(1, 0, 0,
					0, cos(verticalAngle), -sin(verticalAngle),
					0, sin(verticalAngle), cos(verticalAngle));
				worldPsA = mul(worldPsA, matRot4);


				float2 ndc = float2(worldPsA.x / worldPs.w, worldPsA.y / worldPs.w);
				float2 uvSun = (1 + float2(ndc.x, ndc.y)) * 0.5;
				//float _Angle = 20;
				//float theta = _Angle * 3.14159 / 180;
				//float2x2 matRot = float2x2(cos(theta), sin(theta),
				//	-sin(theta), cos(theta));
				//uvSun = mul(uvSun, matRot);
				//float4 texMoon = tex2D(_SunTexture, uvSun * 0.0005 + float2(0.0,0.0));

				//int signA = sign(dot(dir, worldPs));
				//float3 forwardVector = float3(0, 0, 1);
				//float3 rotationVector = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz - _WorldSpaceCameraPos;
				//float rotRad =  acos(dot(normalize(rotationVector.xz), forwardVector.xz)); //atan2(rotationVector.x, rotationVector.z);//

				//make direction vector
				float3 one = float3(0, 0, 1);
				
				//one.xyz = mul(one.xyz, matRot3);
				//one.xyz = mul(one.xyz, matRot4);

				//float dir2 = mul(dir, matRot3);
				//dir2 = mul(dir2, matRot4);
				//float signA = dot(normalize(one + _WorldSpaceCameraPos), normalize(worldPsA + _WorldSpaceCameraPos)); //dot(worldPs, forwardCamera);
				//float signA = dot(normalize(forwardCamera), normalize(-one + _WorldSpaceCameraPos));
				//float signA = dot(normalize(one), normalize(_WorldSpaceCameraPos));
				float signA = dot(normalize(one), normalize(worldPsA));

				float4 texMoon = tex2D(_MoonTexture, (float2((uvSun.x)	, (uvSun.y)) * 0.001));// *dot(dir, SUN_DIR);// *dot(dir, _WorldSpaceCameraPos);// +float2(_moonPos.x, _moonPos.y));
				
				



				//ADD GALAXY HDRP v0.3
				//_galaxyPos
				horizontalAngle = _galaxyPos.y;
				horizontalAngle = horizontalAngle * 3.14159 / 180;
				matRot3 = float3x3(cos(horizontalAngle), 0, sin(horizontalAngle),
					0, 1, 0,
					-sin(horizontalAngle), 0, cos(horizontalAngle));
				worldPsA = mul(worldPs.xyz, matRot3);
				verticalAngle = _galaxyPos.x;
				verticalAngle = verticalAngle * 3.14159 / 180;
				matRot4 = float3x3(1, 0, 0,
					0, cos(verticalAngle), -sin(verticalAngle),
					0, sin(verticalAngle), cos(verticalAngle));
				worldPsA = mul(worldPsA, matRot4);
				ndc = float2(worldPsA.x / _galaxyShading.w, worldPsA.y / _galaxyShading.w);
				float2 uvSun1 = (1 + float2(ndc.x, ndc.y)) * 0.5;
				float _Light = 3.79 * _galaxyShading.y;
				float4 _CloudSpeed = float4(0.5, 1, 0, 0) * (_galaxyShading.z+1);// float4(0.005, 0, 0, 0);
				float _CloudDensity = 0.16;// 1.85; //scale galaxy
				float _CloudSharpness = 13.4;
				float _CloudCover = 0.547;
				float4 _Color = float4(230.0 / 255.0, 230.0 / 255.0, 230.0 / 255.0, 206.0 / 255.0);
				float2 TexC = float2((uvSun1.x), (uvSun1.y)) * 0.001;// float2(i.uv.x, i.uv.y);
				float2 offset = _Time.y * _CloudSpeed.xy;
				TexC.y = clamp(TexC.y, -11, 3);
				float4 texG = tex2D(_GalaxyTexture, TexC * _CloudDensity + float2(_galaxyPos.z, _galaxyPos.w));
				float Bumped_lightingN = 1;
				float4 tex2N = tex2D(_JitterTexture, (TexC + offset / 15) * _CloudDensity * 11);
				//texG = max(texG - (1 - _CloudCover * 2), 0);
				float4 colGALAXY = //_Color.a * lerp(pow(tex2N, 2), 0.6, _CloudSharpness) * 10 * texG * 10;
					//texG + tex2N;// _Color.a * lerp(pow(tex2N, 2), 0.6, _CloudSharpness) *
					_Color.a * //lerp(pow(tex2N, 2), 0.6, _CloudSharpness) * //v0.4
					float4 (0.95 * _Color.r * Bumped_lightingN * texG.r, 
							0.95 * _Color.g * Bumped_lightingN * texG.g, 
							0.95 * _Color.b * Bumped_lightingN * texG.b, 
							_Color.a) * _galaxyShading.x;
				//sky += res.rgb * (1-res.a);
				//return pow(abs(res), _Light);
				colGALAXY += pow(abs(colGALAXY), _Light)/1;
				colGALAXY = colGALAXY / 4;
				//colGALAXY = texG;
				if (colGALAXY.r > 0.04 || colGALAXY.g > 0.021 || colGALAXY.b > 0.021) {
					enchanceOpaqueG = 1;				
				}


			    
				float sun = 0.005 * dot(dir, SUN_DIR);// step(cos(M_PI / 360.0), dot(dir, SUN_DIR)); //v0.4


				//APPLY SUN TEXTURE HERE v0.4
				//3D rotate world position around UP Y axis
				//float3 SUN_DIR2 = SUN_DIR;
				//SUN_DIR2 = float3(SUN_DIR2.x,0, SUN_DIR2.z);
				//SUN_DIR2 = SUN_DIR2 / length(SUN_DIR2);
				//horizontalAngle = 90 - acos(SUN_DIR2.x);// (360 + 90 - acos(SUN_DIR2.x));
				//if(horizontalAngle < 0)
				//{
				//	horizontalAngle = 180 + horizontalAngle;
				//}
				horizontalAngle = -(180-SUN_DIR_EULER.y);
				horizontalAngle = horizontalAngle * 3.14159 / 180;
				matRot3 = float3x3(cos(horizontalAngle), 0, sin(horizontalAngle),
					0, 1, 0,
					-sin(horizontalAngle), 0, cos(horizontalAngle));
				
				//SUN_DIR2 = float3(0, SUN_DIR2.y, SUN_DIR2.z);
				//SUN_DIR2 = SUN_DIR2 / length(SUN_DIR2);
				//verticalAngle = 0;// (90 - asin(SUN_DIR2.y));
				//verticalAngle = (asin(SUN_DIR2.y));// (SUN_DIR2.y);
				//verticalAngle = verticalAngle * 1;// 3.14159 / 180;
				verticalAngle = -SUN_DIR_EULER.x;
				verticalAngle = verticalAngle * 3.14159 / 180;
				matRot4 = float3x3(1, 0, 0,
					0, cos(verticalAngle), -sin(verticalAngle),
					0, sin(verticalAngle), cos(verticalAngle));
				float3 worldPsB = mul(worldPs.xyz, matRot3);
				worldPsB = mul(worldPsB, matRot4);


				float _SunScale = 1;
				ndc = float2(worldPsB.x*_SunScale / _sunShading.w, worldPsB.y*_SunScale / _sunShading.w);
				float2 uvSun2 = (1 + float2(ndc.x, ndc.y)) * 0.5;
								
				//signA = dot(normalize(one), normalize(worldPsB)); //dot(worldPs, forwardCamera);
				float signA1 = dot(normalize(one), normalize(worldPsB));

				float _sunNoiseSpeed = 0.00005;
				float _sunNoiseStrength = 1761;
				/*uvSun.x += cos(tex2N*_Time.x*_sunNoiseSpeed) * _sunNoiseStrength;
				uvSun.y += sin(tex2N*_Time.x*_sunNoiseSpeed) * _sunNoiseStrength;*/
				//uvSun2.x += -210+1+cos(tex2N*_Time.x*_sunNoiseSpeed) * _sunNoiseStrength;
				//uvSun2.y += 511+sin(tex2N*_Time.x*_sunNoiseSpeed) * _sunNoiseStrength;
				//uvSun2.x += 0 + 0 + cos(tex2N*_Time.x*_sunNoiseSpeed) * _sunNoiseStrength;
				//uvSun2.y += 0 + sin(tex2N*_Time.x*_sunNoiseSpeed) * _sunNoiseStrength;

				//float4 texSun = tex2D(_SunTexture, (float2((uvSun.x-2*572), (uvSun.y+256)) * 0.001 + 1*float2(0.04*cos(uvSun.y*_Time.y*0.05), 0.04*cos(uvSun.x*_Time.y*0.05))));// *dot(dir, SUN_DIR);// *dot(dir, _WorldSpaceCameraPos);// +float2(_moonPos.x, _moonPos.y));
				float4 texSun =2* tex2D(_SunTexture, (float2((uvSun2.x + 2*320), (uvSun2.y +128)) * 0.001
					//+ 0.1*cos(_Time.x) * float2(1+0.04*cos(uvSun2.y*1*0.05+ _Time.x),1+ 0.04*cos(uvSun2.x*1*0.05))
					*  float2(1 + 0.03*cos(uvSun2.y * _Time.y * 1.05 + _Time.y), 1 + 0.03*cos(uvSun2.x * _Time.y * 1.05 + _Time.y))
					)
				);
				if (signA1 > -0.1 && (texSun.r > 0.04 || texSun.g > 0.021 || texSun.b > 0.021)) { //-0.35
					enchanceOpaqueB = 1;				
				}
				


				//MOON PHASES
				//moon hases SUN_DIR
				//texMoon = texMoon - _sunShading.x*0.0001*texMoon * abs(dot(SUN_DIR, float3(uvSun.x, uvSun.y, 0)));
				//texMoon = texMoon + _sunShading.x*0.0001*texMoon * abs(dot(SUN_DIR, float3(uvSun.x, uvSun.y, 0)));
				//float distCenter = length(float3(uvSun2.x, uvSun2.y, 0) - float3(uvSun.x, uvSun.y, 0));
				//texMoon = texMoon + distCenter*_sunShading.x*0.0000000000003*texMoon * abs(dot(float3(uvSun2.x, uvSun2.y, 0), float3(uvSun.x, uvSun.y, 0)));

				if (signA > -0.1 && (texMoon.r > 0.04 || texMoon.g > 0.021 || texMoon.b > 0.021)) { //-0.35 signA > 0.3 && 
					//if (uvSun.x < 0 || uvSun.y < 0 ||
					//	uvSun.x > 1 || uvSun.y > 1 )// || c.a <= 0.00f)
					//{
					//DO NOTHING
					//sky += texMoon / 1.2;
					//}
					//else {
					//sky	=  texMoon * 11; 
					enchanceOpaque = 1;
					//}
				}





			    
			    float3 sunColor = float3(sun,sun,sun) * SUN_INTENSITY;

				float3 extinction;
				float3 inscatter = SkyRadiance(_WorldSpaceCameraPos + float3(0,1000,0), dir, extinction); //added float3(0,1000,0) to fix clouds going white under water
				float3 col = sunColor * extinction + inscatter;

				//VOLUMETRIC CLOUDS
				float3 sky = float3(0.0, 0.0, 0.0) + col;// col;// frag_sky(i, col);

				/*if (enchanceOpaque == 0 && enchanceOpaqueB == 0){
					sky += colGALAXY / 1;
				}*/
				

				
					


				float3 ray = -i.rayDir + float3(0,-0.01 * _HorizonYAdjust,0);
				int samples = lerp(_SampleCount1, _SampleCount0, ray.y);

				float dist0 = _Altitude0 / ray.y;
				float dist1 = _Altitude1 / ray.y;
				float stride = (dist1 - dist0) / samples;

				//if (ray.y < 0.01 || dist0 >= _FarDist) return float4(_underDomeColor.rgb, 1);// sky, 1);
				if (ray.y < 0.066 || dist0 >= _FarDist) return float4(_underDomeColor.rgb, 1);// sky, 1); //v0.2

				float3 light = SUN_DIR;// _WorldSpaceLightPos0.xyz;
				float hg = HenyeyGreenstein(dot(ray, light));

				float2 uv = i.uv + _Time.x;
				float offs = UVRandom(uv) * (dist1 - dist0) / samples;

				float3 pos = _WorldSpaceCameraPos + ray * (dist0 + offs);
				float3 acc = 0;

				//v0.2
				float preDevide = samples / _Exposure;
				float3 groundColor1 = _GroundColor.rgb*0.1;
				float3 light1 = _CloudTopColor * _SkyTint;
				float3 intensityMod = _LocalLightPos.w * _LocalLightColor.xyz * pow(10, 7);
				float scatterHG = _Scatter * hg;

				float depth = 0;
				UNITY_LOOP for (int s = 0; s < samples; s++)
				{

					//v0.2
					float4 texInteract = tex2Dlod(_InteractTexture, 0.0003*float4(
						_InteractTexturePos.x*pos.x + _InteractTexturePos.z*-_Scroll1.x * _Time.x + _InteractTextureOffset.x,
						_InteractTexturePos.y*pos.z + _InteractTexturePos.w*-_Scroll1.z * _Time.x + _InteractTextureOffset.y,
						0, 0));					
					//texInteract.a = texInteract.a - _InteractTextureAtr.z * (1 - 0.00024*length(_LocalLightPos.xyz - pos));
					float diffPos = length(_LocalLightPos.xyz - pos);
					texInteract.a = texInteract.a + clamp(_InteractTextureAtr.z * 0.1*(1 - 0.00024*diffPos), -1.5, 0);
					//_NoiseAmp2 = _NoiseAmp2 * clamp(texInteract.a*_InteractTextureAtr.w, _InteractTextureAtr.y, 1);



					//v0.2
					//float n = SampleNoise(pos);
					float n = SampleNoiseA(pos, _Altitude1, _NoiseAmp1, texInteract.a);


					//v0.2
					float expose = 0.00001;
					if (s < preDevide) {  //if(s < samples/3){
						expose = 0;
					}

					float lightAtten = dot(light / length(light), pos / length(pos));


					if (n > expose)//if (n >= expose)//if (n > 0) //v0.2
					{
						float density = n * stride;
						float rand = UVRandom(uv + s + 1);
						//float scatter = density * _Scatter * hg * MarchLight(pos, rand * 0.5);
						//acc += _LightColor0 * scatter * BeerPowder(depth); //_CloudBaseColor, _CloudTopColor

						//v0.2
						float scatter = density * scatterHG * MarchLightA(pos, rand * 0.001, _Altitude1, _NoiseAmp1, texInteract.a); //v4.0

						//acc += _CloudTopColor * scatter * BeerPowder(depth);


						//v0.2
						float3 beer1 = BeerPowder(depth) * intensityMod / pow(diffPos, _LocalLightColor.w);
						float beer2 = 1 - Beer(depth);
						acc += light1 * scatter * BeerPowder(depth) + beer2 * groundColor1 + (beer2*0.01*_CloudTopColor + scatter) * beer1;


						depth += density;
					}
					else if (raysResolution.x > 0 && abs(lightAtten > 0.5 * rayShadowing.y) ) { //else if (raysResolution.x > 0){
						//HDRP v0.3 - sun shafts
						//float diffPosA = dot(_LocalLightPos.xyz, ray);
						//float directSun = diffPosA;
						float rand = UVRandom(uv + s + 1);	
						float divider = 24 / (raysResolution.x);
						//float MarchLight(float3 pos, float rand)
						//{
						float3 light = SUN_DIR;// _WorldSpaceLightPos0.xyz;
							float strideA = (_Altitude1 - pos.y) / (abs(light.y) * _SampleCountL);
							float3 posA = pos;
							//posA += light * strideA * rand* divider * 2 * raysResolution.y;
							posA += light * strideA * divider * 2 * raysResolution.y;

							float AdjustAtten = lightAtten - 0.5 * rayShadowing.y;

							float depth = 0;
							if (raysResolution.w == 0) {

								

									//UNITY_LOOP for (int s = 0; s < _SampleCountL / divider; s++)
									//{
									//depth += SampleNoise(posA) * strideA * 1 * raysResolution.z;
								//depth += SampleNoise(posA) * strideA * rayShadowing.x * raysResolution.z;// -depther;
								depth += SampleNoise(posA) * strideA * rayShadowing.x * raysResolution.z * 1;// -depther;
									//posA += light * strideA * divider * 2 * raysResolution.w;
									//if (depth > -100000) {
										//break;
									//}
									//	}	

									//posA += light * strideA * rand* divider * 1 * raysResolution.y;
									//depth += SampleNoise(posA) * strideA * rayShadowing * raysResolution.z;
									//posA += light * strideA * rand* divider * 1 * raysResolution.y;
									//depth += SampleNoise(posA) * strideA * rayShadowing * raysResolution.z;
							}
							else {
								UNITY_LOOP for (int s = 0; s < _SampleCountL / divider; s++)
								{
									depth += SampleNoise(posA) * strideA * rayShadowing.x * raysResolution.z;
									posA += light * strideA * divider * 2 * raysResolution.w;
									//if (depth > -100000) {
									//break;
									//}
								}								
							}
							//float depther = BeerPowder(depth);
							float depther = BeerPowder(depth);
							//return BeerPowder(depth);
							//}

							if (depther == 0) {
								depther = 0.02 * 1;
							}
						//acc += 10*_rayColor.w * 0.06 * float3(_rayColor.x, _rayColor.y, _rayColor.z) * depther;// *directSun;
						acc += 10 * _rayColor.w * 0.06 * float3(_rayColor.x, _rayColor.y, _rayColor.z) * depther * pow(AdjustAtten,2);
					}
					pos += ray * stride;
				}

				//acc += Beer(depth) * sky;
				//acc = lerp(acc, sky, saturate(dist0 / _FarDist));

				//HDRP v0.3 - hide sun behind clouds with enchanceOpaque
				//sky = clamp(sky - float3(1, 1, 1)*enchanceOpaque, 0, 1);	
				float controlColor = 1; float3 controlColorA = float3(0,0,0);
				float sunColorextra = _sunShading.z * (0.1+0.07*cos(uvSun.y*_Time.y*0.02)+0.03 + 0.02*cos(uvSun.x*_Time.y*0.12));
				float3 tester = acc + Beer(depth);
				if (enchanceOpaque > 0){// && (acc.r > 0 || acc.g  > 0 || acc.b  > 0) ) { //v0.5
					//sky = 1*(acc * 0.4 + pow(Beer(depth),4)* texMoon* 0.4 * _moonPos.z + col);// +Beer(depth)*colGALAXY;
					sky += Beer(depth)*Beer(depth)*texMoon* 0.4 * _moonPos.z;
					//sky = sky * 0.3;
				}//else
				//if (enchanceOpaqueB > 0 && (acc.r > 0 || acc.g  > 0 || acc.b  > 0)) {
					//sky = Beer(depth)* texSun* 0.02 * _moonPos.z * acc + float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					//acc = 0;
					//controlColor = 0.02;
				//}
				if (enchanceOpaque == 0 && enchanceOpaqueB == 0) {
					//sky += colGALAXY / 1;
					sky += Beer(depth)*Beer(depth)*colGALAXY;
				}

				if (enchanceOpaque <= 0 && enchanceOpaqueB > 0 && (acc.r > 0 || acc.g  > 0 || acc.b  > 0)) {
					//sky = Beer(depth)* texSun* 0.02 * _moonPos.z * acc + float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					//acc += Beer(depth)* texSun* 0.02 * _sunShading.z * acc + Beer(depth)*float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					//acc += Beer(depth)* texSun* 0.02 * _sunShading.z * acc + Beer(depth)*float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					sky += Beer(depth)*Beer(depth)* (texSun* 0.02 * _sunShading.z + 1 * float3(1, 0.05, 0) * sunColorextra * 12);

					//acc = 0;
					controlColor = 1;
					//controlColorA = 0.1 * acc;
					//controlColorA = 0.00133 + float3(0.002,0,0);
					controlColorA = Beer(depth)* (float3(0.6, 0, 0) + acc * acc * acc * 0.0001);
				}

				acc += Beer(depth) * sky;
				acc = lerp(acc, sky, saturate(dist0 / _FarDist));
				
				if (enchanceOpaque <= 0 &&  enchanceOpaqueB > 0 && (acc.r > 0 || acc.g  > 0 || acc.b  > 0)) {
					//sky = Beer(depth)* texSun* 0.02 * _moonPos.z * acc + float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					//acc += Beer(depth)* texSun* 0.02 * _sunShading.z * acc + Beer(depth)*float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					//acc += Beer(depth)* texSun* 0.02 * _sunShading.z * acc + Beer(depth)*float3(1, 0.05, 0) * sunColorextra * 120 + acc;
					//acc += Beer(depth)*Beer(depth)* (texSun* 0.02 * _sunShading.z + 1*float3(1, 0.05, 0) * sunColorextra * 1);

					//acc = 0;
					//controlColor = 1;
					//controlColorA = 0.1 * acc;
					//controlColorA = 0.00133 + float3(0.002,0,0);
					controlColorA = Beer(depth)* (float3(0.6, 0, 0)+ acc * acc * acc * 0.0001);
				}

				//END VOLUMETRIC CLOUDS
		
				//return float4(hdr(col)*0.5 + acc , 1.0);
				//return float4(hdr(col)*acc + acc, 1.0);
				//return float4(acc * sky * 3 * controlColor + acc * controlColor + controlColorA + hdr(col)*0.1, 1);
				return float4(acc * sky * 3 * controlColor + acc * controlColor + 0 + 0, 1);
			}
			
			ENDCG

    	}
	}
}