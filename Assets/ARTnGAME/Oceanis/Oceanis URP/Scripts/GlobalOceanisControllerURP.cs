using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

//v0.7
using UnityEngine.Rendering.Universal;//v1.1.8n
using UnityEngine.Rendering;
using System.Reflection;

using UnityEngine.Serialization;
//using UnityEngine.Rendering.HighDefinition;

using Artngame.BrunetonsOcean;
using Artngame.BrunetonsAtmosphere;

namespace Artngame.Oceanis
{
    //v0.8
#if UNITY_EDITOR
    [InitializeOnLoadAttribute]
    public static class PlayModeStateChangedOCEANIS
    {
        // register an event handler when the class is initialized
        static PlayModeStateChangedOCEANIS()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }
        private static void LogPlayModeState(PlayModeStateChange state)
        {
            GlobalOceanisControllerURP ocean = GameObject.FindObjectOfType<GlobalOceanisControllerURP>();
            if (ocean != null && (state.ToString() == "ExitingEditMode"))// || state.ToString() == "ExitingPlayMode"))
            {
                if (ocean.cascade0 != null)
                {
                    ocean.cascade0.Dispose();
                }
                if (ocean.cascade1 != null)
                {
                    ocean.cascade1.Dispose();
                }
                if (ocean.cascade2 != null)
                {
                    ocean.cascade2.Dispose();
                }
                //ocean.WaveGenerator_Awake();
                //Debug.Log(state + " _aaa");
            }
            //Debug.Log(state);
        }
    }
#endif

    [ExecuteInEditMode]
    public class GlobalOceanisControllerURP : MonoBehaviour //GLOBAL OCEANIS CONTROLLER URP !!!
    {

        [Header("-------------------- Planar Waves Generator --------------------")]

        //v0.3
        public bool previewWater = true;
        public bool resetOceanisWater = false;

        //v0.2 - Choose ocean type
        public bool projectedGrid = true; //v0.7
        public bool createMeshesGrid = false;
        public bool doDepthRendering = false;

        public bool doReflections = true;

        //v0.4
        public Transform localLight;
        public Vector4 localLightProps = Vector4.zero;

        #region Planar Waves generator Variables
        //[SerializeField] 
        //WavesGenerator wavesGenerator;
        [SerializeField]
        Transform viewer;
        [SerializeField]
        Material oceanMaterial;
        [SerializeField]
        bool updateMaterialProperties;
        [SerializeField]
        bool showMaterialLods;

        [SerializeField]
        float lengthScale = 10;
        [SerializeField, Range(1, 40)]
        int vertexDensity = 30;
        [SerializeField, Range(0, 8)]
        int clipLevels = 8;
        [SerializeField, Range(0, 100)]
        float skirtSize = 50;

        List<Element> rings = new List<Element>();
        List<Element> trims = new List<Element>();
        Element center;
        Element skirt;
        Quaternion[] trimRotations;
        int previousVertexDensity;
        float previousSkirtSize;

        Material[] materials;
        #endregion


        // Cleanup all the objects we possibly have created
        private void OnDisable()
        {
            CleanupREFL();

            //Depth Renderer
            //if (doDepthRendering) //v1.4
            {
                Cleanup();
            }

            // Explicitly dispose of cascades on disable
            if (cascade0 != null)
            {
                cascade0.Dispose();
                cascade0 = null;
            }
            if (cascade1 != null)
            {
                cascade1.Dispose();
                cascade1 = null;
            }
            if (cascade2 != null)
            {
                cascade2.Dispose();
                cascade2 = null;
            }
        }

        private void OnDestroy()
        {
            CleanupREFL();

            //if (Application.isPlaying)
            {
                //Wave generator
                if (cascade0 != null)
                {
                    cascade0.Dispose();
                }
                if (cascade1 != null)
                {
                    cascade1.Dispose();
                }
                if (cascade2 != null)
                {
                    cascade2.Dispose();
                }
            }

            //Depth Renderer
            //if (doDepthRendering) //v1.4
            {
                Cleanup();
            }
        }

        //v0.3
        void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            resetOceanisWater = true;
        }

        //NEW
        //https://github.com/0lento/OcclusionProbes/blob/package/OcclusionProbes/OcclusionProbes.cs#L41
        private void OnEnable()
        {

            //v0.4
            if (savedPropsMaterial != null)
            {
                copyInnerToOceanMaterial();
            }
            else
            {
                if (savedPropsMaterial == null && oceanMaterial != null)
                {
                    Debug.Log("Creating new Inner Ocean Material");
                    savedPropsMaterial = new Material(oceanMaterial);
                    savedPropsMaterial.name = "Saved Ocean Props";
                }
            }

            //v0.3
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
#endif

            if (doReflections) //v1.4
            {
                OnEnableREFL();
            }

            //Depth Renderer
            if (doDepthRendering)
            {

                //v0.8a
                if (m_ReflectionCamera && prevRenderer != rendererID)
                {
                    UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
                    if (thisCameraData != null)
                    {
                        thisCameraData.SetRenderer(rendererID);
                        prevRenderer = rendererID;
                    }
                }

                RenderPipelineManager.beginCameraRendering += DoDepthSM;
                planarReflectionTextureID = Shader.PropertyToID(refractionTextureNameREFL);
                prevPosition = Camera.main.transform.position;
            }
        }
        //END NEW

        void Awake()
        {
            //v0.4
            if (savedPropsMaterial != null)
            {
                copyInnerToOceanMaterial();
            }
            else
            {
                if (savedPropsMaterial == null && oceanMaterial != null)
                {
                    Debug.Log("Creating new Inner Ocean Material");
                    savedPropsMaterial = new Material(oceanMaterial);
                    savedPropsMaterial.name = "Saved Ocean Props";
                }
            }

            //Wave generator
            if (Application.isPlaying)
            {
                WaveGenerator_Awake();
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                UnderWaterFX_Start();
            }
            //Wave generator
            //if (Application.isPlaying)
            //{
            //    WaveGenerator_Awake();
            //}
            StartREFL();

            ProjectedGrid_Start();

            ControlReflectDepthFX_Start();

            if (Application.isPlaying)
            {

                #region Planar Waves generator Start
                if (viewer == null)
                    viewer = Camera.main.transform;

                //oceanMaterial.SetTexture("_Displacement_c0", wavesGenerator.cascade0.Displacement);
                //oceanMaterial.SetTexture("_Derivatives_c0", wavesGenerator.cascade0.Derivatives);
                //oceanMaterial.SetTexture("_Turbulence_c0", wavesGenerator.cascade0.Turbulence);

                //oceanMaterial.SetTexture("_Displacement_c1", wavesGenerator.cascade1.Displacement);
                //oceanMaterial.SetTexture("_Derivatives_c1", wavesGenerator.cascade1.Derivatives);
                //oceanMaterial.SetTexture("_Turbulence_c1", wavesGenerator.cascade1.Turbulence);

                //oceanMaterial.SetTexture("_Displacement_c2", wavesGenerator.cascade2.Displacement);
                //oceanMaterial.SetTexture("_Derivatives_c2", wavesGenerator.cascade2.Derivatives);
                //oceanMaterial.SetTexture("_Turbulence_c2", wavesGenerator.cascade2.Turbulence);

                oceanMaterial.SetTexture("_Displacement_c0", cascade0.Displacement);
                oceanMaterial.SetTexture("_Derivatives_c0", cascade0.Derivatives);
                oceanMaterial.SetTexture("_Turbulence_c0", cascade0.Turbulence);

                oceanMaterial.SetTexture("_Displacement_c1", cascade1.Displacement);
                oceanMaterial.SetTexture("_Derivatives_c1", cascade1.Derivatives);
                oceanMaterial.SetTexture("_Turbulence_c1", cascade1.Turbulence);

                oceanMaterial.SetTexture("_Displacement_c2", cascade2.Displacement);
                oceanMaterial.SetTexture("_Derivatives_c2", cascade2.Derivatives);
                oceanMaterial.SetTexture("_Turbulence_c2", cascade2.Turbulence);

                materials = new Material[3];
                materials[0] = new Material(oceanMaterial);
                materials[0].EnableKeyword("CLOSE");

                materials[1] = new Material(oceanMaterial);
                materials[1].EnableKeyword("MID");
                materials[1].DisableKeyword("CLOSE");

                materials[2] = new Material(oceanMaterial);
                materials[2].DisableKeyword("MID");
                materials[2].DisableKeyword("CLOSE");

                trimRotations = new Quaternion[]
                {
            Quaternion.AngleAxis(180, Vector3.up),
            Quaternion.AngleAxis(90, Vector3.up),
            Quaternion.AngleAxis(270, Vector3.up),
            Quaternion.identity,
                };

                //v0.2
                if (!projectedGrid && createMeshesGrid)
                {
                    InstantiateMeshes();
                }

                //v0.8
                //float timers = 0;
                //for (int i = 0; i < 120; i++)
                //{
                //    timers = timers + 0.01f;
                //    cascade0.CalculateWavesAtTime(timers);
                //    cascade1.CalculateWavesAtTime(timers);
                //    cascade2.CalculateWavesAtTime(timers);
                //}
                ////RequestReadbacks();
                //cascade0.CalculateWavesAtTime(Time.time);
                //cascade1.CalculateWavesAtTime(Time.time);
                //cascade2.CalculateWavesAtTime(Time.time);
            }

            //v0.1
            //UpdateMaterials();
            #endregion

            #region Depth Renderer Start
            if (1 == 1 && doDepthRendering)
            {
                prevPosition = Camera.main.transform.position;
                prevVolPosition = Camera.main.transform.position;
                renderedDepth = -1;

                if (clearAtStart) //use for full refraction case
                {
                    if (m_ReflectionCamera)
                    {
                        m_ReflectionCamera.targetTexture = null;
                        SafeDestroy(m_ReflectionCamera.gameObject);
                    }

                    RenderPipelineManager.beginCameraRendering -= DoDepthSM;
                    Cleanup();
                    RenderPipelineManager.beginCameraRendering += DoDepthSM;

                    planarReflectionTextureID = Shader.PropertyToID(refractionTextureName);

                    //v0.5
                    Update();
                    DoDepthSM2(Camera.main);
                }
            }
            #endregion

        }

        private void Update()
        {

            if(Camera.main != null)
            {
                //Debug.Log("Changed camera rot 1  " + Camera.main.transform.eulerAngles.y);
                if (Mathf.Abs(Camera.main.transform.eulerAngles.y) == 180)
                {
                    Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, 180.01f, Camera.main.transform.eulerAngles.z);
                }
                if (Mathf.Abs(Camera.main.transform.eulerAngles.y) < 0.01f)
                {
                    //Debug.Log("Changed camera rot");
                    Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, 0.01f, Camera.main.transform.eulerAngles.z);
                }
                
            }

            loadOceanMaterial();
            updateSpecular();

            if (Application.isPlaying)
            {
                UnderWaterFX_Update();
            }

            UpdateREFL();

            ProjectedGrid_Update();

            ControlReflectDepthFX_Update();

            WaterBaseSM_Update();

            UpdateGernstner();

            //Wave generator
            if (Application.isPlaying || previewWater) //v0.3
            {
                //v0.3
                if (cascade0 == null || resetOceanisWater) //v0.3
                {
                    resetOceanisWater = false; //v0.3

                    WaveGenerator_Awake();

                    //float boundary1 = 2 * Mathf.PI / lengthScale1 * 6f;
                    //float boundary2 = 2 * Mathf.PI / lengthScale2 * 6f;
                    //cascade0.CalculateInitials(wavesSettings, lengthScale0, 0.0001f, boundary1);
                    //cascade1.CalculateInitials(wavesSettings, lengthScale1, boundary1, boundary2);
                    //cascade2.CalculateInitials(wavesSettings, lengthScale2, boundary2, 9999);

                    //Start();
                    oceanMaterial.SetTexture("_Displacement_c0", cascade0.Displacement);
                    oceanMaterial.SetTexture("_Derivatives_c0", cascade0.Derivatives);
                    oceanMaterial.SetTexture("_Turbulence_c0", cascade0.Turbulence);

                    oceanMaterial.SetTexture("_Displacement_c1", cascade1.Displacement);
                    oceanMaterial.SetTexture("_Derivatives_c1", cascade1.Derivatives);
                    oceanMaterial.SetTexture("_Turbulence_c1", cascade1.Turbulence);

                    oceanMaterial.SetTexture("_Displacement_c2", cascade2.Displacement);
                    oceanMaterial.SetTexture("_Derivatives_c2", cascade2.Derivatives);
                    oceanMaterial.SetTexture("_Turbulence_c2", cascade2.Turbulence);

                    materials = new Material[3];
                    materials[0] = new Material(oceanMaterial);
                    materials[0].EnableKeyword("CLOSE");

                    materials[1] = new Material(oceanMaterial);
                    materials[1].EnableKeyword("MID");
                    materials[1].DisableKeyword("CLOSE");

                    materials[2] = new Material(oceanMaterial);
                    materials[2].DisableKeyword("MID");
                    materials[2].DisableKeyword("CLOSE");

                    trimRotations = new Quaternion[]
                    {
                    Quaternion.AngleAxis(180, Vector3.up),
                    Quaternion.AngleAxis(90, Vector3.up),
                    Quaternion.AngleAxis(270, Vector3.up),
                    Quaternion.identity,
                    };

                    //v0.2
                    if (!projectedGrid && createMeshesGrid)
                    {
                        InstantiateMeshes();
                    }

                    //v0.8
                    //float timers = 0;
                    //for (int i = 0; i < 120; i++)
                    //{
                    //    timers = timers + 0.01f;
                    //    cascade0.CalculateWavesAtTime(timers);
                    //    cascade1.CalculateWavesAtTime(timers);
                    //    cascade2.CalculateWavesAtTime(timers);
                    //}
                    ////RequestReadbacks();
                    //cascade0.CalculateWavesAtTime(Time.time);
                    //cascade1.CalculateWavesAtTime(Time.time);
                    //cascade2.CalculateWavesAtTime(Time.time);
                }

                if (cascade0 != null) //v0.8
                {
                    //v1.4
                    if (updateFFTperTime == 0 || (updateFFTperTime > 0 && Time.fixedTime - prevUpdateTime > updateFFTperTime))
                    {
                        prevUpdateTime = Time.fixedTime;//v1.4

                        WaveGenerator_Update();
                    }
                }
            }

            if (updateSky)
            {
                UpdateSky();
            }

            if (Application.isPlaying)
            {

                #region Planar Waves generator UPDATE
                //v0.2
                if (!projectedGrid)
                {
                    //v0.2
                    oceanMaterial.SetFloat("enableProjectedGrid", 0);//remove projected grid flag            

                    if (createMeshesGrid)
                    {
                        if (rings.Count != clipLevels || trims.Count != clipLevels
                    || previousVertexDensity != vertexDensity || !Mathf.Approximately(previousSkirtSize, skirtSize))
                        {
                            InstantiateMeshes();
                            previousVertexDensity = vertexDensity;
                            previousSkirtSize = skirtSize;
                        }

                        UpdatePositions();
                        UpdateMaterials();
                    }
                    //v0.2
                    //register the materials to the depth provider !!! - materials[0] - materials[1] - materials[2]

                }
                else
                {
                    //v0.2
                    oceanMaterial.SetFloat("enableProjectedGrid", 1);//add projected grid flag
                }
                #endregion

            }

            #region Depth Renderer UPDATE
            if (doDepthRendering && !disableDepthRendering)
            {
                DepthRendererUPDATE();
            }
            #endregion
        }

        #region Planar Waves generator

        void UpdateMaterials()
        {
            if (updateMaterialProperties && !showMaterialLods)
            {
                for (int i = 0; i < 3; i++)
                {
                    materials[i].CopyPropertiesFromMaterial(oceanMaterial);
                }
                materials[0].EnableKeyword("CLOSE");
                materials[1].EnableKeyword("MID");
                materials[1].DisableKeyword("CLOSE");
                materials[2].DisableKeyword("MID");
                materials[2].DisableKeyword("CLOSE");
            }
            if (showMaterialLods)
            {
                materials[0].SetColor("_Color", Color.red * 0.6f);
                materials[1].SetColor("_Color", Color.green * 0.6f);
                materials[2].SetColor("_Color", Color.blue * 0.6f);
            }

            int activeLevels = ActiveLodlevels();
            center.MeshRenderer.material = GetMaterial(clipLevels - activeLevels - 1);

            for (int i = 0; i < rings.Count; i++)
            {
                rings[i].MeshRenderer.material = GetMaterial(clipLevels - activeLevels + i);
                trims[i].MeshRenderer.material = GetMaterial(clipLevels - activeLevels + i);
            }
        }

        Material GetMaterial(int lodLevel)
        {
            if (lodLevel - 2 <= 0)
                return materials[0];

            if (lodLevel - 2 <= 2)
                return materials[1];

            return materials[2];
        }

        void UpdatePositions()
        {
            int k = GridSize();
            int activeLevels = ActiveLodlevels();

            float scale = ClipLevelScale(-1, activeLevels);
            Vector3 previousSnappedPosition = Snap(viewer.position, scale * 2);
            center.Transform.position = previousSnappedPosition + OffsetFromCenter(-1, activeLevels);
            center.Transform.localScale = new Vector3(scale, 1, scale);

            for (int i = 0; i < clipLevels; i++)
            {
                rings[i].Transform.gameObject.SetActive(i < activeLevels);
                trims[i].Transform.gameObject.SetActive(i < activeLevels);
                if (i >= activeLevels) continue;

                scale = ClipLevelScale(i, activeLevels);
                Vector3 centerOffset = OffsetFromCenter(i, activeLevels);
                Vector3 snappedPosition = Snap(viewer.position, scale * 2);

                Vector3 trimPosition = centerOffset + snappedPosition + scale * (k - 1) / 2 * new Vector3(1, 0, 1);
                int shiftX = previousSnappedPosition.x - snappedPosition.x < float.Epsilon ? 1 : 0;
                int shiftZ = previousSnappedPosition.z - snappedPosition.z < float.Epsilon ? 1 : 0;
                trimPosition += shiftX * (k + 1) * scale * Vector3.right;
                trimPosition += shiftZ * (k + 1) * scale * Vector3.forward;
                trims[i].Transform.position = trimPosition;
                trims[i].Transform.rotation = trimRotations[shiftX + 2 * shiftZ];
                trims[i].Transform.localScale = new Vector3(scale, 1, scale);

                rings[i].Transform.position = snappedPosition + centerOffset;
                rings[i].Transform.localScale = new Vector3(scale, 1, scale);
                previousSnappedPosition = snappedPosition;
            }

            scale = lengthScale * 2 * Mathf.Pow(2, clipLevels);
            skirt.Transform.position = new Vector3(-1, 0, -1) * scale * (skirtSize + 0.5f - 0.5f / GridSize()) + previousSnappedPosition;
            skirt.Transform.localScale = new Vector3(scale, 1, scale);
        }

        int ActiveLodlevels()
        {
            return clipLevels - Mathf.Clamp((int)Mathf.Log((1.7f * Mathf.Abs(viewer.position.y) + 1) / lengthScale, 2), 0, clipLevels);
        }

        float ClipLevelScale(int level, int activeLevels)
        {
            return lengthScale / GridSize() * Mathf.Pow(2, clipLevels - activeLevels + level + 1);
        }

        Vector3 OffsetFromCenter(int level, int activeLevels)
        {
            return (Mathf.Pow(2, clipLevels) + GeometricProgressionSum(2, 2, clipLevels - activeLevels + level + 1, clipLevels - 1))
                   * lengthScale / GridSize() * (GridSize() - 1) / 2 * new Vector3(-1, 0, -1);
        }

        float GeometricProgressionSum(float b0, float q, int n1, int n2)
        {
            return b0 / (1 - q) * (Mathf.Pow(q, n2) - Mathf.Pow(q, n1));
        }

        int GridSize()
        {
            return 4 * vertexDensity + 1;
        }

        Vector3 Snap(Vector3 coords, float scale)
        {
            if (coords.x >= 0)
                coords.x = Mathf.Floor(coords.x / scale) * scale;
            else
                coords.x = Mathf.Ceil((coords.x - scale + 1) / scale) * scale;

            if (coords.z < 0)
                coords.z = Mathf.Floor(coords.z / scale) * scale;
            else
                coords.z = Mathf.Ceil((coords.z - scale + 1) / scale) * scale;

            coords.y = 0;
            return coords;
        }

        void InstantiateMeshes()
        {
            foreach (var child in gameObject.GetComponentsInChildren<Transform>())
            {
                if (child != transform)
                    Destroy(child.gameObject);
            }
            rings.Clear();
            trims.Clear();

            int k = GridSize();
            center = InstantiateElement("Center", CreatePlaneMesh(2 * k, 2 * k, 1, Seams.All), materials[materials.Length - 1]);
            Mesh ring = CreateRingMesh(k, 1);
            Mesh trim = CreateTrimMesh(k, 1);
            for (int i = 0; i < clipLevels; i++)
            {
                rings.Add(InstantiateElement("Ring " + i, ring, materials[materials.Length - 1]));
                trims.Add(InstantiateElement("Trim " + i, trim, materials[materials.Length - 1]));
            }
            skirt = InstantiateElement("Skirt", CreateSkirtMesh(k, skirtSize), materials[materials.Length - 1]);
        }

        Element InstantiateElement(string name, Mesh mesh, Material mat)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = true;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            meshRenderer.material = mat;
            meshRenderer.allowOcclusionWhenDynamic = false;
            return new Element(go.transform, meshRenderer);
        }

        Mesh CreateSkirtMesh(int k, float outerBorderScale)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap skirt";
            CombineInstance[] combine = new CombineInstance[8];

            Mesh quad = CreatePlaneMesh(1, 1, 1);
            Mesh hStrip = CreatePlaneMesh(k, 1, 1);
            Mesh vStrip = CreatePlaneMesh(1, k, 1);


            Vector3 cornerQuadScale = new Vector3(outerBorderScale, 1, outerBorderScale);
            Vector3 midQuadScaleVert = new Vector3(1f / k, 1, outerBorderScale);
            Vector3 midQuadScaleHor = new Vector3(outerBorderScale, 1, 1f / k);

            combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, cornerQuadScale);
            combine[0].mesh = quad;

            combine[1].transform = Matrix4x4.TRS(Vector3.right * outerBorderScale, Quaternion.identity, midQuadScaleVert);
            combine[1].mesh = hStrip;

            combine[2].transform = Matrix4x4.TRS(Vector3.right * (outerBorderScale + 1), Quaternion.identity, cornerQuadScale);
            combine[2].mesh = quad;

            combine[3].transform = Matrix4x4.TRS(Vector3.forward * outerBorderScale, Quaternion.identity, midQuadScaleHor);
            combine[3].mesh = vStrip;

            combine[4].transform = Matrix4x4.TRS(Vector3.right * (outerBorderScale + 1)
                + Vector3.forward * outerBorderScale, Quaternion.identity, midQuadScaleHor);
            combine[4].mesh = vStrip;

            combine[5].transform = Matrix4x4.TRS(Vector3.forward * (outerBorderScale + 1), Quaternion.identity, cornerQuadScale);
            combine[5].mesh = quad;

            combine[6].transform = Matrix4x4.TRS(Vector3.right * outerBorderScale
                + Vector3.forward * (outerBorderScale + 1), Quaternion.identity, midQuadScaleVert);
            combine[6].mesh = hStrip;

            combine[7].transform = Matrix4x4.TRS(Vector3.right * (outerBorderScale + 1)
                + Vector3.forward * (outerBorderScale + 1), Quaternion.identity, cornerQuadScale);
            combine[7].mesh = quad;
            mesh.CombineMeshes(combine, true);
            return mesh;
        }

        Mesh CreateTrimMesh(int k, float lengthScale)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap trim";
            CombineInstance[] combine = new CombineInstance[2];

            combine[0].mesh = CreatePlaneMesh(k + 1, 1, lengthScale, Seams.None, 1);
            combine[0].transform = Matrix4x4.TRS(new Vector3(-k - 1, 0, -1) * lengthScale, Quaternion.identity, Vector3.one);

            combine[1].mesh = CreatePlaneMesh(1, k, lengthScale, Seams.None, 1);
            combine[1].transform = Matrix4x4.TRS(new Vector3(-1, 0, -k - 1) * lengthScale, Quaternion.identity, Vector3.one);

            mesh.CombineMeshes(combine, true);
            return mesh;
        }

        Mesh CreateRingMesh(int k, float lengthScale)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap ring";
            if ((2 * k + 1) * (2 * k + 1) >= 256 * 256)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            CombineInstance[] combine = new CombineInstance[4];

            combine[0].mesh = CreatePlaneMesh(2 * k, (k - 1) / 2, lengthScale, Seams.Bottom | Seams.Right | Seams.Left);
            combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

            combine[1].mesh = CreatePlaneMesh(2 * k, (k - 1) / 2, lengthScale, Seams.Top | Seams.Right | Seams.Left);
            combine[1].transform = Matrix4x4.TRS(new Vector3(0, 0, k + 1 + (k - 1) / 2) * lengthScale, Quaternion.identity, Vector3.one);

            combine[2].mesh = CreatePlaneMesh((k - 1) / 2, k + 1, lengthScale, Seams.Left);
            combine[2].transform = Matrix4x4.TRS(new Vector3(0, 0, (k - 1) / 2) * lengthScale, Quaternion.identity, Vector3.one);

            combine[3].mesh = CreatePlaneMesh((k - 1) / 2, k + 1, lengthScale, Seams.Right);
            combine[3].transform = Matrix4x4.TRS(new Vector3(k + 1 + (k - 1) / 2, 0, (k - 1) / 2) * lengthScale, Quaternion.identity, Vector3.one);

            mesh.CombineMeshes(combine, true);
            return mesh;
        }

        Mesh CreatePlaneMesh(int width, int height, float lengthScale, Seams seams = Seams.None, int trianglesShift = 0)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap plane";
            if ((width + 1) * (height + 1) >= 256 * 256)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
            int[] triangles = new int[width * height * 2 * 3];
            Vector3[] normals = new Vector3[(width + 1) * (height + 1)];

            for (int i = 0; i < height + 1; i++)
            {
                for (int j = 0; j < width + 1; j++)
                {
                    int x = j;
                    int z = i;

                    if ((i == 0 && seams.HasFlag(Seams.Bottom)) || (i == height && seams.HasFlag(Seams.Top)))
                        x = x / 2 * 2;
                    if ((j == 0 && seams.HasFlag(Seams.Left)) || (j == width && seams.HasFlag(Seams.Right)))
                        z = z / 2 * 2;

                    vertices[j + i * (width + 1)] = new Vector3(x, 0, z) * lengthScale;
                    normals[j + i * (width + 1)] = Vector3.up;
                }
            }

            int tris = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = j + i * (width + 1);
                    if ((i + j + trianglesShift) % 2 == 0)
                    {
                        triangles[tris++] = k;
                        triangles[tris++] = k + width + 1;
                        triangles[tris++] = k + width + 2;

                        triangles[tris++] = k;
                        triangles[tris++] = k + width + 2;
                        triangles[tris++] = k + 1;
                    }
                    else
                    {
                        triangles[tris++] = k;
                        triangles[tris++] = k + width + 1;
                        triangles[tris++] = k + 1;

                        triangles[tris++] = k + 1;
                        triangles[tris++] = k + width + 1;
                        triangles[tris++] = k + width + 2;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            return mesh;
        }

        class Element
        {
            public Transform Transform;
            public MeshRenderer MeshRenderer;

            public Element(Transform transform, MeshRenderer meshRenderer)
            {
                Transform = transform;
                MeshRenderer = meshRenderer;
            }
        }


        [System.Flags]
        enum Seams
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
            All = Left | Right | Top | Bottom
        };

        #endregion

       

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Depth Renderer ----------------------")]
        #region Depth Renderer
        //v1.2 - optionally setup volume depth renderer with depth camera and grab texture from there
        public bool useVolumeRenderer = false;
        public GameObject volumeRendererDEPTHOBJ; ///DepthRendererSM_HDRP_VOLUME volumeRendererDEPTH;
        //DepthRendererSM_HDRP_VOLUME volumeRendererDEPTH;
        public int passID = 0;
        public bool updateVolumePerDistance = false;
        public Vector3 prevVolPosition;
        bool enabledVolume = false;
        public bool renderOnce = true; //render only one time in non distance based update

        //v1.2 - disable after start
        public bool disableAfterStart = false;
        public bool disableDepthRendering = false;
        private void LateUpdate()
        {
            if (disableAfterStart && Application.isPlaying)
            {
                //this.enabled = false;
                disableDepthRendering = true;
            }
        }

        public int renderDepth = -16;
        public bool updatePerDistance = true;
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
        [System.Serializable]
        public class PlanarReflectionSettings
        {
            public ResolutionMulltiplier m_ResolutionMultiplier = ResolutionMulltiplier.Third;
            public float m_ClipPlaneOffset = 0.07f;
            public LayerMask m_ReflectLayers = -1;
        }

        public List<Material> Materials = new List<Material>();
        public string depthTextureName = "_ShoreContourTex";//"_TopDownDepthTexture";

        [SerializeField]
        public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();

        public GameObject target;
        [FormerlySerializedAs("camOffset")] public float m_planeOffset;

        //private static Camera m_ReflectionCamera;
        public static Camera m_ReflectionCamera;
        private RenderTexture m_ReflectionTexture = null;
        private int planarReflectionTextureID;
        public string refractionTextureName = "_ShoreContourTex";
        public float depthCameraHeight = 1000;

        public bool clearAtStart = false;

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
            RenderPipelineManager.beginCameraRendering -= DoDepthSM;
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

        private void UpdateReflectionCamera(Camera realCamera)
        {

            planarReflectionTextureID = Shader.PropertyToID(refractionTextureName); //////////// reset name if changed

            if (m_ReflectionCamera == null)
            {
                m_ReflectionCamera = CreateMirrorObjects(realCamera);
            }

            //v0.8
            if (m_ReflectionCamera && prevRenderer != rendererID)
            {
                UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
                if (thisCameraData != null)
                {
                    //Debug.Log("DE");
                    thisCameraData.SetRenderer(rendererID);
                    prevRenderer = rendererID;
                }
                else
                {
                    UniversalAdditionalCameraData thisCameraDatA = m_ReflectionCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                    thisCameraDatA.SetRenderer(rendererID);
                    prevRenderer = rendererID;
                }
            }

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

            Vector4 clipPlane = CameraSpacePlane(m_ReflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
            Matrix4x4 projection = realCamera.CalculateObliqueMatrix(clipPlane);
            //      m_ReflectionCamera.projectionMatrix = projection;
            m_ReflectionCamera.cullingMask = m_settings.m_ReflectLayers; // never render water layer

            //HDRP   
            m_ReflectionCamera.clearFlags = CameraClearFlags.Nothing;
            //if (m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>() != null)
            //{
            //    m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().volumeLayerMask = 0;
            //    m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().probeLayerMask = 0;
            //    //Debug.Log("Removed 2");
            //}
            //else
            //{
            //    m_ReflectionCamera.gameObject.AddComponent<HDAdditionalCameraData>().volumeLayerMask = 0; //ADD the HDAdditionalCameraData here, as they are only added in editor if camera selected !!!!
            //    m_ReflectionCamera.gameObject.GetComponent<HDAdditionalCameraData>().probeLayerMask = 0;
            //    // Debug.Log("Removed 3");
            //}
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

            var reflectionCamera = go.GetComponent<Camera>();

            reflectionCamera.transform.position = currentCamera.transform.position + new Vector3(0, depthCameraHeight, 0);
            reflectionCamera.transform.forward = -Vector3.up;

            reflectionCamera.allowMSAA = currentCamera.allowMSAA;
            reflectionCamera.depth = renderDepth;// -10;
                                                 //     reflectionCamera.enabled = false;
            reflectionCamera.allowHDR = currentCamera.allowHDR;
            go.hideFlags = HideFlags.DontSave;

            return reflectionCamera;
        }

        private Vector2 ReflectionResolution(Camera cam, float scale)
        {
            var x = (int)(cam.pixelWidth * scale * GetScaleValue());
            var y = (int)(cam.pixelHeight * scale * GetScaleValue());
            return new Vector2(x, y);
        }

        RenderTexture m_ReflectionTextureDEPTH;
        int renderedDepth = -1;
        int max;
        float bias;
        //NEW

        void DepthRendererUPDATE()
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
                            Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                        }
                    }
                }
            }
        }

        Vector3 m_ReflectionCamera_transform_position;

        //v0.5
        public void DoDepthSM(ScriptableRenderContext context, Camera camera)
        {
            if (!disableDepthRendering)
            {
                DoDepthSM2(camera);
            }

            //v0.9
            if (Materials.Count >= 0 && Materials[0] != null)
            {
                for (int i = 0; i < Materials.Count; i++)
                {
                    if (Materials[i] != null)
                    {
                        Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                        //Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                    }
                }
            }
        }

        public void DoDepthSM2(Camera camera)//ScriptableRenderContext context, Camera camera) //v0.5
        {
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

            if (m_ReflectionTextureDEPTH == null)
            {
                m_ReflectionTextureDEPTH = new RenderTexture((int)(Screen.width * GetScaleValue()), (int)(Screen.height * GetScaleValue()), 24, RenderTextureFormat.Depth);
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
                //    CustomPassVolume volume = volumeRendererDEPTHOBJ.GetComponent<CustomPassVolume>();
                //    volumeRendererDEPTH = (DepthRendererSM_HDRP_VOLUME)volume.customPasses[passID];
                //}
                //volumeRendererDEPTH.targetTexture = m_ReflectionTextureDEPTH;
                //volumeRendererDEPTH.bakingCamera = m_ReflectionCamera;
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
                                        Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, m_ReflectionCamera.transform.position.y, m_ReflectionCamera.transform.position.z, 1));
                                    }
                                }
                            }
                            enabledVolume = false;
                        }
                        else
                        {
                            //volumeRendererDEPTH.render = false;
                        }
                    }
                }
                else
                {
                    if (!renderOnce)
                    {
                        if (Materials.Count >= 0 && Materials[0] != null)
                        {
                            for (int i = 0; i < Materials.Count; i++)
                            {
                                if (Materials[i] != null)
                                {
                                    Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                    Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera.transform.position.x, m_ReflectionCamera.transform.position.y, m_ReflectionCamera.transform.position.z, 1));

                                }
                            }
                        }
                        m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                    }
                }
            }

            m_ReflectionCamera.orthographic = true;
            m_ReflectionCamera.farClipPlane = depthCameraHeight + depthUnderWater;
            m_ReflectionCamera.orthographicSize = depthCameraHeight + depthUnderWater;

            m_ReflectionCamera.transform.forward = -Vector3.up;

            Vector3 prevPos = camera.transform.position;
            Quaternion prevPRot = camera.transform.rotation;

            ////////////////////////////////////////////////////////     LightweightRenderPipeline.RenderSingleCamera(context, m_ReflectionCamera); //////////////////////////////////
            if (Time.fixedTime < 1 || (updatePerDistance && updateDistance > 0) || !Application.isPlaying)
            {
                if (Camera.main != null && Camera.main.transform.position.y > waterHeight)//v1.3  //< waterHeight)//v0.9
                {
                    if ((renderedDepth == -1 && Vector3.Distance(Camera.main.transform.position, prevPosition) > updateDistance) || !Application.isPlaying || Time.fixedTime < 1)
                    {
                        if (!useVolumeRenderer)//v1.2
                        {
                            m_ReflectionCamera.enabled = true;
                        }
                        prevPosition = Camera.main.transform.position;
                        renderedDepth = framesForCameraRender;
                        m_ReflectionCamera.transform.position = new Vector3(camera.transform.position.x, depthCameraHeight, camera.transform.position.z);

                        if (!Application.isPlaying || Time.fixedTime < 1)
                        {
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
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (renderedDepth > 0 && updateDistance > 0)
                        {
                            //apply to materials
                            if (renderedDepth == 2)
                            {
                                //save camera position at that point
                                m_ReflectionCamera_transform_position = m_ReflectionCamera.transform.position;
                            }

                            if (renderedDepth == 1)
                            {
                                m_ReflectionCamera.enabled = false; //v0.2
                            }
                            renderedDepth--;

                            if (renderedDepth == 0)
                            {
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
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (renderedDepth == 0)
                            {
                                m_ReflectionCamera.enabled = false;
                                renderedDepth = -1;

                                if (Materials.Count >= 0 && Materials[0] != null)
                                {
                                    for (int i = 0; i < Materials.Count; i++)
                                    {
                                        if (Materials[i] != null)
                                        {
                                            Materials[i].SetVector("_DepthCameraPos", new Vector4(m_ReflectionCamera_transform_position.x, depthCameraHeight, m_ReflectionCamera_transform_position.z, 1));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    m_ReflectionCamera.enabled = false;
                }
            }
            else
            {
                if (!useVolumeRenderer)//v1.2
                {
                    m_ReflectionCamera.enabled = true;

                    m_ReflectionCamera.transform.position = new Vector3(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z);
                    if (Materials.Count >= 0 && Materials[0] != null)
                    {
                        for (int i = 0; i < Materials.Count; i++)
                        {
                            if (Materials[i] != null)
                            {
                                Materials[i].SetTexture(depthTextureName, m_ReflectionTextureDEPTH);
                                Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
                            }
                        }
                    }
                }
            }

            QualitySettings.maximumLODLevel = max;
            QualitySettings.lodBias = bias;
        }

        void Update1()
        {
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
                            Materials[i].SetVector("_DepthCameraPos", new Vector4(Camera.main.transform.position.x, depthCameraHeight, Camera.main.transform.position.z, 1));
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
            }
        }
        public Texture2D testTex;

        Texture2D toTexture2D(RenderTexture rTex)
        {
            if (testTex == null)
            {
                testTex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            }
            RenderTexture.active = rTex;
            testTex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            testTex.Apply();
            return testTex;
        }

        #endregion//DEPTH RENDERER

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Projected Grid -----------------------")]

        #region Projected Grid
        //public class ProjectedGrid : MonoBehaviour
        //{
        //v1.2
        public float minAngleSpeed = 15; //if previosu to nxt frame has 15 degrees difference, do update
        float prevEuler = 0;
        public bool createMeshes = false;
        public Transform SUN;
        public Material m_oceanMaterial;
        public GameObject m_grid;
        private Projection m_projection;
        public int pixelsPerQuad = 8;
        public float forwardOffset = 6;//100;//200
        public float heightOffset = 12;//50;//100
        public float heightScale = 0.3f;
        public float maxheight = 10;

        void ProjectedGrid_Start()
        {
            m_projection = new Projection();
            m_projection.OceanLevel = transform.position.y;
            m_projection.MaxHeight = Camera.main.transform.position.y; //maxheight;// 10.0f;

            MeshFilter meshes = this.gameObject.GetComponentInChildren<MeshFilter>(true);

            if (meshes == null && m_grid == null)
            {
                CreateGrid(pixelsPerQuad); //CreateGrid(8);
                UpdateA();
            }
            //v1.2
            prevEuler = Camera.main.transform.eulerAngles.y;
        }

        public int updateEvery = 1;
        public int updateEveryRot = 2;
        int currentUpdate = 0;
        int currentRotUpdate = 0;

        void ProjectedGrid_Update()
        {
            if (!Application.isPlaying)
            {
                //if meshes found and empty, auto recreate
                MeshFilter meshes = this.gameObject.GetComponentInChildren<MeshFilter>(true);
                if (meshes != null && meshes.sharedMesh == null)
                {
                    RecreateMesh(pixelsPerQuad);
                    UpdateA();
                }
            }

            if (createMeshes)
            {
                RecreateMesh(pixelsPerQuad);
                createMeshes = false;
            }

            if (m_projection == null)
            {
                ProjectedGrid_Start();
            }

            //if (Application.isPlaying)
            if (updateEvery > 0 && Time.fixedTime > 2) //v1.2
            {
                if (!Application.isPlaying || currentUpdate > updateEvery || (Camera.main.transform.eulerAngles.y != prevEuler
                    && (currentRotUpdate > updateEveryRot || Mathf.Abs(Camera.main.transform.eulerAngles.y - prevEuler) > minAngleSpeed)))//v1.2
                {
                    UpdateA();
                    currentUpdate = 0;
                    currentRotUpdate = 0;//v1.2

                    prevEuler = Camera.main.transform.eulerAngles.y;//v1.2
                }
            }
            else
            {
                UpdateA(); //v1.2
            }
            currentUpdate++;

            if (Camera.main.transform.eulerAngles.y != prevEuler)//v1.2
            {
                currentRotUpdate++;
            }
        }

        //v1.1
        public float eulerX = 0;
        public float heightScaler = 200;
        public bool autoAdjustGrid = false;

        void UpdateA()
        {
            Camera cam = Camera.main;
            if (cam == null || m_oceanMaterial == null) return;

            m_projection.OceanLevel = transform.position.y;
            m_projection.MaxHeight = 10;

            //v1.1
            if (autoAdjustGrid)
            {
                m_projection.MaxHeight = maxheight + cam.transform.position.y - 100;// maxheight;// 10.0f;                
                if (cam.transform.position.y < 4)
                {
                    m_projection.MaxHeight = 322 + cam.transform.position.y - 100;
                }
                //v1.2a
                if (cam.transform.position.y < -3)
                {
                    maxheight = 372;
                    m_projection.MaxHeight = 372;
                }
                if (cam.transform.position.y < -7)
                {
                    m_projection.MaxHeight = 222 + cam.transform.position.y - 100;
                }

                //v1.7
                if (cam.transform.position.y < m_projection.OceanLevel - 4)
                {
                    m_projection.MaxHeight = 50 + cam.transform.position.y;
                }
            }
            Vector3 keepEuler = cam.transform.eulerAngles;//v1.1
            Vector3 keepPos = cam.transform.position;
            cam.transform.position += new Vector3(0, heightOffset, 0) - cam.transform.forward * forwardOffset;

            //v1.1
            if (autoAdjustGrid)
            {
                float eulerXA = Mathf.Lerp(cam.transform.eulerAngles.x, eulerX, m_projection.MaxHeight / heightScaler);
                if (cam.transform.eulerAngles.x > 180)
                {
                    eulerXA = Mathf.Lerp(cam.transform.eulerAngles.x - 360, eulerX, m_projection.MaxHeight / heightScaler);
                }
                cam.transform.eulerAngles = new Vector3(eulerXA, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);
            }
            m_projection.UpdateProjection(cam);//m_projection.UpdateProjection(cam);
                                               //v1.1
            cam.transform.eulerAngles = keepEuler;
            cam.transform.position = keepPos;// new Vector3(0, -heightOffset, 0) + cam.transform.forward * forwardOffset;
            m_oceanMaterial.SetMatrix("_Interpolation", m_projection.Interpolation);

            //Once the camera goes below the projection plane (the ocean level) the projected
            //grid will flip the triangle winding order. 
            //Need to flip culling so the top remains the top.
            bool isFlipped = m_projection.IsFlipped;
            m_oceanMaterial.SetInt("_CullFace", (isFlipped) ? (int)CullMode.Front : (int)CullMode.Back);

            //SUN LIGHT
            if (SUN != null)
            {
                m_oceanMaterial.SetVector("SUN_DIR", SUN.transform.forward * -1.0f);
            }
            //scale it 
            this.transform.localScale = new Vector3(transform.localScale.x, heightScale, transform.localScale.z);
        }

        /// <summary>
        /// Creates the ocean mesh gameobject.
        /// The resolutions is how many pixels per quad in mesh.
        /// The higher the number the less verts in mesh.
        /// </summary>
        void CreateGrid(int resolution)
        {
            int width = 1320;// Screen.width;
            int height = 720;// Screen.height;
            int numVertsX = width / resolution;
            int numVertsY = height / resolution;

            Mesh mesh = CreateQuad(numVertsX, numVertsY);
            if (mesh == null) return;

            //The position of the mesh is not known until its projected in the shader. 
            //Make the bounds large enough so the camera will draw it.
            float bigNumber = 1e6f;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));

            m_grid = new GameObject("Ocean mesh");
            m_grid.transform.parent = transform;
            MeshFilter filter = m_grid.AddComponent<MeshFilter>();
            MeshRenderer renderer = m_grid.AddComponent<MeshRenderer>();

            filter.sharedMesh = mesh;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.sharedMaterial = m_oceanMaterial;
        }

        //v0.5
        void RecreateMesh(int resolution)
        {
            if (resolution < 3)
            {
                resolution = 3;
            }
            int width = 1320;// Screen.width;
            int height = 720;// Screen.height;
            int numVertsX = width / resolution;
            int numVertsY = height / resolution;

            Mesh mesh = CreateQuad(numVertsX, numVertsY);
            if (mesh == null) return;

            //The position of the mesh is not known until its projected in the shader. 
            //Make the bounds large enough so the camera will draw it.
            float bigNumber = 1e6f;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));
            MeshFilter[] meshes = this.gameObject.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].sharedMesh == null)
                {
                    meshes[i].sharedMesh = mesh;
                    meshes[i].mesh = mesh;
                }
            }
        }

        public Mesh CreateQuad(int numVertsX, int numVertsY)
        {
            Vector3[] vertices = new Vector3[numVertsX * numVertsY];
            Vector2[] texcoords = new Vector2[numVertsX * numVertsY];
            int[] indices = new int[numVertsX * numVertsY * 6];

            for (int x = 0; x < numVertsX; x++)
            {
                for (int y = 0; y < numVertsY; y++)
                {
                    Vector2 uv = new Vector3(x / (numVertsX - 1.0f), y / (numVertsY - 1.0f));

                    texcoords[x + y * numVertsX] = uv;
                    vertices[x + y * numVertsX] = new Vector3(uv.x, uv.y, 0.0f);
                }
            }
            int num = 0;
            for (int x = 0; x < numVertsX - 1; x++)
            {
                for (int y = 0; y < numVertsY - 1; y++)
                {
                    indices[num++] = x + y * numVertsX;
                    indices[num++] = x + (y + 1) * numVertsX;
                    indices[num++] = (x + 1) + y * numVertsX;

                    indices[num++] = x + (y + 1) * numVertsX;
                    indices[num++] = (x + 1) + (y + 1) * numVertsX;
                    indices[num++] = (x + 1) + y * numVertsX;
                }
            }
            if (vertices.Length > 65000)
            {
                //Too many verts to make a mesh. 
                //You will need to split the mesh.
                return null;
            }
            else
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.uv = texcoords;
                mesh.triangles = indices;

                return mesh;
            }
        }
        //}
        #endregion


        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Control Reflect Depth FX HDRP ----------------------")]
        #region ReflectDepth controlls
        //public Material oceanMaterial;
        public bool applyProperties = false;

        public bool applyPreset = false;
        public int presetID = 0;

        public bool reInitScales = false;
        public bool autoScaleDepthMultiplier = true;
        public float depthWorldScaleMultiplier = 3.18f; //maps pixels to worldspace actual size of top down depth texture rendering camera, double use half res

        //adjust reflection if custom planar refelct script (based on downscale) or reflect probe (use 1) is used
        public Vector4 reflectPowerTiling = new Vector4(42, 5, 0.5f, 1);
        Vector4 reflectPowerTilingPreset1 = new Vector4(42, 5, 0.5f, 1);    //for Prefab 2 
        Vector4 reflectPowerTilingPreset2 = new Vector4(0.65f, 5, 1f, 1);   //for Prefab 5 

        //restore waves if previous scene had reset power to zero if camera above water
        public bool resetWavesPower = false;
        public Vector4 wavesMainPower = new Vector4(1, 1, 1, 1);

        //Refract line adjust - make black line bigger to apply the water-air separation or not
        public float cameraNearClipPlane = -0.61f;
        public Vector4 blackLine = new Vector4(0.1f, 1, 14, 1);

        //Scale and offset depth pyramid texture (also must add to change the same in clouds etc scripts !!!!! - TO DO)
        public bool autoDepthScaleUpdate = false;
        public Vector4 depthPyramidScale = new Vector4(1f, 0.6667f, 0, 0);
        Vector4 depthPyramidScalePreset1 = new Vector4(1f, 0.6667f, 0, 0);      //for Prefab 2 
        Vector4 depthPyramidScalePreset2 = new Vector4(0.475f, 0.153f, 0, 0);   //for Prefab 5 

        // Start is called before the first frame update
        void ControlReflectDepthFX_Start()
        {
            if (resetWavesPower)
            {
                oceanMaterial.SetVector("_GAmplitude", wavesMainPower);
            }

            if (oceanMaterial != null)//&& depthRenderer != null)
            {
                //oceanMaterial.SetVector("_TerrainScale", 
                //    new Vector4((int)(Screen.width * depthRenderer.GetScaleValue()), 
                //    (int)(Screen.height * depthRenderer.GetScaleValue()), 0,0));
                oceanMaterial.SetVector("_TerrainScale",
                   new Vector4((Screen.width * GetScaleValue()),
                   (Screen.height * GetScaleValue()), 0, 0));
                //oceanMaterial.SetVector("offsetScale", new Vector4(offsetScale.x, offsetScale.y, 0, 0));

                //oceanMaterial.SetFloat("_WorldScale", depthWorldScaleMultiplier / depthRenderer.GetScaleValue());
                if (autoScaleDepthMultiplier)
                {
                    depthWorldScaleMultiplier = (depthCameraHeight + depthUnderWater) / (Screen.height / 2);
                }
                oceanMaterial.SetFloat("_WorldScale", depthWorldScaleMultiplier / GetScaleValue());
            }
        }
        //public DepthRendererSM_HDRP depthRenderer;/////////////public DepthRendererSM_HDRP depthRenderer;
        // Update is called once per frame
        void ControlReflectDepthFX_Update()
        {
            if (applyPreset)
            {
                if (presetID == 0)  //for Prefab 2 (Fly through full volume clouds)
                {
                    depthPyramidScale = depthPyramidScalePreset1;
                    reflectPowerTiling = reflectPowerTilingPreset1;
                }
                if (presetID == 1) //for Prefab 5 (Dome background volume clouds)
                {
                    depthPyramidScale = depthPyramidScalePreset2;
                    reflectPowerTiling = reflectPowerTilingPreset2;
                }
                applyPreset = false;
            }

            if (oceanMaterial != null)// && applyProperties)
            {
                oceanMaterial.SetVector("_controlReflect", reflectPowerTiling); // (Power,Distort,Downscale)

                if (autoDepthScaleUpdate)
                {
                    oceanMaterial.SetVector("DepthPyramidScale", new Vector4(Screen.width * 0.439f / 900.0f, Screen.height * 0.15f / 452.0f, 0, 0));
                }
                else
                {
                    oceanMaterial.SetVector("DepthPyramidScale", depthPyramidScale);
                }
                oceanMaterial.SetVector("cameraNearControl", blackLine);
                oceanMaterial.SetFloat("cameraNearClip", cameraNearClipPlane); //cameraNearColor cameraNearColor("Control air line color", COLOR) = (0.2, 0.4, 0.6, 1)
                                                                               //applyProperties = false;
                                                                               //_TerrainScale - cameraOffset
                                                                               //oceanMaterial.SetFloat("cameraOffset", cameraOffsetMultiplier);
                if ((!Application.isPlaying || reInitScales))//&& depthRenderer != null)
                {
                    oceanMaterial.SetVector("_TerrainScale",
                   new Vector4((Screen.width * GetScaleValue()),
                   (Screen.height * GetScaleValue()), 0, 0));

                    //oceanMaterial.SetVector("offsetScale", new Vector4(offsetScale.x, offsetScale.y,0,0));
                    if (autoScaleDepthMultiplier)
                    {
                        depthWorldScaleMultiplier = (depthCameraHeight + depthUnderWater) / (Screen.height / 2);
                    }
                    oceanMaterial.SetFloat("_WorldScale", depthWorldScaleMultiplier / GetScaleValue());

                    reInitScales = false;
                }
            }
        }
        #endregion

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Water Base Setup ----------------------")]
        #region Depth Renderer
        public Material sharedMaterial;
        //public Artngame.SKYMASTER.WaterQuality waterQuality = Artngame.SKYMASTER.WaterQuality.High;
        public bool edgeBlend = true;

        public void UpdateShader()
        {
            //if (waterQuality > Artngame.SKYMASTER.WaterQuality.Medium)
            //{
            sharedMaterial.shader.maximumLOD = 501;
            //}
            //else if (waterQuality > Artngame.SKYMASTER.WaterQuality.Low)
            //{
            //    sharedMaterial.shader.maximumLOD = 301;
            //}
            //else
            //{
            //    sharedMaterial.shader.maximumLOD = 201;
            //}

            // If the system does not support depth textures (ie. NaCl), turn off edge bleeding,
            // as the shader will render everything as transparent if the depth texture is not valid.
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                edgeBlend = false;
            }

            if (edgeBlend)
            {
                Shader.EnableKeyword("WATER_EDGEBLEND_ON");
                Shader.DisableKeyword("WATER_EDGEBLEND_OFF");
                // just to make sure (some peeps might forget to add a water tile to the patches)
                if (Camera.main)
                {
                    Camera.main.depthTextureMode |= DepthTextureMode.Depth;
                }
            }
            else
            {
                Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
                Shader.DisableKeyword("WATER_EDGEBLEND_ON");
            }
        }

        public void WaterTileBeingRendered(Transform tr, Camera currentCam)
        {
            if (currentCam && edgeBlend)
            {
                currentCam.depthTextureMode |= DepthTextureMode.Depth;
            }
        }

        public void WaterBaseSM_Update()
        {
            if (sharedMaterial)
            {
                UpdateShader();
            }
        }

        #endregion

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Gernstner Waves  ----------------------")]
        #region Depth Renderer
        public bool enableGernstner = false;
        bool prevGernstnerChoice = false;
        void UpdateGernstner()
        {
            if (prevGernstnerChoice != enableGernstner)
            {
                if (enableGernstner == false)
                {
                    GernstnerDisable();
                }
                else
                {
                    GernstnerEnable();
                }
                prevGernstnerChoice = enableGernstner;
            }
        }
        public void GernstnerEnable()
        {
            Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
            Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
        }


        public void GernstnerDisable()
        {
            Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
            Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
        }
        #endregion


        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Sky ----------------------")]
        public bool updateSky = false;
        public GameObject m_skyGO;
        public Sky m_sky;
        #region Sky Renderer
        void UpdateSky()
        {

            if (m_skyGO != null && m_sky == null)
            {
                m_sky = m_skyGO.GetComponent<Sky>();
            }
            //Need to apply the sky settings for ocean.
            if (m_sky != null)
            {
                m_sky.UpdateMat(m_oceanMaterial);
            }
            if (Application.isPlaying)
            {
                updateSky = false;
            }
        }
        #endregion

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("--------------------- FFT waves generator ----------------------")]
        #region FFT waves generator

        //v1.4
        public float updateFFTperTime = 0;
        float prevUpdateTime = 0;

        public Transform heightSampler;
        public float lodScale = 10;

        public WavesCascade cascade0;
        public WavesCascade cascade1;
        public WavesCascade cascade2;

        // must be a power of 2
        [SerializeField]
        public int size = 256;

        [SerializeField]
        WavesSettings wavesSettings;
        [SerializeField]
        public bool alwaysRecalculateInitials = false;

        public Vector4 lengthScales = new Vector4(1, 1, 1, 1);

        [SerializeField]
        public float lengthScale0 = 250;
        [SerializeField]
        public float lengthScale1 = 17;
        [SerializeField]
        public float lengthScale2 = 5;

        [SerializeField]
        ComputeShader fftShader;
        [SerializeField]
        ComputeShader initialSpectrumShader;
        [SerializeField]
        ComputeShader timeDependentSpectrumShader;
        [SerializeField]
        ComputeShader texturesMergerShader;

        Texture2D gaussianNoise;
        FastFourierTransform fft;
        Texture2D physicsReadback;

        private void WaveGenerator_Awake()
        {
            Application.targetFrameRate = -1;
            fft = new FastFourierTransform(size, fftShader);
            gaussianNoise = GetNoiseTexture(size);

            //v0.8
            if (cascade0 != null)
            {
                cascade0.Dispose();
            }
            if (cascade1 != null)
            {
                cascade1.Dispose();
            }
            if (cascade2 != null)
            {
                cascade2.Dispose();
            }

            cascade0 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
            cascade1 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
            cascade2 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);

            InitialiseCascades();

            //Debug.Log("Set 2");
            //Debug.Log("Set 3 =" + wavesSettings.g); 

            physicsReadback = new Texture2D(size, size, TextureFormat.RGBAFloat, false);

            //v0.1
            //Awake();
            //if (alwaysRecalculateInitials)
            //{
            //    InitialiseCascades();
            //}
            float timers = 0;
            for (int i = 0; i < 120; i++)
            {
                timers = timers + 0.01f;
                cascade0.CalculateWavesAtTime(timers);
                cascade1.CalculateWavesAtTime(timers);
                cascade2.CalculateWavesAtTime(timers);
            }
            RequestReadbacks();
        }

        void InitialiseCascades()
        {
            if (cascade0 != null && wavesSettings != null)//v0.8
            {
                float boundary1 = 2 * Mathf.PI / lengthScale1 * 6f;
                float boundary2 = 2 * Mathf.PI / lengthScale2 * 6f;
                cascade0.CalculateInitials(wavesSettings, lengthScale0, 0.0001f, boundary1);
                cascade1.CalculateInitials(wavesSettings, lengthScale1, boundary1, boundary2);
                cascade2.CalculateInitials(wavesSettings, lengthScale2, boundary2, 9999);

                Shader.SetGlobalFloat("LengthScale0", lengthScale0);
                Shader.SetGlobalFloat("LengthScale1", lengthScale1);
                Shader.SetGlobalFloat("LengthScale2", lengthScale2);
                Shader.SetGlobalVector("lengthScales", lengthScales);

                //Debug.Log("Set1");
                //Debug.Log("Set 4 =" + wavesSettings.g);
            }
        }

        private void WaveGenerator_Update()
        {
            if (alwaysRecalculateInitials && cascade0 != null) //v0.8
            {
                InitialiseCascades();
            }

            cascade0.CalculateWavesAtTime(Time.time);
            cascade1.CalculateWavesAtTime(Time.time);
            cascade2.CalculateWavesAtTime(Time.time);

            RequestReadbacks();

            //v0.1
            if (heightSampler != null)
            {
                Vector3 displacment = GetWaterDisplacement(heightSampler.position);
                if (Mathf.Abs(displacment.y) < 40)
                {
                    Vector3 heightSampler_position = new Vector3(heightSampler.position.x + displacment.x,
                                                                    GetWaterHeight(heightSampler.position),
                                                                    heightSampler.position.z + +displacment.z);
                    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler_position, Time.deltaTime * lerpSpeed);
                    heightSampler.position = new Vector3(heightSampler.position.x, GetWaterHeight(heightSampler.transform.position), heightSampler.position.z);
                }

                Vector3 MotionDirection = (heightSampler.position - prev_pos).normalized;
                //Debug.DrawLine(heightSampler.position, heightSampler.position + MotionDirection* 5,Color.red,1);
                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    Quaternion.AngleAxis(-100 * MotionDirection.y, (Vector3.up + MotionDirection)),
                    Time.deltaTime * lerpRotSpeed * 0.05f);

                //Sample boat back and front !!!
                Vector3 frontPos = heightSampler.position + heightSampler.forward * boatLength / 2;
                Vector3 backPos = heightSampler.position - heightSampler.forward * boatLength / 2;

                Debug.DrawLine(frontPos, frontPos + Vector3.up * 4, Color.red, 1);
                Debug.DrawLine(backPos, backPos + Vector3.up * 4, Color.red, 1);
                float frontH = GetWaterHeight(frontPos);
                float backH = GetWaterHeight(backPos);
                float diffBF = frontH - backH;
                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    Quaternion.AngleAxis(diffBF, heightSampler.right),
                    Time.deltaTime * lerpRotSpeed * 0.05f);
                Debug.DrawLine(new Vector3(frontPos.x, frontH, frontPos.z), new Vector3(frontPos.x, frontH, frontPos.z) + Vector3.up * 4, Color.green, 2);
                Debug.DrawLine(new Vector3(backPos.x, backH, backPos.z), new Vector3(backPos.x, backH, backPos.z) + Vector3.up * 4, Color.green, 2);
                Vector3 waterGradient = new Vector3(frontPos.x, frontH, frontPos.z) - new Vector3(backPos.x, backH, backPos.z);
                if (controlBoat)
                {
                    Vector3 Forward_flat = new Vector3(heightSampler.forward.x, 0, heightSampler.forward.z);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                    Vector2 move =
                     Keyboard.current != null
                     ? new Vector2(
                         (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                         (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
                       )
                     : Vector2.zero;

                    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler.position + new Vector3(waterGradient.x * 1 * move.y,
                        0, waterGradient.z * 1 * move.y), Time.deltaTime * BoatSpeed);

                    CurrentRot = Mathf.Lerp(CurrentRot, CurrentRot + move.x, Time.deltaTime * BoatRotSpeed);

#else
                    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler.position + new Vector3(waterGradient.x * 1 * Input.GetAxis("Vertical"),
                        0, waterGradient.z * 1 * Input.GetAxis("Vertical")), Time.deltaTime * BoatSpeed);

                    CurrentRot = Mathf.Lerp(CurrentRot, CurrentRot + Input.GetAxis("Horizontal"), Time.deltaTime * BoatRotSpeed);
#endif


                    heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                         Quaternion.AngleAxis(CurrentRot, Vector3.up) * Quaternion.AngleAxis(diffBF * 2, heightSampler.right),
                         Time.deltaTime * BoatRotSpeed);
                }
                //------------------------------------------------FLOATERS
#region Floaters
                if (castFloaters)
                {
                    //HDRP 1 - sample randomly to add wave splashes
                    if (waterSpray != null)
                    {
                        if (waterSprays.Count == 0)
                        { //create 6 instances
                            for (int i = 0; i < maxSplashes; i++)
                            {
                                GameObject splash = Instantiate(waterSpray);
                                ParticleSystem[] partsA = splash.GetComponentsInChildren<ParticleSystem>(true);
                                if (partsA.Length > 0)
                                {
                                    partsA[0].gameObject.SetActive(false); //and disable them
                                }
                                waterSprays.Add(splash);
                            }
                        }
                        for (int i = 0; i < waterSprays.Count; i++)
                        {
                            //sample a random placement for the first disabled
                            if (!waterSprays[i].activeInHierarchy)
                            {
                                float aheadDistX = 4 * Random.Range(splashDistMin, splashDistMax);
                                float aheadDistZ = 4 * Random.Range(splashDistMin, splashDistMax);
                                float PosX1 = Camera.main.transform.position.x + Camera.main.transform.forward.x * aheadDistX;
                                float PosZ1 = Camera.main.transform.position.z + Camera.main.transform.forward.z * aheadDistZ;

                                PosX1 = PosX1 + Camera.main.transform.right.x * Random.Range(-splashDistMin, splashDistMax) * 2;
                                PosZ1 = PosZ1 + Camera.main.transform.right.z * Random.Range(-splashDistMin, splashDistMax) * 2;

                                float PosY1 = Camera.main.transform.position.y + Camera.main.transform.forward.y;
                                //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                                Vector3 cameraPos = new Vector3(PosX1, PosY1, PosZ1);
                                Vector3 displacementTO = GetWaterDisplacement(cameraPos);
                                Vector3 GerstnerOffsets1 = new Vector3(cameraPos.x + displacementTO.x,
                                                                                    GetWaterHeight(cameraPos),
                                                                                    cameraPos.z + +displacementTO.z);
                                waterSprays[i].transform.position = new Vector3(GerstnerOffsets1.x * 1 + PosX1, GerstnerOffsets1.y * heightFactor1 + heightOffsetY, GerstnerOffsets1.z * 1 + PosZ1);
                                waterSprays[i].SetActive(true);
                                //v0.6 - splash
                                ParticleSystem[] parts = waterSprays[i].GetComponentsInChildren<ParticleSystem>(true);
                                if (parts.Length > 0)
                                {
                                    parts[0].gameObject.SetActive(true);
                                }
                                break;//stop here
                            }
                            else //if stopped play, disable again                        
                            {
                                ParticleSystem[] parts = waterSprays[i].GetComponentsInChildren<ParticleSystem>(true);
                                if (parts.Length > 0)
                                {
                                    if (!parts[0].isPlaying)
                                    {
                                        parts[0].gameObject.SetActive(false);
                                        waterSprays[i].SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                    for (int i = WaterObjects.Count - 1; i >= 0; i--)
                    {
                        if (WaterObjects[i] == null)
                        {
                            WaterObjects.RemoveAt(i);
                            WaterObjectsWaterPos.RemoveAt(i);
                            WaterObjectsStartPos.RemoveAt(i);
                        }
                    }
                    for (int i = ThrowObjects.Count - 1; i >= 0; i--)
                    {
                        if (ThrowObjects[i] == null)
                        {
                            ThrowObjects.RemoveAt(i);
                            ThrowObjectsWaterPos.RemoveAt(i);
                            ThrowObjectsStartPos.RemoveAt(i);
                        }
                    }
                    for (int i = 0; i < WaterObjects.Count; i++)
                    {
                        float PosX1 = WaterObjects[i].position.x;
                        float PosZ1 = WaterObjects[i].position.z;
                        float PosY1 = WaterObjects[i].position.y;
                        //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                        Vector3 displacementTO = GetWaterDisplacement(WaterObjects[i].position);
                        Vector3 GerstnerOffsets1 = new Vector3(WaterObjects[i].position.x + displacementTO.x,
                                                                            GetWaterHeight(WaterObjects[i].position),
                                                                            WaterObjects[i].position.z + displacementTO.z);
                        if (ShiftHorPositionFloaters)
                        {
                            WaterObjectsWaterPos[i] = new Vector3(displacementTO.x * ShiftHorPositionFactor.x,
                                GerstnerOffsets1.y * heightFactor1 + heightOffsetY,
                                displacementTO.z * ShiftHorPositionFactor.z);
                        }
                        else
                        {
                            WaterObjectsWaterPos[i] = new Vector3(0, GerstnerOffsets1.y * heightFactor1 + heightOffsetY, 0);
                        }

                        if (alignToNormalFloaters)
                        {
                            WaterObjects[i].up = new Vector3((Mathf.Abs(displacementTO.x) - alignToNormalFactor.x),
                                (Mathf.Abs(displacementTO.y) - alignToNormalFactor.y),
                                (Mathf.Abs(displacementTO.z) - alignToNormalFactor.z));
                        }
                    }
                    for (int i = 0; i < WaterObjectsWaterPos.Count; i++)
                    {
                        if (LerpMotion)
                        {
                            WaterObjects[i].position = Vector3.Lerp(WaterObjects[i].position, new Vector3(WaterObjectsStartPos[i].x, this.transform.position.y - 0.45f, WaterObjectsStartPos[i].z) + WaterObjectsWaterPos[i], Time.deltaTime * lerpSpeed);
                        }
                        else
                        {
                            WaterObjects[i].position = new Vector3(WaterObjectsStartPos[i].x, this.transform.position.y - 0.45f, WaterObjectsStartPos[i].z) + WaterObjectsWaterPos[i];
                        }
                    }
                    for (int i = 0; i < ThrowObjects.Count; i++)
                    {
                        float PosX1 = ThrowObjects[i].position.x;
                        float PosZ1 = ThrowObjects[i].position.z;
                        float PosY1 = ThrowObjects[i].position.y;
                        //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                        Vector3 displacementTO = GetWaterDisplacement(ThrowObjects[i].position);
                        Vector3 GerstnerOffsets1 = new Vector3(ThrowObjects[i].position.x + displacementTO.x,
                                                                            GetWaterHeight(ThrowObjects[i].position),
                                                                            ThrowObjects[i].position.z + +displacementTO.z);
                        ThrowObjectsWaterPos[i] = new Vector3(GerstnerOffsets1.x, GerstnerOffsets1.y * heightFactor1, GerstnerOffsets1.z);
                    }
                    for (int i = 0; i < ThrowObjectsWaterPos.Count; i++)
                    {
                        if (ThrowObjectsWaterPos[i].y + this.transform.position.y > ThrowObjects[i].position.y)
                        {
                            //v0.6 - splash
                            ParticleSystem[] parts = ThrowObjects[i].GetComponentsInChildren<ParticleSystem>(true);
                            if (parts.Length > 0)
                            {
                                parts[0].gameObject.SetActive(true);
                            }
                            //add to water list
                            ThrowObjects[i].GetComponent<Rigidbody>().isKinematic = true;
                            WaterObjects.Add(ThrowObjects[i]);
                            WaterObjectsStartPos.Add(ThrowObjects[i].position);
                            WaterObjectsWaterPos.Add(ThrowObjectsWaterPos[i]);
                            ThrowObjectsWaterPos.RemoveAt(i);
                            ThrowObjects.RemoveAt(i);
                            ThrowObjectsStartPos.RemoveAt(i);
                        }
                    }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                    if (
                        Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame
                    )
                    {
#else
                    if (Input.GetMouseButtonDown(1))
                    { //v3.4
#endif

                        if (ThrowItem != null)
                        {
                            GameObject TEMP = (GameObject)Instantiate(ThrowItem, heightSampler.transform.position + new Vector3(0, 5, 0), Quaternion.identity);
                            TEMP.transform.localScale = TEMP.transform.localScale * 5;
                            ThrowObjects.Add(TEMP.transform);
                            ThrowObjectsWaterPos.Add(new Vector3(0, 0, 0));
                            ThrowObjectsStartPos.Add(TEMP.transform.position);
                            if (TEMP != null)
                            {
                                Rigidbody RGB = TEMP.GetComponent<Rigidbody>() as Rigidbody;
                                if (RGB != null)
                                {
                                    if (Camera.main != null)
                                    {
                                        RGB.AddForce(Camera.main.transform.forward * ThrowPower);
                                    }
                                    else
                                    {
                                        RGB.AddForce(heightSampler.transform.forward * ThrowPower);
                                    }
                                }
                            }
                        }
                    }
                    //HDRP
                    Vector3 Velocity = (heightSampler.transform.position - prev_pos) / Time.deltaTime;

                    //HDRP - check collisions with boat
                    for (int i = 0; i < WaterObjects.Count; i++)
                    {
                        float distBoatBall = Vector3.Distance(WaterObjects[i].position, heightSampler.transform.position);
                        if (distBoatBall < 6)
                        {
                            WaterObjects[i].GetComponent<Rigidbody>().isKinematic = false;
                            ThrowObjects.Add(WaterObjects[i]);
                            ThrowObjectsStartPos.Add(WaterObjects[i].position);
                            ThrowObjectsWaterPos.Add(WaterObjectsWaterPos[i]);

                            WaterObjectsWaterPos.RemoveAt(i);
                            WaterObjects.RemoveAt(i);
                            WaterObjectsStartPos.RemoveAt(i);

                            Rigidbody RGB = ThrowObjects[ThrowObjects.Count - 1].GetComponent<Rigidbody>() as Rigidbody;
                            if (RGB != null)
                            {
                                if (Camera.main != null)
                                {
                                    //RGB.AddForce(Camera.main.transform.forward * ThrowPower);
                                    RGB.AddForce((Vector3.up * (0.1f + 0.12f * Random.Range(0.0f, 1.0f)) + 0.0075f * new Vector3(Velocity.x, 0, Velocity.z)) * ThrowPower);
                                }
                                else
                                {
                                    RGB.AddForce(heightSampler.transform.forward * ThrowPower);
                                }
                            }
                        }
                    }
                }
#endregion
                //---------------------------------------------------END FLOATERS
                prev_pos = heightSampler.position;
            }
        }

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("--------------------- Boat and Floaters ------------------------")]
        public bool controlBoat = false;
        public float boatLength = 5;
        public float lerpSpeed = 4;
        public float lerpRotSpeed = 20;
        Vector3 prev_pos;
        float CurrentRot;
        public float BoatSpeed = 1;
        public float BoatRotSpeed = 1;
        //FLOATERS   
        public bool castFloaters = false;
        public float heightOffsetY = 0;
        public float heightFactor1 = 1;
        public bool ShiftHorPositionFloaters = false;
        public Vector3 ShiftHorPositionFactor = new Vector3(1, 1, 1);
        public bool alignToNormalFloaters = false;
        public Vector3 alignToNormalFactor = new Vector3(1, 1, 1);
        public bool LerpMotion = false;
        //public float lerpSpeed = 1;
        //HDRP v0.2
        public GameObject waterSpray;
        public List<GameObject> waterSprays = new List<GameObject>();
        public int maxSplashes = 6;
        public float splashDistMin = 30;
        public float splashDistMax = 70;//////
        public List<Transform> ThrowObjects = new List<Transform>();
        public List<Vector3> ThrowObjectsWaterPos = new List<Vector3>();
        public List<Vector3> ThrowObjectsStartPos = new List<Vector3>();
        //public List<Vector3> ThrowObjectsStartPos;
        public List<Transform> WaterObjects = new List<Transform>();
        public List<Vector3> WaterObjectsWaterPos = new List<Vector3>();
        public List<Vector3> WaterObjectsStartPos = new List<Vector3>();
        public GameObject ThrowItem;
        public float ThrowPower = 1150;
        //END FLOATERS

        Texture2D GetNoiseTexture(int size)
        {
            string filename = "GaussianNoiseTexture" + size.ToString() + "x" + size.ToString();
            Texture2D noise = Resources.Load<Texture2D>("GaussianNoiseTextures/" + filename);
            return noise ? noise : GenerateNoiseTexture(size, true);
        }

        Texture2D GenerateNoiseTexture(int size, bool saveIntoAssetFile)
        {
            Texture2D noise = new Texture2D(size, size, TextureFormat.RGFloat, false, true);
            noise.filterMode = FilterMode.Point;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    noise.SetPixel(i, j, new Vector4(NormalRandom(), NormalRandom()));
                }
            }
            noise.Apply();

#if UNITY_EDITOR
            if (saveIntoAssetFile)
            {
                string filename = "GaussianNoiseTexture" + size.ToString() + "x" + size.ToString();
                string path = "Assets/Resources/GaussianNoiseTextures/";
                AssetDatabase.CreateAsset(noise, path + filename + ".asset");
                Debug.Log("Texture \"" + filename + "\" was created at path \"" + path + "\".");
            }
#endif
            return noise;
        }

        float NormalRandom()
        {
            return Mathf.Cos(2 * Mathf.PI * Random.value) * Mathf.Sqrt(-2 * Mathf.Log(Random.value));
        }
        //private void OnDestroy()
        //{
        //    cascade0.Dispose();
        //    cascade1.Dispose();
        //    cascade2.Dispose();
        //}
        void RequestReadbacks()
        {
            AsyncGPUReadback.Request(cascade0.Displacement, 0, TextureFormat.RGBAFloat, OnCompleteReadback);
        }

        public float GetWaterHeight(Vector3 position)
        {
            Vector3 displacement = GetWaterDisplacement(position);
            displacement = GetWaterDisplacement(position - displacement);
            displacement = GetWaterDisplacement(position - displacement);
            return GetWaterDisplacement(position - displacement).y;
        }

        public Vector3 GetWaterDisplacement(Vector3 position)
        {
            Color c = physicsReadback.GetPixelBilinear(position.x / lengthScale0, position.z / lengthScale0);
            return new Vector3(c.r, c.g, c.b) * (lodScale / 10); //v0.1
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest request) => OnCompleteReadback(request, physicsReadback);
        void OnCompleteReadback(AsyncGPUReadbackRequest request, Texture2D result)
        {
            if (request.hasError)
            {
                Debug.Log("GPU readback error detected.");
                return;
            }
            if (result != null)
            {
                result.LoadRawTextureData(request.GetData<Color>());
                result.Apply();
            }
        }
#endregion





        //WATER REFLECTIONS
        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("----------------------- Water Reflections ----------------------")]
#region Water Reflections
        //v1.2 - optionally setup volume reflection texture renderer with reflection camera and grab texture from there
        public bool useVolumeRendererREFL = false;
        public GameObject volumeRendererREFLOBJ;
        //PlanarReflectionsSM_LWRP_VOLUME volumeRendererREFL;
        public int passIDREFL = 1;
        public bool compensateCamMotion = false;
        public float compensatePower = 1;
        public int renderDepthREFL = -16;
        public List<Material> materialsREFL = new List<Material>(); //USE SAME LIST AS DEPTH !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public bool invertCulling = true; //v0.1 
        [System.Serializable]
        public enum ResolutionMulltiplierREFL
        {
            Full,
            Half,
            Third,
            Quarter
        }
        [System.Serializable]
        public class PlanarReflectionSettingsREFL
        {
            public ResolutionMulltiplierREFL m_ResolutionMultiplier = ResolutionMulltiplierREFL.Third;
            public float m_ClipPlaneOffset = 0.07f;
            public LayerMask m_ReflectLayers = -1;
        }

        [SerializeField]
        public PlanarReflectionSettingsREFL m_settingsREFL = new PlanarReflectionSettingsREFL();

        public GameObject targetREFL;
        [FormerlySerializedAs("camOffsetREFL")] public float m_planeOffsetREFL;

        public static Camera m_ReflectionCameraREFL;//v0.8a

        private Vector2 m_TextureSize = new Vector2(256, 128);
        private RenderTexture m_ReflectionTextureREFL = null;
        //private int planarReflectionTextureID = Shader.PropertyToID("_PlanarReflectionTexture");
        private int planarReflectionTextureIDREFL;
        public string refractionTextureNameREFL = "_PlanarReflectionTexture";

        private Vector2 m_OldReflectionTextureSize;

        // Cleanup all the objects we possibly have created
        //private void OnDisable()
        //{
        //    Cleanup();
        //}
        // private void OnDestroy()
        //{
        //prevRenderer = -1;
        //   Cleanup();
        //}
        public int rendererID = 1;
        int prevRenderer = -1;

        //v0.8a
        public int rendererIDREFL = 1;
        int prevRendererREFL = -1;
        //NEW
        //sample code 
        //https://github.com/0lento/OcclusionProbes/blob/package/OcclusionProbes/OcclusionProbes.cs#L41
        private void OnEnableREFL()
        {
            //v0.8
            if (m_ReflectionCameraREFL && prevRendererREFL != rendererIDREFL)
            {
                UniversalAdditionalCameraData thisCameraData = m_ReflectionCameraREFL.gameObject.GetComponent<UniversalAdditionalCameraData>();
                if (thisCameraData != null)
                {
                    thisCameraData.SetRenderer(rendererIDREFL);
                    prevRendererREFL = rendererIDREFL;
                }
            }
            RenderPipelineManager.beginCameraRendering += DoReflections;

            RenderPipelineManager.endFrameRendering += applyReflectionsA;

            RenderPipelineManager.endCameraRendering += applyReflections;
            planarReflectionTextureIDREFL = Shader.PropertyToID(refractionTextureNameREFL);
        }
        //END NEW

        void CleanupREFL()
        {
            if (m_ReflectionCameraREFL)
            {
                m_ReflectionCameraREFL.targetTexture = null;
                SafeDestroyREFL(m_ReflectionCameraREFL.gameObject);
            }
            //Graphics.SetRenderTarget(null);
            //if (m_ReflectionTexture)
            //{
            //    RenderTexture.ReleaseTemporary(m_ReflectionTexture);
            //}
            //new
            RenderPipelineManager.beginCameraRendering -= DoReflections;
            //prevRenderer = -1;
        }

        void SafeDestroyREFL(Object obj)
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

        //v0.5
        void StartREFL()
        {
            prevCamRot = Camera.main.transform.eulerAngles.y;
        }
        float prevCamRot = 0;
        void UpdateREFL()
        {
            if (compensateCamMotion)
            {
                if (Application.isPlaying)
                {
                    float camRot = Camera.main.transform.eulerAngles.y;
                    if (Mathf.Abs(camRot - prevCamRot) > 0)
                    {
                        for (int i = 0; i < materialsREFL.Count; i++)
                        {
                            materialsREFL[i].SetVector("offsetRflect", 0.004f * compensatePower * new Vector4(camRot - prevCamRot, 0, 0, 0));
                        }
                        prevCamRot = camRot;
                    }
                }
            }
        }

        private void UpdateCameraREFL(Camera src, Camera dest)
        {
            if (dest == null)
                return;
            dest.CopyFrom(src);
        }

        private void UpdateReflectionCameraREFL(Camera realCamera)
        {
            planarReflectionTextureIDREFL = Shader.PropertyToID(refractionTextureNameREFL); //////////// reset name if changed

            if (m_ReflectionCameraREFL == null)
            {
                m_ReflectionCameraREFL = CreateMirrorObjectsREFL(realCamera);
            }

            //v0.8
            if (m_ReflectionCameraREFL && prevRendererREFL != rendererIDREFL)
            {
                UniversalAdditionalCameraData thisCameraData = m_ReflectionCameraREFL.gameObject.GetComponent<UniversalAdditionalCameraData>();
                if (thisCameraData != null)
                {
                    thisCameraData.SetRenderer(rendererIDREFL);
                    prevRendererREFL = rendererIDREFL;
                }
            }

            // find out the reflection plane: position and normal in world space
            Vector3 pos = Vector3.zero;
            Vector3 normal = Vector3.up;
            if (targetREFL != null)
            {
                pos = targetREFL.transform.position + Vector3.up * m_planeOffsetREFL;
                normal = targetREFL.transform.up;
            }
            UpdateCameraREFL(realCamera, m_ReflectionCameraREFL);

            // Render reflection
            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - m_settingsREFL.m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.identity;
            reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            Vector3 oldpos = realCamera.transform.position - new Vector3(0, pos.y * 2, 0);
            Vector3 newpos = ReflectPosition(oldpos);
            m_ReflectionCameraREFL.transform.forward = Vector3.Scale(realCamera.transform.forward, new Vector3(1, -1, 1));
            m_ReflectionCameraREFL.worldToCameraMatrix = realCamera.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlaneREFL(m_ReflectionCameraREFL, pos - Vector3.up * 0.1f, normal, 1.0f);
            Matrix4x4 projection = realCamera.CalculateObliqueMatrix(clipPlane);
            m_ReflectionCameraREFL.projectionMatrix = projection;
            m_ReflectionCameraREFL.cullingMask = m_settingsREFL.m_ReflectLayers; // never render water layer
            m_ReflectionCameraREFL.transform.position = newpos;

            m_ReflectionCameraREFL.depth = renderDepthREFL;//v0.5
        }

        // Calculates reflection matrix around the given plane
        //private static void CalculateReflectionMatrixREFL(ref Matrix4x4 reflectionMat, Vector4 plane)
        //{
        //    reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        //    reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        //    reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        //    reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        //    reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        //    reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        //    reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        //    reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        //    reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        //    reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        //    reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        //    reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        //    reflectionMat.m30 = 0F;
        //    reflectionMat.m31 = 0F;
        //    reflectionMat.m32 = 0F;
        //    reflectionMat.m33 = 1F;
        //}

        //private static Vector3 ReflectPosition(Vector3 pos)
        //{
        //    Vector3 newPos = new Vector3(pos.x, -pos.y, pos.z);
        //    return newPos;
        //}

        private float GetScaleValueREFL()
        {
            switch (m_settingsREFL.m_ResolutionMultiplier)
            {
                case ResolutionMulltiplierREFL.Full:
                    return 1f;
                case ResolutionMulltiplierREFL.Half:
                    return 0.5f;
                case ResolutionMulltiplierREFL.Third:
                    return 0.33f;
                case ResolutionMulltiplierREFL.Quarter:
                    return 0.25f;
            }
            return 0.5f; // default to half res
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlaneREFL(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * m_settingsREFL.m_ClipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        private Camera CreateMirrorObjectsREFL(Camera currentCamera)
        {
            GameObject go =
                new GameObject("Planar Refl Camera id" + GetEntityId() + " for " + currentCamera.GetEntityId(),
                    typeof(Camera));

            var reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
            reflectionCamera.allowMSAA = currentCamera.allowMSAA;
            reflectionCamera.depth = renderDepthREFL;
            //reflectionCamera.enabled = false;

            //v1.5
            UniversalAdditionalCameraData thisCameraData = reflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            if (thisCameraData != null)
            {
            }
            else
            {
                UniversalAdditionalCameraData thisCameraDataA = reflectionCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();               
            }

            if (!useVolumeRendererREFL)//v1.2
            {
                reflectionCamera.enabled = true;
            }

            reflectionCamera.allowHDR = false;// currentCamera.allowHDR;
            go.hideFlags = HideFlags.DontSave;

            reflectionCamera.depthTextureMode = DepthTextureMode.Depth;/////////////////// NEW

            return reflectionCamera;
        }

        private Vector2 ReflectionResolutionREFL(Camera cam, float scale)
        {
            var x = (int)(cam.pixelWidth * scale * GetScaleValueREFL());
            var y = (int)(cam.pixelHeight * scale * GetScaleValueREFL());
            return new Vector2(x, y);
        }

        // public void ExecuteBeforeCameraRender(
        //     LightweightRenderPipeline pipelineInstance,
        //     ScriptableRenderContext context,
        //     Camera camera)

        void applyReflections(ScriptableRenderContext context, Camera camera)
        {
            for (int i = 0; i < materialsREFL.Count; i++)
            {
                materialsREFL[i].SetTexture(planarReflectionTextureIDREFL, m_ReflectionTextureREFL);
            }

            //GL.invertCulling = false;
        }

        void applyReflectionsA(ScriptableRenderContext context, Camera[] cameras)
        {


            //GL.invertCulling = false;
        }

        //NEW
        public void DoReflections(ScriptableRenderContext context, Camera camera)
        {
            if (!enabled)
                return;

            camera = Camera.main;

            //v0.1
            //if (m_ReflectionCamera && prevRenderer != rendererID)
            //{
            //    UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            //    thisCameraData.SetRenderer(rendererID);
            //    prevRenderer = rendererID;
            //}

            GL.invertCulling = invertCulling;// true; v0.1
                                             //RenderSettings.fog = false;
            var max = QualitySettings.maximumLODLevel;
            var bias = QualitySettings.lodBias;
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.lodBias = bias * 0.5f;

            UpdateReflectionCameraREFL(camera);

            var res = ReflectionResolutionREFL(camera, 1);
            if (m_ReflectionTextureREFL == null)
            {
                m_ReflectionTextureREFL = RenderTexture.GetTemporary((int)res.x, (int)res.y, 16, RenderTextureFormat.DefaultHDR);// RenderTextureFormat.DefaultHDR);///////
            }

            if (!useVolumeRendererREFL)//v1.2
            {
                m_ReflectionCameraREFL.targetTexture = m_ReflectionTextureREFL;
            }
            else
            {
                //if (volumeRendererREFL == null || !Application.isPlaying)
                //{
                //    //volumeRendererDEPTH = volumeRendererDEPTHOBJ.GetComponent<DepthRendererSM_HDRP_VOLUME>();
                //    CustomPassVolume volume = volumeRendererREFLOBJ.GetComponent<CustomPassVolume>();
                //    volumeRendererREFL = (PlanarReflectionsSM_LWRP_VOLUME)volume.customPasses[passIDREFL];
                //}

                ////WaterAirMaskSM tempMask;
                ////if (volume.customPasses.TryGet<WaterAirMaskSM>(out tempMask))
                ////{
                ////    waterAirMask = tempMask;
                ////    //Debug.Log("Got script");
                ////}
                ////Debug.Log("11");
                //volumeRendererREFL.targetTexture = m_ReflectionTextureREFL;
                //volumeRendererREFL.bakingCamera = m_ReflectionCameraREFL;
                //m_ReflectionCameraREFL.enabled = false;
                //m_ReflectionCameraREFL.targetTexture = m_ReflectionTextureREFL;
            }

            //new
            //LightweightRenderPipeline.RenderSingleCamera(pipelineInstance, context, m_ReflectionCamera);
            //    // UnityEngine.Rendering.Universal.UniversalRenderPipeline.RenderSingleCamera( context, m_ReflectionCamera);
            //  GL.invertCulling = false;

            if (m_ReflectionCameraREFL != null && context != null)//v0.1
            {
                //v1.0.8a
                UniversalAdditionalCameraData thisCameraData = m_ReflectionCameraREFL.gameObject.GetComponent<UniversalAdditionalCameraData>();
                if (thisCameraData != null)
                {
                    //Debug.Log("Rendering REFL HAS DATA");
                    thisCameraData.SetRenderer(rendererIDREFL);
                }
                else
                {
                    UniversalAdditionalCameraData thisCameraDataA = m_ReflectionCameraREFL.gameObject.AddComponent<UniversalAdditionalCameraData>();
                    thisCameraDataA.SetRenderer(rendererIDREFL);
                }
                UnityEngine.Rendering.Universal.UniversalRenderPipeline.RenderSingleCamera(context, m_ReflectionCameraREFL);
            }
            GL.invertCulling = false;

            //RenderSettings.fog = true;
            QualitySettings.maximumLODLevel = max;
            QualitySettings.lodBias = bias;
            ////           Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture);
            for (int i = 0; i < materialsREFL.Count; i++)
            {
                materialsREFL[i].SetTexture(planarReflectionTextureIDREFL, m_ReflectionTextureREFL);
            }
        }
#endregion
        //END WATER REFLECTIONS




        //UNDERWATER EFFECTS
        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Underwater Effects ----------------------")]
#region Underwater Effects
        //v1.2a
        public float midAirShiftHeight = 180;
        Vector4 overWaterbumpParams = new Vector4(0.01f, 0.01f, 0.001f, 0.001f);
        Vector4 overWaterflowParams = new Vector4(-211, 528, 781, 1);
        Vector4 overWaterwaveParams = new Vector4(0, 0, 0, 0);
        public float overWaterPlaneHeight = 5;
        public float waterPlaneHeight = 3;

        //v1.2 - optionally setup volume depth renderer with depth camera and grab texture from there
        public bool useVolumeMaskRenderer = false;
        public bool useCameraMaskRenderer = false;
        public GameObject volumeRendererMASKOBJ; ///DepthRendererSM_HDRP_VOLUME volumeRendererDEPTH;
        //public WaterMaskRendererSM_HDRP_VOLUME volumeRendererMASK;
        public RenderTexture MaskMap;
        public int passIDMASK = 2;
        public float maskScaler = 0.2f;
        public Camera WaterMaskCamera;

        //v0.4a
        //public connectSuntoFullVolumeCloudsHDRP fullVolumeClouds;
        public bool setFog = true;
        public bool manualSetFog = false;
        public Color forColor = Color.black;

        public Transform SunLight;
        public float refractLineWidth = -1.5f;
        public float waterBlackLineWidth = 19;

        public float waterHeight;

        public float sunShaftsIntensity = 1; //v0.4a

        [Tooltip("start underwater effects below this distance to water.")]
        public float cutoffOffset = 1; //start underwater effects below this distance to water
                                       //ACCESS MASK
                                       //WaterAirMaskSM waterAirMask;

        //PASS MASK
        //SunShaftsSM_HDRP shafts;

        //FOG
        //Fog fog;
        public GameObject imageEffectsVolumeHolder;
        void UnderWaterFX_Start()
        {
            //Volume volume = imageEffectsVolumeHolder.GetComponent<Volume>();
            //WaterAirMaskSM tempMask;

            //if (volume.profile.TryGet<WaterAirMaskSM>(out tempMask))
            //{
            //    waterAirMask = tempMask;
            //    //Debug.Log("Got script");
            //}
            //if (waterAirMask != null && waterAirMask.m_TemporaryColorTexture != null)
            //{
            //    Debug.Log(waterAirMask.m_TemporaryColorTexture.name);
            //}
            ////depthOfField.focusDistance.value = 42f;

            //Fog tempFog;
            //if (volume.profile.TryGet<Fog>(out tempFog))
            //{
            //    fog = tempFog;
            //    //Debug.Log("Got FOG script");
            //}
            //SunShaftsSM_HDRP tempShafts;
            //if (volume.profile.TryGet<SunShaftsSM_HDRP>(out tempShafts))
            //{
            //    shafts = tempShafts;
            //    // Debug.Log("Got SHAFTS script");
            //}

        }
        //   public ProjectedGrid projectedGrid; !!!!!!!!!!!!!!!!!!!!!!!!!
        public bool shaftsFollowCamera = false;
        public bool smoothFadeToAir = false;

        // Update is called once per frame
        void UnderWaterFX_Update()
        {

            //v1.2 configure mask by volume renderer
            //if (shafts != null && useVolumeMaskRenderer)
            //{
            //    if (volumeRendererMASK == null || !Application.isPlaying) //pass the mask renderer
            //    {
            //        //volumeRendererDEPTH = volumeRendererDEPTHOBJ.GetComponent<DepthRendererSM_HDRP_VOLUME>();
            //        CustomPassVolume volume = volumeRendererMASKOBJ.GetComponent<CustomPassVolume>();
            //        volumeRendererMASK = (WaterMaskRendererSM_HDRP_VOLUME)volume.customPasses[passIDMASK];
            //        //shafts.volumeRendererMASK = volumeRendererMASK;
            //        shafts.useVolumeMaskRenderer.value = true;
            //        //shafts.passID = passID;
            //    }
            //    if (shafts.m_TemporaryColorTextureScaled != null)
            //    {
            //        volumeRendererMASK.targetTexture = shafts.m_TemporaryColorTextureScaled;
            //    }

            //    if (WaterMaskCamera != null)
            //    {
            //        volumeRendererMASK.bakingCamera = WaterMaskCamera;// Camera.main;
            //    }
            //    //volumeRendererMASK.bakingCamera.
            //    //shafts.volumeRendererMASK = volumeRendererMASK;
            //    shafts.useVolumeMaskRenderer.value = true;
            //    //shafts.passID = passID;
            //    if (MaskMap != null)
            //    {
            //        shafts.MaskMap.value = MaskMap;
            //        volumeRendererMASK.targetTexture = MaskMap;
            //    }
            //}
            //else
            //{
            //    shafts.useVolumeMaskRenderer.value = false;
            //}

            //v1.2
            if (useCameraMaskRenderer && WaterMaskCamera != null)
            {
                if (MaskMap != null)
                {
                    //shafts.MaskMap.value = MaskMap;
                    //WaterMaskCamera.targetTexture = MaskMap;
                }
                //shafts.useVolumeMaskRenderer.value = true;
            }

            //v1.2
            //shafts.scalerMask.value = maskScaler;

            //shafts.shaftsFollowCamera.value = shaftsFollowCamera;

            //if (SunLight != null)
            //{
            //    shafts.sunTransform.value = SunLight.position;
            //}

            float diff = Camera.main.transform.position.y - waterHeight;

            //if (smoothFadeToAir)
            //{
            //    float diffA = Camera.main.transform.position.y - shafts.waterHeight.value + 3;

            //    shafts.BumpScaleRL.value = (0.3f + Mathf.Clamp(-diffA, -6, 6) * 0.297f / 6.0f) - 0.16f;
            //    shafts.sunShaftIntensity.value = Mathf.Clamp(1f + Mathf.Clamp(-diffA, -6, 6) * 1.0f / 6.0f, 0, 1) + sunShaftsIntensity;
            //}

            if (Camera.main != null)// && projectedGrid != null)!!!!!!!!!!!!!!
            {
                if (diff < -4)
                {
                    if (heightOffset > 0)
                    {
                        heightOffset = -heightOffset;
                    }
                    heightOffset = 10;// -17;
                }
                else
                {
                    if (heightOffset < 0)
                    {
                        heightOffset = -heightOffset;
                    }
                    heightOffset = 27;
                }
            }

            if (waterBlackLineWidth > 0)
            {
                //if (Camera.main != null && diff < -10)
                //{
                //    //from - 10 in (-1.5)
                //    //to - 20   1.5

                //    if (shafts != null && shafts.active)
                //    {
                //        if (diff > -20)
                //        {
                //            shafts.refractLineWidth.value = -1.5f - (diff + 10) / 3.33f;
                //        }
                //        else
                //        {
                //            shafts.refractLineWidth.value = 1.5f;
                //        }
                //    }
                //}
                //else
                //{
                //    shafts.refractLineWidth.value = refractLineWidth;// - 1.5f;
                //}
            }

            if (Camera.main != null && diff > cutoffOffset)
            {
                ////disable effects
                //if (shafts != null && shafts.active)
                //{
                //    shafts.active = false;
                //}
                //if (fog != null && fog.active)
                //{
                //    fog.active = false;
                //}

                ////v1.2
                //if (volumeRendererMASK != null && volumeRendererMASK.enabled)
                //{
                //    volumeRendererMASK.enabled = false;
                //}
                if (WaterMaskCamera != null)
                {
                    WaterMaskCamera.gameObject.SetActive(false);
                }
            }
            else
            {
                //enable effects
                //if (shafts != null && !shafts.active)
                //{
                //    shafts.active = true;
                //}
                //if (fog != null && !fog.active)
                //{
                //    fog.active = true;
                //}

                ////v1.2
                //if (volumeRendererMASK != null && !volumeRendererMASK.enabled)
                //{
                //    volumeRendererMASK.enabled = true;
                //}
                if (WaterMaskCamera != null)
                {
                    WaterMaskCamera.gameObject.SetActive(true);
                }
            }

            float lerpSpeedA = Time.deltaTime * lerpSpeedUNDER;
            float lerpSpeedB = Time.deltaTime * lerpSpeed2;
            //v0.4a - Reduce fog intensity and clouds underwater

            if (waterMaterial != null)
            {
                //v1.2a
                //record starting material parameters when above water to restore
                if (!grabMaterialInit)
                {
                    bumpParams = waterMaterial.GetVector("_BumpTiling");
                    flowParams = waterMaterial.GetVector("_BumpDirection");
                    waveParams = waterMaterial.GetVector("_GAmplitude");
                    grabMaterialInit = true;

                    //float diff = Camera.main.transform.position.y - waterHeight;
                    if (diff > midAirShiftHeight) //v1.2a
                    {
                        Vector4 tmpA = waterMaterial.GetVector("_BumpTiling");
                        tmpA = waterMaterial.GetVector("_GAmplitude");
                        waterMaterial.SetVector("_GAmplitude", overWaterwaveParams);
                    }
                    else
                    {
                        Vector4 tmpA = waterMaterial.GetVector("_BumpTiling");
                        tmpA = waterMaterial.GetVector("_GAmplitude");
                        waterMaterial.SetVector("_GAmplitude", waveParams);
                    }

                    //waterPlaneHeight = waterMaterial.GetFloat("waterHeight");
                }

                // if (waterMaterial.HasProperty(""))
                //{
                // }

                if (manualSetFog)
                {
                    waterMaterial.SetColor("fogColor", forColor);
                }

                if (setFog)
                {
                    if (diff < -25) //v1.2 was 31
                    {
                        //fogThres
                        float tmpA = waterMaterial.GetFloat("fogThres");
                        waterMaterial.SetFloat("fogThres", Mathf.Lerp(tmpA, 5, lerpSpeedB));
                        if (diff < -4)
                        {
                            //waterMaterial.SetFloat("fogThres", Mathf.Lerp(tmpA, 5, diff));
                        }
                        else if (diff < -12 && diff > -24)
                        {
                            //Color tmpAA = waterMaterial.GetColor("fogColor");
                            //waterMaterial.SetColor("fogColor", Color.Lerp(tmpAA, new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f), (Mathf.Abs(diff)-12) / 12));
                        }

                        //fog.albedo.value = Color.Lerp(new Color(1, 1, 1), new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f), (Mathf.Abs(diff) - 25) / 180);
                        Color lightColor = new Color(114.0f / 255.0f, 193.0f / 255.0f, 207.0f / 255.0f);
                        Color darkColor = new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f);
                        waterMaterial.SetColor("fogColor", Color.Lerp(lightColor, darkColor, (Mathf.Abs(diff) - 31) / 180));
                    }
                    else if (diff > 0)//v1.2
                    {
                        float tmpA = waterMaterial.GetFloat("fogThres");
                        waterMaterial.SetFloat("fogThres", Mathf.Lerp(tmpA, 284, lerpSpeedB));
                        Color tmpAA = waterMaterial.GetColor("fogColor");
                        waterMaterial.SetColor("fogColor", Color.Lerp(tmpAA, new Color(114.0f / 255.0f, 193.0f / 255.0f, 207.0f / 255.0f), lerpSpeedB * 5));
                        //fog.albedo.value = Color.Lerp(new Color(1, 1, 1), new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f), (Mathf.Abs(diff) - 1) / 180);
                    }
                    else
                    {
                        float tmpA = waterMaterial.GetFloat("fogThres");
                        waterMaterial.SetFloat("fogThres", Mathf.Lerp(tmpA, 284 / 1, lerpSpeedB));
                        Color tmpAA = waterMaterial.GetColor("fogColor");
                        //waterMaterial.SetColor("fogColor", Color.Lerp(tmpAA, new Color(114.0f / 255.0f, 193.0f / 255.0f, 207.0f / 255.0f), lerpSpeedB * 5));

                        //fog.albedo.value = Color.Lerp(new Color(1, 1, 1), new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f), (Mathf.Abs(diff) - 1) / 180);

                        //v1.2
                        Color lightColor = new Color(114.0f / 255.0f, 193.0f / 255.0f, 207.0f / 255.0f);
                        Color darkColor = new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f);
                        //waterMaterial.SetColor("fogColor", Color.Lerp(lightColor, darkColor, (Mathf.Abs(diff) - 31) / 180));
                        //waterMaterial.SetColor("fogColor", Color.Lerp(tmpAA, lightColor , lerpSpeedB * Time.fixedTime));
                        waterMaterial.SetColor("fogColor", Color.Lerp(lightColor, darkColor, (Mathf.Abs(diff) - 31) / 180));
                        //fog.albedo.value = Color.Lerp(new Color(1, 1, 1), new Color(32.0f / 255.0f, 65.0f / 255.0f, 70.0f / 255.0f), (Mathf.Abs(diff) - 31) / 180);
                    }
                }

                if (diff > midAirShiftHeight) //v1.2a
                {
                    Vector4 tmpA = waterMaterial.GetVector("_BumpTiling");
                    // waterMaterial.SetVector("_BumpTiling", Vector4.Lerp(tmpA, overWaterbumpParams, lerpSpeedB));
                    //tmpA = waterMaterial.GetVector("_BumpDirection");
                    //waterMaterial.SetVector("_BumpDirection", Vector4.Lerp(tmpA, overWaterflowParams, lerpSpeedB));
                    tmpA = waterMaterial.GetVector("_GAmplitude");
                    waterMaterial.SetVector("_GAmplitude", Vector4.Lerp(tmpA, overWaterwaveParams, lerpSpeedB));

                    float currentWatHeight = waterMaterial.GetFloat("waterHeight");
                    waterMaterial.SetFloat("waterHeight", Mathf.Lerp(currentWatHeight, overWaterPlaneHeight, lerpSpeedB));
                }
                else
                {
                    Vector4 tmpA = waterMaterial.GetVector("_BumpTiling");
                    //waterMaterial.SetVector("_BumpTiling", Vector4.Lerp(tmpA, bumpParams, lerpSpeedB));
                    //tmpA = waterMaterial.GetVector("_BumpDirection");
                    //waterMaterial.SetVector("_BumpDirection", Vector4.Lerp(tmpA, flowParams, lerpSpeedB));
                    tmpA = waterMaterial.GetVector("_GAmplitude");
                    waterMaterial.SetVector("_GAmplitude", Vector4.Lerp(tmpA, waveParams, lerpSpeedB));

                    float currentWatHeight = waterMaterial.GetFloat("waterHeight");
                    waterMaterial.SetFloat("waterHeight", Mathf.Lerp(currentWatHeight, waterPlaneHeight, lerpSpeedB));
                }
            }
        }
        Vector4 bumpParams, flowParams, waveParams; //remove jitter when above 200m from sea
        public bool grabMaterialInit = false;
        public float lerpSpeedUNDER = 1;
        public float lerpSpeed2 = 1;
        public float cloudCoverageAboveWater = 1;
        public Material waterMaterial;
#endregion
        //END UNDERWATER EFFECTS


        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Copy preset ----------------------")]
#region Copy preset
        //HDRP 1
        public bool copyPreset = false;
        public List<Material> oceanPresets = new List<Material>();
        public int copyID = 0; //copy first list material by default
        void loadOceanMaterial()
        {
            //HDRP 1
            if (copyPreset)
            {
                if (oceanPresets.Count > 0 && copyID < oceanPresets.Count)
                {
                    Texture tex1 = oceanMaterial.GetTexture("_MainTex");
                    Texture tex2 = oceanMaterial.GetTexture("_NormalTex");
                    oceanMaterial.CopyPropertiesFromMaterial(oceanPresets[copyID]);
                    oceanMaterial.SetTexture("_MainTex", tex1);
                    oceanMaterial.SetTexture("_NormalTex", tex2);

                    //v1.6
                    WaveGenerator_Awake();
                    oceanMaterial.SetTexture("_Displacement_c0", cascade0.Displacement);
                    oceanMaterial.SetTexture("_Derivatives_c0", cascade0.Derivatives);
                    oceanMaterial.SetTexture("_Turbulence_c0", cascade0.Turbulence);

                    oceanMaterial.SetTexture("_Displacement_c1", cascade1.Displacement);
                    oceanMaterial.SetTexture("_Derivatives_c1", cascade1.Derivatives);
                    oceanMaterial.SetTexture("_Turbulence_c1", cascade1.Turbulence);

                    oceanMaterial.SetTexture("_Displacement_c2", cascade2.Displacement);
                    oceanMaterial.SetTexture("_Derivatives_c2", cascade2.Derivatives);
                    oceanMaterial.SetTexture("_Turbulence_c2", cascade2.Turbulence);
                }
                copyPreset = false;
            }
        }
#endregion


        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("------------------------- Copy preset ----------------------")]
#region Depth Renderer
        public bool enableSpecular = true;
        void updateSpecular()
        {
            if (SunLight != null)
            {
                oceanMaterial.SetVector("_WorldLightDir", SunLight.forward);
            }

            //local lights
            if(localLight != null)
            {
                //localLight
                Light lightA = localLight.GetComponent<Light>();
                float lightP = lightA.intensity;
                Color lightC = lightA.color;
                oceanMaterial.SetVector("_localLightAPos", new Vector4(localLight.transform.position.x, localLight.transform.position.y, 
                    localLight.transform.position.z, lightP * (5000) ));
                oceanMaterial.SetVector("_localLightAProps", new Vector4(lightC.r, lightC.g, lightC.b, lightA.range));
                //Debug.Log(oceanMaterial.GetVector("_localLightAPos"));
                //Debug.Log(oceanMaterial.GetVector("_localLightAProps"));
            }
            else
            {
                oceanMaterial.SetVector("_localLightAPos", new Vector4(0,0,0,0));
                oceanMaterial.SetVector("_localLightAProps", new Vector4(0, 0, 0, 0));
            }
        }
#endregion

        //v0.7
        //VOLUME FX SHADERS - MATERIALS
        public Shader volumeUnderwaterFogShader;
        public Shader volumeUnderwaterLightsShader;
        public Material volumeDistortShaftsMaterial;

        //PREFABS
        public GameObject waterPlanesPREFAB;
        public GameObject helperCamerasPREFAB;
        public GameObject skyDomePREFAB;
        public GameObject boatPREFAB;
        public GameObject causticVolumePREFAB;
        public GameObject underWaterVolumeFXPREFAB;
        public GameObject GPUParticlesPREFAB;
        public GameObject throwObjectPREFAB;

        ///////////////////////////////////////// Underwater Fog Feature SETUP /////////////////////////////////////////
        public bool setupUnderwaterFogFeature = false;
        public void setupUnderwaterFogFeatureFunc(bool disable)
        {
#if (UNITY_EDITOR)
            UniversalRenderPipelineAsset pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline);

            FieldInfo propertyInfoA = pipeline.GetType().GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            int rendererDefaultIndex = ((int)propertyInfoA?.GetValue(pipeline));
            //Debug.Log("Default renderer ID = " + rendererDefaultIndex);

            FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            ScriptableRendererData renderDATA = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[rendererDefaultIndex];

            List<ScriptableRendererFeature> features = renderDATA.rendererFeatures;
            bool foundFogFeature = false;
            for (int i = 0; i < features.Count; i++)
            {
                //if find, all good, if not set it up
                if (features[i].GetType() == typeof(GradientFogRenderFeatureOCEANIS)) ///////////////////// FEATURE
                {
                    if (disable)
                    {
                        features[i].SetActive(false);
                        setupUnderwaterFogFeature = false;
                    }
                    else
                    {
                        features[i].SetActive(true);
                        setupUnderwaterFogFeature = true;
                    }
                    foundFogFeature = true;
                }
            }
            if (foundFogFeature && !disable)
            {
                Debug.Log("The Underwater Fog forward renderer feature is already added in the Default renderer in the URP pipeline asset.");
            }
            else if (!disable)
            {
                //SET IT UP
                //if (volumeFogMaterial != null)
                {
                    GradientFogRenderFeatureOCEANIS volumeFOGFeature = ScriptableObject.CreateInstance<GradientFogRenderFeatureOCEANIS>();  ///////////// FEATURE

                    //Define settings
                    if (volumeUnderwaterFogShader != null)
                    {
                        volumeFOGFeature.settings.shader = volumeUnderwaterFogShader;
                    }
                    volumeFOGFeature.settings.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                    volumeFOGFeature.name = "Underwater Fog Oceanis";                                                    ///////////// FEATURE
                    ScriptableRendererFeature BlitVolumeFogSRPfeature = volumeFOGFeature as ScriptableRendererFeature;
                    BlitVolumeFogSRPfeature.Create();

                    AssetDatabase.AddObjectToAsset(BlitVolumeFogSRPfeature, renderDATA);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(BlitVolumeFogSRPfeature, out var guid, out long localId);
                    renderDATA.rendererFeatures.Add(BlitVolumeFogSRPfeature);
                    renderDATA.SetDirty();
                    EditorUtility.SetDirty(renderDATA);

                    Debug.Log("The Underwater Fog forward renderer feature is now added in the Default renderer in the URP pipeline asset.");
                }
            }
#endif
        }
        ///////////////////////////////////////// Underwater Volume Lights Feature SETUP /////////////////////////////////////////
        public bool setupUnderwaterVolumeLightsFeature = false;
        public void setupUnderwaterVolumeLightsFeatureFunc(bool disable)
        {
#if (UNITY_EDITOR)
            UniversalRenderPipelineAsset pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline);

            FieldInfo propertyInfoA = pipeline.GetType().GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            int rendererDefaultIndex = ((int)propertyInfoA?.GetValue(pipeline));
            //Debug.Log("Default renderer ID = " + rendererDefaultIndex);

            FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            ScriptableRendererData renderDATA = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[rendererDefaultIndex];

            List<ScriptableRendererFeature> features = renderDATA.rendererFeatures;
            bool foundFogFeature = false;
            for (int i = 0; i < features.Count; i++)
            {
                //if find, all good, if not set it up
                if (features[i].GetType() == typeof(LightShaftRenderFeatureOCEANIS)) ///////////////////// FEATURE
                {
                    if (disable)
                    {
                        features[i].SetActive(false);
                        setupUnderwaterVolumeLightsFeature = false;
                    }
                    else
                    {
                        features[i].SetActive(true);
                        setupUnderwaterVolumeLightsFeature = true;
                    }
                    foundFogFeature = true;
                }
            }
            if (foundFogFeature && !disable)
            {
                Debug.Log("The Underwater Volume Lights forward renderer feature is already added in the Default renderer in the URP pipeline asset.");
            }
            else if (!disable)
            {
                //SET IT UP
                //if (volumeFogMaterial != null)
                {
                    LightShaftRenderFeatureOCEANIS volumeFOGFeature = ScriptableObject.CreateInstance<LightShaftRenderFeatureOCEANIS>();  ///////////// FEATURE

                    //Define settings
                    if (volumeUnderwaterLightsShader != null)
                    {
                        volumeFOGFeature.settings.shader = volumeUnderwaterLightsShader;
                    }
                    volumeFOGFeature.settings.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                    volumeFOGFeature.name = "Underwater Volume Lights Oceanis";                                                    ///////////// FEATURE
                    ScriptableRendererFeature BlitVolumeFogSRPfeature = volumeFOGFeature as ScriptableRendererFeature;
                    BlitVolumeFogSRPfeature.Create();

                    AssetDatabase.AddObjectToAsset(BlitVolumeFogSRPfeature, renderDATA);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(BlitVolumeFogSRPfeature, out var guid, out long localId);
                    renderDATA.rendererFeatures.Add(BlitVolumeFogSRPfeature);
                    renderDATA.SetDirty();
                    EditorUtility.SetDirty(renderDATA);

                    Debug.Log("The Underwater Volume Lights forward renderer feature is now added in the Default renderer in the URP pipeline asset.");
                }
            }
#endif
        }
        ///////////////////////////////////////// Underwater Distort Feature SETUP /////////////////////////////////////////
        public bool setupUnderwaterDistortFeature = false;
        public void setupUnderwaterDistortFeatureFunc(bool disable)
        {
#if (UNITY_EDITOR)
            UniversalRenderPipelineAsset pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline);

            FieldInfo propertyInfoA = pipeline.GetType().GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            int rendererDefaultIndex = ((int)propertyInfoA?.GetValue(pipeline));
            //Debug.Log("Default renderer ID = " + rendererDefaultIndex);

            FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            ScriptableRendererData renderDATA = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[rendererDefaultIndex];

            List<ScriptableRendererFeature> features = renderDATA.rendererFeatures;
            bool foundFogFeature = false;
            for (int i = 0; i < features.Count; i++)
            {
                //if find, all good, if not set it up
                if (features[i].GetType() == typeof(BlitSunShaftsWaterSRP)) ///////////////////// FEATURE
                {
                    if (disable)
                    {
                        features[i].SetActive(false);
                        setupUnderwaterDistortFeature = false;
                    }
                    else
                    {
                        features[i].SetActive(true);
                        setupUnderwaterDistortFeature = true;
                    }
                    foundFogFeature = true;
                }
            }
            if (foundFogFeature && !disable)
            {
                Debug.Log("The Underwater Distort forward renderer feature is already added in the Default renderer in the URP pipeline asset.");
            }
            else if (!disable)
            {
                //SET IT UP
                //if (volumeFogMaterial != null)
                {
                    BlitSunShaftsWaterSRP volumeFOGFeature = ScriptableObject.CreateInstance<BlitSunShaftsWaterSRP>();  ///////////// FEATURE

                    //Define settings
                    if (volumeDistortShaftsMaterial != null)
                    {
                        volumeFOGFeature.settings.blitMaterial = volumeDistortShaftsMaterial;
                    }
                    volumeFOGFeature.settings.Event = RenderPassEvent.BeforeRenderingPostProcessing;
                    volumeFOGFeature.name = "Underwater Distort Oceanis";                                                    ///////////// FEATURE
                    ScriptableRendererFeature BlitVolumeFogSRPfeature = volumeFOGFeature as ScriptableRendererFeature;
                    BlitVolumeFogSRPfeature.Create();

                    AssetDatabase.AddObjectToAsset(BlitVolumeFogSRPfeature, renderDATA);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(BlitVolumeFogSRPfeature, out var guid, out long localId);
                    renderDATA.rendererFeatures.Add(BlitVolumeFogSRPfeature);
                    renderDATA.SetDirty();
                    EditorUtility.SetDirty(renderDATA);

                    Debug.Log("The UnderwaterDistort forward renderer feature is now added in the Default renderer in the URP pipeline asset.");
                }
            }
#endif
        }

        //v0.8
        public void setRenderer(Camera m_ReflectionCamera, int rendererID)
        {
            if (m_ReflectionCamera)
            {
                UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
                if (thisCameraData != null)
                {
                    thisCameraData.SetRenderer(rendererID);
                }
            }
        }

        //layer names
        public string waterLayer = "Water";
        public string waterMaskLayer = "WaterMask";
        public string terrainLayer = "Terrain";
        //public Mesh waterMeshHQ;
        public GameObject CausticsArray;
        public GameObject CausticsVolume;
        public Material causticsMat;
        public int rendererForReflections = 1;
        public int rendererWithoutImageFX = 1;
        public float setupWaterHeight = 3;
        public Material LensFlareMaterial;

        //BOAT
        public GameObject BoatPREFAB;
        public GameObject FloaterPREFAB;


        //MATERIALS
        public void copyInnerToOceanMaterial()
        {
            copyOceanMaterial(oceanMaterial, savedPropsMaterial); 
            //v1.6
            WaveGenerator_Awake();
            oceanMaterial.SetTexture("_Displacement_c0", cascade0.Displacement);
            oceanMaterial.SetTexture("_Derivatives_c0", cascade0.Derivatives);
            oceanMaterial.SetTexture("_Turbulence_c0", cascade0.Turbulence);

            oceanMaterial.SetTexture("_Displacement_c1", cascade1.Displacement);
            oceanMaterial.SetTexture("_Derivatives_c1", cascade1.Derivatives);
            oceanMaterial.SetTexture("_Turbulence_c1", cascade1.Turbulence);

            oceanMaterial.SetTexture("_Displacement_c2", cascade2.Displacement);
            oceanMaterial.SetTexture("_Derivatives_c2", cascade2.Derivatives);
            oceanMaterial.SetTexture("_Turbulence_c2", cascade2.Turbulence);
        }
        public void copyOceanMaterialToInner()
        {
            if(savedPropsMaterial == null)
            {
                Debug.Log("Creating new Inner Ocean Material");
                savedPropsMaterial = new Material(oceanMaterial);
                savedPropsMaterial.name = "Saved Ocean Props";
            }
            copyOceanMaterial(savedPropsMaterial, oceanMaterial);
        }
        public void copyOceanMaterial(Material oceanMaterial, Material sourceMat)
        {
            //HDRP 1
            if (oceanMaterial != null && sourceMat != null)
            {                
                    Texture tex1 = oceanMaterial.GetTexture("_MainTex");
                    Texture tex2 = oceanMaterial.GetTexture("_NormalTex");
                    oceanMaterial.CopyPropertiesFromMaterial(sourceMat);
                    oceanMaterial.SetTexture("_MainTex", tex1);
                    oceanMaterial.SetTexture("_NormalTex", tex2);

                    //v1.6
                    //WaveGenerator_Awake();
                    //oceanMaterial.SetTexture("_Displacement_c0", cascade0.Displacement);
                    //oceanMaterial.SetTexture("_Derivatives_c0", cascade0.Derivatives);
                    //oceanMaterial.SetTexture("_Turbulence_c0", cascade0.Turbulence);

                    //oceanMaterial.SetTexture("_Displacement_c1", cascade1.Displacement);
                    //oceanMaterial.SetTexture("_Derivatives_c1", cascade1.Derivatives);
                    //oceanMaterial.SetTexture("_Turbulence_c1", cascade1.Turbulence);

                    //oceanMaterial.SetTexture("_Displacement_c2", cascade2.Displacement);
                    //oceanMaterial.SetTexture("_Derivatives_c2", cascade2.Derivatives);
                    //oceanMaterial.SetTexture("_Turbulence_c2", cascade2.Turbulence);                
            }
        }
        public bool materialEditorActive = false;
        public Material savedPropsMaterial;
    }
}