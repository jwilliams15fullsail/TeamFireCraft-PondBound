using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

//AETuts youtube tutorial
//https://forum.unity.com/threads/detect-if-a-package-is-installed.1100338/

namespace Artngame.SKYMASTER
{
    [ExecuteInEditMode]
    public class setVFXFirePropertiesSM : MonoBehaviour
    {
        public Transform fireBall;
        public Material material;
        public float noisePower = 0.5f;
        public float noiseSize = 1;

        VisualEffect effect;

        // Start is called before the first frame update
        void Start()
        {
            effect = GetComponent<VisualEffect>();
        }

        // Update is called once per frame
        void Update()
        {
            if (fireBall != null && material != null)
            {
                effect.SetVector3("BallPos", fireBall.position);
                effect.SetFloat("BallSize", fireBall.localScale.x);
                effect.SetFloat("NoiseSize",  noiseSize);
                effect.SetFloat("NoisePower", noisePower);

                material.SetVector("_BallPos", fireBall.position);
                material.SetFloat("_BallSize", fireBall.localScale.x);
                material.SetFloat("_NoiseSize", noiseSize);
                material.SetFloat("_NoisePower", noisePower);
            }
        }
    }
}