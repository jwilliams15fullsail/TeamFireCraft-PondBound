using System;
using Artngame.RiverStudio.FastFluids.Assets.Scripts.Utils;
using UnityEngine;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
namespace Artngame.RiverStudio.FastFluids
{
    public class SimulationInfiniRIVER : MonoBehaviour
    {
        //v0.3
        public bool advectFoam = false;
        public RenderTexture advectedFoam;
        RenderTexture advectedFoamD;
        public bool updateFoam = false;
        public RenderTexture advectedFoamADD;//add foam in sources
        public float foamDissipation = 0.99f;
        public Texture2D foamTexture;
        public int foamWidth = 1024;
        public int foamHeight = 1024;
        public float foamVelocity = 1;
        public float foamBrightness = 1;
        public Vector4 foamBrightnessA = new Vector4(1,1,1,1);

        //v0.2
        public Vector2 TerrainSize = new Vector2(1024,1024);
        public Transform Terrain;
        public float sourcesPower = 30;
        public float sourcesRadius = 1;
        //v0.1
        public int advectMode = 0;//0 is no advect

        [Header("References")]
        public Material[] Materials;
        public ComputeShader ErosionComputeShader;
        public Texture2D InitialState;
        public Material InitHeightMap;

        [Header("Drawing")]
        public float BrushAmount = 0f;

        public InputModes InputMode = InputModes.AddWater;

        public enum InputModes : int
        {
            AddWater = 0,
            RemoveWater = 1,
            AddTerrain = 2,
            RemoveTerrain = 3
        }

        [Serializable]
        public class SimulationSettings
        {
            [Range(0f, 10f)]
            public float TimeScale = 1f;

            public float PipeLength = 1f / 256;
            public Vector2 CellSize = new Vector2(1f / 256, 1f / 256);

            [Range(0, 0.5f)]
            public float RainRate = 0.012f;

            [Range(0, 1f)]
            public float Evaporation = 0.015f;

            [Range(0.001f, 1000)]
            public float PipeArea = 20;

            [Range(0.1f, 20f)]
            public float Gravity = 9.81f;

            [Header("Hydraulic erosion")]
            [Range(0.1f, 3f)]
            public float SedimentCapacity = 1f;

            [Range(0.1f, 2f)]
            public float SoilSuspensionRate = 0.5f;

            [Range(0.1f, 3f)]
            public float SedimentDepositionRate = 1f;

            [Range(0f, 10f)]
            public float SedimentSofteningRate = 5f;

            [Range(0f, 40f)]
            public float MaximalErosionDepth = 10f;

            [Header("Thermal erosion")]
            [Range(0, 1000f)]
            public float ThermalErosionTimeScale = 1f;

            [Range(0, 1f)]
            public float ThermalErosionRate = 0.15f;

            [Range(0f, 1f)]
            public float TalusAngleTangentCoeff = 0.8f;

            [Range(0f, 1f)]
            public float TalusAngleTangentBias = 0.1f;

        }

        [Header("Simulation settings")]
        [Range(32, 4096)]
        public int Width = 256;
        [Range(32, 4096)]
        public int Height = 256;
        public SimulationSettings Settings;

        // State texture ARGBFloat
        // R - surface height  [0, +inf]
        // G - water over surface height [0, +inf]
        // B - Suspended sediment amount [0, +inf]
        // A - Hardness of the surface [0, 1]
        public  RenderTexture _stateTexture;
        public RenderTexture _stateTextureBuffer;

        // Output water flux-field texture
        // represents how much water is OUTGOING in each direction
        // R - flux to the left cell [0, +inf]
        // G - flux to the right cell [0, +inf]
        // B - flux to the top cell [0, +inf]
        // A - flux to the bottom cell [0, +inf]
        private RenderTexture _waterFluxTexture;

        // Output terrain flux-field texture
        // represents how much landmass is OUTGOING in each direction
        // Used in thermal erosion process
        // R - flux to the left cell [0, +inf]
        // G - flux to the right cell [0, +inf]
        // B - flux to the top cell [0, +inf]
        // A - flux to the bottom cell [0, +inf]
        private RenderTexture _terrainFluxTexture;

        // Velocity texture
        // R - X-velocity [-inf, +inf]
        // G - Y-velocity [-inf, +inf]
        private RenderTexture _velocityTexture;
        private RenderTexture _velocityTextureD;

        // List of kernels in the compute shader to be dispatched
        // Sequentially in this order
        private readonly string[] _kernelNames = {
        "RainAndControl",
        "FluxComputation",
        "FluxApply",
        "HydraulicErosion",
        "SedimentAdvection",
        "ThermalErosion",
        "ApplyThermalErosion",
        "PaintFoam"
    };

        // Kernel-related data
        private int[] _kernels;
        private uint _threadsPerGroupX;
        private uint _threadsPerGroupY;
        private uint _threadsPerGroupZ;

        // Rendering stuff
        private const string StateTextureKey = "_StateTex";

        // Brush
        private Plane _floor = new Plane(Vector3.up, Vector3.zero);
        public float _brushRadius = 0.05f;//private float _brushRadius = 0.05f;
        private Vector4 _inputControls;
        private Material _copyMat;

        void Start()
        {
            Camera.main.depthTextureMode = DepthTextureMode.Depth;

            _copyMat = new Material(Shader.Find("Hidden/CopyInfiniRIVER"));

            // Set everything up
            Initialize();

            if (advectMode == 2)
            {
                StartDetailFLUID();
            }
        }

        //v0.1
        public bool enableMouseControls = true;

        void Update()
        {
            var amount = 0f;
            var brushX = 0f;
            var brushY = 0f;

            if (enableMouseControls)//v0.1
            {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                Vector3 mousePos = Mouse.current.position.ReadValue();
                Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
#else
				Vector3 mousePos = Input.mousePosition;
                Vector2 scrollDelta = Input.mouseScrollDelta;
#endif


                // Controls
                //_brushRadius = Mathf.Clamp(_brushRadius + Input.mouseScrollDelta.y * Time.deltaTime * 0.2f, 0.01f, 1f);
                _brushRadius = Mathf.Clamp(_brushRadius + scrollDelta.y * Time.deltaTime * 0.2f, 0.01f, 1f);

                var ray = Camera.main.ScreenPointToRay(mousePos);
               

                if (_floor.Raycast(ray, out var enter))
                {
                    var hitPoint = ray.GetPoint(enter);
                    brushX = hitPoint.x / Width;
                    brushY = hitPoint.z / Height;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                    if (Mouse.current != null &&
                            (Mouse.current.leftButton.isPressed) //isPressed //wasPressedThisFrame
                    )
                    {
#else
                    if (Input.GetMouseButton(0))
                    {
#endif
                        amount = BrushAmount;
                    }
                }
                else
                {
                    amount = 0f;
                }
            }

            //v0.2
            if (amount == 0)
            {
                for (int i = 0; i < waterSources.Count; i++)
                {
                    if (i == currentSource)
                    {

                        if (Terrain != null)
                        {
                            brushX = (Width / TerrainSize.x) * (waterSources[i].position.x -Terrain.position.x) / (Width);
                            brushY = (Height / TerrainSize.y) * (waterSources[i].position.z - Terrain.position.z) / (Height);
                        }
                        else
                        {
                            brushX = (Width / TerrainSize.x) * waterSources[i].position.x / (Width);
                            brushY = (Height / TerrainSize.y) * waterSources[i].position.z / (Height);
                        }

                        amount = sourcesPower;// 30;// BrushAmount;
                        _brushRadius = sourcesRadius * 0.025f;
                    }
                }
                currentSource++;
                if (currentSource > waterSources.Count)
                {
                    currentSource = 0;
                }
            }

            _inputControls = new Vector4(brushX, brushY, _brushRadius, amount);

            //Debug.Log(_inputControls);

            Shader.SetGlobalVector("_InputControls", _inputControls);
        }

        //v0.2
        public List<Transform> waterSources = new List<Transform>();
        public int currentSource = 0;

        void FixedUpdate()
        {
            // Compute dispatch
            if (ErosionComputeShader != null)
            {
                if (Settings != null)
                {
                    // General parameters
                    ErosionComputeShader.SetFloat("_TimeDelta", Time.fixedDeltaTime * Settings.TimeScale);
                    ErosionComputeShader.SetFloat("_PipeArea", Settings.PipeArea);
                    ErosionComputeShader.SetFloat("_Gravity", Settings.Gravity);
                    ErosionComputeShader.SetFloat("_PipeLength", Settings.PipeLength);
                    ErosionComputeShader.SetVector("_CellSize", Settings.CellSize);
                    ErosionComputeShader.SetFloat("_Evaporation", Settings.Evaporation);
                    ErosionComputeShader.SetFloat("_RainRate", Settings.RainRate);

                    // Hydraulic erosion
                    ErosionComputeShader.SetFloat("_SedimentCapacity", Settings.SedimentCapacity);
                    ErosionComputeShader.SetFloat("_MaxErosionDepth", Settings.MaximalErosionDepth);
                    ErosionComputeShader.SetFloat("_SuspensionRate", Settings.SoilSuspensionRate);
                    ErosionComputeShader.SetFloat("_DepositionRate", Settings.SedimentDepositionRate);
                    ErosionComputeShader.SetFloat("_SedimentSofteningRate", Settings.SedimentSofteningRate);

                    // Thermal erosion
                    ErosionComputeShader.SetFloat("_ThermalErosionRate", Settings.ThermalErosionRate);
                    ErosionComputeShader.SetFloat("_TalusAngleTangentCoeff", Settings.TalusAngleTangentCoeff);
                    ErosionComputeShader.SetFloat("_TalusAngleTangentBias", Settings.TalusAngleTangentBias);
                    ErosionComputeShader.SetFloat("_ThermalErosionTimeScale", Settings.ThermalErosionTimeScale);

                    // Inputs
                    ErosionComputeShader.SetVector("_InputControls", _inputControls);
                    ErosionComputeShader.SetInt("_InputMode", (int)InputMode);
                }

                

                // Dispatch all passes sequentially
                foreach (var kernel in _kernels)
                {
                    if(_kernelNames[kernel] == "PaintFoam")
                    {
                        ErosionComputeShader.SetInt("_Width", foamWidth);
                        ErosionComputeShader.SetInt("_Height", foamHeight);
                        ErosionComputeShader.SetFloat("foamBrightness", foamBrightness);
                        ErosionComputeShader.SetTexture(kernel, "_foamTex", foamTexture,0);
                    }
                    else
                    {
                        ErosionComputeShader.SetInt("_Width", Width);
                        ErosionComputeShader.SetInt("_Height", Height);
                    }

                    ErosionComputeShader.Dispatch(kernel,
                        _stateTexture.width / (int)_threadsPerGroupX,
                        _stateTexture.height / (int)_threadsPerGroupY, 1);
                }


                //v0.3
                if (advectMode == 0)
                {

                }
                else if (advectMode == 1)
                {
                    if (m_advectMat != null)
                    {
                        Advect(_velocityTexture, _velocityTexture, _velocityTextureD, m_velocityDissipation);
                        Materials[1].SetTexture("_WaterFlowADVECT", _velocityTextureD);
                        Graphics.Blit(_velocityTexture, m_obstaclesTex);
                        Graphics.Blit(_velocityTextureD, _velocityTexture);

                        //v0.3
                        if (advectFoam)
                        {
                            // AdvectFOAM(_velocityTexture, advectedFoam, advectedFoam, foamDissipation);
                            // Materials[1].SetTexture("_WaterFlowFOAM", advectedFoam);
                            //Graphics.Blit(foamTexture, advectedFoam);

                            //v0.4
                            if (updateFoam)
                            {
                                ///Graphics.Blit(null, dest, m_advectFoamMat,0);
                                //m_advectFoamMat.SetTexture("_SourceFOAM", advectedFoam);
                                //m_advectFoamMat.SetTexture("_VelocityFOAM", advectedFoamADD);
                                //m_advectFoamMat.SetTexture("_foamTex", foamTexture);
                                //Graphics.Blit(advectedFoamADD, advectedFoamD, m_advectFoamMat, 1);
                                //Graphics.Blit(advectedFoamD, advectedFoam);
                                m_advectFoamMat.SetVector("foamBrightnessA", foamBrightnessA);
                                m_advectFoamMat.SetFloat("foamBrightness", foamBrightness);
                                m_advectFoamMat.SetTexture("_SourceFOAM", advectedFoam);
                                m_advectFoamMat.SetTexture("_VelocityFOAM", advectedFoamADD);
                                m_advectFoamMat.SetTexture("_foamTex", foamTexture);
                                Graphics.Blit(advectedFoamADD, advectedFoamD, m_advectFoamMat, 1);
                                Graphics.Blit(advectedFoamD, advectedFoam);

                                m_advectFoamMat.SetFloat("fluidSpeed", foamVelocity);
                                AdvectFOAM(_velocityTextureD, advectedFoam, advectedFoamD, foamDissipation);
                                Materials[1].SetTexture("_WaterFlowFOAM", advectedFoamD);
                                Graphics.Blit(advectedFoamD, advectedFoam);
                            }
                            else
                            {

                                // Graphics.Blit(foamTexture, advectedFoam);
                                AdvectFOAM(_velocityTextureD, advectedFoam, advectedFoamD, foamDissipation);
                                Materials[1].SetTexture("_WaterFlowFOAM", advectedFoamD);
                                Graphics.Blit(advectedFoamD, advectedFoam);
                            }
                        }
                    }
                }
                else if (advectMode == 2)//FULL FLUID MODE
                {
                    //Obstacles only need to be added once unless changed.
                    AddObstacles();
                    
                    int READ = 0;
                    int WRITE = 1;

                    //v0.1
                    Graphics.Blit(_velocityTexture, m_velocityTex[READ]);

                    if (!noAdvectSelf)
                    {
                        //Advect velocity against its self
                        Advect(m_velocityTex[READ], m_velocityTex[READ], m_velocityTex[WRITE], m_velocityDissipation);
                    }
                    if (!noAdvectTemperature)
                    {
                        //Advect temperature against velocity
                        Advect(m_velocityTex[READ], m_temperatureTex[READ], m_temperatureTex[WRITE], m_temperatureDissipation);
                    }
                    if (!noAdvectDensity)
                    {
                        //Advect density against velocity
                        Advect(m_velocityTex[READ], m_densityTex[READ], m_densityTex[WRITE], m_densityDissipation);
                    }

                    if (!noAdvectSelf)
                    {
                        Swap(m_velocityTex);
                    }
                    if (!noAdvectTemperature)
                    {
                        Swap(m_temperatureTex);
                    }
                    if (!noAdvectDensity)
                    {
                        Swap(m_densityTex);
                    }

                    //Determine how the flow of the fluid changes the velocity
                    if (!noBuoyancy)
                    {
                        ApplyBuoyancy(m_velocityTex[READ], m_temperatureTex[READ], m_densityTex[READ], m_velocityTex[WRITE]);
                    }

                    if (!noBuoyancy)
                    {
                        Swap(m_velocityTex);
                    }

                    //Refresh the impluse of density and temperature
                    //v0.2
                    ApplyImpulse(m_temperatureTex[READ], m_temperatureTex[WRITE], m_implusePos, m_impluseRadius, m_impulseTemperature * new Vector3(brushColor.r, brushColor.g, brushColor.b));
                    ApplyImpulse(m_densityTex[READ], m_densityTex[WRITE], m_implusePos, m_impluseRadius, m_impulseDensity * new Vector3(brushColor.r, brushColor.g, brushColor.b));

                    Swap(m_temperatureTex);
                    Swap(m_densityTex);

                    //Calculates how divergent the velocity is
                    if (!noDivergence)
                    {
                        ComputeDivergence(m_velocityTex[READ], m_divergenceTex);
                    }

                    ClearSurface(m_pressureTex[READ]);

                    if (!noJaconi)
                    {
                        int i = 0;
                        for (i = 0; i < m_numJacobiIterations; ++i)
                        {
                            Jacobi(m_pressureTex[READ], m_divergenceTex, m_pressureTex[WRITE]);
                            Swap(m_pressureTex);
                        }
                    }

                    if (!noGradient)
                    {
                        //Use the pressure tex that was last rendered into. This computes divergence free velocity
                        SubtractGradient(m_velocityTex[READ], m_pressureTex[READ], m_velocityTex[WRITE]);
                        Swap(m_velocityTex);
                    }

                    //v0.1
                    Graphics.Blit(m_velocityTex[WRITE],_velocityTexture);

                }//END FULL FLUID MODE


            }
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            /* ========= Setup computation =========== */
            // If there are already existing textures - release them
            if (_stateTexture != null)
                _stateTexture.Release();

            if (_waterFluxTexture != null)
                _waterFluxTexture.Release();

            if (_velocityTexture != null)
                _velocityTexture.Release();

            if (_velocityTextureD != null)
                _velocityTextureD.Release();

            //v0.3
            if (advectedFoam != null)
                advectedFoam.Release();

            if (advectedFoamD != null)
                advectedFoamD.Release();

            if (advectedFoamADD != null)
                advectedFoamADD.Release();

            if (m_obstaclesTex != null)
                m_obstaclesTex.Release();

            // Initialize texture for storing height map
            _stateTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            _stateTextureBuffer = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            // Initialize texture for storing flow
            _waterFluxTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            // Initialize texture for storing flow for thermal erosion
            _terrainFluxTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };

            // Velocity texture
            _velocityTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.RGFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,//filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _velocityTextureD = new RenderTexture(Width, Height, 0, RenderTextureFormat.RGFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,//filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            m_obstaclesTex = new RenderTexture(Width, Height, 0, RenderTextureFormat.RGFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,//filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            //v0.3
            advectedFoam = new RenderTexture(foamWidth, foamHeight, 0, RenderTextureFormat.DefaultHDR)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,//filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            if (!advectedFoam.IsCreated())
            {
                advectedFoam.Create();
            }
            advectedFoamD = new RenderTexture(foamWidth, foamHeight, 0, RenderTextureFormat.DefaultHDR)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,//filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            if (!advectedFoamD.IsCreated())
            {
                advectedFoamD.Create();
            }

            //v0.4
            advectedFoamADD = new RenderTexture(foamWidth, foamHeight, 0, RenderTextureFormat.DefaultHDR)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,//filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            if (!advectedFoamADD.IsCreated())
            {
                advectedFoamADD.Create();
            }

            if (!_stateTexture.IsCreated())
                _stateTexture.Create();

            if (!_waterFluxTexture.IsCreated())
                _waterFluxTexture.Create();

            if (!_terrainFluxTexture.IsCreated())
                _terrainFluxTexture.Create();

            if (!_velocityTexture.IsCreated())
                _velocityTexture.Create();

            if (!_velocityTextureD.IsCreated())
                _velocityTextureD.Create();

            if (!m_obstaclesTex.IsCreated())
                m_obstaclesTex.Create();

            if (InitialState != null)
            {
                if (InitHeightMap != null)
                    Graphics.Blit(InitialState, _stateTexture, InitHeightMap);
                else
                    Graphics.Blit(InitialState, _stateTexture);
            }

            // Setup computation shader
            if (ErosionComputeShader != null)
            {
                _kernels = new int[_kernelNames.Length];
                var i = 0;
                foreach (var kernelName in _kernelNames)
                {
                    var kernel = ErosionComputeShader.FindKernel(kernelName); ;
                    _kernels[i++] = kernel;

                    //v0.3
                    if (advectMode == 0)
                    {

                    }
                    else if (advectMode == 1)
                    {
                        if (m_advectMat != null)
                        {
                            Advect(_velocityTexture, _velocityTexture, _velocityTextureD, m_velocityDissipation);
                            Materials[1].SetTexture("_WaterFlowADVECT", _velocityTextureD);

                            //v0.3
                            if (advectFoam)
                            {
                                //v0.4
                                if (!updateFoam)
                                {
                                    Graphics.Blit(foamTexture, advectedFoam);
                                }
                                AdvectFOAM(_velocityTexture, advectedFoam, advectedFoamD, foamDissipation);
                                Materials[1].SetTexture("_WaterFlowFOAM", advectedFoamD);
                                Graphics.Blit(advectedFoamD, advectedFoam);
                            }
                        }
                    }
                    else if (advectMode == 2)
                    {

                    }

                    // Set all textures
                    ErosionComputeShader.SetTexture(kernel, "HeightMap", _stateTexture);
                    ErosionComputeShader.SetTexture(kernel, "VelocityMap", _velocityTexture);
                    ErosionComputeShader.SetTexture(kernel, "FluxMap", _waterFluxTexture);
                    ErosionComputeShader.SetTexture(kernel, "TerrainFluxMap", _terrainFluxTexture);

                    //v0.4
                    ErosionComputeShader.SetTexture(kernel, "advectedFoamADD", advectedFoamADD);//add foam in sources!
                }

                ErosionComputeShader.SetInt("_Width", Width);
                ErosionComputeShader.SetInt("_Height", Height);
                ErosionComputeShader.GetKernelThreadGroupSizes(_kernels[0], out _threadsPerGroupX, out _threadsPerGroupY, out _threadsPerGroupZ);

                
            }

            // Debug information
            DebuggerInfiniRIVER.Instance.Display("Width", Width);
            DebuggerInfiniRIVER.Instance.Display("Height", Height);
            DebuggerInfiniRIVER.Instance.Display("HeightMap", _stateTexture);
            DebuggerInfiniRIVER.Instance.Display("FluxMap", _waterFluxTexture);
            DebuggerInfiniRIVER.Instance.Display("TerrainFluxMap", _terrainFluxTexture);
            DebuggerInfiniRIVER.Instance.Display("VelocityMap", _velocityTexture);

            //v0.4
            //DebuggerInfiniRIVER.Instance.Display("advectedFoamADD", advectedFoamADD);

            /* ========= Setup Rendering =========== */
            // Assign state texture to materials
            foreach (var material in Materials)
            {
                material.SetTexture(StateTextureKey, _stateTexture);
            }
        }





        //v0.3 - Advect(m_velocityTex[READ], m_velocityTex[READ], m_velocityTex[WRITE], m_velocityDissipation);
        //v0.2
        public bool useColor = false;
        public Color brushColor = Color.white;
        public float brightness = 1;
        public Vector4 m_windDirection = new Vector4(1, 1, 0, 0);

        public Color m_fluidColor = Color.red;

        public Color m_obstacleColor = Color.white;

        public Material m_advectMat, m_buoyancyMat, m_divergenceMat, m_jacobiMat, m_impluseMat, m_gradientMat, m_obstaclesMat;

        public Material m_advectFoamMat;

        //RenderTexture m_guiTex, 
        RenderTexture m_divergenceTex, m_obstaclesTex;
        RenderTexture[] m_velocityTex, m_densityTex, m_pressureTex, m_temperatureTex;

        public float m_timeStep = 0.125f;
        public float m_impulseTemperature = 10.0f;
        public float m_impulseDensity = 1.0f;
        public float m_temperatureDissipation = 0.99f;
        public float m_velocityDissipation = 0.99f;
        public float m_densityDissipation = 0.9999f;
        public float m_ambientTemperature = 0.0f;
        public float m_smokeBuoyancy = 1.0f;
        public float m_smokeWeight = 0.05f;

        public float m_cellSize = 1.0f;
        public float m_gradientScale = 1.0f;

        Vector2 m_inverseSize;
        public int m_numJacobiIterations = 50;

        public Vector2 m_implusePos = new Vector2(0.5f, 0.0f);
        public float m_impluseRadius = 0.1f;
        public float m_mouseImpluseRadius = 0.05f;

        public Vector2 m_obstaclePos = new Vector2(0.5f, 0.5f);
        public float m_obstacleRadius = 0.1f;

        //GUITexture m_gui;

        public int m_width, m_height;
        public Vector2 m_offset;

        public bool noAdvectSelf = false;
        public bool noAdvectTemperature = false;
        public bool noAdvectDensity = false;
        public bool noBuoyancy = false;
        public bool noDivergence = false;
        public bool noJaconi = false;
        public bool noGradient = false;
        public void StartDetailFLUID()
        {
            m_offset = new Vector2(0, 0);// new Vector2(m_gui.pixelInset.x, m_gui.pixelInset.y);

            m_inverseSize = new Vector2(1.0f / m_width, 1.0f / m_height);

            m_velocityTex = new RenderTexture[2];
            m_densityTex = new RenderTexture[2];
            m_temperatureTex = new RenderTexture[2];
            m_pressureTex = new RenderTexture[2];

            CreateSurface(m_velocityTex, RenderTextureFormat.RGHalf, FilterMode.Bilinear);

            if (useColor)
            {
                CreateSurface(m_densityTex, RenderTextureFormat.ARGB32, FilterMode.Bilinear);  //v0.2
            }
            else
            {
                CreateSurface(m_densityTex, RenderTextureFormat.RHalf, FilterMode.Bilinear);
            }
            CreateSurface(m_temperatureTex, RenderTextureFormat.RHalf, FilterMode.Bilinear);
            CreateSurface(m_pressureTex, RenderTextureFormat.RHalf, FilterMode.Point);

            m_divergenceTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            m_divergenceTex.filterMode = FilterMode.Point;
            m_divergenceTex.wrapMode = TextureWrapMode.Clamp;
            m_divergenceTex.Create();

            m_obstaclesTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            m_obstaclesTex.filterMode = FilterMode.Point;
            m_obstaclesTex.wrapMode = TextureWrapMode.Clamp;
            m_obstaclesTex.Create();
        }
        void CreateSurface(RenderTexture[] surface, RenderTextureFormat format, FilterMode filter)
        {
            surface[0] = new RenderTexture(m_width, m_height, 0, format, RenderTextureReadWrite.Linear);
            surface[0].filterMode = filter;
            surface[0].wrapMode = TextureWrapMode.Clamp;
            surface[0].Create();
            surface[1] = new RenderTexture(m_width, m_height, 0, format, RenderTextureReadWrite.Linear);
            surface[1].filterMode = filter;
            surface[1].wrapMode = TextureWrapMode.Clamp;
            surface[1].Create();
        }
        void Advect(RenderTexture velocity, RenderTexture source, RenderTexture dest, float dissipation)
        {
            m_advectMat.SetVector("_InverseSize", m_inverseSize);
            m_advectMat.SetFloat("_TimeStep", m_timeStep);
            m_advectMat.SetFloat("_Dissipation", dissipation);
            m_advectMat.SetTexture("_Velocity", velocity);
            m_advectMat.SetTexture("_Source", source);
            m_advectMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_advectMat);
        }
        void AdvectFOAM(RenderTexture velocity, RenderTexture source, RenderTexture dest, float dissipation)
        {
            m_advectFoamMat.SetVector("_InverseSize", m_inverseSize);
            m_advectFoamMat.SetFloat("_TimeStep", m_timeStep);
            m_advectFoamMat.SetFloat("_Dissipation", foamDissipation);
            m_advectFoamMat.SetTexture("_Velocity", velocity);
            m_advectFoamMat.SetTexture("_Source", source);
            m_advectFoamMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_advectFoamMat,0);
        }
        void ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture dest)
        {
            m_buoyancyMat.SetTexture("_Velocity", velocity);
            m_buoyancyMat.SetTexture("_Temperature", temperature);
            m_buoyancyMat.SetTexture("_Density", density);
            m_buoyancyMat.SetFloat("_AmbientTemperature", m_ambientTemperature);
            m_buoyancyMat.SetFloat("_TimeStep", m_timeStep);
            m_buoyancyMat.SetFloat("_Sigma", m_smokeBuoyancy);
            m_buoyancyMat.SetFloat("_Kappa", m_smokeWeight);
            //v0.1
            m_buoyancyMat.SetVector("_Wind", m_windDirection);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_buoyancyMat);            
        }
        void ApplyImpulse(RenderTexture source, RenderTexture dest, Vector2 pos, float radius, Vector3 val) //v0.2
        {

            m_impluseMat.SetVector("_Point", pos);
            m_impluseMat.SetFloat("_Radius", radius);
            //m_impluseMat.SetFloat("_Fill", val);
            m_impluseMat.SetVector("_Fill", val); //v0.2
            m_impluseMat.SetTexture("_Source", source);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_impluseMat);
        }
        void ComputeDivergence(RenderTexture velocity, RenderTexture dest)
        {
            m_divergenceMat.SetFloat("_HalfInverseCellSize", 0.5f / m_cellSize);
            m_divergenceMat.SetTexture("_Velocity", velocity);
            m_divergenceMat.SetVector("_InverseSize", m_inverseSize);
            m_divergenceMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_divergenceMat);
        }
        void Jacobi(RenderTexture pressure, RenderTexture divergence, RenderTexture dest)
        {
            m_jacobiMat.SetTexture("_Pressure", pressure);
            m_jacobiMat.SetTexture("_Divergence", divergence);
            m_jacobiMat.SetVector("_InverseSize", m_inverseSize);
            m_jacobiMat.SetFloat("_Alpha", -m_cellSize * m_cellSize);
            m_jacobiMat.SetFloat("_InverseBeta", 0.25f);
            m_jacobiMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_jacobiMat);
        }
        void SubtractGradient(RenderTexture velocity, RenderTexture pressure, RenderTexture dest)
        {
            m_gradientMat.SetTexture("_Velocity", velocity);
            m_gradientMat.SetTexture("_Pressure", pressure);
            m_gradientMat.SetFloat("_GradientScale", m_gradientScale);
            m_gradientMat.SetVector("_InverseSize", m_inverseSize);
            m_gradientMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_gradientMat);
        }
        void AddObstacles()
        {
            m_obstaclesMat.SetVector("_InverseSize", m_inverseSize);
            m_obstaclesMat.SetVector("_Point", m_obstaclePos);
            m_obstaclesMat.SetFloat("_Radius", m_obstacleRadius);
            m_obstaclesTex.DiscardContents();
            ClearSurface(m_obstaclesTex);
            Graphics.Blit(null, m_obstaclesTex, m_obstaclesMat);
        }
        void ClearSurface(RenderTexture surface)
        {
            Graphics.SetRenderTarget(surface);
            GL.Clear(false, true, new Color(0, 0, 0, 0));
            Graphics.SetRenderTarget(null);
        }
        void Swap(RenderTexture[] texs)
        {
            RenderTexture temp = texs[0];
            texs[0].DiscardContents();
            texs[0] = texs[1];
            texs[1] = temp;
        }
        /*
        void Swap(RenderTexture[] texs)
        {
            RenderTexture temp = texs[0];
            texs[0].DiscardContents();
            texs[0] = texs[1];
            texs[1] = temp;
        }
        public float m_velocityDissipation = 0.99f;
        void ClearSurface(RenderTexture surface)
        {
            Graphics.SetRenderTarget(surface);
            GL.Clear(false, true, new Color(0, 0, 0, 0));
            Graphics.SetRenderTarget(null);
        }
        Vector2 m_inverseSize;
        public int m_width, m_height;
        //public int m_numJacobiIterations = 50;
        public float m_timeStep = 0.125f;
        RenderTexture m_obstaclesTex;
        //public Material m_guiMat, m_advectMat, m_buoyancyMat, m_divergenceMat, m_jacobiMat, m_impluseMat, m_gradientMat, m_obstaclesMat;
        public Material m_advectMat;
        //RenderTexture m_divergenceTex, m_obstaclesTex;
        //RenderTexture[] m_velocityTex, m_densityTex, m_pressureTex, m_temperatureTex;
        void Advect(RenderTexture velocity, RenderTexture source, RenderTexture dest, float dissipation)
        {
            m_inverseSize = new Vector2(1.0f / m_width, 1.0f / m_height);

            m_advectMat.SetVector("_InverseSize", m_inverseSize);
            m_advectMat.SetFloat("_TimeStep", m_timeStep);
            m_advectMat.SetFloat("_Dissipation", dissipation);
            m_advectMat.SetTexture("_Velocity", velocity);
            m_advectMat.SetTexture("_Source", source);
            m_advectMat.SetTexture("_Obstacles", m_obstaclesTex);

            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_advectMat);

            //Debug.Log(dest.width);

            //v0.3
            //if (m_guiTexB != null)
            //{
                //Graphics.Blit(dest, m_guiTexB);
                //m_guiTexA = dest;
            //}
        }
        */

        public enum StateChannel : int
        {
            SurfaceHeight = 0,
            Water = 1,
            SuspendedSediment = 2,
            SurfaceHardness = 3
        }

        public void UpdateStateFromTexture(Texture source,
            int sourceChannel = 0,
            StateChannel targetStateChannel = StateChannel.SurfaceHeight,
            float scale = 1f,
            float bias = 0f)
        {
            Graphics.Blit(_stateTexture, _stateTextureBuffer);
            DebuggerInfiniRIVER.Instance.Display("StateSource", source);
            _copyMat.SetTexture("_PrevTex", _stateTextureBuffer);
            _copyMat.SetTexture("_HTex", source);
            _copyMat.SetInt("_SrcChannel", sourceChannel);
            _copyMat.SetInt("_TgtChannel", (int)targetStateChannel);
            _copyMat.SetFloat("_Scale", scale);
            _copyMat.SetFloat("_Bias", bias);
            _copyMat.SetFloat("_Append", 0);
            Graphics.Blit(null, _stateTexture, _copyMat);
        }

        //v0.1
        public void UpdateStateFromTextureAPPEND(RenderTexture terrainDataA, float depthFactor, float depthOffset, float depthCamFactor, float depthScale, Texture source,
            int sourceChannel = 0,
            StateChannel targetStateChannel = StateChannel.SurfaceHeight,
            float scale = 1f,
            float bias = 0f)
        {
            Graphics.Blit(_stateTexture, _stateTextureBuffer);
            DebuggerInfiniRIVER.Instance.Display("StateSource", source);
            _copyMat.SetTexture("_AppendTex", terrainDataA);
            _copyMat.SetTexture("_PrevTex", _stateTextureBuffer);
            _copyMat.SetTexture("_HTex", source);
            _copyMat.SetInt("_SrcChannel", sourceChannel);
            _copyMat.SetInt("_TgtChannel", (int)targetStateChannel);
            _copyMat.SetFloat("_Scale", scale);
            _copyMat.SetFloat("_Bias", bias);
            _copyMat.SetFloat("_Append", 1);
            _copyMat.SetFloat("depthFactor", depthFactor);
            _copyMat.SetFloat("depthOffset", depthOffset);

            _copyMat.SetVector("_DepthCameraPos", Materials[1].GetVector("_DepthCameraPos")); //Materials[1] is water //v1.0.9e
            _copyMat.SetFloat("_TerrainScale", Materials[1].GetFloat("_TerrainScale"));
            _copyMat.SetFloat("depthCamFactor", depthCamFactor);
            _copyMat.SetFloat("depthScale", depthScale);
            //Debug.Log(Materials[1].GetVector("_DepthCameraPos") + ",," + Materials[1].GetFloat("_TerrainScale"));
            Graphics.Blit(null, _stateTexture, _copyMat);
        }
        public void UpdateSurfaceFromRenderTexture(RenderTexture terrainData)
        {
            UpdateStateFromTexture(terrainData, 0,
                StateChannel.SurfaceHeight, scale: terrainData.height * 2);
        }
        public void UpdateSurfaceFromTerrainDataAPPEND(TerrainData terrainData, RenderTexture terrainDataA, float depthFactor, float depthOffset, float depthCamFactor, float depthScale)
        {
            UpdateStateFromTextureAPPEND(terrainDataA, depthFactor, depthOffset, depthCamFactor, depthScale, terrainData.heightmapTexture, 0,
                StateChannel.SurfaceHeight, scale: terrainData.size.y * 2);
        }
        public void UpdateSurfaceFromRenderTextureAPPEND(RenderTexture terrainData, RenderTexture terrainDataA, float depthFactor, float depthOffset, float depthCamFactor, float depthScale)
        {
            UpdateStateFromTextureAPPEND(terrainDataA, depthFactor, depthOffset, depthCamFactor, depthScale, terrainData, 0,
                StateChannel.SurfaceHeight, scale: terrainData.height * 2);
        }

        public void UpdateSurfaceFromTerrainData(TerrainData terrainData)
        {
            UpdateStateFromTexture(terrainData.heightmapTexture, 0,
                StateChannel.SurfaceHeight, scale: terrainData.size.y * 2);
        }

        //v0.1
        public bool enabledGUI = false;

        public void OnGUI()
        {
            if (enabledGUI)//v0.1
            {
                var inputModes = new[] { "Add water", "Remove water", "Add Terrain", "Remove terrain" };
                GUILayout.BeginArea(new Rect(10, 10, 400, 400));
                InputMode = (InputModes)GUILayout.Toolbar((int)InputMode, inputModes);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Brush strength");
                BrushAmount = GUILayout.HorizontalSlider(BrushAmount, 1f, 100f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Brush size");
                _brushRadius = GUILayout.HorizontalSlider(_brushRadius, 0.01f, 0.2f);
                GUILayout.EndHorizontal();

                GUILayout.Label("[W][A][S][D] - Fly, hold [Shift] - fly faster");
                GUILayout.Label("hold [RMB] - rotate camera");
                GUILayout.Label("[LMB] - draw");
                GUILayout.EndArea();
            }
        }
    }
}