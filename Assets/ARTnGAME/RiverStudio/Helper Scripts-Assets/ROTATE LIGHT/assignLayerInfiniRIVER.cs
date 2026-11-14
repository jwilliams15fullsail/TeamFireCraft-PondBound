using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.RiverStudio
{
    [ExecuteInEditMode]
    public class assignLayerInfiniRIVER : MonoBehaviour
    {
        public float assignAfter = 0; //assign layer to objects after this time
        public bool assignInLateUpdate = false;
        public bool assignInUpdate = false;
        public bool assignInStart = true;
        public bool assignToDisabled = false;

        public int layerID = 0;
        public string DebugSelectedLayer = "Default";
        public GameObject objectToAssignLayers;

        // Start is called before the first frame update
        void Start()
        {
            if (assignInStart && objectToAssignLayers != null)
            {
                Transform[] tranforms = GetComponentsInChildren<Transform>(assignToDisabled);
                for (int i=0;i< tranforms.Length; i++)
                {
                    tranforms[i].gameObject.layer = layerID;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying)
            {
                DebugSelectedLayer = LayerMask.LayerToName(layerID);
            }
            if (assignInUpdate && objectToAssignLayers != null)
            {
                Transform[] tranforms = GetComponentsInChildren<Transform>(assignToDisabled);
                for (int i = 0; i < tranforms.Length; i++)
                {
                    tranforms[i].gameObject.layer = layerID;
                }
            }
        }
        void LateUpdate()
        {            
            if (assignInLateUpdate && objectToAssignLayers != null)
            {
                Transform[] tranforms = GetComponentsInChildren<Transform>(assignToDisabled);
                for (int i = 0; i < tranforms.Length; i++)
                {
                    tranforms[i].gameObject.layer = layerID;
                }
            }
        }
    }
}