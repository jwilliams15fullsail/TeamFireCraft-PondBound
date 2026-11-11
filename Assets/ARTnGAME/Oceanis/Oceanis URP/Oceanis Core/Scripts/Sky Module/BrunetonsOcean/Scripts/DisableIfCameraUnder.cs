using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Oceanis
{
    public class DisableIfCameraUnder : MonoBehaviour
    {
        public float offset = 2;
        MeshRenderer mesh;
        // Start is called before the first frame update
        void Start()
        {
            mesh = GetComponent<MeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (mesh != null && Camera.main.transform.position.y < transform.position.y + offset)
            {
                mesh.enabled = false;
            }
            if (mesh != null && Camera.main.transform.position.y >= transform.position.y + offset)
            {
                mesh.enabled = true;
            }
        }
    }
}