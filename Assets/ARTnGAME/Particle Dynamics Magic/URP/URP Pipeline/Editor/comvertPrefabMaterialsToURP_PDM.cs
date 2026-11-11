using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Artngame.PDM {
    [ExecuteInEditMode]
    public class comvertPrefabMaterialsToURP_PDM : MonoBehaviour
    {

        public string materialNametoReplace = "Default-Diffuse";
        public Material URP_material;
        public List<GameObject> objects = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {

        }
        public bool convertNow = false;
        // Update is called once per frame
        void Update()
        {
            if (convertNow)
            {
                convertNow = false;
                for (int i = 0; i < objects.Count; i++)
                {
                    MeshRenderer[] renderers = objects[i].GetComponentsInChildren<MeshRenderer>(true);
                    if (renderers != null)
                    {
                        for (int j = 0; j < renderers.Length; j++)
                        {
                            //change materials
                            if (renderers[j].sharedMaterial != null && renderers[j].sharedMaterial.name == materialNametoReplace)
                            {
                                renderers[j].sharedMaterial = URP_material;
                            }
                        }
                    }
                    PrefabUtility.ApplyPrefabInstance(objects[i], InteractionMode.AutomatedAction);

                }
            }
        }
    }
}