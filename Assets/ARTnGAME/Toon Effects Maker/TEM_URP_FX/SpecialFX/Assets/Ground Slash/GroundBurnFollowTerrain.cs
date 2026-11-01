using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.TEM
{
    public class GroundBurnFollowTerrain : MonoBehaviour
    {
        public float velocity = 20;
        public float slowDownSpeed = 0.01f;
        public float minimumDetectLength = 0.1f;
        public float delayDestoryTime = 5;

        Rigidbody rigBody;
        bool finished = false;

        // Start is called before the first frame update
        void Start()
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);

            if(GetComponent<Rigidbody>() != null)
            {
                rigBody = GetComponent<Rigidbody>();
                StartCoroutine(SlowRate());
            }
            else
            {
                //
            }
            Destroy(gameObject, delayDestoryTime);
        }
               

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!finished)
            {
                RaycastHit hit;
                Vector3 dist = new Vector3(transform.position.x, transform.position.y+1, transform.position.z);

                if (Physics.Raycast(dist,transform.TransformDirection(-Vector3.up), out hit, minimumDetectLength))
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                }
                else
                {
                   // transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                }
                Debug.DrawRay(dist, transform.TransformDirection(-Vector3.up * minimumDetectLength), Color.red, 10);
            }  
        }

        IEnumerator SlowRate()
        {
            float Ta = 1;
            while (Ta > 0)
            {
                rigBody.linearVelocity = Vector3.Lerp(Vector3.zero, rigBody.linearVelocity, Ta);
                Ta -= slowDownSpeed;
                yield return new WaitForSeconds(0.1f);
            }
            finished = true;
        }
    }
}