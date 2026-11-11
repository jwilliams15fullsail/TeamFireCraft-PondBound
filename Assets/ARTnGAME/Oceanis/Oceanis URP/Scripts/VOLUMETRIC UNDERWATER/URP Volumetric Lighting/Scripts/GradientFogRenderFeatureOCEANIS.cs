//using Toguchi.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.Oceanis
{
    public class GradientFogRenderFeatureOCEANIS : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
        }

        public Settings settings = new Settings();

        private GradientFogPassOCEANIS _pass;

        public override void Create()
        {
            this.name = "GradientFogOCEANIS";
            _pass = new GradientFogPassOCEANIS(settings.renderPassEvent, settings.shader);
        }

        public bool enableAllTime = false;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (Camera.main != null && (Camera.main.transform.position.y < _pass.cutoffHeigth || (Application.isPlaying && enableAllTime)))
            {
#if UNITY_2022_1_OR_NEWER
               // _pass.Setup(renderingData.cameraData.renderer.cameraColorTargetHandle);//renderer.cameraColorTarget); //v0.1
#else
              //  _pass.setupINIT(renderingData);
               //_pass.Setup(renderer.cameraColorTarget); //v0.1
               // _pass.SetupMA(); //v0.1
#endif

                renderer.EnqueuePass(_pass);
            }
        }
    }
}
