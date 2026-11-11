using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.CommonTools
{
    [ExecuteInEditMode]
    public class passTexToShaderGlobalsSM : MonoBehaviour
    {
        public RenderTexture texA;
        public string texAName = "";
        public RenderTexture texB;
        public string texBName = "";
        public RenderTexture texC;
        public string texCName = "";
        public RenderTexture texD;
        public string texDName = "";

        public Texture2D texBump;
        public string texBumpName = "";

        // Start is called before the first frame update
        void Start() { }

        public RenderTexture depthNormals;
        public string depthNormalsName = "_CameraDepthNormalsTexture";
        public RenderTexture depthNormalsTop;
        public string depthNormalsTopName = "_CameraDepthNormalsTextureTOP";

        public RenderTexture depthMask;
        public Camera depthNormalsMaskCamera;
        public Material testDEPTHmask;

        public RenderTexture skybox;

        // Update is called once per frame
        void Update()
        {
            //SUN SHAFTS
            if (skybox != null)
            {
                Shader.SetGlobalTexture("_skyboxOnly", skybox);
            }

            //GIBLION
            //cmd.SetGlobalTexture("_CameraDepthNormalsTexture", Shader.PropertyToID(destination.name));
            if (depthNormals != null)
            {
                Shader.SetGlobalTexture(depthNormalsName, depthNormals);
            }
            if (depthNormalsTop != null)
            {
                Shader.SetGlobalTexture(depthNormalsTopName, depthNormalsTop);
            }

            //NEW InfiniSNOW
            if (depthMask != null && depthNormalsMaskCamera != null)
            {
                RenderTexture rt = UnityEngine.RenderTexture.active;
                UnityEngine.RenderTexture.active = depthMask;
                GL.Clear(true, true, Color.clear);
                UnityEngine.RenderTexture.active = rt;

                GL.ClearWithSkybox(true, depthNormalsMaskCamera);
                depthMask.DiscardContents();

                depthNormalsMaskCamera.Render();
            }
            if (testDEPTHmask != null)
            {
                //snowMaterial.SetTexture("_MainTex", depthMask);
                if (testDEPTHmask.HasProperty("_MainTex"))
                {
                    testDEPTHmask.SetTexture("_MainTex", depthMask);
                }
                if (testDEPTHmask.HasProperty("_BaseTex"))
                {
                    testDEPTHmask.SetTexture("_BaseTex", depthMask);
                }
            }
            //v0.5
            //if (snowMaterial.GetTexture("_depthMask") != depthMask)
            //{
            //    Debug.Log("ASSIGN MASK");
            //    snowMaterial.SetTexture("_depthMask", depthMask);
            //}

            if (texBump != null)
            {
                Shader.SetGlobalTexture(texBumpName, texBump);
            }
            if (texA != null)
            {
                Shader.SetGlobalTexture(texAName, texA);
            }
            if (texB != null)
            {
                Shader.SetGlobalTexture(texBName, texB);
            }
            if (texC != null)
            {
                Shader.SetGlobalTexture(texCName, texC);
            }
            if (texD != null)
            {
                Shader.SetGlobalTexture(texDName, texD);
            }
        }
    }
}