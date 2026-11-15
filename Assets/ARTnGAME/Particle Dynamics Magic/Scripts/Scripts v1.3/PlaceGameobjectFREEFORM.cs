using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
	public class PlaceGameobjectFREEFORM : MonoBehaviour {
	
	void Start () {

		
			if(p11==null){
				p11=this.gameObject.GetComponent<ParticleSystem>();
				
				if(p11!=null){
					ParticleSystem.MainModule main = p11.main;//v2.3
					main.maxParticles = particle_count;
					p11.Emit(particle_count);
				}
				
			}
			
			if(p11 !=null){
				aaa = new ParticleSystem.Particle[p11.particleCount];
			}

		//Grab_time=Time.fixedTime;


			noise = new PerlinPDM ();
			
			if(Application.isPlaying){
				
				
				if(Preview_mode){
					
					for(int i=Parent_OBJ.transform.childCount-1;i>=0;i--){
						DestroyImmediate(Parent_OBJ.transform.GetChild(i).gameObject);
					}
					
				}
				
				
				Preview_mode=false;
				
				if(Gameobj_instances!=null){
					for(int i=Gameobj_instances.Count-1;i>=0;i--){
						
						DestroyImmediate(Gameobj_instances[i]);
					}
				}
				

			}
			
			//colliders_last_status = Remove_colliders;

			Current_Grow_time = Time.fixedTime;
	}

	void Awake () {
		p11 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

			if(Parent_OBJ==null | Gameobj == null | p11==null){
				Debug.Log ("Please add a pool gameobject and a gameobject to be emitted");
			}

			if(Application.isPlaying){Preview_mode=false;}

		//Flammable_objects = GameObject.FindGameObjectsWithTag("Flammable");

		//Flamer_objects = GameObject.FindGameObjectsWithTag("Flamer");

		Registered_enflamed_positions = new List<Vector2>();

	}


		public bool Preview_mode=false;
		
		public int particle_count = 100;
		[HideInInspector]
		public List<GameObject> Gameobj_instances;
		
		public GameObject Gameobj;
		public bool Gravity_Mode=false;
		public float grav_factor=0.1f;
		
		public GameObject Parent_OBJ;
		
		public bool Angled=false;
		public bool Asign_rot=false;
		public Vector3 Local_rot = Vector3.zero;
		private PerlinPDM  noise;
		public float Wind_speed=1f;
		
		public float Return_speed=0.005f;
		
		public bool follow_particles=false;
		public bool Remove_colliders=false;
		
		//private bool colliders_last_status=false;
		private ParticleSystem.Particle[] aaa;


		public bool Use_stencil=false;
		public Texture2D Stencil;
		public Vector2 tex_scale= new Vector2(3,3);
		public float Coloration_ammount = 0.5f; 

	 //GameObject[] Flammable_objects;

	 //GameObject[] Flamer_objects;

	 List<Vector2> Registered_enflamed_positions; 

	[HideInInspector]
	public int maxemitter_count;
	[HideInInspector]
	public int current_emitters_count;

	public float brush_size=1f;

	public bool Erase_mode=false;
	public float Marker_size=0.5f;

	public ParticleSystem p11;

	[HideInInspector]
	public List<GameObject> Emitter_objects;
	[HideInInspector]
		public List<Vector3> Registered_paint_positions; 
		[HideInInspector]
		public List<Vector3> Updated_Registered_paint_positions; 



		[HideInInspector]
		public List<Vector3> Registered_paint_rotations; 
		public bool fix_initial = false;


	[HideInInspector]
	public List<Vector3> Registered_initial_positions;
	[HideInInspector]
	public List<Quaternion> Registered_initial_rotation;

		[HideInInspector]
		public List<Vector3> Registered_initial_normal_rotation;

	[HideInInspector]
	public List<Vector3> Registered_initial_scale;

	ParticleSystem.Particle[] ParticleList;

	//private float Grab_time;
	public float Delay=1;
	public bool Optimize=false;

	public bool relaxed = true;
	
	public bool draw_in_sequence;

		public bool Enable_particles;
		public bool Scale_by_texture;
		public bool Color_by_texture;
		public bool Follow_scale;
		public bool Grow_trees;
		public float Grow_time=1f;
		private float Current_Grow_time;

		Vector3 Keep_last_mouse_pos;

	void LateUpdate () {

		


			if(p11==null){
				p11 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
			}

			if(Parent_OBJ==null | Gameobj == null | p11==null){
				Debug.Log ("Please add a pool gameobject and a gameobject to be emitted");
			}

			if(aaa == null){
				
				aaa = new ParticleSystem.Particle[p11.particleCount];
				
			}



			if(noise ==null){
				noise = new PerlinPDM ();
			}


			if(Application.isPlaying){Preview_mode=false;}

		

			if(Registered_enflamed_positions==null){
				Registered_enflamed_positions = new List<Vector2>();
			}

			if(Preview_mode & !Application.isPlaying){
				
				if(Gameobj_instances!=null){
					
					if(Parent_OBJ.transform.childCount > Gameobj_instances.Count){
						
						for(int i=Parent_OBJ.transform.childCount-1;i>=0;i--){
							DestroyImmediate(Parent_OBJ.transform.GetChild(i).gameObject);
						}
						
					}
					
				}
				
			}

			if(Gameobj_instances==null){
				Gameobj_instances = new List<GameObject>();
			}
			
			
			if(!Preview_mode & !Application.isPlaying){
				
				for(int i=Gameobj_instances.Count-1;i>=0;i--){
					
					DestroyImmediate(Gameobj_instances[i]);
				}
				
			}
			
			if(Gameobj_instances!=null){
				for(int i=Gameobj_instances.Count-1;i>=0;i--){
					
					
					if(Gameobj_instances[i] ==null){
						
						Gameobj_instances.RemoveAt(i);
					}
					
					
				}
			}

			if(!fix_initial){
				//Registered_paint_positions.Clear();
				//Registered_paint_rotations.Clear();
			}
			/////////////////////////


			






			if(Registered_paint_positions!=null &1==1){
		for (int i=Registered_paint_positions.Count-1;i>=0 ;i--){
			if(Emitter_objects[i] == null)
			{
				
						Registered_paint_rotations.RemoveAt(i);
						Updated_Registered_paint_positions.RemoveAt(i);

				Registered_paint_positions.RemoveAt(i);


						DestroyImmediate(Gameobj_instances[i]);
						Gameobj_instances.RemoveAt(i);
						


				Registered_initial_positions.RemoveAt(i);
						Registered_initial_rotation.RemoveAt(i); Registered_initial_normal_rotation.RemoveAt(i);
				Registered_initial_scale.RemoveAt(i);
				Emitter_objects.RemoveAt(i);
				
				for(int k=Registered_enflamed_positions.Count-1;k>=0;k--){
					Vector2 ADD_ITEM1 = Registered_enflamed_positions[k];
					
					if((int)ADD_ITEM1.y-1 == i){
						
						Registered_enflamed_positions.RemoveAt(k);
					}
				}
			}
		}
		}

		
			
		if(!Use_stencil & Application.isPlaying){
			if(Input.GetMouseButtonDown(0))
			//if(  (cur.type == EventType.MouseDrag && cur.button == 0) | (Vector3.Distance(Keep_last_mouse_pos,cur.mousePosition)>8 & cur.type == EventType.MouseDrag && cur.button == 1)  ) 
			{

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


			
			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				
				if(hit.collider.gameObject.tag == "PPaint"){

					if(Emitter_objects!=null){
						
						if(!Erase_mode){
									if(Emitter_objects.Count > (p11.main.maxParticles/2)){//v2.1
								//do nothing
							}else{
								Emitter_objects.Add(hit.collider.gameObject);
								Registered_paint_positions.Add(hit.point);
										Updated_Registered_paint_positions.Add(hit.point);

										Registered_paint_rotations.Add(hit.normal);

								Registered_initial_positions.Add(hit.collider.gameObject.transform.position);
								Registered_initial_scale.Add (hit.collider.gameObject.transform.localScale);
								//Registered_initial_rotation.Add(hit.collider.gameObject.transform.eulerAngles);
										Registered_initial_rotation.Add(hit.collider.gameObject.transform.rotation);

										Registered_initial_normal_rotation.Add(hit.normal);


										if(Gameobj_instances.Count < (particle_count)){
											GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[Registered_paint_positions.Count-1],Quaternion.identity)as GameObject;
											
											if(TEMP.GetComponent<Collider>()!=null){
												if(Remove_colliders ){
													TEMP.GetComponent<Collider>().enabled=false;
												}else{TEMP.GetComponent<Collider>().enabled=true;}
											}
											
											Gameobj_instances.Add(TEMP);
											TEMP.transform.position = Registered_paint_positions[Registered_paint_positions.Count-1];
											
											if(Angled){
												
												TEMP.transform.localEulerAngles = Registered_paint_rotations[Registered_paint_positions.Count-1];
												
											}
											
											TEMP.transform.parent = Parent_OBJ.transform;
										}

										//Debug.Log ("Player add point");

							}
						}else if(Erase_mode){
							
							
							for (int i=0;i< Registered_paint_positions.Count;i++){
								
								if( Vector3.Distance(hit.point,Registered_paint_positions[i]) < (0.5f* brush_size))
								{
									Emitter_objects.RemoveAt(i);
									Registered_paint_positions.RemoveAt(i);
											Updated_Registered_paint_positions.RemoveAt(i);

											Registered_paint_rotations.RemoveAt(i);


											DestroyImmediate(Gameobj_instances[i]);
											Gameobj_instances.RemoveAt(i);



									Registered_initial_positions.RemoveAt(i);
											Registered_initial_rotation.RemoveAt(i); Registered_initial_normal_rotation.RemoveAt(i);
									Registered_initial_scale.RemoveAt(i);
									break;
								}
								
							}
							
						}
						
					}
				}
			}
		}
	}

			if(Use_stencil & Application.isPlaying & 1==1){
									
					
					int counter=0;
					
					for( int j=0; j<Stencil.width ;j=j+8){
						for( int k=0; k<Stencil.height ;k=k+8){
							
							
							Color tex_col = Stencil.GetPixel(j,k);
							
							if(1==0 | tex_col.a > 0.9f){
								
								
							Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition
							                             - new Vector3(tex_scale.x*Stencil.width/2,tex_scale.y*Stencil.height/2,0) 
							                             + new Vector3(tex_scale.x*j,tex_scale.y*k,0) 
							                             );
								
								RaycastHit hit = new RaycastHit();
								if (Physics.Raycast(ray, out hit, Mathf.Infinity))
									
								{
									if(hit.collider.gameObject.tag == "PPaint"){
										
										
										if(Emitter_objects!=null){
											
											if(!Erase_mode){
												
											if(Emitter_objects.Count > (p11.main.maxParticles/2)){//v2.1
													//do nothing
												}else{
													
													
													if( Input.GetMouseButtonDown(0)) // cur.type == EventType.MouseDown && cur.button == 1)
													{
														
														
														Emitter_objects.Add(hit.collider.gameObject);
														Registered_paint_positions.Add(hit.point);
														Updated_Registered_paint_positions.Add(hit.point);
														
														Registered_paint_rotations.Add(hit.normal);
														Registered_initial_normal_rotation.Add(hit.normal);
														
														Registered_initial_positions.Add(hit.collider.gameObject.transform.position);
														Registered_initial_scale.Add (hit.collider.gameObject.transform.localScale);
														Registered_initial_rotation.Add(hit.collider.gameObject.transform.rotation);
														
														
														
														
														
														if(Gameobj_instances.Count < (particle_count)){
															
															
															GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[Registered_paint_positions.Count-1],Quaternion.identity)as GameObject;
															
															
															
															if(TEMP.GetComponent<Collider>()!=null){
																if(Remove_colliders ){
																	TEMP.GetComponent<Collider>().enabled=false;
																}else{TEMP.GetComponent<Collider>().enabled=true;}
															}
															
															Gameobj_instances.Add(TEMP);
															TEMP.transform.position = Registered_paint_positions[Registered_paint_positions.Count-1];
															
															if(Angled){
																
																TEMP.transform.localEulerAngles = Registered_paint_rotations[Registered_paint_positions.Count-1];
																
															}
															
															if(Scale_by_texture){
																
																
																TEMP.transform.localScale = new Vector3(600*tex_col.b/255,600*tex_col.b/255,600*tex_col.b/255);
																
																
															}
															if(Color_by_texture){
																
																Renderer[] renderer = TEMP.GetComponentsInChildren< Renderer >();
																
																if(!Application.isPlaying){
																	renderer[0].sharedMaterial.color = Color.Lerp(renderer[0].sharedMaterial.color, tex_col,0.5f);
																	renderer[1].sharedMaterial.color = Color.Lerp(renderer[0].sharedMaterial.color, tex_col,0.5f);
																}else{
																	renderer[0].material.color = Color.Lerp(renderer[0].material.color, tex_col,Coloration_ammount);
																	renderer[1].material.color = Color.Lerp(renderer[0].material.color, tex_col,Coloration_ammount);
																	
																}
															}
															
															TEMP.transform.parent = Parent_OBJ.transform;
														}
														
													}
												}
											}
											else if(Erase_mode){
												
												
												for (int i=0;i< Updated_Registered_paint_positions.Count;i++){
													
													if( Vector3.Distance(hit.point,Updated_Registered_paint_positions[i]) < (0.5f* brush_size))
													{
														Emitter_objects.RemoveAt(i);
														Registered_paint_positions.RemoveAt(i);
														Updated_Registered_paint_positions.RemoveAt(i);
														
														Registered_paint_rotations.RemoveAt(i);
														
														
														DestroyImmediate(Gameobj_instances[i]);
														Gameobj_instances.RemoveAt(i);
																											
														
														Registered_initial_positions.RemoveAt(i);
														Registered_initial_rotation.RemoveAt(i); Registered_initial_normal_rotation.RemoveAt(i);
														Registered_initial_scale.RemoveAt(i);
														break;
													}
													
												}
												
											}
											
										}
									}
								}
								
							maxemitter_count = ((int)p11.main.maxParticles/2)+1;//v2.1
								if(Emitter_objects!=null){
									current_emitters_count = Emitter_objects.Count;
								}
								
							}
							counter=counter+1;
						}
						
					}

			}

			

			///// AAAA GAMEOBJECT MODE

			if(Preview_mode | Application.isPlaying){
				
				if(Registered_paint_positions!=null){
					
				
					if(1==1)
					{
						
						for(int i=0;i<Registered_paint_positions.Count;i++){
							

							if(1==1 & Gameobj_instances.Count < (Registered_paint_positions.Count)){

								//Undo.
								GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[i],Quaternion.identity)as GameObject;
								//Debug.Log ("added "+i);

								if(TEMP.GetComponent<Collider>()!=null){
									if(Remove_colliders ){
										TEMP.GetComponent<Collider>().enabled=false;
									}else{TEMP.GetComponent<Collider>().enabled=true;}
								}
								
								Gameobj_instances.Add(TEMP);
								TEMP.transform.position = Registered_paint_positions[i];
								
								if(Angled){
									
									TEMP.transform.localEulerAngles = Registered_paint_rotations[i];
									
								}
								
								TEMP.transform.parent = Parent_OBJ.transform;
							}
						}
					}
					

					
				}
				
				for(int i=0;i<Gameobj_instances.Count;i++){
					
					if(i < Registered_paint_positions.Count){

						
						if(Angled){
							
							
						
							//Quaternion rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Registered_paint_rotations[i]);
							
							//Quaternion	rot1 = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Gameobj_instances[i].transform.right);
							
							if(Asign_rot){
								
								
								
								if(Wind_speed>0 & Application.isPlaying){
									
									float timex = Time.time * Wind_speed + 0.1365143f * 10*i;
									
									
									Local_rot.y  =  noise.Noise(timex+10, timex+20, timex);
									
								}
								
						
								
							}
							
						
							
						}else{
							
						
						}
						
					}
					else
					{
						if(Registered_paint_positions.Count>0){
							
							
							
							if((i-Registered_paint_positions.Count) < Registered_paint_positions.Count){
								
								Gameobj_instances[i].transform.position = Registered_paint_positions[i-Registered_paint_positions.Count];
								
							}else{
								Gameobj_instances[i].transform.position = Registered_paint_positions[0];
							}
							
							
							if(Angled){
								
								
								Gameobj_instances[i].transform.rotation = Quaternion.identity;
								//Quaternion rot = Quaternion.identity;
								
								//if((i-Registered_paint_rotations.Count)<Registered_paint_rotations.Count){
								//	rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Registered_paint_rotations[i-Registered_paint_rotations.Count]);
								//}else{
								//	rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Registered_paint_rotations[0]);
								//}
								
								//Quaternion	rot1 = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Gameobj_instances[i].transform.right);
								
								if(Asign_rot){
									
									
									
									if(Wind_speed>0 & Application.isPlaying){
										
										float timex = Time.time * Wind_speed + 0.1365143f * 10*i;
										
										
										Local_rot.y  =  noise.Noise(timex+10, timex+20, timex);
										
									}
									
							
									
								}
								
							
								
							}else{
								
						
							}
						}
						
					}
				}
			}


			///// AAAA END GAMEOBEJCT MODE
			/// 
			/// 

			int counter_regsitered = 0;
			//if(Application.isPlaying)
			{

			

			if(Preview_mode | Application.isPlaying){
				if(Registered_paint_positions!=null & Registered_paint_positions.Count > 0 & !draw_in_sequence & !fix_initial){ 

					for(int i =0;i<Registered_paint_positions.Count;i++){

					Vector3 FIND_moved_toZERO = Registered_paint_positions[counter_regsitered] 
					+Emitter_objects[counter_regsitered].gameObject.transform.position 
						- Registered_initial_positions[counter_regsitered]
						-Emitter_objects[counter_regsitered].gameObject.transform.position ;

					Vector3 FIXED_ROT = Emitter_objects[counter_regsitered].gameObject.transform.eulerAngles;
					
					Quaternion relative = Quaternion.Euler(FIXED_ROT)*Quaternion.Inverse(Registered_initial_rotation[counter_regsitered]) ;
					Vector3 FIND_rotated = relative*(FIND_moved_toZERO);  //+ Emitter_objects[counter_regsitered].gameObject.transform.eulerAngles ;

					Vector3 FIND_scaled = new Vector3(FIND_rotated.x*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.x / Registered_initial_scale[counter_regsitered].x),
					                                  FIND_rotated.y*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.y / Registered_initial_scale[counter_regsitered].y),
					                                  FIND_rotated.z*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.z / Registered_initial_scale[counter_regsitered].z)  );
					
					Vector3 FIND_re_translated = FIND_scaled+Emitter_objects[counter_regsitered].gameObject.transform.position;
					Vector3 FIND_moved_pos = FIND_re_translated;
					
					Vector3 FIND_moved_normal_toZERO = Registered_initial_normal_rotation[counter_regsitered];//+(FIND_moved_pos - Emitter_objects[counter_regsitered].gameObject.transform.position);

					//Vector3 FIND_rotated_normal = relative*(FIND_moved_normal_toZERO);
													
						if(counter_regsitered < Gameobj_instances.Count & !follow_particles){
						
						Gameobj_instances[counter_regsitered].transform.position = FIND_moved_pos; //+(Emitter_objects[counter_regsitered].gameObject.transform.position - Registered_initial_positions[counter_regsitered]);
						
						Gameobj_instances[counter_regsitered].transform.rotation = relative*Quaternion.FromToRotation(Vector3.up,FIND_moved_normal_toZERO); 



							if(Angled){

								Quaternion rot1 = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Gameobj_instances[i].transform.right);
								
								if(Asign_rot){
									
									
									
									if(Wind_speed>0 & Application.isPlaying){
										
										float timex = Time.time * Wind_speed + 0.1365143f * 10*i;
										
										
										Local_rot.y  =  noise.Noise(timex+10, timex+20, timex);
										
									}
									
									Gameobj_instances[i].transform.localRotation *= rot1*new Quaternion(Local_rot.x,Local_rot.y,Local_rot.z,1);
									
								}
							}




							if(Grow_trees){ 

								if(Emitter_objects[counter_regsitered].gameObject.transform.localScale != Registered_initial_scale[counter_regsitered]){


									if ((Time.fixedTime-Current_Grow_time) < Grow_time ){
									

									Gameobj_instances[counter_regsitered].transform.localScale = new Vector3(Gameobj_instances[counter_regsitered].transform.localScale.x * (Emitter_objects[counter_regsitered].gameObject.transform.localScale.x/Registered_initial_scale[counter_regsitered].x),
										                                                                            Gameobj_instances[counter_regsitered].transform.localScale.y * (Emitter_objects[counter_regsitered].gameObject.transform.localScale.y/Registered_initial_scale[counter_regsitered].y),
										                                                                Gameobj_instances[counter_regsitered].transform.localScale.z * (Emitter_objects[counter_regsitered].gameObject.transform.localScale.z/Registered_initial_scale[counter_regsitered].z));
									}

								}

							}//END GROW TREES




					}
					
					Updated_Registered_paint_positions[counter_regsitered] = FIND_moved_pos;
													
					counter_regsitered=counter_regsitered+1;
					if(counter_regsitered > Registered_paint_positions.Count-1 ){
						counter_regsitered=0;
					}

				}
				}
			}

				
		if(1==0 | follow_particles){

				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);
				
					 counter_regsitered = 0;
					for (int i=0; i < ParticleList.Length;i++)
					{
						
					if(Registered_paint_positions!=null){
					if(Registered_paint_positions!=null & Registered_paint_positions.Count > 0 & draw_in_sequence){

						if(Emitter_objects[counter_regsitered].activeInHierarchy )
						{

						if(((counter_regsitered+1)*(ParticleList.Length/Registered_paint_positions.Count)) > i){


						}else{  
							if(counter_regsitered < Registered_paint_positions.Count-1 ){
								counter_regsitered= counter_regsitered+1;
							}else{counter_regsitered=0;}
						}
					}
				}

					if(Registered_paint_positions!=null & Registered_paint_positions.Count > 0 & !draw_in_sequence){ 

							

						
							ParticleList[i].startLifetime=16;
							
							if(ParticleList[i].remainingLifetime < 0.1f*ParticleList[i].startLifetime){
								ParticleList[i].remainingLifetime = 15;
							}




							float FIND_Y = ParticleList[i].position.y;
							Vector3 FIND_moved_pos1 = Updated_Registered_paint_positions[counter_regsitered];																
							FIND_Y = FIND_moved_pos1.y;


						if(!relaxed){
								if(Emitter_objects[counter_regsitered].activeInHierarchy){
							ParticleList[i].position  = new Vector3(FIND_moved_pos1.x,FIND_Y,FIND_moved_pos1.z) ; 
								}
						}

						if(relaxed){
								if(Emitter_objects[counter_regsitered].activeInHierarchy){
							if(ParticleList[i].remainingLifetime > 0.9f*ParticleList[i].startLifetime){
								ParticleList[i].position  = new Vector3(FIND_moved_pos1.x,FIND_Y,FIND_moved_pos1.z) ; 
							}
								}
						}

							if(relaxed & Gravity_Mode){

								ParticleList[i].position  = Vector3.Lerp (ParticleList[i].position,FIND_moved_pos1,0.5f*grav_factor );

							}

							if(Gameobj_instances.Count>0){
							Gameobj_instances[counter_regsitered].transform.position = ParticleList[i].position; //+(Emitter_objects[counter_regsitered].gameObject.transform.position - Registered_initial_positions[counter_regsitered]);
							}

						counter_regsitered=counter_regsitered+1;
						if(counter_regsitered > Registered_paint_positions.Count-1 ){
							counter_regsitered=0;
						}

					}
											

				}
					}
				p11.SetParticles(ParticleList,p11.particleCount);
				
		}
			}
	
	}
}

}