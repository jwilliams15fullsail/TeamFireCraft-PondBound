using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Oceanis.Utils
{
    public class WaterMaskFollowCameraSM : MonoBehaviour
    {

        public Transform waterMaskPlane;
        public float maskHeight;
        float startDistZ = 0;
        public bool setHeight = false;
        // Start is called before the first frame update
        void Start()
        {
            startDistZ = waterMaskPlane.localPosition.z;
        }

        // Update is called once per frame
        void Update()
        {
            if (waterMaskPlane != null)
            {
                waterMaskPlane.transform.forward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                if (setHeight)
                {
                    waterMaskPlane.transform.position = new Vector3(waterMaskPlane.transform.position.x, maskHeight, waterMaskPlane.transform.position.z);
                }
            }
        }
    }
}