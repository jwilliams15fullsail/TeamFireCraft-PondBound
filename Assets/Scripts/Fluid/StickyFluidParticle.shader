// Draws StickyFluid particles as round, lit blobs (sphere impostors) in URP.
// Each particle is a camera-facing quad. The pixel shader turns it into a sphere and writes depth,
// so the blobs overlap and merge into one lumpy goo surface.
Shader "PondBound/StickyFluidParticle"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.65, 0.22, 1)
        _DeepColor ("Shadow Color", Color) = (0.01, 0.2, 0.05, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.7
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.7
        _RimColor ("Rim Color", Color) = (0.55, 1, 0.5, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _Translucency ("Translucency", Range(0, 1)) = 0.25
        _Refraction ("Refraction", Range(0, 0.1)) = 0.03
        _SmoothSurface ("Smooth Surface", Range(0, 1)) = 0.85
        _Foam ("Foam", Range(0, 1)) = 0
        _FoamSpeed ("Foam Speed", Range(0.1, 10)) = 2.5
        _FoamColor ("Foam Color", Color) = (0.92, 0.96, 1, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Particle
        {
            float3 position;
            float  density;
            float3 velocity;
            float  pressure;
            float3 normal;
            float  pad0;
            float3 force;
            float  pad1;
        };

        StructuredBuffer<Particle> _Particles;
        float4x4 _SimToWorld;
        float _ParticleRadius;
        float _RestDensity;

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _DeepColor;
            half4 _RimColor;
            half _Smoothness;
            half _SpecularStrength;
            half _RimPower;
            half _Translucency;
            half _Refraction;
            half _SmoothSurface;
            half4 _FoamColor;
            half _Foam;
            half _FoamSpeed;
        CBUFFER_END

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;              // -1..1 across the quad
            float3 centerWS : TEXCOORD1;
            float3 fluidNormalWS : TEXCOORD2;   // outward goo normal, longer at the surface
            float thin : TEXCOORD3;             // 1 where the goo is stretched thin
            float foam : TEXCOORD4;             // 1 where the water is fast or spraying
            float radius : TEXCOORD5;           // drawn size, smaller for flying drops
        };

        static const float2 kCorners[6] =
        {
            float2(-1, -1), float2(1, -1), float2(1, 1),
            float2(-1, -1), float2(1, 1), float2(-1, 1)
        };

        Varyings Vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
        {
            Particle p = _Particles[instanceID];
            float3 centerWS = mul(_SimToWorld, float4(p.position, 1)).xyz;
            float2 corner = kCorners[vertexID];

            // Camera (or shadow light) right and up vectors in world space
            float3 right = UNITY_MATRIX_V[0].xyz;
            float3 up = UNITY_MATRIX_V[1].xyz;

                       Varyings o;
            float fill = p.density / _RestDensity;                // about 1 inside, lower at the edges and in flying drops
            o.thin = saturate((0.85 - fill) * 2.0);
            float spray = saturate((0.45 - fill) * 4.0);          // 1 for a drop flying on its own
            o.foam = saturate(max(length(p.velocity) / _FoamSpeed - 1.0, spray)) * _Foam;
            o.radius = _ParticleRadius * (1.0 - 0.5 * spray * _Foam);
            o.positionCS = TransformWorldToHClip(centerWS + (right * corner.x + up * corner.y) * o.radius);
            o.uv = corner;
            o.centerWS = centerWS;
            o.fluidNormalWS = mul((float3x3)_SimToWorld, -p.normal);
            return o;
        }

        // Turns the quad pixel into a point on a sphere. Pixels outside the circle are thrown away.
        void SpherePoint(Varyings i, out float3 positionWS, out float3 normalWS)
        {
            float r2 = dot(i.uv, i.uv);
            clip(1.0 - r2);
            float3 right = UNITY_MATRIX_V[0].xyz;
            float3 up = UNITY_MATRIX_V[1].xyz;
            float3 back = UNITY_MATRIX_V[2].xyz;
            normalWS = right * i.uv.x + up * i.uv.y + back * sqrt(1.0 - r2);
            positionWS = i.centerWS + normalWS * i.radius;
        }

        float ClipDepth(float4 positionCS)
        {
            return positionCS.z / positionCS.w;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            ZTest LEqual
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            half4 Frag(Varyings i, out float depth : SV_Depth) : SV_Target
            {
                float3 positionWS, sphereNormal;
                SpherePoint(i, positionWS, sphereNormal);

                float4 positionCS = TransformWorldToHClip(positionWS);
                depth = ClipDepth(positionCS);

                // Lean towards the goo's own surface normal so neighbouring blobs shade as one surface
                float surface = saturate(length(i.fluidNormalWS));
                float3 fluidNormal = surface > 0.05 ? normalize(i.fluidNormalWS) : sphereNormal;
                float3 n = normalize(lerp(sphereNormal, fluidNormal, _SmoothSurface * surface));
                float3 v = GetWorldSpaceNormalizeViewDir(positionWS);

                Light light = GetMainLight(TransformWorldToShadowCoord(positionWS));
                half lit = light.shadowAttenuation; // not distanceAttenuation: procedural draws have no per-object light data
                half wrap = saturate((dot(n, light.direction) + 0.5) / 1.5) * lit;

                half3 color = lerp(_DeepColor.rgb, _BaseColor.rgb, wrap) * light.color;
                color += SampleSH(n) * _BaseColor.rgb * 0.5;

                // See-through jelly: bent view of whatever is behind, tinted by the goo colour
                half fresnel = pow(1.0 - saturate(dot(n, v)), _RimPower);
                half see = saturate(_Translucency + i.thin * 0.4) * (1.0 - fresnel);
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                float2 bend = float2(dot(n, UNITY_MATRIX_V[0].xyz), dot(n, UNITY_MATRIX_V[1].xyz)) * _Refraction;
                half3 behind = SampleSceneColor(screenUV - bend) * _BaseColor.rgb * 1.6;
                color = lerp(color, behind, see);
                
                // White water where it is fast or spraying. Foam is 0 on the slime material.
                color = lerp(color, _FoamColor.rgb * (0.45 + 0.55 * wrap) * light.color + SampleSH(n) * 0.25, i.foam);

                // Wet highlight and rim
                float3 halfDir = normalize(light.direction + v);
                half specPower = exp2(10.0 * _Smoothness + 1.0);
                color += pow(saturate(dot(n, halfDir)), specPower) * _SpecularStrength * light.color * lit;
                color += fresnel * _RimColor.rgb * 0.5;

                color = MixFog(color, ComputeFogFactor(positionCS.z));
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment FragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            half4 FragShadow(Varyings i, out float depth : SV_Depth) : SV_Target
            {
                float3 positionWS, normalWS;
                SpherePoint(i, positionWS, normalWS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                depth = ClipDepth(ApplyShadowClamping(positionCS));
                return 0;
            }
            ENDHLSL
        }
    }
}