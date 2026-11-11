//TerrainDepthInfiniGRASS.cs
using UnityEngine;
using System.Collections.Generic;

namespace Artngame.CommonTools.DepthRenderer
{

	[ExecuteInEditMode]
	public class DepthRendererCT_SRP : MonoBehaviour
	{

		//v2.1.25
		public RenderTexture layer1;
		public float heightFactorLayer1 = 0.5f; //to raise the lower ones

		//v2.0.8 - add depth to other materials
		public List<Material> DepthToMats = new List<Material>();

		//HANDLE CAMERA
		public bool followCamera = false;
		public Transform TerrainDepthCamera;
		public bool UpdateOnCamMove = false; // update only when the camera is moving, to further reduce the minor displacement when a lower res rendertexture is used
		Vector3 prevPos;
		Vector3 prevUpdatePos;
		public float speedThres = 10.5f;
		public bool debugSpeed = false;
		public int UpdateDist = 256;
		public float stopTimedUpdateIn = 10;

		float lastFollowTime = 0;
		public float followInterval = 0;
		//public Material oceanMat;


		[Range(0f, 15f)]//5
		public float heightFactor = 0.5f;
		[Range(0f, 1.1f)]//0.1
		public float heightCutoff = 0.1f;
		[Range(0.01f, 1.15f)]//0.15
		public float contrast = 0.1f;


		public Shader DepthShader;
		Material material;


        void Update()
        {

            //follow camera


            //v2.1.25
            //			GetComponent<Camera> ().Render ();
            //			prevCullMask = GetComponent<Camera> ().cullingMask;
            //			GetComponent<Camera> ().cullingMask = layer1Mask.value;
            //			GetComponent<Camera> ().Render ();
            //			GetComponent<Camera> ().cullingMask = prevCullMask;


            //shore line
            //			if (TerrainDepthCamera != null && oceanMat != null) {
            //				oceanMat.SetVector("_DepthCameraPos", new Vector4(TerrainDepthCamera.position.x,TerrainDepthCamera.position.y,TerrainDepthCamera.position.z,1));
            //			}
            //			if (oceanMat != null) {
            //				oceanMat.SetFloat("_ShoreFadeFactor", ShoreWavesFade);
            //				oceanMat.SetFloat("_TerrainScale", TerrainScale);
            //			}

            if (Application.isPlaying)
            {

                //if (Time.fixedTime - lastFollowTime > followInterval || Vector3.Distance( prevUpdatePos, Camera.main.transform.position ) > UpdateDist ) {
                if (stopTimedUpdateIn == 0 || (Time.fixedTime < stopTimedUpdateIn && Time.fixedTime - lastFollowTime > followInterval) || Vector3.Distance(prevUpdatePos, Camera.main.transform.position) > UpdateDist)
                {
                    //if (Vector3.Distance( prevUpdatePos, Camera.main.transform.position ) > UpdateDist ) {

                    //Debug.Log ("in");
                    //GetComponent<Camera> ().enabled = true;

                    if (!UpdateOnCamMove || (UpdateOnCamMove && (Vector3.Distance(prevPos, Camera.main.transform.position) / Time.deltaTime) > speedThres))
                    { //prevPos != Camera.main.transform.position)){

                        //GetComponent<Camera> ().enabled = true; //v2.1.21

                        if (debugSpeed)
                        {
                            Debug.Log("Camera Speed:" + (Vector3.Distance(prevPos, Camera.main.transform.position) / Time.deltaTime).ToString());
                        }
                        if (followCamera && Camera.main != null && TerrainDepthCamera != null)
                        {
                            Vector3 Campos = Camera.main.transform.position;
                            TerrainDepthCamera.position = new Vector3(Campos.x, TerrainDepthCamera.position.y, Campos.z);
                        }
                        lastFollowTime = Time.fixedTime;
                        prevUpdatePos = Camera.main.transform.position;

                        //v2.1.21
                        GetComponent<Camera>().Render();

                        //v2.1.25
                        if (layer1 != null)
                        {
                            //GetComponent<Camera> ().Render ();
                            toggleLayer = true;
                            prevCullMask = GetComponent<Camera>().cullingMask;
                            GetComponent<Camera>().cullingMask = layer1Mask.value;
                            GetComponent<Camera>().Render();
                            GetComponent<Camera>().cullingMask = prevCullMask;
                            toggleLayer = false;
                        }
                    }
                    lastFollowTime = Time.fixedTime;

                }
                else
                {
                    //GetComponent<Camera> ().enabled = false;
                    if (stopTimedUpdateIn == 0 || Time.fixedTime < stopTimedUpdateIn)
                    {

                        //v2.1.21
                        //						if (Time.fixedTime - lastFollowTime > 4 * followInterval / 5) {
                        //							GetComponent<Camera> ().enabled = true;
                        //						} else {
                        //							GetComponent<Camera> ().enabled = true;//false;
                        //						}

                        //v2.1.21
                        GetComponent<Camera>().Render();

                    }
                    else
                    {

                        //if (Vector3.Distance (prevUpdatePos, Camera.main.transform.position) > 9 * UpdateDist / 10) { //v2.1.1
                        //GetComponent<Camera> ().enabled = true;
                        //} else {
                        GetComponent<Camera>().enabled = false;
                        //}
                    }
                }

                prevPos = Camera.main.transform.position;



            }

            //v0.1
            if (!Application.isPlaying)
            {
                if (followCamera && Camera.main != null && TerrainDepthCamera != null)
                {
                    Vector3 Campos = Camera.main.transform.position;
                    TerrainDepthCamera.position = new Vector3(Campos.x, TerrainDepthCamera.position.y, Campos.z);
                }
            }

		}



		private void Start ()
		{

		
			//HANDLE CAMERA
//			if (TerrainDepthCamera != null && oceanMat != null) {
//				oceanMat.SetVector("_DepthCameraPos", new Vector4(TerrainDepthCamera.position.x,TerrainDepthCamera.position.y,TerrainDepthCamera.position.z,1));
//				//oceanMat.SetFloat("_ShoreFadeFactor", ShoreWavesFade);
//				//oceanMat.SetFloat("_TerrainScale", TerrainScale);
//			}
//			if (oceanMat != null) {
//				oceanMat.SetFloat("_ShoreFadeFactor", ShoreWavesFade);
//				oceanMat.SetFloat("_TerrainScale", TerrainScale);
//			}

			if (followCamera && Camera.main != null && TerrainDepthCamera != null) {
				Vector3 Campos = Camera.main.transform.position;
				TerrainDepthCamera.position = new Vector3 (Campos.x, TerrainDepthCamera.position.y, Campos.z);
			}
			prevPos = Camera.main.transform.position;



			if (DepthShader == null) {
				DepthShader = Shader.Find ("SkyMaster/TerrainDepthSM");
			}

			if (material == null)
			{
				material = new Material(DepthShader);
				material.hideFlags = HideFlags.HideAndDontSave;
			}

			GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

			//if (!SystemInfo.supportsImageEffects)
			//{
			//	Debug.Log("Image effects not available");
			//	enabled = false;
			//	return;
			//}
			if (!DepthShader.isSupported || DepthShader == null)
			{
				enabled = false;
				Debug.Log("DepthShader " + DepthShader.name + " not available");
				return;
			}          
		}

		private void OnDisable()
		{
			if (material != null) {
				DestroyImmediate (material);
			}
		}

//		void OnPreRender(){
//
//			if (Application.isPlaying) {
//
//				if (Time.fixedTime - lastFollowTime > followInterval) {
//					Debug.Log ("in");
//					GetComponent<Camera> ().enabled = true;
//					if (followCamera && Camera.main != null && TerrainDepthCamera != null) {
//						Vector3 Campos = Camera.main.transform.position;
//						TerrainDepthCamera.position = new Vector3 (Campos.x, TerrainDepthCamera.position.y, Campos.z);
//					}
//					lastFollowTime = Time.fixedTime;
//				}else {
//					GetComponent<Camera> ().enabled = false;
//				}
//			} 
//		}


		void LateUpdate(){
//
//			if (TerrainDepthCamera != null && oceanMat != null) {
//				oceanMat.SetVector("_DepthCameraPos", new Vector4(TerrainDepthCamera.position.x,TerrainDepthCamera.position.y,TerrainDepthCamera.position.z,1));
//			}
//
			//v2.0.8
			for(int i=0;i<DepthToMats.Count;i++){
				DepthToMats [i].SetVector ("_DepthCameraPos",new Vector4(TerrainDepthCamera.position.x,TerrainDepthCamera.position.y,TerrainDepthCamera.position.z,1  ));
			}

		}
	
		//v2.1.25
		private void OnPostRender(){
			//layer1.colorBuffer = GetComponent<Camera> ().targetTexture.colorBuffer;
			//Graphics.CopyTexture(GetComponent<Camera> ().targetTexture,layer1);

			//GetComponent<Camera> ().cullingMask = prevCullMask;
		}
		private void OnPreRender(){
			//prevCullMask = GetComponent<Camera> ().cullingMask;
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{

//			if (Application.isPlaying) {
//
//				if (Time.fixedTime - lastFollowTime > followInterval) {
//					//Debug.Log ("in");
//					//GetComponent<Camera> ().enabled = true;
//					if (followCamera && Camera.main != null && TerrainDepthCamera != null) {
//						Vector3 Campos = Camera.main.transform.position;
//						TerrainDepthCamera.position = new Vector3 (Campos.x, TerrainDepthCamera.position.y, Campos.z);
//					}
//					lastFollowTime = Time.fixedTime;
//				}else {
//					//GetComponent<Camera> ().enabled = false;
//				}
//			} 

			//Graphics.Blit (source, destination);	
	
			//if (Time.fixedTime - lastFollowTime > followInterval) {
				if (material == null) {
					material = new Material (DepthShader);
					material.hideFlags = HideFlags.HideAndDontSave;
				}

				if (DepthShader != null) {

					//v2.1.25
					if(layer1 != null && toggleLayer){
						material.SetFloat ("_heightFactor",heightFactorLayer1);
					}else{
						material.SetFloat ("_heightFactor", heightFactor);
					}
					material.SetFloat ("_heightCutoff", heightCutoff);
					material.SetFloat ("_contrast", contrast);
						
					//Graphics.Blit (source, destination, material);
					//Graphics.Blit (source, destination);
					//Graphics.Blit (destination, destination, material);

					//v.2.1.25
					if (layer1 != null && toggleLayer) {					
						//GetComponent<Camera> ().cullingMask = layer1Mask.value;
						RenderTexture temp = RenderTexture.GetTemporary(layer1.width, layer1.height);
						Graphics.Blit (source, temp, material,0);
						//GetComponent<Camera> ().cullingMask = prevCullMask;
						material.SetTexture ("_MainTexLayer1", temp);
						Graphics.Blit (destination, layer1, material,1);
						toggleLayer = false;
						temp.Release ();
					}else{
						Graphics.Blit (source, destination, material,0);
					}

				} else {
					Graphics.Blit (source, destination);
				}

			//}
		}

		//v2.1.25
		public LayerMask layer1Mask;
		int prevCullMask;
		bool toggleLayer = false;

	}
}