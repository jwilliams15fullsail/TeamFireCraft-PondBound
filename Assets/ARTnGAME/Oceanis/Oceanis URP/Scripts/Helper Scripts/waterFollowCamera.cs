using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Oceanis.Utils
{
    public class waterFollowCamera : MonoBehaviour
    {
        public bool followCamera;
        public Transform Water;
        public Transform CloudDome;
        public Transform Caustics;
        public Transform DetailedWaterPlane;
        Vector3 prevPos;
        public float threshodlDist = 50;
        public float threshodlCaustics = 0;

        // Start is called before the first frame update
        void Start()
        {

            prevPos = Camera.main.transform.position;
            if (followCamera && Camera.main != null && Water != null)
            {
                Water.transform.position = new Vector3(Camera.main.transform.position.x, Water.transform.position.y, Camera.main.transform.position.z);
            }
            if (CloudDome != null)
            {
                CloudDome.transform.position = new Vector3(Camera.main.transform.position.x, CloudDome.transform.position.y, Camera.main.transform.position.z);
            }
            if (Caustics != null)
            {
                Caustics.transform.position = new Vector3(Camera.main.transform.position.x, Caustics.transform.position.y, Camera.main.transform.position.z);
            }
        }

        // Update is called once per frame
        void Update()
        {
            float distance = Vector2.Distance(new Vector2(prevPos.x, prevPos.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z));

            if (followCamera && Camera.main != null && Water != null && distance > threshodlDist)
            {
                Water.transform.position = new Vector3(Camera.main.transform.position.x, Water.transform.position.y, Camera.main.transform.position.z);
                prevPos = Camera.main.transform.position;
            }

            if (CloudDome != null)
            {
                CloudDome.transform.position = new Vector3(Camera.main.transform.position.x, CloudDome.transform.position.y, Camera.main.transform.position.z);
            }
            if (Caustics != null)
            {
                Caustics.transform.position = new Vector3(Camera.main.transform.position.x, Caustics.transform.position.y, Camera.main.transform.position.z);
            }
            if (DetailedWaterPlane != null)
            {
                //rotation lock
                //DetailedWaterPlane.localEulerAngles = new Vector3(-Camera.main.transform.eulerAngles.x, DetailedWaterPlane.localEulerAngles.y, DetailedWaterPlane.localEulerAngles.z);

                //height lock
                //DetailedWaterPlane.localPosition = new Vector3(DetailedWaterPlane.localPosition.x, -Camera.main.transform.position.y, DetailedWaterPlane.localPosition.z);
                DetailedWaterPlane.position = new Vector3(Camera.main.transform.position.x, DetailedWaterPlane.position.y, Camera.main.transform.position.z)
                    - new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * 10; //move towards camera 
                                                                                                             //-45 in camera 0 rot, 135 in camera -180 rot
                DetailedWaterPlane.eulerAngles = new Vector3(DetailedWaterPlane.eulerAngles.x, Camera.main.transform.eulerAngles.y - 45, DetailedWaterPlane.eulerAngles.z);

                // DetailedWaterPlane.position = new Vector3(Camera.main.transform.position.x, DetailedWaterPlane.position.y, Camera.main.transform.position.z)
                //- new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * 10; //move towards camera 
                //+ new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                // - new Vector3(Camera.main.transform.right.x, 0, 0)*50 + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z)*40; //88
            }
        }
    }
}