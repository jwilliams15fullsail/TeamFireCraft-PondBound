using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

public class TransitionsMusicManagerPDM : MonoBehaviour {

		public bool use_audio = false;
		public bool use_GUI = true;
		public bool Loop_off=false;
		public bool Loop_ended=false;

		public bool Destroy_after_loop=false;
		public bool Disable_after_loop=false;
		public float Disable_Destroy_after = 1f;
		public float Start_count_down=-1;

	public bool trigger_ice_grow=false;
	public bool Auto_cycle_targets=false;
	public float Auto_cycle_time=3f;
	public float last_auto_cycle;
	public bool cycle_in_order=false;

	public GameObject ICE_SYSTEM;
	private ParticleSystem ICE_Particles;
	private SKinColoredMasked ICE_Script;
	private PlaceParticleOnSpline SPLINE_Script;
	private ParticleSheetProjection PROJECT_Script;

	public float grow_speed;

	public int chosen_target=0;//iterate targets

	public List<GameObject> Targets;
		
		//v1.3.5
		public bool use_timer=false;
		public List<float> Time_to_next;
		//v1.6
		public bool use_force_states=false;
		public List<AttractParticles> Attractors_per_step;
		//v1.6
		public bool per_step_type=false;
		public List<ParticleSystem> Type_per_step;
		public bool Randomize_all=false;//make forces and types random
		public int last_particle_type_id;

		//v1.7
		public bool per_step_mask=false;
		public List<Texture2D> Mask_per_step;
		public float size_growth_speed=0.35f;
		public float grav_return_speed=0.08f;
		public bool size_growth_speed_per_step=false;
		public bool grav_return_speed_per_step=false;
		public List<float> per_step_size_growth_speed;
		public List<float> per_step_grav_return_speed;
		public bool exp_grav_return_speed=false;

		public bool dist_based_speed=false;
		public bool dist_from_first=false;
		public bool dist_squared=false;
		public float dist_speed_grow_factor=1f;

		public bool let_loose_on_mesh_per_step=false;
		public List<int> per_step_let_loose_on_mesh;
		public bool keep_previous_per_step=false;
		public List<int> per_step_keep_previous;
		public bool change_time_per_step=false;
		public List<float> per_step_change_time;

		public bool remove_grav_per_step=false;
		public List<int> per_step_remove_grav;
		public bool reach_size_time_per_step=false;
		public List<float> per_step_reach_size_time;

		public bool inject_3d_particles=false;//check if gameobject particles painted and put current particle to follow


	public float max_particle_size;

	bool zeroed_scale = false;
	private float Start_cast_time;
	private float Effect_duration;


	void Start () {

		//v2.3 - fix issue where transitions will not work correctly when instantiated during game time
		last_auto_cycle = Time.fixedTime;

		chosen_target=1;

		ICE_Particles = ICE_SYSTEM.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
		ICE_Script = ICE_SYSTEM.GetComponent(typeof(SKinColoredMasked)) as SKinColoredMasked;
		SPLINE_Script = ICE_SYSTEM.GetComponent(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
		PROJECT_Script = ICE_SYSTEM.GetComponent(typeof(ParticleSheetProjection)) as ParticleSheetProjection;

		Max_size_previous = ICE_Particles.main.startSize.constant; //v2.3


		// AUDIO
		startScale = ICE_Script.Scale_factor;

		Start_angle = ICE_Script.emitter.transform.eulerAngles;
		Start_particle_audio_size = ICE_Script.Start_size;
		interpolation_time=Time.fixedTime;

			if(Attractor!=null){
				Local_attractor = Attractor.GetComponent(typeof(AttractParticles)) as AttractParticles;
				start_attractor_pos = Attractor.transform.position;
			}
			if(Attractor_planar!=null){
				Local_attractor_planar = Attractor_planar.GetComponent(typeof(AttractParticles)) as AttractParticles;

				start_planar_attractor_pos = Attractor_planar.transform.position;
			}		

		if(use_audio & Audio_source!=null){
			This_audio = Audio_source.GetComponent(typeof(AudioSource)) as AudioSource;
		}

			//v1.3.5
			//if times not filled, fill for missing targets
			if(Time_to_next==null){

				Time_to_next = new List<float>();
			}
			if(Time_to_next!=null){
				if(Time_to_next.Count < Targets.Count){
					for (int i=Time_to_next.Count;i<Targets.Count;i++){
						Time_to_next.Add (1); // 1sec defualt
					}
				}
			}

	}


	Vector3 Start_angle;
	float Start_particle_audio_size;

	AttractParticles Local_attractor;
	public GameObject Attractor;
	AttractParticles Local_attractor_planar;
	public GameObject Attractor_planar;
	private Vector3 start_planar_attractor_pos;
	private Vector3 start_attractor_pos;

	private Vector3 Particle_SOURCE;
	private Vector3 Particle_TARGET;

	string A11 = "Enable mask";

	public bool Coloration=false;
	public Color Start_color = Color.white;


	// AUDIO
	public int detail  = 500;

	public float amplitude = 0.1f;

	private float startScale;
	public GameObject Audio_source;
	private AudioSource This_audio;

	public float toggle_interpolation_time=0.2f;//toggle
	private float interpolation_time;

	public bool dancing_splines=false;
	public bool Switch_particle=false;//v.1.4
	public ParticleSystem Target_Particle;//v.1.4
	ParticleSystem.Particle[] ParticleList;//v1.4
	ParticleSystem.Particle[] ParticleList_TARGET;//v1.4
	public bool keep_previous=false;//v1.4
	public float Change_time=0;//v1.4
	float Change_current_time;//v1.4
	bool Started_change=false;//v1.4
	public bool Smooth_change=false;//v1.4
	public float Reach_size_speed=0.05f;//v1.4
		float Max_size_previous;//v1.4

		public bool Switch_on_transition=false;

		public bool let_loose_on_mesh=false;
		public bool Remove_gravity=false;
		public float return_to_mesh_speed=0.13f;


	void Update () {

		if(use_audio & Audio_source!=null){

			if(This_audio==null){
				This_audio = Audio_source.GetComponent(typeof(AudioSource)) as AudioSource;
			}
		}
		
			//v1.4
			if(Switch_particle){

				if(Target_Particle!=null){

					//v1.7
					if(change_time_per_step){
						if(per_step_change_time!=null){
							if(per_step_change_time.Count >= chosen_target){
								Change_time = per_step_change_time[chosen_target-1];
							}
						}
					}

					if(Change_time>0){

						if(Started_change){
							if(Time.fixedTime-Change_current_time < Change_time){

								Target_Particle.Emit(ICE_Script.p11.particleCount);
								ParticleList = new ParticleSystem.Particle[ICE_Script.p11.particleCount];
								ParticleList_TARGET = new ParticleSystem.Particle[ICE_Script.p11.particleCount];
								ICE_Script.p11.GetParticles(ParticleList);								
							
								if(ParticleList.Length >= ParticleList_TARGET.Length){
									int counted = 0;
									for(int i=0;i<ParticleList.Length;i++){
										
										ParticleList_TARGET[counted].angularVelocity = ParticleList[i].angularVelocity;
										ParticleList_TARGET[counted].axisOfRotation = ParticleList[i].axisOfRotation;
										ParticleList_TARGET[counted].startColor= ParticleList[i].startColor;
										ParticleList_TARGET[counted].remainingLifetime= ParticleList[i].remainingLifetime;
										ParticleList_TARGET[counted].position= ParticleList[i].position;
										ParticleList_TARGET[counted].randomSeed= ParticleList[i].randomSeed;
										ParticleList_TARGET[counted].rotation= ParticleList[i].rotation;

										if(Smooth_change){

											//v1.7
											if(reach_size_time_per_step){
												if(per_step_reach_size_time!=null){
													if(per_step_reach_size_time.Count >= chosen_target){
														Reach_size_speed = per_step_reach_size_time[chosen_target-1];
													}
												}
											}

											if(Target_Particle == ICE_Particles){
												if(ParticleList_TARGET[counted].startSize < Max_size_previous & ParticleList_TARGET[i].startSize < max_particle_size){
													ParticleList_TARGET[counted].startSize = ParticleList_TARGET[counted].startSize + Reach_size_speed*0.06f*Mathf.Abs(Mathf.Pow((Max_size_previous-ParticleList_TARGET[counted].startSize),1));
												}
											}
											else{
												if(ParticleList_TARGET[counted].startSize < Target_Particle.main.startSize.constant & ParticleList_TARGET[i].startSize < max_particle_size){
													ParticleList_TARGET[counted].startSize = ParticleList_TARGET[counted].startSize + Reach_size_speed*0.06f*Mathf.Abs(Mathf.Pow((Target_Particle.main.startSize.constant-ParticleList_TARGET[counted].startSize),1));
												}
											}

											ParticleList[i].startSize = ParticleList[i].startSize-Reach_size_speed*0.02f;

										}else{
											ParticleList_TARGET[counted].startSize= ParticleList[i].startSize;
										}

										ParticleList_TARGET[counted].startLifetime= ParticleList[i].startLifetime;
										ParticleList_TARGET[counted].velocity= ParticleList[i].velocity;
										if(counted < ParticleList_TARGET.Length){
											counted+=1;
										}
										else{
											counted=0;
										}
									}		
								}else{
									int counted = 0;
									for(int i=0;i<ParticleList_TARGET.Length;i++){
										
										ParticleList_TARGET[i].angularVelocity = ParticleList[counted].angularVelocity;
										ParticleList_TARGET[i].axisOfRotation = ParticleList[counted].axisOfRotation;
										ParticleList_TARGET[i].startColor= ParticleList[counted].startColor;
										ParticleList_TARGET[i].remainingLifetime= ParticleList[counted].remainingLifetime;
										ParticleList_TARGET[i].position= ParticleList[counted].position;
										ParticleList_TARGET[i].randomSeed= ParticleList[counted].randomSeed;
										ParticleList_TARGET[i].rotation= ParticleList[counted].rotation;

										if(Smooth_change){

											//v1.7
											if(reach_size_time_per_step){
												if(per_step_reach_size_time!=null){
													if(per_step_reach_size_time.Count >= chosen_target){
														Reach_size_speed = per_step_reach_size_time[chosen_target-1];
													}
												}
											}

											if(Target_Particle == ICE_Particles){
												if(ParticleList_TARGET[i].startSize < Max_size_previous & ParticleList_TARGET[i].startSize < max_particle_size){
													ParticleList_TARGET[i].startSize = ParticleList_TARGET[i].startSize + Reach_size_speed*0.06f*Mathf.Abs(Mathf.Pow((Max_size_previous-ParticleList_TARGET[i].startSize),1));
												}
												ParticleList[counted].startSize = ParticleList[counted].startSize-Reach_size_speed*0.02f;

											}else{
												if(ParticleList_TARGET[i].startSize < Target_Particle.main.startSize.constant & ParticleList_TARGET[i].startSize < max_particle_size){
													ParticleList_TARGET[i].startSize = ParticleList_TARGET[i].startSize +  Reach_size_speed*0.06f*Mathf.Abs(Mathf.Pow((Target_Particle.main.startSize.constant-ParticleList_TARGET[i].startSize),1));
												}
												ParticleList[counted].startSize = ParticleList[counted].startSize-Reach_size_speed*0.02f;
											}

										}else{
											ParticleList_TARGET[i].startSize= ParticleList[counted].startSize;
										}

										ParticleList_TARGET[i].startLifetime= ParticleList[counted].startLifetime;
										ParticleList_TARGET[i].velocity= ParticleList[counted].velocity;
										if(counted < ParticleList.Length){
											counted+=1;
										}
										else{
											counted=0;
										}
									}									
								}
								Target_Particle.SetParticles(ParticleList_TARGET,ICE_Script.p11.particleCount);	

								if(Smooth_change){
									ICE_Script.p11.SetParticles(ParticleList,ICE_Script.p11.particleCount);

								}
							}
							else{

								if(Smooth_change){

									ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
									ParticleSystem.MinMaxCurve Curve = main.startSize;
									//Curve.constant =  //v2.3

									if(Target_Particle == ICE_Particles){
										//ICE_Particles.startSize = Max_size_previous;
										Curve.constant = Max_size_previous;//v2.3
										ICE_Script.Start_size = Max_size_previous;
									}else{
										//ICE_Particles.startSize = Target_Particle.main.startSize.constant;
										Curve.constant = Target_Particle.main.startSize.constant;//v2.3
										ICE_Script.Start_size = Target_Particle.main.startSize.constant;
									}
								}

								Started_change=false;
								
								//v1.7
								bool previous_per_step_enter = true;
								if(keep_previous_per_step){
									if(per_step_keep_previous!=null){
										if(per_step_keep_previous.Count >= chosen_target){
											if(per_step_keep_previous[chosen_target-1]!=1){
												previous_per_step_enter= false;
											}									
										}
									}
								}

								if(!(keep_previous& previous_per_step_enter)){

									ICE_Script.p11.Clear();

									//v2.1
									ParticleSystem.EmissionModule em = ICE_Script.p11.emission;
									em.enabled = false;
									//ICE_Script.p11.enableEmission=false;

								}
								
								ICE_Script.p11 = Target_Particle;
								PROJECT_Script.p2 = Target_Particle;

								//v1.6
								if(!per_step_type){
									Target_Particle=SPLINE_Script.p2; //cycle previous here
								}else{
									//choose proper from list
									if(Type_per_step!=null){
										if(Type_per_step.Count >= chosen_target){
											if(Randomize_all){
												int ID = Random.Range(0,Type_per_step.Count);
												if(ID != last_particle_type_id & Type_per_step[ID].gameObject != Type_per_step[last_particle_type_id].gameObject){
													Target_Particle = Type_per_step[ID];
													last_particle_type_id =ID;
												}else{
													Target_Particle = Type_per_step[chosen_target-1];
													last_particle_type_id =chosen_target-1;
												}
											}else{
												Target_Particle = Type_per_step[chosen_target-1];
											}
										}
									}
								}
								if(inject_3d_particles){
									ParticlePropagationPDM Propagator = this.gameObject.GetComponent(typeof(ParticlePropagationPDM)) as ParticlePropagationPDM;
									if(Propagator!=null){
										Propagator.p11 =  ICE_Script.p11;
									}
								}


								SPLINE_Script.p2= ICE_Script.p11;								

								Switch_particle=false;


							}
						}else
						
						if(!Started_change){

							Started_change=true;
							Change_current_time = Time.fixedTime;

								//v2.1
								ParticleSystem.EmissionModule em = Target_Particle.emission;
								em.enabled = true;
							//Target_Particle.enableEmission=true;

							
							ParticleList = new ParticleSystem.Particle[ICE_Script.p11.particleCount];
							ParticleList_TARGET = new ParticleSystem.Particle[ICE_Script.p11.particleCount];
							ICE_Script.p11.GetParticles(ParticleList);

							if(ParticleList.Length >= ParticleList_TARGET.Length){
								int counted = 0;
								for(int i=0;i<ParticleList.Length;i++){
									
									ParticleList_TARGET[counted].angularVelocity = ParticleList[i].angularVelocity;
									ParticleList_TARGET[counted].axisOfRotation = ParticleList[i].axisOfRotation;
										ParticleList_TARGET[counted].startColor= ParticleList[i].startColor;
									ParticleList_TARGET[counted].remainingLifetime= ParticleList[i].remainingLifetime;
									ParticleList_TARGET[counted].position= ParticleList[i].position;
									ParticleList_TARGET[counted].randomSeed= ParticleList[i].randomSeed;
									ParticleList_TARGET[counted].rotation= ParticleList[i].rotation;

									if(Smooth_change){
											ParticleList_TARGET[counted].startSize= 0;

									}else{
											ParticleList_TARGET[counted].startSize= ParticleList[i].startSize;
									}

									ParticleList_TARGET[counted].startLifetime= ParticleList[i].startLifetime;
									ParticleList_TARGET[counted].velocity= ParticleList[i].velocity;
									if(counted < ParticleList_TARGET.Length){
										counted+=1;
									}
									else{
										counted=0;
									}
								}		
							}else{
								int counted = 0;
								for(int i=0;i<ParticleList_TARGET.Length;i++){
									
									ParticleList_TARGET[i].angularVelocity = ParticleList[counted].angularVelocity;
									ParticleList_TARGET[i].axisOfRotation = ParticleList[counted].axisOfRotation;
										ParticleList_TARGET[i].startColor= ParticleList[counted].startColor;
									ParticleList_TARGET[i].remainingLifetime= ParticleList[counted].remainingLifetime;
									ParticleList_TARGET[i].position= ParticleList[counted].position;
									ParticleList_TARGET[i].randomSeed= ParticleList[counted].randomSeed;
									ParticleList_TARGET[i].rotation= ParticleList[counted].rotation;

									if(Smooth_change){
											ParticleList_TARGET[i].startSize= 0;

									}
									else{
											ParticleList_TARGET[i].startSize= ParticleList[counted].startSize;
									}

									ParticleList_TARGET[i].startLifetime= ParticleList[counted].startLifetime;
									ParticleList_TARGET[i].velocity= ParticleList[counted].velocity;
									if(counted < ParticleList.Length){
										counted+=1;
									}
									else{
										counted=0;
									}
								}
								
							}											

							if(Smooth_change){
								Target_Particle.SetParticles(ParticleList_TARGET,ICE_Script.p11.particleCount);
							}

						}



					}


					else{
						Target_Particle.Emit(ICE_Script.p11.particleCount);
						ParticleList = new ParticleSystem.Particle[ICE_Script.p11.particleCount];
						ParticleList_TARGET = new ParticleSystem.Particle[ICE_Script.p11.particleCount];
						ICE_Script.p11.GetParticles(ParticleList);

						//ParticleSystem Source_Particle = ICE_Script.p11;

						for(int i=0;i<ParticleList.Length;i++){

							ParticleList_TARGET[i].angularVelocity = ParticleList[i].angularVelocity;
							ParticleList_TARGET[i].axisOfRotation = ParticleList[i].axisOfRotation;
							ParticleList_TARGET[i].startColor= ParticleList[i].startColor;
							ParticleList_TARGET[i].remainingLifetime= ParticleList[i].remainingLifetime;
							ParticleList_TARGET[i].position= ParticleList[i].position;
							ParticleList_TARGET[i].randomSeed= ParticleList[i].randomSeed;
							ParticleList_TARGET[i].rotation= ParticleList[i].rotation;
							ParticleList_TARGET[i].startSize= ParticleList[i].startSize;
							ParticleList_TARGET[i].startLifetime= ParticleList[i].startLifetime;
							ParticleList_TARGET[i].velocity= ParticleList[i].velocity;
						}

						/////


						Target_Particle.SetParticles(ParticleList,ICE_Script.p11.particleCount);

						//v1.7
						bool previous_per_step_enter = true;
						if(keep_previous_per_step){
							if(per_step_keep_previous!=null){
								if(per_step_keep_previous.Count >= chosen_target){
									if(per_step_keep_previous[chosen_target-1]!=1){
										previous_per_step_enter= false;
									}									
								}
							}
						}

						if(!(keep_previous & previous_per_step_enter)){
							ICE_Script.p11.Clear();

							//v2.1
							ParticleSystem.EmissionModule em = ICE_Script.p11.emission;
							em.enabled = false;
							//ICE_Script.p11.enableEmission=false;
						}

						ICE_Script.p11 = Target_Particle;
						PROJECT_Script.p2 = Target_Particle;
						//SPLINE_Script.p2=Target_Particle;

						//v1.6
						if(!per_step_type){
							Target_Particle=SPLINE_Script.p2; //cycle previous here
						}else{
							//choose proper from list
							if(Type_per_step!=null){
								if(Type_per_step.Count >= chosen_target){
									if(Randomize_all){
										int ID = Random.Range(0,Type_per_step.Count);
										if(ID != last_particle_type_id & Type_per_step[ID].gameObject != Type_per_step[last_particle_type_id].gameObject){
											Target_Particle = Type_per_step[ID];
											last_particle_type_id =ID;
										}else{
											Target_Particle = Type_per_step[chosen_target-1];
											last_particle_type_id =chosen_target-1;
										}
									}else{
										Target_Particle = Type_per_step[chosen_target-1];
									}
								}
							}
						}
						if(inject_3d_particles){
							ParticlePropagationPDM Propagator = this.gameObject.GetComponent(typeof(ParticlePropagationPDM)) as ParticlePropagationPDM;
							if(Propagator!=null){
								Propagator.p11 =  ICE_Script.p11;
							}
						}

						SPLINE_Script.p2= ICE_Script.p11;

						//this.Target_Particle = 

						//ParticleSystem Target_Particle = this.particleSystem;
						Switch_particle=false;
					}
				}
			}


			//v1.3.5
			//if times not filled, fill for missing targets
			if(Time_to_next==null){
				
				Time_to_next = new List<float>();
			}
			if(Time_to_next!=null){
				if(Time_to_next.Count < Targets.Count){
					for (int i=Time_to_next.Count;i<Targets.Count;i++){
						Time_to_next.Add(1); // 1sec defualt
					}
				}
			}


		//AUTO CYCLE

		if(!Auto_cycle_targets){

			last_auto_cycle = Time.fixedTime;

				if(Start_count_down == -1){
					Start_count_down = Time.fixedTime;
				}

				if(Destroy_after_loop){
					if(Time.fixedTime - Start_count_down > Disable_Destroy_after){
						Destroy(this.transform.root.gameObject);
					}
				}
				if(Disable_after_loop){
					if(Time.fixedTime - Start_count_down > Disable_Destroy_after){
						this.transform.root.gameObject.SetActive(false);
					}
				}
		}
		else
		{
				if ( ((Time.fixedTime - last_auto_cycle > Auto_cycle_time) & !use_timer) | ((Time.fixedTime - last_auto_cycle > Time_to_next[chosen_target-1]) & use_timer) )
				{

				int redefine_end = Targets.Count;


				//Random.seed = Random.Range (10,10000);
					Random.InitState(Random.Range (10,10000));//v2.3
				int target_random = Random.Range(1,redefine_end);



				Particle_SOURCE = Targets[chosen_target-1].transform.position;
				


				if(!cycle_in_order){


					if(chosen_target==4){
						chosen_target=3;

						int Chance = Random.Range(100,200);
						if(Chance == 150){chosen_target=2;}
					}
					else if(target_random == chosen_target){
											
						chosen_target=chosen_target+1;
						if(chosen_target > Targets.Count){

							chosen_target=chosen_target-1;
						}
					}else{chosen_target=target_random;}

				}else{

					chosen_target=chosen_target+1;
					if(chosen_target > Targets.Count){
						chosen_target=1;

							if(Loop_off){
								Auto_cycle_targets = false;

								chosen_target = Targets.Count;

								Loop_ended = true;

								//Type_per_step[Type_per_step.Count-1].Stop();
								Type_per_step[Type_per_step.Count-1].GetComponent<Renderer>().enabled=false;

//								if(Destroy_after_loop){
//									Destroy(this.transform.root.gameObject);
//								}
//								if(Disable_after_loop){
//									this.transform.root.gameObject.SetActive(false);
//								}
							}else{
								chosen_target=1;
							}

					}
				}
				
				Particle_TARGET = Targets[chosen_target-1].transform.position;
				
				Transitions();

				last_auto_cycle = Time.fixedTime;


					if(Switch_on_transition){

//						//v1.6
//						if(per_step_type){
//							//choose proper from list
//							if(Type_per_step!=null){
//								if(Type_per_step.Count >= chosen_target){
//									if(Randomize_all){
//										Target_Particle = Type_per_step[Random.Range(0,Type_per_step.Count)];
//									}else{
//										Target_Particle = Type_per_step[chosen_target-1];
//									}
//								}
//							}
//						}

						Switch_particle=true;
					}
					//v1.6
					if(use_force_states){

						if(Attractors_per_step.Count >= chosen_target){
							
							//disable all but the needed one
							for(int i = 0 ; i <Attractors_per_step.Count;i++){
								Attractors_per_step[i].enabled=false;
							}
							if(Randomize_all){
								Attractors_per_step[Random.Range(0,Type_per_step.Count)].enabled=true;
							}else{
								Attractors_per_step[chosen_target-1].enabled=true;
							}
							
						}

					}

			}

		}

		//AUDIO
	if(use_audio & Audio_source!=null){

		float[] info  = new float[detail];
		AudioListener.GetOutputData(info, 0);
		float packagedData  = 0.0f;
		
		for(int x = 0; x < info.Length; x++)
		{
			packagedData += System.Math.Abs(info[x]);  
		}		

		ICE_Script.Scale_factor = Mathf.Lerp(ICE_Script.Scale_factor, (packagedData * amplitude) + startScale,16f*Time.deltaTime);

		ICE_Script.Start_size = 0.4f*(packagedData * amplitude) + Start_particle_audio_size -1.0f;		

		if( Time.fixedTime - interpolation_time > toggle_interpolation_time){
			SPLINE_Script.Interpolate_steps = Random.Range(1+(int)(packagedData * amplitude)-5,20)*(int)This_audio.volume ;

			interpolation_time = Time.fixedTime;
		}
		ICE_Script.emitter.transform.eulerAngles = Vector3.Slerp(ICE_Script.emitter.transform.eulerAngles,  new Vector3(Start_angle.x,20.1f*(packagedData * amplitude)+Start_angle.y,Start_angle.z),8.5f*Time.deltaTime);
						
		//CONTROL TURBULENCE
				if(Local_attractor!=null){
		Local_attractor.Bounce_factor = Random.Range(1+2*(int)(packagedData * amplitude)-8+5,40+200)*This_audio.volume;
				}

		float Random_Y_motion = Random.Range(-5,1);//use for planar
		float Lerp_factor=0.5f;
		
		Random_Y_motion = Random.Range(-8+2*(int)(packagedData * amplitude),9)*This_audio.volume;//use for rainbow
		Lerp_factor=11.2f*Time.deltaTime;

				if(Attractor_planar!=null){
		Attractor_planar.transform.position = Vector3.Lerp(Attractor_planar.transform.position, new Vector3(start_planar_attractor_pos.x, start_planar_attractor_pos.y+Random_Y_motion,start_planar_attractor_pos.z),Lerp_factor);
				}
				if(Attractor!=null){
		Attractor.transform.position = Vector3.Lerp(Attractor.transform.position, new Vector3(start_attractor_pos.x, start_attractor_pos.y+Random_Y_motion,start_attractor_pos.z),Lerp_factor);
				}
				if(Local_attractor_planar!=null){
		Local_attractor_planar.Vortex_angularvelocity = This_audio.volume;
				}		

		//DANCING SPLINES

			if(dancing_splines){


				SplinerP TEST_SPLINE = Targets[chosen_target-1].GetComponent(typeof(SplinerP)) as SplinerP;

				float spline_motion_factor = 0.01f;
				float freq = 0.4f;

				if(TEST_SPLINE!=null){

					for(int i = 0;i<TEST_SPLINE.SplinePoints.Count;i=i+2){


						TEST_SPLINE.control_points_children[i].transform.position = Vector3.Lerp(
							TEST_SPLINE.control_points_children[i].transform.position , 
							new Vector3(
							TEST_SPLINE.control_points_children[i].transform.position.x+1.3f*spline_motion_factor*Random_Y_motion*Mathf.Sin(freq*Time.fixedTime), 
							TEST_SPLINE.control_points_children[i].transform.position.y+spline_motion_factor*Random_Y_motion*Mathf.Sin(freq*Time.fixedTime),
							TEST_SPLINE.control_points_children[i].transform.position.z+2.0f*spline_motion_factor*Random_Y_motion*Mathf.Sin(freq*Time.fixedTime)),Lerp_factor
							);
					}
				}
			}
	}

		if(move_to_target==3){ //move to spline
			
			if(SPLINE_Script.Return_Speed<0.09f){
				SPLINE_Script.Return_Speed += Time.deltaTime*0.05f;

				if(Coloration){
					Colorize();
				}

			}else{
				move_to_target=0;
				SPLINE_Script.extend_life=false;
				SPLINE_Script.Gravity=false;
			}
		}

		if(move_to_target==4){ //move to hero			

			if(ICE_Script.Return_speed<return_to_mesh_speed){

				if(!grav_return_speed_per_step){
					ICE_Script.Return_speed += Time.deltaTime*grav_return_speed;//0.08f //v1.7
				}else{
					if(per_step_grav_return_speed!=null){
							if(per_step_grav_return_speed.Count >= chosen_target){

								float Dist_based_increase=0f;
								if(dist_based_speed){

									if(!dist_from_first & chosen_target > 1){
										if(!dist_squared){
											Dist_based_increase = dist_speed_grow_factor * (Targets[chosen_target-1].gameObject.transform.position-Targets[chosen_target-2].gameObject.transform.position).magnitude;
										}else{
											Dist_based_increase = dist_speed_grow_factor * (Targets[chosen_target-1].gameObject.transform.position-Targets[chosen_target-2].gameObject.transform.position).sqrMagnitude;
										}
									}
									if(dist_from_first){
										if(!dist_squared){
											Dist_based_increase = dist_speed_grow_factor * (Targets[chosen_target-1].gameObject.transform.position-Targets[0].gameObject.transform.position).magnitude;
										}else{
											Dist_based_increase = dist_speed_grow_factor * (Targets[chosen_target-1].gameObject.transform.position-Targets[0].gameObject.transform.position).sqrMagnitude;
										}
									}
								}


								if(exp_grav_return_speed){
									//per_step_grav_return_speed[chosen_target-1] += per_step_grav_return_speed[chosen_target-1];
									ICE_Script.Return_speed = Mathf.Lerp(ICE_Script.Return_speed, ICE_Script.Return_speed + Dist_based_increase + Time.deltaTime*per_step_grav_return_speed[chosen_target-1]*(ICE_Script.Return_speed+per_step_grav_return_speed[chosen_target-1]),0.5f);
								}
								else{
									ICE_Script.Return_speed += Time.deltaTime*per_step_grav_return_speed[chosen_target-1] + Dist_based_increase;//0.35f //v1.7
								}

						}
					}
				}

				if(Coloration){
					Colorize();
				}

				if(ICE_Script.Start_size>max_particle_size){

					if(!size_growth_speed_per_step){
						ICE_Script.Start_size -= Time.deltaTime*size_growth_speed;//0.35f //v1.7
					}else{
						if(per_step_size_growth_speed!=null){
								if(per_step_size_growth_speed.Count >= chosen_target){
									ICE_Script.Start_size -= Time.deltaTime*per_step_size_growth_speed[chosen_target-1];//0.35f //v1.7									
							}
						}
					}
				}
				ICE_Particles.gameObject.transform.position = Vector3.Slerp(Particle_SOURCE, Particle_TARGET,Time.deltaTime*0.15f);
			}else{
				move_to_target=0;

					//v1.7
					bool per_step_choice_to_mesh=true;
					if(let_loose_on_mesh_per_step){
						if(per_step_let_loose_on_mesh!=null){
							if(per_step_let_loose_on_mesh.Count >= chosen_target){
								if(per_step_let_loose_on_mesh[chosen_target-1]!=1){
									per_step_choice_to_mesh = false;
								}									
							}
						}
					}

					if(let_loose_on_mesh & per_step_choice_to_mesh){

						//v1.7
						bool per_step_remove_gravity=true;
						if(remove_grav_per_step){
							if(per_step_remove_grav!=null){
								if(per_step_remove_grav.Count >= chosen_target){
									if(per_step_remove_grav[chosen_target-1]!=1){
										per_step_remove_gravity= false;
									}									
								}
							}
						}
						
						if( Remove_gravity & per_step_remove_gravity){
						//if(Remove_gravity){
							ICE_Script.Gravity_Mode=false;
							ICE_Script.extend_life=false;
							ICE_Script.Let_loose=true;
							ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
							main.gravityModifier=-0.05f;
							ICE_Script.keep_in_position_factor=0.95f;
						}else{
							ICE_Script.Let_loose=true;
						}
					}else{

						//v1.7
						bool per_step_remove_gravity=true;
						if(remove_grav_per_step){
							if(per_step_remove_grav!=null){
								if(per_step_remove_grav.Count >= chosen_target){
									if(per_step_remove_grav[chosen_target-1]!=1){
										per_step_remove_gravity= false;
									}									
								}
							}
						}
						
						if( Remove_gravity & per_step_remove_gravity){
						//if(Remove_gravity){
							ICE_Script.Gravity_Mode=true;
							ICE_Script.extend_life=true;
							ICE_Script.Let_loose=false;
							ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
							main.gravityModifier=0.0f;
							ICE_Script.keep_in_position_factor=1f;
						}else{
							ICE_Script.Let_loose=false;
						}

//						if(!Remove_gravity){
//							ICE_Script.Let_loose=false;
//						}else{
//							ICE_Script.Gravity_Mode=true;
//							ICE_Script.extend_life=true;
//							ICE_Script.Let_loose=false;
//							ICE_Particles.gravityModifier=0.0f;
//							ICE_Script.keep_in_position_factor=1f;
//						}
					}
			}
		}

		if(move_to_target==2){ //move to spline

			if(SPLINE_Script.Return_Speed<0.11f){
				SPLINE_Script.Return_Speed += Time.deltaTime*0.05f;

				if(Coloration){
					Colorize();
				}

			}else{
				move_to_target=0;
				SPLINE_Script.extend_life=true;

				if(Targets[chosen_target-1].name =="SPLINE 1 star"){
					
					SPLINE_Script.Interpolate = true;
					SPLINE_Script.Interpolate_steps = 6;
					SPLINE_Script.Gravity=false;
					SPLINE_Script.relaxed=false;

					
				}else{
					SPLINE_Script.Interpolate = true;//audio
					SPLINE_Script.Interpolate_steps = 2;

					SPLINE_Script.Gravity=true;
					SPLINE_Script.relaxed=true;

				}

			}		

		}

		if(move_to_target==1){ //move to spline
			
			if(SPLINE_Script.Return_Speed>0.09f){
				SPLINE_Script.Return_Speed -= Time.deltaTime*0.05f;

				if(Coloration){
					Colorize();
				}

			}else{
				move_to_target=0;
				SPLINE_Script.extend_life=false;
			}
		}

		//SPELL1
		if(ICE_Particles==null){
			ICE_Particles = ICE_SYSTEM.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
		}
		if(ICE_Script==null){
			ICE_Script = ICE_SYSTEM.GetComponent(typeof(SKinColoredMasked)) as SKinColoredMasked;
		}
		if(SPLINE_Script==null){
			SPLINE_Script = ICE_SYSTEM.GetComponent(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
		}


		if(trigger_ice_grow){
		if(!zeroed_scale){
			
			ICE_Script.Start_size =0;
			//ICE_Particles.main.startSize.constant=0;
					ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = 0;
			zeroed_scale=true;
		}else{
			
			if(trigger_ice_grow){
				if(ICE_Script.Start_size < max_particle_size){
					ICE_Script.Start_size += Time.deltaTime*Time.deltaTime*(grow_speed);
				}
			}
			
		}
		}
	}

	float TEMP_SIZE;
	int TEMP_STEPS;
	private int move_to_target=0;

	void OnGUI(){
			if(use_GUI){

		if(!trigger_ice_grow){
			GUI.TextField(new Rect(0+10,40,160,20),"Particle size(Mesh)");
					TEMP_SIZE = GUI.HorizontalSlider(new Rect(0+10,60,150,20),ICE_Script.Start_size,0.05f,ICE_Particles.main.startSize.constant);

					ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					//Curve.constant =  //v2.3

					ICE_Script.Start_size = TEMP_SIZE;


					//ICE_Particles.startSize = TEMP_SIZE;
					Curve.constant = TEMP_SIZE; //v2.3


			if(Targets[chosen_target-1].name =="SPLINE 1 star"){

				GUI.TextField(new Rect(0+10,80,160,20),"Interpolation steps(Spline)");
				TEMP_STEPS = (int)GUI.HorizontalSlider(new Rect(0+10,100,150,20),SPLINE_Script.Interpolate_steps,6,9);
				SPLINE_Script.Interpolate_steps = TEMP_STEPS;
				
			}
		}

		if( GUI.Button(new Rect(0+10,120,150,20),"Toggle relaxed (spline)")  ){
			
			if(!SPLINE_Script.Gravity | !SPLINE_Script.relaxed){
				SPLINE_Script.Gravity = true;
				SPLINE_Script.relaxed = true;
				SPLINE_Script.extend_life = true;
				SPLINE_Script.hold_emission = false;
						ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
						main.gravityModifier=0.0f;
				SPLINE_Script.keep_in_position_factor = 1;
			}else if(SPLINE_Script.Gravity | !SPLINE_Script.relaxed){
				SPLINE_Script.Gravity = false;
				SPLINE_Script.relaxed = false;
				SPLINE_Script.extend_life = true;
				SPLINE_Script.hold_emission = false;
				SPLINE_Script.keep_in_position_factor = 1;
			}
		}

		if( GUI.Button(new Rect(150+10,120,150,20),"Toggle free (spline)")  ){
			
			if(SPLINE_Script.Gravity | !SPLINE_Script.relaxed){
				SPLINE_Script.Gravity = false;
				SPLINE_Script.relaxed = true;
				SPLINE_Script.extend_life = false;
				SPLINE_Script.keep_in_position_factor = 0.93f;
				SPLINE_Script.hold_emission = true;
						ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
						main.gravityModifier=-0.01f;

			}else if(!SPLINE_Script.Gravity | SPLINE_Script.relaxed){
				SPLINE_Script.Gravity = true;
				SPLINE_Script.relaxed = false;
				SPLINE_Script.extend_life = true;
				SPLINE_Script.keep_in_position_factor = 1;
				SPLINE_Script.hold_emission = false;
						ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
						main.gravityModifier=0.0f;
			}
		}

		if( GUI.Button(new Rect(0+10,140,150,20),"Toggle colored")  ){

			if(ICE_Script.Colored){
				ICE_Script.Colored = false;
			}else if(!ICE_Script.Colored){
				ICE_Script.Colored = true;
			}
		
		}
		if( GUI.Button(new Rect(150+10,140,150,20),"Toggle free (Mesh)")  ){
			
			if(ICE_Script.Gravity_Mode | ICE_Script.extend_life){

				ICE_Script.Gravity_Mode=false;
				ICE_Script.extend_life=false;
				ICE_Script.Let_loose=true;
						ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
						main.gravityModifier=-0.05f;
				ICE_Script.keep_in_position_factor=0.95f;

			}else {

				ICE_Script.Gravity_Mode=true;
				ICE_Script.extend_life=true;
				ICE_Script.Let_loose=false;
						ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
						main.gravityModifier=0.0f;
				ICE_Script.keep_in_position_factor=1f;
			}
			
		}

		if( GUI.Button(new Rect(0+10,160,150,20),A11)  ){

			if(A11 == "Enable mask"){
				ICE_Script.mask = Resources.Load("HERO_MASK", typeof(Texture2D)) as Texture2D;
				A11 = "Disable mask";
			}else
			if(A11 == "Disable mask"){
				ICE_Script.mask = ICE_Script.emitter.GetComponent<Renderer>().material.mainTexture as Texture2D;
				A11 = "Enable mask";
			}
		}


		Time.timeScale = GUI.HorizontalSlider(new Rect(0+10,200,150,20),Time.timeScale,0.0f,1.2f);
		if( GUI.Button(new Rect(0+10+150,200,150,20),"Reset Speed="+Time.timeScale)  ){

			Time.timeScale=1f;

		}

		if(use_audio & Audio_source!=null){
			This_audio.pitch = Time.timeScale;

			This_audio.volume = GUI.HorizontalSlider(new Rect(0+10,240,150,20),This_audio.volume,0.0f,1f);
			if( GUI.Button(new Rect(0+10+150,240,150,20),"Reset Volume="+This_audio.volume)  ){
				
				This_audio.volume=1f;
				
			}
		}

		if( GUI.Button(new Rect(0+10,260,150,20),"Toggle effects")  ){

			if(Attractor.transform.parent.transform.parent.gameObject.activeInHierarchy){

				Attractor.transform.parent.transform.parent.gameObject.SetActive(false);

			}else{
				Attractor.transform.parent.transform.parent.gameObject.SetActive(true);
			}

			if(Attractor_planar.transform.parent.transform.parent.gameObject.activeInHierarchy){
				
				Attractor_planar.transform.parent.transform.parent.gameObject.SetActive(false);
				
			}else{
				Attractor_planar.transform.parent.transform.parent.gameObject.SetActive(true);
			}

		}

		string CYCLE = "Auto cycle in "+Auto_cycle_time+"s";
		if(!Auto_cycle_targets){

			CYCLE = "Auto cycle";

		}

		if( GUI.Button(new Rect(0+10+150,260,150,20),CYCLE)  ){
			if(Auto_cycle_targets){
				Auto_cycle_targets=false;
			}else{
				Auto_cycle_targets=true;
			}
		}
		Auto_cycle_time = GUI.HorizontalSlider(new Rect(0+10+150,280,150,20),Auto_cycle_time,1f,10f);

		if( GUI.Button(new Rect(0+10+150,320,150,20),"Toggle dancing splines")  ){
			if(dancing_splines){
				dancing_splines=false;
			}else{
				dancing_splines=true;
			}
		}

				//v1.4
				if( GUI.Button(new Rect(0+10+150,340,150,20),"Toggle particle")  ){
					if(!Switch_particle){
						Switch_particle=true;
					}
				}
		///////////// HANDLE TARGETS ///////////////


		string Target_name = Targets[1-1].name;
		int Xstep = 130;
		if(Targets.Count > 0){
		if( GUI.Button(new Rect(0+10,10,Xstep,20),Target_name)  ){
			
			Particle_SOURCE = Targets[chosen_target-1].transform.position;
			
			chosen_target=1;
			
			Particle_TARGET = Targets[chosen_target-1].transform.position;

			Transitions();
		}
		}
		if(Targets.Count > 1){
		Target_name = Targets[2-1].name;
		if( GUI.Button(new Rect(0+10+(1*Xstep),10,Xstep,20),Target_name)  ){
			
			Particle_SOURCE = Targets[chosen_target-1].transform.position;
			
			chosen_target=2;
			
			Particle_TARGET = Targets[chosen_target-1].transform.position;
			
			Transitions();
		}
		}
		if(Targets.Count > 2){
		Target_name = Targets[3-1].name;
		if( GUI.Button(new Rect(0+10+(2*Xstep),10,Xstep,20),Target_name)  ){
			
			Particle_SOURCE = Targets[chosen_target-1].transform.position;
			
			chosen_target=3;
			
			Particle_TARGET = Targets[chosen_target-1].transform.position;
			
			Transitions();
		}
		}
		if(Targets.Count > 3){
		Target_name = Targets[4-1].name;
		if( GUI.Button(new Rect(0+10+(3*Xstep),10,Xstep,20),Target_name)  ){
			
			Particle_SOURCE = Targets[chosen_target-1].transform.position;
			
			chosen_target=4;
			
			Particle_TARGET = Targets[chosen_target-1].transform.position;
			
			Transitions();
		}
		}
		if(Targets.Count > 4){
		Target_name = Targets[5-1].name;
		if( GUI.Button(new Rect(0+10+(4*Xstep),10,Xstep,20),Target_name)  ){
			
			Particle_SOURCE = Targets[chosen_target-1].transform.position;
			
			chosen_target=5;
			
			Particle_TARGET = Targets[chosen_target-1].transform.position;
			
			Transitions();
		}
		}
		if(Targets.Count > 5){
		Target_name = Targets[6-1].name;
		if( GUI.Button(new Rect(0+10+(5*Xstep),10,Xstep,20),Target_name)  ){
			
			Particle_SOURCE = Targets[chosen_target-1].transform.position;
			
			chosen_target=6;
			
			Particle_TARGET = Targets[chosen_target-1].transform.position;
			
			Transitions();
		}
		}
		if(Targets.Count > 6){
			Target_name = Targets[7-1].name;
			if( GUI.Button(new Rect(0+10+(6*Xstep),10,Xstep,20),Target_name)  ){
				
				Particle_SOURCE = Targets[chosen_target-1].transform.position;
				
				chosen_target=7;
				
				Particle_TARGET = Targets[chosen_target-1].transform.position;
				
				Transitions();
			}
		}

		if(ICE_Script.Colored){

			string TAG_ME = "Custom color";
			if(  ICE_Script.Custom_Color == false){TAG_ME = "Texture color";}
			if (GUI.Button(new Rect(10, Xstep+(2f*40)-20+100, 100, 20), TAG_ME)){

				if(ICE_Script.Custom_Color == false){

					ICE_Script.Custom_Color = true;
				}else{

					ICE_Script.Custom_Color = false;
				}

			}

			if(  ICE_Script.Custom_Color){

				Start_color.r = GUI.HorizontalSlider(new Rect(10, Xstep+100-10+100, 100, 30),Start_color.r,0,1);

				Start_color.g = GUI.HorizontalSlider(new Rect(10, Xstep+130-10+100, 100, 30),Start_color.g,0,1);

				Start_color.b = GUI.HorizontalSlider(new Rect(10, Xstep+160-10+100, 100, 30),Start_color.b,0,1);

				ICE_Script.End_color = Start_color;
			}
		}

		}
	}

	void Transitions(){

		//check type
		int Target_type = 0;
		SplinerP TEST_SPLINE = Targets[chosen_target-1].GetComponent(typeof(SplinerP)) as SplinerP;
		SkinnedMeshRenderer TEST_SKINNED_MESH =  Targets[chosen_target-1].GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		MeshFilter TEST_MESH =  Targets[chosen_target-1].GetComponent(typeof(MeshFilter)) as MeshFilter;
		
		if(TEST_SPLINE!=null){
			Target_type = 1; //spline
		}
		if(TEST_SKINNED_MESH!=null | TEST_MESH!=null){
			Target_type = 2; //mesh
		}
		
		if(Target_type!=1 & Target_type!=2){
			//if not mesh or spline, should be a projector
			SPLINE_Script.enabled=false;
			ICE_Script.enabled=false;
			PROJECT_Script.enabled=true;
		}else{PROJECT_Script.enabled=false;}
		
		if(Target_type == 1){ //is spline
			
			//do same resets as when we choose mesh
			SPLINE_Script.enabled=false;
			SPLINE_Script.extend_life=false;
			SPLINE_Script.Return_Speed=0;
			SPLINE_Script.Gravity=true;
			SPLINE_Script.relaxed=true;
			
			SPLINE_Script.SplinerP_OBJ = Targets[chosen_target-1]; //add new target
			
			if(Targets[chosen_target-1].name =="SPLINE 1 star"){
				
				SPLINE_Script.Interpolate = true;
				SPLINE_Script.Interpolate_steps = 8;
				
			}else{
				SPLINE_Script.Interpolate = true;//audio
				SPLINE_Script.Interpolate_steps = 2;
			}

			SPLINE_Script.hold_emission = false;
				ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
				main.gravityModifier=0.0f;
			SPLINE_Script.keep_in_position_factor =1f;
			ICE_Script.keep_in_position_factor=1f;
			ICE_Script.Gravity_Mode=true;
			ICE_Script.extend_life=true;
			
			SPLINE_Script.Start ();
			
			ICE_Script.enabled=false;
			SPLINE_Script.enabled=true;
			move_to_target=2;
			SPLINE_Script.Gravity=true;
			
		}
		
		if(Target_type == 2){
			
			ICE_Script.emitter = Targets[chosen_target-1]; //add new target
			
			ICE_Script.face_emit=true;

			if(TEST_SKINNED_MESH!=null ){

				//v1.7
				if(per_step_mask){
						if(Mask_per_step!=null){
							if(Mask_per_step.Count >= chosen_target){

								ICE_Script.mask = Mask_per_step[chosen_target-1];

							}
						}
				}
				else{
					ICE_Script.mask = ICE_Script.emitter.GetComponent<Renderer>().material.mainTexture as Texture2D;
				}

				ICE_Script.face_emit=false;
				ICE_Script.Return_speed=0;
				move_to_target=4;
				//ICE_Particles.gameObject.transform.position = ICE_Script.emitter.transform.position;
			}
			
			ICE_Script.Transition = true;
			ICE_Script.Start ();
			
			SPLINE_Script.enabled=false;
			SPLINE_Script.extend_life=false;
			SPLINE_Script.Return_Speed=0;
			SPLINE_Script.Gravity=true;
			SPLINE_Script.hold_emission = false;
			//SPLINE_Script.hold_emission = false;
				ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
				main.gravityModifier=0.0f;
			SPLINE_Script.keep_in_position_factor =1f;
			ICE_Script.keep_in_position_factor=1f;
			ICE_Script.Gravity_Mode=true;
			ICE_Script.extend_life=true;
			
			ICE_Script.Let_loose=true;
			
			ICE_Script.enabled=true;

			ICE_Script.Return_speed=0;
			move_to_target=4;
			
		}

	}

	void Colorize(){

		ICE_Script.Custom_Color = true;
		ICE_Script.End_color = Start_color;

	}

}
}