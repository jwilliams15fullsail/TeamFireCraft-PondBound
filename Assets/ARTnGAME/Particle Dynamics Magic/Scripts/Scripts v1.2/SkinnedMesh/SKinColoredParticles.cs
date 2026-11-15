using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Artngame.PDM;

namespace Artngame.PDM {
	
	[ExecuteInEditMode()]
	[System.Serializable()]
	public class SKinColoredParticles : MonoBehaviour {
		
		void OnEnable(){
			if(!Application.isPlaying){
				
				p11 = this.gameObject.GetComponent<ParticleSystem>();
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
		
		
		public bool Colored=true;

		
		
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

						if(p11.main.maxParticles!=(int)(vertices.Length/Every_other_vertex)){
							ParticleSystem.MainModule main = p11.main;//v2.3
							main.maxParticles=(int)(vertices.Length/Every_other_vertex);
						}

						
						p11.Emit((int)(vertices.Length/Every_other_vertex));
					}
					if(face_emit){
						
						if(!let_loose){
							p11.Emit(p11.main.maxParticles);
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

							if(p11.main.maxParticles!=(int)(vertices.Length/Every_other_vertex)){
								ParticleSystem.MainModule main = p11.main;//v2.3
								main.maxParticles=(int)(vertices.Length/Every_other_vertex);
							}

							p11.Emit((int)(vertices.Length/Every_other_vertex)); 
						}
						if(face_emit){
							
							p11.Emit(p11.main.maxParticles); 
						}
						Debug.Log (p11.particleCount);
						
						
					}
				}
			}
			
			
			Particle_Num = particle_count;
			
			Registered_paint_positions = new List<Vector3>();
			Registered_paint_rotations = new List<Vector3>();
			
			noise = new PerlinPDM ();
			

			
			if(Application.isPlaying){
				
			
				

				
				
				if(face_emit){
					if(!extend_life){
						//p11.Clear ();
					}
				}
				
				if(!face_emit){
					
					p11.Clear ();
					
				}
				
			}
			
			if(p11==null ){
				p11 = this.gameObject.GetComponent<ParticleSystem>();
			}
			
			if(p11 !=null){
				
				keep_max_particle_number = p11.main.maxParticles;
				
			}else{
				
				Debug.Log ("Please attach a gameobject with skinned mesh or meshfilter as emitter and attach script to a particle system");
			}
			
		}
		
		private int keep_max_particle_number;
		

		//[Range(0.1f,100)]
		public float Every_other_vertex=1;
		
		public int particle_count = 100;
		
		bool got_positions=false;
		
		
		Vector3[] positions;
		Vector2[] rand_offsets;
		int[] tile;
		
		public ParticleSystem p11;
		
		
		
		private List<Vector3> Registered_paint_positions; 
		private List<Vector3> Registered_paint_rotations; 
		
			

		
		public float Y_offset=0f;
		
		public bool fix_initial = false;
		private bool let_loose = false;
		public bool Let_loose = false;

		public float keep_in_position_factor =0.95f;
		public float keep_alive_factor =0.5f;

		private int place_start_pos;
		
		public bool extend_life=false;
		
		public bool Gravity_Mode=false;
		

		

		
		private int Particle_Num;
		


		private PerlinPDM  noise;

		
		public float Return_speed=0.005f;
		


		
		void Update () {
			
			
			
			if(emitter ==null | p11 == null){
				
				
				//Debug.Log ("Please attach a gameobject with skinned mesh or meshfilter as emitter and attach script to a particle system"); 
				
				return;
			}

			if(Every_other_vertex <0.05f){
				Every_other_vertex = 0.05f;
			}

			
			if(mesh ==null & Application.isPlaying){
				mesh = emitter.GetComponent<SkinnedMeshRenderer>();
			}

			//v1.2.2
			if(animated_mesh==null & !Application.isPlaying){
				animated_mesh = new Mesh();
				animated_mesh.hideFlags = HideFlags.HideAndDontSave;
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
			
			if(p11 != null){ 
				if(p11.main.startSize.constant < Start_size & Application.isPlaying){//if(p11.main.startSize < Start_size & Application.isPlaying){ //v2.3
					ParticleSystem.MainModule main = p11.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = main.startSize.constant +(Start_size/3); //v2.3
					//main.startSize = main.startSize+(Start_size/3);
				}else{
					
				}
				
				//reset if changed parricle max number
				if(p11.main.maxParticles != keep_max_particle_number){
					Start ();
					got_positions=false;
					positions=null;
					keep_max_particle_number=p11.main.maxParticles;
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

				
				if(Particle_Num != particle_count){
					
					got_positions=false;
					Particle_Num = particle_count;
				}
				
				let_loose = Let_loose;
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

						if(p11.main.maxParticles!=(int)(vertices.Length/Every_other_vertex)){
							ParticleSystem.MainModule main = p11.main;//v2.3
							main.maxParticles=(int)(vertices.Length/Every_other_vertex);
						}

						
						p11.Emit((int)(vertices.Length/Every_other_vertex));
					}
					if(face_emit){
						
						
						if(p11.particleCount<p11.main.maxParticles){
							
							p11.Emit(p11.main.maxParticles);
						}
					}
					
				}
				
				if(mesh!=null){
					
					if(animated_mesh!=null){
						mesh.BakeMesh(animated_mesh);
						vertices = animated_mesh.vertices;
						{
							
							if(!face_emit){

								if(p11.main.maxParticles!=(int)(vertices.Length/Every_other_vertex)){
									ParticleSystem.MainModule main = p11.main;//v2.3
									main.maxParticles=(int)(vertices.Length/Every_other_vertex);
								}

								p11.Emit((int)(vertices.Length/Every_other_vertex)); 
							}
							if(face_emit){
								
								p11.Emit(p11.main.maxParticles); 
							}
							
						}
					}
				}
				
				if(vertices!=null){
					int DIVIDED_VERTEXES = (int)(vertices.Length/Every_other_vertex);
					
					if(face_emit){
						DIVIDED_VERTEXES = p11.main.maxParticles;
					}
					
					
					
					if(  p11.particleCount >= (DIVIDED_VERTEXES)){ 
						
						if(simple_mesh!=null){
							
							if(Application.isPlaying){
								vertices = simple_mesh.mesh.vertices;
							}else{vertices = simple_mesh.sharedMesh.vertices;}






							int ParticlesNeeded = vertices.Length;
							
							if(p11.particleCount < ParticlesNeeded){
							}
							
							int Count_uvs =0;
							
							if(Application.isPlaying){
								
								Count_uvs = simple_mesh.mesh.uv2.Length;
							}else{
								
								Count_uvs = simple_mesh.sharedMesh.uv2.Length;
							}
							
							Vector2[] uvs    = new Vector2[Count_uvs];
							
							if(Application.isPlaying){
								
								uvs = simple_mesh.mesh.uv2;
							}else{
								
								uvs = simple_mesh.sharedMesh.uv2; 
							}
							
							colorsA = new Color32[ uvs.Length ]; 
							Vector4 offset1 = new Vector4(0,0,0,0);
							
							if(Application.isPlaying){
								offset1 = simple_mesh.gameObject.GetComponent<Renderer>().material.mainTextureOffset;
							}
							else{
								offset1 = simple_mesh.gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureOffset;
							}
							
							Texture2D pixels =  simple_mesh.gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
							
							int uvl = uvs.Length;
							
							if(this.transform.parent != null){

								if(this.transform.parent.GetComponent<Renderer>().sharedMaterial.mainTexture!=null){
									if(this.transform.parent.GetComponent<Renderer>().sharedMaterial.mainTexture.filterMode == FilterMode.Bilinear){
										for ( int j=0; j<uvl; j++) {
											
											Vector2 uv = uvs[ j ];
											
											colorsA[ j ] = pixels.GetPixelBilinear( ( uv.x)+offset1.x , ( uv.y)+offset1.y  );
											
										}
									}else{colorsA = pixels.GetPixels32();}
								}

							}else{
								Debug.Log ("Please attach the particle to the emitter mesh object");
							}



							if(p11 != null){ 
								
								
								ParticleList = new ParticleSystem.Particle[p11.particleCount];
								p11.GetParticles(ParticleList);
								
								if(p11.particleCount >=(DIVIDED_VERTEXES)){
									
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
												ParticleList[i].position = vertices[count_vertices]*Scale_factor + new Vector3(0f,0f,0f)+ p11.transform.position;
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
															ParticleList[i].position = vertices[count_vertices]*Scale_factor + new Vector3(0f,0f,0f)+ p11.transform.position;
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


										if(Colored){
											ParticleList[i].startColor = colorsA[count_vertices];
										}


										
										count_vertices=count_vertices+1;
										
										if(count_vertices>vertices.Length-1){
											count_vertices=0;
										}
									}
									
									p11.SetParticles(ParticleList,p11.particleCount);
								}
							}
							
						}
						
						if(mesh!=null & 1==1){


							Vector2[] uvs    =  animated_mesh.uv2;
							colorsA = new Color32[ uvs.Length ]; 
							
							Texture2D pixels = mesh.gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
							
							int uvl = uvs.Length;
							for ( int j=0; j<uvl; j++) {
								
								Vector2 uv = uvs[ j ];
								
								colorsA[ j ] = pixels.GetPixelBilinear( ( uv.x) , ( uv.y)  );
							}


							if(p11 != null){ 
								
								if(p11.particleCount >=(DIVIDED_VERTEXES)){
									ParticleList = new ParticleSystem.Particle[p11.particleCount];
									p11.GetParticles(ParticleList);
									
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
											
											ParticleList[i].position = emitter.transform.rotation*(vertices[count_vertices]*Scale_factor+new Vector3(0f,0f,0f))+ p11.transform.position;
											
										}
										
										if(let_loose){
											if(!extend_life & ParticleList[i].remainingLifetime > (ParticleList[i].startLifetime*keep_in_position_factor)){
												
												ParticleList[i].position = emitter.transform.rotation*(vertices[count_vertices]*Scale_factor+new Vector3(0f,0f,0f))+ p11.transform.position;
												
											}
											
											//Gravity
											if(let_loose & Gravity_Mode){
												
												ParticleList[i].position = Vector3.Slerp(ParticleList[i].position, (emitter.transform.rotation*(vertices[count_vertices]*Scale_factor+new Vector3(0f,0f,0f))+ p11.transform.position),Return_speed);
												
												ParticleList[i].velocity= Vector3.Slerp(ParticleList[i].velocity,Vector3.zero,0.05f);
											}
											
										}

										if(Colored){
											ParticleList[i].startColor = colorsA[count_vertices];
										}

										count_vertices=count_vertices+1;
										
										if(count_vertices>vertices.Length-1){
											count_vertices=0;
										}
										
									}
									p11.SetParticles(ParticleList,p11.particleCount);
								}
							}
							
						}
						
					}}
				
				#endregion
				if(ParticleList	!=null){	
					
					if(!got_positions | 1==0){
						positions = new Vector3[p11.particleCount];
						tile = new int[p11.particleCount];
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
					

					
					
				}
				
			}
		}//end update
	}
	
}


