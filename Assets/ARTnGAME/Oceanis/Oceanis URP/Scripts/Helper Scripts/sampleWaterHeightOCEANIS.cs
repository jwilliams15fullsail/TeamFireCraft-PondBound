using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Oceanis {
    [ExecuteInEditMode]
    public class sampleWaterHeightOCEANIS : MonoBehaviour
    {
        public Transform heightSampler;

        public Transform cubeSampler;

        public GlobalOceanisControllerURP oceanController;
        Vector3 prev_pos;
        public float lerpSpeed = 1;
        
        public float lerpRotSpeed = 1;
        public float boatLength = 1;

        public float waterHeight = 0;

        public float heightOffsetY = 0;
        public float heightFactor1 = 1;

        public bool useGernstner = false;
        public float lerpSpeedG = 1;

        // Start is called before the first frame update
        void Start()
        {
            start_pos = heightSampler.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (heightSampler != null)
            {
                Vector3 displacment = oceanController.GetWaterDisplacement(heightSampler.position);
                if (Mathf.Abs(displacment.y) < 40)
                {
                    Vector3 heightSampler_position = new Vector3(heightSampler.position.x + displacment.x,
                                                                    oceanController.GetWaterHeight(heightSampler.position) * heightFactor1 + heightOffsetY,
                                                                    heightSampler.position.z + +displacment.z);
                    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler_position, Time.deltaTime * lerpSpeed);

                    heightSampler.position = new Vector3(heightSampler.position.x, 
                        oceanController.GetWaterHeight(heightSampler.transform.position)* heightFactor1 + waterHeight + heightOffsetY, 
                        heightSampler.position.z);
                }


                //v0.1
                if(cubeSampler != null)
                {
                    cubeSampler.position = new Vector3(cubeSampler.position.x, oceanController.GetWaterHeight(cubeSampler.position), cubeSampler.position.z);
                }

                Vector3 MotionDirection = (heightSampler.position - prev_pos).normalized;
                //Debug.DrawLine(heightSampler.position, heightSampler.position + MotionDirection* 5,Color.red,1);
                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    Quaternion.AngleAxis(-100 * MotionDirection.y, (Vector3.up + MotionDirection)),
                    Time.deltaTime * lerpRotSpeed * 0.05f);

                //Sample boat back and front !!!
                Vector3 frontPos = heightSampler.position + heightSampler.forward * boatLength / 2;
                Vector3 backPos = heightSampler.position - heightSampler.forward * boatLength / 2;

                //Debug.DrawLine(frontPos, frontPos + Vector3.up * 4, Color.red, 1);
                //Debug.DrawLine(backPos, backPos + Vector3.up * 4, Color.red, 1);
                float frontH = oceanController.GetWaterHeight(frontPos);
                float backH = oceanController.GetWaterHeight(backPos);
                float diffBF = frontH - backH;
                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    Quaternion.AngleAxis(diffBF, heightSampler.right),
                    Time.deltaTime * lerpRotSpeed * 0.05f);
                //Debug.DrawLine(new Vector3(frontPos.x, frontH, frontPos.z), new Vector3(frontPos.x, frontH, frontPos.z) + Vector3.up * 4, Color.green, 2);
                //Debug.DrawLine(new Vector3(backPos.x, backH, backPos.z), new Vector3(backPos.x, backH, backPos.z) + Vector3.up * 4, Color.green, 2);
                Vector3 waterGradient = new Vector3(frontPos.x, frontH, frontPos.z) - new Vector3(backPos.x, backH, backPos.z);
                //if (controlBoat)
                //{
                //    Vector3 Forward_flat = new Vector3(heightSampler.forward.x, 0, heightSampler.forward.z);
                //    heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler.position + new Vector3(waterGradient.x * 1 * Input.GetAxis("Vertical"),
                //        0, waterGradient.z * 1 * Input.GetAxis("Vertical")), Time.deltaTime * BoatSpeed);

                //    CurrentRot = Mathf.Lerp(CurrentRot, CurrentRot + Input.GetAxis("Horizontal"), Time.deltaTime * BoatRotSpeed);
                //    heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                //         Quaternion.AngleAxis(CurrentRot, Vector3.up) * Quaternion.AngleAxis(diffBF * 2, heightSampler.right),
                //         Time.deltaTime * BoatRotSpeed);
                //}
                //------------------------------------------------FLOATERS
                #region Floaters
                /*
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
                                                                            WaterObjects[i].position.z + displacementTO.z);
                        if (ShiftHorPositionFloaters)
                        {
                            WaterObjectsWaterPos[i] = new Vector3(displacementTO.x * ShiftHorPositionFactor.x,
                                GerstnerOffsets1.y * heightFactor1 + heightOffsetY,
                                displacementTO.z * ShiftHorPositionFactor.z);
                        }
                        else
                        {
                            WaterObjectsWaterPos[i] = new Vector3(0, GerstnerOffsets1.y * heightFactor1 + heightOffsetY, 0);
                        }

                        if (alignToNormalFloaters)
                        {
                            WaterObjects[i].up = new Vector3((Mathf.Abs(displacementTO.x) - alignToNormalFactor.x),
                                (Mathf.Abs(displacementTO.y) - alignToNormalFactor.y),
                                (Mathf.Abs(displacementTO.z) - alignToNormalFactor.z));
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
                */
                #endregion
                //---------------------------------------------------END FLOATERS

                if (useGernstner && Application.isPlaying)
                {
                    float PosX = heightSampler.transform.position.x;
                    float PosZ = heightSampler.transform.position.z;
                    float PosY = heightSampler.transform.position.y;

                    Vector4 _GAmplitude = oceanController.waterMaterial.GetVector("_GAmplitude");                                               // amplitude
                    Vector4 _GFrequency = oceanController.waterMaterial.GetVector("_GFrequency");                                               // frequency
                    Vector4 _GSteepness = oceanController.waterMaterial.GetVector("_GSteepness");                                               // steepness
                    Vector4 _GSpeed = oceanController.waterMaterial.GetVector("_GSpeed");                                                   // speed
                    Vector4 _GDirectionAB = oceanController.waterMaterial.GetVector("_GDirectionAB");                                               // direction # 1, 2
                    Vector4 _GDirectionCD = oceanController.waterMaterial.GetVector("_GDirectionCD");
                    float heightFactor = 1;
                    //if (Waterhandler != null)
                    //{
                    //    heightFactor = (Waterhandler.waterScaleFactor.y / 1.0f + Waterhandler.waterScaleOffset.y) / 1.0f;//v3.2 - changed from 1.5f and 1.2f
                    //}

                    float heightFactor1 = 1;
                    //if (Waterhandler != null)
                    //{
                    //    //heightFactor1 = (Waterhandler.waterScaleFactor.y/1.0f  + Waterhandler.waterScaleOffset.y)/1.0f;
                    //    heightFactor1 = this.transform.localScale.y * 1.1f;
                    //}
                    Vector3 GerstnerOffsets = GerstnerOffset(new Vector2(PosX, PosZ), PosY, new Vector2(PosX, PosZ), _GAmplitude, _GFrequency, _GSteepness, _GSpeed, _GDirectionAB, _GDirectionCD);

                    //Debug.Log(GerstnerOffsets);

                    Vector3 ShiftPos = new Vector3(0, GerstnerOffsets.y * heightFactor, 0);

                    if (ShiftHorPosition)
                    {
                        ShiftPos = new Vector3(GerstnerOffsets.x, GerstnerOffsets.y * heightFactor, GerstnerOffsets.z);
                    }

                    if (LerpMotion)
                    {
                        start_pos.y = heightSampler.transform.position.y;
                        heightSampler.transform.position = Vector3.Lerp(heightSampler.transform.position, start_pos + ShiftPos, Time.deltaTime * lerpSpeedG);
                    }
                    else
                    {
                        heightSampler.transform.position = start_pos + ShiftPos;
                    }

                    Vector3 MotionDirectionA = (heightSampler.transform.position - prev_pos).normalized;

                    heightSampler.transform.rotation = Quaternion.Lerp(heightSampler.transform.rotation, 
                        Quaternion.AngleAxis(100 * MotionDirection.y, (Vector3.up + MotionDirection)), 
                        Time.deltaTime * lerpRotSpeed);//0.18f

                    Vector3 Velocity = (heightSampler.transform.position - prev_pos) / Time.deltaTime;                   

                }

                prev_pos = heightSampler.position;
            }         

        }

        public Vector3 start_pos;
        public bool ShiftHorPosition = false;
        public bool LerpMotion = false;

        ///// HELPER GERNSTNER
        public Vector3 GerstnerOffset(
            Vector2 Position, float PosY, Vector2 tileableVtx,                      // offsets, nrml will be written
                                                                                    //Vector4 _GAmplitude,												// amplitude
                                                                                    //Vector4 _GFrequency,												// frequency
                                                                                    //Vector4 _GSteepness,												// steepness
                                                                                    //Vector4 _GSpeed,													// speed
                                                                                    //Vector4 _GDirectionAB,												// direction # 1, 2
                                                                                    //Vector4 _GDirectionCD												// direction # 3, 4


        Vector4 amplitude, Vector4 frequency, Vector4 steepness,
        Vector4 speed, Vector4 directionAB, Vector4 directionCD

        )
        {
            Vector3 Offsets = Vector3.zero;

            float Intensity1 = oceanController.waterMaterial.GetFloat("_GerstnerIntensity1");
            //float Intensity2 = WaterMaterial.GetFloat("_GerstnerIntensity2");

            Vector4 _GerstnerIntensities = oceanController.waterMaterial.GetVector("_GerstnerIntensities");
            Vector4 _Gerstnerfactors2 = oceanController.waterMaterial.GetVector("_Gerstnerfactors2");
            Vector4 _Gerstnerfactors = oceanController.waterMaterial.GetVector("_Gerstnerfactors");
            Vector4 _GerstnerfactorsSteep = oceanController.waterMaterial.GetVector("_GerstnerfactorsSteep");
            Vector4 _GerstnerfactorsDir = oceanController.waterMaterial.GetVector("_GerstnerfactorsDir");

            Vector2 tileableVtx_xz = new Vector2(tileableVtx.x, tileableVtx.y);

            Offsets = GerstnerOffset4(tileableVtx_xz, steepness, amplitude, frequency, speed, directionAB, directionCD)
                + Intensity1 * 0.5f * GerstnerOffset4(tileableVtx_xz + new Vector2(0, 0), steepness / 36, amplitude * 2.2f * 4, frequency * 0.002f, speed * 0.1f, directionAB - new Vector4(11.1f, 0, 10.2f, 0), directionCD + new Vector4(111.1f, 0, 10.2f, 0))
                    //+ Intensity2*GerstnerOffset4(tileableVtx_xz+new Vector2(1,2), steepness/13, amplitude*0.5f, frequency*3, speed*0.4f, directionAB-new Vector4(111.1f,0,0.2f,0), directionCD+new Vector4(111.1f,110,0.2f,0));

                    //+ _GerstnerIntensity1*0.5*GerstnerOffset4(tileableVtx.xz+float3(1,2,3), steepness/36, amplitude*2.2*4, frequency*0.002, speed*0.1, directionAB-float4(11.1,0,10.2,0), directionCD+float4(111.1,0,10.2,0))		
                    //	+ _GerstnerIntensity2*GerstnerOffset4(tileableVtx.xz+float3(1,2,3), steepness/13, amplitude*0.5, frequency*3, speed*0.4, directionAB-float4(111.1,0,0.2,0), directionCD+float4(111.1,110,0.2,0));

                    //		nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, amplitude, frequency, speed, directionAB, directionCD)			
                    //			+ _GerstnerIntensity1*0.5*GerstnerNormal4(tileableVtx.xz+float3(1,2,3) + offs.xz, amplitude*4.2*4, frequency*0.002, speed*0.1, directionAB-float4(11.1,0,10.2,0), directionCD+float4(111.1,0,10.2,0)) 			
                    //				+ _GerstnerIntensity2*GerstnerNormal4(tileableVtx.xz+float3(1,2,3) + offs.xz, amplitude*0.6, frequency*3, speed*0.4, directionAB-float4(1.1,0,0.2,0), directionCD+float4(111.1,110,0.2,0));			

                    + _GerstnerIntensities.x * 0.1f * tileableVtx.x * GerstnerOffset4(tileableVtx, _GerstnerfactorsSteep.x * steepness, -_Gerstnerfactors2.x * amplitude, _Gerstnerfactors.x * frequency / 2, -0.2f * speed, -0.2f * new Vector4(_GerstnerfactorsDir.x * directionAB.x, directionAB.y, directionAB.z, directionAB.w), 0.3f * directionCD)
                    + _GerstnerIntensities.y * 0.15f * tileableVtx.y * GerstnerOffset4(tileableVtx, _GerstnerfactorsSteep.y * steepness, -_Gerstnerfactors2.y * 0.1f * amplitude, _Gerstnerfactors.y * 0.1f * frequency / 1.2f, -speed, -new Vector4(directionAB.x, _GerstnerfactorsDir.y * directionAB.y, directionAB.z, directionAB.w), 0.4f * directionCD)
                    + _GerstnerIntensities.z * 0.05f * PosY * GerstnerOffset4(tileableVtx, _GerstnerfactorsSteep.z * steepness, -_Gerstnerfactors2.z * 0.1f * amplitude, _Gerstnerfactors.z * 0.2f * frequency / 0.9f, -0.5f * speed, -0.5f * new Vector4(_GerstnerfactorsDir.z * directionAB.x, directionAB.y, directionAB.z, directionAB.w), directionCD)
                    ;

            return Offsets;
        }

        Vector3 GerstnerOffset4(Vector2 xzVtx, Vector4 steepness, Vector4 amp, Vector4 freq, Vector4 speed, Vector4 dirAB, Vector4 dirCD)
        {
            Vector3 offsets = Vector3.zero;

            //Vector4 steepness_xxyy = new Vector4(steepness.x,steepness.x,steepness.y,steepness.y);
            //Vector4 steepness_zzww = new Vector4(steepness.z,steepness.z,steepness.w,steepness.w);
            //Vector4 amp_xxyy = new Vector4(amp.x,amp.x,amp.y,amp.y);
            //Vector4 amp_zzww = new Vector4(amp.z,amp.z,amp.w,amp.w);
            //Vector4 dirAB_xyzw = new Vector4(dirAB.x,dirAB.y,dirAB.z,dirAB.w);
            //Vector4 dirCD_xyzw = new Vector4(dirCD.x,dirCD.y,dirCD.z,dirCD.w);

            //Vector4 freq_xyzw = new Vector4(freq.x,freq.y,freq.z,freq.w);

            Vector4 dirAB_xy = new Vector2(dirAB.x, dirAB.y);
            Vector4 dirAB_zw = new Vector2(dirAB.z, dirAB.w);
            Vector4 dirCD_xy = new Vector2(dirCD.x, dirCD.y);//HDRP - v4.9.8
            Vector4 dirCD_zw = new Vector2(dirCD.z, dirCD.w);//HDRP - v4.9.8

            //Vector4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
            //Vector4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
            Vector4 AB = new Vector4(steepness.x * amp.x * dirAB.x, steepness.x * amp.x * dirAB.y, steepness.y * amp.y * dirAB.z, steepness.y * amp.y * dirAB.w);  //steepness_xxyy * amp_xxyy * dirAB_xyzw;
            Vector4 CD = new Vector4(steepness.z * amp.z * dirCD.x, steepness.z * amp.z * dirCD.y, steepness.w * amp.w * dirCD.z, steepness.w * amp.w * dirCD.w);  //steepness_zzww * amp_zzww * dirCD_xyzw;

            Vector4 tempA = new Vector4(Vector2.Dot(dirAB_xy, xzVtx), Vector2.Dot(dirAB_zw, xzVtx), Vector2.Dot(dirCD_xy, xzVtx), Vector2.Dot(dirCD_zw, xzVtx));
            Vector4 dotABCD = new Vector4(freq.x * tempA.x, freq.y * tempA.y, freq.z * tempA.z, freq.w * tempA.w);
            //Vector4 TIME = _Time.yyyy * speed;
            //Vector4 TIME = Time.fixedTime * speed;
            //HDRP - v4.9.8
            Vector4 shaderTime = Shader.GetGlobalVector("_Time");
            Vector4 TIME = shaderTime.y * speed;

            Vector4 COS = new Vector4(Mathf.Cos(dotABCD.x + TIME.x), Mathf.Cos(dotABCD.y + TIME.y), Mathf.Cos(dotABCD.z + TIME.z), Mathf.Cos(dotABCD.w + TIME.w));
            Vector4 SIN = new Vector4(Mathf.Sin(dotABCD.x + TIME.x), Mathf.Sin(dotABCD.y + TIME.y), Mathf.Sin(dotABCD.z + TIME.z), Mathf.Sin(dotABCD.w + TIME.w));

            //		offsets.x = Vector4.Dot(COS, Vector4(AB.xz, CD.xz));
            //		offsets.z = Vector4.Dot(COS, Vector4(AB.yw, CD.yw));
            offsets.x = Vector4.Dot(COS, new Vector4(AB.x, AB.z, CD.x, CD.z));
            offsets.z = Vector4.Dot(COS, new Vector4(AB.y, AB.w, CD.y, CD.w));
            offsets.y = Vector4.Dot(SIN, amp);

            return offsets;
        }


    }
}