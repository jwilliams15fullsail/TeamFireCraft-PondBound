// Water for a flat surface (a Plane) in URP: ripples that move on their own, a colour that gets
// deeper with the water's depth, a bent view of what is underneath, reflections, and foam where
// the water touches walls, floors and frogs.
// Flowing: the ripples run along the object's blue (Z) axis, for a stream in a pipe.
// RisingWater drives the rest: a stream that arrives behind a ragged, foamy front, and water that
// spreads out across a room's floor from where it pours in, with ripples rushing outward from there.
// Needs Depth Texture and Opaque Texture on in the URP asset (both are on in PC_RPAsset).
// Draws after StickyFluid particles, so particles under the water are hidden by it.
Shader "PondBound/WaterSurface"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.36, 0.52, 0.46, 1)
        _DeepColor ("Deep Color", Color) = (0.05, 0.14, 0.12, 1)
        _DepthFade ("Depth Fade (m)", Range(0.05, 5)) = 0.8
        _Clarity ("Clarity", Range(0, 1)) = 0.6
        _Refraction ("Refraction", Range(0, 0.1)) = 0.03
        _WaveSize ("Wave Size (m)", Range(0.1, 10)) = 1.2
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.4
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.3
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _Reflection ("Reflection", Range(0, 1)) = 0.7
        _FoamColor ("Foam Color", Color) = (0.85, 0.9, 0.85, 1)
        _FoamWidth ("Foam Width (m)", Range(0, 0.5)) = 0.12
        [ToggleUI] _Flowing ("Flowing (along the blue Z axis)", Float) = 0
        _FlowSpeed ("Flow Speed (m/s)", Range(0, 5)) = 1.5
        _FlowFoam ("Flow Foam", Range(0, 1)) = 0.5
        _InflowRadius ("Inflow Radius (m)", Range(0.5, 20)) = 6
        _InflowSpeed ("Inflow Speed (m/s)", Range(0, 5)) = 1.5
        [HideInInspector] _FlowOffset ("Flow Offset", Float) = 0
        [HideInInspector] _Reach ("Reach", Float) = 100000
        [HideInInspector] _ReachStart ("Reach Start", Vector) = (0, 0, 0, 0)
        [HideInInspector] _Spread ("Spread", Float) = 100000
        [HideInInspector] _InflowPoint ("Inflow Point", Vector) = (0, 0, 0, 0)
        [HideInInspector] _InflowDir ("Inflow Direction", Vector) = (0, 0, 1, 0)
        [HideInInspector] _InflowStrength ("Inflow Strength", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+10" }

        Pass
        {
            Name "WaterSurface"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            ZTest LEqual
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                float4 _ReachStart;
                float4 _InflowPoint;
                float4 _InflowDir;
                float _DepthFade;
                float _Clarity;
                float _Refraction;
                float _WaveSize;
                float _WaveSpeed;
                float _WaveStrength;
                float _Smoothness;
                float _Reflection;
                float _FoamWidth;
                float _Flowing;
                float _FlowSpeed;
                float _FlowFoam;
                float _InflowRadius;
                float _InflowSpeed;
                float _FlowOffset;
                float _Reach;
                float _Spread;
                float _InflowStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogFactor : TEXCOORD1;
                float2 flowDir : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                o.flowDir = normalize(TransformObjectToWorldDir(float3(0, 0, 1)).xz + float2(0, 1e-4)); // the object's blue (Z) axis
                return o;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Smooth noise: x = value, yz = its slope.
            float3 Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float2 du = 6.0 * f * (1.0 - f);
                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));
                float k = a - b - c + d;
                float value = a + (b - a) * u.x + (c - a) * u.y + k * u.x * u.y;
                return float3(value, du * float2(b - a + k * u.y, c - a + k * u.x));
            }

            // Two layers of moving noise make the ripples: x = a 0-1 value used for foam, yz = their slope.
            float3 Ripples(float2 p)
            {
                float t = _Time.y * _WaveSpeed;
                float3 n1 = Noise(p + t * float2(0.7, 0.3));
                float3 n2 = Noise(p * 2.3 - t * float2(0.4, 0.9));
                return float3(n1.x, (n1.yz + 0.5 * n2.yz) * _WaveStrength);
            }

            // Ripples running along dir, stretched out along it. 'along' and 'across' are metres
            // down and across the flow. Returns the same as Ripples, with the slope in world X/Z.
            float3 RunningRipples(float along, float across, float2 dir)
            {
                float3 r = Ripples(float2(along * 0.4, across) / _WaveSize);
                return float3(r.x, dir * (r.y * 0.4) + float2(-dir.y, dir.x) * r.z);
            }

            // Distance from the camera to whatever is behind this pixel, in metres.
            float SceneDistance(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            half4 Frag(Varyings i, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                float2 xz = i.positionWS.xz;
                float2 dir = i.flowDir;

                // Ripples: still, or running along the blue axis and carried downstream
                float3 ripples;
                half streaks = 0.0;
                if (_Flowing > 0.5)
                {
                    float travel = _FlowOffset + _Time.y * _FlowSpeed;
                    ripples = RunningRipples(dot(xz, dir) - travel, dot(xz, float2(-dir.y, dir.x)), dir);
                    streaks = saturate((ripples.x - 0.55) * 4.0) * _FlowFoam;
                }
                else
                    ripples = Ripples(xz / _WaveSize);
                float2 slope = ripples.yz;

                // A stream that is still arriving, or water still spreading out from where it pours in:
                // nothing past its ragged front, and foam along it
                float behindFront = min(_Reach - dot(xz - _ReachStart.xz, dir), _Spread - distance(xz, _InflowPoint.xz));
                behindFront += (ripples.x - 0.5) * 1.6;
                clip(behindFront);
                half frontFoam = saturate(1.0 - behindFront / 0.6);

                // Water pouring in at the Inflow Point: white water where it lands, then rings of
                // ripples and streaks of foam rushing outward from there
                if (_InflowStrength > 0.0)
                {
                    float2 away = xz - _InflowPoint.xz;
                    float dist = length(away);
                    float2 outward = away / max(dist, 0.001);
                    float run = dist - _Time.y * _InflowSpeed;
                    half rush = _InflowStrength * saturate(1.0 - dist / _InflowRadius);
                    rush *= rush;
                    // Rings, bent by the still ripples so they aren't perfect circles
                    slope += outward * (cos(run * 9.0 + ripples.x * 4.0) * _WaveStrength * 1.5 * rush);
                    // Lines of foam fanning out toward the middle of the water. The fan's angle wraps
                    // around behind the inflow, so the lines fade out before they get there.
                    float2 into = _InflowDir.xz;
                    float angle = dist > 0.001 ? atan2(dot(outward, float2(-into.y, into.x)), dot(outward, into)) : 0.0;
                    float foam = Ripples(float2(run * 0.6, angle * 3.0) / _WaveSize).x;
                    half ahead = saturate(dot(outward, into) * 2.0 + 1.0);
                    streaks = max(streaks, saturate(1.0 - abs(foam - 0.5) * 8.0) * rush * ahead);
                    streaks = max(streaks, saturate((1.0 - dist / 0.9) * 2.5 + (foam - 0.5) * 1.5) * _InflowStrength);
                }

                float3 n = normalize(float3(-slope.x, 1.0, -slope.y));
                if (!frontFace)
                    n = -n; // seen from under the water

                float3 v = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float surfaceDistance = -TransformWorldToView(i.positionWS).z;
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);

                // How much water is between the surface and what is under it
                float2 bentUV = screenUV + n.xz * _Refraction;
                if (SceneDistance(bentUV) < surfaceDistance)
                    bentUV = screenUV; // don't bend things that stick out in front of the water
                float waterDepth = max(SceneDistance(bentUV) - surfaceDistance, 0.0);
                float deep = saturate(waterDepth / _DepthFade);

                // Lighting
                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half shadow = light.shadowAttenuation;
                half3 lightColor = light.color * shadow;
                half3 ambient = SampleSH(n);

                // Water body: the floor shows through where it is shallow and clear
                half3 tint = lerp(_ShallowColor.rgb, _DeepColor.rgb, deep);
                half3 body = tint * (ambient + lightColor * saturate(dot(n, light.direction)));
                half3 under = SampleSceneColor(bentUV) * _ShallowColor.rgb * 1.4;
                half3 color = lerp(body, under, (1.0 - deep) * _Clarity);

                // Reflection, stronger at grazing angles
                half roughness = PerceptualSmoothnessToPerceptualRoughness(_Smoothness);
                half3 reflection = GlossyEnvironmentReflection(reflect(-v, n), i.positionWS, roughness, 1.0h);
                half fresnel = 0.02 + 0.98 * pow(1.0 - saturate(dot(n, v)), 5.0);
                color = lerp(color, reflection, saturate(fresnel * _Reflection));

                // Sun glint
                float3 halfDir = normalize(light.direction + v);
                half glint = pow(saturate(dot(n, halfDir)), exp2(10.0 * _Smoothness + 1.0));
                color += glint * lightColor * _Smoothness;

                // Foam where the water meets walls, the floor and frogs
                half3 foamColor = _FoamColor.rgb * (ambient + lightColor);
                if (frontFace && _FoamWidth > 0.0)
                {
                    float edge = max(SceneDistance(screenUV) - surfaceDistance, 0.0);
                    half foam = saturate(1.0 - edge / _FoamWidth) * saturate(ripples.x * 1.6 + 0.3);
                    color = lerp(color, foamColor, foam);
                }

                // White streaks on running and pouring water, and along the front of arriving or spreading water
                if (frontFace)
                    color = lerp(color, foamColor, saturate(max(streaks, frontFoam)));

                color = MixFog(color, i.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}