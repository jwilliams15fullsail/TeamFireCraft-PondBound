//#define HDRP

using UnityEngine;
using System.Collections.Generic;
//using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
//using UnityEngine.Rendering.LWRP;
//using UnityEngine.Experimental.Rendering.LWRP;
//using UnityEngine.Experimental.Rendering;
//using UnityEngine.Experimental.Rendering.HDPipeline;
//using UnityEngine.Experimental.Rendering.HDPipeline;

#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Artngame.CommonTools.DepthRenderer
{
    [ExecuteAlways]//[ImageEffectAllowedInSceneView] //[ExecuteAlways]
    public class DepthRendererCT_HDRP : MonoBehaviour//, IBeforeCameraRender
    {
#if HDRP

        //v0.1
        public List<Material> passDepthCamPosToMats = new List<Material>();
        public bool passDepthCamPosOtherMats = true;
        public bool followCamera = false;
        public Vector2 DepthCameraInitPos = new Vector2(0, 0);

        //v0.2
        public float depthCameraNearPlane = 0.3f;
        public float depthCameraFarPlane = 100;
        public float depthCameraSize = 128;

        //v1.2 - optionally setup volume depth renderer with depth camera and grab texture from there
        public bool useVolumeRenderer = false;
        public GameObject volumeRendererDEPTHOBJ; ///DepthRendererSM_HDRP_VOLUME volumeRendererDEPTH;
        //DepthRendererCT_HDRP_VOLUME volumeRendererDEPTH;
        public int passID = 0;
        public bool updateVolumePerDistance = false;
        public Vector3 prevVolPosition;
        bool enabledVolume = false;
        public bool renderOnce = true; //render only one time in non distance based update

        //v1.2 - disable after start
        public bool disableAfterStart = false;
        private void LateUpdate()
        {
            if (disableAfterStart && Application.isPlaying)
            {
                this.enabled = false;
            }
        }

        public int renderDepth = -16;
        public bool updatePerDistance = false;
        public bool updateAlways = false;
        public int framesForCameraRender = 3;
        public float updateDistance = 100;
        public float depthUnderWater = 8;
        public Vector3 prevPosition;

        [System.Serializable]
        public enum ResolutionMulltiplier
        {
            Full,
            Half,
            Third,
            Quarter
        }
        public int depthResolution = 2048;
        public bool useScreenRes = false;
        [System.Serializable]
        public class PlanarReflectionSettings
        {
            public ResolutionMulltiplier m_ResolutionMultiplier = ResolutionMulltiplier.Third;
            public float m_ClipPlaneOffset = 0.07f;
            public LayerMask m_ReflectLayers = -1;
        }

        public List<Material> Materials = new List<Material>();
        public string depthTextureName = "_TopDownDepthTexture";

        [SerializeField]
        public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();

        public GameObject target;
        [FormerlySerializedAs("camOffset")] public float m_planeOffset;

        private static Camera m_ReflectionCamera;
        //private int2 m_TextureSize = new int2(256, 128);
        //private Vector2 m_TextureSize = new Vector2(256, 128);
        private RenderTexture m_ReflectionTexture = null;
        //private int planarReflectionTextureID = Shader.PropertyToID("_RefractionTextureSM");
        private int planarReflectionTextureID;
        public string refractionTextureName = "_ShoreContourTex";//"_topDownDepthSM";
        public float depthCameraHeight = 1000;

        // private int2 m_OldReflectionTextureSize;
        // private Vector2 m_OldReflectionTextureSize;

        // Cleanup all the objects we possibly have created
        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        public bool clearAtStart = false;
        void Start()
        {
            prevPosition = Camera.main.transform.position;
            prevVolPosition = Camera.main.transform.position;
            renderedDepth = -1;

            if (clearAtStart) //use for full refraction case
            {
                //if (m_ReflectionCamera != null)
                //{
                //    Destroy(m_ReflectionCamera);
                //}
                if (m_ReflectionCamera)
                {
                    m_ReflectionCamera.targetTexture = null;
                    SafeDestroy(m_ReflectionCamera.gameObject);
                }
                //if (m_ReflectionTextureDEPTH)
                //{
                //RenderTexture.ReleaseTemporary(m_ReflectionTextureDEPTH);
                // }          
                RenderPipelineManager.beginCameraRendering -= DoDepthSM;


                Cleanup();
                RenderPipelineManager.beginCameraRendering += DoDepthSM;
                //RenderPipelineManager.endCameraRendering += DoReflections;

                //DoDepthSM();

                planarReflectionTextureID = Shader.PropertyToID(refractionTextureName);

                //m_ReflectionCamera.enabled = true;

                //v0.5
                Update();
                DoDepthSM2(Camera.main);
            }
        }

        //NEW
        //sample code 
        //https://github.com/0lento/OcclusionProbes/blob/package/OcclusionProbes/OcclusionProbes.cs#L41
        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += DoDepthSM;
            //RenderPipelineManager.endCameraRendering += DoReflections;

            planarReflectionTextureID = Shader.PropertyToID(refractionTextureName);

            prevPosition = Camera.main.transform.position;
        }
        //END NEW

        void Cleanup()
        {
            if (m_ReflectionCamera)
            {
                m_ReflectionCamera.targetTexture = null;
                SafeDestroy(m_ReflectionCamera.gameObject);
            }
            if (m_ReflectionTexture)
            {
                RenderTexture.ReleaseTemporary(m_ReflectionTexture);
            }
            //if (m_ReflectionTextureDEPTH)
            //{
            //    RenderTexture.ReleaseTemporary(m_ReflectionTextureDEPTH);
            // }
            //new
            RenderPipelineManager.beginCameraRendering -= DoDepthSM;
            //RenderPipelineManager.endCameraRendering += DoReflections;
        }

        void SafeDestroy(Object obj)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        private void UpdateCamera(Camera src, Camera dest)
        {
            if (dest == null)
                return;
            //dest.CopyFrom(src);
        }

        //void Update()
        //{
        //    //apply to materials
        //    for (int i = 0; i < Materials.Count; i++)
        //    {
        //        Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
        //        Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
        //    }
        //}

        private void UpdateReflectionCamera(Camera realCamera)
        {

            planarReflectionTextureID = Shader.PropertyToID(refractionTextureName); //////////// reset name if changed

            if (m_ReflectionCamera == null)
                m_ReflectionCamera = CreateMirrorObjects(realCamera);

            // find out the reflection plane: position and normal in world space
            Vector3 pos = Vector3.zero;
            Vector3 normal = Vector3.up;
            if (target != null)
            {
                pos = target.transform.position + Vector3.up * m_planeOffset;
                normal = target.transform.up;
            }

            UpdateCamera(realCamera, m_ReflectionCamera);

            // Render reflection
            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - m_settings.m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.identity;
            reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            Vector3 oldpos = realCamera.transform.position - new Vector3(0, pos.y * 2, 0);
            Vector3 newpos = ReflectPosition(oldpos);
            //    m_ReflectionCamera.transform.forward = Vector3.Scale(realCamera.transform.forward, new Vector3(1, -1, 1));
            //    m_ReflectionCamera.worldToCameraMatrix = realCamera.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(m_ReflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
            Matrix4x4 projection = realCamera.CalculateObliqueMatrix(clipPlane);
            //      m_ReflectionCamera.projectionMatrix = projection;
            m_ReflectionCamera.cullingMask = m_settings.m_ReflectLayers; // never render water layer
                                                                         //      m_ReflectionCamera.transform.position = newpos;

            //HDRP   
            m_ReflectionCamera.clearFlags = CameraClearFlags.Nothing;
            if (m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>() != null)
            {
                m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().volumeLayerMask = 0;
                m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().probeLayerMask = 0;
                //Debug.Log("Removed 2");
            }
            else
            {
                m_ReflectionCamera.gameObject.AddComponent<HDAdditionalCameraData>().volumeLayerMask = 0; //ADD the HDAdditionalCameraData here, as they are only added in editor if camera selected !!!!
                m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().probeLayerMask = 0;
                // Debug.Log("Removed 3");
            }
        }

        // Calculates reflection matrix around the given plane
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        private static Vector3 ReflectPosition(Vector3 pos)
        {
            Vector3 newPos = new Vector3(pos.x, -pos.y, pos.z);
            return newPos;
        }

        public float GetScaleValue()
        {
            switch (m_settings.m_ResolutionMultiplier)
            {
                case ResolutionMulltiplier.Full:
                    return 1f;
                case ResolutionMulltiplier.Half:
                    return 0.5f;
                case ResolutionMulltiplier.Third:
                    return 0.33f;
                case ResolutionMulltiplier.Quarter:
                    return 0.25f;
            }
            return 0.5f; // default to half res
        }

        // Compare two int2
        //private static bool Int2Compare(int2 a, int2 b)
        //{
        //    if (a.x == b.x && a.y == b.y)
        //        return true;
        //    else
        //        return false;
        //}

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        private Camera CreateMirrorObjects(Camera currentCamera)
        {
            GameObject go =
                new GameObject("Depth Camera id" + GetEntityId() + " for " + currentCamera.GetEntityId(),
                    typeof(Camera));

            //HDAdditionalCameraData lwrpCamData = go.AddComponent(typeof(HDAdditionalCameraData)) as HDAdditionalCameraData;
            //lwrpCamData.physicalParameters.
            //     LWRPAdditionalCameraData lwrpCamData =
            //          go.AddComponent(typeof(LWRPAdditionalCameraData)) as LWRPAdditionalCameraData;
            //     lwrpCamData.renderShadows = false; // turn off shadows for the reflection camera
            //      lwrpCamData.requiresColorOption = CameraOverrideOption.Off;
            //      lwrpCamData.requiresDepthOption = CameraOverrideOption.On;
            var reflectionCamera = go.GetComponent<Camera>();
            ////////reflectionCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
            //reflectionCamera.targetTexture = m_ReflectionTexture;

            
            //v0.1
            if (followCamera)// || Time.fixedTime < 1)
            {
                reflectionCamera.transform.position = currentCamera.transform.position + new Vector3(0, depthCameraHeight, 0);
            }
            else
            {
                reflectionCamera.transform.position = new Vector3(0 + DepthCameraInitPos.x,
                    depthCameraHeight, 0 + DepthCameraInitPos.y);
            }



            reflectionCamera.transform.forward = -Vector3.up;

            reflectionCamera.allowMSAA = currentCamera.allowMSAA;
            reflectionCamera.depth = renderDepth;// -10;
                                                 //     reflectionCamera.enabled = false;
            reflectionCamera.allowHDR = currentCamera.allowHDR;
            go.hideFlags = HideFlags.DontSave;

            return reflectionCamera;
        }

        //private int2 ReflectionResolution(Camera cam, float scale)
        //{
        //    var x = (int)(cam.pixelWidth * scale * GetScaleValue());
        //    var y = (int)(cam.pixelHeight * scale * GetScaleValue());
        //    return new int2(x, y);
        //}
        private Vector2 ReflectionResolution(Camera cam, float scale)
        {
            var x = (int)(cam.pixelWidth * scale * GetScaleValue());
            var y = (int)(cam.pixelHeight * scale * GetScaleValue());
            return new Vector2(x, y);
        }

        // public void ExecuteBeforeCameraRender(
        //     LightweightRenderPipeline pipelineInstance,
        //     ScriptableRenderContext context,
        //     Camera camera)
        RenderTexture m_ReflectionTextureDEPTH;
        int renderedDepth = -1;
        int max;
        float bias;
        //NEW

        void Update()
        {
            if (!Application.isPlaying && m_ReflectionCamera != null)
            {

                if (!useVolumeRenderer)
                {
                    m_ReflectionCamera.enabled = true;
                }
                else
                {
                    m_ReflectionCamera.enabled = false;
                }
                prevPosition = Camera.main.transform.position;
                m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                if (Materials.Count >= 0 && Materials[0] != null)
                {
                    for (int i = 0; i < Materials.Count; i++)
                    {
                        if (Materials[i] != null)
                        {
                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));

                            if (Materials[i].HasProperty("_BaseColorMap"))
                            {
                                Materials[i].SetTexture("_BaseColorMap", m_ReflectionTextureDEPTH);
                            }
                            else
                            {
                                Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                            }
                            if (passDepthCamPosOtherMats)//v0.1
                            {
                                Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                            }
                        }
                    }
                }
            }

        }

        Vector3 m_ReflectionCamera_transform_position;

        //v0.5
        public void DoDepthSM(ScriptableRenderContext context, Camera camera)
        {
            DoDepthSM2(camera);
        }
        //void Update ()//public void DoDepthSM(ScriptableRenderContext context, Camera camera)
        public void DoDepthSM2(Camera camera)//ScriptableRenderContext context, Camera camera) //v0.5
        {
            //if (!enabled)
            // return;
            //if (renderedDepth > 0)
            //{
            //    Debug.Log("renderedDepth:" + renderedDepth);
            //}
            camera = Camera.main;

            //GL.invertCulling = true; /////////////////////////// no cull in refractions
            // RenderSettings.fog = false;
            max = QualitySettings.maximumLODLevel;
            bias = QualitySettings.lodBias;
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.lodBias = bias * 0.5f;

            if (m_ReflectionCamera == null)
            {
                UpdateReflectionCamera(camera);
            }

            var res = ReflectionResolution(camera, 1);
            if (m_ReflectionTexture == null)
            {
                // m_ReflectionTexture = RenderTexture.GetTemporary(res.x, res.y, 16, RenderTextureFormat.DefaultHDR);
            }

            if (m_ReflectionTextureDEPTH == null)
            {
                // m_ReflectionTextureDEPTH = RenderTexture.GetTemporary(res.x, res.y, 24, RenderTextureFormat.Depth);
                //m_ReflectionTextureDEPTH = RenderTexture.GetTemporary(depthResolution, depthResolution, 24, RenderTextureFormat.Depth); //URP

                //m_ReflectionTextureDEPTH = new RenderTexture(depthResolution, depthResolution, 24, RenderTextureFormat.Depth);
                if (useScreenRes)
                {
                    m_ReflectionTextureDEPTH = new RenderTexture((int)(Screen.width * GetScaleValue()),
                        (int)(Screen.height * GetScaleValue()), 24, RenderTextureFormat.Depth);
                }
                else
                {
                    m_ReflectionTextureDEPTH = new RenderTexture(depthResolution, depthResolution, 24, RenderTextureFormat.Depth);
                }
            }

            if (!useVolumeRenderer)//v1.2
            {
                m_ReflectionCamera.targetTexture = m_ReflectionTextureDEPTH;// m_ReflectionTexture;
                m_ReflectionCamera.depthTextureMode = DepthTextureMode.Depth;
            }
            else
            {
                //if (volumeRendererDEPTH == null || !Application.isPlaying)
                //{
                //    //volumeRendererDEPTH = volumeRendererDEPTHOBJ.GetComponent<DepthRendererSM_HDRP_VOLUME>();
                //    CustomPassVolume volume = volumeRendererDEPTHOBJ.GetComponent<CustomPassVolume>();
                //    volumeRendererDEPTH = (DepthRendererCT_HDRP_VOLUME)volume.customPasses[passID];
                //}

                //WaterAirMaskSM tempMask;

                //if (volume.customPasses.TryGet<WaterAirMaskSM>(out tempMask))
                //{
                //    waterAirMask = tempMask;
                //    //Debug.Log("Got script");
                //}
                //Debug.Log("11");
                //volumeRendererDEPTH.targetTexture = m_ReflectionTextureDEPTH;
               // volumeRendererDEPTH.bakingCamera = m_ReflectionCamera;
                m_ReflectionCamera.enabled = false;
                m_ReflectionCamera.targetTexture = m_ReflectionTextureDEPTH;// m_ReflectionTexture;
                m_ReflectionCamera.depthTextureMode = DepthTextureMode.Depth;

                //v1.2a
                if (Time.fixedTime > 1 && (updateVolumePerDistance && updateDistance > 0) && Application.isPlaying)
                {
                    //do  distance based enable and disable

                    if (Vector3.Distance(Camera.main.transform.position, prevVolPosition) >= updateDistance)
                    {
                        prevVolPosition = Camera.main.transform.position;
                        enabledVolume = true;
                        //enable renderer
                        //volumeRendererDEPTH.render = true;
                        //pass camera postion now
                        m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                        //m_ReflectionCamera_transform_position = m_ReflectionCamera.transform.position;
                        //Debug.Log("Rendering depth");
                    }
                    else
                    {


                        if (enabledVolume)
                        {
                            if (Materials.Count >= 0 && Materials[0] != null)
                            {
                                for (int i = 0; i < Materials.Count; i++)
                                {
                                    if (Materials[i] != null)
                                    {
                                        Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                        if (passDepthCamPosOtherMats)//v0.1
                                        {
                                            Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, m_ReflectionCamera.transform.position.y, m_ReflectionCamera.transform.position.z, 1));
                                        }
                                        //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                                    }
                                }
                            }
                            enabledVolume = false;
                        }
                        else
                        {

                            //if (volumeRendererDEPTH.render)
                            //{
                            //    if (Materials.Count >= 0 && Materials[0] != null)
                            //    {
                            //        for (int i = 0; i < Materials.Count; i++)
                            //        {
                            //            if (Materials[i] != null)
                            //            {
                            //                Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                            //                Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, m_ReflectionCamera.transform.position.y, m_ReflectionCamera.transform.position.z, 1));
                            //                //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                            //            }
                            //        }
                            //    }
                            //}
                            //volumeRendererDEPTH.render = false;

                        }
                    }

                }
                else
                {
                    if (!renderOnce)
                    {
                        //m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                        if (Materials.Count >= 0 && Materials[0] != null)
                        {
                            for (int i = 0; i < Materials.Count; i++)
                            {
                                if (Materials[i] != null)
                                {
                                    Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                    if (passDepthCamPosOtherMats)
                                    {
                                        Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, m_ReflectionCamera.transform.position.y, m_ReflectionCamera.transform.position.z, 1));
                                    }
                                    //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                                }
                            }
                        }

                        //v0.1 - passDepthCamPosToMats
                        if (m_ReflectionCamera != null)
                        {
                            for (int i = 0; i < passDepthCamPosToMats.Count; i++)
                            {
                                //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);               
                                passDepthCamPosToMats[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                            }
                        }

                        m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                    }
                }
            }



            //   m_ReflectionCamera.SetTargetBuffers(m_ReflectionTexture.colorBuffer, m_ReflectionTextureDEPTH.depthBuffer);      
            //   m_ReflectionCamera.targetTexture = null;

            //new
            //m_ReflectionCamera.CopyFrom(camera); 


            //INFINIGRASS
            //       m_ReflectionCamera.farClipPlane = camera.farClipPlane;
            ///m_ReflectionCamera.transform.position
            m_ReflectionCamera.orthographic = true;
            //  m_ReflectionCamera.transform.position = new Vector3(camera.transform.position.x, depthCameraHeight, camera.transform.position.z);

            //m_ReflectionCamera.farClipPlane = depthCameraHeight + depthUnderWater;
            //m_ReflectionCamera.orthographicSize = depthCameraHeight + depthUnderWater;
            m_ReflectionCamera.nearClipPlane = depthCameraNearPlane;
            m_ReflectionCamera.farClipPlane = depthCameraFarPlane + depthUnderWater;
            m_ReflectionCamera.orthographicSize = depthCameraSize + depthUnderWater;

            // m_ReflectionCamera.projectionMatrix = camera.projectionMatrix;
            //m_ReflectionCamera.worldToCameraMatrix = camera.worldToCameraMatrix;
            // m_ReflectionCamera.transform.position = camera.transform.position + new Vector3(0,depthCameraHeight,0);
            m_ReflectionCamera.transform.forward = -Vector3.up; // m_ReflectionCamera.transform.rotation = camera.transform.rotation;

            //m_ReflectionCamera.clearFlags = CameraClearFlags.SolidColor;
            //LightweightRenderPipeline.RenderSingleCamera(pipelineInstance, context, m_ReflectionCamera);
            Vector3 prevPos = camera.transform.position;
            Quaternion prevPRot = camera.transform.rotation;

            //camera.transform.position = m_ReflectionCamera.transform.position;
            //camera.transform.rotation = m_ReflectionCamera.transform.rotation;

            //LightweightRenderPipeline.RenderSingleCamera(context, camera);
            //camera.transform.position = prevPos;
            //camera.transform.rotation = prevPRot;

            ////////////////////////////////////////////////////////     LightweightRenderPipeline.RenderSingleCamera(context, m_ReflectionCamera); //////////////////////////////////
            if (Time.fixedTime < 1 || (updatePerDistance && updateDistance > 0) || !Application.isPlaying || updateAlways)
            {

                if (Camera.main != null)
                {
                    if ((renderedDepth == -1 && Vector3.Distance(Camera.main.transform.position, prevPosition) > updateDistance) || !Application.isPlaying || Time.fixedTime < 1 || updateAlways)
                    {
                        if (!useVolumeRenderer)//v1.2
                        {
                            m_ReflectionCamera.enabled = true;
                        }
                        prevPosition = Camera.main.transform.position;
                        //m_ReflectionCamera.Render();
                        //Debug.Log("m_ReflectionCamera" + m_ReflectionCamera.gameObject.name + "enabled =" +m_ReflectionCamera.enabled + " at" + Camera.main.transform.position);
                        renderedDepth = framesForCameraRender;

                        //v0.1
                        if (followCamera)// || Time.fixedTime < 1)
                        {
                            m_ReflectionCamera.transform.position = new Vector3(camera.transform.position.x, depthCameraHeight, camera.transform.position.z);
                        }
                        else
                        {
                            m_ReflectionCamera.transform.position = new Vector3(0 + DepthCameraInitPos.x, 
                                depthCameraHeight, 0 + DepthCameraInitPos.y);
                        }

                        //v0.1
                        if (updateAlways && (followCamera || Time.fixedTime < 1 || !Application.isPlaying))
                        {
                            if (Materials.Count >= 0 && Materials[0] != null)
                            {
                                for (int i = 0; i < Materials.Count; i++)
                                {
                                    if (Materials[i] != null)
                                    {
                                        if (passDepthCamPosOtherMats)//v0.1
                                        {
                                            Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera_transform_position.x, depthCameraHeight, m_ReflectionCamera_transform_position.z, 1));
                                        }
                                    }
                                }
                            }

                            //v0.1 - passDepthCamPosToMats
                            if (m_ReflectionCamera != null)
                            {
                                for (int i = 0; i < passDepthCamPosToMats.Count; i++)
                                {              
                                    passDepthCamPosToMats[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                }
                            }
                        }


                        if (!Application.isPlaying || Time.fixedTime < 1 || updateAlways)
                        {
                            //Debug.Log("in1");
                            if (Materials.Count >= 0 && Materials[0] != null)
                            {
                                for (int i = 0; i < Materials.Count; i++)
                                {
                                    if (Materials[i] != null)
                                    {
                                        //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));

                                        if (Materials[i].HasProperty("_BaseColorMap"))
                                        {
                                            Materials[i].SetTexture("_BaseColorMap", m_ReflectionTextureDEPTH);
                                        }
                                        else
                                        {
                                            Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                        }
                                        // Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        if (renderedDepth > 0 && updateDistance > 0)
                        {
                            //Debug.Log("m_ReflectionCamera.enabled:" + m_ReflectionCamera.enabled + " at " + renderedDepth);
                            //apply to materials
                            if (renderedDepth == 2)
                            {
                                //save camera position at that point
                                m_ReflectionCamera_transform_position = m_ReflectionCamera.transform.position;
                            }
                            if (renderedDepth == 1)
                            {
                                if (Materials.Count >= 0 && Materials[0] != null)
                                {
                                    for (int i = 0; i < Materials.Count; i++)
                                    {
                                        if (Materials[i] != null)
                                        {
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                            //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                            // Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera_transform_position.x, depthCameraHeight, m_ReflectionCamera_transform_position.z, 1));
                                            //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                                        }
                                    }
                                }
                            }

                            if (renderedDepth == 1)
                            {
                                m_ReflectionCamera.enabled = false; //v0.2
                            }

                            renderedDepth--;
                            if (m_ReflectionCamera != null)
                            {
                                // m_ReflectionCamera.enabled = true;
                            }

                            if (renderedDepth == 0)
                            {
                                //GL.invertCulling = false;
                                //RenderSettings.fog = true;
                                //QualitySettings.maximumLODLevel = max;
                                // QualitySettings.lodBias = bias;
                                // Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTextureDEPTH);

                                //apply to materials
                                if (Materials.Count >= 0 && Materials[0] != null)
                                {
                                    for (int i = 0; i < Materials.Count; i++)
                                    {
                                        if (Materials[i] != null)
                                        {
                                            if (Materials[i].HasProperty("_BaseColorMap"))
                                            {
                                                Materials[i].SetTexture("_BaseColorMap", m_ReflectionTextureDEPTH);
                                            }
                                            else
                                            {
                                                Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                            }
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {

                            if (renderedDepth == 0)
                            {
                                if (Materials.Count >= 0 && Materials[0] != null)
                                {
                                    for (int i = 0; i < Materials.Count; i++)
                                    {
                                        if (Materials[i] != null)
                                        {
                                            // Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                            //Debug.Log("m_ReflectionCamera " + " at" + m_ReflectionCamera.transform.position);
                                        }
                                    }
                                }
                                // Debug.Log("m_ReflectionCamera.enabled:" + m_ReflectionCamera.enabled + " at " + renderedDepth);
                                m_ReflectionCamera.enabled = false;
                                renderedDepth = -1;

                                if (Materials.Count >= 0 && Materials[0] != null)
                                {
                                    for (int i = 0; i < Materials.Count; i++)
                                    {
                                        if (Materials[i] != null)
                                        {
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                            //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                            //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                            // Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                            if (passDepthCamPosOtherMats)//v0.1
                                            {
                                                Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera_transform_position.x, depthCameraHeight, m_ReflectionCamera_transform_position.z, 1));
                                            }
                                            //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                                        }
                                    }
                                }

                                //v0.1 - passDepthCamPosToMats
                                if (m_ReflectionCamera != null)
                                {
                                    for (int i = 0; i < passDepthCamPosToMats.Count; i++)
                                    {
                                        //Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);               
                                        passDepthCamPosToMats[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, depthCameraHeight, m_ReflectionCamera.transform.position.z, 1));
                                        //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                                    }
                                }

                            }
                        }
                    }
                }

            }
            else
            {

                //if (m_ReflectionCamera == null)
                {
                    // UpdateReflectionCamera(Camera.main);
                }

                if (!useVolumeRenderer)//v1.2
                {
                    m_ReflectionCamera.enabled = true;

                   

                    //v0.1
                    if (followCamera)// || Time.fixedTime < 1)
                    {
                        m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                    }
                    else
                    {
                        m_ReflectionCamera.transform.position = new Vector3(0 + DepthCameraInitPos.x,
                            depthCameraHeight, 0 + DepthCameraInitPos.y);
                    }


                    if (Materials.Count >= 0 && Materials[0] != null)
                    {
                        for (int i = 0; i < Materials.Count; i++)
                        {
                            if (Materials[i] != null)
                            {
                                Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                if (passDepthCamPosOtherMats)//v0.1
                                {
                                    Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                                }
                                //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                            }
                        }
                    }
                }
                //Debug.Log(Camera.main.transform.position);
            }
            //
            //if (1 == 0)
            //{
            //    if (testTex == null)
            //    {
            //        testTex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            //    }
            //    //Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            //    RenderTexture.active = m_ReflectionTexture;
            //    testTex.ReadPixels(new Rect(0, 0, m_ReflectionTexture.width, m_ReflectionTexture.height), 0, 0);
            //    testTex.Apply();
            //}

            //m_ReflectionCamera.tex
            //Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture.depthBuffer);
            //m_ReflectionTexture.de
        }

        void Update1()
        {

            //if (m_ReflectionCamera != null && m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>() != null)
            //{
            //    m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().volumeLayerMask = 0;
            //    m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().probeLayerMask = 0;
            //    Debug.Log("Removed 1");
            //}
            if ((Time.fixedTime < 1.5f || !Application.isPlaying))
            {
                if (m_ReflectionCamera == null)
                {
                    UpdateReflectionCamera(Camera.main);
                }
                if (!useVolumeRenderer)//v1.2
                {
                    m_ReflectionCamera.enabled = true;
                }

                if (Materials.Count >= 0 && Materials[0] != null)
                {
                    for (int i = 0; i < Materials.Count; i++)
                    {
                        if (Materials[i] != null)
                        {
                            Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                            if (passDepthCamPosOtherMats)//v0.1
                            {
                                Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                            }
                            //Materials[i].SetFloat("_TerrainScale", depthCameraHeight);
                        }
                    }
                }
            }


        }

        void OnGUI1()
        {
            testTex = toTexture2D(m_ReflectionTexture);
            if (testTex != null && Event.current.type.Equals(EventType.Repaint))
            {
                Graphics.DrawTexture(new Rect(10, 10, testTex.width, testTex.height), testTex);
                //Graphics.DrawTexture(new Rect(testTex.width + 10, 10, testTex2.width, testTex2.height), testTex2);
            }
        }
        public Texture2D testTex;

        Texture2D toTexture2D(RenderTexture rTex)
        {
            if (testTex == null)
            {
                testTex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            }
            //Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            RenderTexture.active = rTex;
            testTex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            testTex.Apply();
            return testTex;
        }
#endif
    }
}