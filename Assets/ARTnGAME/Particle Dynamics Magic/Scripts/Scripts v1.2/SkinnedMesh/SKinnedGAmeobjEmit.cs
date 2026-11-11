using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
[System.Serializable()]
public class SKinnedGAmeobjEmit : MonoBehaviour {

	void OnEnable(){
		if(!Application.isPlaying){
			
			p2 = this.gameObject.GetComponent<ParticleSystem>();
		}
	}

	Vector3[] vertices ;
	public float Scale_factor=1f;
	Color32[] colorsA  ;

	public float Start_size=0.2f;
	
	Color[] Pcolors;
	
	
	ParticleSystem.Particle[] ParticleList;
	
	
	
	public GameObject emitter;

	[SerializeField,HideInInspector]
	public SkinnedMeshRenderer mesh ;

	[SerializeField,HideInInspector]
	public MeshFilter simple_mesh ;

	[SerializeField,HideInInspector]
	public Mesh animated_mesh;

	
		public float keep_in_position_factor =0.95f;
		public float keep_alive_factor =0.5f;
	

	public bool face_emit=false;

	void Start () {
		
		if(simple_mesh!=null & 1==1){
			

				if(1==1){
			if(Application.isPlaying){
				vertices = simple_mesh.mesh.vertices;
			}
			else{
				vertices = simple_mesh.sharedMesh.vertices;
			}


			
			
			if(!face_emit){
				
						if(p2.main.maxParticles!=vertices.Length/Every_other_vertex){
							ParticleSystem.MainModule main = p2.main;//v2.3
							main.maxParticles=vertices.Length/Every_other_vertex;//v2.3
						}
				
				p2.Emit(vertices.Length/Every_other_vertex);
			}
			if(face_emit){
				
				if(!let_loose){
					p2.Emit(p2.main.maxParticles);
				}

			}
				}
			
		}
		
		if(mesh!=null){

				if(animated_mesh!=null){
			mesh.BakeMesh(animated_mesh);
				

			vertices = animated_mesh.vertices;
			{
			
				
				if(!face_emit){

				if(p2.main.maxParticles!=vertices.Length/Every_other_vertex){
					ParticleSystem.MainModule main = p2.main;//v2.3
					main.maxParticles=vertices.Length/Every_other_vertex;
				}


				p2.Emit(vertices.Length/Every_other_vertex); 
				}
				if(face_emit){
					
					p2.Emit(p2.main.maxParticles); 
				}
				Debug.Log (p2.particleCount);
				
				
			}
				}
		}
		
	
		Particle_Num = particle_count;
		
		Registered_paint_positions = new List<Vector3>();
		Registered_paint_rotations = new List<Vector3>();
		
		noise = new PerlinPDM ();

		Gameobj_instances = new List<GameObject>();
		
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
			
			
			if(face_emit){
					if(!extend_life){
						//p2.Clear ();
					}
			}

			if(!face_emit){

						p2.Clear ();

			}

		}
		
			if(p2==null ){
				p2 = this.gameObject.GetComponent<ParticleSystem>();
			}

		if(p2 !=null){

			keep_max_particle_number = p2.main.maxParticles;

		}else{

				Debug.Log ("Please attach a gameobject with skinned mesh or meshfilter as emitter and attach script to a particle system");
		}

	}

	private int keep_max_particle_number;
	
	public bool Preview_mode=false;

	public int Every_other_vertex=1;

	public int particle_count = 100;
	
	bool got_positions=false;
	
	
	Vector3[] positions;
	Vector2[] rand_offsets;
	int[] tile;

	public ParticleSystem p2;
	
	
	
	private List<Vector3> Registered_paint_positions; 
	private List<Vector3> Registered_paint_rotations; 
	
	private List<GameObject> Gameobj_instances;
	
	public GameObject Gameobj;

	public float Y_offset=0f;
	
	public bool fix_initial = false;
	private bool let_loose = false;
	public bool letloose = false;
	private int place_start_pos;

	public bool extend_life=false;

	public bool Gravity_Mode=false;
	
	public GameObject Parent_OBJ;
	
	public bool Angled=false;
	
	private int Particle_Num;
	
	public bool Asign_rot=false;
	public Vector3 Local_rot = Vector3.zero;
	private PerlinPDM  noise;
	public float Wind_speed=1f;
	
	public float Return_speed=0.005f;
	
	public bool follow_particles=false;
	public bool Remove_colliders=false;

	void Update () {
		


			if(emitter ==null | p2 == null){


				//Debug.Log ("Please attach a gameobject with skinned mesh or meshfilter as emitter and attach script to a particle system"); 

				return;
			}

			if(Application.isPlaying){Preview_mode=false;}

			if(Preview_mode & !Application.isPlaying){
				
				if(Gameobj_instances!=null){
					
					if(Parent_OBJ.transform.childCount > Gameobj_instances.Count){
						
						for(int i=Parent_OBJ.transform.childCount-1;i>=0;i--){
							DestroyImmediate(Parent_OBJ.transform.GetChild(i).gameObject);
						}
						
					}
					
				}
				
			}



			
			if(mesh ==null & Application.isPlaying){
				mesh = emitter.GetComponent<SkinnedMeshRenderer>();
			}
			
			if(animated_mesh==null & Application.isPlaying){
				
				animated_mesh = new Mesh();
				
				if(mesh!=null){
					mesh.BakeMesh(animated_mesh);
				}
				
			}
			
			if(simple_mesh==null & Application.isPlaying){
				simple_mesh = emitter.GetComponent<MeshFilter>();
			}


		////////////////// SKINNED //////////////
		
		if(p2 != null){ 
			if(p2.main.startSize.constant < Start_size & Application.isPlaying){
					//p2.startSize = p2.startSize+(Start_size/3);//v2.3
					ParticleSystem.MainModule main = p2.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant =  p2.main.startSize.constant+(Start_size/3);//v2.3
					//main.startSizeMultiplier = p2.main.startSize.constant+(Start_size/3);//v2.3
			}else{
			
			}

			//reset if changed parricle max number
			if(p2.main.maxParticles != keep_max_particle_number){
				Start ();
				got_positions=false;
				positions=null;
				keep_max_particle_number=p2.main.maxParticles;
				Debug.Log ("adjusted");
			}

		}
		
		if(Every_other_vertex<=0){
			Every_other_vertex=1;
		}
		
		if(mesh!=null){
				if(animated_mesh!=null){
			mesh.BakeMesh(animated_mesh);
				}
		}
		///////////////// END SKINNED //////////////
		if(1==1){
		if(!Preview_mode & !Application.isPlaying){
			
				if(Gameobj_instances!=null){
					for(int i=Gameobj_instances.Count-1;i>=0;i--){
						
						DestroyImmediate(Gameobj_instances[i]);
					}
				}
			
		}

			if(Gameobj_instances!=null){
				for(int i=Gameobj_instances.Count-1;i>=0;i--){
										
					if(Gameobj_instances[i] ==null){
						
						Gameobj_instances.RemoveAt(i);
					}
					
				}
			}
		
		if(Particle_Num != particle_count){
			
			got_positions=false;
			Particle_Num = particle_count;
		}
			
		let_loose = letloose;
		if(!Application.isPlaying){ 
								
			//let_loose = false;
			
			if(noise ==null){
				noise = new PerlinPDM ();
			}
			
		}
			
			int tileCount=15;

			#region A
						
			if(simple_mesh!=null & 1==1){
				
				if(Application.isPlaying){
					vertices = simple_mesh.mesh.vertices;
				}
				else{
					vertices = simple_mesh.sharedMesh.vertices;
				}
				
				
				if(!face_emit){

					if(p2.main.maxParticles!=vertices.Length/Every_other_vertex){
							ParticleSystem.MainModule main = p2.main;//v2.3
							main.maxParticles=vertices.Length/Every_other_vertex;
					}


					p2.Emit(vertices.Length/Every_other_vertex);
				}
				if(face_emit){
					

					if(p2.particleCount<p2.main.maxParticles){

							p2.Emit(p2.main.maxParticles);
					}
				}
										
			}
			
			if(mesh!=null){

					if(animated_mesh!=null){
				mesh.BakeMesh(animated_mesh);
				vertices = animated_mesh.vertices;
				{
					
					if(!face_emit){

						if(p2.main.maxParticles!=vertices.Length/Every_other_vertex){
									ParticleSystem.MainModule main = p2.main;//v2.3
									main.maxParticles=vertices.Length/Every_other_vertex;
						}


						p2.Emit(vertices.Length/Every_other_vertex); 
					}
					if(face_emit){
					
								p2.Emit(p2.main.maxParticles); 
					}
										
				}
					}
			}
	
				if(vertices!=null){
			int DIVIDED_VERTEXES = vertices.Length/Every_other_vertex;

			if(face_emit){
				DIVIDED_VERTEXES = p2.main.maxParticles;
			}
						
			
				
				if(  p2.particleCount >= (DIVIDED_VERTEXES)){ 
					
					if(simple_mesh!=null){
					
						if(Application.isPlaying){
							vertices = simple_mesh.mesh.vertices;
						}else{vertices = simple_mesh.sharedMesh.vertices;}
										
						if(p2 != null){ 
							
							
							ParticleList = new ParticleSystem.Particle[p2.particleCount];
							p2.GetParticles(ParticleList);
							
							if(p2.particleCount >=(DIVIDED_VERTEXES)){
														
								int count_vertices=0;
								for (int i=0; i < ParticleList.Length;i++)
								{
									

										if(extend_life){
											if(tile!=null){
												if(i<tile.Length){
													ParticleList[i].remainingLifetime = tileCount + 1 - tile[i];
												}
											}
										}

										if(ParticleList[i].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
											ParticleList[i].startLifetime = tileCount;
										}

									if(!let_loose){
			
										if(!face_emit){
											ParticleList[i].position = vertices[count_vertices]*Scale_factor + new Vector3(0f,0f,0f)+ p2.transform.position;
										}else{

											if(positions!=null ){ 
												if(positions!=null & i<positions.Length){
											ParticleList[i].position = positions[i];
											}}
										}

									}
									if(let_loose){
										
										if(positions!=null){ 
											if(positions!=null & i<positions.Length){

													if(!extend_life & ParticleList[i].remainingLifetime > ParticleList[i].startLifetime*keep_in_position_factor){
														if(!face_emit){
															ParticleList[i].position = vertices[count_vertices]*Scale_factor + new Vector3(0f,0f,0f)+ p2.transform.position;
														}else{
															
															if(positions!=null ){ 
																if(positions!=null & i<positions.Length){
																	ParticleList[i].position = positions[i];
																}}
														}
												}
											
											
											

											//Gravity
											if(let_loose & Gravity_Mode){
												
												ParticleList[i].position = Vector3.Slerp(ParticleList[i].position, positions[i]+ new Vector3(i*0.005f,Y_offset,i*0.007f),Return_speed);
																									
												ParticleList[i].velocity= Vector3.Slerp(ParticleList[i].velocity,Vector3.zero,0.05f);
											}
										}}
										
									}
																		
		
									count_vertices=count_vertices+1;
									
									if(count_vertices>vertices.Length-1){
										count_vertices=0;
									}
								}
								
								p2.SetParticles(ParticleList,p2.particleCount);
							}
						}
												
					}
					
					if(mesh!=null & 1==1){
										
						if(p2 != null){ 
							
							if(p2.particleCount >=(DIVIDED_VERTEXES)){
								ParticleList = new ParticleSystem.Particle[p2.particleCount];
								p2.GetParticles(ParticleList);
																
								int count_vertices=0;
							
								for (int i=0; i < ParticleList.Length;i=i+1)
								{

									if(extend_life){
											if(tile!=null){
												if(i<tile.Length){
													ParticleList[i].remainingLifetime = tileCount + 1 - tile[i];
												}
											}
									}

										if(ParticleList[i].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
											ParticleList[i].startLifetime = tileCount;
										}

									
									if(!let_loose){
										
										ParticleList[i].position = emitter.transform.rotation*(vertices[count_vertices]*Scale_factor+new Vector3(0f,0f,0f))+ p2.transform.position;

									}

									if(let_loose){
											if(!extend_life & ParticleList[i].remainingLifetime > (ParticleList[i].startLifetime*keep_in_position_factor)){

											ParticleList[i].position = emitter.transform.rotation*(vertices[count_vertices]*Scale_factor+new Vector3(0f,0f,0f))+ p2.transform.position;

										}

										//Gravity
										if(let_loose & Gravity_Mode){
																						
											ParticleList[i].position = Vector3.Slerp(ParticleList[i].position, (emitter.transform.rotation*(vertices[count_vertices]*Scale_factor+new Vector3(0f,0f,0f))+ p2.transform.position),Return_speed);
																						
											ParticleList[i].velocity= Vector3.Slerp(ParticleList[i].velocity,Vector3.zero,0.05f);
										}

									}
									
									count_vertices=count_vertices+Every_other_vertex;
									
									if(count_vertices>vertices.Length-1){
										count_vertices=0;
									}
									
								}
								p2.SetParticles(ParticleList,p2.particleCount);
							}
						}
						
					}
										
				}}
							
#endregion
				if(ParticleList	!=null){	

			if(!got_positions | 1==0){
				positions = new Vector3[p2.particleCount];
				tile = new int[p2.particleCount];
				got_positions = true;
								
				for(int i=0;i<ParticleList.Length;i++){
					
					positions[i] = ParticleList[i].position;
					tile[i] = Random.Range(0,15);
				}
							
			}
			
		// PROJECTION
		

		
	
			
			
						
		if(!fix_initial){
			Registered_paint_positions.Clear();
			Registered_paint_rotations.Clear();
		}
		
		if(Registered_paint_positions!=null){
							
				for(int i=0;i<ParticleList.Length;i++){

					Registered_paint_positions.Add(ParticleList[i].position);
					Registered_paint_rotations.Add(Vector3.zero);
				}
							
		}
		
		if(Preview_mode | Application.isPlaying){
			
			if(Registered_paint_positions!=null){
				
				if(!follow_particles){
					
					for(int i=0;i<Registered_paint_positions.Count;i++){
											
						if(Gameobj_instances.Count < (particle_count)){
							GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[i],Quaternion.identity)as GameObject;
							
							Gameobj_instances.Add(TEMP);
							TEMP.transform.position = Registered_paint_positions[i];
							
							if(Angled){
								
								TEMP.transform.localEulerAngles = Registered_paint_rotations[i];
							}
							
							TEMP.transform.parent = Parent_OBJ.transform;
						}
					}
				}
				
				if(follow_particles){
					
						for(int i=0;i<ParticleList.Length-1;i=i+2){
						
						if(Gameobj_instances.Count < (particle_count/2) & Registered_paint_positions.Count > (i+2) ){
							GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[i],Quaternion.identity)as GameObject;
							
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
					
						Gameobj_instances[i].transform.position = Registered_paint_positions[i];

					if(Angled){
												
						Gameobj_instances[i].transform.rotation = Quaternion.identity;
						Quaternion rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Registered_paint_rotations[i]);
						
						Quaternion	rot1 = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Gameobj_instances[i].transform.right);
						
						if(Asign_rot){
							
							if(Wind_speed>0 & Application.isPlaying){
								
								float timex = Time.time * Wind_speed + 0.1365143f * 10*i;
															
								Local_rot.y  =  noise.Noise(timex+10, timex+20, timex);
								
							}

							Gameobj_instances[i].transform.localRotation *= rot1*new Quaternion(Local_rot.x,Local_rot.y,Local_rot.z,1);
							
						}
						
						Gameobj_instances[i].transform.rotation *= rot;
						
					}					
				
				}
			}
		}


			}

	}
	}//end update
}

}


