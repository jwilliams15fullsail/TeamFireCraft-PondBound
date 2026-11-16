//#define HDRP
#define URP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Artngame.CommonTools.DepthRenderer;
using Artngame.CommonTools.FluidSim2D;

#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Artngame.RiverStudio {
    [ExecuteInEditMode]
    public class InfiniRIVER_Water_SETUP : MonoBehaviour
    {
        //PREFABS
        public bool insertPrefabs = false;
        public float insertAtHeight = 0;
        public GameObject depthRenderersPrefab;
        public GameObject waterCreatorPrefab;
        public GameObject fluidSimulatorPrefab;
        public GameObject rippleCausticsPrefab;

        public RippleCausticsGeneratorSM rippleGenerator;

        //URP - HDRP
        public bool HDRP_Mode = false;
        public bool URP_Mode = false;
        public GameObject depthRendererHDRP;//if exists, configure, if not add it first then configure
        public GameObject depthRendererURP;
        public Material ObstacleRenderMaterialHDRP;
        public Material WaterMaterialHDRP;
        public Material ObstacleRenderMaterialURP;
        public Material WaterMaterialURP;
#if HDRP

#endif
#if URP

#endif

        public Vector3 depthCameraPos = new Vector3(0,100,0);
        public float planeSize = 128; //depth camera must be set to cover plane width, size is size on scene after any scaling
        //public extendFactor =1 ; Extend depth camera beyond mesh if needed, must adjust FluidSize,e.g. if 2, must be 0.25,0.25,0.5,0.5
        public Camera depthCamera;
        public Material ObstacleRenderMaterial;
        public Material WaterMaterial;
        public OceanRenderer OceanRenderer;
        public FluidSim fluidSim;

        public bool enableCameraFollow = false;

        public bool enableWorldSpace = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        

        public bool setupWater = false;
        // Update is called once per frame
        void Update()
        {


            if (insertPrefabs)
            {
                Vector3 prefabsPlacement = new Vector3(0, insertAtHeight,0);
                if(OceanRenderer == null)
                {
                    GameObject ocean = Instantiate(waterCreatorPrefab, prefabsPlacement, Quaternion.identity);
                    OceanRenderer = ocean.GetComponent<OceanRenderer>();

                    //tile ripples
                    WaterMaterial.SetVector("_LocalWavesTiling", new Vector4(0.5f, 0.5f, -0.5f, -0.5f));
                    WaterMaterialHDRP.SetVector("_LocalWavesTiling", new Vector4(0.5f, 0.5f, -0.5f, -0.5f));
                    WaterMaterialURP.SetVector("_LocalWavesTiling", new Vector4(0.5f, 0.5f, -0.5f, -0.5f));
                }

                if (rippleGenerator == null)
                {
                    //ADD RIPPLE GEN PREFAB
                    GameObject ripples = Instantiate(rippleCausticsPrefab, prefabsPlacement, Quaternion.identity);
                    rippleGenerator = ripples.GetComponent<RippleCausticsGeneratorSM>();
                    ripples.transform.localScale = new Vector3(planeSize / 10, 1, planeSize / 10);
                    

                    //projector
                    ripples.GetComponentInChildren<Projector>(true).orthographicSize = planeSize / 2;
                    ripples.GetComponentInChildren<Projector>(true).gameObject.SetActive(true);
                    ripples.GetComponentInChildren<Projector>(true).gameObject.transform.eulerAngles = new Vector3(90, 360, -180);
                    ripples.transform.eulerAngles = new Vector3(0, 180, 0);
                }
                if (fluidSim == null)
                {
                    //ADD RIPPLE GEN PREFAB
                    GameObject fluidSimul = Instantiate(fluidSimulatorPrefab, Vector3.zero, Quaternion.identity);
                    fluidSim = fluidSimul.GetComponent<FluidSim>();
                }
                //bool createdDepthRenders = false;
                if (depthRendererURP == null || depthRendererHDRP == null || depthCamera == null)
                {
                    GameObject depthRendersOBJ = Instantiate(depthRenderersPrefab, Vector3.zero, Quaternion.identity);
                    depthRendersOBJ.transform.eulerAngles = new Vector3(90,0,0);
                    if (depthRendererURP == null)
                    {
                        depthRendererURP = depthRendersOBJ;
                    }
                    if (depthRendererHDRP == null)
                    {
                        depthRendererHDRP = depthRendersOBJ;                        
                    }
                    if (depthCamera == null)
                    {
                        depthCamera = depthRendersOBJ.GetComponent<Camera>();
                        depthCamera.transform.position = depthCameraPos;
                    }
                }

                insertPrefabs = false;
            }



            if (setupWater)
            {
                if(OceanRenderer != null && (
                    (   !URP_Mode && !HDRP_Mode && depthCamera != null && ObstacleRenderMaterial != null && WaterMaterial != null)
                        || (URP_Mode && depthRendererURP != null && ObstacleRenderMaterialURP != null && WaterMaterialURP != null)
                        || (HDRP_Mode && depthRendererHDRP != null && ObstacleRenderMaterialHDRP != null && WaterMaterialHDRP != null)
                    )
                    )
                {

                    //RIPPLES TILING
                    if (rippleGenerator != null)
                    {
                        rippleGenerator.gameObject.transform.localScale = new Vector3(planeSize / 10, 1, planeSize / 10);
                    }

                    //ASSIGN MATERIALS
                    if (URP_Mode)
                    {

                        //#if URP
                        //Enable Caustics decal projector
                        if (rippleGenerator != null)
                        {
                            rippleGenerator.gameObject.GetComponentInChildren<Projector>(true).gameObject.SetActive(false);
                            rippleGenerator.gameObject.GetComponentInChildren<BoxCollider>(true).gameObject.SetActive(true);
#if HDRP
                            rippleGenerator.gameObject.GetComponentInChildren<DecalProjector>(true).gameObject.SetActive(false);
#endif
                            rippleGenerator.gameObject.GetComponentInChildren<BoxCollider>(true).gameObject.transform.localEulerAngles = new Vector3(90, 360, -180);
                            rippleGenerator.gameObject.GetComponentInChildren<BoxCollider>(true).gameObject.transform.localPosition = new Vector3(0, -20, 0);
                            rippleGenerator.gameObject.GetComponentInChildren<BoxCollider>(true).gameObject.transform.localScale = new Vector3(10, 10, 40);
                            //#endif
                        }

                        fluidSim.m_obstaclesMat = ObstacleRenderMaterialURP;
                        OceanRenderer.gameObject.GetComponent<MeshRenderer>().sharedMaterial = WaterMaterialURP;
                        if(depthCamera != null)//DISABLE SRP Depth camera
                        {
                            depthCamera.enabled = false;
                            if (depthCamera.GetComponent<DepthRendererCT_SRP>() != null)
                            {
                                depthCamera.GetComponent<DepthRendererCT_SRP>().enabled = false;
                            }
                        }
                        if (depthRendererURP.GetComponent<DepthRendererCT_HDRP>() != null)//DISABLE HDRP Depth camera
                        {
                            if (depthRendererURP.GetComponent<DepthRendererCT_HDRP>() != null)
                            {
                                depthRendererURP.GetComponent<DepthRendererCT_HDRP>().enabled = false;
                            }
                        }
                        if (depthRendererHDRP.GetComponent<DepthRendererCT_URP>() != null)//ENABLE URP Depth camera
                        {                            
                            if (depthRendererHDRP.GetComponent<DepthRendererCT_URP>() != null)
                            {
                                depthRendererHDRP.GetComponent<DepthRendererCT_URP>().enabled = true;
                            }
                        }

                    }
                    else if (HDRP_Mode)
                    {

#if HDRP
                        //Enable HDRP Caustics decal projector 
                        if (rippleGenerator != null)
                        {
                            rippleGenerator.gameObject.GetComponentInChildren<Projector>(true).gameObject.SetActive(false);
                            rippleGenerator.gameObject.GetComponentInChildren<BoxCollider>(true).gameObject.SetActive(false);
                            rippleGenerator.gameObject.GetComponentInChildren<DecalProjector>(true).gameObject.SetActive(true);
                            rippleGenerator.gameObject.GetComponentInChildren<DecalProjector>(true).gameObject.transform.localEulerAngles = new Vector3(90, 360, -180);
                            rippleGenerator.gameObject.GetComponentInChildren<DecalProjector>(true).gameObject.transform.localPosition = new Vector3(0, -insertAtHeight / 2, 0);
                            rippleGenerator.gameObject.GetComponentInChildren<DecalProjector>(true).size = new Vector3(planeSize, planeSize, insertAtHeight);
                        }

#endif


                        fluidSim.m_obstaclesMat = ObstacleRenderMaterialHDRP;
                        OceanRenderer.gameObject.GetComponent<MeshRenderer>().sharedMaterial = WaterMaterialHDRP;

                        if (depthCamera != null)//DISABLE SRP Depth camera
                        {
                            depthCamera.enabled = false;
                            if (depthCamera.GetComponent<DepthRendererCT_SRP>() != null)
                            {
                                depthCamera.GetComponent<DepthRendererCT_SRP>().enabled = false;
                            }
                        }
                        if (depthRendererURP.GetComponent<DepthRendererCT_HDRP>() != null)//ENABLE HDRP Depth camera
                        {
                            if (depthRendererURP.GetComponent<DepthRendererCT_HDRP>() != null)
                            {
                                depthRendererURP.GetComponent<DepthRendererCT_HDRP>().enabled = true;
                            }
                        }
                        if (depthRendererHDRP.GetComponent<DepthRendererCT_URP>() != null)//DISABLE URP Depth camera
                        {
                            if (depthRendererHDRP.GetComponent<DepthRendererCT_URP>() != null)
                            {
                                depthRendererHDRP.GetComponent<DepthRendererCT_URP>().enabled = false;
                            }
                        }
                    }
                    else
                    {

                        //Enable Caustics decal projector
                        if (rippleGenerator != null)
                        {
                            rippleGenerator.gameObject.GetComponentInChildren<Projector>(true).gameObject.SetActive(true);
                            rippleGenerator.gameObject.GetComponentInChildren<BoxCollider>(true).gameObject.SetActive(false);
#if HDRP
                            rippleGenerator.gameObject.GetComponentInChildren<DecalProjector>(true).gameObject.SetActive(false);
#endif
                            rippleGenerator.gameObject.GetComponentInChildren<Projector>(true).gameObject.transform.localEulerAngles = new Vector3(90, 360, -180);
                        }
                        fluidSim.m_obstaclesMat = ObstacleRenderMaterial;
                        OceanRenderer.gameObject.GetComponent<MeshRenderer>().sharedMaterial = WaterMaterial;

                        if (depthCamera != null)//ENABLE SRP Depth camera
                        {
                            depthCamera.enabled = true;
                            if (depthCamera.GetComponent<DepthRendererCT_SRP>() != null)
                            {
                                depthCamera.GetComponent<DepthRendererCT_SRP>().enabled = true;
                            }
                        }
                        if (depthRendererURP.GetComponent<DepthRendererCT_HDRP>() != null)//DISABLE HDRP Depth camera
                        {
                            if (depthRendererURP.GetComponent<DepthRendererCT_HDRP>() != null)
                            {
                                depthRendererURP.GetComponent<DepthRendererCT_HDRP>().enabled = false;
                            }
                        }
                        if (depthRendererHDRP.GetComponent<DepthRendererCT_URP>() != null)//DISABLE URP Depth camera
                        {
                            if (depthRendererHDRP.GetComponent<DepthRendererCT_URP>() != null)
                            {
                                depthRendererHDRP.GetComponent<DepthRendererCT_URP>().enabled = false;
                            }
                        }
                    }

                    if (enableWorldSpace)
                    {

                        //tile ripples
                        //WaterMaterial.SetVector("_LocalWavesTiling", new Vector4(planeSize / 256, planeSize / 256, -0.5f, -0.5f));
                        //WaterMaterialHDRP.SetVector("_LocalWavesTiling", new Vector4(planeSize / 256, planeSize / 256, -0.5f, -0.5f));
                        //WaterMaterialURP.SetVector("_LocalWavesTiling", new Vector4(planeSize / 256, planeSize / 256, -0.5f, -0.5f));
                        WaterMaterial.SetVector("_LocalWavesTiling", new Vector4(0.5f, 0.5f, -0.5f, -0.5f));
                        WaterMaterialHDRP.SetVector("_LocalWavesTiling", new Vector4(0.5f, 0.5f, -0.5f, -0.5f));
                        WaterMaterialURP.SetVector("_LocalWavesTiling", new Vector4(0.5f, 0.5f, -0.5f, -0.5f));

                        OceanRenderer.useWorldPosition = true;
                        OceanRenderer.autoUpdateTerrScale = false;
                        OceanRenderer.TerrainScale = 0.93f / (planeSize / 128);
                        //depthCamera.orthographicSize = planeSize / 2;
                        ObstacleRenderMaterial.SetFloat("_ShoreFadeFactor", planeSize);
                        ObstacleRenderMaterial.SetVector("_DepthCameraPos", new Vector4(0, planeSize / 2, 0, 0));
                        WaterMaterial.SetVector("_DepthCameraPos", new Vector4(depthCamera.transform.position.x,
                            depthCamera.transform.position.y, depthCamera.transform.position.z, 1));
                        WaterMaterial.SetVector("FluidSize", new Vector4(0, 0, 1, 1));

                        ObstacleRenderMaterialURP.SetFloat("_ShoreFadeFactor", planeSize);
                        ObstacleRenderMaterialURP.SetVector("_DepthCameraPos", new Vector4(0, planeSize / 2, 0, 0));
                        WaterMaterialURP.SetVector("FluidSize", new Vector4(0, 0, 1, 1));

                        ObstacleRenderMaterialHDRP.SetFloat("_ShoreFadeFactor", planeSize);
                        ObstacleRenderMaterialHDRP.SetVector("_DepthCameraPos", new Vector4(0, planeSize / 2, 0, 0));
                        WaterMaterialHDRP.SetVector("FluidSize", new Vector4(0, 0, 1, 1));

                        if (URP_Mode)
                        {

#if URP
                            if (depthRendererURP == null)
                            {
                                //CREATE
                            }

                            if (depthRendererURP != null)
                            {
                                DepthRendererCT_URP depthRendURP = depthRendererURP.GetComponent<DepthRendererCT_URP>();
                                if (depthRendURP != null)
                                {
                                    //pass depth texture to obstacle material
                                    depthRendURP.passDepthCamPosOtherMats = false;//dont pass depth cam position
                                    depthRendURP.Materials.Clear();
                                    depthRendURP.Materials.Add(ObstacleRenderMaterialURP);
                                    depthRendURP.depthTextureName = "_MainTex";
                                 

                                    depthRendURP.depthCameraHeight = depthCameraPos.y;
                                    depthRendURP.depthCameraSize = planeSize / 2;

                                    //FOLLOW
                                    if (enableCameraFollow)
                                    {
                                        depthRendURP.passDepthCamPosToMats.Clear();
                                        depthRendURP.passDepthCamPosToMats.Add(WaterMaterialURP);
                                        depthRendURP.followCamera = true;
                                    }
                                    else
                                    {
                                        depthRendURP.passDepthCamPosToMats.Clear();
                                        depthRendURP.followCamera = false;
                                        depthRendURP.DepthCameraInitPos = new Vector2(depthCameraPos.x, depthCameraPos.z);
                                        WaterMaterialURP.SetVector("_DepthCameraPos", new Vector4(depthCameraPos.x, depthRendURP.depthCameraHeight, depthCameraPos.z, 1));
                                    }
                                }
                            }
#endif
                            ObstacleRenderMaterialURP.SetFloat("_TerrainScale", planeSize);
                        }
                        else if (HDRP_Mode)
                        {
#if HDRP
                            if (depthRendererHDRP == null)
                            {
                                //CREATE
                            }

                            if (depthRendererHDRP != null)
                            {
                                DepthRendererCT_HDRP depthRendHDRP = depthRendererHDRP.GetComponent<DepthRendererCT_HDRP>();
                                if (depthRendHDRP != null)
                                {

                                    //pass depth texture to obstacle material
                                    depthRendHDRP.passDepthCamPosOtherMats = false;//dont pass depth cam position
                                    depthRendHDRP.Materials.Clear();
                                    depthRendHDRP.Materials.Add(ObstacleRenderMaterialHDRP);
                                    depthRendHDRP.depthTextureName = "_MainTex";
                                    depthRendHDRP.depthUnderWater = 0;

                                    depthRendHDRP.depthCameraHeight = depthCameraPos.y;
                                    depthRendHDRP.depthCameraSize = planeSize / 2;

                                    //FOLLOW
                                    if (enableCameraFollow)
                                    {
                                        depthRendHDRP.passDepthCamPosToMats.Clear();
                                        depthRendHDRP.passDepthCamPosToMats.Add(WaterMaterialHDRP);
                                        depthRendHDRP.followCamera = true;
                                    }
                                    else
                                    {
                                        depthRendHDRP.passDepthCamPosToMats.Clear();
                                        depthRendHDRP.followCamera = false;
                                        depthRendHDRP.DepthCameraInitPos = new Vector2(depthCameraPos.x, depthCameraPos.z);
                                        WaterMaterialHDRP.SetVector("_DepthCameraPos", new Vector4(depthCameraPos.x, depthRendHDRP.depthCameraHeight, depthCameraPos.z, 1));
                                    }
                                    depthRendHDRP.DoDepthSM2(Camera.main);
                                }
                            }
#endif
                            ObstacleRenderMaterialHDRP.SetFloat("_TerrainScale", planeSize);
                           
                        }
                        else
                        {
                            depthCamera.orthographicSize = planeSize / 2;

                            depthCamera.enabled = true;
                            depthCamera.depthTextureMode = DepthTextureMode.Depth;
                            depthCamera.forceIntoRenderTexture = true;
                            depthCamera.Render();
                            if (enableCameraFollow)
                            {
                                depthCamera.GetComponent<DepthRendererCT_SRP>().DepthToMats.Add(WaterMaterial);
                                depthCamera.GetComponent<DepthRendererCT_SRP>().followCamera = true;
                            }
                            else
                            {
                                depthCamera.GetComponent<DepthRendererCT_SRP>().DepthToMats.Clear();
                                depthCamera.GetComponent<DepthRendererCT_SRP>().followCamera = false;
                                depthCamera.transform.position = new Vector3(depthCameraPos.x, depthCameraPos.y, depthCameraPos.z); ;
                                WaterMaterial.SetVector("_DepthCameraPos", new Vector4(depthCamera.transform.position.x,
                                depthCamera.transform.position.y, depthCamera.transform.position.z, 1));
                            }
                        }


                        if (fluidSim != null)
                        {
                            bool keepPrewarmState = fluidSim.prewarm;
                            fluidSim.Start();
                            fluidSim.prewarm = true;
                            fluidSim.LateUpdate();
                            fluidSim.prewarm = keepPrewarmState;
                        }
                        // depthCamera.
                        // depthCamera.GetComponent<DepthRendererCT_SRP>().
                        WaterMaterial.SetFloat("_TerrainScale", OceanRenderer.TerrainScale);
                        
                    }
                    else
                    {
                        //Ripples tiling
                        WaterMaterial.SetVector("_LocalWavesTiling", new Vector4(1f, 1f, -1f, -1f));
                        WaterMaterialHDRP.SetVector("_LocalWavesTiling", new Vector4(1f, 1f, -1f, -1f));
                        WaterMaterialURP.SetVector("_LocalWavesTiling", new Vector4(1f, 1f, -1f, -1));


                        OceanRenderer.useWorldPosition = false;                        
                        ObstacleRenderMaterial.SetFloat("_ShoreFadeFactor", planeSize);
                        ObstacleRenderMaterial.SetVector("_DepthCameraPos", new Vector4(0, planeSize/2, 0,0));
                        WaterMaterial.SetVector("FluidSize", new Vector4(0,0,1,1));

                        ObstacleRenderMaterialURP.SetFloat("_ShoreFadeFactor", planeSize);
                        ObstacleRenderMaterialURP.SetVector("_DepthCameraPos", new Vector4(0, planeSize / 2, 0, 0));
                        WaterMaterialURP.SetVector("FluidSize", new Vector4(0, 0, 1, 1));

                        ObstacleRenderMaterialHDRP.SetFloat("_ShoreFadeFactor", planeSize);
                        ObstacleRenderMaterialHDRP.SetVector("_DepthCameraPos", new Vector4(0, planeSize / 2, 0, 0));
                        WaterMaterialHDRP.SetVector("FluidSize", new Vector4(0, 0, 1, 1));

                        if (URP_Mode)
                        {
#if URP
                            if (depthRendererURP == null)
                            {
                                //CREATE
                            }

                            if (depthRendererURP != null)
                            {
                                DepthRendererCT_URP depthRendURP = depthRendererURP.GetComponent<DepthRendererCT_URP>();
                                if(depthRendURP != null)
                                {
                                    depthRendURP.depthCameraHeight = depthCameraPos.y;
                                    depthRendURP.depthCameraSize = planeSize/2;
                                }
                            }
#endif
                            ObstacleRenderMaterialURP.SetFloat("_TerrainScale", planeSize);
                        }
                        else if (HDRP_Mode)
                        {
#if HDRP
                            if (depthRendererHDRP == null)
                            {
                                //CREATE
                            }

                            if (depthRendererHDRP != null)
                            {
                                DepthRendererCT_HDRP depthRendHDRP = depthRendererHDRP.GetComponent<DepthRendererCT_HDRP>();
                                if (depthRendHDRP != null)
                                {
                                    depthRendHDRP.depthCameraHeight = depthCameraPos.y;
                                    depthRendHDRP.depthCameraSize = planeSize / 2;
                                    depthRendHDRP.depthUnderWater = 0;
                                    depthRendHDRP.DoDepthSM2(Camera.main);
                                }
                            }
#endif
                            ObstacleRenderMaterialHDRP.SetFloat("_TerrainScale", planeSize);
                        }
                        else
                        {
                            depthCamera.orthographicSize = planeSize / 2;
                        }
                    }
                }
                setupWater = false;
            }
        }
    }
}