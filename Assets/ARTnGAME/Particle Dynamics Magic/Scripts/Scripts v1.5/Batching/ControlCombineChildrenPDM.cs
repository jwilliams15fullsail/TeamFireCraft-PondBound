using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Artngame.PDM {
	/*
Attach this script as a parent to some game objects. The script will then combine the meshes at startup.
This is useful as a performance optimization since it is faster to render one big mesh than many small meshes. See the docs on graphics performance optimization for more info.

Different materials will cause multiple meshes to be created, thus it is useful to share as many textures/material as you can.
*/
	
	//[AddComponentMenu("Mesh/Combine Children")]
	public class ControlCombineChildrenPDM : MonoBehaviour {
		
		public bool generateTriangleStrips = true;
		//private Vector3 hiddenPosition = new Vector3(0, -100000, 0);
		
		
		public bool Auto_Disable=false;
		
		public int skip_every_N_frame=0;
		
		bool run_once=false;
		
		/// This option has a far longer preprocessing time at startup but leads to better runtime performance.
		void Start () {
			
			if(Destroy_list==null){
				Destroy_list = new List<GameObject>();
			}			
			
			Component[] filters  = GetComponentsInChildren(typeof(MeshFilter));
			//Matrix4x4 myTransform = transform.worldToLocalMatrix;
			//Hashtable materialToMesh= new Hashtable();
			
			//v1.7
			if(Self_dynamic_enable & !run_once){
				if(Children_list!=null){
					if(filters.Length != Children_list.Count){
						//if(filters[i].gameObject != 
						Children_list.Clear ();
						Positions_list.Clear();
						
						if(Self_dynamic_check_rot){
							Rotations_list.Clear();
						}
						if(Self_dynamic_check_scale){
							Scale_list.Clear();
						}						
					}
				}
			}
			
			for (int i=0;i<filters.Length;i++) {
				//MeshFilter filter = (MeshFilter)filters[i];
				Renderer curRenderer  = filters[i].GetComponent<Renderer>();
				
				//v1.7
				if(Self_dynamic_enable & !run_once){
					if(Children_list!=null){
						if(filters.Length != Children_list.Count){
							//if(filters[i].gameObject != 
							Children_list.Add (filters[i].gameObject.transform);
							Positions_list.Add(filters[i].gameObject.transform.position);
							
							//if(Self_dynamic_check_rot){
								Rotations_list.Add(filters[i].gameObject.transform.rotation);
							//}
							//if(Self_dynamic_check_scale){
								Scale_list.Add(filters[i].gameObject.transform.localScale);
							//}
						}
					}
				}
				
				if (curRenderer != null && !curRenderer.enabled ) {
					
					
					curRenderer.enabled = true;
				}
			}	
			
			//run_once = true;
			previous_children_count = transform.childCount; 
			
			self_dyn_enable_time = Time.fixedTime;
		}
		
		public bool MakeActive=false;
		private List<GameObject> Destroy_list;
		int count_frames;
		
		//v1.7
		private List<Vector3> Positions_list;
		private List<Quaternion> Rotations_list;
		private List<Vector3> Scale_list;
		private List<Transform> Children_list;
		public bool Self_dynamic_enable=false;
		public bool Self_dynamic_check_rot=false;
		public bool Self_dynamic_check_scale=false;
		
		public bool is_active=false;
		
		public float Min_dist = 0.2f; //minimal move distance to trigger mesh update
		int previous_children_count;
		public float stop_self_dyn_after = 0; //let objects settle and then stop self_dyn after x secs, if > 0
		float self_dyn_enable_time;
		public bool Decombine = false;
		
		void LateUpdate(){
			
			is_active=false;//start with false, if active signal it
			
			if(Self_dynamic_enable){
				
				if(stop_self_dyn_after > 0){
					if(Time.fixedTime - self_dyn_enable_time > stop_self_dyn_after){
						self_dyn_enable_time = Time.fixedTime;
						Self_dynamic_enable = false;
					}
				}
				
				
				if(Children_list!=null){
					//v1.7 check if items in list are null and remove from both lists.
					for(int i=Children_list.Count-1;i>=0;i--){
						if(Children_list[i]!=null){
							if(Children_list[i].gameObject == null){
								Children_list.RemoveAt(i);
								Positions_list.RemoveAt(i);
							}
						}
					}
					//if item changed position
					for(int i=Children_list.Count-1;i>=0;i--){
						if(Children_list[i]!=null){
							//if(Children_list[i].position != Positions_list[i]){
							if(Vector3.Distance(Children_list[i].position,Positions_list[i]) > Min_dist){
								MakeActive=true;
								Positions_list[i] = Children_list[i].position; //save new pos
								//Debug.Log ("ID ="+i);
							}
							if(Self_dynamic_check_rot){
								if(Rotations_list[i] != Children_list[i].rotation){
									MakeActive=true;
									Rotations_list[i] = Children_list[i].rotation; //save new rot
								}
							}
							if(Self_dynamic_check_scale){
								if(Scale_list[i] != Children_list[i].localScale){
									MakeActive=true;
									Scale_list[i] = Children_list[i].localScale; //save new scale
								}
							}
						}
					}
					
					
					//if item has been added
					
					int child_count  = transform.childCount; //v1.8.1 Check PREVIOUS COUNT and not children count
					//int child_count  = previous_children_count;
					
					if(child_count != previous_children_count){
						
						previous_children_count = child_count;
						
						//if(child_count != Children_list.Count){
						//if(filters[i].gameObject != 
						//Children_list.Add (filters[i].gameObject.transform);
						//Positions_list.Add(filters[i].gameObject.transform.position);
						MakeActive=true;
						//Debug.Log ("COUNT DIFF"+child_count);
						//Debug.Log ("COUNT DIFF2 = "+Children_list.Count);
					}
					
				}else{
					Children_list = new List<Transform>();
					Positions_list = new List<Vector3>();
					//if(Self_dynamic_check_rot){
						Rotations_list = new List<Quaternion>();
					//}
					//if(Self_dynamic_check_scale){
						Scale_list = new List<Vector3>();
					//}
				}
			}			
			
			
			if(Decombine){
				Decombine = false; //disable immediately
				
				Start (); //reenable meshfilters
				MakeActive = false;
				is_active = false;
				Self_dynamic_enable = false;
				
				//clean up
				MeshFilter filter1  = this.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
				if(filter1!=null){
					Mesh meshD = filter1.sharedMesh;// this.gameObject.GetComponent <MeshFilter>().sharedMesh;
					//Destroy(filter1);
					DestroyImmediate(meshD,true);
					DestroyImmediate(filter1,true);
				}else{					
					if(Destroy_list.Count>0){
						for(int i=0;i<Destroy_list.Count;i++){
							MeshFilter filter11  = Destroy_list[i].GetComponent(typeof(MeshFilter)) as MeshFilter;
							if(filter11!=null){
								Mesh meshD = filter11.sharedMesh;// this.gameObject.GetComponent <MeshFilter>().sharedMesh;
								//Destroy(filter1);
								DestroyImmediate(meshD,true);
								DestroyImmediate(filter11,true);
							}
						}
						for(int i=Destroy_list.Count-1;i>=0;i--){
							DestroyImmediate(Destroy_list[i]);
							Destroy_list.RemoveAt(i);
						}
					}						
				}
				
			}
			
			//erase previous mesh ?
			if(MakeActive){
				
				//Debug.Log("INSIDE");
				is_active=true;//start with false, if active signal it
				
				if(Auto_Disable){
					MakeActive=false;
				}
				
				if(skip_every_N_frame>0){
					if(count_frames >= skip_every_N_frame){ 
						count_frames=0; 
						//Debug.Log ("Return"); 
						return;
					}else{
						count_frames=count_frames+1;
					}
					//return;
				}
				
				//activate children
				if(1==1){
					Start ();
					
					MeshFilter filter1  = this.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if(filter1!=null){
						Mesh meshD = filter1.sharedMesh;// this.gameObject.GetComponent <MeshFilter>().sharedMesh;
						//Destroy(filter1);
						DestroyImmediate(meshD,true);
						DestroyImmediate(filter1,true);
					}else{
						
						if(Destroy_list.Count>0){
							for(int i=0;i<Destroy_list.Count;i++){
								MeshFilter filter11  = Destroy_list[i].GetComponent(typeof(MeshFilter)) as MeshFilter;
								if(filter11!=null){
									Mesh meshD = filter11.sharedMesh;// this.gameObject.GetComponent <MeshFilter>().sharedMesh;
									//Destroy(filter1);
									DestroyImmediate(meshD,true);
									DestroyImmediate(filter11,true);
								}
							}
							for(int i=Destroy_list.Count-1;i>=0;i--){
								DestroyImmediate(Destroy_list[i]);
								Destroy_list.RemoveAt(i);
							}
						}						
					}
					
					Component[] filters  = GetComponentsInChildren(typeof(MeshFilter));
					
					//MeshFilter[] filters  = GetComponentsInChildren(typeof(MeshFilter)) as MeshFilter[];
					
					if(filters != null | 1==0 ){
						if(filters.Length >0 | 1==0){
							
							Matrix4x4 myTransform = transform.worldToLocalMatrix;
							Hashtable materialToMesh= new Hashtable();
							
							//Debug.Log ("Filters count = "+filters.Length);
							
							int Group_start = 0;
							int Group_end = filters.Length;	
							
							//for (int i=0;i<filters.Length;i++) {
							for (int i=Group_start;i<Group_end;i++) {
								MeshFilter filter = (MeshFilter)filters[i];
								Renderer curRenderer  = filters[i].GetComponent<Renderer>();
								MeshCombineUtilityPDM.MeshInstance instance = new MeshCombineUtilityPDM.MeshInstance ();
								instance.mesh = filter.sharedMesh;
								if (curRenderer != null && curRenderer.enabled && instance.mesh != null) {
									instance.transform = myTransform * filter.transform.localToWorldMatrix;
									
									Material[] materials = curRenderer.sharedMaterials;
									for (int m=0;m<materials.Length;m++) {
										instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);
										
										ArrayList objects = (ArrayList)materialToMesh[materials[m]];
										if (objects != null) {
											objects.Add(instance);
										}
										else
										{
											objects = new ArrayList ();
											objects.Add(instance);
											materialToMesh.Add(materials[m], objects);
										}
									}
									
									curRenderer.enabled = false;
								}
							}								
							
							foreach (DictionaryEntry de  in materialToMesh) {
								ArrayList elements = (ArrayList)de.Value;
								MeshCombineUtilityPDM.MeshInstance[] instances = (MeshCombineUtilityPDM.MeshInstance[])elements.ToArray(typeof(MeshCombineUtilityPDM.MeshInstance));
								
								
								List<int> Split_index = new List<int>();
								Split_index.Add(0);
								
								int vertexes_count = 0;
								for(int i=0;i<instances.Length;i++){
									
									//MeshFilter filter = (MeshFilter)filters[i];
									vertexes_count = vertexes_count + instances[i].mesh.vertexCount;// filter.sharedMesh.vertexCount;
									
									if(vertexes_count > 64000){
										vertexes_count = 0;
										Split_index.Add(i);
										//Debug.Log ("Split at ="+i);
									}
								}
								
								//Debug.Log ("Matrial ID ="+de.Key+" "+de.Value);
								
								for (int j=0;j<Split_index.Count;j++) {	
									
									if(j < Split_index.Count-1){
										Group_start = Split_index[j];
										Group_end = Split_index[j+1]-1;
									}else{
										Group_start = Split_index[j];
										Group_end =  instances.Length;
									}
									
									MeshCombineUtilityPDM.MeshInstance[] instances_Split = new MeshCombineUtilityPDM.MeshInstance[Group_end-Group_start];
									for (int k=0;k<(Group_end-Group_start);k++) {
										instances_Split[k] = instances[Group_start+k-0];
									}									
									
									GameObject go = new GameObject("Combined mesh");
									go.transform.parent = transform;
									go.transform.localScale = Vector3.one;
									go.transform.localRotation = Quaternion.identity;
									go.transform.localPosition = Vector3.zero;
									go.AddComponent(typeof(MeshFilter));
									go.AddComponent<MeshRenderer>();
									go.GetComponent<Renderer>().material = (Material)de.Key;
									MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
									filter.mesh = MeshCombineUtilityPDM.Combine(instances_Split, generateTriangleStrips);
									
									Destroy_list.Add(go);
								}
								
							}
							
							
						}}//END check filters array if exists
					
				}//end if use bones
				//END IF BONES
			}	
			
			
			
			
		}
		
	}
}