using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

public class LOD_PDM : MonoBehaviour {

	// Use this for initialization
	void Start () {

		last_dist_check = Time.fixedTime;
		Disabled_for_dist = new List<GameObject>();
			Disabled_for_dist_TAGS = new List<string>();
			//Tags_to_disable = new List<string>();

	}


	public bool Disable_distant = false;
	//public bool Also_Disable_splines = false;
	//public bool Disable_LOD_PDM_Layer = false;
	public float check_dist_interval = 0.5f; //seconds to delay new seach
		//public float check_dist_OBJ_interval = 0.5f;
	private float last_dist_check;

		[HideInInspector]
		public List<GameObject> Disabled_for_dist;
		[HideInInspector]
		public List<string> Disabled_for_dist_TAGS;

	public float cut_off_dist = 80f;
		//public float cut_off_OBJ_dist = 150f;

		public List<string> Tags_to_disable; 
		public bool use_PDM_LOD=true;
	
	void Update () {

			///int Layer_ID = LayerMask.NameToLayer("LOD_PDM"); 

		if(Disable_distant){

			if(Time.fixedTime - last_dist_check > check_dist_interval){
				
				last_dist_check = Time.fixedTime;
				
				for (int i=Disabled_for_dist.Count-1;i>=0;i--){

					if(Disabled_for_dist[i] == null){
						Disabled_for_dist.RemoveAt(i);
						Disabled_for_dist_TAGS.RemoveAt(i);
					}
				}

				for (int i=Disabled_for_dist.Count-1;i>=0;i--){

					if(Vector3.Distance(Disabled_for_dist[i].transform.position, Camera.main.transform.position) <= (cut_off_dist-2)  ){
						Disabled_for_dist[i].SetActive(true);
					//	Disabled_for_dist[i].tag = "PDM_LOD";

							Disabled_for_dist[i].tag =	Disabled_for_dist_TAGS[i];

						Disabled_for_dist.RemoveAt(i);
							Disabled_for_dist_TAGS.RemoveAt(i);
							break;
					}
					
				}
						
					if(use_PDM_LOD){
						object[] obj = GameObject.FindGameObjectsWithTag("PDM_LOD");

						foreach (object o in obj)
						{
							GameObject g = (GameObject) o;

								if(Vector3.Distance(g.transform.position, Camera.main.transform.position) > (cut_off_dist+5) ){
									g.SetActive(false);	
									
									
									Disabled_for_dist_TAGS.Add(g.tag);

									g.tag = "Untagged";
									Disabled_for_dist.Add(g);
								}
													
						}
					}
					//

					if(Tags_to_disable !=null){

						if(Tags_to_disable.Count>0){

							for(int i =0;i<Tags_to_disable.Count;i++){

								object[] obj1 = GameObject.FindGameObjectsWithTag(Tags_to_disable[i]);
								
								foreach (object o in obj1)
								{
									GameObject g = (GameObject) o;
									
									if(Vector3.Distance(g.transform.position, Camera.main.transform.position) > (cut_off_dist+5) ){
										g.SetActive(false);	

										Disabled_for_dist_TAGS.Add(g.tag);

										g.tag = "Untagged";
										Disabled_for_dist.Add(g);
									}
									
								}

							}

						}

					}

					//

			}

		}

		
	}
			
}
}