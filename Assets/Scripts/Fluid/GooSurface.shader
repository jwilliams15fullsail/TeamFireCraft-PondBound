// Thick, sticky goo for a flat floor (a Plane) in URP. The goo creeps along slowly in glossy
// lumps, bubbles swell up and sink back, and thin goo glows a little where light shines through.
// GooTrails (on the same object) presses trails into it where frogs walk, and they slowly close up.
// Lit by every URP light, with shadows, like the rest of the level.
Shader "PondBound/GooSurface"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.04, 0.22, 0.03, 1)
        _TopColor ("Top Color", Color) = (0.30, 0.70, 0.10, 1)
        _GlowColor ("Glow Color", Color) = (0.45, 1.0, 0.20, 1)
        _Glow ("Glow", Range(0, 1)) = 0.25
        _Smoothness ("Smoothness", Range(0, 1)) = 0.85
        _BlobSize ("Blob Size (m)", Range(0.1, 5)) = 0.9
        _OozeSpeed ("Ooze Speed", Range(0, 1)) = 0.08
        _Bumpiness ("Bumpiness", Range(0, 2)) = 0.6
        _BubbleSize ("Bubble Size (m)", Range(0.02, 0.5)) = 0.12
        _Bubbles ("Bubbles", Range(0, 1)) = 0.35
        _TrailDepth ("Trail Depth", Range(0, 3)) = 1
        [HideInInspector] _TrailTex ("Trails", 2D) = "black" {}
        [HideInInspector] _TrailRect ("Trail Area", Vector) = (-5, -5, 10, 10)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "GooForward"
            Tags { "LightMode" = "UniversalForward" }

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
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_TrailTex);
            SAMPLER(sampler_TrailTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _TopColor;
                half4 _GlowColor;
                float4 _TrailRect;
                float4 _TrailTex_TexelSize;
                float _Glow;
                float _Smoothness;
                float _BlobSize;
                float _OozeSpeed;
                float _Bumpiness;
                float _BubbleSize;
                float _Bubbles;
                float _TrailDepth;
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

            // The goo's lumps: x = height 0-1, yz = slope per metre. A slow warp makes them creep.
            float3 Lumps(float2 xz)
            {
                float t = _Time.y * _OozeSpeed;
                float2 p = xz / _BlobSize;
                float3 warp = Noise(p * 0.45 + t * float2(0.3, -0.2));
                float2 q = p + (warp.x - 0.5) * 1.6 + t * float2(0.2, 0.1);
                float3 n1 = Noise(q);
                float3 n2 = Noise(q * 2.1 + 3.7 - t * 0.6);
                float h = n1.x * 0.7 + n2.x * 0.3;
                float2 slope = (n1.yz * 0.7 + n2.yz * 2.1 * 0.3) / _BlobSize;
                // Rounder tops and softer valleys, like thick goo
                float shaped = smoothstep(0.15, 0.85, h);
                float dShaped = 6.0 * saturate((h - 0.15) / 0.7) * (1.0 - saturate((h - 0.15) / 0.7)) / 0.7;
                return float3(shaped, slope * dShaped);
            }

            // Bubbles: in some cells a bubble swells up, then sinks back into the goo.
            // x = how much it bulges 0-1, yz = slope per metre.
            float3 Bubbles(float2 xz)
            {
                float cellSize = max(_BubbleSize * 4.0, 0.01);
                float2 cp = xz / cellSize;
                float2 cell = floor(cp);
                float2 f = frac(cp);
                float r1 = Hash(cell);
                float r2 = Hash(cell + 17.3);
                if (r2 > _Bubbles)
                    return 0;
                float2 centre = 0.3 + 0.4 * float2(r1, Hash(cell + 5.1));
                float phase = frac(_Time.y * 0.12 + r1 * 7.0);
                float grow = smoothstep(0.0, 0.7, phase) * (1.0 - smoothstep(0.85, 1.0, phase));
                float radius = 0.25 * grow + 1e-4;
                float2 d = (f - centre) / radius;
                float dd = dot(d, d);
                if (dd >= 1.0)
                    return 0;
                float dome = sqrt(1.0 - dd);
                float2 slope = -d / max(dome, 0.15) / (radius * cellSize);
                return float3(dome * grow, slope * grow * 0.25);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 xz = i.positionWS.xz;
                float3 lumps = Lumps(xz);
                float3 bubble = Bubbles(xz);

                // Trails: pressed-in grooves where frogs went (1 = just pushed through)
                float2 texel = _TrailTex_TexelSize.xy;
                float trail = SAMPLE_TEXTURE2D(_TrailTex, sampler_TrailTex, i.trailUV).r;
                float trailX = SAMPLE_TEXTURE2D(_TrailTex, sampler_TrailTex, i.trailUV + float2(texel.x, 0)).r;
                float trailZ = SAMPLE_TEXTURE2D(_TrailTex, sampler_TrailTex, i.trailUV + float2(0, texel.y)).r;
                float2 trailSlope = float2(trailX - trail, trailZ - trail) * 10.0 * _TrailDepth;

                float2 slope = lumps.yz * _Bumpiness * (1.0 - 0.7 * trail) + bubble.yz - trailSlope;
                float3 n = normalize(float3(-slope.x, 1.0, -slope.y));
                float3 up = normalize(i.normalWS);
                if (up.y < 0.999) // the floor isn't flat on the ground: tilt the bumps with it
                {
                    float3 side = normalize(cross(up, float3(0, 0, 1)));
                    float3 fwd = cross(side, up);
                    n = normalize(side * -n.x + up * n.y + fwd * n.z);
                }

                float height = saturate(lumps.x * (1.0 - 0.6 * trail) + bubble.x * 0.5);
                half3 albedo = lerp(_DeepColor.rgb, _TopColor.rgb, height);
                albedo = lerp(albedo, _DeepColor.rgb * 0.45, saturate(trail * _TrailDepth) * 0.8); // pushed-aside goo is thinner and darker
                albedo = lerp(albedo, _TopColor.rgb * 1.15, bubble.x * 0.4); // bubble tops catch the light

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
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo;
                surface.metallic = 0;
                surface.specular = 0;
                surface.smoothness = saturate(_Smoothness + trail * 0.1);
                surface.occlusion = lerp(1.0, 0.75, 1.0 - height); // valleys between lumps are a little darker
                surface.alpha = 1;
                surface.normalTS = float3(0, 0, 1);
                // Light glows through thin goo: valleys and grazing views glow most
                surface.emission = _GlowColor.rgb * _Glow * (0.1 + 0.3 * (1.0 - height) + 0.6 * pow(1.0 - facing, 3.0));

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
    }
}