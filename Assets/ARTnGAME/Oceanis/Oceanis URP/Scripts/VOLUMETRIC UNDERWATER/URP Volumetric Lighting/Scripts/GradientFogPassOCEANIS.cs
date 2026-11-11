using System.Collections;
using System.Collections.Generic;
//using Toguchi.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.Oceanis
{
    public class GradientFogPassOCEANIS : CustomPostProcessingPassOCEANIS<GradientFogOCEANIS>
    {
        private static readonly int MaskTexId = UnityEngine.Shader.PropertyToID("_MaskTex");
        private static readonly int RampTexId = UnityEngine.Shader.PropertyToID("_RampTex");
        private static readonly int IntensityId = UnityEngine.Shader.PropertyToID("_Intensity");

        //v0.1
        private static readonly int cutoffHeigthId = UnityEngine.Shader.PropertyToID("cutoffHeigth");
        private static readonly int depthFadeId = UnityEngine.Shader.PropertyToID("depthFade");
        private static readonly int fogTintId = UnityEngine.Shader.PropertyToID("fogTint");

        protected override string RenderTag => "GradientFogOCEANIS";

        protected override void BeforeRender(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            Material.SetTexture(MaskTexId, Component.maskTexture.value);
            Material.SetTexture(RampTexId, Component.gradientTexture.value);
            Material.SetFloat(IntensityId, Component.intensity.value);

            //v0.1
            Material.SetFloat(cutoffHeigthId, Component.cutoffHeigth.value);
            cutoffHeigth = Component.cutoffHeigth.value;
            Material.SetFloat(depthFadeId, Component.depthFade.value);
            Material.SetColor(fogTintId, Component.fogTint.value);

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
            Material.SetTexture(MaskTexId, Component.maskTexture.value);
            Material.SetTexture(RampTexId, Component.gradientTexture.value);
            Material.SetFloat(IntensityId, Component.intensity.value);

            //v0.1
            Material.SetFloat(cutoffHeigthId, Component.cutoffHeigth.value);
            cutoffHeigth = Component.cutoffHeigth.value;
            Material.SetFloat(depthFadeId, Component.depthFade.value);
            Material.SetColor(fogTintId, Component.fogTint.value);

            SetupM(data.colorTargetHandleA);

//            //v0.1
//#if UNITY_2022_1_OR_NEWER
//            SetupM(renderingData.cameraData.renderer.cameraColorTargetHandle);
//#else
//            SetupM(renderingData.cameraData.renderer.cameraColorTarget);
//#endif
        }
#endif

        public float cutoffHeigth = 0;

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

        protected override bool IsActive()
        {
            cutoffHeigth = Component.cutoffHeigth.value;
            return Component.IsActive;
        }

        public GradientFogPassOCEANIS(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader)
        {
           
        }
    }
}