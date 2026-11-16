using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Oceanis
{
    public class disableIFnearWaterSurfaceSM : MonoBehaviour
    {
        public GameObject objectToDisable;
        public float waterHeight = 0;
        public float disableMargin = 1;
        public float disableMarginABOVE = 1;
        public MeshRenderer rendererA;

        public AudioSource disableAudioIfUnderA;
        float startVolume = 0;

        // Start is called before the first frame update
        void Start()
        {
            if (objectToDisable != null)
            {
                rendererA = objectToDisable.GetComponent<MeshRenderer>();
            }

            if (disableAudioIfUnderA != null)
            {
                startVolume = disableAudioIfUnderA.volume;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (objectToDisable != null)
            {
                if (Camera.main != null)
                {
                    if (Camera.main.transform.position.y < waterHeight)
                    {
                        if (Mathf.Abs(Camera.main.transform.position.y - waterHeight) > disableMargin)
                        {
                            //if(!objectToDisable.activeInHierarchy)
                            //{
                            //    objectToDisable.SetActive(true);
                            //}
                            if (!rendererA.enabled)
                            {
                                rendererA.enabled = true;
                            }
                        }
                        else
                        {
                            //if (objectToDisable.activeInHierarchy)
                            //{
                            //    objectToDisable.SetActive(false);
                            //}
                            if (rendererA.enabled)
                            {
                                rendererA.enabled = false;
                            }
                        }

                        if (disableAudioIfUnderA != null)
                        {
                            //disableAudioIfUnderA.enabled = false;
                            disableAudioIfUnderA.volume = Mathf.Lerp(startVolume, 0, 0.1f*(waterHeight - Camera.main.transform.position.y));
                        }

                    }
                    else
                    {
                        if (Mathf.Abs(Camera.main.transform.position.y - waterHeight) > disableMarginABOVE)
                        {
                            if (!rendererA.enabled)
                            {
                                rendererA.enabled = true;
                            }
                        }
                        else
                        {
                            if (rendererA.enabled)
                            {
                                rendererA.enabled = false;
                            }
                        }


                        if (disableAudioIfUnderA != null)
                        {
                            //disableAudioIfUnderA.enabled = true;
                            disableAudioIfUnderA.volume = Mathf.Lerp(disableAudioIfUnderA.volume, startVolume, 10);
                        }


                    }
                }
            }
        }
    }
}