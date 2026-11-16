using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//using Unity.Mathematics;

namespace Artngame.Oceanis
{
  public class VolumetricLightScattering : ScriptableRendererFeature
  {
    [System.Serializable]
    public class VolumetricLightScatteringSettings
    {
      [Header("Volumetric Properties")]
      [Range(0.1f, 1f)]
      public float resolutionScale = 0.5f;
      [Range(0.0f, 1.0f)]
      public float intensity = 1.0f;
      [Range(0.0f, 1.0f)]
      public float blurWidth = 0.85f;
      [Range(0.0f, 0.2f)]
      public float fadeRange = 0.85f;
      [Range(50, 200)]
      public uint numSamples = 100;

      [Header("Noise Properties")]
      //public float2 noiseSpeed = 0.5f;
      public Vector2 noiseSpeed = 0.5f*Vector2.one;
      public float noiseScale = 1.0f;
      [Range(0.0f, 1.0f)]
      public float noiseStrength = 0.6f;
    }

    class LightScatteringPass : ScriptableRenderPass
    {
#if UNITY_2022_1_OR_NEWER
            private RTHandle _occluders;// = RenderTargetHandle.CameraTarget; //v0.1
#else
            private RenderTargetHandle _occluders = RenderTargetHandle.CameraTarget; //v0.1
#endif

            private readonly VolumetricLightScatteringSettings _settings;
      private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();
      private Material _occludersMaterial;
      private Material _radialBlurMaterial;
      private FilteringSettings _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
      private RenderTargetIdentifier _cameraColorTargetIdent;

      public LightScatteringPass(VolumetricLightScatteringSettings settings)
      {

#if UNITY_2022_1_OR_NEWER
                _occluders = RTHandles.Alloc("_OccludersMap", name: "_OccludersMap"); //v0.1
#else
                _occluders.Init("_OccludersMap"); //v0.1
#endif
                //

                _settings = settings;

        _shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        _shaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        _shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
        _shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
      }

      public void SetCameraColorTarget(RenderTargetIdentifier _cameraColorTargetIdent)
        => this._cameraColorTargetIdent = _cameraColorTargetIdent;

#if UNITY_6000_4_OR_NEWER
            //DO NOTHING
#else
            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in a performant manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)// public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
#if UNITY_2022_1_OR_NEWER
                _occluders = renderingData.cameraData.renderer.cameraColorTargetHandle;//v0.1
#else
                //_occluders = RenderTargetHandle.CameraTarget;//v0.1
#endif


                // get a copy of the current camera’s RenderTextureDescriptor
                // this descriptor contains all the information you need to create a new texture
                RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;

                // disable the depth buffer because we are not going to use it
                cameraTextureDescriptor.depthBufferBits = 0;

                // scale the texture dimensions
                cameraTextureDescriptor.width = Mathf.RoundToInt(cameraTextureDescriptor.width * _settings.resolutionScale);
                cameraTextureDescriptor.height = Mathf.RoundToInt(cameraTextureDescriptor.height * _settings.resolutionScale);

#if UNITY_2022_1_OR_NEWER
                // create temporary render texture
                cmd.GetTemporaryRT(Shader.PropertyToID(_occluders.name), cameraTextureDescriptor, FilterMode.Bilinear);//v0.1

                // finish configuration
                ConfigureTarget(_occluders);//.Identifier());
#else
                // create temporary render texture
                cmd.GetTemporaryRT(_occluders.id, cameraTextureDescriptor, FilterMode.Bilinear);//v0.1

                // finish configuration
                ConfigureTarget(_occluders.Identifier());
#endif

            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!_occludersMaterial || !_radialBlurMaterial) InitializeMaterials();



                if (RenderSettings.sun == null || !RenderSettings.sun.enabled) return;




                // get command buffer pool
                CommandBuffer cmd = CommandBufferPool.Get();

                using (new ProfilingScope(cmd, new ProfilingSampler("VolumetricLightScattering")))
                {



                    // prepares command buffer
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    OnCameraSetup(cmd, ref renderingData);

                    Camera camera = renderingData.cameraData.camera;
                    context.DrawSkybox(camera);

                    DrawingSettings drawSettings = CreateDrawingSettings(
                      _shaderTagIdList, ref renderingData, SortingCriteria.CommonOpaque
                    );
                    drawSettings.overrideMaterial = _occludersMaterial;
                    context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _filteringSettings);

                    // schedule it for execution and release it after the execution
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);

                    //float3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;
                    //float3 cameraDirectionWorldSpace = camera.transform.forward;
                    //float3 cameraPositionWorldSpace = camera.transform.position;
                    //float3 sunPositionWorldSpace = cameraPositionWorldSpace + sunDirectionWorldSpace;
                    //float3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);
                    Vector3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;
                    Vector3 cameraDirectionWorldSpace = camera.transform.forward;
                    Vector3 cameraPositionWorldSpace = camera.transform.position;
                    Vector3 sunPositionWorldSpace = cameraPositionWorldSpace + sunDirectionWorldSpace;
                    Vector3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);

                    //float dotProd = math.dot(-cameraDirectionWorldSpace, sunDirectionWorldSpace);
                    //dotProd -= math.dot(cameraDirectionWorldSpace, Vector3.down);

                    float dotProd = Vector3.Dot(-cameraDirectionWorldSpace, sunDirectionWorldSpace);
                    dotProd -= Vector3.Dot(cameraDirectionWorldSpace, Vector3.down);

                    float intensityFader = dotProd / _settings.fadeRange;
                    intensityFader = Mathf.Clamp01(intensityFader); //intensityFader = math.saturate(intensityFader);

                    _radialBlurMaterial.SetColor("_Color", RenderSettings.sun.color);
                    _radialBlurMaterial.SetVector("_Center", new Vector4(
                      sunPositionViewportSpace.x, sunPositionViewportSpace.y, 0.0f, 0.0f
                    ));
                    _radialBlurMaterial.SetFloat("_BlurWidth", _settings.blurWidth);
                    _radialBlurMaterial.SetFloat("_NumSamples", _settings.numSamples);
                    _radialBlurMaterial.SetFloat("_Intensity", _settings.intensity * intensityFader);

                    //_radialBlurMaterial.SetVector("_NoiseSpeed", new float4(_settings.noiseSpeed, 0.0f, 0.0f));
                    _radialBlurMaterial.SetVector("_NoiseSpeed", new Vector4(_settings.noiseSpeed.x, _settings.noiseSpeed.y, 0.0f, 0.0f));

                    _radialBlurMaterial.SetFloat("_NoiseScale", _settings.noiseScale);
                    _radialBlurMaterial.SetFloat("_NoiseStrength", _settings.noiseStrength);

#if UNITY_2022_1_OR_NEWER
                    cmd.Blit( _occluders, _cameraColorTargetIdent, _radialBlurMaterial);//v0.1
#else
                    cmd.Blit(_occluders.id, _cameraColorTargetIdent, _radialBlurMaterial);//v0.1
#endif


                    OnCameraCleanup(cmd);
                }
            }
#endif

            // Cleanup any allocated resources that were created during the execution of this render pass.
            //public override void OnCameraCleanup(CommandBuffer cmd)
            //{
            //  cmd.ReleaseTemporaryRT(_occluders.id);
            //}
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
#if UNITY_2022_1_OR_NEWER
                cmd.ReleaseTemporaryRT(Shader.PropertyToID(_occluders.name));//v0.1
#else
                cmd.ReleaseTemporaryRT(_occluders.id);//v0.1
#endif

            }

            private void InitializeMaterials()
      {
        _occludersMaterial = new Material(Shader.Find("Hidden/UnlitColorOCEANIS"));
        _radialBlurMaterial = new Material(Shader.Find("Hidden/RadialBlurOCEANIS"));
      }
    }

    private LightScatteringPass _scriptablePass;
    public VolumetricLightScatteringSettings _settings =  new VolumetricLightScatteringSettings();

    /// <inheritdoc/>
    public override void Create()
    {
      _scriptablePass = new LightScatteringPass(_settings);

      // Configures where the render pass should be injected.
      _scriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
      renderer.EnqueuePass(_scriptablePass);
#if UNITY_2022_1_OR_NEWER
#if UNITY_6000_4_OR_NEWER
            //DO NOTHING
#else
            _scriptablePass.SetCameraColorTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);//renderer.cameraColorTarget);//v0.1
#endif
#else
            _scriptablePass.SetCameraColorTarget(renderer.cameraColorTarget);//v0.1
#endif

        }
    }
}

