using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

//[ExecuteInEditMode()]
public class FlockCollisionsPDM : MonoBehaviour {	
	
		public bool Cast_to_enemy = false;
		//public float Stop_cast_after = 1;//after 1 sec of having found enemy, remove transform from pool
		public AttractParticles Cast_distributer;
		public float CastEnemyDist = 15;

	public void Start () {

			//cache transform
			//This_transf = this.transform;

			//if particle has not been assigned, try to find on same object
			if(p2 == null){
				p2 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
			}

			//if not found on same object, give  a warning
			if(p2 == null){
				Debug.Log ("Please attach the script to a particle system");
			}

			if(p2 != null){
				Propagator = p2.gameObject.GetComponent(typeof(ParticlePropagationPDM)) as ParticlePropagationPDM;
			}

			timer = Time.fixedTime;
	}
	
	//casching of major systems
	//Transform This_transf;
	public ParticleSystem p2;
	ParticlePropagationPDM Propagator;
					
	//grab particle helpers
	//public Particle[] particles;
	ParticleSystem.Particle[] ParticleList;

	public Vector3 Leader_speed = new Vector3(1,1,1);
	public float Leader_freq = 1;

	//AI parameters
	public float speedMult  = 2.0f;
	public Transform Swarm_center;
	public float FlockSpeed = 11;
	public float Avoid_speed = 0.1f;
	public float Rot_speed = 0.1f;
	public float Min_obj_dist = 0.8f;//distance to start avoidance in forward vector
	public float Min_ground_dist = 1;
			
	//Ground hit
	public float RaycastDist = 100f;
	public float CastDistForw = 1f;
	public float CastDistLeft = 0.2f;
	public bool Rot_to_motion = false;
	public bool Debug_on = false;
	public bool Log_Debug = false;
	public float Start_after = 4;//start after a timer, so propagator has spread items
	float timer;

	List<Vector3> Previous_pos = new List<Vector3>();

		public bool Constant_motion = true;

		public bool Free_leader = false; //notify that leader will be controlled by the script and waypoints

		public bool look_at_leader = false;
		public Vector2 Min_max_speed = new Vector2(1,2);

		public float Leader_min_dist = 1; //stop going towards leader below this dist
	
		public float Side_avoid_speed = 0;//factor to control towards left/right speed when in butterfly mode (stay in place option)
		public bool Front_lr_check = false;
		public bool do_sweep = true;
		float sweep_angle = 0;
		public float max_slope = 35;
		public float to_leader_factor =0.2f;
		public float to_forward =0.7f;

		//rotation factors
		List<Vector3> Rot_towards_vector = new List<Vector3>();
		List<float> Rot_towards_timer = new List<float>();
		public float Rot_towards_update = 3f;//change rotation vector update timer


		float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {

			Vector3 perp = Vector3.Cross(fwd, targetDir);
			float dir  = Vector3.Dot(perp, up);
			
			if (dir > 0.0f) {
				return 1.0f;
			} else if (dir < 0.0f) {
				return -1.0f;
			} else {
				return 0.0f;
			}
		}
		
		float ContAngle(Vector3 fwd, Vector3 targetDir, Vector3 upDir) {
			var angle = Vector3.Angle(fwd, targetDir);

			if (AngleDir(fwd, targetDir, upDir) == -1) {
				return 360 - angle;
			} else {
				return angle;
			}
		}

		public float Left_Right_speed = 3;
		public bool cut_motion_lr = false;
		public bool Stay_in_place = false; //stay upon collision, dont sweep left/right
		public float Forward_col_speed = 0.2f; //speed with which ostacle is avoided to opposite direction

		int current_leader_id = 0;

		public bool Use_GUI = false;
		bool AI_enabled = true;

	void LateUpdate () {

		if(AI_enabled){

			if(Time.fixedTime - timer < Start_after){
				return;
			}

				//handle enemy
				if(Cast_to_enemy){
					if(Cast_distributer !=null){
						Cast_distributer.Emit_transforms.Clear();
					}
				}

			for(int i=0;i<Propagator.Gameobj_instances.Count;i++){

				if(Rot_towards_vector.Count < i+1){
					Rot_towards_vector.Add(Propagator.Gameobj_instances[i].right);
				}
				if(Rot_towards_timer.Count < i+1){
					Rot_towards_timer.Add(Time.fixedTime);
				}
			}

		//retry to grab particle
		if(p2 == null ){
			p2 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

			if(p2==null){
				Debug.Log ("Please attach the script to a particle system");
				return;
			}
		}
		//Grab propagation / spray script, that controls the particles and holds the attached gameobjects
		if(p2 != null){
			Propagator = p2.gameObject.GetComponent(typeof(ParticlePropagationPDM)) as ParticlePropagationPDM;
		}		

		//Grab particles
		ParticleSystem p11=p2;							
		ParticleList = new ParticleSystem.Particle[p11.particleCount];
		p11.GetParticles(ParticleList);
				
		//Go through gameobjects (from propagation script, adapt later to grab mesh / projected etc gameobject particles)
		if(Propagator != null){
			if(Propagator.gameobject_mode){
				if(Propagator.Gameobj_instances != null){
					if(Propagator.Gameobj_instances.Count > 0){
												
		//start iterating objects, after checks
		for(int i=0; i < Propagator.Gameobj_instances.Count; i++){ // adapt to spread checks over frames

			//Place on ground
			bool enter_waypoint_motion = true;

			//Propagator.follow_particles = false;
			RaycastHit hit;
			if(Physics.Raycast(Propagator.Gameobj_instances[i].position, Vector3.down, out hit, RaycastDist)) {
				Vector3 targetPos = hit.point;
				targetPos += new Vector3(0, Propagator.Gameobj_instances[i].localScale.y / 2, 0);

				if(Propagator.Gameobj_instances[i].position.y < targetPos.y+Min_ground_dist){

					Propagator.Gameobj_instances[i].position = new Vector3(Propagator.Gameobj_instances[i].position.x,
						                                                       targetPos.y+Min_ground_dist+0.1f,
						                                                       Propagator.Gameobj_instances[i].position.z);
				}					
			}

			Vector3 Rotated_vector = Propagator.Gameobj_instances[i].forward;
			
			if(do_sweep){
				Quaternion q = Quaternion.AngleAxis(sweep_angle, Vector3.up);
				Rotated_vector = q*Rotated_vector;
				sweep_angle = sweep_angle+300*Time.deltaTime;
				if(sweep_angle > 350){
					sweep_angle=0;
				}
			}	

			//AVOIDANCE
			if(1==1){
			//look in front and around in cone

			//handle enemy
			if(Cast_to_enemy & Cast_distributer != null){
				if(Physics.Raycast(Propagator.Gameobj_instances[i].position, Propagator.Gameobj_instances[i].forward, out hit, CastEnemyDist)) {
					if(hit.collider.gameObject.tag =="Enemy"){
						Cast_distributer.Emit_transforms.Add(Propagator.Gameobj_instances[i].transform);
					}
				}
			}

			bool Check_forw_lr = true; // if Front_lr_check = false and below check is true, dont enter left/right check

			if(Physics.Raycast(Propagator.Gameobj_instances[i].position, Propagator.Gameobj_instances[i].forward, out hit, CastDistForw) & 1==1) {
				
				if(Vector3.Angle(hit.normal,Vector3.up) >  max_slope | 1==0){ //if angle shows a flat to 35 degrees ground
					
					Vector3 targetPos = hit.point;							
					Vector3 ToTargetVec = targetPos - Propagator.Gameobj_instances[i].position;
													

											if(Front_lr_check){
												Check_forw_lr = true;
											}else{
												Check_forw_lr = false;
											}

					//Replace current_leader_id with Closest_leader_id for multi leader flocks
					float Angle_hero = AngleDir(Propagator.Gameobj_instances[i].forward,
							Propagator.Gameobj_instances[current_leader_id].position-Propagator.Gameobj_instances[i].position,Vector3.up);
					//Debug.Log (Angle_hero);//-1 = left, 1 = right, 0 = front

					if(Time.fixedTime-Rot_towards_timer[i] > Rot_towards_update){
						Rot_towards_vector[i] = Propagator.Gameobj_instances[i].right;
						if(Angle_hero == -1 & Random.Range (1,5) > 3){							
								Rot_towards_vector[i] = -Propagator.Gameobj_instances[i].right;							
						}
						Rot_towards_timer[i] = Time.fixedTime;
					}
				
					if(ToTargetVec.magnitude < Min_obj_dist & Stay_in_place){					
												Propagator.Gameobj_instances[i].position =  targetPos + ToTargetVec.normalized*Min_obj_dist ;													
					}else{
												Vector3 ROT_FINAL = Rot_towards_vector[i]* Side_avoid_speed;

						Propagator.Gameobj_instances[i].position = Vector3.Lerp(Propagator.Gameobj_instances[i].position, 
								Propagator.Gameobj_instances[i].position 								
								- (CastDistForw-hit.distance/CastDistForw)*(ToTargetVec.normalized*Forward_col_speed + ROT_FINAL)								
								,Avoid_speed*Time.deltaTime);
					}
					
					Vector3 Motion_vec2 = Propagator.Gameobj_instances[i].forward;					
					
					if(Debug_on){
						if( i == current_leader_id){
							Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.1f,0)+3.0f*Motion_vec2,Color.red,0.5f);
							Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.2f,0)+3.0f*Propagator.Gameobj_instances[i].forward,Color.cyan,0.5f);
							Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.3f,0)+3.0f*Rot_towards_vector[i],Color.yellow,0.5f);
							Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point,Color.magenta,0.5f);
							
						}else{
							Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+3.0f*Motion_vec2,Color.green,3);
							Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point+3.0f*Motion_vec2,Color.cyan,3);
						}
					}	

					Quaternion _lookRotation = Quaternion.LookRotation(Rot_towards_vector[i]);
					Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp(Propagator.Gameobj_instances[i].rotation,
										 _lookRotation, Rot_speed*Time.deltaTime);

					enter_waypoint_motion = false;
					if(Log_Debug){
						Debug.Log("avoid obstacle in front = "+ hit.collider.gameObject.name);
					}
				}			

			}
			else
			if(Physics.Raycast(Propagator.Gameobj_instances[i].position, Rotated_vector, out hit, 2*CastDistForw) & do_sweep & 1==1) {

				if(Vector3.Angle(hit.normal,Vector3.up)< max_slope | 1==1){ //if angle shows a flat to 35 degrees ground

									//Vector3 targetPos = hit.point;							
									//Vector3 ToTargetVec = targetPos - Propagator.Gameobj_instances[i].position;

											if(Front_lr_check){
												Check_forw_lr = true;
											}else{
												Check_forw_lr = false;
											}							

									Vector3 Motion_vec2 = Propagator.Gameobj_instances[i].forward;
								
									if(Debug_on){
										if( i == current_leader_id){
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.1f,0)+3.0f*Motion_vec2,Color.red,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.2f,0)+3.0f*Propagator.Gameobj_instances[i].forward,Color.cyan,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.3f,0)+3.0f*Propagator.Gameobj_instances[i].right,Color.yellow,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point,Color.magenta,0.5f);

										}else{
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+3.0f*Motion_vec2,Color.green,3);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point+3.0f*Motion_vec2,Color.red,3);
										}
									}

									Quaternion _lookRotation = Quaternion.LookRotation(Rot_towards_vector[i]);

									Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp(Propagator.Gameobj_instances[i].rotation,
									                                                           _lookRotation, Rot_speed*Time.deltaTime);

									enter_waypoint_motion = false;
									if(Log_Debug){
									Debug.Log("avoid obstacle in front = "+ hit.collider.gameObject.name);
									}
								}
							}
		//else
										if(Check_forw_lr){
							if(Physics.Raycast(Propagator.Gameobj_instances[i].position, Propagator.Gameobj_instances[i].right, 
									                  out hit, CastDistLeft)) {
									Vector3 targetPos = hit.point;							
									Vector3 ToTargetVec = targetPos - Propagator.Gameobj_instances[i].position;

										Vector3 GO_towards = -Propagator.Gameobj_instances[i].right;
										Vector3 ROT_towards = -Propagator.Gameobj_instances[i].right;
										//ROT_towards = Propagator.Gameobj_instances[i].forward;

										//forces from particles will try to force items into colliders, use Stay in place to lock them
										if(ToTargetVec.magnitude < Min_obj_dist & Stay_in_place){
											
											Propagator.Gameobj_instances[i].position =  targetPos + ToTargetVec.normalized*Min_obj_dist ;
												//+ Avoid_speed*GO_towards;
										}else{
									Propagator.Gameobj_instances[i].position = Vector3.Lerp(Propagator.Gameobj_instances[i].position, 
										 Propagator.Gameobj_instances[i].position - (CastDistLeft-hit.distance/CastDistLeft)*(ToTargetVec.normalized + GO_towards)
									      ,Left_Right_speed*Avoid_speed*Time.deltaTime);
										}								

										Quaternion _lookRotation = Quaternion.LookRotation(ROT_towards);
									Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp(Propagator.Gameobj_instances[i].rotation,
									                                                            _lookRotation, Rot_speed*Time.deltaTime);

										Vector3 Motion_vec2 = Propagator.Gameobj_instances[i].forward;
										if(Debug_on){
										if( i == current_leader_id){
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.1f,0)+3.0f*Motion_vec2,Color.red,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.2f,0)+3.0f*Propagator.Gameobj_instances[i].forward,Color.cyan,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.3f,0)+3.0f*Propagator.Gameobj_instances[i].right,Color.yellow,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point,Color.magenta,0.5f);
											
										}else{
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+3.0f*Motion_vec2,Color.white,3);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point+3.0f*Motion_vec2,Color.yellow,3);
										}
										}

										if(cut_motion_lr | ToTargetVec.magnitude < 0.1f){
									enter_waypoint_motion = false;
										}
							}
							else
							if(Physics.Raycast(Propagator.Gameobj_instances[i].position, -Propagator.Gameobj_instances[i].right, 
									                  out hit, CastDistLeft)) {
									Vector3 targetPos = hit.point;							
									Vector3 ToTargetVec = targetPos - Propagator.Gameobj_instances[i].position;

										Vector3 GO_towards = Propagator.Gameobj_instances[i].right;
										Vector3 ROT_towards = Propagator.Gameobj_instances[i].right;
										//ROT_towards=Propagator.Gameobj_instances[i].forward;

										//forces from particles will try to force items into colliders, use Stay in place to lock them
										if(ToTargetVec.magnitude < Min_obj_dist & Stay_in_place){
											
											Propagator.Gameobj_instances[i].position =  targetPos + ToTargetVec.normalized*Min_obj_dist ;
												//+ Avoid_speed*GO_towards;
										}else{
									Propagator.Gameobj_instances[i].position = Vector3.Lerp(Propagator.Gameobj_instances[i].position, 
										Propagator.Gameobj_instances[i].position - (CastDistLeft-hit.distance/CastDistLeft)*(ToTargetVec.normalized + GO_towards)
										,Left_Right_speed*Avoid_speed*Time.deltaTime);
										}								

										Quaternion _lookRotation = Quaternion.LookRotation(ROT_towards);
									Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp(Propagator.Gameobj_instances[i].rotation,
									                                                            _lookRotation, Rot_speed*Time.deltaTime);

										Vector3 Motion_vec2 = Propagator.Gameobj_instances[i].forward;
										if(Debug_on){
										if( i == current_leader_id){
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.1f,0)+3.0f*Motion_vec2,Color.red,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.2f,0)+3.0f*Propagator.Gameobj_instances[i].forward,Color.cyan,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+new Vector3(0,0.3f,0)+3.0f*Propagator.Gameobj_instances[i].right,Color.yellow,0.5f);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point,Color.magenta,0.5f);
											
										}else{
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].position+3.0f*Motion_vec2,Color.magenta,3);
											Debug.DrawLine(Propagator.Gameobj_instances[i].position,hit.point+3.0f*Motion_vec2,Color.red,3);
										}
										}

										if(cut_motion_lr | ToTargetVec.magnitude < 0.1f){
									enter_waypoint_motion = false;
										}
							}
							else
							if(Physics.Raycast(Propagator.Gameobj_instances[i].position, -Propagator.Gameobj_instances[i].forward, 
								                  out hit, CastDistLeft)) {
								Vector3 targetPos = hit.point;							
								Vector3 ToTargetVec = targetPos - Propagator.Gameobj_instances[i].position;
								Propagator.Gameobj_instances[i].position = Vector3.Lerp(Propagator.Gameobj_instances[i].position, 
									                                                        Propagator.Gameobj_instances[i].position - (CastDistLeft-hit.distance/CastDistLeft)*(ToTargetVec.normalized + Propagator.Gameobj_instances[i].right)
								                                                        ,Avoid_speed*Time.deltaTime);
								
								if(ToTargetVec.magnitude < Min_obj_dist){									
									Propagator.Gameobj_instances[i].position =  targetPos + ToTargetVec.normalized*Min_obj_dist 
										+ Avoid_speed*Propagator.Gameobj_instances[i].right;
								}
								
								Quaternion _lookRotation = Quaternion.LookRotation(Propagator.Gameobj_instances[i].right);
								Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp(Propagator.Gameobj_instances[i].rotation,
								                                                            _lookRotation, Rot_speed*Time.deltaTime);
								
								enter_waypoint_motion = false;
							}
							
								}//END if Check_forw_lr = false;
							//Look at direction
							//else 
							if(i < Previous_pos.Count & Rot_to_motion){
									Vector3 Motion_vec = Propagator.Gameobj_instances[i].position - Previous_pos[i];								

									Quaternion New_rot = Quaternion.identity;
									//if(Motion_vec != Vector3.zero){
									if(Motion_vec.magnitude > 0.01f){
										New_rot = Quaternion.LookRotation(1*Motion_vec);
										Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp( Propagator.Gameobj_instances[i].rotation,New_rot,Time.deltaTime*116);
									}
									if(Log_Debug){
									Debug.Log("rot to direction");
									}									
							}
							//////////////							
						}//END if 1==1

							//MAIN MOTION
							if(enter_waypoint_motion | Constant_motion){
								if( i == current_leader_id){
									
									if(Debug_on){
										//Debug.DrawRay(Propagator.Gameobj_instances[current_leader_id].position,Propagator.Gameobj_instances[current_leader_id].forward,Color.red,1);
									}

									//move leader independently
									//Vector3 Disp = new Vector3(1,0,1);
									if(Swarm_center != null){

																			
											if(!Free_leader){
												Propagator.Gameobj_instances[current_leader_id].RotateAround(Swarm_center.position, Vector3.up, speedMult * 21 * Time.deltaTime);
											}else{

											}
									}
									//raycast to change direction if obstacle ahead
									
									//add cos and turbulent motion
									if(!Free_leader){
												Propagator.Gameobj_instances[current_leader_id].position = 
												new Vector3(Propagator.Gameobj_instances[current_leader_id].position.x,
										        Leader_speed.y*0.1f*Mathf.Cos(Leader_freq*Time.fixedTime)
												            +Propagator.Gameobj_instances[current_leader_id].position.y, 
												            Propagator.Gameobj_instances[current_leader_id].position.z);
									}									
									
								}else{ //herd motion
									
									if(Debug_on){
										//Debug.DrawRay(Propagator.Gameobj_instances[i].position,Propagator.Gameobj_instances[i].forward,Color.blue,1);
									}
									
									Vector3 Disp =  Propagator.Gameobj_instances[current_leader_id].position - Propagator.Gameobj_instances[i].position;
									//Random.seed = i;
											Random.InitState (i);//v2.3
									float Rand_speed = Random.Range(Min_max_speed.x,Min_max_speed.y);
																
									if(Disp.magnitude > Leader_min_dist){
										
										float Angle = Vector3.Angle(Disp,
											                            Propagator.Gameobj_instances[i].forward);
										float Angle_factor = (1-(Angle/180));
										

												Propagator.Gameobj_instances[i].position += 
												to_forward*Propagator.Gameobj_instances[i].forward* Time.deltaTime * FlockSpeed * Rand_speed
												+ to_leader_factor*(Angle_factor * Disp.normalized * Time.deltaTime * FlockSpeed * Rand_speed);
											

										if(look_at_leader){
											Quaternion rotation = Quaternion.LookRotation(Propagator.Gameobj_instances[current_leader_id].position - Propagator.Gameobj_instances[i].position);
											Propagator.Gameobj_instances[i].rotation = Quaternion.Slerp(Propagator.Gameobj_instances[i].rotation, rotation, Time.deltaTime * 1);
										}
									}									
								}
							}	
						}//END loop objects
					}
				}
			}
		}

			//keep previous position
			//Previous_pos.Clear();
			for(int i=0;i<Propagator.Gameobj_instances.Count;i++){
				//Previous_pos.Add(Propagator.Gameobj_instances[i].position);

				//v1.4
				if(i < Previous_pos.Count){
					if(  (Previous_pos[i] - Propagator.Gameobj_instances[i].position).magnitude >0.01f){
						Previous_pos[i] = Propagator.Gameobj_instances[i].position;
					}else{
						//Propagator.Gameobj_instances[i].position = Previous_pos[i];
					}
				}
			}
		}//End if enabled
		}// END update

		void SmoothLook(Transform Item, Vector3 newDirection){
			transform.rotation = Quaternion.Lerp(Item.rotation, Quaternion.LookRotation(newDirection), Time.deltaTime);
		}

		void OnGUI(){
			if(Use_GUI){
				string aionoff = "on";
				if(!AI_enabled){
					aionoff = "off";
				}
				if (GUI.Button(new Rect(5, 5, 100, 25), "AI is "+aionoff)){
					if(!AI_enabled){
						AI_enabled = true;							
					}else{
						AI_enabled = false;							
					}
				}
			}
		}

	}
}
