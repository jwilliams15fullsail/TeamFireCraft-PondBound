using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Artngame.CommonTools.FluidSim2D;
namespace Artngame.RiverStudio
{
    public class addInteractorRipplesSM : MonoBehaviour
    {

        public RippleCausticsGeneratorSM rippleMaker;

        // Start is called before the first frame update
        void Start()
        {

        }
        public bool keepAliveOnStay = false;
        private void OnCollisionEnter(Collision collision)
        {
            if (rippleMaker != null && UnityEngine.Random.Range(1, chancesMax) == 2 && keepAliveOnStay)
            {
                //add itself
                rippleMaker.interactor = this.transform;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (rippleMaker != null && UnityEngine.Random.Range(1, chancesMax) == 2 && keepAliveOnStay)
            {
                //add itself
                rippleMaker.interactor = this.transform;
            }
        }
        Vector3 previousPos;
        public int chancesMax = 5;
        private void OnCollisionStay(Collision collision)
        {
            if (rippleMaker != null && UnityEngine.Random.Range(1, chancesMax) == 2 && (keepAliveOnStay || (Vector3.Distance(previousPos, this.transform.position) > 0.01f) ))
            {
                //add itself
                rippleMaker.interactor = this.transform;
                previousPos = this.transform.position;
                //Debug.Log("Adding cube:" + this.gameObject.name);
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
