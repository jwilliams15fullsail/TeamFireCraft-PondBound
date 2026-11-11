using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Artngame.SKYMASTER;
using Artngame.BrunetonsAtmosphere;
namespace Artngame.Oceanis
{
    public class airWaterInterfaceControlSM : MonoBehaviour
    {
        public bool adjustWaterLine = false;
        public bool adjustWaterLineRunTime = false;
        public Vector4 waterLineAdjust = new Vector4(1.24f, 7.7f,10,1);
        public float waterAirMode = 1;

        public GlobalOceanisControllerURP oceanis;
        public connectSuntoSunShaftsWaterURP waterScript;

        public bool recreateWaterMask = false;
        public RenderTexture waterMask;
        //public RenderTexture waterMaskOriginal;

        public Volume volFX;
        public LightShaftOCEANIS shafts;
        public GradientFogOCEANIS fog;

        public bool adjustCameraNearCull = false;
        public float cameraNearMax = 34;
        public float cameraNearMin = 0.3f;
        public float waterHeight = 0;
        public float transitionsSpeed = 1;
        public float transitionsPower = 2;
        // Start is called before the first frame update

        public bool controlBackPlane = false;
        public GameObject backPlane;

        Vector3 keepCamPos;
        int finishCamAdjust = 0;

        void Start()
        {
            if (adjustWaterLine)
            {
                oceanis.m_oceanMaterial.SetFloat("waterAirMode", waterAirMode);
                oceanis.m_oceanMaterial.SetVector("NearClipAdjust", waterLineAdjust);
            }


            keepCamPos = Camera.main.transform.position;
            //move camera below zero to enable shaft volumetrics and then back above
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, -0.01f, Camera.main.transform.position.z);

            //recreate water mask
            if (recreateWaterMask)
            {
                //if not good, change
                if(oceanis.WaterMaskCamera != null)
                {
                    if(oceanis.WaterMaskCamera.targetTexture.width != Screen.width || oceanis.WaterMaskCamera.targetTexture.height != Screen.height)
                    {
                        if(waterMask == null)
                        {
                            waterMask = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 0);
                        }
                        if (waterMask != null && 
                            (waterMask.width != Screen.width || waterMask.height != Screen.height)
                            )
                        {
                            waterMask.Release();
                            waterMask = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 0);
                        }
                        oceanis.WaterMaskCamera.targetTexture = waterMask;
                        oceanis.MaskMap = waterMask;
                        waterScript.MaskMap = waterMask;

                        //get volume effects
                        if (volFX != null)
                        {
                            volFX.profile.TryGet(out shafts);
                            volFX.profile.TryGet(out fog);
                            if (shafts != null)
                            {
                                shafts.maskTexture.value = waterMask;
                            }
                            if (fog != null)
                            {
                                fog.maskTexture.value = waterMask;
                            }
                        }
                    }
                }
            }
        }
        public float backPlaneEnableHeight = 20;
        public float decreaseNearLowThreholdOver = 4;
        public float decreaseNearLowThreholdUnder = -4;
        // Update is called once per frame
        void Update()
        {
            finishCamAdjust++;
            if (finishCamAdjust == 2)
            {
                Camera.main.transform.position = keepCamPos;
            }

            if (adjustWaterLineRunTime)
            {
                oceanis.m_oceanMaterial.SetFloat("waterAirMode", waterAirMode);
                oceanis.m_oceanMaterial.SetVector("NearClipAdjust", waterLineAdjust);
            }

            //if(waterMaskOriginal != null)
            //{
            //    Graphics.Blit(waterMask, waterMaskOriginal);
            //}

            if (controlBackPlane && backPlane != null)
            {
                if(Camera.main.transform.position.y > waterHeight - backPlaneEnableHeight && Camera.main.transform.position.y < waterHeight)
                {
                    if (backPlane.activeInHierarchy)
                    {
                        backPlane.SetActive(false);
                    }
                }
                else
                {
                    if (!backPlane.activeInHierarchy)
                    {
                        backPlane.SetActive(true);
                    }
                }
            }

            if (adjustCameraNearCull)
            {
                //if above water lower the far distance
                float dist = Camera.main.transform.position.y - waterHeight;
                if (dist > decreaseNearLowThreholdOver)
                {
                    Camera.main.nearClipPlane = Mathf.Clamp(cameraNearMax - transitionsSpeed * Mathf.Pow(dist, transitionsPower), cameraNearMin, cameraNearMax);
                }else
                if (dist < decreaseNearLowThreholdUnder)
                {
                    Camera.main.nearClipPlane = Mathf.Clamp(cameraNearMax - transitionsSpeed * Mathf.Pow(Mathf.Abs(dist), transitionsPower), cameraNearMin, cameraNearMax);
                }
                else
                {
                    Camera.main.nearClipPlane = Mathf.Clamp(cameraNearMax, cameraNearMin, cameraNearMax);
                }
            }
        }
    }
}