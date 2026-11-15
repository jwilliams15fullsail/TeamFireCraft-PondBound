// PondBound/GrungeLit - URP Lit with world-space grime layered on top.
// Uses the same texture/property names as URP Lit, so switching a Lit material
// to this shader keeps its textures. Grime is world-space: it flows across
// modular wall pieces with no seams and never repeats with the texture tiling.
Shader "PondBound/GrungeLit"
{
    Properties
    {
        [Header(Base Surface (same as URP Lit))]
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [ToggleOff(_SPECULAR_SETUP)] _WorkflowMode("Metallic Workflow (off = Specular)", Float) = 1
        _Metallic("Metallic", Range(0, 1)) = 0
        _SpecColor("Specular Color", Color) = (0.2, 0.2, 0.2, 1)
        [Toggle(_METALLICSPECGLOSSMAP)] _UseGlossMap("Use Metallic/Specular Map", Float) = 0
        _MetallicGlossMap("Metallic Map", 2D) = "white" {}
        _SpecGlossMap("Specular Map", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        [Toggle(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)] _SmoothnessTextureChannel("Smoothness From Albedo Alpha", Float) = 0
        _BumpScale("Normal Scale", Float) = 1
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _OcclusionStrength("Occlusion Strength", Range(0, 1)) = 1
        _OcclusionMap("Occlusion", 2D) = "white" {}
        [ToggleUI] _OcclusionFromRed("AO In Red Channel (Unreal ORM map)", Float) = 1
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull (Off = both sides)", Float) = 2
        [ToggleOff(_SPECULARHIGHLIGHTS_OFF)] _SpecularHighlights("Specular Highlights", Float) = 1
        [ToggleOff(_ENVIRONMENTREFLECTIONS_OFF)] _EnvironmentReflections("Environment Reflections", Float) = 1

        [Header(Grime Stains)]
        _GrimeColor("Grime Color", Color) = (0.3, 0.25, 0.17, 0.85)
        _GrimeAmount("Stain Coverage", Range(0, 1)) = 0.4
        _GrimeScale("Stain Size (m)", Float) = 3
        _GrimeContrast("Stain Edge Sharpness", Range(1, 12)) = 6
        _CavityGrime("Dirt In Cracks (uses AO map)", Range(0, 1)) = 0.8
        _GrimeSmoothness("Grime Smoothness", Range(0, 1)) = 0.12
        _SmoothnessBreakup("Shine Breakup", Range(0, 1)) = 0.6
        _EmissionGrime("Grime Over Lights", Range(0, 1)) = 0.4

        [Header(Walls)]
        _FloorY("Floor Height (world Y)", Float) = 0
        _RiseHeight("Dirt Rising From Floor (m)", Float) = 1.4
        _RiseStrength("Floor Dirt Strength", Range(0, 1)) = 0.85
        _StreakColor("Drip Streak Tint", Color) = (0.42, 0.33, 0.22, 1)
        _StreakAmount("Drip Streaks", Range(0, 1)) = 0.6
        _StreakDensity("Streaks Per Metre", Float) = 1.6

        [Header(Floors)]
        _FloorDust("Floor Dust", Range(0, 1)) = 0.3
        _Puddles("Damp Patches", Range(0, 1)) = 0.5
        _PuddleScale("Damp Patch Size (m)", Float) = 4

        // Hidden URP Lit properties (kept so the CBUFFER and other passes match Lit)
        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        [HideInInspector] _Parallax("Scale", Range(0.005, 0.08)) = 0.005
        [HideInInspector] _ParallaxMap("Height Map", 2D) = "black" {}
        [HideInInspector] _DetailMask("Detail Mask", 2D) = "white" {}
        [HideInInspector] _DetailAlbedoMapScale("Scale", Range(0, 2)) = 1
        [HideInInspector] _DetailAlbedoMap("Detail Albedo x2", 2D) = "linearGrey" {}
        [HideInInspector] _DetailNormalMapScale("Scale", Range(0, 2)) = 1
        [HideInInspector] [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}
        [HideInInspector] _ClearCoatMask("_ClearCoatMask", Float) = 0
        [HideInInspector] _ClearCoatSmoothness("_ClearCoatSmoothness", Float) = 0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
        LOD 300

        HLSLINCLUDE
        // Adds the grime settings to URP Lit's own per-material CBUFFER
        // (keeps the shader SRP Batcher compatible and lets every pass share it).
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #undef UNITY_TEXTURE_STREAMING_DEBUG_VARS
        #define UNITY_TEXTURE_STREAMING_DEBUG_VARS \
            float4 unity_MipmapStreaming_DebugTex_ST; \
            float4 unity_MipmapStreaming_DebugTex_TexelSize; \
            float4 unity_MipmapStreaming_DebugTex_MipInfo; \
            float4 unity_MipmapStreaming_DebugTex_StreamInfo; \
            half4 _GrimeColor; \
            half4 _StreakColor; \
            float _GrimeScale; \
            float _FloorY; \
            float _RiseHeight; \
            float _StreakDensity; \
            float _PuddleScale; \
            half _GrimeAmount; \
            half _GrimeContrast; \
            half _CavityGrime; \
            half _GrimeSmoothness; \
            half _SmoothnessBreakup; \
            half _EmissionGrime; \
            half _RiseStrength; \
            half _StreakAmount; \
            half _FloorDust; \
            half _Puddles; \
            half _OcclusionFromRed;
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // These maps are always on (missing maps fall back to neutral defaults).
            #define _NORMALMAP 1
            #define _OCCLUSIONMAP 1
            #define _EMISSION 1

            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_REFLECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

            // ---------- grime noise ----------
            float GrimeHash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float GrimeNoise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(GrimeHash(i + float3(0, 0, 0)), GrimeHash(i + float3(1, 0, 0)), f.x),
                                 lerp(GrimeHash(i + float3(0, 1, 0)), GrimeHash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(GrimeHash(i + float3(0, 0, 1)), GrimeHash(i + float3(1, 0, 1)), f.x),
                                 lerp(GrimeHash(i + float3(0, 1, 1)), GrimeHash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            float GrimeFbm(float3 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int k = 0; k < 5; k++)
                {
                    v += a * GrimeNoise(p);
                    p = p * 2.03 + 17.13;
                    a *= 0.5;
                }
                return v / 0.96875;
            }

            // ---------- grime layer ----------
            void InitializeGrungeLitSurfaceData(float2 uv, float3 positionWS, float3 normalWS, out SurfaceData s)
            {
                InitializeStandardLitSurfaceData(uv, s);

                // Unreal-style packed maps keep AO in red (green is roughness).
                half aoRed = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).r;
                s.occlusion = lerp(s.occlusion, LerpWhiteTo(aoRed, _OcclusionStrength), _OcclusionFromRed);

                float3 n = normalize(normalWS);
                float3 p = positionWS;
                half up = saturate(n.y);
                half wall = 1.0h - abs(n.y);

                // Big blotchy stains plus fine grit.
                half blotch = GrimeFbm(p / max(_GrimeScale, 0.01));
                half fine = GrimeFbm(p * (8.0 / max(_GrimeScale, 0.01)) + 31.7);
                half start = 1.0h - _GrimeAmount;
                half stains = smoothstep(start, start + 1.0h / _GrimeContrast, blotch + (fine - 0.5h) * 0.25h) * (0.55h + 0.45h * fine);

                // Dirt in cracks and panel seams, from the AO map.
                half cavity = saturate((1.0h - s.occlusion) * 2.5h) * _CavityGrime;

                // Walls: dirt rising from the floor with a ragged top edge.
                half rise = saturate(1.0h - (p.y - _FloorY) / max(_RiseHeight, 0.01) + (blotch - 0.5h) * 0.6h);
                rise = rise * rise * wall * _RiseStrength;

                // Walls: vertical drip streaks.
                float across = abs(n.x) > abs(n.z) ? p.z : p.x;
                half streakLine = GrimeNoise(float3(across * _StreakDensity, p.y * 0.12, 3.1)) * 0.65h
                                + GrimeNoise(float3(across * _StreakDensity * 4.3, p.y * 0.3, 7.7)) * 0.35h;
                half streakFade = GrimeNoise(float3(across * _StreakDensity * 0.35, p.y * 0.45, 11.3));
                half streak = smoothstep(0.6h, 0.74h, streakLine) * smoothstep(0.25h, 0.75h, streakFade) * wall * _StreakAmount;

                // Floors: dust everywhere, damp patches with dried dirty rims.
                half dust = up * _FloorDust * (0.35h + 0.65h * blotch);
                half damp = GrimeFbm(float3(p.x, 0.0, p.z) / max(_PuddleScale, 0.01) + 5.3);
                half wet = smoothstep(0.6h, 0.66h, damp) * up * _Puddles;
                half rim = smoothstep(0.54h, 0.6h, damp) * (1.0h - wet / max(_Puddles, 0.001h)) * up * _Puddles * 0.8h;

                half dirt = saturate(max(max(stains, cavity), max(max(rise, dust), rim)));

                // Albedo: grime keeps a little of the texture's light/dark detail.
                half3 grimeAlbedo = _GrimeColor.rgb * lerp(0.55h, 1.3h, fine) * (0.65h + 0.7h * dot(s.albedo, half3(0.2126h, 0.7152h, 0.0722h)));
                s.albedo = lerp(s.albedo, grimeAlbedo, dirt * _GrimeColor.a);
                s.albedo = lerp(s.albedo, s.albedo * _StreakColor.rgb, streak);
                s.albedo *= lerp(1.0h, 0.82h + 0.36h * fine, _GrimeAmount * 0.6h);

                // Grime is matte; shine gets broken up everywhere.
                half matte = saturate(dirt + streak * 0.6h);
                s.smoothness *= lerp(1.0h, 0.45h + 0.55h * blotch, _SmoothnessBreakup);
                s.smoothness = lerp(s.smoothness, _GrimeSmoothness, matte);
            #if defined(_SPECULAR_SETUP)
                s.specular = lerp(s.specular, half3(0.03h, 0.03h, 0.03h), matte);
            #else
                s.metallic = lerp(s.metallic, 0.0h, matte);
            #endif
                s.occlusion *= lerp(1.0h, 0.75h, dirt);
                s.emission *= lerp(1.0h, 0.25h, saturate(dirt + streak) * _EmissionGrime);

                // Damp patches: darker and glossy.
                s.albedo *= lerp(1.0h, 0.55h, wet);
                s.smoothness = lerp(s.smoothness, 0.92h, wet);
            }

            // Swap URP's surface setup for the grime version inside the stock Lit forward pass.
            #define InitializeStandardLitSurfaceData(uvIn, sd) InitializeGrungeLitSurfaceData(uvIn, input.positionWS, input.normalWS, sd)
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #define _NORMALMAP 1
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile _ _WRITE_SMOOTHNESS
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit
            #define _EMISSION 1
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}