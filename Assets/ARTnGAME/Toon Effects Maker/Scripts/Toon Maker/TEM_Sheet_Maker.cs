using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.TEM {

public class TEM_Sheet_Maker : MonoBehaviour {

	void Start () {

			#if UNITY_WEBPLAYER
				Debug.Log ("Please choose a platform other than the webplayer in order to use the Sprite Sheet Maker");
			#endif

//			#if UNITY_EDITOR
//			#if !UNITY_5
//			if(!TEM_Layer_set){
//				SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
//				SerializedProperty it1 = tagManager.GetIterator();
//				bool showChildren = true;	
//				bool found = false;
//				while (it1.NextVisible(showChildren))
//				{
//					if(it1.propertyType == SerializedPropertyType.String){
//						if(it1.stringValue == "TEM_Sheet_Maker" & it1.name.Contains("User Layer")){
//							Camera.main.cullingMask = 1 << LayerMask.NameToLayer("TEM_Sheet_Maker");
//							TEM_Layer_set = true;
//							int layerMask = 1 << LayerMask.NameToLayer("TEM_Sheet_Maker");
//							RenderLayer = layerMask;
//							found = true;
//						}
//					}
//				}
//				if(!found){
//					Debug.Log ("Toon Effect Maker warning - Please add the 'TEM_Sheet_Maker' layer to use the Sprite Sheet Maker");
//					return;
//				}
//			}
//			#endif
//			#endif

		atlas = new Texture2D(Screen.width * Col_count , Screen.height * Row_count,TextureFormat.ARGB32,false);
		atlasTextures = new List<Texture2D>();

		atlasTextures2 = new List<Texture2D>();

		Cam_init_col = Camera.main.backgroundColor;

			//#if UNITY_5
			Camera.main.cullingMask = RenderLayer;
			//#endif
	
		foreach(GameObject Obj in Objects){
			
			//Debug.Log(Obj.layer);
			//Debug.Log(LayerMask.NameToLayer("TEM_Sheet_Maker"));			

			MoveToLayer(Obj.transform, LayerMask.NameToLayer("TEM_Sheet_Maker"));			

			if(Text_debug){
				TextMesh TextMeshy = Obj.GetComponentInChildren(typeof(TextMesh)) as TextMesh;
				if(TextMeshy!=null){
					TextObjects.Add (TextMeshy);
				}
			}

			Obj.SetActive(false);
			
		}
	}

		public bool Save_debug_frames = false;

	public bool Unity_packer = false;
	public bool grad_edge_fade = false;
	public float Cut_off_dist = 0.014f;//low at 0.004 to grab round error, 0.12 gives nice result in border
	public float Check_Cut_off = 0.014f;

		public bool Remove_init_check=false;

	public bool Text_debug = false;
	public List<TextMesh> TextObjects;

	public List<GameObject> Objects;

	public Color Cam_init_col;
	public Color Cut_off_color = Color.black;
	public Color Check_color = Color.black;

	public bool Use_unity_atlas = false;
	public List<Texture2D> atlasTextures;
	public List<Texture2D> atlasTextures2;
	public Rect[] rects;
	public int Row_count=2;
	public int Col_count=2;
	Texture2D atlas;
		public Vector4 Remove_border = new Vector4(7,7,7,7);//remove 7 pixels by default

	public bool StartCreatingSheet=false;
	int counter=0;
	public int CaptureEvery=10;
	int currentframe;

	public float MaxTime = 5;
	float time;
	string path ="";

	public LayerMask RenderLayer;

	void MoveToLayer(Transform root, int layer) {
		root.gameObject.layer = layer;
		foreach(Transform child in root)
			MoveToLayer(child, layer);
	}

		public bool real_time_use=false;
		public Material Test_mat;
		public GameObject Test_obj;
		public bool Atlas_created=false;
		public int Upscale_factor=2;

		public bool TEM_Layer_set=true;

	void Update () {

//			#if UNITY_EDITOR
//			#if !UNITY_5
//			if(!TEM_Layer_set){
//				SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
//				SerializedProperty it1 = tagManager.GetIterator();
//				bool showChildren = true;	
//				bool found = false;
//				while (it1.NextVisible(showChildren))
//				{
//					if(it1.propertyType == SerializedPropertyType.String){
//						if(it1.stringValue == "TEM_Sheet_Maker" & it1.name.Contains("User Layer")){
//							Camera.main.cullingMask = 1 << LayerMask.NameToLayer("TEM_Sheet_Maker");
//							TEM_Layer_set = true;
//							int layerMask = 1 << LayerMask.NameToLayer("TEM_Sheet_Maker");
//							RenderLayer = layerMask;
//							found = true;
//						}
//					}
//				}
//				if(!found){
//					//Debug.Log ("Toon Effect Maker warning - Please add the 'TEM_Sheet_Maker' layer to use the Sprite Sheet Maker");
//					return;
//				}
//			}
//			#endif
//			#endif

			if(Atlas_created & real_time_use){
				if(Test_obj!=null){
					if(!Test_obj.activeInHierarchy){
						Test_obj.SetActive(true);
					}
				}
			}

		for(int i=0;i<TextObjects.Count;i++){
			TextObjects[i].text = counter.ToString();
		}

		if(Use_unity_atlas){
			MaxTime = (2 * Row_count * Col_count)/3;// Divide by 3 to get max time and mutliply by two to be on the safe side
		}

		if(StartCreatingSheet){

			foreach(GameObject Obj in Objects){

				Obj.SetActive(true);
			}

			if(path.Equals (""))
			{
				#if UNITY_EDITOR
				if(!real_time_use){
					path = EditorUtility.SaveFilePanel("Save",Application.dataPath,"","");
				}
				#endif

				if(path.Equals ("") & !real_time_use){
					StartCreatingSheet = false;
				}
			}

			if(!toggle_cam){
				time+= Time.deltaTime;
				currentframe++;

				if(currentframe > CaptureEvery){

					StartCoroutine("CaptureScreen"); toggle_cam = true;
					currentframe=0;
					counter++;
					//Debug.Log("Active");

					if( (time > MaxTime & Unity_packer) | (counter > (Row_count * Col_count)+1)  ){	//Unity_packer
						time=0;
						StartCreatingSheet = false;

						int count_textures=0;
						foreach(Texture2D texture in atlasTextures2){							

							for (int x = 0; x < texture.width; x++) {
								for (int y = 0; y < texture.height; y++) {
									Color color = texture.GetPixel(x, y);
									//float alpha = color.a;

									Color Mirror_texture_color = atlasTextures[count_textures].GetPixel(x, y);

									float Color_dist = Vector3.Distance(new Vector3(Check_color.r,Check_color.g,Check_color.b),new Vector3(color.r,color.g,color.b));

										float Check_Color_dist = Vector3.Distance(new Vector3(Cam_init_col.r,Cam_init_col.g,Cam_init_col.b),new Vector3(Mirror_texture_color.r,Mirror_texture_color.g,Mirror_texture_color.b));

									//Cut_off_color
									if( (color == Check_color 
									     | (color.r == Check_color.r & color.g ==Check_color.g & color.b == Check_color.b)
									     | Color_dist < Cut_off_dist
									     )
										   & (Remove_init_check | 
										   ( !Remove_init_check & 
										 ((Mirror_texture_color.r == Cam_init_col.r & 
										 Mirror_texture_color.g == Cam_init_col.g & 
										  Mirror_texture_color.b == Cam_init_col.b) | Check_Color_dist < Check_Cut_off)
										 ))
									)
									{

										if(grad_edge_fade){
											//fade transparency from cut off
											color.a = Vector3.Distance(new Vector3(Cut_off_color.r,Cut_off_color.g,Cut_off_color.b),new Vector3(color.r,color.g,color.b));
										}else{
											color = Color.clear;
											color.a = 0;
										}

										if(color == Check_color
										   | (color.r == Check_color.r & color.g ==Check_color.g & color.b == Check_color.b)
										   | Color_dist < Cut_off_dist
										   )
										{
											color = Color.clear;
											color.a = 0;
										}
										texture.SetPixel(x, y, color);
									}
								}
							}
							count_textures++;
						}

						if(Unity_packer){
							rects = atlas.PackTextures(atlasTextures2.ToArray(), 2, 2*Mathf.Max(Screen.width *Col_count, Screen.height * Row_count));
						}else{
							//APPLY PACKING and resize

							int textureWidthCounter = 0;
							int textureHeightCounter = (Row_count-1)*Screen.height;
							//go through textures
							for (int i = 0; i < (Row_count*Col_count)+0; i++) {

									//go through pixels
									for (int x = 0; x < Screen.width; x++) {
										for (int y = 0; y < Screen.height; y++) {

											atlas.SetPixel(x + textureWidthCounter, y + textureHeightCounter, atlasTextures2[i].GetPixel(x,y));
										
											//remove bug line above texture
											//if(y==0 | y==1 | y==2 | y==3 | y==4 | y==5){
											if(x < Remove_border.x | x > Screen.width - Remove_border.y | y < Remove_border.z | y > Screen.height - Remove_border.w){
												atlas.SetPixel(x + textureWidthCounter, y + textureHeightCounter, new Color(0,0,0,0));
											}										
										}
									}

								if(textureWidthCounter >= (Col_count-1)*Screen.width){
									textureHeightCounter -=  atlasTextures2[i].height;
									textureWidthCounter=0;
								}else{
									textureWidthCounter  +=  atlasTextures2[i].width;
								}

							}
							atlas.Apply();							
						}

						if(Use_unity_atlas){
								Texture2D temp = ScaleTexture(atlas,Screen.width * Col_count*Upscale_factor,Screen.height * Row_count*Upscale_factor);

								if(!real_time_use){
									#if UNITY_EDITOR
									#if !UNITY_WEBPLAYER
									byte[] bytes2  = temp.EncodeToPNG();
									File.WriteAllBytes(path+counter+"Sheet.png",bytes2);
									Debug.Log("Atlas written");
									#endif
									#endif
								}else{
									Test_mat.mainTexture = temp;
									Atlas_created = true;
									Debug.Log("Atlas created");
								}
						}
					}
				}
			}else{
				//
			}
		}
	}

	bool toggle_cam = false;

	private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
		Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,true);
		Color[] rpixels=result.GetPixels(0);
		float incX=(1.0f / (float)targetWidth);
		float incY=(1.0f / (float)targetHeight);
		for(int px=0; px<rpixels.Length; px++) {
			rpixels[px] = source.GetPixelBilinear(incX*((float)px%targetWidth), incY*((float)Mathf.Floor(px/targetWidth)));
		}
		result.SetPixels(rpixels,0);
		result.Apply();
		return result;
	}

	IEnumerator CaptureScreen2()
	{
		Camera.main.backgroundColor = Check_color;
		yield return new WaitForEndOfFrame();
		
		Texture2D texture1 = new Texture2D(Screen.width, Screen.height,TextureFormat.ARGB32,false);
		texture1.ReadPixels(new Rect(0,0,Screen.width, Screen.height),0,0);
		texture1.Apply();

		//Debug.Log ("CAPTURE2");

		atlasTextures2.Add(texture1);

		Counters.Add(counter);	
		
		yield return 0;

			#if UNITY_EDITOR
			#if !UNITY_WEBPLAYER
		if(!real_time_use){
				if(Save_debug_frames){
			byte[] bytes1 = texture1.EncodeToPNG();
			File.WriteAllBytes(path+counter+"A.png",bytes1);
				}
		}
			#endif
			#endif

		toggle_cam = false;
		Time.timeScale = 1;
		Camera.main.backgroundColor = Cam_init_col;		
	}

	public List<int> Counters = new List<int>();

	IEnumerator CaptureScreen()
	{
		Camera.main.backgroundColor = Cam_init_col;

		yield return new WaitForEndOfFrame();

		Texture2D texture = new Texture2D(Screen.width, Screen.height,TextureFormat.ARGB32,false);
		texture.ReadPixels(new Rect(0,0,Screen.width, Screen.height),0,0);
		texture.Apply();

		atlasTextures.Add(texture);

		Time.timeScale = 0;

		//Debug.Log ("CAPTURE1");
					
		yield return 0;

			#if UNITY_EDITOR
			#if !UNITY_WEBPLAYER
			if(!real_time_use){
				if(Save_debug_frames){
		byte[] bytes  = texture.EncodeToPNG();
		File.WriteAllBytes(path+counter+".png",bytes);
				}
			}
			#endif
			#endif

		Camera.main.backgroundColor = Check_color;
		StartCoroutine("CaptureScreen2");
	}


		public bool Use_GUI=false;
		void OnGUI(){

			if(!creating_sheet & Use_GUI){
				if(GUI.Button(new Rect(5,5,140,25),"Create sheet")){

					StartCreatingSheet = true;
					creating_sheet = true;

					if(TEM_Layer_set || 1==1){
						Debug.Log ("Working ...");
					}else{
						Debug.Log ("Toon Effect Maker warning - Please add the 'TEM_Sheet_Maker' layer to use the Sprite Sheet Maker");
					}
				}
				GUI.Label(new Rect(5,5+30,340,25),"Border cutoff (0=thick,1=thin) = "+Mathf.Abs(Cut_off_dist));
				GUI_border_cut_off = GUI.HorizontalSlider(new Rect(5,5+55,140,25),Cut_off_dist,0.01f,1);
				Cut_off_dist = GUI_border_cut_off;
				Check_Cut_off = GUI_border_cut_off;

			}else if(Use_GUI){
				//GUI.Label(new Rect(0,0,240,15),"Working ...");
				//Debug.Log ("Working ...");
			}
		}
		bool creating_sheet=false;
		float GUI_border_cut_off = 0.11f;
}
}