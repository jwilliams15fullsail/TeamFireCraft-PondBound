using UnityEngine;
using System.IO;
//using UnityEngine.Experimental.Rendering.HDPipeline;

namespace Artngame.BrunetonsAtmosphere
{
    [ExecuteInEditMode]
    public class Sky : MonoBehaviour
    {
        public float offsetX = 0;
        public float offsetY = 0;
        public Camera cloudRendererCamera;
        public GameObject cloudPlane;
        public float dome_height_offset = 0;
        float prevRotX = 0;
        float prevRotY = 0;
       // public ControlPlanarReflectProbeSM

        int counterEnabled = 0;
        public int updateEvery = 1;

        const float SCALE = 1000.0f;

        const int TRANSMITTANCE_WIDTH = 256;
        const int TRANSMITTANCE_HEIGHT = 64;
        const int TRANSMITTANCE_CHANNELS = 3;

        const int IRRADIANCE_WIDTH = 64;
        const int IRRADIANCE_HEIGHT = 16;
        const int IRRADIANCE_CHANNELS = 3;

        const int INSCATTER_WIDTH = 256;
        const int INSCATTER_HEIGHT = 128;
        const int INSCATTER_DEPTH = 32;
        const int INSCATTER_CHANNELS = 4;

        public bool m_showSkyMap = false;

        public string m_filePath = "/BrunetonsAtmosphere/Textures";

        public Material m_skyMapMaterial;

        public Material m_skyMaterial;

        public Material m_postEffectMaterial;

        public GameObject m_sun;

        public Vector3 m_betaR = new Vector3(0.0058f, 0.0135f, 0.0331f);

        public float m_mieG = 0.75f;

        public float m_sunIntensity = 100.0f;

        private RenderTexture m_skyMap, m_displaySkyMap;

        private Texture2D m_transmittance, m_irradiance;

        private Texture3D m_inscatter;

        private void Start()
        {
            if (m_skyMap == null)
            {
                m_skyMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                m_skyMap.filterMode = FilterMode.Trilinear;
                m_skyMap.wrapMode = TextureWrapMode.Clamp;
                m_skyMap.useMipMap = true;
                m_skyMap.Create();

                m_displaySkyMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                m_displaySkyMap.filterMode = FilterMode.Trilinear;
                m_displaySkyMap.wrapMode = TextureWrapMode.Clamp;
                m_displaySkyMap.useMipMap = true;
                m_displaySkyMap.Create();

                //NOTE - These raw files will not be included by Unity in the build so you will get a 
                //error saying they are missing. You will need to manually place them in the build folder
                //or change to using a supported format like exr.

                //Transmittance is responsible for the change in the sun color as it moves
                //The raw file is a 2D array of 32 bit floats with a range of 0 to 1
                //string path = System.IO.Directory.GetCurrentDirectory() + m_filePath + "/transmittance.raw"; //Application.dataPath + m_filePath + "/transmittance.raw";
                string path = Application.streamingAssetsPath + "\\" + "transmittance.raw"; //Application.dataPath + m_filePath + "/transmittance.raw";

#if UNITY_STANDLONE_OSX || UNITY_EDITOR_OSX
                path = Application.streamingAssetsPath + "//transmittance.raw";
#endif

                int size = TRANSMITTANCE_WIDTH * TRANSMITTANCE_HEIGHT * TRANSMITTANCE_CHANNELS;

                m_transmittance = new Texture2D(TRANSMITTANCE_WIDTH, TRANSMITTANCE_HEIGHT, TextureFormat.RGBAHalf, false, true);
                m_transmittance.SetPixels(ToColor(LoadRawFile(path, size), TRANSMITTANCE_CHANNELS));
                m_transmittance.Apply();

                //path = System.IO.Directory.GetCurrentDirectory() + m_filePath + "/irradiance.raw";
                path = Application.streamingAssetsPath + "\\" + "irradiance.raw";

#if UNITY_STANDLONE_OSX || UNITY_EDITOR_OSX
                path = Application.streamingAssetsPath + "//irradiance.raw";
#endif

                size = IRRADIANCE_WIDTH * IRRADIANCE_HEIGHT * IRRADIANCE_CHANNELS;

                m_irradiance = new Texture2D(IRRADIANCE_WIDTH, IRRADIANCE_HEIGHT, TextureFormat.RGBAHalf, false, true);
                m_irradiance.SetPixels(ToColor(LoadRawFile(path, size), IRRADIANCE_CHANNELS));
                m_irradiance.Apply();

                //Inscatter is responsible for the change in the sky color as the sun moves
                //The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
                //As there is not such thing as a 4D texture the data is packed into a 3D texture 
                //and the shader manually performs the sample for the 4th dimension
                //path = System.IO.Directory.GetCurrentDirectory() + m_filePath + "/inscatter.raw";
                path = Application.streamingAssetsPath + "\\" + "inscatter.raw";

#if UNITY_STANDLONE_OSX || UNITY_EDITOR_OSX
                path = Application.streamingAssetsPath + "//inscatter.raw";
#endif

                size = INSCATTER_WIDTH * INSCATTER_HEIGHT * INSCATTER_DEPTH * INSCATTER_CHANNELS;

                //Should be linear color space. I presume 3D textures always are.
                m_inscatter = new Texture3D(INSCATTER_WIDTH, INSCATTER_HEIGHT, INSCATTER_DEPTH, TextureFormat.RGBAHalf, false);
                m_inscatter.SetPixels(ToColor(LoadRawFile(path, size), INSCATTER_CHANNELS));
                m_inscatter.Apply();
            }

        }
        Vector3 cloudPlane_transform_prev;
        private void Update()
        {
            Vector3 pos = Camera.main.transform.position;
            pos.y = 0.0f;

            //centre sky dome at player pos
            transform.localPosition = pos - new Vector3(0, dome_height_offset,0);//transform.localPosition = pos;

            UpdateMat(m_skyMapMaterial);
            UpdateMat(m_skyMaterial);
            UpdateMat(m_postEffectMaterial);

            m_skyMapMaterial.SetFloat("_ApplyHDR", 0);
            Graphics.Blit(null, m_skyMap, m_skyMapMaterial);

            if (m_showSkyMap)
            {
                m_skyMapMaterial.SetFloat("_ApplyHDR", 1);
                Graphics.Blit(null, m_displaySkyMap, m_skyMapMaterial);
            }


            if(cloudRendererCamera != null && cloudPlane != null)
            {
                if (cloudRendererCamera.enabled)
                {
                    cloudRendererCamera.enabled = false;
                    counterEnabled = updateEvery;

                    //rotate render camera to compensate motion
                    cloudRendererCamera.transform.localEulerAngles = Vector3.zero;
                        cloudPlane.transform.localEulerAngles = Vector3.zero;
                    //cloudPlane.transform.localEulerAngles = new Vector3(-(Camera.main.transform.eulerAngles.x),
                    //        cloudPlane.transform.localEulerAngles.y,
                    //       cloudPlane.transform.localEulerAngles.z);

                    //  cloudRendererCamera.transform.localEulerAngles = Vector3.Lerp(cloudRendererCamera.transform.localEulerAngles, Vector3.zero, Time.deltaTime);
                    //  cloudPlane.transform.localEulerAngles = Vector3.Lerp(cloudPlane.transform.localEulerAngles, Vector3.zero, Time.deltaTime);

                    cloudPlane.transform.localPosition = new Vector3(0,-100,10000);// cloudPlane_transform_prev;

                    //keep reference starting rotation
                    prevRotX = Camera.main.transform.eulerAngles.x;
                    prevRotY = Camera.main.transform.eulerAngles.y;
                }
                else
                {
                    if (counterEnabled == 0)
                    {
                        cloudRendererCamera.enabled = true;

                        //reset local camera rotation
               //         cloudRendererCamera.transform.localEulerAngles = Vector3.zero;
               //         cloudPlane.transform.localEulerAngles = Vector3.zero;
                        //  cloudPlane.transform.localEulerAngles = new Vector3(-(Camera.main.transform.eulerAngles.x),
                        //     cloudPlane.transform.localEulerAngles.y,
                        //    cloudPlane.transform.localEulerAngles.z);

                        //     cloudRendererCamera.transform.localEulerAngles = Vector3.Lerp(cloudRendererCamera.transform.localEulerAngles, Vector3.zero, Time.deltaTime);
                        //     cloudPlane.transform.localEulerAngles = Vector3.Lerp(cloudPlane.transform.localEulerAngles, Vector3.zero, Time.deltaTime);

                        if (updateEvery == 0)
                        {
                            //change angle
                            //cloudPlane.transform.localEulerAngles = new Vector3(4f*(Camera.main.transform.eulerAngles.x - prevRotX),
                           // cloudPlane.transform.localEulerAngles = new Vector3(0.23f * (Camera.main.transform.eulerAngles.x - prevRotX),
                           //   0.33f * (Camera.main.transform.eulerAngles.y - prevRotY),
                           //    cloudPlane.transform.localEulerAngles.z);

                            cloudPlane_transform_prev = cloudPlane.transform.localPosition;
                            cloudPlane.transform.localPosition = new Vector3(cloudPlane.transform.localPosition.x + offsetX * (Camera.main.transform.eulerAngles.y - prevRotY),
                              cloudPlane.transform.localPosition.y +offsetY*(Camera.main.transform.eulerAngles.x - prevRotX),
                               cloudPlane.transform.localPosition.z);

                            prevRotX = Camera.main.transform.eulerAngles.x;
                            prevRotY = Camera.main.transform.eulerAngles.y;

                            //cloudPlane.transform.localEulerAngles = new Vector3((Camera.main.transform.eulerAngles.x - 0),
                            //   (Camera.main.transform.eulerAngles.y - 0),
                            //    cloudPlane.transform.localEulerAngles.z);
                            // cloudPlane.transform.localEulerAngles = new Vector3(1.1f * (Camera.main.transform.eulerAngles.x - prevRotX),
                            //  1.1f * (Camera.main.transform.eulerAngles.y - prevRotY),
                            //   cloudPlane.transform.localEulerAngles.z);

                            // cloudRendererCamera.transform.localEulerAngles = new Vector3(-(Camera.main.transform.eulerAngles.x - prevRotX),
                            //    -(Camera.main.transform.eulerAngles.y - prevRotY),
                            //    cloudRendererCamera.transform.localEulerAngles.z);
                        }

                    }
                    else
                    {
                        counterEnabled--;

                        //compensate motion
                        // cloudPlane.transform.localEulerAngles = new Vector3((Camera.main.transform.eulerAngles.x - prevRotX),
                        //    (Camera.main.transform.eulerAngles.y - prevRotY),
                        //    cloudPlane.transform.localEulerAngles.z);

                        if (updateEvery == 1)
                        {
                            cloudPlane.transform.localEulerAngles = new Vector3((Camera.main.transform.eulerAngles.x - prevRotX),
                                (Camera.main.transform.eulerAngles.y - prevRotY),
                                cloudPlane.transform.localEulerAngles.z);
                            cloudRendererCamera.transform.localEulerAngles = new Vector3(-(Camera.main.transform.eulerAngles.x - prevRotX),
                                -(Camera.main.transform.eulerAngles.y - prevRotY),
                                cloudRendererCamera.transform.localEulerAngles.z);
                        }

                        if (updateEvery == 0)
                        {
                            cloudPlane.transform.localEulerAngles = new Vector3((Camera.main.transform.eulerAngles.x - prevRotX),
                               (Camera.main.transform.eulerAngles.y - prevRotY),
                               cloudPlane.transform.localEulerAngles.z);

                            //cloudPlane.transform.localEulerAngles = new Vector3((Camera.main.transform.eulerAngles.x - 0),
                            //   (Camera.main.transform.eulerAngles.y - 0),
                            //    cloudPlane.transform.localEulerAngles.z);
                            // cloudPlane.transform.localEulerAngles = new Vector3(1.1f * (Camera.main.transform.eulerAngles.x - prevRotX),
                            //  1.1f * (Camera.main.transform.eulerAngles.y - prevRotY),
                            //   cloudPlane.transform.localEulerAngles.z);

                            cloudRendererCamera.transform.localEulerAngles = new Vector3(-1.1f*(Camera.main.transform.eulerAngles.x - prevRotX),
                                -1.3f * (Camera.main.transform.eulerAngles.y - prevRotY),
                                cloudRendererCamera.transform.localEulerAngles.z);
                        }

                        //cloudRendererCamera.transform.localEulerAngles = Vector3.zero;
                        //cloudPlane.transform.localEulerAngles = Vector3.zero;

                        // cloudPlane.transform.localEulerAngles = Vector3.Lerp(cloudPlane.transform.localEulerAngles, new Vector3((Camera.main.transform.eulerAngles.x - prevRotX),
                        //         (Camera.main.transform.eulerAngles.y - prevRotY),
                        //         cloudPlane.transform.localEulerAngles.z), Time.deltaTime);

                        // cloudRendererCamera.transform.localEulerAngles = Vector3.Lerp(cloudRendererCamera.transform.localEulerAngles, new Vector3(-(Camera.main.transform.eulerAngles.x - prevRotX),
                        //     -(Camera.main.transform.eulerAngles.y - prevRotY),
                        //     cloudRendererCamera.transform.localEulerAngles.z), Time.deltaTime);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!m_showSkyMap) return;
            GUI.DrawTexture(new Rect(0, 0, 256, 256), m_displaySkyMap);
        }

        public void UpdateMat(Material mat)
        {
            if (mat == null) return;

            mat.SetVector("betaR", m_betaR / SCALE);
            mat.SetFloat("mieG", m_mieG);
            mat.SetTexture("_Transmittance", m_transmittance);
            mat.SetTexture("_Irradiance", m_irradiance);
            mat.SetTexture("_Inscatter", m_inscatter);
            mat.SetTexture("_SkyMap", m_skyMap);
            mat.SetFloat("SUN_INTENSITY", m_sunIntensity);
            mat.SetVector("EARTH_POS", new Vector3(0.0f, 6360010.0f, 0.0f));
            mat.SetVector("SUN_DIR", m_sun.transform.forward * -1.0f);
            mat.SetVector("SUN_DIR_EULER", m_sun.transform.eulerAngles);
            //Debug.Log(m_sun.transform.eulerAngles);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Destroy(m_displaySkyMap);
                Destroy(m_skyMap);
                Destroy(m_transmittance);
                Destroy(m_inscatter);
                Destroy(m_irradiance);
            }
            else {
                DestroyImmediate(m_displaySkyMap);
                DestroyImmediate(m_skyMap);
                DestroyImmediate(m_transmittance);
                DestroyImmediate(m_inscatter);
                DestroyImmediate(m_irradiance);
            }
        }

        private float[] LoadRawFile(string path, int size)
        {
            FileInfo fi = new FileInfo(path);

            if (fi == null)
            {
                Debug.Log("Raw file not found (" + path + ")");
                return null;
            }

            FileStream fs = fi.OpenRead();
            byte[] data = new byte[fi.Length];
            fs.Read(data, 0, (int)fi.Length);
            fs.Close();

            //divide by 4 as there are 4 bytes in a 32 bit float
            if (size > fi.Length / 4)
            {
                Debug.Log("Raw file is not the required size (" + path + ")");
                return null;
            }

            float[] map = new float[size];
            for (int x = 0, i = 0; x < size; x++, i += 4)
            {
                //Convert 4 bytes to 1 32 bit float
                map[x] = System.BitConverter.ToSingle(data, i);
            };

            return map;
        }

        private Color[] ToColor(float[] data, int channels)
        {
            int count = data.Length / channels;
            Color[] col = new Color[count];
            
            for(int i = 0; i < count; i++)
            {
                if (channels > 0) col[i].r = data[i * channels + 0];
                if (channels > 1) col[i].g = data[i * channels + 1];
                if (channels > 2) col[i].b = data[i * channels + 2];
                if (channels > 3) col[i].a = data[i * channels + 3];
            }

            return col;
        }
    }
	
}

