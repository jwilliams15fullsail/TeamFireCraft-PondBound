using Artngame.SKYMASTER;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

#if UNITY_2023_3_OR_NEWER
//GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace Artngame.Oceanis// UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Copy the given color buffer to the given destination color buffer.
    ///
    /// You can use this pass to copy a color buffer to the destination,
    /// so you can use it later in rendering. For example, you can copy
    /// the opaque texture to use it for distortion effects.
    /// </summary>
    internal class BlitPassSunShaftsWaterSRP : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {

#if UNITY_2023_3_OR_NEWER
        //GRAPH
        /// ///////// GRAPH
        /// </summary>
        // This class stores the data needed by the pass, passed as parameter to the delegate function that executes the pass
        private class PassData
        {    //v0.1               
            internal TextureHandle src;
            public Material BlitMaterial { get; set; }
        }
        private Material m_BlitMaterial;

        TextureHandle tmpBuffer1A;
        TextureHandle tmpBuffer1Aa;

        RTHandle _handleA;
        TextureHandle tmpBuffer2A;

        RTHandle _handleTAART;
        TextureHandle _handleTAA;

        RTHandle _handleTAART2;
        TextureHandle _handleTAA2;

        RTHandle _handleTAART3;
        TextureHandle _handleTAA3;
        RTHandle _handleTAART4;
        TextureHandle _handleTAA4;

        Camera currentCamera;
        float prevDownscaleFactor;//v0.1
        //public Material blitMaterial = null;

        int offset = 6;

        // Each ScriptableRenderPass can use the RenderGraph handle to add multiple render passes to the render graph
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (connector == null)
            {
                connector = cameraData.camera.GetComponent<connectSuntoSunShaftsWaterURP>();
                if (connector == null && Camera.main != null)
                {
                    connector = Camera.main.GetComponent<connectSuntoSunShaftsWaterURP>();

                    //v0.2
                    if (connector == null)
                    {
                        try
                        {
                            GameObject effects = GameObject.FindWithTag("SkyMasterEffects");
                            if (effects != null)
                            {
                                connector = effects.GetComponent<connectSuntoSunShaftsWaterURP>();
                            }
                        }
                        catch
                        { }
                    }
                }
            }
            //Debug.Log(Camera.main.GetComponent<connectSuntoSunShaftsURP>().sun.transform.position);
            if (connector != null)
            {
                //this.enableShafts = connector.enableShafts;
                //this.sunTransform = connector.sun.transform.position;
                //this.screenBlendMode = connector.screenBlendMode;
                ////public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
                //this.radialBlurIterations = connector.radialBlurIterations;
                //this.sunColor = connector.sunColor;
                //this.sunThreshold = connector.sunThreshold;
                //this.sunShaftBlurRadius = connector.sunShaftBlurRadius;
                //this.sunShaftIntensity = connector.sunShaftIntensity;
                //this.maxRadius = connector.maxRadius;
                //this.useDepthTexture = connector.useDepthTexture;
            



                this.enableShafts = connector.enableShafts;
                this.sunTransform = connector.sun.transform.position;
                this.screenBlendMode = connector.screenBlendMode;
                //public Vector3 sunTransform = new Vector3(0f, 0f, 0f);

                this.autoRegulateEffect = connector.autoRegulateEffect;
                this.cutoffHeigth = connector.cutoffHeigth;


                this.radialBlurIterations = connector.radialBlurIterations;
                this.sunColor = connector.sunColor;
                this.sunThreshold = connector.sunThreshold;
                this.sunShaftBlurRadius = connector.sunShaftBlurRadius;
                this.sunShaftIntensity = connector.sunShaftIntensity;
                this.maxRadius = connector.maxRadius;
                this.useDepthTexture = connector.useDepthTexture;



                //v4.8
                intensity = connector.intensity;
                shaftsFollowCamera = connector.shaftsFollowCamera;
                useWaterAirMask = connector.useWaterAirMask;
                resetScaling = connector.resetScaling;
                useVolumeMaskRenderer = connector.useVolumeMaskRenderer;
                useTexture2DMask = connector.useTexture2DMask;
                MaskMap = connector.MaskMap;
                MaskMapA = connector.MaskMapA;
                waterHeight = connector.waterHeight;
                BumpMap = connector.BumpMap;
                underwaterDepthFade = connector.underwaterDepthFade;
                BumpIntensity = connector.BumpIntensity;
                BumpVelocity = connector.BumpVelocity;
                BumpScale = connector.BumpScale;
                underWaterTint = connector.underWaterTint;
                BumpIntensityRL = connector.BumpIntensityRL;
                BumpScaleRL = connector.BumpScaleRL;
                BumpLineHeight = connector.BumpLineHeight;
                refractLineWidth = connector.refractLineWidth;
                refractLineXDisp = connector.refractLineXDisp;
                refractLineXDispA = connector.refractLineXDispA;
                refractLineFade = connector.refractLineFade;
                scalerMask = connector.scalerMask;
            }

            //if still null, disable effect
            bool connectorFound = true;
            if (connector == null)
            {
                connectorFound = false;
            }

            if (enableShafts && connectorFound && (!connector.autoRegulateEffect || (connector.autoRegulateEffect && Camera.main.transform.position.y < connector.cutoffHeigth)))
            {

                bool useOnlyShafts = false;
                if (useOnlyShafts)
                {
                    GraphSunShaftsOnly(renderGraph, frameData);
                }
                else
                {
                    GraphOceanis(renderGraph, frameData);
                }
            }
            
        }//END MAIN GRAPH

        void GraphOceanis(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (Camera.main != null)
            {
                m_BlitMaterial = blitMaterial;
                Camera.main.depthTextureMode = DepthTextureMode.Depth;
                
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                if (Camera.main != null && cameraData.camera != Camera.main)
                {
                    return;
                }

                //CONFIGURE
                float downScaler = 1;
                float downScaledX = (desc.width / (float)(downScaler));
                float downScaledY = (desc.height / (float)(downScaler));

                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;
                int rtW = desc.width;
                int rtH = desc.height;
                int xres = (int)(rtW / ((float)1));
                int yres = (int)(rtH / ((float)1));
                if (_handleA == null || _handleA.rt.width != xres || _handleA.rt.height != yres)
                {
                    //_handleA = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D);
                    _handleA = RTHandles.Alloc(Mathf.CeilToInt(downScaledX), Mathf.CeilToInt(downScaledY), colorFormat: GraphicsFormat.R32G32B32A32_SFloat,
                        dimension: TextureDimension.Tex2D);
                }
                tmpBuffer2A = renderGraph.ImportTexture(_handleA);//reflectionMapID                            

                if (_handleTAART == null || _handleTAART.rt.width != xres/4 || _handleTAART.rt.height != yres/4 || _handleTAART.rt.useMipMap == false)
                {
                    //_handleTAART.rt.DiscardContents();
                    //_handleTAART.rt.useMipMap = true;// = 8;
                    //_handleTAART.rt.autoGenerateMips = true;                       
                    _handleTAART = RTHandles.Alloc(xres / 4, yres / 4, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureDimension.Tex2D,
                        useMipMap: true, autoGenerateMips: true
                        );
                    _handleTAART.rt.wrapMode = TextureWrapMode.Clamp;
                    _handleTAART.rt.filterMode = FilterMode.Trilinear;
                    //Debug.Log(_handleTAART.rt.mipmapCount);
                }
                _handleTAA = renderGraph.ImportTexture(_handleTAART); //_TempTex

                if (_handleTAART2 == null || _handleTAART2.rt.width != xres || _handleTAART2.rt.height != yres || _handleTAART2.rt.useMipMap == false)
                {
                    _handleTAART2 = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D,
                        useMipMap: true, autoGenerateMips: true
                        );
                    _handleTAART2.rt.wrapMode = TextureWrapMode.Clamp;
                    _handleTAART2.rt.filterMode = FilterMode.Trilinear;
                }
                _handleTAA2 = renderGraph.ImportTexture(_handleTAART2); //_TempTex


                if (_handleTAART3 == null || _handleTAART3.rt.width != xres || _handleTAART3.rt.height != yres || _handleTAART3.rt.useMipMap == false)
                {
                    _handleTAART3 = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D,
                        useMipMap: true, autoGenerateMips: true
                        );
                    _handleTAART3.rt.wrapMode = TextureWrapMode.Clamp;
                    _handleTAART3.rt.filterMode = FilterMode.Trilinear;
                }
                _handleTAA3 = renderGraph.ImportTexture(_handleTAART3); //_TempTex

                if (_handleTAART4 == null || _handleTAART4.rt.width != xres || _handleTAART4.rt.height != yres || _handleTAART4.rt.useMipMap == false)
                {
                    _handleTAART4 = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D,
                        useMipMap: true, autoGenerateMips: true
                        );
                    _handleTAART4.rt.wrapMode = TextureWrapMode.Clamp;
                    _handleTAART4.rt.filterMode = FilterMode.Trilinear;
                }
                _handleTAA4 = renderGraph.ImportTexture(_handleTAART4); //_TempTex


                tmpBuffer1A = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "tmpBuffer1A", true);
                tmpBuffer1Aa = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "tmpBuffer1Aa", false);
                TextureHandle sourceTexture = resourceData.activeColorTexture;

                ////////////////  SUN SHAFTS  ////////////////////////////////////////////////////////////////////////////////
                Material sheetSHAFTS = blitMaterial;



                /////////////////////////////////////////////////////////////////////////////////// OCEANIS START
                //v4.8
                //int rtW = opaqueDesc.width;
                //int rtH = opaqueDesc.height;
        //        var formatA = RenderTextureFormat.DefaultHDR; //v3.4.9
        //        RenderTexture tmpBufferA = RenderTexture.GetTemporary(rtW, rtH, 0, formatA);
                //v0.1
        //        cmd.Blit(source, tmpBufferA);
                sheetSHAFTS.SetFloat("_Intensity", intensity);
        //        sheetSHAFTS.SetTexture("_InputTexture", tmpBufferA);
                //v4.9
        //        RenderTexture TemporaryColorTextureScaled = RenderTexture.GetTemporary(rtW, rtH, 0, formatA);
                //v4.8
                if (useWaterAirMask)
                {
                    if (Camera.main != null)
                    {
                        float camXRot = Camera.main.transform.eulerAngles.x;
                        if (camXRot > 180)
                        {
                            camXRot = -(360 - camXRot);
                        }
                        sheetSHAFTS.SetFloat("_CamXRot", camXRot);
                    }
         //           sheetSHAFTS.SetTexture("_InputTexture", tmpBufferA);
                    sheetSHAFTS.SetFloat("waterHeight", waterHeight);
         //            cmd.Blit(source, TemporaryColorTextureScaled, sheetSHAFTS, 7);
                }
                else if (useVolumeMaskRenderer)
                {
                    if (useTexture2DMask)
                    {
                       // sheetSHAFTS.SetTexture("_InputTextureA", MaskMapA); //MaskMap
                    }
                    else
                    {
                       // sheetSHAFTS.SetTexture("_InputTextureA", MaskMap); //MaskMap
                    }
                    sheetSHAFTS.SetFloat("_Height", Camera.main.transform.position.y);
                    //          cmd.Blit(source, TemporaryColorTextureScaled, sheetSHAFTS, 11);

                   

                }
                else
                {
          //          cmd.Blit(source, TemporaryColorTextureScaled, sheetSHAFTS, 10);
                }
                //END v4.8
                //sheetSHAFTS.SetFloat("_Blend", blend);
                /////////////////////////////////////////////////////////////////////////////////// END OCEANIS START

                string passNameAAaaa = "DO 1aaaa";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAAaaa, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.SetRenderAttachment(_handleTAA4, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    //   ExecuteBlitPassCLEAR(data, context, 14, passData.src));// 
                    ExecuteBlitPass(data, context, 22, passData.src));
                }

                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(_handleTAA4, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(sourceTexture, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}
                //return;


                sheetSHAFTS.SetFloat("_Blend", blend);
                Camera camera = Camera.main;
                if (useDepthTexture)
                {
                    camera.depthTextureMode |= DepthTextureMode.Depth;
                }
                Vector3 v = Vector3.one * 0.5f;
                if (sunTransform != Vector3.zero)
                {
                    v = Camera.main.WorldToViewportPoint(sunTransform);// - Camera.main.transform.position;
                }
                else
                {
                    v = new Vector3(0.5f, 0.5f, 0.0f);
                }

                var formatA = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; 
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
                sheetSHAFTS.SetVector("_SunThreshold", sunThreshold);

                //Debug.Log("IN1");
                //             cmd.Blit(source, m_TemporaryColorTexture); //KEEP BACKGROUND
                string passNameA = "DO 2";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameA, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer1A, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    ExecuteBlitPassCLEAR(data, context, 14 + offset, passData.src));// 
                    //   ExecuteBlitPassCLEAR(data, context, 14 + offset, passData.src));// 
                    //ExecuteBlitPass(data, context, 7, passData.src));
                }
                
                passNameA = "DO 2a";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameA, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer2A, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    //   ExecuteBlitPassCLEAR(data, context, 14, passData.src));// 
                    ExecuteBlitPass(data, context, 7 + offset, passData.src));
                }
                
                if (!useDepthTexture)
                {
                    var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                    string passNameAA = "DO 1";
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAA, out var passData))
                    {
                        passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        builder.UseTexture(passData.src, AccessFlags.Read);
                        builder.UseTexture(tmpBuffer1A, AccessFlags.Read);
                        builder.SetRenderAttachment(_handleTAA2, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        passData.BlitMaterial = sheetSHAFTS;
                        builder.AllowGlobalStateModification(true);
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPassTWO2(data, context, 3 + 8 + offset, passData.src, tmpBuffer1A));
                    }
                }
                else
                {
                    string passNameAAA = "DO 1";
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAAA, out var passData))
                    {
                        passData.src = resourceData.activeColorTexture;
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        builder.UseTexture(passData.src, AccessFlags.Read);
                        builder.UseTexture(tmpBuffer1A, AccessFlags.Read);
                        builder.SetRenderAttachment(_handleTAA2, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        passData.BlitMaterial = sheetSHAFTS;
                        builder.AllowGlobalStateModification(true);
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPassTWO2(data, context, 2 + 8 + offset, passData.src, tmpBuffer1A));
                    }
                }

                radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);

                float ofs = sunShaftBlurRadius * (1.0f / 768.0f);
                // Debug.Log(ofs);
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

                float adjustX = 0.5f;
                if (v.x < 0.5f)
                {
                    float diff = 0.5f - v.x;
                    adjustX = adjustX - 0.5f * diff;
                }
                float adjustY = 0.5f;
                if (v.y > 1.25f)
                {
                    float diff2 = v.y - 1.25f;
                    adjustY = adjustY - 0.3f * diff2;
                }
                if (v.y > 1.8f)
                {
                    v.y = 1.8f;
                    float diff3 = v.y - 1.25f;
                    adjustY = 0.5f - 0.3f * diff3;
                }

                sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x * 0.5f + adjustX, v.y * 0.5f + adjustY, v.z, maxRadius));
                
                //TEST2                
                for (int it2 = 0; it2 < radialBlurIterations; it2++)
                {
                    string passName = "SAVE TEMP" + it2;
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    {
                        //passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                        passData.src = resourceData.activeColorTexture;
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        //builder.UseTexture(passData.src, IBaseRenderGraphBuilder.AccessFlags.Read);
                        builder.UseTexture(_handleTAA2, AccessFlags.Read);
                        builder.SetRenderAttachment(_handleTAA, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        //builder.AllowGlobalStateModification(true);
                        passData.BlitMaterial = sheetSHAFTS;
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPassA(data, context, 1 + 8 + offset, _handleTAA2));
                    }

                    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve1", out var passData, m_ProfilingSampler))
                    {
                        passData.BlitMaterial = sheetSHAFTS;
                        // Similar to the previous pass, however now we set destination texture as input and source as output.
                        builder.UseTexture(_handleTAA, AccessFlags.Read);
                        passData.src = _handleTAA;
                        builder.SetRenderAttachment(_handleTAA2, 0, AccessFlags.Write);
                        builder.AllowGlobalStateModification(true);
                        // We use the same BlitTexture API to perform the Blit operation.
                        builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                    }

                    ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                    sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
                    
                    ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                    sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
                }

                if (v.z >= 0.0f)
                {
                    sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
                }
                else
                {
                    sheetSHAFTS.SetVector("_SunColor", Vector4.zero); // no backprojection !
                }
                //       cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer);
                //       cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, (screenBlendMode == BlitSunShaftsSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);

                string passNameAAa = "DO 1aa";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAAa, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.UseTexture(_handleTAA2, AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer2A, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecuteBlitPassTWO2a(data, context, 0 + 8 + offset, passData.src, _handleTAA2));
                }

                ////BLIT FINAL
                //cmd.Blit(temp1, renderingData.cameraData.renderer.cameraColorTargetHandle); //v0.1
                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(tmpBuffer2A, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(sourceTexture, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}
                //return;

                //////////////////////// OCEANIS MORE
                //v0.1
                //            cmd.Blit(m_TemporaryColorTexture, lrColorB, sheetSHAFTS,
                //                (screenBlendMode == BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);


                //v4.8
                //var UPsampleProperties1 = new MaterialPropertyBlock();
                // UPsampleProperties1.SetTexture("_WaterInterfaceTex", m_TemporaryColorTextureScaled);
                if (useTexture2DMask)
                {
                    sheetSHAFTS.SetTexture("_WaterInterfaceTex", MaskMapA);
                }
                else
                {
   //                 sheetSHAFTS.SetTexture("_WaterInterfaceTex", TemporaryColorTextureScaled);// MaskMap);
                }
    //            sheetSHAFTS.SetTexture("_MainTex", lrColorB);
    ////            sheetSHAFTS.SetTexture("_SourceTex", tmpBufferA);
                sheetSHAFTS.SetFloat("scalerMask", scalerMask);
                //v1.3
                ////////////////// REFRACT LINE
                if (BumpMap != null)
                {
                    sheetSHAFTS.SetTexture("_BumpTex", BumpMap);
                    //Debug.Log(BumpMap.value.width);
                }
                sheetSHAFTS.SetFloat("_BumpMagnitude", BumpIntensity);
                sheetSHAFTS.SetFloat("_BumpScale", BumpScale);
                //v4.6
                sheetSHAFTS.SetFloat("_BumpMagnitudeRL", BumpIntensityRL);
                sheetSHAFTS.SetFloat("_BumpScaleRL", BumpScaleRL);
                sheetSHAFTS.SetFloat("_BumpLineHeight", BumpLineHeight);
                sheetSHAFTS.SetVector("_BumpVelocity", BumpVelocity);
                sheetSHAFTS.SetVector("_underWaterTint", underWaterTint);
                sheetSHAFTS.SetFloat("_underwaterDepthFade", underwaterDepthFade);
                sheetSHAFTS.SetFloat("_refractLineWidth", refractLineWidth); //v4.6
                sheetSHAFTS.SetFloat("_refractLineXDisp", refractLineXDisp); //v4.6
                sheetSHAFTS.SetFloat("_refractLineXDispA", refractLineXDispA); //v4.6
                sheetSHAFTS.SetVector("_refractLineFade", refractLineFade); //v4.6
                                                                            ////////////////// END REFRACT LINE ////
                                                                            //v0.1
                                                                            //cmd.Blit(lrColorB, source, sheetSHAFTS, 8);

                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve1", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(tmpBuffer2A, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(_handleTAA3, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}

                string passNameAAaa = "DO 1aaa";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAAaa, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.UseTexture(tmpBuffer2A, AccessFlags.Read);
                    builder.UseTexture(_handleTAA4, AccessFlags.Read);
                    //builder.UseTexture(_handleTAA3, IBaseRenderGraphBuilder.AccessFlags.Read);
                    //builder.UseTexture(_handleTAA3, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.SetRenderAttachment(_handleTAA3, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    //builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecuteBlitPassTWO2b(data, context, 21, tmpBuffer2A, _handleTAA4, MaskMap, BumpMap, connector.cutoffHeigth));//ExecuteBlitPassTWO2b(data, context, 21, tmpBuffer2A, _handleTAA4));
                }

                //BLIT FINAL
                //cmd.Blit(temp1, renderingData.cameraData.renderer.cameraColorTargetHandle); //v0.1
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                {
                    passData.BlitMaterial = sheetSHAFTS;
                    // Similar to the previous pass, however now we set destination texture as input and source as output.
                    builder.UseTexture(_handleTAA3, AccessFlags.Read);
                    passData.src = _handleTAA3;
                    builder.SetRenderAttachment(sourceTexture, 0, AccessFlags.Write);
                    builder.AllowGlobalStateModification(true);
                    // We use the same BlitTexture API to perform the Blit operation.
                    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                }

            }//END CAMERA CHECK
        }//END FULL OCEANIS GRAPH

        void GraphSunShaftsOnly(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (Camera.main != null)
            {
                //ConfigureInput(ScriptableRenderPassInput.Color);
                //ConfigureInput(ScriptableRenderPassInput.Depth);

                m_BlitMaterial = blitMaterial;

                Camera.main.depthTextureMode = DepthTextureMode.Depth;

                //if (cameraData != null)
                //{
                //    Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
                //    //Matrix4x4 projectionMatrix = cameraData.GetGPUProjectionMatrix(0);
                //    Matrix4x4 projectionMatrix = cameraData.camera.projectionMatrix;
                //    //projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);
                //}

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                if (Camera.main != null && cameraData.camera != Camera.main)
                {
                    return;
                }

                //CONFIGURE

                //reflectionMapID = Shader.PropertyToID("_ReflectedColorMap");
                float downScaler = 1;
                float downScaledX = (desc.width / (float)(downScaler));
                float downScaledY = (desc.height / (float)(downScaler));
                //cmd.GetTemporaryRT(reflectionMapID, Mathf.CeilToInt(downScaledX), Mathf.CeilToInt(downScaledY), 0, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, 1, false);

                //tempRenderID = Shader.PropertyToID("_TempTex");
                //cmd.GetTemporaryRT(tempRenderID, cameraTextureDescriptor, FilterMode.Trilinear);

                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;
                int rtW = desc.width;
                int rtH = desc.height;
                int xres = (int)(rtW / ((float)1));
                int yres = (int)(rtH / ((float)1));
                if (_handleA == null || _handleA.rt.width != xres || _handleA.rt.height != yres)
                {
                    //_handleA = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D);
                    _handleA = RTHandles.Alloc(Mathf.CeilToInt(downScaledX), Mathf.CeilToInt(downScaledY), colorFormat: GraphicsFormat.R32G32B32A32_SFloat,
                        dimension: TextureDimension.Tex2D);
                }
                tmpBuffer2A = renderGraph.ImportTexture(_handleA);//reflectionMapID                            

                if (_handleTAART == null || _handleTAART.rt.width != xres || _handleTAART.rt.height != yres || _handleTAART.rt.useMipMap == false)
                {
                    //_handleTAART.rt.DiscardContents();
                    //_handleTAART.rt.useMipMap = true;// = 8;
                    //_handleTAART.rt.autoGenerateMips = true;                       
                    _handleTAART = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D,
                        useMipMap: true, autoGenerateMips: true
                        );
                    _handleTAART.rt.wrapMode = TextureWrapMode.Clamp;
                    _handleTAART.rt.filterMode = FilterMode.Trilinear;
                    //Debug.Log(_handleTAART.rt.mipmapCount);
                }
                _handleTAA = renderGraph.ImportTexture(_handleTAART); //_TempTex

                if (_handleTAART2 == null || _handleTAART2.rt.width != xres || _handleTAART2.rt.height != yres || _handleTAART2.rt.useMipMap == false)
                {
                    _handleTAART2 = RTHandles.Alloc(xres, yres, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, dimension: TextureDimension.Tex2D,
                        useMipMap: true, autoGenerateMips: true
                        );
                    _handleTAART2.rt.wrapMode = TextureWrapMode.Clamp;
                    _handleTAART2.rt.filterMode = FilterMode.Trilinear;
                }
                _handleTAA2 = renderGraph.ImportTexture(_handleTAART2); //_TempTex

                tmpBuffer1A = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "tmpBuffer1A", true);
                tmpBuffer1Aa = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "tmpBuffer1Aa", false);
                TextureHandle sourceTexture = resourceData.activeColorTexture;

                ////////////////  SUN SHAFTS  ////////////////////////////////////////////////////////////////////////////////
                Material sheetSHAFTS = blitMaterial;
                sheetSHAFTS.SetFloat("_Blend", blend);
                Camera camera = Camera.main;
                if (useDepthTexture)
                {
                    camera.depthTextureMode |= DepthTextureMode.Depth;
                }
                Vector3 v = Vector3.one * 0.5f;
                if (sunTransform != Vector3.zero)
                {
                    v = Camera.main.WorldToViewportPoint(sunTransform);// - Camera.main.transform.position;
                }
                else
                {
                    v = new Vector3(0.5f, 0.5f, 0.0f);
                }
                //v0.1
                //int rtW = desc.width;
                // int rtH = desc.height;

                var formatA = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                                                                                                              //         RenderTexture m_TemporaryColorTexture = RenderTexture.GetTemporary(desc.width, desc.height, 0, formatA);
                                                                                                              //         RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(desc.width, desc.height, 0, formatA);

                //         cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode);
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
                sheetSHAFTS.SetVector("_SunThreshold", sunThreshold);



                //Debug.Log("IN1");
                //             cmd.Blit(source, m_TemporaryColorTexture); //KEEP BACKGROUND
                string passNameA = "DO 2";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameA, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer1A, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    ExecuteBlitPassCLEAR(data, context, 14 + offset, passData.src));// 
                    //   ExecuteBlitPassCLEAR(data, context, 14 + offset, passData.src));// 
                    //ExecuteBlitPass(data, context, 7, passData.src));
                }




                passNameA = "DO 2a";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameA, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer2A, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    //   ExecuteBlitPassCLEAR(data, context, 14, passData.src));// 
                    ExecuteBlitPass(data, context, 7 + offset, passData.src));
                }



                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(tmpBuffer1A, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(sourceTexture, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}
                //return;

                if (!useDepthTexture)
                {
                    var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                                                                                                                 //              RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                                                                                                                 //               RenderTexture.active = tmpBuffer1A;
                                                                                                                 ///               GL.ClearWithSkybox(false, camera);
                    //                sheetSHAFTS.SetTexture("_Skybox", tmpBuffer1A);

                    //             cmd.Blit(source, lrDepthBuffer, sheetSHAFTS, 3);
                    string passNameAA = "DO 1";
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAA, out var passData))
                    {
                        passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        builder.UseTexture(passData.src, AccessFlags.Read);
                        builder.UseTexture(tmpBuffer1A, AccessFlags.Read);
                        builder.SetRenderAttachment(_handleTAA2, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        passData.BlitMaterial = sheetSHAFTS;
                        builder.AllowGlobalStateModification(true);
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPassTWO2(data, context, 3 + 8 + offset, passData.src, tmpBuffer1A));
                    }
                    //             RenderTexture.ReleaseTemporary(tmpBuffer);
                }
                else
                {
                    //               cmd.Blit(source, lrDepthBuffer, sheetSHAFTS, 2);
                    string passNameAAA = "DO 1";
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAAA, out var passData))
                    {
                        passData.src = resourceData.activeColorTexture;
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        builder.UseTexture(passData.src, AccessFlags.Read);
                        builder.UseTexture(tmpBuffer1A, AccessFlags.Read);
                        builder.SetRenderAttachment(_handleTAA2, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        passData.BlitMaterial = sheetSHAFTS;
                        builder.AllowGlobalStateModification(true);
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPassTWO2(data, context, 2 + 8 + offset, passData.src, tmpBuffer1A));
                    }
                }

                //// lrDepthBuffer == _handleTAA2
                //// m_TemporaryColorTexture = _handleTAA

                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(tmpBuffer1A, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(sourceTexture, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}
                //return;

                radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);

                float ofs = sunShaftBlurRadius * (1.0f / 768.0f);
                // Debug.Log(ofs);
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

                float adjustX = 0.5f;
                if (v.x < 0.5f)
                {
                    float diff = 0.5f - v.x;
                    adjustX = adjustX - 0.5f * diff;
                }
                float adjustY = 0.5f;
                if (v.y > 1.25f)
                {
                    float diff2 = v.y - 1.25f;
                    adjustY = adjustY - 0.3f * diff2;
                }
                if (v.y > 1.8f)
                {
                    v.y = 1.8f;
                    float diff3 = v.y - 1.25f;
                    adjustY = 0.5f - 0.3f * diff3;
                }

                sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x * 0.5f + adjustX, v.y * 0.5f + adjustY, v.z, maxRadius));

                // lrColorB == _handleTAART
                //// lrDepthBuffer == _handleTAA2
                //// m_TemporaryColorTexture = _handleTAA



                //TEST2                
                for (int it2 = 0; it2 < radialBlurIterations; it2++)
                {
                    //                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
                    //                   cmd.Blit(lrDepthBuffer, lrColorB, sheetSHAFTS, 1);//Blit(cmd, lrDepthBuffer, lrColorB, sheetSHAFTS, 1); //Blit(cmd, lrDepthBuffer.Identifier(), lrColorB, sheetSHAFTS, 1);//v0.1
                    //                cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name));//  lrDepthBuffer.id);//v0.1
                    string passName = "SAVE TEMP" + it2;
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    {
                        //passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                        passData.src = resourceData.activeColorTexture;
                        desc.msaaSamples = 1; desc.depthBufferBits = 0;
                        //builder.UseTexture(passData.src, IBaseRenderGraphBuilder.AccessFlags.Read);
                        builder.UseTexture(_handleTAA2, AccessFlags.Read);
                        builder.SetRenderAttachment(_handleTAA, 0, AccessFlags.Write);
                        builder.AllowPassCulling(false);
                        //builder.AllowGlobalStateModification(true);
                        passData.BlitMaterial = sheetSHAFTS;
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                            ExecuteBlitPassA(data, context, 1 + 8 + offset, _handleTAA2));
                    }



                    //passNameA = "DO 2+it2";
                    //using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameA, out var passData))
                    //{
                    //    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    //    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    //    builder.UseTexture(_handleTAA, IBaseRenderGraphBuilder.AccessFlags.Read);
                    //    builder.SetRenderAttachment(tmpBuffer1Aa, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                    //    builder.AllowPassCulling(false);
                    //    passData.BlitMaterial = sheetSHAFTS;
                    //    builder.AllowGlobalStateModification(true);
                    //    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    //        ExecuteBlitPass(data, context, 14, _handleTAA));
                    //}
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve1", out var passData, m_ProfilingSampler))
                    {
                        passData.BlitMaterial = sheetSHAFTS;
                        // Similar to the previous pass, however now we set destination texture as input and source as output.
                        builder.UseTexture(_handleTAA, AccessFlags.Read);
                        passData.src = _handleTAA;
                        builder.SetRenderAttachment(_handleTAA2, 0, AccessFlags.Write);
                        builder.AllowGlobalStateModification(true);
                        // We use the same BlitTexture API to perform the Blit operation.
                        builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                    }





                    ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                    sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
                    // cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode);   //    lrDepthBuffer.id, opaqueDesc, filterMode);   //v0.1 
                    // cmd.Blit(lrColorB, lrDepthBuffer, sheetSHAFTS, 1); //Blit(cmd, lrColorB, lrDepthBuffer.Identifier(), sheetSHAFTS, 1);//v0.1
                    // RenderTexture.ReleaseTemporary(lrColorB);  //v0.1
                    // passName = "SAVE TEMP 2" + it2;
                    //using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                    //{
                    //    //passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    //    passData.src = resourceData.activeColorTexture;
                    //    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    //    //builder.UseTexture(passData.src, IBaseRenderGraphBuilder.AccessFlags.Read);
                    //    builder.UseTexture(tmpBuffer1Aa, IBaseRenderGraphBuilder.AccessFlags.Read);
                    //    builder.SetRenderAttachment(_handleTAA2, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                    //    builder.AllowPassCulling(false);
                    //    //builder.AllowGlobalStateModification(true);
                    //    passData.BlitMaterial = sheetSHAFTS;
                    //    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    //        ExecuteBlitPassA(data, context, 1+8, tmpBuffer1Aa));
                    //}



                    ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                    sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
                }

                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve1", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(_handleTAA2, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}
                //return;

                ////cmd.Blit(temp1, renderingData.cameraData.renderer.cameraColorTargetHandle); //v0.1
                //using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                //{
                //    passData.BlitMaterial = sheetSHAFTS;
                //    // Similar to the previous pass, however now we set destination texture as input and source as output.
                //    passData.src = builder.UseTexture(_handleTAA2, IBaseRenderGraphBuilder.AccessFlags.Read);
                //    builder.SetRenderAttachment(sourceTexture, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                //    builder.AllowGlobalStateModification(true);
                //    // We use the same BlitTexture API to perform the Blit operation.
                //    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                //}
                //return;////


                if (v.z >= 0.0f)
                {
                    sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
                }
                else
                {
                    sheetSHAFTS.SetVector("_SunColor", Vector4.zero); // no backprojection !
                }
                //       cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer);
                //       cmd.Blit(m_TemporaryColorTexture, source, sheetSHAFTS, (screenBlendMode == BlitSunShaftsSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);

                string passNameAAa = "DO 1aa";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passNameAAa, out var passData))
                {
                    passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    builder.UseTexture(passData.src, AccessFlags.Read);
                    builder.UseTexture(_handleTAA2, AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer2A, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecuteBlitPassTWO2a(data, context, 0 + 8 + offset, passData.src, _handleTAA2));
                }

                //BLIT FINAL
                //cmd.Blit(temp1, renderingData.cameraData.renderer.cameraColorTargetHandle); //v0.1
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Color Blit Resolve", out var passData, m_ProfilingSampler))
                {
                    passData.BlitMaterial = sheetSHAFTS;
                    // Similar to the previous pass, however now we set destination texture as input and source as output.
                    builder.UseTexture(tmpBuffer2A, AccessFlags.Read);
                    passData.src = tmpBuffer2A;
                    builder.SetRenderAttachment(sourceTexture, 0, AccessFlags.Write);
                    builder.AllowGlobalStateModification(true);
                    // We use the same BlitTexture API to perform the Blit operation.
                    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
                }
                // this.prevViewProjectionMatrix = cameraData.camera.nonJitteredProjectionMatrix * cameraData.camera.worldToCameraMatrix;
                //  cameraData.camera.ResetProjectionMatrix();

                /*
                passName = "SAVE TEMP2";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    //passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    passData.src = resourceData.activeColorTexture;
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    //builder.UseTexture(passData.src, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.UseTexture(_handleTAA, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.SetRenderAttachment(tmpBuffer1A, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecuteBlitPass(data, context, 2, _handleTAA));
                }
                passName = "SAVE TEMP";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    //passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    passData.src = resourceData.activeColorTexture;
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    //builder.UseTexture(passData.src, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.UseTexture(_handleTAA2, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.SetRenderAttachment(_handleTAA, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecuteBlitPass(data, context, 2, _handleTAA2));
                }
                passName = "SAVE TEMP";
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    //passData.src = resourceData.activeColorTexture; //SOURCE TEXTURE
                    passData.src = resourceData.activeColorTexture;
                    desc.msaaSamples = 1; desc.depthBufferBits = 0;
                    //builder.UseTexture(passData.src, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.UseTexture(tmpBuffer1A, IBaseRenderGraphBuilder.AccessFlags.Read);
                    builder.SetRenderAttachment(_handleTAA2, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    builder.AllowGlobalStateModification(true);
                    passData.BlitMaterial = sheetSHAFTS;
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        ExecuteBlitPass(data, context, 2, tmpBuffer1A));
                }
                */
                // END PING PONG


                //RESET CAMERA
                //cameraData.camera.ResetWorldToCameraMatrix();
                //cameraData.camera.ResetProjectionMatrix();

                //cameraData.camera.nonJitteredProjectionMatrix = cameraData.camera.projectionMatrix;

                //Matrix4x4 p = cameraData.camera.projectionMatrix;
                //float2 jitter = (float2)(2 * Halton2364Seq[Time.frameCount % HaltonLength] - 1) * JitterSpread;
                //p.m02 = jitter.x / (float)Screen.width;
                //p.m12 = jitter.y / (float)Screen.height;
                //cameraData.camera.projectionMatrix = p;

            }

        }//END SUN SHAFTS ONLY GRAPH

        static void ExecuteBlitPassTEX9NAME(PassData data, RasterGraphContext context, int pass,
     string texname1, TextureHandle tmpBuffer1,
     string texname2, TextureHandle tmpBuffer2,
     string texname3, TextureHandle tmpBuffer3,
     string texname4, TextureHandle tmpBuffer4,
     string texname5, TextureHandle tmpBuffer5,
     string texname6, TextureHandle tmpBuffer6,
     string texname7, TextureHandle tmpBuffer7,
     string texname8, TextureHandle tmpBuffer8,
     string texname9, TextureHandle tmpBuffer9,
     string texname10, TextureHandle tmpBuffer10
     )
        {
            data.BlitMaterial.SetTexture(texname1, tmpBuffer1);
            data.BlitMaterial.SetTexture(texname2, tmpBuffer2);
            data.BlitMaterial.SetTexture(texname3, tmpBuffer3);
            data.BlitMaterial.SetTexture(texname4, tmpBuffer4);
            data.BlitMaterial.SetTexture(texname5, tmpBuffer5);
            data.BlitMaterial.SetTexture(texname6, tmpBuffer6);
            data.BlitMaterial.SetTexture(texname7, tmpBuffer7);
            data.BlitMaterial.SetTexture(texname8, tmpBuffer8);
            data.BlitMaterial.SetTexture(texname9, tmpBuffer9);
            data.BlitMaterial.SetTexture(texname10, tmpBuffer10);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        //temporal
        static void ExecuteBlitPassTEN(PassData data, RasterGraphContext context, int pass,
            TextureHandle tmpBuffer1, TextureHandle tmpBuffer2, TextureHandle tmpBuffer3,
            string varname1, float var1,
            string varname2, float var2,
            string varname3, Matrix4x4 var3,
            string varname4, Matrix4x4 var4,
            string varname5, Matrix4x4 var5,
            string varname6, Matrix4x4 var6,
            string varname7, Matrix4x4 var7
            )
        {
            data.BlitMaterial.SetTexture("_CloudTex", tmpBuffer1);
            data.BlitMaterial.SetTexture("_PreviousColor", tmpBuffer2);
            data.BlitMaterial.SetTexture("_PreviousDepth", tmpBuffer3);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
            //lastFrameViewProjectionMatrix = viewProjectionMatrix;
            //lastFrameInverseViewProjectionMatrix = viewProjectionMatrix.inverse;
        }

        static void ExecuteBlitPassTHREE(PassData data, RasterGraphContext context, int pass,
            TextureHandle tmpBuffer1, TextureHandle tmpBuffer2, TextureHandle tmpBuffer3)
        {
            data.BlitMaterial.SetTexture("_ColorBuffer", tmpBuffer1);
            data.BlitMaterial.SetTexture("_PreviousColor", tmpBuffer2);
            data.BlitMaterial.SetTexture("_PreviousDepth", tmpBuffer3);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        static void ExecuteBlitPass(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1aa)
        {
            data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1aa);
            if (data.BlitMaterial == null)
            {
                Debug.Log("data.BlitMaterial == null");
            }

            Blitter.BlitTexture(context.cmd,
                data.src,
                new Vector4(1, 1, 0, 0),
                data.BlitMaterial,
                pass);
        }

        static void ExecuteBlitPassCLEAR(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1aa)
        {
            //context.cmd.ClearRenderTarget(true, true, Color.clear);
            //GL.ClearWithSkybox(false, Camera.main);
            //RenderTexture.active = context.cmd.ta;

            //context.cmd.
            //context.cmd.

            //data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1aa);
            //if (data.BlitMaterial == null)
            //{
            //    Debug.Log("data.BlitMaterial == null");
            //}

            Blitter.BlitTexture(context.cmd,
                data.src,
                new Vector4(1, 1, 0, 0),
                data.BlitMaterial,
                pass);
            //RenderTexture.active = data.src;
            //GL.ClearWithSkybox(false, Camera.main);
        }

        static void ExecuteBlitPassA(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1aa)
        {
            data.BlitMaterial.SetTexture("_MainTexA", tmpBuffer1aa);
            if (data.BlitMaterial == null)
            {
                Debug.Log("data.BlitMaterial == null");
            }

            Blitter.BlitTexture(context.cmd,
                data.src,
                new Vector4(1, 1, 0, 0),
                data.BlitMaterial,
                pass);
        }
        static void ExecuteBlitPassNOTEX(PassData data, RasterGraphContext context, int pass, UniversalCameraData cameraData)
        {
            //Matrix4x4 projectionMatrix = cameraData.GetGPUProjectionMatrix(0);



            // data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1aa);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }

        static void ExecuteBlitPassTWO2(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle tmpBuffer2)
        {
            data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1);// _CloudTexP", tmpBuffer1);
            data.BlitMaterial.SetTexture("_Skybox", tmpBuffer2);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        static void ExecuteBlitPassTWO2a(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle tmpBuffer2)
        {
            data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1);// _CloudTexP", tmpBuffer1);
            data.BlitMaterial.SetTexture("_ColorBuffer", tmpBuffer2);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        static void ExecuteBlitPassTWO2c(PassData data, RasterGraphContext context, int pass)
        {
           // data.BlitMaterial.SetTexture("_InputTextureA", tmpBuffer1);// _CloudTexP", tmpBuffer1);           
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        //static void ExecuteBlitPassTWO2b(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle MaskMapA)//, TextureHandle tmpBuffer3)
        //{
        //    //data.BlitMaterial.SetTexture("_MainTex", data.src);// _CloudTexP", tmpBuffer1);
        //    data.BlitMaterial.SetTexture("_SourceTex", data.src);
        //    data.BlitMaterial.SetTexture("_MainTexB", tmpBuffer1);
        //    //data.BlitMaterial.SetTexture("_WaterInterfaceTex", tmpBuffer2);
        //    //data.BlitMaterial.SetTexture("_BumpTex", BumpMap);
        //    //data.BlitMaterial.SetTexture("_SourceTex", tmpBuffer3);
        //    data.BlitMaterial.SetTexture("_WaterInterfaceTex", MaskMapA);
        //    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        //}
        static void ExecuteBlitPassTWO2b(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, 
        TextureHandle MaskMapA, RenderTexture mask, Texture2D BumpMap, float cutoffHeigth)//, TextureHandle tmpBuffer3)
        {
            //data.BlitMaterial.SetTexture("_MainTex", data.src);// _CloudTexP", tmpBuffer1);
            data.BlitMaterial.SetTexture("_SourceTex", data.src);
            data.BlitMaterial.SetTexture("_MainTexB", tmpBuffer1);
            //data.BlitMaterial.SetTexture("_WaterInterfaceTex", tmpBuffer2);
            data.BlitMaterial.SetTexture("OceanisMaskBumpGlob", BumpMap);
            //data.BlitMaterial.SetTexture("_SourceTex", tmpBuffer3);

            /////data.BlitMaterial.SetTexture("_WaterInterfaceTex", mask);
            if (Camera.main.transform.position.y < -cutoffHeigth)
            {
                data.BlitMaterial.SetTexture("_WaterInterfaceTex", MaskMapA);// mask);
            }
            else
            {
                data.BlitMaterial.SetTexture("_WaterInterfaceTex", mask);// mask);
            }


            data.BlitMaterial.SetTexture("_InputTextureA", mask); //MaskMap
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }

        static void ExecuteBlitPassTWO(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle tmpBuffer2)
        {
            data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1);// _CloudTexP", tmpBuffer1);
            data.BlitMaterial.SetTexture("_TemporalAATexture", tmpBuffer2);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        static void ExecuteBlitPassTWO_MATRIX(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1, TextureHandle tmpBuffer2, Matrix4x4 matrix)
        {
            data.BlitMaterial.SetTexture("_MainTex", tmpBuffer1);// _CloudTexP", tmpBuffer1);
            data.BlitMaterial.SetTexture("_CameraDepthCustom", tmpBuffer2);
            data.BlitMaterial.SetMatrix("frustumCorners", matrix);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        static void ExecuteBlitPassTEXNAME(PassData data, RasterGraphContext context, int pass, TextureHandle tmpBuffer1aa, string texname)
        {
            data.BlitMaterial.SetTexture(texname, tmpBuffer1aa);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        static void ExecuteBlitPassTEX5NAME(PassData data, RasterGraphContext context, int pass,
            string texname1, TextureHandle tmpBuffer1,
            string texname2, TextureHandle tmpBuffer2,
            string texname3, TextureHandle tmpBuffer3,
            string texname4, TextureHandle tmpBuffer4,
            string texname5, TextureHandle tmpBuffer5
            )
        {
            data.BlitMaterial.SetTexture(texname1, tmpBuffer1);
            data.BlitMaterial.SetTexture(texname2, tmpBuffer2);
            data.BlitMaterial.SetTexture(texname3, tmpBuffer3);
            data.BlitMaterial.SetTexture(texname4, tmpBuffer4);
            data.BlitMaterial.SetTexture(texname5, tmpBuffer5);
            Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, pass);
        }
        // It is static to avoid using member variables which could cause unintended behaviour.
        static void ExecutePass(PassData data, RasterGraphContext rgContext)
        {
            Blitter.BlitTexture(rgContext.cmd, data.src, new Vector4(1, 1, 0, 0), data.BlitMaterial, 12);
        }
        //private Material m_BlitMaterial;
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("After Opaques");
        ////// END GRAPH
#endif






        //v0.4  - Unity 2020.1
#if UNITY_2020_2_OR_NEWER
        public BlitSunShaftsWaterSRP.BlitSettings settings;

#if UNITY_2022_1_OR_NEWER
        RTHandle _handle;//v0.1
#else
        RenderTargetHandle _handle;//v0.1
#endif


#if UNITY_6000_4_OR_NEWER
        //DO NOTHING
#else
        public override void OnCameraSetup(CommandBuffer cmd, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {

            var renderer = renderingData.cameraData.renderer;

            //v0.1
            //_handle.Init(settings.textureId);



#if UNITY_2022_1_OR_NEWER
             _handle = RTHandles.Alloc(settings.textureId, name: settings.textureId); //v0.1
            destination = (settings.destination == BlitSunShaftsWaterSRP.Target.Color)
                ? renderingData.cameraData.renderer.cameraColorTargetHandle //UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget //v0.1
                : _handle;          
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;  //renderer.cameraColorTarget; //v0.1
#else
            _handle.Init(settings.textureId);
            destination = (settings.destination == BlitSunShaftsWaterSRP.Target.Color)
               ? UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget //v0.1
               : _handle;
            source = renderer.cameraColorTarget; //v0.1
#endif

        }
#endif
#endif


        public bool enableShafts = true;
        //SUN SHAFTS         
        public BlitSunShaftsWaterSRP.BlitSettings.SunShaftsResolution resolution = BlitSunShaftsWaterSRP.BlitSettings.SunShaftsResolution.Normal;
        public BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode.Screen;
        public Vector3 sunTransform = new Vector3(0f, 0f, 0f); // Transform sunTransform;

        //v0.1
        public bool autoRegulateEffect = false;
        public float cutoffHeigth = 0; //cutoff the effect above this height

        public int radialBlurIterations = 2;
        public Color sunColor = Color.white;
        public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        public float sunShaftBlurRadius = 2.5f;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
        public float blend = 0.5f;


        //v4.8
        [Tooltip("Controls the intensity of the effect.")]
        public float intensity = 0;
        public bool shaftsFollowCamera = false;
        //v1.2
        public bool useWaterAirMask =true;
        public bool resetScaling = false;
        //v1.2
        public bool useVolumeMaskRenderer =false;        
        public RenderTexture MaskMap;
        public bool useTexture2DMask = false;
        public Texture2D MaskMapA;
        //MASK
        public float waterHeight = 0;
        /////REFRACT LINE
        public Texture2D BumpMap;
        public float underwaterDepthFade = 1.1f; //put above 1 to fade faster towards the surface - TO DO: Regulate based on player distance to surface as well
        public float BumpIntensity = 0.02f;
        public Vector4 BumpVelocity = new Vector4(0.15f, 0.10f, 0.2f, 0.3f);
        public float BumpScale = 0.1f;
        //v4.6
        public Color underWaterTint = Color.cyan;
        public float BumpIntensityRL = 0.17f;
        public float BumpScaleRL = 0.4f;
        public float BumpLineHeight = 0.1f;
        public float refractLineWidth = 1; //v4.6
        public float refractLineXDisp = 1; //v4.6
        public float refractLineXDispA = 1; //v4.6
        public Vector4 refractLineFade =new Vector4(1, 1, 1, 1);
        public float scalerMask = 0.2f;
        /////END REFRACT LINE




        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }

        public Material blitMaterial = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RenderTargetIdentifier source { get; set; }

#if UNITY_2022_1_OR_NEWER
        private RTHandle destination { get; set; }//UnityEngine.Rendering.Universal.RenderTargetHandle destination { get; set; } //v0.1
#else
        UnityEngine.Rendering.Universal.RenderTargetHandle destination { get; set; } //v0.1
#endif


        
        string m_ProfilerTag;


        //SUN SHAFTS
        RenderTexture lrColorB;
        // RenderTexture lrDepthBuffer;
        // RenderTargetHandle lrColorB;


        //RTHandle m_TemporaryColorTexture;//v0.1
        //RTHandle lrDepthBuffer; //v0.1
        ////v4.8
        //public RTHandle m_TemporaryColorTextureScaled;  //v0.1 
       


        //RTHandle lrDepthBuffer;
        //RTHandle m_TemporaryColorTexture;
        //public RTHandle m_TemporaryColorTextureScaled;
        //RTHandle lrColorB;

        /// <summary>
        /// Create the CopyColorPass
        /// </summary>
        public BlitPassSunShaftsWaterSRP(UnityEngine.Rendering.Universal.RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag, BlitSunShaftsWaterSRP.BlitSettings settings)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;
            
            
            //m_TemporaryColorTexture.Init("_TemporaryColorTexture"); //v0.1
    //        m_TemporaryColorTexture = RTHandles.Alloc("_TemporaryColorTexture", name: "_TemporaryColorTexture"); //v0.1
    //        m_TemporaryColorTextureScaled = RTHandles.Alloc("m_TemporaryColorTextureScaled", name: "m_TemporaryColorTextureScaled"); //v0.1
    //        lrDepthBuffer = RTHandles.Alloc("lrDepthBuffer", name: "lrDepthBuffer"); //v0.1

            //SUN SHAFTS
            this.resolution = settings.resolution;
            this.screenBlendMode = settings.screenBlendMode;
            this.sunTransform = settings.sunTransform;
            this.radialBlurIterations = settings.radialBlurIterations;
            this.sunColor = settings.sunColor;
            this.sunThreshold = settings.sunThreshold;
            this.sunShaftBlurRadius = settings.sunShaftBlurRadius;
            this.sunShaftIntensity = settings.sunShaftIntensity;
            this.maxRadius = settings.maxRadius;
            this.useDepthTexture = settings.useDepthTexture;
            this.blend = settings.blend;


            //v4.8
            intensity = settings.intensity;
            shaftsFollowCamera = settings.shaftsFollowCamera;
            useWaterAirMask = settings.useWaterAirMask;
            resetScaling = settings.resetScaling;
            useVolumeMaskRenderer = settings.useVolumeMaskRenderer;
            useTexture2DMask = settings.useTexture2DMask;
            MaskMap = settings.MaskMap;
            MaskMapA = settings.MaskMapA;
            waterHeight = settings.waterHeight;
            BumpMap = settings.BumpMap;
            underwaterDepthFade = settings.underwaterDepthFade;
            BumpIntensity = settings.BumpIntensity;
            BumpVelocity = settings.BumpVelocity;
            BumpScale = settings.BumpScale;
            underWaterTint = settings.underWaterTint;
            BumpIntensityRL = settings.BumpIntensityRL;
            BumpScaleRL = settings.BumpScaleRL;
            BumpLineHeight = settings.BumpLineHeight;
            refractLineWidth = settings.refractLineWidth;
            refractLineXDisp = settings.refractLineXDisp;
            refractLineXDispA = settings.refractLineXDispA;
            refractLineFade = settings.refractLineFade;
            scalerMask = settings.scalerMask;


        }

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Target</param>

#if UNITY_2022_1_OR_NEWER
 public void Setup(RenderTargetIdentifier source, RTHandle destination) //v0.1
        {
            this.source = source;
            this.destination = destination;
        }
#else
        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination) //v0.1
        {
            this.source = source;
            this.destination = destination;
        }
#endif



        connectSuntoSunShaftsWaterURP connector;

#if UNITY_6000_4_OR_NEWER
        //DO NOTHING
#else
        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {            
            //grab settings if script on scene camera
            if (connector == null)
            {
                connector = renderingData.cameraData.camera.GetComponent<connectSuntoSunShaftsWaterURP>();
                if(connector == null && Camera.main != null)
                {
                    connector = Camera.main.GetComponent<connectSuntoSunShaftsWaterURP>();                    
                }                
            }
            //Debug.Log(Camera.main.GetComponent<connectSuntoSunShaftsURP>().sun.transform.position);
            if (connector != null)
            {
                this.enableShafts = connector.enableShafts;
                this.sunTransform = connector.sun.transform.position;
                this.screenBlendMode = connector.screenBlendMode;
                //public Vector3 sunTransform = new Vector3(0f, 0f, 0f);

                this.autoRegulateEffect = connector.autoRegulateEffect;
                this.cutoffHeigth = connector.cutoffHeigth;


                this.radialBlurIterations = connector.radialBlurIterations;
                this.sunColor = connector.sunColor;
                this.sunThreshold = connector.sunThreshold;
                this.sunShaftBlurRadius = connector.sunShaftBlurRadius;
                this.sunShaftIntensity = connector.sunShaftIntensity;
                this.maxRadius = connector.maxRadius;
                this.useDepthTexture = connector.useDepthTexture;



                //v4.8
                intensity = connector.intensity;
                shaftsFollowCamera = connector.shaftsFollowCamera;
                useWaterAirMask = connector.useWaterAirMask;
                resetScaling = connector.resetScaling;
                useVolumeMaskRenderer = connector.useVolumeMaskRenderer;
                useTexture2DMask = connector.useTexture2DMask;
                MaskMap = connector.MaskMap;
                MaskMapA = connector.MaskMapA;
                waterHeight = connector.waterHeight;
                BumpMap = connector.BumpMap;
                underwaterDepthFade = connector.underwaterDepthFade;
                BumpIntensity = connector.BumpIntensity;
                BumpVelocity = connector.BumpVelocity;
                BumpScale = connector.BumpScale;
                underWaterTint = connector.underWaterTint;
                BumpIntensityRL = connector.BumpIntensityRL;
                BumpScaleRL = connector.BumpScaleRL;
                BumpLineHeight = connector.BumpLineHeight;
                refractLineWidth = connector.refractLineWidth;
                refractLineXDisp = connector.refractLineXDisp;
                refractLineXDispA = connector.refractLineXDispA;
                refractLineFade = connector.refractLineFade;
                scalerMask = connector.scalerMask;
            }

            //if still null, disable effect
            bool connectorFound = true;
            if (connector == null)
            {
                connectorFound = false;
            }

            if (enableShafts && connectorFound &&  (!connector.autoRegulateEffect || (connector.autoRegulateEffect && Camera.main.transform.position.y < connector.cutoffHeigth) ) )
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;

#if UNITY_2022_1_OR_NEWER
                // Can't read and write to same color target, create a temp render target to blit. 
                if (destination == renderingData.cameraData.renderer.cameraColorTargetHandle) //v0.1
                {
#else
                // Can't read and write to same color target, create a temp render target to blit. 
                if (destination == RenderTargetHandle.CameraTarget) //v0.1
                {
#endif

          //          cmd.GetTemporaryRT(Shader.PropertyToID(m_TemporaryColorTexture.name), opaqueDesc, filterMode); //v0.1
          //          cmd.GetTemporaryRT(Shader.PropertyToID(m_TemporaryColorTextureScaled.name), opaqueDesc, filterMode); //v4.8 //v0.1
                    
                    //Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, 0);// blitShaderPassIndex);
                    //Blit(cmd, m_TemporaryColorTexture.Identifier(), source);

                    ////blitMaterial.SetFloat("_Delta",100);
                    //Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, 0);// blitShaderPassIndex);
                    //Blit(cmd, m_TemporaryColorTexture.Identifier(), source);

                    //RenderShafts(context, renderingData, cmd, opaqueDesc);
                    RenderMaskedShafts(context, renderingData, cmd, opaqueDesc);
                }
                else
                {
                    //Blit(cmd, source, destination.Identifier(), blitMaterial, blitShaderPassIndex);
                }

                // RenderShafts(context, renderingData);
                //Camera camera = Camera.main;
                //cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitMaterial);
                //cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

                //context.ExecuteCommandBuffer(cmd);
                // CommandBufferPool.Release(cmd);
            }
        }
#endif
        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            //if (destination == renderingData.cameraData.renderer.cameraColorTargetHandle) //v0.1
            //{
                //cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
                //cmd.ReleaseTemporaryRT(m_TemporaryColorTextureScaled.id); //v4.8               
                //cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
            //}
        }

        //RenderTexture lrColorBACK;
        //RenderTargetHandle lrColorBACK;

        //SUN SHAFTS
        /*
        public void RenderShafts(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        {
            opaqueDesc.depthBufferBits = 0;
            Material sheetSHAFTS = blitMaterial;
            sheetSHAFTS.SetFloat("_Blend", blend);

            Camera camera = Camera.main;
            // we actually need to check this every frame
            if (useDepthTexture)
            {               
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }           
            Vector3 v = Vector3.one * 0.5f;         
            if (sunTransform != Vector3.zero) {              
                v = Camera.main.WorldToViewportPoint(sunTransform);// - Camera.main.transform.position;
            }
            else {
                v = new Vector3(0.5f, 0.5f, 0.0f);
            }
            //v0.1
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;
            cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode);//v0.1
            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);        
            sheetSHAFTS.SetVector("_SunThreshold", sunThreshold);

            //v4.8
           // var formatA = RenderTextureFormat.DefaultHDR ; //v3.4.9
            //RenderTexture tmpBufferA = RenderTexture.GetTemporary(rtW, rtH, 0, formatA);
            //Blit(cmd, source, tmpBufferA);           

            if (!useDepthTexture)
            {               
                var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                RenderTexture.active = tmpBuffer;
                GL.ClearWithSkybox(false, camera);             
                sheetSHAFTS.SetTexture("_Skybox", tmpBuffer);

                //v4.8
               // sheetSHAFTS.SetTexture("_MainTexA", tmpBufferA);//v4.8

		//v0.1
                cmd.Blit( source, lrDepthBuffer, sheetSHAFTS, 3);
                RenderTexture.ReleaseTemporary(tmpBuffer);              
            }
            else
            {              
                //v4.8
               // sheetSHAFTS.SetTexture("_MainTexA", tmpBufferA);//v4.8

                //v0.1
                cmd.Blit( source, lrDepthBuffer, sheetSHAFTS, 2);
            }          

            //v4.8
            //sheetSHAFTS.SetTexture("_MainTexA", tmpBufferA);//v4.8

            //v0.1
            cmd.Blit( source, m_TemporaryColorTexture); //KEEP BACKGROUND           
            radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);

            float ofs = sunShaftBlurRadius * (1.0f / 768.0f);
            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

            float adjustX = 0.5f;
            if (v.x < 0.5f) {               
                float diff = 0.5f - v.x; adjustX = adjustX - 0.5f * diff;
            }
            float adjustY = 0.5f;
            if (v.y > 1.25f)
            {             
                float diff2 = v.y - 1.25f; adjustY = adjustY - 0.3f * diff2;
            }
            if (v.y > 1.8f)
            {             
                v.y = 1.8f;
                float diff3 = v.y - 1.25f; adjustY = 0.5f - 0.3f * diff3;
            }

            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x * 0.5f + adjustX, v.y * 0.5f + adjustY, v.z, maxRadius));           
           
            for (int it2 = 0; it2 < radialBlurIterations; it2++)
            {
               lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);

                //v4.8
                //sheetSHAFTS.SetTexture("_MainTexA", tmpBufferA);//v4.8

                //v0.1
                cmd.Blit( lrDepthBuffer, lrColorB, sheetSHAFTS, 1);

               cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name));//v0.1
               ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
               sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
               cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode);//v0.1

                //v4.8
                //sheetSHAFTS.SetTexture("_MainTexA", lrColorB);//v4.8


                //v0.1
                cmd.Blit( lrColorB, lrDepthBuffer, sheetSHAFTS, 1);

               RenderTexture.ReleaseTemporary(lrColorB);
               ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;               
               sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }

            if (v.z >= 0.0f)
            {
                sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
            }
            else
            {
                sheetSHAFTS.SetVector("_SunColor", Vector4.zero);
            }
            cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer);           

            //v4.8
            //sheetSHAFTS.SetTexture("_MainTexA", tmpBufferA);//v4.8

            //v0.1
            cmd.Blit( m_TemporaryColorTexture, source, sheetSHAFTS, (screenBlendMode == BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
          
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name));//v0.1
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_TemporaryColorTexture.name));//v0.1
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            //v4.8
           // RenderTexture.ReleaseTemporary(tmpBufferA);            
            RenderTexture.ReleaseTemporary(lrColorB);
        }
        */

        //MASKED SHAFTS
        public void RenderMaskedShafts(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        {

            //GL.invertCulling = false;


            opaqueDesc.depthBufferBits = 0;
            Material sheetSHAFTS = blitMaterial;

            //v4.8
            if (sheetSHAFTS == null)
                return;

           

            //v4.8
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;
            var formatA = RenderTextureFormat.DefaultHDR ; //v3.4.9
            RenderTexture tmpBufferA = RenderTexture.GetTemporary(rtW, rtH, 0, formatA);
            //v0.1
            cmd.Blit( source, tmpBufferA);  
            sheetSHAFTS.SetFloat("_Intensity", intensity);
            sheetSHAFTS.SetTexture("_InputTexture", tmpBufferA);

            //v4.9
            RenderTexture TemporaryColorTextureScaled = RenderTexture.GetTemporary(rtW, rtH, 0, formatA);

            //v4.8
            //ACCESS MASK
            if (useWaterAirMask)
            {
                if (Camera.main != null)
                {
                    float camXRot = Camera.main.transform.eulerAngles.x;
                    if (camXRot > 180)
                    {
                        camXRot = -(360 - camXRot);
                    }
                    sheetSHAFTS.SetFloat("_CamXRot", camXRot);
                }
                //var downsampleProperties = new MaterialPropertyBlock();
                //downsampleProperties.SetTexture("_InputTexture", tmpBufferA);
                //downsampleProperties.SetFloat("waterHeight", waterHeight);
                //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, m_TemporaryColorTextureScaled, downsampleProperties, 7);

                sheetSHAFTS.SetTexture("_InputTexture", tmpBufferA);
                sheetSHAFTS.SetFloat("waterHeight", waterHeight);
                //Blit(cmd, source, m_TemporaryColorTextureScaled.Identifier(), sheetSHAFTS, 7);
                //v0.1
                cmd.Blit( source, TemporaryColorTextureScaled, sheetSHAFTS, 7);
            }
            else if (useVolumeMaskRenderer)
            {
                //var downsampleProperties = new MaterialPropertyBlock();
                //downsampleProperties.SetTexture("_InputTextureA", MaskMapA); //MaskMap
                //v1.2a
                //downsampleProperties.SetFloat("_Height", Camera.main.transform.position.y);
                //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, m_TemporaryColorTextureScaled, downsampleProperties, 11);

                if (useTexture2DMask)
                {
                    sheetSHAFTS.SetTexture("_InputTextureA", MaskMapA); //MaskMap
                }
                else
                {
                    sheetSHAFTS.SetTexture("_InputTextureA", MaskMap); //MaskMap
                }
                sheetSHAFTS.SetFloat("_Height", Camera.main.transform.position.y);
                // Blit(cmd, source, m_TemporaryColorTextureScaled.Identifier(), sheetSHAFTS, 11);
                //v0.1
                cmd.Blit( source, TemporaryColorTextureScaled, sheetSHAFTS, 11);
            }
            else
            {
                //var downsampleProperties = new MaterialPropertyBlock();
                //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, m_TemporaryColorTextureScaled, downsampleProperties, 10);
                //Blit(cmd, source, m_TemporaryColorTextureScaled.Identifier(), sheetSHAFTS, 10);
                //v0.1
                cmd.Blit( source, TemporaryColorTextureScaled, sheetSHAFTS, 10);
            }
            //END v4.8




            sheetSHAFTS.SetFloat("_Blend", blend);
            Camera camera = Camera.main;
            if (useDepthTexture)
            {
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }
            Vector3 v = Vector3.one * 0.5f;
            if (sunTransform != Vector3.zero)
            {
                v = Camera.main.WorldToViewportPoint(sunTransform);// - Camera.main.transform.position;
            }
            else
            {
                v = new Vector3(0.5f, 0.5f, 0.0f);
            }


            //v4.8
            /////// UNDERWATER ONLY, always face camera
            if (shaftsFollowCamera == true)
            {
                v = Camera.main.WorldToViewportPoint(Camera.main.transform.position
                    + (new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * 1000 + Vector3.up * 500));
            }


            //v0.1
            var formatB = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
            RenderTexture m_TemporaryColorTexture = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatB);
            RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatB);
            RenderTexture m_TemporaryColorTextureScaled = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatB);
            //RTHandle lrDepthBuffer; //v0.1
            ////v4.8
            //public RTHandle m_TemporaryColorTextureScaled;  //v0.1 


            //v0.1
            //int rtW = opaqueDesc.width;
            //int rtH = opaqueDesc.height;
   //         cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode); //v0.1
            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
            sheetSHAFTS.SetVector("_SunThreshold", sunThreshold);

            if (!useDepthTexture)
            {
                var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                RenderTexture.active = tmpBuffer;
                GL.ClearWithSkybox(false, camera);

                sheetSHAFTS.SetTexture("_Skybox", tmpBuffer);

                //v0.1
                cmd.Blit( source, lrDepthBuffer, sheetSHAFTS, 3);
                RenderTexture.ReleaseTemporary(tmpBuffer);
            }
            else
            {
                //v0.1
                cmd.Blit( source, lrDepthBuffer, sheetSHAFTS, 2);
            }





            //v4.8
            //v0.1
            cmd.Blit( source, m_TemporaryColorTexture); //KEEP BACKGROUND 
            //Blit(cmd, source, tmpBufferA);
            //sheetSHAFTS.SetFloat("scalerMask", 1);
            //Blit(cmd, source, tmpBufferA);
            //sheetSHAFTS.SetTexture("_MainTex", tmpBufferA);
            //sheetSHAFTS.SetTexture("_InputTexture", tmpBufferA);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, m_TemporaryColorTexture, UPsampleProperties03, 6);
           // Blit(cmd, source, m_TemporaryColorTexture.Identifier(), sheetSHAFTS,6);


            radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);

            float ofs = sunShaftBlurRadius * (1.0f / 768.0f);
            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

            float adjustX = 0.5f;
            if (v.x < 0.5f)
            {
                float diff = 0.5f - v.x; adjustX = adjustX - 0.5f * diff;
            }
            float adjustY = 0.5f;
            if (v.y > 1.25f)
            {
                float diff2 = v.y - 1.25f; adjustY = adjustY - 0.3f * diff2;
            }
            if (v.y > 1.8f)
            {
                v.y = 1.8f; float diff3 = v.y - 1.25f; adjustY = 0.5f - 0.3f * diff3;
            }

            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x * 0.5f + adjustX, v.y * 0.5f + adjustY, v.z, maxRadius));

            for (int it2 = 0; it2 < radialBlurIterations; it2++)
            {
                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);

                //v0.1
                cmd.Blit( lrDepthBuffer, lrColorB, sheetSHAFTS, 1);

                RenderTexture.ReleaseTemporary(lrDepthBuffer); //cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name)); //v0.1
                ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
                lrDepthBuffer = RenderTexture.GetTemporary(opaqueDesc.width, opaqueDesc.height, 0, formatB); //cmd.GetTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name), opaqueDesc, filterMode); //v0.1

                //v0.1
                cmd.Blit( lrColorB, lrDepthBuffer, sheetSHAFTS, 1);

                RenderTexture.ReleaseTemporary(lrColorB);
                ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }

            if (v.z >= 0.0f)
            {
                sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
            }
            else
            {
                sheetSHAFTS.SetVector("_SunColor", Vector4.zero);
            }
            cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer);



            //v4.8
            //MASK RESULT
            //var UPsampleProperties05 = new MaterialPropertyBlock();
            //UPsampleProperties05.SetTexture("_MainTexA", m_TemporaryColorTexture);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrColorB, UPsampleProperties05, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);
            //v0.1
            cmd.Blit( m_TemporaryColorTexture, lrColorB, sheetSHAFTS, 
                (screenBlendMode == BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);


            //v4.8
            //var UPsampleProperties1 = new MaterialPropertyBlock();
            // UPsampleProperties1.SetTexture("_WaterInterfaceTex", m_TemporaryColorTextureScaled);
            if (useTexture2DMask)
            {
                sheetSHAFTS.SetTexture("_WaterInterfaceTex", MaskMapA);
            }
            else
            {
                sheetSHAFTS.SetTexture("_WaterInterfaceTex", TemporaryColorTextureScaled);// MaskMap);
            }
            sheetSHAFTS.SetTexture("_MainTex", lrColorB);
            sheetSHAFTS.SetTexture("_SourceTex", tmpBufferA);
            sheetSHAFTS.SetFloat("scalerMask", scalerMask);
            //v1.3
            //if (useVolumeLightsHDRPMap)
            //{
            //    UPsampleProperties1.SetFloat("_InputTextureBPower", VolumeLightsHDRPBlend);
            //    UPsampleProperties1.SetTexture("_InputTextureB", VolumeLightsHDRPMap.vue);
            //}
            ////////////////// REFRACT LINE
            if (BumpMap != null)
            {
                sheetSHAFTS.SetTexture("_BumpTex", BumpMap);
                //Debug.Log(BumpMap.value.width);
            }
            sheetSHAFTS.SetFloat("_BumpMagnitude", BumpIntensity);
            sheetSHAFTS.SetFloat("_BumpScale", BumpScale);
            //v4.6
            sheetSHAFTS.SetFloat("_BumpMagnitudeRL", BumpIntensityRL);
            sheetSHAFTS.SetFloat("_BumpScaleRL", BumpScaleRL);
            sheetSHAFTS.SetFloat("_BumpLineHeight", BumpLineHeight);
            sheetSHAFTS.SetVector("_BumpVelocity", BumpVelocity);
            sheetSHAFTS.SetVector("_underWaterTint", underWaterTint);
            sheetSHAFTS.SetFloat("_underwaterDepthFade", underwaterDepthFade);
            sheetSHAFTS.SetFloat("_refractLineWidth", refractLineWidth); //v4.6
            sheetSHAFTS.SetFloat("_refractLineXDisp", refractLineXDisp); //v4.6
            sheetSHAFTS.SetFloat("_refractLineXDispA", refractLineXDispA); //v4.6
            sheetSHAFTS.SetVector("_refractLineFade", refractLineFade); //v4.6
                                                                        ////////////////// END REFRACT LINE ////


            //v0.1
            cmd.Blit( lrColorB, source, sheetSHAFTS, 8);
            //Blit(cmd, lrColorB, source);//, sheetSHAFTS, 8);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, UPsampleProperties1, 8);
            //Blit(cmd, m_TemporaryColorTexture.Identifier(), source, sheetSHAFTS, (screenBlendMode == BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);

           

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            RenderTexture.ReleaseTemporary(lrDepthBuffer);
            RenderTexture.ReleaseTemporary(m_TemporaryColorTexture);
            RenderTexture.ReleaseTemporary(m_TemporaryColorTextureScaled);

            cmd.ReleaseTemporaryRT(Shader.PropertyToID(lrDepthBuffer.name)); //v0.1
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_TemporaryColorTexture.name)); //v0.1
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_TemporaryColorTextureScaled.name)); //v4.8 //v0.1

            RenderTexture.ReleaseTemporary(lrColorB);
            RenderTexture.ReleaseTemporary(tmpBufferA);
            RenderTexture.ReleaseTemporary(TemporaryColorTextureScaled);
        }


    }
}
