using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.Oceanis
{
    public class LightShaftPassOCEANIS : CustomPostProcessingPassOCEANIS<LightShaftOCEANIS>
    {
        public LightShaftPassOCEANIS(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader)
        {
        }
        private static readonly int MaskTexId = UnityEngine.Shader.PropertyToID("_MaskTex");
        private static readonly int LightShaftTempId = UnityEngine.Shader.PropertyToID("_LightShaftTempTex");

        protected override string RenderTag => "LightShaftOCEANIS";
        
        protected override void BeforeRender(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            Material.SetTexture(MaskTexId, Component.maskTexture.value);
            Material.SetMatrix("_CamFrustum", FrustumCorners(camera));
            Material.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix);
            Material.SetVector("_CamWorldSpace", camera.transform.position);
            Material.SetInt("_MaxIterations", Component.maxIterations.value);

            Material.SetVector("shaftsDensity", Component.shaftsDensity.value);

            Material.SetFloat("_MaxDistance", Component.maxDistance.value);
            Material.SetFloat("_MinDistance", Component.minDistance.value);
            Material.SetFloat("_Intensity", Component.intensity.value);

            //v0.1
            Material.SetInt("_visibleLightsCount", renderingData.cullResults.visibleLights.Length);

            //v0.1
            Material.SetFloat("cutoffHeigth", Component.cutoffHeigth.value);
            cutoffHeigth = Component.cutoffHeigth.value;

            //v0.2
            //Material.SetTexture("CookieTex", Component.maskTexture.value);
            Transform sun = null;
            if(Camera.main != null)
            {
                cycleCaustucsTexture cycler = Camera.main.GetComponent<cycleCaustucsTexture>();
                if (cycler != null)
                {
                    sun = cycler.sun;
                    //v0.3 - coookie custom
                    //if (sun != null)
                    {
                        Matrix4x4 cookieUVTransform = Matrix4x4.identity;
                        GetLightUVScaleOffset( ref cookieUVTransform,cycler);
                        Matrix4x4 cookieMatrix = s_DirLightProj * cookieUVTransform *
                               sun.localToWorldMatrix.inverse;
                        Material.SetMatrix("_MainLightWorldToLight", cookieMatrix);
                    }

                    Material.SetFloat("customCookieIntensityA", cycler.customCookieIntensityA);

                    if (cycler.autoRegulateShafts && cycler.GetComponent<Camera>() != null)
                    {
                        float diff = cycler.waterHeight - cycler.GetComponent<Camera>().transform.position.y;
                        if (diff > 0)
                        {
                            Material.SetFloat("_MaxDistance", -cycler.GetComponent<Camera>().transform.position.y + cycler.maxDistOffset);
                            Material.SetFloat("_MinDistance", -cycler.GetComponent<Camera>().transform.position.y);
                            Material.SetFloat("_Intensity", Component.intensity.value / Mathf.Pow(diff, cycler.fadeOutPower));
                            Material.SetFloat("customCookieIntensityA", cycler.customCookieIntensityA / Mathf.Pow(diff+0.5f, cycler.fadeOutPower));
                        }
                    }

                    Material.SetTexture("_CookieTex", cycler.Caustics[cycler.currentFrame]);
                    Material.SetFloat("lightCookieIntensity", cycler.lightCookieIntensity);
                    Material.SetFloat("customCookieIntensity", cycler.customCookieIntensity);
                    
                    Material.SetFloat("customCookieScaler", cycler.customCookieScaler);
                    Material.SetFloat("customCookieScalerA", cycler.customCookieScalerA);
                }
            }

            //v0.1
#if UNITY_2022_1_OR_NEWER
    #if UNITY_6000_4_OR_NEWER
                //DO NOTHING
    #else
                SetupM(renderingData.cameraData.renderer.cameraColorTargetHandle);
    #endif
#else
            SetupM(renderingData.cameraData.renderer.cameraColorTarget);
#endif

        }


#if UNITY_2023_3_OR_NEWER
        protected override void BeforeRenderRG(CommandBuffer commandBuffer, PassData data)
        {
            ref var cameraData = ref data.cameraData;
            var camera = cameraData.camera;

            Material.SetTexture(MaskTexId, Component.maskTexture.value);
            Material.SetMatrix("_CamFrustum", FrustumCorners(camera));
            Material.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix);
            Material.SetVector("_CamWorldSpace", camera.transform.position);
            Material.SetInt("_MaxIterations", Component.maxIterations.value);

            Material.SetVector("shaftsDensity", Component.shaftsDensity.value);

            Material.SetFloat("_MaxDistance", Component.maxDistance.value);
            Material.SetFloat("_MinDistance", Component.minDistance.value);
            Material.SetFloat("_Intensity", Component.intensity.value);

            //v0.1
            Material.SetInt("_visibleLightsCount", data.cullResults.visibleLights.Length);

            //v0.1
            Material.SetFloat("cutoffHeigth", Component.cutoffHeigth.value);
            cutoffHeigth = Component.cutoffHeigth.value;

            //v0.2
            //Material.SetTexture("CookieTex", Component.maskTexture.value);
            Transform sun = null;
            if (Camera.main != null)
            {
                cycleCaustucsTexture cycler = Camera.main.GetComponent<cycleCaustucsTexture>();
                if (cycler != null)
                {
                    sun = cycler.sun;
                    //v0.3 - coookie custom
                    //if (sun != null)
                    {
                        Matrix4x4 cookieUVTransform = Matrix4x4.identity;
                        GetLightUVScaleOffset(ref cookieUVTransform, cycler);
                        Matrix4x4 cookieMatrix = s_DirLightProj * cookieUVTransform *
                               sun.localToWorldMatrix.inverse;
                        Material.SetMatrix("_MainLightWorldToLight", cookieMatrix);
                    }

                    Material.SetFloat("customCookieIntensityA", cycler.customCookieIntensityA);

                    if (cycler.autoRegulateShafts && cycler.GetComponent<Camera>() != null)
                    {
                        float diff = cycler.waterHeight - cycler.GetComponent<Camera>().transform.position.y;
                        if (diff > 0)
                        {
                            Material.SetFloat("_MaxDistance", -cycler.GetComponent<Camera>().transform.position.y + cycler.maxDistOffset);
                            Material.SetFloat("_MinDistance", -cycler.GetComponent<Camera>().transform.position.y);
                            Material.SetFloat("_Intensity", Component.intensity.value / Mathf.Pow(diff, cycler.fadeOutPower));
                            Material.SetFloat("customCookieIntensityA", cycler.customCookieIntensityA / Mathf.Pow(diff + 0.5f, cycler.fadeOutPower));
                        }
                    }

                    Material.SetTexture("_CookieTex", cycler.Caustics[cycler.currentFrame]);
                    Material.SetFloat("lightCookieIntensity", cycler.lightCookieIntensity);
                    Material.SetFloat("customCookieIntensity", cycler.customCookieIntensity);

                    Material.SetFloat("customCookieScaler", cycler.customCookieScaler);
                    Material.SetFloat("customCookieScalerA", cycler.customCookieScalerA);
                }
            }

            SetupM(data.colorTargetHandleA);
//            //v0.1
//#if UNITY_2022_1_OR_NEWER
//            SetupM(renderingData.cameraData.renderer.cameraColorTargetHandle);
//#else
//            SetupM(renderingData.cameraData.renderer.cameraColorTarget);
//#endif

        }

        protected override void RenderRG(CommandBuffer commandBuffer, PassData data, RenderTargetIdentifier source,
      RenderTargetIdentifier dest)
        {
            cutoffHeigth = Component.cutoffHeigth.value;

            ref var cameraData = ref data.cameraData;

            ///v0.1
            //commandBuffer.GetTemporaryRT(LightShaftTempId,cameraData.camera.scaledPixelWidth / 4, cameraData.camera.scaledPixelHeight / 4);
            //commandBuffer.GetTemporaryRT(LightShaftTempId, cameraData.camera.scaledPixelWidth / 2, cameraData.camera.scaledPixelHeight / 2);
            commandBuffer.GetTemporaryRT(LightShaftTempId, cameraData.camera.scaledPixelWidth / 2, cameraData.camera.scaledPixelHeight / 2, 16, FilterMode.Trilinear, RenderTextureFormat.DefaultHDR);

            // LightShaft生成
            commandBuffer.Blit(null, LightShaftTempId, Material, 0);
            commandBuffer.SetGlobalTexture(LightShaftTempId, new RenderTargetIdentifier(LightShaftTempId));
            commandBuffer.Blit(source, dest, Material, 1);

            commandBuffer.ReleaseTemporaryRT(LightShaftTempId);
        }
#endif

        //v0.3
        // Unity defines directional light UVs over a unit box centered at light.
        // i.e. (0, 1) uv == (-0.5, 0.5) world area instead of the (0,1) world area.
        static readonly Matrix4x4 s_DirLightProj = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, -0.5f, 0.5f);
        private void GetLightUVScaleOffset( ref Matrix4x4 uvTransform, cycleCaustucsTexture cycler)
        {
            Vector2 uvScale = Vector2.one / cycler.lightCookieSize;
            Vector2 uvOffset = Vector2.zero;

            //if (Mathf.Abs(uvScale.x) < half.MinValue)
            //    uvScale.x = Mathf.Sign(uvScale.x) * half.MinValue;
            //if (Mathf.Abs(uvScale.y) < half.MinValue)
            //    uvScale.y = Mathf.Sign(uvScale.y) * half.MinValue;

            uvTransform = Matrix4x4.Scale(new Vector3(uvScale.x, uvScale.y, 1));
            uvTransform.SetColumn(3, new Vector4(-uvOffset.x * uvScale.x, -uvOffset.y * uvScale.y, 0, 1));
        }


        //v0.1
        public void setupINIT(RenderingData renderingData)
        {
            //v0.1
#if UNITY_2022_1_OR_NEWER
    #if UNITY_6000_4_OR_NEWER
                //DO NOTHING
    #else
                SetupM(renderingData.cameraData.renderer.cameraColorTargetHandle);
    #endif
#else
            SetupM(renderingData.cameraData.renderer.cameraColorTarget);
#endif
        }

        public float cutoffHeigth = 0;

        protected override void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source,
            RenderTargetIdentifier dest)
        {
            cutoffHeigth = Component.cutoffHeigth.value;

            ref var cameraData = ref renderingData.cameraData;

            //v0.1
            //commandBuffer.GetTemporaryRT(LightShaftTempId,cameraData.camera.scaledPixelWidth / 4, cameraData.camera.scaledPixelHeight / 4);
            //commandBuffer.GetTemporaryRT(LightShaftTempId, cameraData.camera.scaledPixelWidth / 2, cameraData.camera.scaledPixelHeight / 2);
            commandBuffer.GetTemporaryRT(LightShaftTempId, cameraData.camera.scaledPixelWidth / 2, cameraData.camera.scaledPixelHeight / 2, 0, FilterMode.Trilinear, RenderTextureFormat.DefaultHDR);

            // LightShaft生成
            commandBuffer.Blit(null, LightShaftTempId, Material, 0);
            commandBuffer.SetGlobalTexture(LightShaftTempId, new RenderTargetIdentifier(LightShaftTempId));
            commandBuffer.Blit(source, dest, Material, 1);
            
            commandBuffer.ReleaseTemporaryRT(LightShaftTempId);
        }
     

        // 参考: http://hventura.com/unity-post-process-v2-raymarching.html
        private Matrix4x4 FrustumCorners(Camera cam)
        {
            Transform camtr = cam.transform;

            Vector3[] frustumCorners = new Vector3[4];
            cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1),
                cam.farClipPlane, cam.stereoActiveEye, frustumCorners);

            Matrix4x4 frustumVectorsArray = Matrix4x4.identity;
            
            frustumVectorsArray.SetRow(0,  camtr.TransformVector(frustumCorners[0]));
            frustumVectorsArray.SetRow(1, camtr.TransformVector(frustumCorners[3]));
            frustumVectorsArray.SetRow(2, camtr.TransformVector(frustumCorners[1]));
            frustumVectorsArray.SetRow(3, camtr.TransformVector(frustumCorners[2]));
            
            return frustumVectorsArray;
        }

        protected override bool IsActive()
        {
            cutoffHeigth = Component.cutoffHeigth.value;
            return Component.IsActive;
        }
    }
}