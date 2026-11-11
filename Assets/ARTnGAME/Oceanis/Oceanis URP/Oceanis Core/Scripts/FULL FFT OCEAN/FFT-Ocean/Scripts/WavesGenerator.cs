using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
namespace Artngame.Oceanis
{
    public class WavesGenerator : MonoBehaviour
    {
        public Transform heightSampler;
        public float lodScale = 10;

        public WavesCascade cascade0;
        public WavesCascade cascade1;
        public WavesCascade cascade2;

        // must be a power of 2
        [SerializeField]
        int size = 256;

        [SerializeField]
        WavesSettings wavesSettings;
        [SerializeField]
        bool alwaysRecalculateInitials = false;

        public Vector4 lengthScales = new Vector4(1, 1, 1, 1);

        [SerializeField]
        float lengthScale0 = 250;
        [SerializeField]
        float lengthScale1 = 17;
        [SerializeField]
        float lengthScale2 = 5;

        [SerializeField]
        ComputeShader fftShader;
        [SerializeField]
        ComputeShader initialSpectrumShader;
        [SerializeField]
        ComputeShader timeDependentSpectrumShader;
        [SerializeField]
        ComputeShader texturesMergerShader;

        Texture2D gaussianNoise;
        FastFourierTransform fft;
        Texture2D physicsReadback;

        private void Awake()
        {
            Application.targetFrameRate = -1;
            fft = new FastFourierTransform(size, fftShader);
            gaussianNoise = GetNoiseTexture(size);

            cascade0 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
            cascade1 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
            cascade2 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);

            InitialiseCascades();

            physicsReadback = new Texture2D(size, size, TextureFormat.RGBAFloat, false);

            //v0.1
            //Awake();
            //if (alwaysRecalculateInitials)
            //{
            //    InitialiseCascades();
            //}
            float timers = 0;
            for (int i = 0; i < 120; i++)
            {
                timers = timers + 0.01f;
                cascade0.CalculateWavesAtTime(timers);
                cascade1.CalculateWavesAtTime(timers);
                cascade2.CalculateWavesAtTime(timers);
            }
            RequestReadbacks();
        }

        void InitialiseCascades()
        {
            float boundary1 = 2 * Mathf.PI / lengthScale1 * 6f;
            float boundary2 = 2 * Mathf.PI / lengthScale2 * 6f;
            cascade0.CalculateInitials(wavesSettings, lengthScale0, 0.0001f, boundary1);
            cascade1.CalculateInitials(wavesSettings, lengthScale1, boundary1, boundary2);
            cascade2.CalculateInitials(wavesSettings, lengthScale2, boundary2, 9999);

            Shader.SetGlobalFloat("LengthScale0", lengthScale0);
            Shader.SetGlobalFloat("LengthScale1", lengthScale1);
            Shader.SetGlobalFloat("LengthScale2", lengthScale2);
            Shader.SetGlobalVector("lengthScales", lengthScales);
        }

        void Start()
        {

        }

        private void Update()
        {
            if (alwaysRecalculateInitials)
            {
                InitialiseCascades();
            }

            cascade0.CalculateWavesAtTime(Time.time);
            cascade1.CalculateWavesAtTime(Time.time);
            cascade2.CalculateWavesAtTime(Time.time);

            RequestReadbacks();

            //v0.1
            if (heightSampler != null)
            {


                // heightSampler.transform.position = new Vector3(heightSampler.transform.position.x,  GetWaterHeight(heightSampler.transform.position),heightSampler.transform.position.z);
                Vector3 displacment = GetWaterDisplacement(heightSampler.position);

                if (Mathf.Abs(displacment.y) < 40)
                {
                    Vector3 heightSampler_position = new Vector3(heightSampler.position.x + displacment.x,
                                                                    GetWaterHeight(heightSampler.position),
                                                                    heightSampler.position.z + +displacment.z);
                    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler_position, Time.deltaTime * lerpSpeed);

                    heightSampler.position = new Vector3(heightSampler.position.x, GetWaterHeight(heightSampler.transform.position), heightSampler.position.z);
                }

                Vector3 MotionDirection = (heightSampler.position - prev_pos).normalized;
                //Debug.DrawLine(heightSampler.position, heightSampler.position + MotionDirection* 5,Color.red,1);
                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    Quaternion.AngleAxis(-100 * MotionDirection.y, (Vector3.up + MotionDirection)),
                    Time.deltaTime * lerpRotSpeed * 0.05f);

                //Sample boat back and front !!!
                Vector3 frontPos = heightSampler.position + heightSampler.forward * boatLength / 2;
                Vector3 backPos = heightSampler.position - heightSampler.forward * boatLength / 2;

                Debug.DrawLine(frontPos, frontPos + Vector3.up * 4, Color.red, 1);
                Debug.DrawLine(backPos, backPos + Vector3.up * 4, Color.red, 1);
                float frontH = GetWaterHeight(frontPos);
                float backH = GetWaterHeight(backPos);
                float diffBF = frontH - backH;
                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    Quaternion.AngleAxis(diffBF, heightSampler.right),
                    Time.deltaTime * lerpRotSpeed * 0.05f);
                Debug.DrawLine(new Vector3(frontPos.x, frontH, frontPos.z), new Vector3(frontPos.x, frontH, frontPos.z) + Vector3.up * 4, Color.green, 2);
                Debug.DrawLine(new Vector3(backPos.x, backH, backPos.z), new Vector3(backPos.x, backH, backPos.z) + Vector3.up * 4, Color.green, 2);
                Vector3 waterGradient = new Vector3(frontPos.x, frontH, frontPos.z) - new Vector3(backPos.x, backH, backPos.z);
                if (controlBoat)
                {
                    Vector3 Forward_flat = new Vector3(heightSampler.forward.x, 0, heightSampler.forward.z);
                    //start_pos = start_pos + Forward_flat * BoatSpeed * Input.GetAxis("Vertical");


                    //heightSampler.position = heightSampler.position + Forward_flat * BoatSpeed * Input.GetAxis("Vertical");// Forward_flat * BoatSpeed * Input.GetAxis("Vertical");
                    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler.position + new Vector3(waterGradient.x * 1 * Input.GetAxis("Vertical"),
                        0, waterGradient.z * 1 * Input.GetAxis("Vertical")), Time.deltaTime * BoatSpeed);

                    //CurrentRot = CurrentRot + Input.GetAxis("Horizontal") * BoatRotSpeed;
                    CurrentRot = Mathf.Lerp(CurrentRot, CurrentRot + Input.GetAxis("Horizontal"), Time.deltaTime * BoatRotSpeed);

                    //heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation, Quaternion.AngleAxis(CurrentRot, Vector3.up), Time.deltaTime * BoatRotSpeed);  
                    //heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation, Quaternion.AngleAxis(CurrentRot, heightSampler.up) * Quaternion.AngleAxis(diffBF, heightSampler.right), Time.deltaTime * BoatRotSpeed);
                    heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                        Quaternion.AngleAxis(CurrentRot, Vector3.up) * Quaternion.AngleAxis(diffBF * 2, heightSampler.right),
                        Time.deltaTime * BoatRotSpeed);
                }






                //------------------------------------------------FLOATERS
                #region Floaters
                if (castFloaters)
                {
                    //HDRP 1 - sample randomly to add wave splashes
                    if (waterSpray != null)
                    {
                        if (waterSprays.Count == 0)
                        { //create 6 instances
                            for (int i = 0; i < maxSplashes; i++)
                            {
                                GameObject splash = Instantiate(waterSpray);
                                ParticleSystem[] partsA = splash.GetComponentsInChildren<ParticleSystem>(true);
                                if (partsA.Length > 0)
                                {
                                    partsA[0].gameObject.SetActive(false); //and disable them
                                }
                                waterSprays.Add(splash);
                            }
                        }
                        for (int i = 0; i < waterSprays.Count; i++)
                        {
                            //sample a random placement for the first disabled
                            if (!waterSprays[i].activeInHierarchy)
                            {
                                float aheadDistX = 4 * Random.Range(splashDistMin, splashDistMax);
                                float aheadDistZ = 4 * Random.Range(splashDistMin, splashDistMax);
                                float PosX1 = Camera.main.transform.position.x + Camera.main.transform.forward.x * aheadDistX;
                                float PosZ1 = Camera.main.transform.position.z + Camera.main.transform.forward.z * aheadDistZ;

                                PosX1 = PosX1 + Camera.main.transform.right.x * Random.Range(-splashDistMin, splashDistMax) * 2;
                                PosZ1 = PosZ1 + Camera.main.transform.right.z * Random.Range(-splashDistMin, splashDistMax) * 2;

                                float PosY1 = Camera.main.transform.position.y + Camera.main.transform.forward.y;
                                //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                                Vector3 cameraPos = new Vector3(PosX1, PosY1, PosZ1);
                                Vector3 displacementTO = GetWaterDisplacement(cameraPos);
                                Vector3 GerstnerOffsets1 = new Vector3(cameraPos.x + displacementTO.x,
                                                                                    GetWaterHeight(cameraPos),
                                                                                    cameraPos.z + +displacementTO.z);
                                waterSprays[i].transform.position = new Vector3(GerstnerOffsets1.x * 1 + PosX1, GerstnerOffsets1.y * heightFactor1 + heightOffsetY, GerstnerOffsets1.z * 1 + PosZ1);
                                waterSprays[i].SetActive(true);
                                //v0.6 - splash
                                ParticleSystem[] parts = waterSprays[i].GetComponentsInChildren<ParticleSystem>(true);
                                if (parts.Length > 0)
                                {
                                    parts[0].gameObject.SetActive(true);
                                }
                                break;//stop here
                            }
                            else //if stopped play, disable again                        
                            {
                                ParticleSystem[] parts = waterSprays[i].GetComponentsInChildren<ParticleSystem>(true);
                                if (parts.Length > 0)
                                {
                                    if (!parts[0].isPlaying)
                                    {
                                        parts[0].gameObject.SetActive(false);
                                        waterSprays[i].SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                    for (int i = WaterObjects.Count - 1; i >= 0; i--)
                    {
                        if (WaterObjects[i] == null)
                        {
                            WaterObjects.RemoveAt(i);
                            WaterObjectsWaterPos.RemoveAt(i);
                            WaterObjectsStartPos.RemoveAt(i);
                        }
                    }
                    for (int i = ThrowObjects.Count - 1; i >= 0; i--)
                    {
                        if (ThrowObjects[i] == null)
                        {
                            ThrowObjects.RemoveAt(i);
                            ThrowObjectsWaterPos.RemoveAt(i);
                            ThrowObjectsStartPos.RemoveAt(i);
                        }
                    }
                    for (int i = 0; i < WaterObjects.Count; i++)
                    {
                        float PosX1 = WaterObjects[i].position.x;
                        float PosZ1 = WaterObjects[i].position.z;
                        float PosY1 = WaterObjects[i].position.y;
                        //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                        Vector3 displacementTO = GetWaterDisplacement(WaterObjects[i].position);
                        Vector3 GerstnerOffsets1 = new Vector3(WaterObjects[i].position.x + displacementTO.x,
                                                                            GetWaterHeight(WaterObjects[i].position),
                                                                            WaterObjects[i].position.z + +displacementTO.z);
                        if (ShiftHorPositionFloaters)
                        {
                            WaterObjectsWaterPos[i] = new Vector3(GerstnerOffsets1.x * 1, GerstnerOffsets1.y * heightFactor1 + heightOffsetY, GerstnerOffsets1.z * 1);
                        }
                        else
                        {
                            WaterObjectsWaterPos[i] = new Vector3(0, GerstnerOffsets1.y * heightFactor1 + heightOffsetY, 0);
                        }
                    }
                    for (int i = 0; i < WaterObjectsWaterPos.Count; i++)
                    {
                        if (LerpMotion)
                        {
                            WaterObjects[i].position = Vector3.Lerp(WaterObjects[i].position, new Vector3(WaterObjectsStartPos[i].x, this.transform.position.y - 0.45f, WaterObjectsStartPos[i].z) + WaterObjectsWaterPos[i], Time.deltaTime * lerpSpeed);
                        }
                        else
                        {
                            WaterObjects[i].position = new Vector3(WaterObjectsStartPos[i].x, this.transform.position.y - 0.45f, WaterObjectsStartPos[i].z) + WaterObjectsWaterPos[i];
                        }
                    }
                    for (int i = 0; i < ThrowObjects.Count; i++)
                    {
                        float PosX1 = ThrowObjects[i].position.x;
                        float PosZ1 = ThrowObjects[i].position.z;
                        float PosY1 = ThrowObjects[i].position.y;
                        //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                        Vector3 displacementTO = GetWaterDisplacement(ThrowObjects[i].position);
                        Vector3 GerstnerOffsets1 = new Vector3(ThrowObjects[i].position.x + displacementTO.x,
                                                                            GetWaterHeight(ThrowObjects[i].position),
                                                                            ThrowObjects[i].position.z + +displacementTO.z);
                        ThrowObjectsWaterPos[i] = new Vector3(GerstnerOffsets1.x, GerstnerOffsets1.y * heightFactor1, GerstnerOffsets1.z);
                    }
                    for (int i = 0; i < ThrowObjectsWaterPos.Count; i++)
                    {
                        if (ThrowObjectsWaterPos[i].y + this.transform.position.y > ThrowObjects[i].position.y)
                        {
                            //v0.6 - splash
                            ParticleSystem[] parts = ThrowObjects[i].GetComponentsInChildren<ParticleSystem>(true);
                            if (parts.Length > 0)
                            {
                                parts[0].gameObject.SetActive(true);
                            }
                            //add to water list
                            ThrowObjects[i].GetComponent<Rigidbody>().isKinematic = true;
                            WaterObjects.Add(ThrowObjects[i]);
                            WaterObjectsStartPos.Add(ThrowObjects[i].position);
                            WaterObjectsWaterPos.Add(ThrowObjectsWaterPos[i]);
                            ThrowObjectsWaterPos.RemoveAt(i);
                            ThrowObjects.RemoveAt(i);
                            ThrowObjectsStartPos.RemoveAt(i);
                        }
                    }
                    if (Input.GetMouseButtonDown(1))
                    { //v3.4
                        if (ThrowItem != null)
                        {
                            GameObject TEMP = (GameObject)Instantiate(ThrowItem, heightSampler.transform.position + new Vector3(0, 5, 0), Quaternion.identity);
                            TEMP.transform.localScale = TEMP.transform.localScale * 5;
                            ThrowObjects.Add(TEMP.transform);
                            ThrowObjectsWaterPos.Add(new Vector3(0, 0, 0));
                            ThrowObjectsStartPos.Add(TEMP.transform.position);
                            if (TEMP != null)
                            {
                                Rigidbody RGB = TEMP.GetComponent<Rigidbody>() as Rigidbody;
                                if (RGB != null)
                                {
                                    if (Camera.main != null)
                                    {
                                        RGB.AddForce(Camera.main.transform.forward * ThrowPower);
                                    }
                                    else
                                    {
                                        RGB.AddForce(heightSampler.transform.forward * ThrowPower);
                                    }
                                }
                            }
                        }
                    }
                    //HDRP
                    Vector3 Velocity = (heightSampler.transform.position - prev_pos) / Time.deltaTime;

                    //HDRP - check collisions with boat
                    for (int i = 0; i < WaterObjects.Count; i++)
                    {
                        float distBoatBall = Vector3.Distance(WaterObjects[i].position, heightSampler.transform.position);
                        if (distBoatBall < 6)
                        {
                            WaterObjects[i].GetComponent<Rigidbody>().isKinematic = false;
                            ThrowObjects.Add(WaterObjects[i]);
                            ThrowObjectsStartPos.Add(WaterObjects[i].position);
                            ThrowObjectsWaterPos.Add(WaterObjectsWaterPos[i]);

                            WaterObjectsWaterPos.RemoveAt(i);
                            WaterObjects.RemoveAt(i);
                            WaterObjectsStartPos.RemoveAt(i);

                            Rigidbody RGB = ThrowObjects[ThrowObjects.Count - 1].GetComponent<Rigidbody>() as Rigidbody;
                            if (RGB != null)
                            {
                                if (Camera.main != null)
                                {
                                    //RGB.AddForce(Camera.main.transform.forward * ThrowPower);
                                    RGB.AddForce((Vector3.up * (0.1f + 0.12f * Random.Range(0.0f, 1.0f)) + 0.0075f * new Vector3(Velocity.x, 0, Velocity.z)) * ThrowPower);
                                }
                                else
                                {
                                    RGB.AddForce(heightSampler.transform.forward * ThrowPower);
                                }
                            }
                        }
                    }
                }
                #endregion
                //---------------------------------------------------END FLOATERS

                prev_pos = heightSampler.position;
            }
        }

        //[Header("-------------------- Planar Waves Generator --------------------")]
        [Header("--------------------- Boat and Floaters ------------------------")]
        public bool controlBoat = false;
        public float boatLength = 5;
        public float lerpSpeed = 4;
        public float lerpRotSpeed = 20;
        Vector3 prev_pos;
        float CurrentRot;
        public float BoatSpeed = 1;
        public float BoatRotSpeed = 1;

        //FLOATERS   
        public bool castFloaters = false;
        public float heightOffsetY = 0;
        public float heightFactor1 = 1;
        public bool ShiftHorPositionFloaters = false;
        public bool LerpMotion = false;
        //public float lerpSpeed = 1;
        //HDRP v0.2
        public GameObject waterSpray;
        public List<GameObject> waterSprays = new List<GameObject>();
        public int maxSplashes = 6;
        public float splashDistMin = 30;
        public float splashDistMax = 70;//////
        public List<Transform> ThrowObjects = new List<Transform>();
        public List<Vector3> ThrowObjectsWaterPos = new List<Vector3>();
        public List<Vector3> ThrowObjectsStartPos = new List<Vector3>();
        //public List<Vector3> ThrowObjectsStartPos;
        public List<Transform> WaterObjects = new List<Transform>();
        public List<Vector3> WaterObjectsWaterPos = new List<Vector3>();
        public List<Vector3> WaterObjectsStartPos = new List<Vector3>();
        public GameObject ThrowItem;
        public float ThrowPower = 1150;
        //END FLOATERS


        Texture2D GetNoiseTexture(int size)
        {
            string filename = "GaussianNoiseTexture" + size.ToString() + "x" + size.ToString();
            Texture2D noise = Resources.Load<Texture2D>("GaussianNoiseTextures/" + filename);
            return noise ? noise : GenerateNoiseTexture(size, true);
        }

        Texture2D GenerateNoiseTexture(int size, bool saveIntoAssetFile)
        {
            Texture2D noise = new Texture2D(size, size, TextureFormat.RGFloat, false, true);
            noise.filterMode = FilterMode.Point;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    noise.SetPixel(i, j, new Vector4(NormalRandom(), NormalRandom()));
                }
            }
            noise.Apply();

#if UNITY_EDITOR
            if (saveIntoAssetFile)
            {
                string filename = "GaussianNoiseTexture" + size.ToString() + "x" + size.ToString();
                string path = "Assets/Resources/GaussianNoiseTextures/";
                AssetDatabase.CreateAsset(noise, path + filename + ".asset");
                Debug.Log("Texture \"" + filename + "\" was created at path \"" + path + "\".");
            }
#endif
            return noise;
        }

        float NormalRandom()
        {
            return Mathf.Cos(2 * Mathf.PI * Random.value) * Mathf.Sqrt(-2 * Mathf.Log(Random.value));
        }

        private void OnDestroy()
        {
            cascade0.Dispose();
            cascade1.Dispose();
            cascade2.Dispose();
        }

        void RequestReadbacks()
        {
            AsyncGPUReadback.Request(cascade0.Displacement, 0, TextureFormat.RGBAFloat, OnCompleteReadback);
        }

        public float GetWaterHeight(Vector3 position)
        {
            Vector3 displacement = GetWaterDisplacement(position);
            displacement = GetWaterDisplacement(position - displacement);
            displacement = GetWaterDisplacement(position - displacement);

            return GetWaterDisplacement(position - displacement).y;
        }

        public Vector3 GetWaterDisplacement(Vector3 position)
        {
            Color c = physicsReadback.GetPixelBilinear(position.x / lengthScale0, position.z / lengthScale0);
            return new Vector3(c.r, c.g, c.b) * (lodScale / 10); //v0.1
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest request) => OnCompleteReadback(request, physicsReadback);

        void OnCompleteReadback(AsyncGPUReadbackRequest request, Texture2D result)
        {
            if (request.hasError)
            {
                Debug.Log("GPU readback error detected.");
                return;
            }
            if (result != null)
            {
                result.LoadRawTextureData(request.GetData<Color>());
                result.Apply();
            }
        }
    }
}