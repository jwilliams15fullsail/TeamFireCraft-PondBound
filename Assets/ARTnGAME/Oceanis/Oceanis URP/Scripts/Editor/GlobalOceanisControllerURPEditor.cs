using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Artngame.BrunetonsAtmosphere;
using UnityEngine.Rendering.Universal;//v1.1.8n
using Artngame.SKYMASTER;
//using Artngame.Oceanis.LensFlares;
using Artngame.Orion.LensFlaresMF;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif


namespace Artngame.Oceanis
{
    [CustomEditor(typeof(GlobalOceanisControllerURP))]
    public class GlobalOceanisControllerURPEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GlobalOceanisControllerURP oceanis = (GlobalOceanisControllerURP)target;

            GUI.color = Color.cyan * 1.3f;
            EditorGUILayout.HelpBox("--------- IMAGE EFFECTS - VOLUME -------", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Add Volume that enables Volume FX - Drawing - Cloud Shadows - Outline - Glitch - Halftone - World Position Effects", MessageType.Info);


            //UNDERWATER FOG
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey * 2;
            if (GUILayout.Button("Setup Underwater Fog Feature on Default URP pipeline renderer"))
            {
                //add volume fx if not exist
                if (oceanis.imageEffectsVolumeHolder == null)
                {
                    //instantiate and reference the volume
                    GameObject volumeFX = Instantiate(oceanis.underWaterVolumeFXPREFAB);
                    oceanis.imageEffectsVolumeHolder = volumeFX;
                }
                oceanis.setupUnderwaterFogFeatureFunc(false);
                SceneView.RepaintAll();
                EditorUtility.SetDirty(oceanis);
            }
            GUI.backgroundColor = Color.grey * 2;
            if (!oceanis.setupUnderwaterFogFeature)
            {
                GUI.backgroundColor = Color.grey * 1;
            }
            if (GUILayout.Button("Disable Feature", GUILayout.Width(115)))
            {
                oceanis.setupUnderwaterFogFeatureFunc(true);
                SceneView.RepaintAll();
                EditorUtility.SetDirty(oceanis);
            }
            EditorGUILayout.EndHorizontal();


            ////// UNDERWATER VOLUME LIGHTS
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey * 2;
            if (GUILayout.Button("Setup Underwater Volume Lights Feature on Default URP renderer"))
            {
                //add volume fx if not exist
                if (oceanis.imageEffectsVolumeHolder == null)
                {
                    //instantiate and reference the volume
                    GameObject volumeFX = Instantiate(oceanis.underWaterVolumeFXPREFAB);
                    oceanis.imageEffectsVolumeHolder = volumeFX;
                }
                oceanis.setupUnderwaterVolumeLightsFeatureFunc(false);
                SceneView.RepaintAll();
                EditorUtility.SetDirty(oceanis);
            }
            GUI.backgroundColor = Color.grey * 2;
            if (!oceanis.setupUnderwaterFogFeature)
            {
                GUI.backgroundColor = Color.grey * 1;
            }
            if (GUILayout.Button("Disable Feature", GUILayout.Width(115)))
            {
                oceanis.setupUnderwaterVolumeLightsFeatureFunc(true);
                SceneView.RepaintAll();
                EditorUtility.SetDirty(oceanis);
            }
            EditorGUILayout.EndHorizontal();


            //UNDERWATER DISTORT
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey * 2;
            if (GUILayout.Button("Setup Underwater Distort Feature on Default URP pipeline renderer"))
            {
                oceanis.setupUnderwaterDistortFeatureFunc(false);
                SceneView.RepaintAll();
                EditorUtility.SetDirty(oceanis);
            }
            GUI.backgroundColor = Color.grey * 2;
            if (!oceanis.setupUnderwaterFogFeature)
            {
                GUI.backgroundColor = Color.grey * 1;
            }
            if (GUILayout.Button("Disable Feature", GUILayout.Width(115)))
            {
                oceanis.setupUnderwaterDistortFeatureFunc(true);
                SceneView.RepaintAll();
                EditorUtility.SetDirty(oceanis);
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.grey * 2;
            EditorGUILayout.HelpBox("Make sure the Default renderer is the one the scene camera will use before add the renderer features. The system also requires a camera" +
                " tagged as MainCamera in the scene.", MessageType.Info);
            EditorGUILayout.HelpBox("Make sure the renderers defined in the Depth and Reflections do not have any image effects or for reflections image effects should be " +
                "compatible with planar reflections, for example Sky Master ULTIMATE clouds have a special cloud renderer feature for that purpose.", MessageType.Info);

            //v1.5
            if (GUILayout.Button("Set Reflections renderer ID again", GUILayout.Width(215)))
            {
                //oceanis.setRenderer(GlobalOceanisControllerURP.m_ReflectionCameraREFL, oceanis.rendererForReflections);
                oceanis.rendererIDREFL = oceanis.rendererForReflections;
            }
            if (GUILayout.Button("Set Depth Renderer ID again", GUILayout.Width(215)))
            {
                //oceanis.setRenderer(GlobalOceanisControllerURP.m_ReflectionCamera, oceanis.rendererWithoutImageFX);
                oceanis.rendererID = oceanis.rendererWithoutImageFX;
            }
            oceanis.rendererForReflections = EditorGUILayout.IntSlider("Renderer ID for Reflections", oceanis.rendererForReflections, 0, 10);
            oceanis.rendererWithoutImageFX = EditorGUILayout.IntSlider("Depth Renderer (No Image FX)", oceanis.rendererWithoutImageFX, 0, 10);
            oceanis.setupWaterHeight = EditorGUILayout.Slider("Water Height", oceanis.setupWaterHeight, -5, 5);

            GUI.color = Color.cyan * 1.4f;
            EditorGUILayout.HelpBox("--------- OCEAN MODULES SETUP -------", MessageType.None);
            GUI.color = Color.white;

            //Add water planes
            GUI.backgroundColor = Color.grey * 2;
            if (GUILayout.Button("Add Water Planes", GUILayout.Width(215)))
            {
                oceanis.gameObject.name = "OCEANIS CONTROLLER";
                oceanis.gameObject.transform.position = Vector3.zero;// + 3 * Vector3.up;
                MeshRenderer meshr = oceanis.gameObject.GetComponentInChildren<MeshRenderer>();
                MeshFilter meshrF = oceanis.gameObject.GetComponentInChildren<MeshFilter>();
                if (meshr != null)
                {
                    meshr.enabled = false;
                    meshr.gameObject.SetActive(false);
                }
                GameObject waterPlanes = Instantiate(oceanis.waterPlanesPREFAB);
                waterPlanes.transform.parent = oceanis.gameObject.transform;
                waterPlanes.transform.localPosition = Vector3.zero;
                waterPlanes.transform.localScale = Vector3.one;
                //MeshFilter[] meshrs = oceanis.gameObject.GetComponentsInChildren<MeshFilter>(true);
                //for (int i=0;i< meshrs.Length; i++)
                //{
                //    meshrs[i].sharedMesh = oceanis.waterMeshHQ;// meshrF.sharedMesh;
                //    meshrs[i].mesh = oceanis.waterMeshHQ;//meshrF.sharedMesh;
                //}

                //fix bounds
                RecalcBOUNDS_SM[] bounds = oceanis.gameObject.GetComponentsInChildren<RecalcBOUNDS_SM>(true);
                for (int i = 0; i < bounds.Length; i++)
                {
                    bounds[i].Start();
                    bounds[i].Update();
                    if (!bounds[i].gameObject.name.Contains("MASK"))
                    {
                        bounds[i].alwaysCalcBounds = false;
                        bounds[i].enabled = true;
                        bounds[i].calcBounds = true;
                        bounds[i].realtiveBoundsCalc = false;
                    }
                    else
                    {
                        bounds[i].alwaysCalcBounds = false;
                        bounds[i].enabled = true;
                        bounds[i].calcBounds = true;
                        bounds[i].realtiveBoundsCalc = false;
                    }
                }

                //DEPTH material
                oceanis.Materials.Clear();
                oceanis.Materials.Add(oceanis.m_oceanMaterial);

                oceanis.GernstnerDisable();

                //set grid best properties
                oceanis.minAngleSpeed = 15;
                oceanis.pixelsPerQuad = 4;
                oceanis.forwardOffset = 36;
                oceanis.heightOffset = 27;
                oceanis.heightScale = 0.2f;
                oceanis.maxheight = 372;
                oceanis.updateEvery = 4;
                oceanis.updateEveryRot = 0;
                oceanis.eulerX = 79.4f;
                oceanis.heightScaler = 300;
                oceanis.autoAdjustGrid = true;

                oceanis.depthPyramidScale.y = 1;

                if (Camera.main != null)
                {
                    Camera.main.farClipPlane = 10000;
                    Camera.main.transform.position += new Vector3(0, 45, 0);
                }

                oceanis.lengthScale0 = 550;
                oceanis.lengthScale1 = 110;
                oceanis.lengthScale2 = 5;
                oceanis.alwaysRecalculateInitials = true;

                //v1.5
                oceanis.copyOceanMaterialToInner();
            }

            if (GUILayout.Button("Setup Sky Dome", GUILayout.Width(215)))
            {
                GameObject skyDome = Instantiate(oceanis.skyDomePREFAB);
                oceanis.m_skyGO = skyDome;
                oceanis.updateSky = true;
                if (oceanis.SUN != null)
                {
                    oceanis.SUN.eulerAngles = new Vector3(9, 170, 0);
                    skyDome.GetComponent<Sky>().m_sun = oceanis.SUN.gameObject;
                    skyDome.GetComponent<Sky>().m_sunIntensity = 8;
                    skyDome.transform.localScale = new Vector3(1, 1, 1) * 120000;

                    //skyDome.layer = 0;
                    skyDome.layer = LayerMask.NameToLayer("Terrain");//v0.8a

                    if (Camera.main != null)
                    {
                        Camera.main.farClipPlane = 140000; //120000;
                        Camera.main.nearClipPlane = 3;
                    }
                }
                else
                {
                    //find sun
                    Light[] foundLightObjects = FindObjectsOfType<Light>();
                    for (int i = 0; i < foundLightObjects.Length; i++)
                    {
                        if (foundLightObjects[i].type == LightType.Directional)
                        {
                            oceanis.SUN = foundLightObjects[i].transform;
                            oceanis.SUN.eulerAngles = new Vector3(9, 170, 0);
                            skyDome.GetComponent<Sky>().m_sun = oceanis.SUN.gameObject;
                            skyDome.GetComponent<Sky>().m_sunIntensity = 8;
                            skyDome.transform.localScale = new Vector3(1, 1, 1) * 120000;

                            //skyDome.layer = 0;
                            skyDome.layer = LayerMask.NameToLayer("Terrain");//v0.8a

                            if (Camera.main != null)
                            {
                                Camera.main.farClipPlane = 120000;
                            }
                            break;
                        }
                    }
                }
            }

            //REFLECTIONS
            if (GUILayout.Button("Setup Reflections", GUILayout.Width(215)))
            {
                //v0.8a
                //oceanis.setRenderer(oceanis.m_ReflectionCameraREFL, oceanis.rendererIDwithoutImageFX);
                oceanis.setRenderer(GlobalOceanisControllerURP.m_ReflectionCameraREFL, oceanis.rendererForReflections);
                //oceanis.m_settingsREFL.m_ReflectLayers = LayerMask.NameToLayer("Terrain");
                oceanis.m_settingsREFL.m_ReflectLayers = 1 << LayerMask.NameToLayer("Terrain");
                oceanis.refractionTextureNameREFL = "_ReflectionTex";
                oceanis.reflectPowerTiling = new Vector4(150, 40, 1, 1);//new Vector4(345,5,1,1);
                oceanis.materialsREFL.Clear();
                oceanis.materialsREFL.Add(oceanis.m_oceanMaterial);
                oceanis.rendererIDREFL = oceanis.rendererForReflections; // v1.5
            }

            //DEPTH RENDERER
            if (GUILayout.Button("Top-Down Depth Renderer", GUILayout.Width(215)))
            {
                oceanis.doDepthRendering = true;
                oceanis.updateVolumePerDistance = true;
                oceanis.renderOnce = true;
                oceanis.disableAfterStart = true;
                oceanis.disableDepthRendering = false;
                oceanis.updatePerDistance = true;
                oceanis.updateDistance = 1;
                oceanis.depthUnderWater = 22;
                //oceanis.Materials.Clear();
                //oceanis.Materials.Add(oceanis.m_oceanMaterial);
                oceanis.depthTextureName = "_ShoreContourTex";
                oceanis.refractionTextureName = "_ShoreContourTex";
                oceanis.m_settings.m_ReflectLayers = 1 << LayerMask.NameToLayer("Terrain"); //LayerMask.NameToLayer("Terrain");
                oceanis.clearAtStart = true;
                oceanis.renderDepth = 2;
                oceanis.rendererID = oceanis.rendererWithoutImageFX; //SET RENDERER without any image effect
                oceanis.setRenderer(GlobalOceanisControllerURP.m_ReflectionCamera, oceanis.rendererWithoutImageFX);
                oceanis.rendererID = oceanis.rendererWithoutImageFX;//v1.5
            }

            //IMAGE FX
            if (GUILayout.Button("Setup Image Effects", GUILayout.Width(215)))
            {
                GameObject effects = Instantiate(oceanis.underWaterVolumeFXPREFAB);
                effects.layer = 0;
                if (Camera.main != null)
                {
                    UniversalAdditionalCameraData dataURP = Camera.main.GetComponent<UniversalAdditionalCameraData>();
                    if (dataURP != null)
                    {
                        dataURP.renderPostProcessing = true;
                    }
                }
            }

            //IMAGE FX
            if (GUILayout.Button("Setup Water Mask", GUILayout.Width(215)))
            {
                GameObject extraCameras = Instantiate(oceanis.helperCamerasPREFAB);

                //v0.8a
                oceanis.cameraNearClipPlane = 6;

                Camera[] Cameras = extraCameras.GetComponentsInChildren<Camera>(true);
                for (int i = 0; i < Cameras.Length; i++)
                {
                    if (Cameras[i].targetTexture.name.Contains("Mask"))
                    {
                        oceanis.WaterMaskCamera = Cameras[i];

                        //v0.8
                        oceanis.setRenderer(oceanis.WaterMaskCamera, oceanis.rendererWithoutImageFX);

                        //v1.5
                        Cameras[i].nearClipPlane = 0.3f;
                    }
                    else
                    {
                        Cameras[i].enabled = false;
                    }
                }

                if (Camera.main != null)
                {
                    //remove maks layer
                    //LayerMask newMask = Camera.main.cullingMask & ~(1 << LayerMask.NameToLayer("WaterMask"));//oldMask & ~(1 << 9);
                    LayerMask newMask = Camera.main.cullingMask & ~(1 << LayerMask.NameToLayer(oceanis.waterMaskLayer));

                    Camera.main.cullingMask = newMask;

                    connectSuntoSunShaftsWaterURP distortion = Camera.main.gameObject.AddComponent<connectSuntoSunShaftsWaterURP>();
                    distortion.sun = oceanis.SUN;
                    distortion.useWaterAirMask = false;
                    distortion.useVolumeMaskRenderer = true;
                    distortion.BumpScale = 0.2f;
                    distortion.refractLineXDispA = 0;
                    distortion.autoRegulateEffect = true;
                    distortion.cutoffHeigth = 5;

                    distortion.scalerMask = 1;
                    distortion.waterHeight = oceanis.setupWaterHeight;
                    distortion.useVolumeMaskRenderer = true;
                    oceanis.blackLine.w = 350;
                    oceanis.waterHeight = oceanis.setupWaterHeight + 2;
                    oceanis.useCameraMaskRenderer = true;
                    oceanis.useVolumeMaskRenderer = false;
                    oceanis.waterPlaneHeight = oceanis.setupWaterHeight;////////////////////////// set global

                    cycleCaustucsTexture caustics = Camera.main.gameObject.AddComponent<cycleCaustucsTexture>();
                    cycleCaustucsTexture causticsSOURCE = oceanis.CausticsArray.GetComponent<cycleCaustucsTexture>();
                    caustics.sun = oceanis.SUN;
                    caustics.Caustics = new Texture2D[causticsSOURCE.Caustics.Length];
                    for (int i = 0; i < causticsSOURCE.Caustics.Length; i++)
                    {
                        caustics.Caustics[i] = causticsSOURCE.Caustics[i];
                    }

                    caustics.lightCookieIntensity = 0;
                    caustics.customCookieIntensity = 0;
                    caustics.customCookieIntensityA = 1;// -0.3f;
                    caustics.customCookieScaler = 2;
                    caustics.customCookieScalerA = 4.5f;// 2.4f;
                    caustics.applyToLightCookie = true;

                    extraCameras.transform.parent = Camera.main.transform;
                    extraCameras.transform.localPosition = Vector3.zero;
                    extraCameras.transform.localRotation = Quaternion.identity;

                    //v1.5
                    //extraCameras.GetComponent<Camera>().nearClipPlane = 0.3f;
                    Camera.main.nearClipPlane = 0.7f;
                }
            }

            if (GUILayout.Button("Setup Lens Flares", GUILayout.Width(215)))
            {
                if (oceanis.SUN != null)
                {
                    if (Camera.main != null)
                    {
                        //v1.5
                        MFFlareLauncher flares = oceanis.SUN.gameObject.AddComponent<MFFlareLauncher>();
                        MFLensFlare flaresCAMERA = Camera.main.gameObject.AddComponent<MFLensFlare>();
                        
                        flares.directionalLight = true;
                        flares.useLightIntensity = true;
                        if (flaresCAMERA.lightSource == null)
                        {
                            flaresCAMERA.lightSource = new List<MFFlareLauncher>();
                        }
                        flaresCAMERA.lightSource.Add(flares);
                        flares.OnEnable();
                        //
                        
                        FlareState Fstate = new FlareState();
                        flaresCAMERA.flareDatas.Add(Fstate);
                        flaresCAMERA.material = oceanis.LensFlareMaterial;
                        flaresCAMERA.maxFadeOutScale = 0.55f;
                    }
                }
            }

            if (GUILayout.Button("Setup Caustics Volume", GUILayout.Width(215)))
            {
                GameObject Caustics = Instantiate(oceanis.CausticsVolume);
                Caustics.transform.position = new Vector3(0, -65, 220);
            }

            if (GUILayout.Button("Setup Boat", GUILayout.Width(215)))
            {
                GameObject Boat = Instantiate(oceanis.BoatPREFAB);
                Boat.transform.position = new Vector3(0, 0, 220);
                oceanis.heightSampler = Boat.transform;
                oceanis.controlBoat = true;
                oceanis.boatLength = 20;
                oceanis.lerpRotSpeed = 0.0005f;
                oceanis.lerpSpeed = 0.1f;
                oceanis.BoatSpeed = 2;
                oceanis.BoatRotSpeed = 250;
            }
            if (GUILayout.Button("Setup Throwable Floaters", GUILayout.Width(215)))
            {
                GameObject Floater = Instantiate(oceanis.FloaterPREFAB);
                oceanis.castFloaters = true;
                oceanis.heightOffsetY = -0.85f;
                oceanis.heightFactor1 = 0.8f;
                oceanis.ShiftHorPositionFloaters = true;
                oceanis.alignToNormalFloaters = true;
                oceanis.alignToNormalFactor = new Vector3(4, -5, 4);
                oceanis.ThrowItem = Floater;
            }

            EditorGUILayout.HelpBox("After adding the modules, make sure to enter Play Mode to reset all systems.", MessageType.Info);

            GUI.color = Color.cyan * 1.6f;
            EditorGUILayout.HelpBox("--------- OCEAN MATERIAL -------", MessageType.None);
            GUI.color = Color.white;

            //MATERIALS
            if (GUILayout.Button("Save material Props", GUILayout.Width(215)))
            {
                oceanis.copyOceanMaterialToInner();
            }
            if (GUILayout.Button("Load material Props", GUILayout.Width(215)))
            {
                oceanis.copyInnerToOceanMaterial();
                SceneView.RepaintAll();
                SceneView.lastActiveSceneView.Repaint();
                //SceneView.lastActiveSceneView.Focus();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            if (GUILayout.Button("Edit Ocean Material"))//, GUILayout.Width(215*2)))
            {
                if (oceanis.materialEditorActive)
                {
                    oceanis.materialEditorActive = false;
                }
                else
                {
                    oceanis.materialEditorActive = true;
                }
            }

            GUI.color = Color.cyan*1.8f;
            EditorGUILayout.HelpBox("--------- OCEAN SETTINGS -------", MessageType.None);
            GUI.color = Color.white;

            //Undo.RecordObject(oceanis, "Ocean Material Change");
            // Undo.RecordObject(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
            //Undo.RecordObject(oceanis.m_oceanMaterial, "Ocean Material Changes");
            //Undo.RegisterCompleteObjectUndo(oceanis.m_oceanMaterial, "Ocean Inner Material Changeaa");
            //Undo.RegisterCompleteObjectUndo(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
            //Undo.RegisterCompleteObjectUndo(oceanis, "Ocean Material Change");
            //Undo.RegisterUndo(oceanis.savedPropsMaterial, "Assigning Textures");
            if (oceanis.materialEditorActive)
            {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                if (Mouse.current != null &&
                        (!Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasPressedThisFrame) //isPressed
                )
                {
#else
                if (!Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) //if (scaleAX != oceanis.lengthScales.x)
                {
#endif
                    //
                    
                    if (oceanis.m_sky != null)
                    {
                        Object[] undoObjects = new Object[2];
                        undoObjects[0] = oceanis;
                        undoObjects[1] = oceanis.m_sky;
                        Undo.RecordObjects(undoObjects, "Oceanis parameter change");
                    }
                    else
                    {
                        Undo.RecordObject(oceanis, "Oceanis parameter change");
                    }
                   
                }

                //SKY
                if (oceanis.m_sky != null)
                {
                    oceanis.m_sky.m_mieG = EditorGUILayout.Slider("Sky Mie Focus", oceanis.m_sky.m_mieG, -2, 2);
                    oceanis.m_sky.m_sunIntensity = EditorGUILayout.Slider("Sky Sun Intensity", oceanis.m_sky.m_sunIntensity, -2, 20);
                }

                //WAVES control
                float scaleAX = oceanis.lengthScales.x;
                float scaleAY = oceanis.lengthScales.y;
                float scaleAZ = oceanis.lengthScales.z;
                float scaleAW = oceanis.lengthScales.w;

                oceanis.lengthScales.x = EditorGUILayout.Slider("Wave Scale Large", scaleAX, 0, 10); 
                oceanis.lengthScales.y = EditorGUILayout.Slider("Wave Scale Medium", scaleAY, 0, 10);
                oceanis.lengthScales.z = EditorGUILayout.Slider("Wave Scale Small", scaleAZ, 0, 10);
                oceanis.lengthScales.w = EditorGUILayout.Slider("Wave Scale Ripple", scaleAW, 0, 10);
                oceanis.lodScale = EditorGUILayout.Slider("Wave LODs Scaling", oceanis.lodScale, 1, 50);
                oceanis.lengthScale0 = EditorGUILayout.Slider("Large Wave Scaling",  oceanis.lengthScale0, 1, 10000);
                oceanis.lengthScale1 = EditorGUILayout.Slider("Medium Wave Scaling", oceanis.lengthScale1, 1, 5000);
                oceanis.lengthScale2 = EditorGUILayout.Slider("Small Wave Scaling",  oceanis.lengthScale2, 1, 2000);

               

                //DEPTH
                //oceanis.m_oceanMaterial.SetFloat("depthFadePower", EditorGUILayout.Slider("Depth Fade Strength", oceanis.m_oceanMaterial.GetFloat("depthFadePower"), -20, 20));
                handleFloatVariable("Depth Fade Strength", oceanis, "depthFadePower", -20, 20);

                //v1.5 - PRO - FULL FFT
                //oceanis.m_oceanMaterial.SetColor("_Color", EditorGUILayout.ColorField("Water Color Realistic", oceanis.m_oceanMaterial.GetColor("_Color")));
                //oceanis.m_oceanMaterial.SetColor("_SSSColor", EditorGUILayout.ColorField("Sub Surface Color Realistic", oceanis.m_oceanMaterial.GetColor("_SSSColor")));
                handleColorVariable("Water Color Realistic", oceanis, "_Color");
                handleColorVariable("Sub Surface Color Realistic", oceanis, "_SSSColor");

                //oceanis.m_oceanMaterial.SetFloat("_SSSStrength", EditorGUILayout.Slider("Sub Surface Strength", oceanis.m_oceanMaterial.GetFloat("_SSSStrength"), 0, 2));
                //oceanis.m_oceanMaterial.SetFloat("_SSSScale", EditorGUILayout.Slider("Sub Surface Scale", oceanis.m_oceanMaterial.GetFloat("_SSSScale"), 0.1f, 50));
                //oceanis.m_oceanMaterial.SetFloat("_SSSBase", EditorGUILayout.Slider("Sub Surface Base", oceanis.m_oceanMaterial.GetFloat("_SSSBase"), -5, 2));

                //oceanis.m_oceanMaterial.SetFloat("_LOD_scale", EditorGUILayout.Slider("Detail LOD Scale", oceanis.m_oceanMaterial.GetFloat("_LOD_scale"), 1, 20));
                //oceanis.m_oceanMaterial.SetFloat("_MaxGloss", EditorGUILayout.Slider("Water Gloss Realistic", oceanis.m_oceanMaterial.GetFloat("_MaxGloss"), -2, 4));

                //oceanis.m_oceanMaterial.SetFloat("_Roughness", EditorGUILayout.Slider("Roughness", oceanis.m_oceanMaterial.GetFloat("_Roughness"), 0, 1));
                //oceanis.m_oceanMaterial.SetFloat("_RoughnessScale", EditorGUILayout.Slider("Roughness Scale", oceanis.m_oceanMaterial.GetFloat("_RoughnessScale"), 0, 0.5f));

                handleFloatVariable("Sub Surface Strength", oceanis, "_SSSStrength", 0, 2);
                handleFloatVariable("Sub Surface Scale", oceanis, "_SSSScale", 0.1f, 50);
                handleFloatVariable("Sub Surface Base", oceanis, "_SSSBase", -5, 2);
                handleFloatVariable("Detail LOD Scale", oceanis, "_LOD_scale", 1, 20);
                handleFloatVariable("Water Gloss Realistic", oceanis, "_MaxGloss", -2, 4);
                handleFloatVariable("Roughness", oceanis, "_Roughness", 0, 1);
                handleFloatVariable("Roughness Scale", oceanis, "_RoughnessScale", 0, 0.5f);

                handleFloatVariable("Ocean color tint", oceanis, "_MultiplyEffect", -150, 150);
                //float FoamIntensity = oceanis.m_oceanMaterial.GetFloat("_FoamBiasLOD2");
                //float FoamIntensityALT = EditorGUILayout.Slider(FoamIntensity, 0,7);
                //oceanis.m_oceanMaterial.SetFloat("_FoamBiasLOD2", FoamIntensityALT);

                //handleFloatVariable("Foam Amount Stylistic", oceanis, "_Foam", -5, 10);
                handleVector4tVariable("Stylistic Foam", "Foam Height", "Foam tiling", "(TBA)", oceanis, "_Foam",
                   new Vector2(-4, 4), new Vector2(-4, 4), new Vector2(-4, 4), new Vector2(-4, 4));

                handleColorVariable("Foam Color Realistic", oceanis, "_FoamColor");
                //oceanis.m_oceanMaterial.SetColor("_FoamColor", EditorGUILayout.ColorField("Foam Color Realistic", oceanis.m_oceanMaterial.GetColor("_FoamColor")));

                handleFloatVariable("Foam Amount Realistic", oceanis, "_FoamBiasLOD2", 0, 7);
                //void handleFloatVariable(GlobalOceanisControllerURP oceanis, string variableName);
                /*
                oceanis.m_oceanMaterial.SetFloat("_FoamBiasLOD2", EditorGUILayout.Slider("Foam Amount Realistic",oceanis.m_oceanMaterial.GetFloat("_FoamBiasLOD2"), 0, 7));
                if (oceanis.m_oceanMaterial.GetFloat("_FoamBiasLOD2") != oceanis.savedPropsMaterial.GetFloat("_FoamBiasLOD2"))
                {
                    if (!Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                    {
                        //Undo.RecordObject(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
                        //Undo.RecordObject(oceanis.m_oceanMaterial, "Ocean Inner Material Changes");
                        Undo.RegisterCompleteObjectUndo(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
                        Undo.RegisterCompleteObjectUndo(oceanis.m_oceanMaterial, "Ocean Inner Material Changed");
                        //Undo.RegisterCompleteObjectUndo(oceanis, "Ocean Inner Material Changed A");
                        oceanis.savedPropsMaterial.SetFloat("_FoamBiasLOD2", oceanis.m_oceanMaterial.GetFloat("_FoamBiasLOD2"));
                        Undo.FlushUndoRecordObjects();
                    }
                }
                */
                //oceanis.savedPropsMaterial.SetFloat("_FoamBiasLOD2", oceanis.m_oceanMaterial.GetFloat("_FoamBiasLOD2"));
                //oceanis.savedPropsMaterial.

                handleFloatVariable("Foam Scale Realistic", oceanis, "_FoamScale",    0, 50);
                handleFloatVariable("Contact Foam Amount", oceanis, "_ContactFoam", -2, 5);
                //oceanis.m_oceanMaterial.SetFloat("_FoamScale", EditorGUILayout.Slider("Foam Scale Realistic", oceanis.m_oceanMaterial.GetFloat("_FoamScale"), 0, 50));
                //oceanis.m_oceanMaterial.SetFloat("_ContactFoam", EditorGUILayout.Slider("Contact Foam Amount", oceanis.m_oceanMaterial.GetFloat("_ContactFoam"), -2, 5));

                //shoreGlow("Shore lighting, Height, Divider, Power"
                handleVector4tVariable("Shore lighting", "Shore Height", "Shore Divider", "Shore Power", oceanis, "shoreGlow", 
                    new Vector2(-4,4), new Vector2(-4, 4), new Vector2(-4, 4), new Vector2(-24, 24));

                //BASE COLOR
                handleColorVariable("Base Water Color", oceanis, "_BaseColor");
                //REFLECT COLOR TINT
                handleColorVariable("Reflection Tint Color", oceanis, "_ReflectionColor");

                //REFLECTIONS
                oceanis.reflectPowerTiling.x = EditorGUILayout.Slider("Water Reflection Power", oceanis.reflectPowerTiling.x, 0, 6000);

                //SPECULAR
                handleColorVariable("Specular Sun Color", oceanis, "_SpecularColor");//FresnelFactor - _FresnelScale
                handleFloatVariable("Fresnel Factor", oceanis, "FresnelFactor", -10,100);
                handleFloatVariable("Fresnel Scale", oceanis, "_FresnelScale", -10, 100);

                //REFLECT - REFRACT
                oceanis.reflectPowerTiling.y = EditorGUILayout.Slider("Reflection Distortion", oceanis.reflectPowerTiling.y, -50, 100);
                handleVector4tVariable("Water Bump Power", "Refract scale", "Fresnel", "Fresnel Factor", oceanis, "_DistortParams",
                    new Vector2(-4, 25), new Vector2(-4, 25), new Vector2(-4,25), new Vector2(-4, 25));

                handleColorVariable("Specular Sun Color Realistic", oceanis, "_controlSkyColor");
                //_controlSkyRadiance("Control Sky Radiance, base, sky, normal, tiling",
                handleVector4tVariable("Base Radiance", "Sky Radiance", "Sky Specular", "Sky Tiling (TBA)", oceanis, "_controlSkyRadiance",
                    new Vector2(-4, 15), new Vector2(-4, 15), new Vector2(-500000, 500000), new Vector2(-4, 4));
                handleVector4tVariable("Sky Power (TBA)", "Radiance Distance", "Specular Power", "Sky Tiling (TBA)", oceanis, "_controlSkyRadianceA",
                    new Vector2(-4, 4), new Vector2(0,500), new Vector2(-40000, 40000), new Vector2(-4, 4));


                handleFloatVariable("depth Fade Power", oceanis, "depthFadePower", -10, 10);
                handleVector4tVariable("Default Normal", "Extra normal A", "Extra normal B", "Extra normal Power", oceanis, "_NormalsOffset3DControls",
                   new Vector2(-4, 4), new Vector2(-4, 4), new Vector2(-4, 4), new Vector2(-4, 4));

                EditorGUILayout.HelpBox("--------- --------------------- -------", MessageType.None);
            }

            

            EditorGUILayout.HelpBox("--------- ADVANCED SETTINGS -------", MessageType.None);
            GUI.backgroundColor = Color.grey * 2;
            DrawDefaultInspector();
            //EditorUtility.SetDirty(tree);
        }

        void handleFloatVariable(string title, GlobalOceanisControllerURP oceanis, string variableName, float min, float max)
        {
            float paramToChange = EditorGUILayout.Slider(title, oceanis.m_oceanMaterial.GetFloat(variableName), min, max);
            //oceanis.m_oceanMaterial.SetFloat(variableName, EditorGUILayout.Slider(title, oceanis.m_oceanMaterial.GetFloat(variableName), min, max));
            //if (oceanis.m_oceanMaterial.GetFloat(variableName) != oceanis.savedPropsMaterial.GetFloat(variableName))
            if (paramToChange != oceanis.savedPropsMaterial.GetFloat(variableName))
            {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                if (Mouse.current != null &&
                        (!Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasPressedThisFrame) //isPressed
                )
                {
#else
                if (!Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                {
#endif
                    //Undo.RecordObject(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
                    //Undo.RecordObject(oceanis.m_oceanMaterial, "Ocean Inner Material Changes");
                    Undo.RegisterCompleteObjectUndo(oceanis.savedPropsMaterial, title + " Ocean Inner Material Changed");
                    Undo.RegisterCompleteObjectUndo(oceanis.m_oceanMaterial, title + " Ocean Material Changed");
                    // Undo.RegisterCompleteObjectUndo(oceanis, "Ocean Inner Material Changed A");
                    paramChanged = true;
                    //Undo.FlushUndoRecordObjects();
                    //Object[] undoObjects = new Object[2];
                    //undoObjects[0] = oceanis;
                    // undoObjects[1] = oceanis.m_sky;
                    // Undo.RecordObjects(undoObjects, "Oceanis parameters change");
                    oceanis.savedPropsMaterial.SetFloat(variableName, paramToChange);
                    //oceanis.savedPropsMaterial.SetFloat(variableName, paramToChange);
                }
                //paramChanged = false;
                //oceanis.savedPropsMaterial.SetFloat(variableName, oceanis.m_oceanMaterial.GetFloat(variableName));
                oceanis.m_oceanMaterial.SetFloat(variableName, paramToChange);
            }
            oceanis.m_oceanMaterial.SetFloat(variableName, paramToChange);
        }
        void handleVector4tVariable(string titleX, string titleY, string titleZ, string titleW, GlobalOceanisControllerURP oceanis, string variableName,
            Vector2 minmaxX, Vector2 minmaxY, Vector2 minmaxZ, Vector2 minmaxW)
        {
            float vecX = EditorGUILayout.Slider(titleX, oceanis.m_oceanMaterial.GetVector(variableName).x, minmaxX.x, minmaxX.y);
            float vecY = EditorGUILayout.Slider(titleY, oceanis.m_oceanMaterial.GetVector(variableName).y, minmaxY.x, minmaxY.y);
            float vecZ = EditorGUILayout.Slider(titleZ, oceanis.m_oceanMaterial.GetVector(variableName).z, minmaxZ.x, minmaxZ.y);
            float vecW = EditorGUILayout.Slider(titleW, oceanis.m_oceanMaterial.GetVector(variableName).w, minmaxW.x, minmaxW.y);
            //oceanis.m_oceanMaterial.SetVector(variableName, new Vector4(vecX, vecY, vecZ, vecW));
            //if (oceanis.m_oceanMaterial.GetVector(variableName) != oceanis.savedPropsMaterial.GetVector(variableName))
            if (new Vector4(vecX, vecY, vecZ, vecW) != oceanis.savedPropsMaterial.GetVector(variableName))
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                if (Mouse.current != null &&
                        (!Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasPressedThisFrame) //isPressed
                )
                {
#else
                if (!Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                {
#endif
                    //Undo.RecordObject(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
                    //Undo.RecordObject(oceanis.m_oceanMaterial, "Ocean Inner Material Changes");
                    Undo.RegisterCompleteObjectUndo(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
                    Undo.RegisterCompleteObjectUndo(oceanis.m_oceanMaterial, "Ocean Material Changed");
                    // Undo.RegisterCompleteObjectUndo(oceanis, "Ocean Inner Material Changed A");
                    paramChanged = true;
                    //Undo.FlushUndoRecordObjects();
                    oceanis.savedPropsMaterial.SetVector(variableName, new Vector4(vecX, vecY, vecZ, vecW));
                }
                //paramChanged = false;
                //oceanis.savedPropsMaterial.SetVector(variableName, oceanis.m_oceanMaterial.GetVector(variableName));
                oceanis.m_oceanMaterial.SetVector(variableName, new Vector4(vecX, vecY, vecZ, vecW));
            }
            oceanis.m_oceanMaterial.SetVector(variableName, new Vector4(vecX, vecY, vecZ, vecW));
        }
        void handleColorVariable(string title, GlobalOceanisControllerURP oceanis, string variableName)
        {
            Color colorToChange = EditorGUILayout.ColorField(title, oceanis.m_oceanMaterial.GetColor(variableName));
            //oceanis.m_oceanMaterial.SetColor(variableName, colorToChange);
            //if (oceanis.m_oceanMaterial.GetColor(variableName) != oceanis.savedPropsMaterial.GetColor(variableName))
            if (colorToChange != oceanis.savedPropsMaterial.GetColor(variableName))
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                if (Mouse.current != null &&
                        (!Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasPressedThisFrame) //isPressed
                )
                {
#else
                if (!Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                {
#endif
                    //Undo.RecordObject(oceanis.savedPropsMaterial, "Ocean Inner Material Changed");
                    //Undo.RecordObject(oceanis.m_oceanMaterial, "Ocean Inner Material Changes");
                    Undo.RegisterCompleteObjectUndo(oceanis.savedPropsMaterial, title + " Ocean Inner Material Changed");
                    Undo.RegisterCompleteObjectUndo(oceanis.m_oceanMaterial, title + " Ocean Material Changed");
                    // Undo.RegisterCompleteObjectUndo(oceanis, "Ocean Inner Material Changed A");
                    paramChanged = true;
                    //Undo.FlushUndoRecordObjects();
                    oceanis.savedPropsMaterial.SetColor(variableName, colorToChange);
                }
                // paramChanged = false;
                //oceanis.savedPropsMaterial.SetColor(variableName, oceanis.m_oceanMaterial.GetColor(variableName));
                oceanis.m_oceanMaterial.SetColor(variableName, colorToChange);
            }
            oceanis.m_oceanMaterial.SetColor(variableName, colorToChange);
        }
        public bool paramChanged = false;

    }
}