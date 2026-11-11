using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Oceanis
{
    public class minMaxPositionLimiterSM : MonoBehaviour
    {
        public Vector2 minMaxPos = new Vector2(-1, 1);
        public bool lockRotation = false;
        public bool useCameraRotation = false;
        public Quaternion startRot;

        // Start is called before the first frame update
        void Start()
        {
            startRot = transform.rotation;
        }

        public bool verticalJitter = false;
        public float jitterfreq = 1;
        // Update is called once per frame
        void Update()
        {

            if (verticalJitter)
            {
                transform.position = new Vector3(transform.position.x,
                    Mathf.Cos(Time.fixedTime * jitterfreq) + (minMaxPos.y - minMaxPos.x) / 2,
                    transform.position.z);
            }


            if (transform.position.y > minMaxPos.y)
            {
                transform.position = new Vector3(transform.position.x, minMaxPos.y, transform.position.z);
            }
            if (transform.position.y < minMaxPos.x)
            {
                transform.position = new Vector3(transform.position.x, minMaxPos.x, transform.position.z);
            }

            if (lockRotation)
            {
                transform.rotation = startRot;
            }

            if (useCameraRotation)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

        }

        void FixedUpdate()
        {
            if (transform.position.y > minMaxPos.y)
            {
                transform.position = new Vector3(transform.position.x, minMaxPos.y, transform.position.z);
            }
            if (transform.position.y < minMaxPos.x)
            {
                transform.position = new Vector3(transform.position.x, minMaxPos.x, transform.position.z);
            }

            if (lockRotation)
            {
                transform.rotation = startRot;
            }
        }

        void LateUpdate()
        {
            if (transform.position.y > minMaxPos.y)
            {
                transform.position = new Vector3(transform.position.x, minMaxPos.y, transform.position.z);
            }
            if (transform.position.y < minMaxPos.x)
            {
                transform.position = new Vector3(transform.position.x, minMaxPos.x, transform.position.z);
            }

            if (lockRotation)
            {
                transform.rotation = startRot;
            }
        }
    }
}