using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Oceanis
{
    public class followSunObject : MonoBehaviour
    {

        public Transform Sun;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Sun != null)
            {
                transform.rotation = Sun.transform.rotation;
            }
        }
    }
}