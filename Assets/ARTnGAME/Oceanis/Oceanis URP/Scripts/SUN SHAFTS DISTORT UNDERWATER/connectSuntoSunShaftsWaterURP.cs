using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

//using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.Universal;
using Artngame.Oceanis;
namespace Artngame.SKYMASTER
{

    [ExecuteInEditMode]
    public class connectSuntoSunShaftsWaterURP : MonoBehaviour
    {
        public bool enableShafts = true;
        public Transform sun;
        public BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitSunShaftsWaterSRP.BlitSettings.ShaftsScreenBlendMode.Screen;

        //v0.1
        public bool autoRegulateEffect = false;
        public float cutoffHeigth = 0; //cutoff the effect above this height
        
        
        //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
        public int radialBlurIterations = 2;
        public Color sunColor = Color.white;
        public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        public float sunShaftBlurRadius = 2.5f;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
        //PostProcessProfile postProfile;


        //v4.8
        [Tooltip("Controls the intensity of the effect.")]
        public float intensity = 0;
        public bool shaftsFollowCamera = false;
        //v1.2
        public bool useWaterAirMask = true;
        public bool resetScaling = false;
        //v1.2
        public bool useVolumeMaskRenderer = false;
        public RenderTexture MaskMap;
        public bool useTexture2DMask = false;
        public Texture2D MaskMapA;
        //MASK
        public float waterHeight = 0;
        /////REFRACT LINE
        public Texture2D BumpMap;
        public float underwaterDepthFade = 1.1f; //put above 1 to fade faster towards the surface - TO DO: Regulate based on player distance to surface as well
        public float BumpIntensity = 0.02f;
        public Vector4 BumpVelocity = new Vector4(0.15f, 0.10f, 0.2f, 0.3f);
        public float BumpScale = 0.1f;
        //v4.6
        public Color underWaterTint = Color.cyan;
        public float BumpIntensityRL = 0.17f;
        public float BumpScaleRL = 0.4f;
        public float BumpLineHeight = 0.1f;
        public float refractLineWidth = 1; //v4.6
        public float refractLineXDisp = 1; //v4.6
        public float refractLineXDispA = 1; //v4.6
        public Vector4 refractLineFade = new Vector4(1, 1, 1, 1);
        public float scalerMask = 0.2f;
        /////END REFRACT LINE

        public RenderTexture skybox;

        // Start is called before the first frame update
        void Start()
        {
            //postProfile = GetComponent<PostProcessVolume>().profile;
        }

        // Update is called once per frame
        void Update()
        {
            if (skybox != null)
            {
                Shader.SetGlobalTexture("_skyboxOnly", skybox);
            }

            if (sun != null)
            {

            }
        }
    }
}