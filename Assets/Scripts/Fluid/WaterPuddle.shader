// A shallow puddle of clear water for a flat floor (a Plane) in URP: a rounded, wobbly puddle
// inside the plane with a bright edge and a damp ring on the floor around it, gentle ripples,
// shiny reflections, and a soft glow that pulses (a checkpoint you can spot from afar).
// GooTrails (on the same object) makes frogs splash rings through it.
// Water Color is the main colour, so a script can tint it (CheckpointManager turns it green).
Shader "PondBound/WaterPuddle"
{
    Properties
    {
        [MainColor] _WaterColor ("Water Color", Color) = (0.25, 0.55, 0.80, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.55
        _Smoothness ("Smoothness", Range(0, 1)) = 0.96
        _RippleSize ("Ripple Size (m)", Range(0.05, 3)) = 0.35
        _RippleSpeed ("Ripple Speed", Range(0, 3)) = 0.6
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.25
        _Glow ("Glow", Range(0, 2)) = 0.35
        _GlowSpeed ("Glow Pulse Speed", Range(0, 6)) = 1.5
        _SplashRipples ("Splash Ripples", Range(0, 3)) = 1
        _EdgeInset ("Edge Inset (m)", Range(0, 5)) = 0.25
        _CornerRadius ("Corner Roundness (m)", Range(0, 20)) = 10
        _EdgeWobble ("Edge Wobble (m)", Range(0, 2)) = 0.15
        _WobbleSize ("Wobble Size (m)", Range(0.2, 10)) = 0.8
        _RimWidth ("Rim Width (m)", Range(0.01, 1)) = 0.12
        _RimRoundness ("Rim Roundness", Range(0, 3)) = 0.6
        _EdgeSoftness ("Edge Softness (m)", Range(0.001, 0.2)) = 0.01
        _WetStain ("Wet Stain", Range(0, 1)) = 0.4
        _WetWidth ("Wet Stain Width (m)", Range(0, 1)) = 0.12
        [HideInInspector] _TrailTex ("Splashes", 2D) = "black" {}
        [HideInInspector] _TrailRect ("Splash Area", Vector) = (-5, -5, 10, 10)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-50" }

        Pass
        {
            Name "PuddleForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_TrailTex);
            SAMPLER(sampler_TrailTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _WaterColor;
                float4 _TrailRect;
                float4 _TrailTex_TexelSize;
                float _Opacity;
                float _Smoothness;
                float _RippleSize;
                float _RippleSpeed;
                float _RippleStrength;
                float _Glow;
                float _GlowSpeed;
                float _SplashRipples;
                float _EdgeInset;
                float _CornerRadius;
                float _EdgeWobble;
                float _WobbleSize;
                float _RimWidth;
                float _RimRoundness;
                float _EdgeSoftness;
                float _WetStain;
                float _WetWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 trailUV : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.trailUV = (input.positionOS.xz - _TrailRect.xy) / _TrailRect.zw; // same mapping GooTrails paints with
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
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

            // Gentle ripples: x = a 0-1 value, yz = slope per metre.
            float3 Ripples(float2 xz)
            {
                float t = _Time.y * _RippleSpeed;
                float2 p = xz / _RippleSize;
                float3 n1 = Noise(p + t * float2(0.7, 0.3));
                float3 n2 = Noise(p * 2.3 - t * float2(0.4, 0.9));
                return float3(n1.x * 0.6 + n2.x * 0.4, (n1.yz + n2.yz * 2.3 * 0.5) / _RippleSize);
            }

            // Metres from this point to the puddle's edge: below 0 inside, above 0 outside. The puddle is
            // a rounded rectangle (a circle or oval when Corner Roundness is big) with a wobbly edge.
            float EdgeDistance(float3 positionWS)
            {
                float3 positionOS = TransformWorldToObject(positionWS);
                float2 scale = float2(length(GetObjectToWorldMatrix()._m00_m10_m20), length(GetObjectToWorldMatrix()._m02_m12_m22));
                float2 centre = _TrailRect.xy + _TrailRect.zw * 0.5;
                float2 p = (positionOS.xz - centre) * scale;
                float2 halfSize = max(_TrailRect.zw * 0.5 * scale - _EdgeInset, 0.05);
                float r = min(_CornerRadius, min(halfSize.x, halfSize.y));
                float2 q = abs(p) - halfSize + r;
                float box = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
                float2 w = positionWS.xz / _WobbleSize;
                float wobble = (Noise(w).x - 0.5) * 1.4 + (Noise(w * 2.7 + 5.3).x - 0.5) * 0.6;
                return box + wobble * _EdgeWobble;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 xz = i.positionWS.xz;

                // Where the puddle ends, with a rounded edge where the water bulges a little
                float edge = EdgeDistance(i.positionWS);
                float2 edgeDir = float2(EdgeDistance(i.positionWS + float3(0.03, 0, 0)) - edge,
                                        EdgeDistance(i.positionWS + float3(0, 0, 0.03)) - edge) / 0.03; // points outward
                float water = 1.0 - smoothstep(-_EdgeSoftness, _EdgeSoftness, edge); // 1 inside, 0 outside
                float rim = saturate(-edge / _RimWidth); // 0 at the edge, 1 past the rim
                float2 rimSlope = rim < 1.0 ? -edgeDir * _RimRoundness * 0.5 / sqrt(max(rim, 0.04)) : 0;

                float3 ripples = Ripples(xz);

                // Splashes: rings run out through the water where frogs went (1 = just splashed)
                float2 texel = _TrailTex_TexelSize.xy;
                float splash = SAMPLE_TEXTURE2D(_TrailTex, sampler_TrailTex, i.trailUV).r;
                float splashX = SAMPLE_TEXTURE2D(_TrailTex, sampler_TrailTex, i.trailUV + float2(texel.x, 0)).r;
                float splashZ = SAMPLE_TEXTURE2D(_TrailTex, sampler_TrailTex, i.trailUV + float2(0, texel.y)).r;
                float rings = cos(splash * 25.0 - _Time.y * 8.0); // a fading splash moves the rings outward
                float2 splashSlope = float2(splashX - splash, splashZ - splash) * rings * 30.0 * _SplashRipples;

                float2 slope = ripples.yz * _RippleStrength * rim + splashSlope * rim + rimSlope;
                slope *= water; // the damp ring outside is flat
                float3 n = normalize(float3(-slope.x, 1.0, -slope.y));
                float3 up = normalize(i.normalWS);
                if (up.y < 0.999) // the floor isn't flat on the ground: tilt the ripples with it
                {
                    float3 side = normalize(cross(up, float3(0, 0, 1)));
                    float3 fwd = cross(side, up);
                    n = normalize(side * -n.x + up * n.y + fwd * n.z);
                }

                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.positionCS = i.positionCS;
                inputData.normalWS = n;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                inputData.fogCoord = i.fogFactor;
                inputData.bakedGI = SampleSH(n);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                float facing = saturate(dot(n, inputData.viewDirectionWS));
                float fresnel = pow(1.0 - facing, 4.0);
                float stain = _WetStain * (1.0 - saturate(edge / max(_WetWidth, 0.001))); // fades out away from the water

                // Clear in the thin water at the edge, deeper colour in the middle, mirror-like when seen low
                float clear = _Opacity * lerp(0.45, 1.0, rim);
                float alpha = lerp(clear, 0.95, fresnel * 0.8);
                half3 tint = _WaterColor.rgb * lerp(1.1, 0.75, rim);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = lerp(_WaterColor.rgb * 0.15, tint, water); // outside: a damp, darker floor
                surface.metallic = 0;
                surface.specular = 0;
                surface.smoothness = lerp(0.7, _Smoothness, water);
                surface.occlusion = 1;
                surface.alpha = saturate(water * alpha + (1.0 - water) * stain);
                surface.normalTS = float3(0, 0, 1);
                // A soft pulsing glow with shimmering light patterns, and a bright line along the edge
                float pulse = 0.65 + 0.35 * sin(_Time.y * _GlowSpeed);
                float shimmer = 0.5 + ripples.x;
                surface.emission = water * _WaterColor.rgb * _Glow * (pulse * shimmer * 0.6 + pow(1.0 - rim, 3.0) * 0.8);

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = surface.alpha;
                return color;
            }
            ENDHLSL
        }
    }
}