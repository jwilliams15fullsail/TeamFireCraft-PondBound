using UnityEngine;
using System.Collections;
//using UnityEngine.Rendering.Experimental;
//using UnityEngine.Experimental.Rendering.HDPipeline;

//using UnityEngine.Rendering.HighDefinition;
//using UnityEngine.
namespace Artngame.Oceanis
{
    [ExecuteInEditMode]
    public class cycleCaustucsTexture : MonoBehaviour
    {

        public Transform cameraA;
        public float waterHeight = 0;
        public float fadeOutPower = 2;
        public float maxDistOffset = 14;
        public bool autoRegulateShafts = false;

        public Transform sun;
        public float lightCookieSize=1;
        public Material causticMat;

        public bool previewCycle = true;

        public float lightCookieIntensity = 0;
        public float customCookieIntensity = 1;
        public float customCookieIntensityA = 0;
        public float customCookieScaler = 1;
        public float customCookieScalerA = 0;

        //Projector CausticProjector;
        //DecalProjector CausticProjector;
        public bool compensateCameraMotion = true; //offset based on camera x-z motion
        Vector2 prevCameraPos;
        public float compensateSpeed = 1;

        public Texture2D[] Caustics;
        public float fps = 30;
        float start_time;

        public int currentFrame = 0;

        public bool applyToLightCookie = false;
        public Light lightA;

        // Use this for initialization
        void Start()
        {
            if(Camera.main != null)
            {
                cameraA = Camera.main.transform;
            }

            //CausticProjector = GetComponent<Projector> ();
            if (!applyToLightCookie)
            {
                //CausticProjector = GetComponent<DecalProjector>();
            }
            else
            {
                lightA = GetComponent<Light>();
            }
            start_time = Time.fixedTime;

            prevCameraPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        }


        void LateUpdate()
        {

            //v0.8 HDRP
            //if (compensateCameraMotion && Camera.main != null && CausticProjector != null)
            //{

            //     CausticProjector.uvBias = new Vector2( CausticProjector.uvBias.x + 0.1f*compensateSpeed*(Camera.main.transform.position.x - prevCameraPos.x),
            //        CausticProjector.uvBias.y + 0.1f * compensateSpeed * (Camera.main.transform.position.z - prevCameraPos.y));

            //    //CausticProjector.uvBias = new Vector2(Camera.main.transform.position.x ,
            //    //   Camera.main.transform.position.z );

            //    prevCameraPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
            //}

        }

        // Update is called once per frame
        void Update()
        {

            //v0.1
            if (causticMat != null)
            {
                var sunMatrix = sun.localToWorldMatrix;
                causticMat.SetMatrix("_MainLightDirection", sunMatrix);
            }

            //v0.8 HDRP
            //if (compensateCameraMotion && Camera.main != null && CausticProjector != null) {

            //   // CausticProjector.uvBias = new Vector2( CausticProjector.uvBias.x + 0.1f*compensateSpeed*(Camera.main.transform.position.x - prevCameraPos.x),
            //    //    CausticProjector.uvBias.y + 0.1f * compensateSpeed * (Camera.main.transform.position.z - prevCameraPos.y));

            //    //CausticProjector.uvBias = new Vector2(Camera.main.transform.position.x ,
            //     //   Camera.main.transform.position.z );

            //    //prevCameraPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
            //}

            if (Time.fixedTime - start_time > (1 / fps) && ((previewCycle && !Application.isPlaying) || Application.isPlaying) )
            {

                //CausticProjector.material.SetTexture ("_CausticTexture", Caustics [currentFrame]);
                if (!applyToLightCookie)
                {
                    //CausticProjector.material.SetTexture("_BaseColorMap", Caustics[currentFrame]);

                    //CausticProjector.material.SetTexture("_EmissiveColorMap", Caustics[currentFrame]);
                }
                else
                {

                    if (lightA == null)
                    {
                        lightA = GetComponent<Light>();
                    }
                    if (lightA != null)
                    {
                        lightA.cookie = Caustics[currentFrame];
                    }
                }

                currentFrame = currentFrame + 1;
                if (currentFrame > 30)
                {
                    currentFrame = 0;
                }

                start_time = Time.fixedTime;
            }

        }
    }
}
