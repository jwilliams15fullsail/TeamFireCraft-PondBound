using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.CommonTools.DepthRenderer
{
    [ExecuteInEditMode]
    public class controlDepthCT_URP : MonoBehaviour
    {
        public bool disableAfterStart = false;
        public GameObject extraDepthController;
        public DepthRendererCT_URP depthRenderer;
        bool disabled = false;
        // Start is called before the first frame update
        void Start()
        {
            if (Application.isPlaying)
            {
                disabled = false;
            }
        }
        int frame = 0;

        public bool exportCameraFarPlane = true;

        private void OnEnable()
        {
            if (exportCameraFarPlane)
            {
                Shader.SetGlobalFloat("_DepthCameraFarPlane", depthRenderer.depthCameraFarPlane);
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {

            if (exportCameraFarPlane)
            {
                Shader.SetGlobalFloat("_DepthCameraFarPlane", depthRenderer.depthCameraFarPlane);
            }

            if (Application.isPlaying)
            {
                if (!disabled && disableAfterStart && frame > 2)
                {
                    if (extraDepthController != null)
                    {
                        extraDepthController.SetActive(false);
                    }
                    depthRenderer.enabled = false;
                    disabled = true;
                }
                if (!disabled && disableAfterStart)
                {
                    frame++;
                }

            }
        }
    }
}