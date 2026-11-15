// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

// Normal Mapping for a Triplanar Shader - Ben Golus 2017
// Unity Surface Shader example shader

// Implements correct triplanar normals in a Surface Shader with out computing or passing additional information from the
// vertex shader.

Shader "Triplanar/URP Shader (RNM)" {
    Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset][Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _Roughness("Roughness", Range(0, 1)) = 0.5
        [NoScaleOffset] _RoughnessMap("Roughness Map", 2D) = "white" {}
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200


            // ------------------------------------------------------------
            // Surface shader code generated out of a CGPROGRAM block:


            // ---- forward rendering base pass:
            Pass {
                Name "FORWARD"
                //Tags { "LightMode" = "ForwardBase" }

        CGPROGRAM
            // compile directives
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #include "HLSLSupport.cginc"
            #define UNITY_INSTANCED_LOD_FADE
            #define UNITY_INSTANCED_SH
            #define UNITY_INSTANCED_LIGHTMAPSTS
            #include "UnityShaderVariables.cginc"
            #include "UnityShaderUtilities.cginc"
            // -------- variant for: <when no other keywords are defined>
            #if !defined(INSTANCING_ON)
            // Surface shader code generated based on:
            // writes to per-pixel normal: YES
            // writes to emission: no
            // writes to occlusion: YES
            // needs world space reflection vector: no
            // needs world space normal vector: YES
            // needs screen space position: no
            // needs world space position: YES
            // needs view direction: no
            // needs world space view direction: no
            // needs world space position for lighting: YES
            // needs world space view direction for lighting: YES
            // needs world space view direction for lightmaps: no
            // needs vertex color: no
            // needs VFACE: no
            // needs SV_IsFrontFace: no
            // passes tangent-to-world matrix to pixel shader: YES
            // reads from normal: no
            // 0 texcoords actually used
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

            #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
            #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
            #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

            // Original surface shader snippet:
            #line 20 ""
            #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
            #endif
            /* UNITY: Original start of shader */
                    // Physically based Standard lighting model, and enable shadows on all light types
                    //#pragma surface surf Standard fullforwardshadows

                    // Use shader model 3.0 target, to get nicer looking lighting
                    //#pragma target 3.0

                    #include "UnityStandardUtils.cginc"

                    // flip UVs horizontally to correct for back side projection
                    #define TRIPLANAR_CORRECT_PROJECTED_U

                    // offset UVs to prevent obvious mirroring
                    // #define TRIPLANAR_UV_OFFSET

                    // Reoriented Normal Mapping
                    // http://blog.selfshadow.com/publications/blending-in-detail/
                    // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                    half3 blend_rnm(half3 n1, half3 n2)
                    {
                        n1.z += 1;
                        n2.xy = -n2.xy;

                        return n1 * dot(n1, n2) / n1.z - n2;
                    }

                    sampler2D _MainTex;
                    float4 _MainTex_ST;

                    sampler2D _BumpMap;
                    sampler2D _OcclusionMap;
                    sampler2D _RoughnessMap;

                    half _Roughness;
                    half _Metallic;

                    half _OcclusionStrength;

                    struct Input {
                        float3 worldPos;
                        float3 worldNormal;
                        INTERNAL_DATA
                    };

                    float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                        float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                        float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                        float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                        float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                        return normalize(mul(t2w, normal));
                    }

                    void surf(Input IN, inout SurfaceOutputStandard o) {
                        // work around bug where IN.worldNormal is always (0,0,0)!
                        IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                        // calculate triplanar blend
                        half3 triblend = saturate(pow(IN.worldNormal, 4));
                        triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                        // calculate triplanar uvs
                        // applying texture scale and offset values ala TRANSFORM_TEX macro
                        float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                        float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                        float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                        // offset UVs to prevent obvious mirroring
                    #if defined(TRIPLANAR_UV_OFFSET)
                        uvY += 0.33;
                        uvZ += 0.67;
                    #endif

                        // minor optimization of sign(). prevents return value of 0
                        half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                        // flip UVs horizontally to correct for back side projection
                    #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                        uvX.x *= axisSign.x;
                        uvY.x *= axisSign.y;
                        uvZ.x *= -axisSign.z;
                    #endif

                        // albedo textures
                        fixed4 colX = tex2D(_MainTex, uvX);
                        fixed4 colY = tex2D(_MainTex, uvY);
                        fixed4 colZ = tex2D(_MainTex, uvZ);
                        fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                        // roughness textures
                        half roughX = tex2D(_RoughnessMap, uvX).r;
                        half roughY = tex2D(_RoughnessMap, uvY).r;
                        half roughZ = tex2D(_RoughnessMap, uvZ).r;
                        half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                        // occlusion textures
                        half occX = tex2D(_OcclusionMap, uvX).g;
                        half occY = tex2D(_OcclusionMap, uvY).g;
                        half occZ = tex2D(_OcclusionMap, uvZ).g;
                        half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                        // tangent space normal maps
                        half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                        half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                        half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                        // flip normal maps' x axis to account for flipped UVs
                    #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                        tnormalX.x *= axisSign.x;
                        tnormalY.x *= axisSign.y;
                        tnormalZ.x *= -axisSign.z;
                    #endif

                        half3 absVertNormal = abs(IN.worldNormal);

                        // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                        tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                        tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                        tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                        // apply world space sign to tangent space Z
                        tnormalX.z *= axisSign.x;
                        tnormalY.z *= axisSign.y;
                        tnormalZ.z *= axisSign.z;

                        // sizzle tangent normals to match world normal and blend together
                        half3 worldNormal = normalize(
                            tnormalX.zyx * triblend.x +
                            tnormalY.xzy * triblend.y +
                            tnormalZ.xyz * triblend.z
                            );

                        // set surface ouput properties
                        o.Albedo = col.rgb;
                        o.Metallic = _Metallic;
                        o.Smoothness = 1 - rough;
                        o.Occlusion = occ;

                        // convert world space normals into tangent normals
                        o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                    }


                    // vertex-to-fragment interpolation data
                    // no lightmaps:
                    #ifndef LIGHTMAP_ON
                    // half-precision fragment shader registers:
                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    #define FOG_COMBINED_WITH_TSPACE
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      float4 tSpace0 : TEXCOORD0;
                      float4 tSpace1 : TEXCOORD1;
                      float4 tSpace2 : TEXCOORD2;
                      #if UNITY_SHOULD_SAMPLE_SH
                      half3 sh : TEXCOORD3; // SH
                      #endif
                      UNITY_LIGHTING_COORDS(4,5)
                      #if SHADER_TARGET >= 30
                      float4 lmap : TEXCOORD6;
                      #endif
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    // high-precision fragment shader registers:
                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      float4 tSpace0 : TEXCOORD0;
                      float4 tSpace1 : TEXCOORD1;
                      float4 tSpace2 : TEXCOORD2;
                      #if UNITY_SHOULD_SAMPLE_SH
                      half3 sh : TEXCOORD3; // SH
                      #endif
                      UNITY_FOG_COORDS(4)
                      UNITY_SHADOW_COORDS(5)
                      #if SHADER_TARGET >= 30
                      float4 lmap : TEXCOORD6;
                      #endif
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    #endif
                    // with lightmaps:
                    #ifdef LIGHTMAP_ON
                    // half-precision fragment shader registers:
                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    #define FOG_COMBINED_WITH_TSPACE
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      float4 tSpace0 : TEXCOORD0;
                      float4 tSpace1 : TEXCOORD1;
                      float4 tSpace2 : TEXCOORD2;
                      float4 lmap : TEXCOORD3;
                      UNITY_LIGHTING_COORDS(4,5)
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    // high-precision fragment shader registers:
                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      float4 tSpace0 : TEXCOORD0;
                      float4 tSpace1 : TEXCOORD1;
                      float4 tSpace2 : TEXCOORD2;
                      float4 lmap : TEXCOORD3;
                      UNITY_FOG_COORDS(4)
                      UNITY_SHADOW_COORDS(5)
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    #endif

                    // vertex shader
                    v2f_surf vert_surf(appdata_full v) {
                      UNITY_SETUP_INSTANCE_ID(v);
                      v2f_surf o;
                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                      o.pos = UnityObjectToClipPos(v.vertex);
                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                      #ifdef DYNAMICLIGHTMAP_ON
                      o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                      #endif
                      #ifdef LIGHTMAP_ON
                      o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                      #endif

                      // SH/ambient and vertex lights
                      #ifndef LIGHTMAP_ON
                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                          o.sh = 0;
                          // Approximated illumination from non-important point lights
                          #ifdef VERTEXLIGHT_ON
                            o.sh += Shade4PointLights(
                              unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                              unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                              unity_4LightAtten0, worldPos, worldNormal);
                          #endif
                          o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                        #endif
                      #endif // !LIGHTMAP_ON

                      UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                      #ifdef FOG_COMBINED_WITH_TSPACE
                        UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                        UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                      #else
                        UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                      #endif
                      return o;
                    }

                    // fragment shader
                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                      UNITY_SETUP_INSTANCE_ID(IN);
                      UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                      // prepare and unpack data
                      Input surfIN;
                      #ifdef FOG_COMBINED_WITH_TSPACE
                        UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                        UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                      #else
                        UNITY_EXTRACT_FOG(IN);
                      #endif
                      #ifdef FOG_COMBINED_WITH_TSPACE
                        UNITY_RECONSTRUCT_TBN(IN);
                      #else
                        UNITY_EXTRACT_TBN(IN);
                      #endif
                      UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                      surfIN.worldPos.x = 1.0;
                      surfIN.worldNormal.x = 1.0;
                      float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                      #ifndef USING_DIRECTIONAL_LIGHT
                        fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                      #else
                        fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                      #endif
                      float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                      surfIN.worldNormal = 0.0;
                      surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                      surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                      surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                      surfIN.worldPos = worldPos;
                      #ifdef UNITY_COMPILER_HLSL
                      SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                      #else
                      SurfaceOutputStandard o;
                      #endif
                      o.Albedo = 0.0;
                      o.Emission = 0.0;
                      o.Alpha = 0.0;
                      o.Occlusion = 1.0;
                      fixed3 normalWorldVertex = fixed3(0,0,1);
                      o.Normal = fixed3(0,0,1);

                      // call surface function
                      surf(surfIN, o);

                      // compute lighting & shadowing factor
                      UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                      fixed4 c = 0;
                      float3 worldN;
                      worldN.x = dot(_unity_tbn_0, o.Normal);
                      worldN.y = dot(_unity_tbn_1, o.Normal);
                      worldN.z = dot(_unity_tbn_2, o.Normal);
                      worldN = normalize(worldN);
                      o.Normal = worldN;

                      // Setup lighting environment
                      UnityGI gi;
                      UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                      gi.indirect.diffuse = 0;
                      gi.indirect.specular = 0;
                      gi.light.color = _LightColor0.rgb;
                      gi.light.dir = lightDir;
                      // Call GI (lightmaps/SH/reflections) lighting function
                      UnityGIInput giInput;
                      UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                      giInput.light = gi.light;
                      giInput.worldPos = worldPos;
                      giInput.worldViewDir = worldViewDir;
                      giInput.atten = atten;
                      #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                        giInput.lightmapUV = IN.lmap;
                      #else
                        giInput.lightmapUV = 0.0;
                      #endif
                      #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                        giInput.ambient = IN.sh;
                      #else
                        giInput.ambient.rgb = 0.0;
                      #endif
                      giInput.probeHDR[0] = unity_SpecCube0_HDR;
                      giInput.probeHDR[1] = unity_SpecCube1_HDR;
                      #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                        giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                      #endif
                      #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                        giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                        giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                        giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                        giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                        giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                      #endif
                      LightingStandard_GI(o, giInput, gi);

                      // realtime lighting: call lighting function
                      c += LightingStandard(o, worldViewDir, gi);
                      UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                      UNITY_OPAQUE_ALPHA(c.a);
                      return c;
                    }


                    #endif

                        // -------- variant for: INSTANCING_ON 
                        #if defined(INSTANCING_ON)
                        // Surface shader code generated based on:
                        // writes to per-pixel normal: YES
                        // writes to emission: no
                        // writes to occlusion: YES
                        // needs world space reflection vector: no
                        // needs world space normal vector: YES
                        // needs screen space position: no
                        // needs world space position: YES
                        // needs view direction: no
                        // needs world space view direction: no
                        // needs world space position for lighting: YES
                        // needs world space view direction for lighting: YES
                        // needs world space view direction for lightmaps: no
                        // needs vertex color: no
                        // needs VFACE: no
                        // needs SV_IsFrontFace: no
                        // passes tangent-to-world matrix to pixel shader: YES
                        // reads from normal: no
                        // 0 texcoords actually used
                        #include "UnityCG.cginc"
                        #include "Lighting.cginc"
                        #include "UnityPBSLighting.cginc"
                        #include "AutoLight.cginc"

                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                        // Original surface shader snippet:
                        #line 20 ""
                        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                        #endif
                        /* UNITY: Original start of shader */
                                // Physically based Standard lighting model, and enable shadows on all light types
                                //#pragma surface surf Standard fullforwardshadows

                                // Use shader model 3.0 target, to get nicer looking lighting
                                //#pragma target 3.0

                                #include "UnityStandardUtils.cginc"

                                // flip UVs horizontally to correct for back side projection
                                #define TRIPLANAR_CORRECT_PROJECTED_U

                                // offset UVs to prevent obvious mirroring
                                // #define TRIPLANAR_UV_OFFSET

                                // Reoriented Normal Mapping
                                // http://blog.selfshadow.com/publications/blending-in-detail/
                                // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                                half3 blend_rnm(half3 n1, half3 n2)
                                {
                                    n1.z += 1;
                                    n2.xy = -n2.xy;

                                    return n1 * dot(n1, n2) / n1.z - n2;
                                }

                                sampler2D _MainTex;
                                float4 _MainTex_ST;

                                sampler2D _BumpMap;
                                sampler2D _OcclusionMap;
                                sampler2D _RoughnessMap;

                                half _Roughness;
                                half _Metallic;

                                half _OcclusionStrength;

                                struct Input {
                                    float3 worldPos;
                                    float3 worldNormal;
                                    INTERNAL_DATA
                                };

                                float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                                    float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                                    float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                                    float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                                    float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                                    return normalize(mul(t2w, normal));
                                }

                                void surf(Input IN, inout SurfaceOutputStandard o) {
                                    // work around bug where IN.worldNormal is always (0,0,0)!
                                    IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                                    // calculate triplanar blend
                                    half3 triblend = saturate(pow(IN.worldNormal, 4));
                                    triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                                    // calculate triplanar uvs
                                    // applying texture scale and offset values ala TRANSFORM_TEX macro
                                    float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                                    float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                                    float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                                    // offset UVs to prevent obvious mirroring
                                #if defined(TRIPLANAR_UV_OFFSET)
                                    uvY += 0.33;
                                    uvZ += 0.67;
                                #endif

                                    // minor optimization of sign(). prevents return value of 0
                                    half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                                    // flip UVs horizontally to correct for back side projection
                                #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                    uvX.x *= axisSign.x;
                                    uvY.x *= axisSign.y;
                                    uvZ.x *= -axisSign.z;
                                #endif

                                    // albedo textures
                                    fixed4 colX = tex2D(_MainTex, uvX);
                                    fixed4 colY = tex2D(_MainTex, uvY);
                                    fixed4 colZ = tex2D(_MainTex, uvZ);
                                    fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                                    // roughness textures
                                    half roughX = tex2D(_RoughnessMap, uvX).r;
                                    half roughY = tex2D(_RoughnessMap, uvY).r;
                                    half roughZ = tex2D(_RoughnessMap, uvZ).r;
                                    half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                                    // occlusion textures
                                    half occX = tex2D(_OcclusionMap, uvX).g;
                                    half occY = tex2D(_OcclusionMap, uvY).g;
                                    half occZ = tex2D(_OcclusionMap, uvZ).g;
                                    half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                                    // tangent space normal maps
                                    half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                                    half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                                    half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                                    // flip normal maps' x axis to account for flipped UVs
                                #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                    tnormalX.x *= axisSign.x;
                                    tnormalY.x *= axisSign.y;
                                    tnormalZ.x *= -axisSign.z;
                                #endif

                                    half3 absVertNormal = abs(IN.worldNormal);

                                    // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                                    tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                                    tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                                    tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                                    // apply world space sign to tangent space Z
                                    tnormalX.z *= axisSign.x;
                                    tnormalY.z *= axisSign.y;
                                    tnormalZ.z *= axisSign.z;

                                    // sizzle tangent normals to match world normal and blend together
                                    half3 worldNormal = normalize(
                                        tnormalX.zyx * triblend.x +
                                        tnormalY.xzy * triblend.y +
                                        tnormalZ.xyz * triblend.z
                                        );

                                    // set surface ouput properties
                                    o.Albedo = col.rgb;
                                    o.Metallic = _Metallic;
                                    o.Smoothness = 1 - rough;
                                    o.Occlusion = occ;

                                    // convert world space normals into tangent normals
                                    o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                                }


                                // vertex-to-fragment interpolation data
                                // no lightmaps:
                                #ifndef LIGHTMAP_ON
                                // half-precision fragment shader registers:
                                #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                #define FOG_COMBINED_WITH_TSPACE
                                struct v2f_surf {
                                  UNITY_POSITION(pos);
                                  float4 tSpace0 : TEXCOORD0;
                                  float4 tSpace1 : TEXCOORD1;
                                  float4 tSpace2 : TEXCOORD2;
                                  #if UNITY_SHOULD_SAMPLE_SH
                                  half3 sh : TEXCOORD3; // SH
                                  #endif
                                  UNITY_LIGHTING_COORDS(4,5)
                                  #if SHADER_TARGET >= 30
                                  float4 lmap : TEXCOORD6;
                                  #endif
                                  UNITY_VERTEX_INPUT_INSTANCE_ID
                                  UNITY_VERTEX_OUTPUT_STEREO
                                };
                                #endif
                                // high-precision fragment shader registers:
                                #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                struct v2f_surf {
                                  UNITY_POSITION(pos);
                                  float4 tSpace0 : TEXCOORD0;
                                  float4 tSpace1 : TEXCOORD1;
                                  float4 tSpace2 : TEXCOORD2;
                                  #if UNITY_SHOULD_SAMPLE_SH
                                  half3 sh : TEXCOORD3; // SH
                                  #endif
                                  UNITY_FOG_COORDS(4)
                                  UNITY_SHADOW_COORDS(5)
                                  #if SHADER_TARGET >= 30
                                  float4 lmap : TEXCOORD6;
                                  #endif
                                  UNITY_VERTEX_INPUT_INSTANCE_ID
                                  UNITY_VERTEX_OUTPUT_STEREO
                                };
                                #endif
                                #endif
                                // with lightmaps:
                                #ifdef LIGHTMAP_ON
                                // half-precision fragment shader registers:
                                #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                #define FOG_COMBINED_WITH_TSPACE
                                struct v2f_surf {
                                  UNITY_POSITION(pos);
                                  float4 tSpace0 : TEXCOORD0;
                                  float4 tSpace1 : TEXCOORD1;
                                  float4 tSpace2 : TEXCOORD2;
                                  float4 lmap : TEXCOORD3;
                                  UNITY_LIGHTING_COORDS(4,5)
                                  UNITY_VERTEX_INPUT_INSTANCE_ID
                                  UNITY_VERTEX_OUTPUT_STEREO
                                };
                                #endif
                                // high-precision fragment shader registers:
                                #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                struct v2f_surf {
                                  UNITY_POSITION(pos);
                                  float4 tSpace0 : TEXCOORD0;
                                  float4 tSpace1 : TEXCOORD1;
                                  float4 tSpace2 : TEXCOORD2;
                                  float4 lmap : TEXCOORD3;
                                  UNITY_FOG_COORDS(4)
                                  UNITY_SHADOW_COORDS(5)
                                  UNITY_VERTEX_INPUT_INSTANCE_ID
                                  UNITY_VERTEX_OUTPUT_STEREO
                                };
                                #endif
                                #endif

                                // vertex shader
                                v2f_surf vert_surf(appdata_full v) {
                                  UNITY_SETUP_INSTANCE_ID(v);
                                  v2f_surf o;
                                  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                  UNITY_TRANSFER_INSTANCE_ID(v,o);
                                  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                  o.pos = UnityObjectToClipPos(v.vertex);
                                  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                  #ifdef DYNAMICLIGHTMAP_ON
                                  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                  #endif
                                  #ifdef LIGHTMAP_ON
                                  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                  #endif

                                  // SH/ambient and vertex lights
                                  #ifndef LIGHTMAP_ON
                                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                      o.sh = 0;
                                      // Approximated illumination from non-important point lights
                                      #ifdef VERTEXLIGHT_ON
                                        o.sh += Shade4PointLights(
                                          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                                          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                                          unity_4LightAtten0, worldPos, worldNormal);
                                      #endif
                                      o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                    #endif
                                  #endif // !LIGHTMAP_ON

                                  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                                  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                                  #else
                                    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                  #endif
                                  return o;
                                }

                                // fragment shader
                                fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                  UNITY_SETUP_INSTANCE_ID(IN);
                                  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                                  // prepare and unpack data
                                  Input surfIN;
                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                  #else
                                    UNITY_EXTRACT_FOG(IN);
                                  #endif
                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                    UNITY_RECONSTRUCT_TBN(IN);
                                  #else
                                    UNITY_EXTRACT_TBN(IN);
                                  #endif
                                  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                  surfIN.worldPos.x = 1.0;
                                  surfIN.worldNormal.x = 1.0;
                                  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                  #ifndef USING_DIRECTIONAL_LIGHT
                                    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                  #else
                                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                  #endif
                                  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                  surfIN.worldNormal = 0.0;
                                  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                  surfIN.worldPos = worldPos;
                                  #ifdef UNITY_COMPILER_HLSL
                                  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                  #else
                                  SurfaceOutputStandard o;
                                  #endif
                                  o.Albedo = 0.0;
                                  o.Emission = 0.0;
                                  o.Alpha = 0.0;
                                  o.Occlusion = 1.0;
                                  fixed3 normalWorldVertex = fixed3(0,0,1);
                                  o.Normal = fixed3(0,0,1);

                                  // call surface function
                                  surf(surfIN, o);

                                  // compute lighting & shadowing factor
                                  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                  fixed4 c = 0;
                                  float3 worldN;
                                  worldN.x = dot(_unity_tbn_0, o.Normal);
                                  worldN.y = dot(_unity_tbn_1, o.Normal);
                                  worldN.z = dot(_unity_tbn_2, o.Normal);
                                  worldN = normalize(worldN);
                                  o.Normal = worldN;

                                  // Setup lighting environment
                                  UnityGI gi;
                                  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                  gi.indirect.diffuse = 0;
                                  gi.indirect.specular = 0;
                                  gi.light.color = _LightColor0.rgb;
                                  gi.light.dir = lightDir;
                                  // Call GI (lightmaps/SH/reflections) lighting function
                                  UnityGIInput giInput;
                                  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                  giInput.light = gi.light;
                                  giInput.worldPos = worldPos;
                                  giInput.worldViewDir = worldViewDir;
                                  giInput.atten = atten;
                                  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                    giInput.lightmapUV = IN.lmap;
                                  #else
                                    giInput.lightmapUV = 0.0;
                                  #endif
                                  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                    giInput.ambient = IN.sh;
                                  #else
                                    giInput.ambient.rgb = 0.0;
                                  #endif
                                  giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                  giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                  #endif
                                  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                  #endif
                                  LightingStandard_GI(o, giInput, gi);

                                  // realtime lighting: call lighting function
                                  c += LightingStandard(o, worldViewDir, gi);
                                  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                  UNITY_OPAQUE_ALPHA(c.a);
                                  return c;
                                }


                                #endif


                                ENDCG

                                }

            // ---- forward rendering additive lights pass:
            Pass {
                Name "FORWARD"
                Tags { "LightMode" = "ForwardAdd" }
                ZWrite Off Blend One One

        CGPROGRAM
                                    // compile directives
                                    #pragma vertex vert_surf
                                    #pragma fragment frag_surf
                                    #pragma target 3.0
                                    #pragma multi_compile_instancing
                                    #pragma multi_compile_fog
                                    #pragma skip_variants INSTANCING_ON
                                    #pragma multi_compile_fwdadd_fullshadows
                                    #include "HLSLSupport.cginc"
                                    #define UNITY_INSTANCED_LOD_FADE
                                    #define UNITY_INSTANCED_SH
                                    #define UNITY_INSTANCED_LIGHTMAPSTS
                                    #include "UnityShaderVariables.cginc"
                                    #include "UnityShaderUtilities.cginc"
                                    // -------- variant for: <when no other keywords are defined>
                                    #if !defined(INSTANCING_ON)
                                    // Surface shader code generated based on:
                                    // writes to per-pixel normal: YES
                                    // writes to emission: no
                                    // writes to occlusion: YES
                                    // needs world space reflection vector: no
                                    // needs world space normal vector: YES
                                    // needs screen space position: no
                                    // needs world space position: YES
                                    // needs view direction: no
                                    // needs world space view direction: no
                                    // needs world space position for lighting: YES
                                    // needs world space view direction for lighting: YES
                                    // needs world space view direction for lightmaps: no
                                    // needs vertex color: no
                                    // needs VFACE: no
                                    // needs SV_IsFrontFace: no
                                    // passes tangent-to-world matrix to pixel shader: YES
                                    // reads from normal: no
                                    // 0 texcoords actually used
                                    #include "UnityCG.cginc"
                                    #include "Lighting.cginc"
                                    #include "UnityPBSLighting.cginc"
                                    #include "AutoLight.cginc"

                                    #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                    #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                    #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                    // Original surface shader snippet:
                                    #line 20 ""
                                    #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                                    #endif
                                    /* UNITY: Original start of shader */
                                            // Physically based Standard lighting model, and enable shadows on all light types
                                            //#pragma surface surf Standard fullforwardshadows

                                            // Use shader model 3.0 target, to get nicer looking lighting
                                            //#pragma target 3.0

                                            #include "UnityStandardUtils.cginc"

                                            // flip UVs horizontally to correct for back side projection
                                            #define TRIPLANAR_CORRECT_PROJECTED_U

                                            // offset UVs to prevent obvious mirroring
                                            // #define TRIPLANAR_UV_OFFSET

                                            // Reoriented Normal Mapping
                                            // http://blog.selfshadow.com/publications/blending-in-detail/
                                            // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                                            half3 blend_rnm(half3 n1, half3 n2)
                                            {
                                                n1.z += 1;
                                                n2.xy = -n2.xy;

                                                return n1 * dot(n1, n2) / n1.z - n2;
                                            }

                                            sampler2D _MainTex;
                                            float4 _MainTex_ST;

                                            sampler2D _BumpMap;
                                            sampler2D _OcclusionMap;
                                            sampler2D _RoughnessMap;

                                            half _Roughness;
                                            half _Metallic;

                                            half _OcclusionStrength;

                                            struct Input {
                                                float3 worldPos;
                                                float3 worldNormal;
                                                INTERNAL_DATA
                                            };

                                            float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                                                float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                                                float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                                                float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                                                float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                                                return normalize(mul(t2w, normal));
                                            }

                                            void surf(Input IN, inout SurfaceOutputStandard o) {
                                                // work around bug where IN.worldNormal is always (0,0,0)!
                                                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                                                // calculate triplanar blend
                                                half3 triblend = saturate(pow(IN.worldNormal, 4));
                                                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                                                // calculate triplanar uvs
                                                // applying texture scale and offset values ala TRANSFORM_TEX macro
                                                float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                                                float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                                                float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                                                // offset UVs to prevent obvious mirroring
                                            #if defined(TRIPLANAR_UV_OFFSET)
                                                uvY += 0.33;
                                                uvZ += 0.67;
                                            #endif

                                                // minor optimization of sign(). prevents return value of 0
                                                half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                                                // flip UVs horizontally to correct for back side projection
                                            #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                uvX.x *= axisSign.x;
                                                uvY.x *= axisSign.y;
                                                uvZ.x *= -axisSign.z;
                                            #endif

                                                // albedo textures
                                                fixed4 colX = tex2D(_MainTex, uvX);
                                                fixed4 colY = tex2D(_MainTex, uvY);
                                                fixed4 colZ = tex2D(_MainTex, uvZ);
                                                fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                                                // roughness textures
                                                half roughX = tex2D(_RoughnessMap, uvX).r;
                                                half roughY = tex2D(_RoughnessMap, uvY).r;
                                                half roughZ = tex2D(_RoughnessMap, uvZ).r;
                                                half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                                                // occlusion textures
                                                half occX = tex2D(_OcclusionMap, uvX).g;
                                                half occY = tex2D(_OcclusionMap, uvY).g;
                                                half occZ = tex2D(_OcclusionMap, uvZ).g;
                                                half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                                                // tangent space normal maps
                                                half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                                                half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                                                half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                                                // flip normal maps' x axis to account for flipped UVs
                                            #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                tnormalX.x *= axisSign.x;
                                                tnormalY.x *= axisSign.y;
                                                tnormalZ.x *= -axisSign.z;
                                            #endif

                                                half3 absVertNormal = abs(IN.worldNormal);

                                                // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                                                tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                                                tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                                                tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                                                // apply world space sign to tangent space Z
                                                tnormalX.z *= axisSign.x;
                                                tnormalY.z *= axisSign.y;
                                                tnormalZ.z *= axisSign.z;

                                                // sizzle tangent normals to match world normal and blend together
                                                half3 worldNormal = normalize(
                                                    tnormalX.zyx * triblend.x +
                                                    tnormalY.xzy * triblend.y +
                                                    tnormalZ.xyz * triblend.z
                                                    );

                                                // set surface ouput properties
                                                o.Albedo = col.rgb;
                                                o.Metallic = _Metallic;
                                                o.Smoothness = 1 - rough;
                                                o.Occlusion = occ;

                                                // convert world space normals into tangent normals
                                                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                                            }


                                            // vertex-to-fragment interpolation data
                                            struct v2f_surf {
                                              UNITY_POSITION(pos);
                                              float3 tSpace0 : TEXCOORD0;
                                              float3 tSpace1 : TEXCOORD1;
                                              float3 tSpace2 : TEXCOORD2;
                                              float3 worldPos : TEXCOORD3;
                                              UNITY_LIGHTING_COORDS(4,5)
                                              UNITY_FOG_COORDS(6)
                                              UNITY_VERTEX_INPUT_INSTANCE_ID
                                              UNITY_VERTEX_OUTPUT_STEREO
                                            };

                                            // vertex shader
                                            v2f_surf vert_surf(appdata_full v) {
                                              UNITY_SETUP_INSTANCE_ID(v);
                                              v2f_surf o;
                                              UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                              UNITY_TRANSFER_INSTANCE_ID(v,o);
                                              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                              o.pos = UnityObjectToClipPos(v.vertex);
                                              float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                              float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                              fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                              fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                              fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                              o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
                                              o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
                                              o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
                                              o.worldPos.xyz = worldPos;

                                              UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                              UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                              return o;
                                            }

                                            // fragment shader
                                            fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                              UNITY_SETUP_INSTANCE_ID(IN);
                                              UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                                              // prepare and unpack data
                                              Input surfIN;
                                              #ifdef FOG_COMBINED_WITH_TSPACE
                                                UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                              #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                              #else
                                                UNITY_EXTRACT_FOG(IN);
                                              #endif
                                              #ifdef FOG_COMBINED_WITH_TSPACE
                                                UNITY_RECONSTRUCT_TBN(IN);
                                              #else
                                                UNITY_EXTRACT_TBN(IN);
                                              #endif
                                              UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                              surfIN.worldPos.x = 1.0;
                                              surfIN.worldNormal.x = 1.0;
                                              float3 worldPos = IN.worldPos.xyz;
                                              #ifndef USING_DIRECTIONAL_LIGHT
                                                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                              #else
                                                fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                              #endif
                                              float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                              surfIN.worldNormal = 0.0;
                                              surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                              surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                              surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                              surfIN.worldPos = worldPos;
                                              #ifdef UNITY_COMPILER_HLSL
                                              SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                              #else
                                              SurfaceOutputStandard o;
                                              #endif
                                              o.Albedo = 0.0;
                                              o.Emission = 0.0;
                                              o.Alpha = 0.0;
                                              o.Occlusion = 1.0;
                                              fixed3 normalWorldVertex = fixed3(0,0,1);
                                              o.Normal = fixed3(0,0,1);

                                              // call surface function
                                              surf(surfIN, o);
                                              UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                              fixed4 c = 0;
                                              float3 worldN;
                                              worldN.x = dot(_unity_tbn_0, o.Normal);
                                              worldN.y = dot(_unity_tbn_1, o.Normal);
                                              worldN.z = dot(_unity_tbn_2, o.Normal);
                                              worldN = normalize(worldN);
                                              o.Normal = worldN;

                                              // Setup lighting environment
                                              UnityGI gi;
                                              UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                              gi.indirect.diffuse = 0;
                                              gi.indirect.specular = 0;
                                              gi.light.color = _LightColor0.rgb;
                                              gi.light.dir = lightDir;
                                              gi.light.color *= atten;
                                              c += LightingStandard(o, worldViewDir, gi);
                                              c.a = 0.0;
                                              UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                              UNITY_OPAQUE_ALPHA(c.a);
                                              return c;
                                            }


                                            #endif


                                            ENDCG

                                            }

                                    // ---- deferred shading pass:
                                    Pass {
                                        Name "DEFERRED"
                                        Tags { "LightMode" = "Deferred" }

                                CGPROGRAM
                                                // compile directives
                                                #pragma vertex vert_surf
                                                #pragma fragment frag_surf
                                                #pragma target 3.0
                                                #pragma multi_compile_instancing
                                                #pragma exclude_renderers nomrt
                                                #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
                                                #pragma multi_compile_prepassfinal
                                                #include "HLSLSupport.cginc"
                                                #define UNITY_INSTANCED_LOD_FADE
                                                #define UNITY_INSTANCED_SH
                                                #define UNITY_INSTANCED_LIGHTMAPSTS
                                                #include "UnityShaderVariables.cginc"
                                                #include "UnityShaderUtilities.cginc"
                                                // -------- variant for: <when no other keywords are defined>
                                                #if !defined(INSTANCING_ON)
                                                // Surface shader code generated based on:
                                                // writes to per-pixel normal: YES
                                                // writes to emission: no
                                                // writes to occlusion: YES
                                                // needs world space reflection vector: no
                                                // needs world space normal vector: YES
                                                // needs screen space position: no
                                                // needs world space position: YES
                                                // needs view direction: no
                                                // needs world space view direction: no
                                                // needs world space position for lighting: YES
                                                // needs world space view direction for lighting: YES
                                                // needs world space view direction for lightmaps: no
                                                // needs vertex color: no
                                                // needs VFACE: no
                                                // needs SV_IsFrontFace: no
                                                // passes tangent-to-world matrix to pixel shader: YES
                                                // reads from normal: no
                                                // 0 texcoords actually used
                                                #include "UnityCG.cginc"
                                                #include "Lighting.cginc"
                                                #include "UnityPBSLighting.cginc"

                                                #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                // Original surface shader snippet:
                                                #line 20 ""
                                                #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                                                #endif
                                                /* UNITY: Original start of shader */
                                                        // Physically based Standard lighting model, and enable shadows on all light types
                                                        //#pragma surface surf Standard fullforwardshadows

                                                        // Use shader model 3.0 target, to get nicer looking lighting
                                                        //#pragma target 3.0

                                                        #include "UnityStandardUtils.cginc"

                                                        // flip UVs horizontally to correct for back side projection
                                                        #define TRIPLANAR_CORRECT_PROJECTED_U

                                                        // offset UVs to prevent obvious mirroring
                                                        // #define TRIPLANAR_UV_OFFSET

                                                        // Reoriented Normal Mapping
                                                        // http://blog.selfshadow.com/publications/blending-in-detail/
                                                        // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                                                        half3 blend_rnm(half3 n1, half3 n2)
                                                        {
                                                            n1.z += 1;
                                                            n2.xy = -n2.xy;

                                                            return n1 * dot(n1, n2) / n1.z - n2;
                                                        }

                                                        sampler2D _MainTex;
                                                        float4 _MainTex_ST;

                                                        sampler2D _BumpMap;
                                                        sampler2D _OcclusionMap;
                                                        sampler2D _RoughnessMap;

                                                        half _Roughness;
                                                        half _Metallic;

                                                        half _OcclusionStrength;

                                                        struct Input {
                                                            float3 worldPos;
                                                            float3 worldNormal;
                                                            INTERNAL_DATA
                                                        };

                                                        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                                                            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                                                            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                                                            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                                                            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                                                            return normalize(mul(t2w, normal));
                                                        }

                                                        void surf(Input IN, inout SurfaceOutputStandard o) {
                                                            // work around bug where IN.worldNormal is always (0,0,0)!
                                                            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                                                            // calculate triplanar blend
                                                            half3 triblend = saturate(pow(IN.worldNormal, 4));
                                                            triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                                                            // calculate triplanar uvs
                                                            // applying texture scale and offset values ala TRANSFORM_TEX macro
                                                            float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                                                            float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                                                            float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                                                            // offset UVs to prevent obvious mirroring
                                                        #if defined(TRIPLANAR_UV_OFFSET)
                                                            uvY += 0.33;
                                                            uvZ += 0.67;
                                                        #endif

                                                            // minor optimization of sign(). prevents return value of 0
                                                            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                                                            // flip UVs horizontally to correct for back side projection
                                                        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                            uvX.x *= axisSign.x;
                                                            uvY.x *= axisSign.y;
                                                            uvZ.x *= -axisSign.z;
                                                        #endif

                                                            // albedo textures
                                                            fixed4 colX = tex2D(_MainTex, uvX);
                                                            fixed4 colY = tex2D(_MainTex, uvY);
                                                            fixed4 colZ = tex2D(_MainTex, uvZ);
                                                            fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                                                            // roughness textures
                                                            half roughX = tex2D(_RoughnessMap, uvX).r;
                                                            half roughY = tex2D(_RoughnessMap, uvY).r;
                                                            half roughZ = tex2D(_RoughnessMap, uvZ).r;
                                                            half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                                                            // occlusion textures
                                                            half occX = tex2D(_OcclusionMap, uvX).g;
                                                            half occY = tex2D(_OcclusionMap, uvY).g;
                                                            half occZ = tex2D(_OcclusionMap, uvZ).g;
                                                            half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                                                            // tangent space normal maps
                                                            half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                                                            half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                                                            half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                                                            // flip normal maps' x axis to account for flipped UVs
                                                        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                            tnormalX.x *= axisSign.x;
                                                            tnormalY.x *= axisSign.y;
                                                            tnormalZ.x *= -axisSign.z;
                                                        #endif

                                                            half3 absVertNormal = abs(IN.worldNormal);

                                                            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                                                            tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                                                            tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                                                            tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                                                            // apply world space sign to tangent space Z
                                                            tnormalX.z *= axisSign.x;
                                                            tnormalY.z *= axisSign.y;
                                                            tnormalZ.z *= axisSign.z;

                                                            // sizzle tangent normals to match world normal and blend together
                                                            half3 worldNormal = normalize(
                                                                tnormalX.zyx * triblend.x +
                                                                tnormalY.xzy * triblend.y +
                                                                tnormalZ.xyz * triblend.z
                                                                );

                                                            // set surface ouput properties
                                                            o.Albedo = col.rgb;
                                                            o.Metallic = _Metallic;
                                                            o.Smoothness = 1 - rough;
                                                            o.Occlusion = occ;

                                                            // convert world space normals into tangent normals
                                                            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                                                        }


                                                        // vertex-to-fragment interpolation data
                                                        struct v2f_surf {
                                                          UNITY_POSITION(pos);
                                                          float4 tSpace0 : TEXCOORD0;
                                                          float4 tSpace1 : TEXCOORD1;
                                                          float4 tSpace2 : TEXCOORD2;
                                                        #ifndef DIRLIGHTMAP_OFF
                                                          float3 viewDir : TEXCOORD3;
                                                        #endif
                                                          float4 lmap : TEXCOORD4;
                                                        #ifndef LIGHTMAP_ON
                                                          #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                            half3 sh : TEXCOORD5; // SH
                                                          #endif
                                                        #else
                                                          #ifdef DIRLIGHTMAP_OFF
                                                            float4 lmapFadePos : TEXCOORD5;
                                                          #endif
                                                        #endif
                                                          UNITY_VERTEX_INPUT_INSTANCE_ID
                                                          UNITY_VERTEX_OUTPUT_STEREO
                                                        };

                                                        // vertex shader
                                                        v2f_surf vert_surf(appdata_full v) {
                                                          UNITY_SETUP_INSTANCE_ID(v);
                                                          v2f_surf o;
                                                          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                          UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                          o.pos = UnityObjectToClipPos(v.vertex);
                                                          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                          fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                          fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                          fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                          o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                          o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                          o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                          float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
                                                          #ifndef DIRLIGHTMAP_OFF
                                                          o.viewDir.x = dot(viewDirForLight, worldTangent);
                                                          o.viewDir.y = dot(viewDirForLight, worldBinormal);
                                                          o.viewDir.z = dot(viewDirForLight, worldNormal);
                                                          #endif
                                                        #ifdef DYNAMICLIGHTMAP_ON
                                                          o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                        #else
                                                          o.lmap.zw = 0;
                                                        #endif
                                                        #ifdef LIGHTMAP_ON
                                                          o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                          #ifdef DIRLIGHTMAP_OFF
                                                            o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
                                                            o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
                                                          #endif
                                                        #else
                                                          o.lmap.xy = 0;
                                                            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                              o.sh = 0;
                                                              o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                            #endif
                                                        #endif
                                                          return o;
                                                        }
                                                        #ifdef LIGHTMAP_ON
                                                        float4 unity_LightmapFade;
                                                        #endif
                                                        fixed4 unity_Ambient;

                                                        // fragment shader
                                                        void frag_surf(v2f_surf IN,
                                                            out half4 outGBuffer0 : SV_Target0,
                                                            out half4 outGBuffer1 : SV_Target1,
                                                            out half4 outGBuffer2 : SV_Target2,
                                                            out half4 outEmission : SV_Target3
                                                        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                            , out half4 outShadowMask : SV_Target4
                                                        #endif
                                                        ) {
                                                          UNITY_SETUP_INSTANCE_ID(IN);
                                                          UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                                                          // prepare and unpack data
                                                          Input surfIN;
                                                          #ifdef FOG_COMBINED_WITH_TSPACE
                                                            UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                          #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                            UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                          #else
                                                            UNITY_EXTRACT_FOG(IN);
                                                          #endif
                                                          #ifdef FOG_COMBINED_WITH_TSPACE
                                                            UNITY_RECONSTRUCT_TBN(IN);
                                                          #else
                                                            UNITY_EXTRACT_TBN(IN);
                                                          #endif
                                                          UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                          surfIN.worldPos.x = 1.0;
                                                          surfIN.worldNormal.x = 1.0;
                                                          float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                          #ifndef USING_DIRECTIONAL_LIGHT
                                                            fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                          #else
                                                            fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                          #endif
                                                          float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                          surfIN.worldNormal = 0.0;
                                                          surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                          surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                          surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                          surfIN.worldPos = worldPos;
                                                          #ifdef UNITY_COMPILER_HLSL
                                                          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                          #else
                                                          SurfaceOutputStandard o;
                                                          #endif
                                                          o.Albedo = 0.0;
                                                          o.Emission = 0.0;
                                                          o.Alpha = 0.0;
                                                          o.Occlusion = 1.0;
                                                          fixed3 normalWorldVertex = fixed3(0,0,1);
                                                          o.Normal = fixed3(0,0,1);

                                                          // call surface function
                                                          surf(surfIN, o);
                                                        fixed3 originalNormal = o.Normal;
                                                          float3 worldN;
                                                          worldN.x = dot(_unity_tbn_0, o.Normal);
                                                          worldN.y = dot(_unity_tbn_1, o.Normal);
                                                          worldN.z = dot(_unity_tbn_2, o.Normal);
                                                          worldN = normalize(worldN);
                                                          o.Normal = worldN;
                                                          half atten = 1;

                                                          // Setup lighting environment
                                                          UnityGI gi;
                                                          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                          gi.indirect.diffuse = 0;
                                                          gi.indirect.specular = 0;
                                                          gi.light.color = 0;
                                                          gi.light.dir = half3(0,1,0);
                                                          // Call GI (lightmaps/SH/reflections) lighting function
                                                          UnityGIInput giInput;
                                                          UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                          giInput.light = gi.light;
                                                          giInput.worldPos = worldPos;
                                                          giInput.worldViewDir = worldViewDir;
                                                          giInput.atten = atten;
                                                          #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                            giInput.lightmapUV = IN.lmap;
                                                          #else
                                                            giInput.lightmapUV = 0.0;
                                                          #endif
                                                          #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                            giInput.ambient = IN.sh;
                                                          #else
                                                            giInput.ambient.rgb = 0.0;
                                                          #endif
                                                          giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                          giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                          #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                            giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                          #endif
                                                          #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                            giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                            giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                            giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                            giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                            giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                          #endif
                                                          LightingStandard_GI(o, giInput, gi);

                                                          // call lighting function to output g-buffer
                                                          outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                                                          #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                            outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
                                                          #endif
                                                          #ifndef UNITY_HDR_ON
                                                          outEmission.rgb = exp2(-outEmission.rgb);
                                                          #endif
                                                        }


                                                        #endif

                                                        // -------- variant for: INSTANCING_ON 
                                                        #if defined(INSTANCING_ON)
                                                        // Surface shader code generated based on:
                                                        // writes to per-pixel normal: YES
                                                        // writes to emission: no
                                                        // writes to occlusion: YES
                                                        // needs world space reflection vector: no
                                                        // needs world space normal vector: YES
                                                        // needs screen space position: no
                                                        // needs world space position: YES
                                                        // needs view direction: no
                                                        // needs world space view direction: no
                                                        // needs world space position for lighting: YES
                                                        // needs world space view direction for lighting: YES
                                                        // needs world space view direction for lightmaps: no
                                                        // needs vertex color: no
                                                        // needs VFACE: no
                                                        // needs SV_IsFrontFace: no
                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                        // reads from normal: no
                                                        // 0 texcoords actually used
                                                        #include "UnityCG.cginc"
                                                        #include "Lighting.cginc"
                                                        #include "UnityPBSLighting.cginc"

                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                        // Original surface shader snippet:
                                                        #line 20 ""
                                                        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                                                        #endif
                                                        /* UNITY: Original start of shader */
                                                                // Physically based Standard lighting model, and enable shadows on all light types
                                                                //#pragma surface surf Standard fullforwardshadows

                                                                // Use shader model 3.0 target, to get nicer looking lighting
                                                                //#pragma target 3.0

                                                                #include "UnityStandardUtils.cginc"

                                                                // flip UVs horizontally to correct for back side projection
                                                                #define TRIPLANAR_CORRECT_PROJECTED_U

                                                                // offset UVs to prevent obvious mirroring
                                                                // #define TRIPLANAR_UV_OFFSET

                                                                // Reoriented Normal Mapping
                                                                // http://blog.selfshadow.com/publications/blending-in-detail/
                                                                // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                                                                half3 blend_rnm(half3 n1, half3 n2)
                                                                {
                                                                    n1.z += 1;
                                                                    n2.xy = -n2.xy;

                                                                    return n1 * dot(n1, n2) / n1.z - n2;
                                                                }

                                                                sampler2D _MainTex;
                                                                float4 _MainTex_ST;

                                                                sampler2D _BumpMap;
                                                                sampler2D _OcclusionMap;
                                                                sampler2D _RoughnessMap;

                                                                half _Roughness;
                                                                half _Metallic;

                                                                half _OcclusionStrength;

                                                                struct Input {
                                                                    float3 worldPos;
                                                                    float3 worldNormal;
                                                                    INTERNAL_DATA
                                                                };

                                                                float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                                                                    float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                                                                    float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                                                                    float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                                                                    float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                                                                    return normalize(mul(t2w, normal));
                                                                }

                                                                void surf(Input IN, inout SurfaceOutputStandard o) {
                                                                    // work around bug where IN.worldNormal is always (0,0,0)!
                                                                    IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                                                                    // calculate triplanar blend
                                                                    half3 triblend = saturate(pow(IN.worldNormal, 4));
                                                                    triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                                                                    // calculate triplanar uvs
                                                                    // applying texture scale and offset values ala TRANSFORM_TEX macro
                                                                    float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                                                                    float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                                                                    float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                                                                    // offset UVs to prevent obvious mirroring
                                                                #if defined(TRIPLANAR_UV_OFFSET)
                                                                    uvY += 0.33;
                                                                    uvZ += 0.67;
                                                                #endif

                                                                    // minor optimization of sign(). prevents return value of 0
                                                                    half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                                                                    // flip UVs horizontally to correct for back side projection
                                                                #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                                    uvX.x *= axisSign.x;
                                                                    uvY.x *= axisSign.y;
                                                                    uvZ.x *= -axisSign.z;
                                                                #endif

                                                                    // albedo textures
                                                                    fixed4 colX = tex2D(_MainTex, uvX);
                                                                    fixed4 colY = tex2D(_MainTex, uvY);
                                                                    fixed4 colZ = tex2D(_MainTex, uvZ);
                                                                    fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                                                                    // roughness textures
                                                                    half roughX = tex2D(_RoughnessMap, uvX).r;
                                                                    half roughY = tex2D(_RoughnessMap, uvY).r;
                                                                    half roughZ = tex2D(_RoughnessMap, uvZ).r;
                                                                    half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                                                                    // occlusion textures
                                                                    half occX = tex2D(_OcclusionMap, uvX).g;
                                                                    half occY = tex2D(_OcclusionMap, uvY).g;
                                                                    half occZ = tex2D(_OcclusionMap, uvZ).g;
                                                                    half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                                                                    // tangent space normal maps
                                                                    half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                                                                    half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                                                                    half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                                                                    // flip normal maps' x axis to account for flipped UVs
                                                                #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                                    tnormalX.x *= axisSign.x;
                                                                    tnormalY.x *= axisSign.y;
                                                                    tnormalZ.x *= -axisSign.z;
                                                                #endif

                                                                    half3 absVertNormal = abs(IN.worldNormal);

                                                                    // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                                                                    tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                                                                    tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                                                                    tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                                                                    // apply world space sign to tangent space Z
                                                                    tnormalX.z *= axisSign.x;
                                                                    tnormalY.z *= axisSign.y;
                                                                    tnormalZ.z *= axisSign.z;

                                                                    // sizzle tangent normals to match world normal and blend together
                                                                    half3 worldNormal = normalize(
                                                                        tnormalX.zyx * triblend.x +
                                                                        tnormalY.xzy * triblend.y +
                                                                        tnormalZ.xyz * triblend.z
                                                                        );

                                                                    // set surface ouput properties
                                                                    o.Albedo = col.rgb;
                                                                    o.Metallic = _Metallic;
                                                                    o.Smoothness = 1 - rough;
                                                                    o.Occlusion = occ;

                                                                    // convert world space normals into tangent normals
                                                                    o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                                                                }


                                                                // vertex-to-fragment interpolation data
                                                                struct v2f_surf {
                                                                  UNITY_POSITION(pos);
                                                                  float4 tSpace0 : TEXCOORD0;
                                                                  float4 tSpace1 : TEXCOORD1;
                                                                  float4 tSpace2 : TEXCOORD2;
                                                                #ifndef DIRLIGHTMAP_OFF
                                                                  float3 viewDir : TEXCOORD3;
                                                                #endif
                                                                  float4 lmap : TEXCOORD4;
                                                                #ifndef LIGHTMAP_ON
                                                                  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                    half3 sh : TEXCOORD5; // SH
                                                                  #endif
                                                                #else
                                                                  #ifdef DIRLIGHTMAP_OFF
                                                                    float4 lmapFadePos : TEXCOORD5;
                                                                  #endif
                                                                #endif
                                                                  UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                  UNITY_VERTEX_OUTPUT_STEREO
                                                                };

                                                                // vertex shader
                                                                v2f_surf vert_surf(appdata_full v) {
                                                                  UNITY_SETUP_INSTANCE_ID(v);
                                                                  v2f_surf o;
                                                                  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                  UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                  o.pos = UnityObjectToClipPos(v.vertex);
                                                                  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
                                                                  #ifndef DIRLIGHTMAP_OFF
                                                                  o.viewDir.x = dot(viewDirForLight, worldTangent);
                                                                  o.viewDir.y = dot(viewDirForLight, worldBinormal);
                                                                  o.viewDir.z = dot(viewDirForLight, worldNormal);
                                                                  #endif
                                                                #ifdef DYNAMICLIGHTMAP_ON
                                                                  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                                #else
                                                                  o.lmap.zw = 0;
                                                                #endif
                                                                #ifdef LIGHTMAP_ON
                                                                  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                  #ifdef DIRLIGHTMAP_OFF
                                                                    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
                                                                    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
                                                                  #endif
                                                                #else
                                                                  o.lmap.xy = 0;
                                                                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                      o.sh = 0;
                                                                      o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                                    #endif
                                                                #endif
                                                                  return o;
                                                                }
                                                                #ifdef LIGHTMAP_ON
                                                                float4 unity_LightmapFade;
                                                                #endif
                                                                fixed4 unity_Ambient;

                                                                // fragment shader
                                                                void frag_surf(v2f_surf IN,
                                                                    out half4 outGBuffer0 : SV_Target0,
                                                                    out half4 outGBuffer1 : SV_Target1,
                                                                    out half4 outGBuffer2 : SV_Target2,
                                                                    out half4 outEmission : SV_Target3
                                                                #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                    , out half4 outShadowMask : SV_Target4
                                                                #endif
                                                                ) {
                                                                  UNITY_SETUP_INSTANCE_ID(IN);
                                                                  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                                                                  // prepare and unpack data
                                                                  Input surfIN;
                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                  #else
                                                                    UNITY_EXTRACT_FOG(IN);
                                                                  #endif
                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                    UNITY_RECONSTRUCT_TBN(IN);
                                                                  #else
                                                                    UNITY_EXTRACT_TBN(IN);
                                                                  #endif
                                                                  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                  surfIN.worldPos.x = 1.0;
                                                                  surfIN.worldNormal.x = 1.0;
                                                                  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                  #ifndef USING_DIRECTIONAL_LIGHT
                                                                    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                  #else
                                                                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                  #endif
                                                                  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                  surfIN.worldNormal = 0.0;
                                                                  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                  surfIN.worldPos = worldPos;
                                                                  #ifdef UNITY_COMPILER_HLSL
                                                                  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                  #else
                                                                  SurfaceOutputStandard o;
                                                                  #endif
                                                                  o.Albedo = 0.0;
                                                                  o.Emission = 0.0;
                                                                  o.Alpha = 0.0;
                                                                  o.Occlusion = 1.0;
                                                                  fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                  o.Normal = fixed3(0,0,1);

                                                                  // call surface function
                                                                  surf(surfIN, o);
                                                                fixed3 originalNormal = o.Normal;
                                                                  float3 worldN;
                                                                  worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                  worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                  worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                  worldN = normalize(worldN);
                                                                  o.Normal = worldN;
                                                                  half atten = 1;

                                                                  // Setup lighting environment
                                                                  UnityGI gi;
                                                                  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                  gi.indirect.diffuse = 0;
                                                                  gi.indirect.specular = 0;
                                                                  gi.light.color = 0;
                                                                  gi.light.dir = half3(0,1,0);
                                                                  // Call GI (lightmaps/SH/reflections) lighting function
                                                                  UnityGIInput giInput;
                                                                  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                                  giInput.light = gi.light;
                                                                  giInput.worldPos = worldPos;
                                                                  giInput.worldViewDir = worldViewDir;
                                                                  giInput.atten = atten;
                                                                  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                                    giInput.lightmapUV = IN.lmap;
                                                                  #else
                                                                    giInput.lightmapUV = 0.0;
                                                                  #endif
                                                                  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                    giInput.ambient = IN.sh;
                                                                  #else
                                                                    giInput.ambient.rgb = 0.0;
                                                                  #endif
                                                                  giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                                  giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                                  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                                    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                                  #endif
                                                                  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                                    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                                    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                                    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                                    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                                    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                                  #endif
                                                                  LightingStandard_GI(o, giInput, gi);

                                                                  // call lighting function to output g-buffer
                                                                  outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                                                                  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                    outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
                                                                  #endif
                                                                  #ifndef UNITY_HDR_ON
                                                                  outEmission.rgb = exp2(-outEmission.rgb);
                                                                  #endif
                                                                }


                                                                #endif


                                                                ENDCG

                                                                }

                                                // ---- meta information extraction pass:
                                                Pass {
                                                    Name "Meta"
                                                    Tags { "LightMode" = "Meta" }
                                                    Cull Off

                                            CGPROGRAM
                                                                    // compile directives
                                                                    #pragma vertex vert_surf
                                                                    #pragma fragment frag_surf
                                                                    #pragma target 3.0
                                                                    #pragma multi_compile_instancing
                                                                    #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
                                                                    #pragma shader_feature EDITOR_VISUALIZATION

                                                                    #include "HLSLSupport.cginc"
                                                                    #define UNITY_INSTANCED_LOD_FADE
                                                                    #define UNITY_INSTANCED_SH
                                                                    #define UNITY_INSTANCED_LIGHTMAPSTS
                                                                    #include "UnityShaderVariables.cginc"
                                                                    #include "UnityShaderUtilities.cginc"
                                                                    // -------- variant for: <when no other keywords are defined>
                                                                    #if !defined(INSTANCING_ON)
                                                                    // Surface shader code generated based on:
                                                                    // writes to per-pixel normal: YES
                                                                    // writes to emission: no
                                                                    // writes to occlusion: YES
                                                                    // needs world space reflection vector: no
                                                                    // needs world space normal vector: YES
                                                                    // needs screen space position: no
                                                                    // needs world space position: YES
                                                                    // needs view direction: no
                                                                    // needs world space view direction: no
                                                                    // needs world space position for lighting: YES
                                                                    // needs world space view direction for lighting: YES
                                                                    // needs world space view direction for lightmaps: no
                                                                    // needs vertex color: no
                                                                    // needs VFACE: no
                                                                    // needs SV_IsFrontFace: no
                                                                    // passes tangent-to-world matrix to pixel shader: YES
                                                                    // reads from normal: no
                                                                    // 0 texcoords actually used
                                                                    #include "UnityCG.cginc"
                                                                    #include "Lighting.cginc"
                                                                    #include "UnityPBSLighting.cginc"

                                                                    #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                    #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                    #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                    // Original surface shader snippet:
                                                                    #line 20 ""
                                                                    #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                                                                    #endif
                                                                    /* UNITY: Original start of shader */
                                                                            // Physically based Standard lighting model, and enable shadows on all light types
                                                                            //#pragma surface surf Standard fullforwardshadows

                                                                            // Use shader model 3.0 target, to get nicer looking lighting
                                                                            //#pragma target 3.0

                                                                            #include "UnityStandardUtils.cginc"

                                                                            // flip UVs horizontally to correct for back side projection
                                                                            #define TRIPLANAR_CORRECT_PROJECTED_U

                                                                            // offset UVs to prevent obvious mirroring
                                                                            // #define TRIPLANAR_UV_OFFSET

                                                                            // Reoriented Normal Mapping
                                                                            // http://blog.selfshadow.com/publications/blending-in-detail/
                                                                            // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                                                                            half3 blend_rnm(half3 n1, half3 n2)
                                                                            {
                                                                                n1.z += 1;
                                                                                n2.xy = -n2.xy;

                                                                                return n1 * dot(n1, n2) / n1.z - n2;
                                                                            }

                                                                            sampler2D _MainTex;
                                                                            float4 _MainTex_ST;

                                                                            sampler2D _BumpMap;
                                                                            sampler2D _OcclusionMap;
                                                                            sampler2D _RoughnessMap;

                                                                            half _Roughness;
                                                                            half _Metallic;

                                                                            half _OcclusionStrength;

                                                                            struct Input {
                                                                                float3 worldPos;
                                                                                float3 worldNormal;
                                                                                INTERNAL_DATA
                                                                            };

                                                                            float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                                                                                float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                                                                                float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                                                                                float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                                                                                float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                                                                                return normalize(mul(t2w, normal));
                                                                            }

                                                                            void surf(Input IN, inout SurfaceOutputStandard o) {
                                                                                // work around bug where IN.worldNormal is always (0,0,0)!
                                                                                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                                                                                // calculate triplanar blend
                                                                                half3 triblend = saturate(pow(IN.worldNormal, 4));
                                                                                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                                                                                // calculate triplanar uvs
                                                                                // applying texture scale and offset values ala TRANSFORM_TEX macro
                                                                                float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                                                                                float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                                                                                float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                                                                                // offset UVs to prevent obvious mirroring
                                                                            #if defined(TRIPLANAR_UV_OFFSET)
                                                                                uvY += 0.33;
                                                                                uvZ += 0.67;
                                                                            #endif

                                                                                // minor optimization of sign(). prevents return value of 0
                                                                                half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                                                                                // flip UVs horizontally to correct for back side projection
                                                                            #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                                                uvX.x *= axisSign.x;
                                                                                uvY.x *= axisSign.y;
                                                                                uvZ.x *= -axisSign.z;
                                                                            #endif

                                                                                // albedo textures
                                                                                fixed4 colX = tex2D(_MainTex, uvX);
                                                                                fixed4 colY = tex2D(_MainTex, uvY);
                                                                                fixed4 colZ = tex2D(_MainTex, uvZ);
                                                                                fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                                                                                // roughness textures
                                                                                half roughX = tex2D(_RoughnessMap, uvX).r;
                                                                                half roughY = tex2D(_RoughnessMap, uvY).r;
                                                                                half roughZ = tex2D(_RoughnessMap, uvZ).r;
                                                                                half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                                                                                // occlusion textures
                                                                                half occX = tex2D(_OcclusionMap, uvX).g;
                                                                                half occY = tex2D(_OcclusionMap, uvY).g;
                                                                                half occZ = tex2D(_OcclusionMap, uvZ).g;
                                                                                half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                                                                                // tangent space normal maps
                                                                                half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                                                                                half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                                                                                half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                                                                                // flip normal maps' x axis to account for flipped UVs
                                                                            #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                                                tnormalX.x *= axisSign.x;
                                                                                tnormalY.x *= axisSign.y;
                                                                                tnormalZ.x *= -axisSign.z;
                                                                            #endif

                                                                                half3 absVertNormal = abs(IN.worldNormal);

                                                                                // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                                                                                tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                                                                                tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                                                                                tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                                                                                // apply world space sign to tangent space Z
                                                                                tnormalX.z *= axisSign.x;
                                                                                tnormalY.z *= axisSign.y;
                                                                                tnormalZ.z *= axisSign.z;

                                                                                // sizzle tangent normals to match world normal and blend together
                                                                                half3 worldNormal = normalize(
                                                                                    tnormalX.zyx * triblend.x +
                                                                                    tnormalY.xzy * triblend.y +
                                                                                    tnormalZ.xyz * triblend.z
                                                                                    );

                                                                                // set surface ouput properties
                                                                                o.Albedo = col.rgb;
                                                                                o.Metallic = _Metallic;
                                                                                o.Smoothness = 1 - rough;
                                                                                o.Occlusion = occ;

                                                                                // convert world space normals into tangent normals
                                                                                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                                                                            }

                                                                    #include "UnityMetaPass.cginc"

                                                                            // vertex-to-fragment interpolation data
                                                                            struct v2f_surf {
                                                                              UNITY_POSITION(pos);
                                                                              float4 tSpace0 : TEXCOORD0;
                                                                              float4 tSpace1 : TEXCOORD1;
                                                                              float4 tSpace2 : TEXCOORD2;
                                                                            #ifdef EDITOR_VISUALIZATION
                                                                              float2 vizUV : TEXCOORD3;
                                                                              float4 lightCoord : TEXCOORD4;
                                                                            #endif
                                                                              UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                              UNITY_VERTEX_OUTPUT_STEREO
                                                                            };

                                                                            // vertex shader
                                                                            v2f_surf vert_surf(appdata_full v) {
                                                                              UNITY_SETUP_INSTANCE_ID(v);
                                                                              v2f_surf o;
                                                                              UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                              UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                              o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
                                                                            #ifdef EDITOR_VISUALIZATION
                                                                              o.vizUV = 0;
                                                                              o.lightCoord = 0;
                                                                              if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                                                                                o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
                                                                              else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
                                                                              {
                                                                                o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
                                                                              }
                                                                            #endif
                                                                              float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                              float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                              fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                              fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                              fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                              o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                              o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                              o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                              return o;
                                                                            }

                                                                            // fragment shader
                                                                            fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                              UNITY_SETUP_INSTANCE_ID(IN);
                                                                              UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                                                                              // prepare and unpack data
                                                                              Input surfIN;
                                                                              #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                              #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                              #else
                                                                                UNITY_EXTRACT_FOG(IN);
                                                                              #endif
                                                                              #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                UNITY_RECONSTRUCT_TBN(IN);
                                                                              #else
                                                                                UNITY_EXTRACT_TBN(IN);
                                                                              #endif
                                                                              UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                              surfIN.worldPos.x = 1.0;
                                                                              surfIN.worldNormal.x = 1.0;
                                                                              float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                              #ifndef USING_DIRECTIONAL_LIGHT
                                                                                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                              #else
                                                                                fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                              #endif
                                                                              surfIN.worldNormal = 0.0;
                                                                              surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                              surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                              surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                              surfIN.worldPos = worldPos;
                                                                              #ifdef UNITY_COMPILER_HLSL
                                                                              SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                              #else
                                                                              SurfaceOutputStandard o;
                                                                              #endif
                                                                              o.Albedo = 0.0;
                                                                              o.Emission = 0.0;
                                                                              o.Alpha = 0.0;
                                                                              o.Occlusion = 1.0;
                                                                              fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                              o.Normal = fixed3(0,0,1);

                                                                              // call surface function
                                                                              surf(surfIN, o);
                                                                              UnityMetaInput metaIN;
                                                                              UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
                                                                              metaIN.Albedo = o.Albedo;
                                                                              metaIN.Emission = o.Emission;
                                                                            #ifdef EDITOR_VISUALIZATION
                                                                              metaIN.VizUV = IN.vizUV;
                                                                              metaIN.LightCoord = IN.lightCoord;
                                                                            #endif
                                                                              return UnityMetaFragment(metaIN);
                                                                            }


                                                                            #endif

                                                                                // -------- variant for: INSTANCING_ON 
                                                                                #if defined(INSTANCING_ON)
                                                                                // Surface shader code generated based on:
                                                                                // writes to per-pixel normal: YES
                                                                                // writes to emission: no
                                                                                // writes to occlusion: YES
                                                                                // needs world space reflection vector: no
                                                                                // needs world space normal vector: YES
                                                                                // needs screen space position: no
                                                                                // needs world space position: YES
                                                                                // needs view direction: no
                                                                                // needs world space view direction: no
                                                                                // needs world space position for lighting: YES
                                                                                // needs world space view direction for lighting: YES
                                                                                // needs world space view direction for lightmaps: no
                                                                                // needs vertex color: no
                                                                                // needs VFACE: no
                                                                                // needs SV_IsFrontFace: no
                                                                                // passes tangent-to-world matrix to pixel shader: YES
                                                                                // reads from normal: no
                                                                                // 0 texcoords actually used
                                                                                #include "UnityCG.cginc"
                                                                                #include "Lighting.cginc"
                                                                                #include "UnityPBSLighting.cginc"

                                                                                #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                // Original surface shader snippet:
                                                                                #line 20 ""
                                                                                #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                                                                                #endif
                                                                                /* UNITY: Original start of shader */
                                                                                        // Physically based Standard lighting model, and enable shadows on all light types
                                                                                        //#pragma surface surf Standard fullforwardshadows

                                                                                        // Use shader model 3.0 target, to get nicer looking lighting
                                                                                        //#pragma target 3.0

                                                                                        #include "UnityStandardUtils.cginc"

                                                                                        // flip UVs horizontally to correct for back side projection
                                                                                        #define TRIPLANAR_CORRECT_PROJECTED_U

                                                                                        // offset UVs to prevent obvious mirroring
                                                                                        // #define TRIPLANAR_UV_OFFSET

                                                                                        // Reoriented Normal Mapping
                                                                                        // http://blog.selfshadow.com/publications/blending-in-detail/
                                                                                        // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
                                                                                        half3 blend_rnm(half3 n1, half3 n2)
                                                                                        {
                                                                                            n1.z += 1;
                                                                                            n2.xy = -n2.xy;

                                                                                            return n1 * dot(n1, n2) / n1.z - n2;
                                                                                        }

                                                                                        sampler2D _MainTex;
                                                                                        float4 _MainTex_ST;

                                                                                        sampler2D _BumpMap;
                                                                                        sampler2D _OcclusionMap;
                                                                                        sampler2D _RoughnessMap;

                                                                                        half _Roughness;
                                                                                        half _Metallic;

                                                                                        half _OcclusionStrength;

                                                                                        struct Input {
                                                                                            float3 worldPos;
                                                                                            float3 worldNormal;
                                                                                            INTERNAL_DATA
                                                                                        };

                                                                                        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                                                                                            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                                                                                            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                                                                                            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                                                                                            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                                                                                            return normalize(mul(t2w, normal));
                                                                                        }

                                                                                        void surf(Input IN, inout SurfaceOutputStandard o) {
                                                                                            // work around bug where IN.worldNormal is always (0,0,0)!
                                                                                            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                                                                                            // calculate triplanar blend
                                                                                            half3 triblend = saturate(pow(IN.worldNormal, 4));
                                                                                            triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                                                                                            // calculate triplanar uvs
                                                                                            // applying texture scale and offset values ala TRANSFORM_TEX macro
                                                                                            float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                                                                                            float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                                                                                            float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                                                                                            // offset UVs to prevent obvious mirroring
                                                                                        #if defined(TRIPLANAR_UV_OFFSET)
                                                                                            uvY += 0.33;
                                                                                            uvZ += 0.67;
                                                                                        #endif

                                                                                            // minor optimization of sign(). prevents return value of 0
                                                                                            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                                                                                            // flip UVs horizontally to correct for back side projection
                                                                                        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                                                            uvX.x *= axisSign.x;
                                                                                            uvY.x *= axisSign.y;
                                                                                            uvZ.x *= -axisSign.z;
                                                                                        #endif

                                                                                            // albedo textures
                                                                                            fixed4 colX = tex2D(_MainTex, uvX);
                                                                                            fixed4 colY = tex2D(_MainTex, uvY);
                                                                                            fixed4 colZ = tex2D(_MainTex, uvZ);
                                                                                            fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

                                                                                            // roughness textures
                                                                                            half roughX = tex2D(_RoughnessMap, uvX).r;
                                                                                            half roughY = tex2D(_RoughnessMap, uvY).r;
                                                                                            half roughZ = tex2D(_RoughnessMap, uvZ).r;
                                                                                            half rough = LerpOneTo(roughX * triblend.x + roughY * triblend.y + roughZ * triblend.z, _OcclusionStrength) * _Roughness;

                                                                                            // occlusion textures
                                                                                            half occX = tex2D(_OcclusionMap, uvX).g;
                                                                                            half occY = tex2D(_OcclusionMap, uvY).g;
                                                                                            half occZ = tex2D(_OcclusionMap, uvZ).g;
                                                                                            half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);

                                                                                            // tangent space normal maps
                                                                                            half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                                                                                            half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                                                                                            half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                                                                                            // flip normal maps' x axis to account for flipped UVs
                                                                                        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                                                                                            tnormalX.x *= axisSign.x;
                                                                                            tnormalY.x *= axisSign.y;
                                                                                            tnormalZ.x *= -axisSign.z;
                                                                                        #endif

                                                                                            half3 absVertNormal = abs(IN.worldNormal);

                                                                                            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                                                                                            tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                                                                                            tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                                                                                            tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                                                                                            // apply world space sign to tangent space Z
                                                                                            tnormalX.z *= axisSign.x;
                                                                                            tnormalY.z *= axisSign.y;
                                                                                            tnormalZ.z *= axisSign.z;

                                                                                            // sizzle tangent normals to match world normal and blend together
                                                                                            half3 worldNormal = normalize(
                                                                                                tnormalX.zyx * triblend.x +
                                                                                                tnormalY.xzy * triblend.y +
                                                                                                tnormalZ.xyz * triblend.z
                                                                                                );

                                                                                            // set surface ouput properties
                                                                                            o.Albedo = col.rgb;
                                                                                            o.Metallic = _Metallic;
                                                                                            o.Smoothness = 1 - rough;
                                                                                            o.Occlusion = occ;

                                                                                            // convert world space normals into tangent normals
                                                                                            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
                                                                                        }

                                                                                #include "UnityMetaPass.cginc"

                                                                                        // vertex-to-fragment interpolation data
                                                                                        struct v2f_surf {
                                                                                          UNITY_POSITION(pos);
                                                                                          float4 tSpace0 : TEXCOORD0;
                                                                                          float4 tSpace1 : TEXCOORD1;
                                                                                          float4 tSpace2 : TEXCOORD2;
                                                                                        #ifdef EDITOR_VISUALIZATION
                                                                                          float2 vizUV : TEXCOORD3;
                                                                                          float4 lightCoord : TEXCOORD4;
                                                                                        #endif
                                                                                          UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                          UNITY_VERTEX_OUTPUT_STEREO
                                                                                        };

                                                                                        // vertex shader
                                                                                        v2f_surf vert_surf(appdata_full v) {
                                                                                          UNITY_SETUP_INSTANCE_ID(v);
                                                                                          v2f_surf o;
                                                                                          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                          UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                          o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
                                                                                        #ifdef EDITOR_VISUALIZATION
                                                                                          o.vizUV = 0;
                                                                                          o.lightCoord = 0;
                                                                                          if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                                                                                            o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
                                                                                          else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
                                                                                          {
                                                                                            o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                            o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
                                                                                          }
                                                                                        #endif
                                                                                          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                          fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                          fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                          fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                          o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                          o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                          o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                          return o;
                                                                                        }

                                                                                        // fragment shader
                                                                                        fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                          UNITY_SETUP_INSTANCE_ID(IN);
                                                                                          UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                                                                                          // prepare and unpack data
                                                                                          Input surfIN;
                                                                                          #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                            UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                          #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                            UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                          #else
                                                                                            UNITY_EXTRACT_FOG(IN);
                                                                                          #endif
                                                                                          #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                            UNITY_RECONSTRUCT_TBN(IN);
                                                                                          #else
                                                                                            UNITY_EXTRACT_TBN(IN);
                                                                                          #endif
                                                                                          UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                          surfIN.worldPos.x = 1.0;
                                                                                          surfIN.worldNormal.x = 1.0;
                                                                                          float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                          #ifndef USING_DIRECTIONAL_LIGHT
                                                                                            fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                          #else
                                                                                            fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                          #endif
                                                                                          surfIN.worldNormal = 0.0;
                                                                                          surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                          surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                          surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                          surfIN.worldPos = worldPos;
                                                                                          #ifdef UNITY_COMPILER_HLSL
                                                                                          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                          #else
                                                                                          SurfaceOutputStandard o;
                                                                                          #endif
                                                                                          o.Albedo = 0.0;
                                                                                          o.Emission = 0.0;
                                                                                          o.Alpha = 0.0;
                                                                                          o.Occlusion = 1.0;
                                                                                          fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                          o.Normal = fixed3(0,0,1);

                                                                                          // call surface function
                                                                                          surf(surfIN, o);
                                                                                          UnityMetaInput metaIN;
                                                                                          UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
                                                                                          metaIN.Albedo = o.Albedo;
                                                                                          metaIN.Emission = o.Emission;
                                                                                        #ifdef EDITOR_VISUALIZATION
                                                                                          metaIN.VizUV = IN.vizUV;
                                                                                          metaIN.LightCoord = IN.lightCoord;
                                                                                        #endif
                                                                                          return UnityMetaFragment(metaIN);
                                                                                        }


                                                                                        #endif


                                                                                        ENDCG

                                                                                        }

                                                                    // ---- end of surface shader generated code

                                                                #LINE 162

        }
            FallBack "Diffuse"
}
