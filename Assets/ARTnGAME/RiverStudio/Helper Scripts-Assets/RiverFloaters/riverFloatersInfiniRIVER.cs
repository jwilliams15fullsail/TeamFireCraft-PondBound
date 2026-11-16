using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.RiverStudio {
    public class riverFloatersInfiniRIVER : MonoBehaviour
    {
        public RenderTexture flow;
        public RenderTexture flowJACOB;
        public RenderTexture flowADVECT;
        public GameObject floaterOBJ;
        //public Camera depthCamera;
        public Material watrMat; //read depth camera from water shader to avoid referencing the different pipelines depth cameras
        public OceanRenderer waterHandler;
        public int samplingPixels = 10;
        public float depthOrthoCameraSize = 120;

        public int updatePerFrames = 0;
        public int currentFrames = 0;
        public bool pushObject = false;
        public float pushPower = 1;

        // Start is called before the first frame update
        void Start()
        {
            flowtexture = new Texture2D(samplingPixels+1, samplingPixels+1, TextureFormat.RGB24, false);
        }

        public Texture2D flowtexture;
        // Update is called once per frame
        public Vector3 debugVelocity;
        public Vector2 debugPixelPos;
        void Update()
        {
            currentFrames++;

            if (floaterOBJ != null && watrMat != null && (updatePerFrames == 0 || (updatePerFrames > 0 && currentFrames > updatePerFrames)) )
            {
                currentFrames = 0;

                //_DepthCameraPos("_DepthCameraPos", Float) = (0,0,0,0)
                //_TerrainScale("_TerrainScale", Float) = 0
                //read texture at the object position and move abject
                Vector4 depthCamPos = watrMat.GetVector("_DepthCameraPos");
                float TerrainScale = waterHandler.TerrainScale;// watrMat.GetFloat("_TerrainScale");
                float startX = 0;
                float startY = 0;

                //depthOrthoCameraSize = 120, 240 side to side, 240m = 1024 pixels (flow.width)
                float flowTexSize = flow.width;
                startX = (floaterOBJ.transform.position.x - depthCamPos.x);//express in relation to the depth camera center
                startX = (startX + depthOrthoCameraSize / 2); //translate to move to texture space where 0,0 is in the corner
                startX = startX * (flowTexSize / depthOrthoCameraSize);// multiply with pixels per meters to convert meters to texture pixels

                startY = (floaterOBJ.transform.position.z - depthCamPos.z);//express in relation to the depth camera center
                startY = (startY + depthOrthoCameraSize / 2); //translate to move to texture space where 0,0 is in the corner
                startY = startY * (flowTexSize / depthOrthoCameraSize);// multiply with pixels per meters to convert meters to texture pixels
                debugPixelPos = new Vector2(startX, startY);

                Rect rectReadPicture = new Rect(startX - samplingPixels/2, startY- samplingPixels / 2, startX+ samplingPixels / 2, startY+ samplingPixels / 2);
                RenderTexture.active = flow;
                // Read pixels
                flowtexture.ReadPixels(rectReadPicture, 0, 0);
                flowtexture.Apply();
                RenderTexture.active = null; // added to avoid errors 
                Vector3 accumulateVel = Vector3.zero;
                for (int i = 0; i < samplingPixels+1; i++)
                {
                    for (int j = 0; j < samplingPixels+1; j++)
                    {
                        Color velocity = flowtexture.GetPixel(j, i);
                        //Debug.Log(velocity);
                         accumulateVel += new Vector3(velocity.r, velocity.g, velocity.b);
                    }
                }
                debugVelocity = accumulateVel;
                if (pushObject)
                {
                    floaterOBJ.transform.position += new Vector3(-accumulateVel.y, 0, -accumulateVel.x) * pushPower;
                }
            }
        }
    }
}