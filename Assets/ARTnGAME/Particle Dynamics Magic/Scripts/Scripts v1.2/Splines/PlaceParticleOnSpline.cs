using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Artngame.PDM {

[ExecuteInEditMode()]
public class PlaceParticleOnSpline : MonoBehaviour {
	
	public void Start () {

			if(Preview_mode & Application.isPlaying){				
				
				if(Parent_OBJ != null){
					int childs = Parent_OBJ.transform.childCount;
					
					for (int i = childs - 1; i > 0; i--)						
					{						
						GameObject.Destroy(Parent_OBJ.transform.GetChild(i).gameObject);						
					}
				}					
			}
			
			if(Gameobj_instances==null){
				Gameobj_instances = new List<GameObject>();
			}
			
			if(SplinerP_OBJ != null){
				Spline_to_Conform = SplinerP_OBJ.GetComponent("SplinerP") as SplinerP;
				keep_curve_quality =Spline_to_Conform.CurveQuality; 
			}
			
			if(p2==null){
				p2=this.gameObject.GetComponent<ParticleSystem>();
			}
			
			if(p2 !=null & SplinerP_OBJ != null){
				ParticleList = new ParticleSystem.Particle[Spline_to_Conform.Curve.Count];
			}
			
			Particle_Num = particle_count;
			
			Registered_paint_positions = new List<Vector3>();
			Registered_paint_rotations = new List<Vector3>();
			
			noise = new PerlinPDM ();
			
			if(Application.isPlaying){
				
				Preview_mode=false;
				
				if(Gameobj_instances!=null){
					for(int i=Gameobj_instances.Count-1;i>=0;i--){
						
						DestroyImmediate(Gameobj_instances[i]);
					}
				}				
			}
			
			//keep_particle_count = particle_count;
			
			if(Parent_OBJ==null | Gameobj == null | p2 == null  | SplinerP_OBJ == null){
				
				//Debug.Log ("Please add an emitter, a gameobject to be used as particle, a container object for pooling and a SplineP object");
				
			}

//			if(p2 == null){
//				p2 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
//			}
//
//			if(p2 == null){
//
//				Debug.Log ("Please attach the script to a particle system");
//			}
//
//		if(p2 == null | SplinerP_OBJ == null){
//			
//			return;
//			
//		}
//
//		Spline_to_Conform = SplinerP_OBJ.GetComponent("SplinerP") as SplinerP;


	}
		//v1.8
		[HideInInspector]
		public List<Vector3> Spline_to_Conform_Curve_pos; 
		int keep_curve_points;
	
	public ParticleSystem p2;

		//v1.8
		public bool Gameobject_mode=false;

	public bool Interpolate=false;
	public bool Angled_Interpolate=true;
	public bool Variable_step = false;
	public int Interpolate_steps=2;
	public float Reduce_factor=1f;

		//v1.8
		public bool Look_at_spline=false; //make particles rotate, best used with horizontal or mesh particles
		public float Rotation_offset = 0;//compensate for initial mesh rotation
		public Vector3 Rot_axis = new Vector3(0,1,0);
		public Vector3 Rot_angle_axis = new Vector3(1,0,0);
		public int Angle_split = 1;

	public GameObject SplinerP_OBJ;
	SplinerP Spline_to_Conform;
	

		public bool Gravity=false;
		public float Return_Speed=0.005f;
	
	//public Particle[] particles;
	ParticleSystem.Particle[] ParticleList;


	
	public bool relaxed = true;
	
	private int counter;

		public float keep_in_position_factor =0.90f;
		public float keep_alive_factor =0.0f;
		
		public bool hold_emission=false;
		
		public bool extend_life=false;
		public bool Transition=false;

		private int Particle_count;
		private int Keep_initial_Particle_count;
		public bool no_overflow=false;




		/// <summary>
		/// GAMEOBJECTS ///////////		/// </summary>

		public Vector3 GameObj_Offset;
		
		public bool Preview_mode=false;
		private List<Vector3> Registered_paint_positions; 
		private List<Vector3> Registered_paint_rotations; 		
		private List<GameObject> Gameobj_instances;
		private int keep_curve_quality;
		//private int keep_particle_count;
		public GameObject Parent_OBJ;
		public GameObject Gameobj;
		public bool Angled=false;
		public int particle_count = 100;
		private int Particle_Num;
		
		public bool Asign_rot=false;
		public Vector3 Local_rot = Vector3.zero;
		private PerlinPDM  noise;
		public float Wind_speed=1f;
		
		public bool follow_particles=false;
		public bool Remove_colliders=false;
		
		[HideInInspector]
		public bool fix_initial = false;
		private bool let_loose = false;
		public bool letloose = false;
		private int place_start_pos;
		
		[Range(1,10)] 
		public int Place_every_N_step = 2;
		bool got_positions=false;
		Vector3[] positions;		
		int[] tile;




	void Update () {

		if(p2 == null | SplinerP_OBJ == null){

			return;

		}

			//if(Spline_to_Conform.Curve != null){
				//Debug.Log (Spline_to_Conform.Curve.Count);
			//}
			Spline_to_Conform_Curve_pos = new List<Vector3>();
			if(Spline_to_Conform_Curve_pos.Count < 1 
			   && Spline_to_Conform != null 
			   && Spline_to_Conform.Curve != null
			   && Spline_to_Conform.Curve.Count > 0
			   ){
				//v1.8
				//Debug.Log ("in");
				//Create a double-less at nodes curve
				//Spline_to_Conform_Curve_pos = new List<Vector3>();
				Spline_to_Conform_Curve_pos.Add(Spline_to_Conform.Curve[0].position);
				for(int i=1;i<Spline_to_Conform.Curve.Count;i++){
					if(Spline_to_Conform.Curve[i].position != Spline_to_Conform.Curve[i-1].position){
						Spline_to_Conform_Curve_pos.Add(Spline_to_Conform.Curve[i].position);
					}
				}
				//Debug.Log (Spline_to_Conform.Curve.Count);
				//Debug.Log (Spline_to_Conform_Curve_pos.Count);
				keep_curve_points = Spline_to_Conform.Curve.Count;
			}else{
				return;
			}


			//GAMEOBJECT MODE
			if(Gameobject_mode){
				if(Gameobj_instances==null){
					Gameobj_instances = new List<GameObject>();
				}
				if(keep_curve_quality != Spline_to_Conform.CurveQuality){
					
					keep_curve_quality = Spline_to_Conform.CurveQuality;
					got_positions=false;
					
				}
			}

			//v1.8
			//Create a double-less at nodes curve
			if(keep_curve_points != Spline_to_Conform.Curve.Count | Spline_to_Conform_Curve_pos.Count < 2 | Spline_to_Conform.repainted){ 
				Spline_to_Conform_Curve_pos = new List<Vector3>();
				Spline_to_Conform_Curve_pos.Add(Spline_to_Conform.Curve[0].position);
				for(int i=1;i<Spline_to_Conform.Curve.Count;i++){

					//float Dist = Vector3.Distance(Spline_to_Conform.Curve[i].position,Spline_to_Conform.Curve[i-1].position);
					float Dist2 = Vector3.Distance(Spline_to_Conform.Curve[i].position,Spline_to_Conform_Curve_pos[Spline_to_Conform_Curve_pos.Count-1]);

					//if(Spline_to_Conform.Curve[i].position != Spline_to_Conform.Curve[i-1].position & Dist > 0.01f){
					if(Spline_to_Conform.Curve[i].position != Spline_to_Conform_Curve_pos[Spline_to_Conform_Curve_pos.Count-1] & Dist2 > 0.1f){
						Spline_to_Conform_Curve_pos.Add(Spline_to_Conform.Curve[i].position);
					}else{
						//if(Dist < 0.1f){
							//Debug.Log (i);
						//}
					}
				}
			}


		if(!Application.isPlaying){ 

			if(Spline_to_Conform==null){
				if(SplinerP_OBJ!=null){
					Spline_to_Conform = SplinerP_OBJ.GetComponent("SplinerP") as SplinerP;
				}
			}

		}

		counter=0;

			//v1.3
			if(Transition)
			{
				Transition=false;
				if(p2!=null){
					if(p2.particleCount < p2.main.maxParticles){

						if( no_overflow & Particle_count >0 ){
							p2.Emit(Particle_count - p2.particleCount);
						}
						else{
							p2.Emit(p2.main.maxParticles - p2.particleCount);
						}

					}
				}

			}

			//v1.3
			if(relaxed & hold_emission){
				if(p2.particleCount < p2.main.maxParticles){

					if( no_overflow & Particle_count >0 ){



						if(p2.particleCount < Keep_initial_Particle_count & 1==1){


							p2.Emit(Keep_initial_Particle_count);

						}



						//Debug.Log ("Count = "+Particle_count);
						//Debug.Log ("P2 Count = "+p2.particleCount);
						//Debug.Log ("Max Count = "+p2.maxParticles);
					}
					else{
						p2.Emit(p2.main.maxParticles - p2.particleCount);
					}
				}
			}

			if(Keep_initial_Particle_count < Particle_count){
				Keep_initial_Particle_count = Particle_count;
			}

			Particle_count=0;


			/////// GAMEOBJECT MODE
			 
			int Spline_curve_count = Spline_to_Conform_Curve_pos.Count;

			/// 
			if(Gameobject_mode){
				if(particle_count > Spline_curve_count*Place_every_N_step){
					particle_count = Spline_curve_count*Place_every_N_step;
				
			}			
			
			if(!Preview_mode & !Application.isPlaying){				
				for(int i=Gameobj_instances.Count-1;i>=0;i--){					
					DestroyImmediate(Gameobj_instances[i]);
				}				
			}
			
			for(int i=Gameobj_instances.Count-1;i>=0;i--){
								
				if(Gameobj_instances[i] ==null){					
					Gameobj_instances.RemoveAt(i);
				}				
			}
			
			//reset, if particle number changes
			if(Particle_Num != particle_count){
				got_positions=false;
				Particle_Num = particle_count;
			}
			
			let_loose = letloose;
			if(!Application.isPlaying){ 
				
					//			aaa = new ParticleSystem.Particle[Spline_curve_count];				
				let_loose = false;
				
				if(noise ==null){
					noise = new PerlinPDM ();
				}				
			}
			
			if(p2.particleCount < particle_count){				
			}
			
			if(!let_loose){
				p2.Clear ();
			}
			
				if(p2.main.maxParticles != Spline_curve_count){
					ParticleSystem.MainModule main = p2.main;//v2.3
					main.maxParticles = Spline_curve_count;
			}			
			
				p2.Emit(Spline_curve_count);
			if(!let_loose){
					//			aaa = new ParticleSystem.Particle[Spline_curve_count];
				got_positions=false;
			}			
			//		int tileCount=15;			
			//		p2.GetParticles(aaa);
			
			if(!got_positions){
				
					//positions = new Vector3[Spline_curve_count];
					//tile = new int[Spline_curve_count];
				if(ParticleList!=null){
					positions = new Vector3[ParticleList.Length];
					tile = new int[ParticleList.Length];
					
					got_positions = true;
					
					for(int i=0;i<ParticleList.Length;i++){
						positions[i] = ParticleList[i].position;
						tile[i] = Random.Range(0,15);
					}
				}
			}	
						
			// PROJECTION
			
			if(!fix_initial){
				Registered_paint_positions.Clear();
				Registered_paint_rotations.Clear();
			}
			
			if(Registered_paint_positions!=null){
				
				int counter1=0;
				for (int i=0; i <  Spline_curve_count;i++)
				{
					if(Registered_paint_positions.Count < Spline_curve_count){
						//Registered_paint_positions.Add(Spline_to_Conform.Curve[counter1].position);
						Registered_paint_positions.Add(Spline_to_Conform_Curve_pos[counter1]);
						Registered_paint_rotations.Add(Vector3.zero);
					}					
					counter1=counter1+1;
						if(counter1 > Spline_curve_count-1) 
					{						
						counter1=0;						
					}
				}				
			}
			
			if(Preview_mode | Application.isPlaying){
				
				if(Registered_paint_positions!=null){
					
					if(!follow_particles){
						
						for(int i=0;i<Registered_paint_positions.Count;i++){
							
							if(Gameobj_instances.Count < (particle_count/Place_every_N_step)){
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
						
						for(int i=0;i<ParticleList.Length-1;i=i+Place_every_N_step){
														
							if(Gameobj_instances.Count < (particle_count/Place_every_N_step) & Registered_paint_positions.Count > (i) )
							{
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
						Gameobj_instances[i].transform.position = Registered_paint_positions[i] + GameObj_Offset;
						
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
						}else{							
							Gameobj_instances[i].transform.rotation = Quaternion.identity;							
							//add look at spline direction
							if(i > 0){
								if(Registered_paint_positions[i]!=Registered_paint_positions[i-1]){
									Gameobj_instances[i].transform.rotation = Quaternion.LookRotation(Registered_paint_positions[i]-Registered_paint_positions[i-1]);
								}
							}
						}						
					}
				}
			}
		}
			/////// END GAMEOBJECT MODE

		if(1==1 ){

			ParticleSystem p11=p2;

				//v1.8
				int Count_objects = 0;
				
				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);
				
				if(!Interpolate){
					for (int i=0; i < ParticleList.Length;i++)
					{

						Particle_count++;

						//v1.3
						int tileCount=15;
						//Random.seed = i; //set to give same number for same i
						Random.InitState(i);//v2.3

						int Sheet_tile = Random.Range(1,15);//4x4 sheet
						if(extend_life){
							

							ParticleList[i].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];
							
						
						if(ParticleList[i].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
							ParticleList[i].startLifetime = tileCount;
						}
						}


						if (relaxed){

							//v1.3
							if(ParticleList[i].remainingLifetime>ParticleList[i].startLifetime*keep_in_position_factor){
								ParticleList[i].position  =  Spline_to_Conform_Curve_pos[counter];
							}


							if(Gravity){

								ParticleList[i].position  = Vector3.Slerp(ParticleList[i].position ,  Spline_to_Conform_Curve_pos[counter], Return_Speed);
							}

						}
						else{
							ParticleList[i].position  =  Spline_to_Conform_Curve_pos[counter];

						//size
						if(Reduce_factor != 1 & Application.isPlaying){   
							
							ParticleList[i].startSize = Reduce_factor*counter;
							
						}

						}

						//v1.8
						if(Look_at_spline){
							ParticleList[i].axisOfRotation = Rot_axis;
							if(counter+1 < Spline_to_Conform_Curve_pos.Count){
								Vector3 Diff = Spline_to_Conform_Curve_pos[counter] - Spline_to_Conform_Curve_pos[counter+1];
								float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
								if(Diff.z > 0){
									angle = - angle;
								}
								ParticleList[i].rotation = Rotation_offset + angle;
							}else{
								
								Vector3 Diff = Spline_to_Conform_Curve_pos[counter] - Spline_to_Conform_Curve_pos[0];
								float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
								ParticleList[i].rotation = Rotation_offset + angle;
								
							}
						}

						//GAMEOBJECT MODE
						if(Gameobject_mode){
							if(follow_particles){
								//if(Gameobj_instances.Count-1 > i ){
								if(Count_objects < Gameobj_instances.Count){
									Gameobj_instances[Count_objects].transform.position = ParticleList[i].position + GameObj_Offset;									
									//remove colliders
									if(Gameobj_instances[Count_objects].GetComponent<Collider>() !=null){
										if(Remove_colliders){											
											Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = false;											
										}
										else if(!Remove_colliders){											
											Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = true;											
										}
									}
								}
								Count_objects++;
							}
						}

						counter=counter+1;
						if(counter > Spline_to_Conform_Curve_pos.Count-1) 
						{

							counter=0;

						}


					}
				}

			if(Interpolate){

				if(Interpolate_steps < 1){

					Interpolate_steps=1;

				}

			  if(Angled_Interpolate){
				for (int i=0; i < ParticleList.Length-Interpolate_steps;i=i+Interpolate_steps)
				{
					Particle_count++;

					//v1.3
					int tileCount=15;
					//Random.seed = i; //set to give same number for same i
							Random.InitState(i);//v2.3
					int Sheet_tile = Random.Range(1,15);//4x4 sheet
					if(extend_life){
																	
						
						ParticleList[i].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];

					
					if(ParticleList[i].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
								ParticleList[i].startLifetime = tileCount;
					}
					}

					if (relaxed){

								//v1.3
								if( ParticleList[i].remainingLifetime > ParticleList[i].startLifetime*keep_in_position_factor ){
									ParticleList[i].position  =  Spline_to_Conform_Curve_pos[counter];
								}									
								for(int j=0;j<Interpolate_steps;j++){
									if(counter <= Spline_to_Conform_Curve_pos.Count-2){
											if((i+j+1)  < (ParticleList.Length-1)){
												if( ParticleList[i+j+1].remainingLifetime > ParticleList[i+j+1].startLifetime*keep_in_position_factor ){
												ParticleList[i+j+1].position  = Spline_to_Conform_Curve_pos[counter] + (j+1)*(Spline_to_Conform_Curve_pos[counter+2-1] -Spline_to_Conform_Curve_pos[counter])/(Interpolate_steps) ;
												}
												if(extend_life){													
													ParticleList[i+j+1].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];

												if(ParticleList[i+j+1].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
													ParticleList[i+j+1].startLifetime = tileCount;
												}
												}

												Particle_count++;
											}
										}
								}


						
						if(Gravity){
									ParticleList[i].position  = Vector3.Slerp(ParticleList[i].position, Spline_to_Conform_Curve_pos[counter], Return_Speed);
									
									for(int j=0;j<Interpolate_steps;j++){
										if(counter <= Spline_to_Conform_Curve_pos.Count-2){
											if((i+j+1)  < (ParticleList.Length-1)){
												ParticleList[i+j+1].position  =Vector3.Slerp(ParticleList[i+j+1].position, Spline_to_Conform_Curve_pos[counter] + (j+1)*(Spline_to_Conform_Curve_pos[counter+2-1] -Spline_to_Conform_Curve_pos[counter])/(Interpolate_steps), Return_Speed);
											}
										}
									}
						}

					}
					else{
								ParticleList[i].position  =  Spline_to_Conform_Curve_pos[counter];

						//size
						if(Reduce_factor != 1 & Application.isPlaying){   
							
							ParticleList[i].startSize = Reduce_factor*counter;
							
						}

						for(int j=0;j<Interpolate_steps;j++){

									if(counter <= Spline_to_Conform_Curve_pos.Count-Interpolate_steps){
								if((i+j+1)  < (ParticleList.Length-1)){
											Vector3 POS1 = Spline_to_Conform_Curve_pos[counter] ;
											Vector3 TRANSLATED = Spline_to_Conform_Curve_pos[counter+2-1];
									ParticleList[i+j+1].position  = POS1 + (j+1)*(POS1 -TRANSLATED)/(Interpolate_steps) ;

									//size
									if(Reduce_factor != 1 & Application.isPlaying){   
										
										ParticleList[i+j+1].startSize = Reduce_factor*(counter+j);
										
									}

									//v1.3
									if(extend_life){													
										ParticleList[i+j+1].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];
									
									if(ParticleList[i+j+1].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
										ParticleList[i+j+1].startLifetime = tileCount;
									}
									}

									Particle_count++;
								}
							}
						}
					}
					
					//GAMEOBJECT MODE
					if(Gameobject_mode){
						if(follow_particles){
							//if(Gameobj_instances.Count-1 > i ){
							if(Count_objects < Gameobj_instances.Count){
										Gameobj_instances[Count_objects].transform.position = ParticleList[i].position + GameObj_Offset;								
								//remove colliders
										if(Gameobj_instances[Count_objects].GetComponent<Collider>() !=null){
									if(Remove_colliders){										
												Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = false;										
									}
									else if(!Remove_colliders){										
												Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = true;										
									}
								}
							}
									Count_objects++;
						}
					}

					counter=counter+1;
							if(counter > Spline_to_Conform_Curve_pos.Count-2) 
					{
						
						counter=0;
						
					}
											
				}
			  }else{ //normal interpolate

					if(!Variable_step){
						for (int i=0; i < ParticleList.Length;i=i+Interpolate_steps)
						{
								Particle_count++;

							//v1.3
							int tileCount=15;
							//Random.seed = i; //set to give same number for same i
								Random.InitState(i);//v2.3
							int Sheet_tile = Random.Range(1,15);//4x4 sheet
							if(extend_life){									

									ParticleList[i].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];									
							
							if(ParticleList[i].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
									ParticleList[i].startLifetime = tileCount;
							}
							}

							if (relaxed){

									//v1.3
									if( ParticleList[i].remainingLifetime > ParticleList[i].startLifetime*keep_in_position_factor | 1==0){
										ParticleList[i].position  =  Spline_to_Conform_Curve_pos[counter];
									}									
									for(int j=0;j<Interpolate_steps;j++){
										if(counter <= Spline_to_Conform_Curve_pos.Count-2){
											if((i+j+1)  < (ParticleList.Length-1)){
												if( ParticleList[i+j+1].remainingLifetime > ParticleList[i+j+1].startLifetime*keep_in_position_factor | 1==0){
													ParticleList[i+j+1].position  = Spline_to_Conform_Curve_pos[counter] - (j+1)*(Spline_to_Conform_Curve_pos[counter] -Spline_to_Conform_Curve_pos[counter+1])/(Interpolate_steps) ;
												}
												if(extend_life){													
													ParticleList[i+j+1].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];
												
												if(ParticleList[i+j+1].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
													ParticleList[i+j+1].startLifetime = tileCount;
												}
												}

												Particle_count++;
											}
										}
									}



									if(Gravity){

										ParticleList[i].position  =Vector3.Slerp(	ParticleList[i].position,  Spline_to_Conform_Curve_pos[counter],Return_Speed);
										
										for(int j=0;j<Interpolate_steps;j++){
											
											if(counter <= Spline_to_Conform_Curve_pos.Count-2){
												if((i+j+1)  < (ParticleList.Length-1)){
													ParticleList[i+j+1].position  =Vector3.Slerp(ParticleList[i+j+1].position , Spline_to_Conform_Curve_pos[counter] - (j+1)*(Spline_to_Conform_Curve_pos[counter] -Spline_to_Conform_Curve_pos[counter+1])/(Interpolate_steps),Return_Speed) ;
												}
											}
										}
									}


							}
							else{
									ParticleList[i].position  =  Spline_to_Conform_Curve_pos[counter];
								
									//v1.8
									if(Look_at_spline){
										ParticleList[i].axisOfRotation = Rot_axis;
										//int Angle_split = 1;
										if(counter+Angle_split < Spline_to_Conform_Curve_pos.Count){
											Vector3 Diff = Spline_to_Conform_Curve_pos[counter] - Spline_to_Conform_Curve_pos[counter+Angle_split];
											float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));

											if(Diff.z > 0){
												angle = - angle;
											}
											ParticleList[i].rotation = Rotation_offset + angle;
										}else{											
											//Vector3 Diff = Spline_to_Conform.Curve[i].position - Spline_to_Conform.Curve[Spline_to_Conform.Curve.Count-(Angle_split+1)].position;
											//float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
											//ParticleList[i].rotation = Rotation_offset + angle;											
										}
									}

								//size
								if(Reduce_factor != 1 & Application.isPlaying){   
									
									ParticleList[i].startSize = Reduce_factor*counter;
									
								}
								
								for(int j=0;j<Interpolate_steps;j++){
									
										if(counter <= Spline_to_Conform_Curve_pos.Count-2){//Interpolate_steps){
										if((i+j+1)  < (ParticleList.Length-1)){
												Vector3 POS1 = Spline_to_Conform_Curve_pos[counter] ;
												Vector3 TRANSLATED = Spline_to_Conform_Curve_pos[counter+1];
											ParticleList[i+j+1].position  = POS1 - (j+1)*(POS1 -TRANSLATED)/(Interpolate_steps) ;
											
												//v1.8
												if(Look_at_spline){
													ParticleList[i+j+1].axisOfRotation = Rot_axis;
													//int Angle_split = 1;
													if(counter+Angle_split < Spline_to_Conform_Curve_pos.Count){
														Vector3 Diff = Spline_to_Conform_Curve_pos[counter] - Spline_to_Conform_Curve_pos[counter+Angle_split];
														float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
														
														if(Diff.z > 0){
															angle = - angle;
														}
														ParticleList[i+j+1].rotation = Rotation_offset + angle;
													}else{											
														//Vector3 Diff = Spline_to_Conform.Curve[i].position - Spline_to_Conform.Curve[Spline_to_Conform.Curve.Count-(Angle_split+1)].position;
														//float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
														//ParticleList[i].rotation = Rotation_offset + angle;											
													}
												}

											//size
											if(Reduce_factor != 1 & Application.isPlaying){   
												
												ParticleList[i+j+1].startSize = Reduce_factor*(counter+j);
												
											}

											//v1.3
											if(extend_life){													
												ParticleList[i+j+1].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];
											//}
											if(ParticleList[i+j+1].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
												ParticleList[i+j+1].startLifetime = tileCount;
											}
											}
											
											//v1.8
											//GAMEOBJECT MODE
											if(Gameobject_mode){
												if(follow_particles){
													//if(Gameobj_instances.Count-1 > i+j+1 ){
													if(Count_objects < Gameobj_instances.Count){
															Gameobj_instances[Count_objects].transform.position = ParticleList[i+j+1].position + GameObj_Offset;										
														//remove colliders
															if(Gameobj_instances[Count_objects].GetComponent<Collider>() !=null){
															if(Remove_colliders){												
																	Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = false;												
															}
															else if(!Remove_colliders){												
																	Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = true;												
															}
														}
													}
														Count_objects++;
												}
											}//gameobject

										}
									}
									
								}
							}
							
							//GAMEOBJECT MODE
							if(Gameobject_mode){
								if(follow_particles){
									//if(Gameobj_instances.Count-1 > i ){
									if(Count_objects < Gameobj_instances.Count){
											Gameobj_instances[Count_objects].transform.position = ParticleList[i].position + GameObj_Offset;										
										//remove colliders
											if(Gameobj_instances[Count_objects].GetComponent<Collider>() !=null){
											if(Remove_colliders){												
													Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = false;												
											}
											else if(!Remove_colliders){												
													Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = true;												
											}
										}
									}
										Count_objects++;
								}
							}

							counter=counter+1;
								if(counter > Spline_to_Conform_Curve_pos.Count-2) 
							{
								
								counter=0;
								
							}
							
						}
					}else{//variable step

							int Particle_start = 0;
							float Particle_size = 0.01f;
							if(p2.main.startSize.constant > 0){Particle_size = p2.main.startSize.constant;}

							int Particle_end = (int)(Interpolate_steps * (Spline_to_Conform_Curve_pos[1]-Spline_to_Conform_Curve_pos[0]).magnitude/Particle_size);

							if(Particle_end - Particle_start < 1 ){Particle_end = Particle_start+1;}



							for (int i=0; i < Spline_to_Conform_Curve_pos.Count-1;i=i+1)
							{

								int particle_count_temp = Particle_end - Particle_start;

								for (int j=Particle_start; j < Particle_end ;j++)
								{
									if(j < ParticleList.Length){

										Particle_count++;

										//v1.3
										int tileCount=15;
										//Random.seed = i; //set to give same number for same i
										Random.InitState(i);//v2.3
										int Sheet_tile = Random.Range(1,15);//4x4 sheet
										if(extend_life){
											

											ParticleList[j].remainingLifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];

										
										if(ParticleList[j].remainingLifetime < ParticleList[j].startLifetime*keep_alive_factor){
											ParticleList[j].startLifetime = tileCount;
										}
										}

										if (relaxed){


											//v1.3
											if(ParticleList[j].remainingLifetime > ParticleList[j].startLifetime*keep_in_position_factor ){
												ParticleList[j].position  =  Spline_to_Conform_Curve_pos[i] + (j+1-Particle_start)*(Spline_to_Conform_Curve_pos[i+1] -Spline_to_Conform_Curve_pos[i])/(particle_count_temp) ;
											}


											if(Gravity){

												ParticleList[j].position  =Vector3.Slerp(ParticleList[j].position ,  Spline_to_Conform_Curve_pos[i] + (j+1-Particle_start)*(Spline_to_Conform_Curve_pos[i+1] -Spline_to_Conform_Curve_pos[i])/(particle_count_temp),Return_Speed) ;

											}

										}else{

											ParticleList[j].position  =  Spline_to_Conform_Curve_pos[i] + (j+1-Particle_start)*(Spline_to_Conform_Curve_pos[i+1] -Spline_to_Conform_Curve_pos[i])/(particle_count_temp) ;

											//size
											if(Reduce_factor != 1 & Application.isPlaying){   
												
												ParticleList[j].startSize = Reduce_factor*(j);
												
											}

										}

										//v1.8
										if(Look_at_spline){
											ParticleList[j].axisOfRotation = Rot_axis;
											//int Angle_split = 2;
											if(i+Angle_split < Spline_to_Conform_Curve_pos.Count){
												Vector3 Diff = Spline_to_Conform_Curve_pos[i] - Spline_to_Conform_Curve_pos[i+Angle_split];
												//Diff = new Vector3(Mathf.Abs(Diff.x),Mathf.Abs(Diff.y),Mathf.Abs(Diff.z));
												float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
												//if(angle>180){
												if(Diff.z > 0){

													//Debug.Log (angle);
													//angle =  180 - angle;
													angle = - angle;
												}
												ParticleList[j].rotation = Rotation_offset + angle;
											}else{

												Vector3 Diff = Spline_to_Conform_Curve_pos[i] - Spline_to_Conform_Curve_pos[Spline_to_Conform_Curve_pos.Count-(Angle_split+1)];
												//Diff = new Vector3(Mathf.Abs(Diff.x),Mathf.Abs(Diff.y),Mathf.Abs(Diff.z));
												float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
												//if(angle>180){
												//if(Diff.z > 0){
													
													//Debug.Log (angle);
													//angle =  180 - angle;
													//angle =  180-angle;
												//}
												ParticleList[j].rotation = Rotation_offset + angle;

													//ParticleList[j].rotation = Rotation_offset +  Mathf.Abs(Vector3.Angle(Rot_angle_axis,
												    //                                                                 Spline_to_Conform.Curve[i].position - Spline_to_Conform.Curve[Spline_to_Conform.Curve.Count-(Angle_split+1)].position));
											}
										}
									}

								}

								Particle_start = Particle_end;

								//GAMEOBJECT MODE
								if(Gameobject_mode){
									if(follow_particles){
										//if(Gameobj_instances.Count-1 > i ){
										if(Count_objects < Gameobj_instances.Count){
											Gameobj_instances[Count_objects].transform.position = ParticleList[i].position + GameObj_Offset;											
											Gameobj_instances[Count_objects].name = i.ToString();
											//remove colliders
											if(Gameobj_instances[Count_objects].GetComponent<Collider>() !=null){
												if(Remove_colliders){													
													Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = false;													
												}
												else if(!Remove_colliders){													
													Gameobj_instances[Count_objects].GetComponent<Collider>().enabled = true;													
												}
											}
										}
										Count_objects++;
									}
								}

								if(p2.main.startSize.constant > 0){Particle_size = p2.main.startSize.constant;}

								if(i < Spline_to_Conform_Curve_pos.Count-2){
									Particle_end = Particle_start +(int)(Interpolate_steps * (Spline_to_Conform_Curve_pos[i+2]-Spline_to_Conform_Curve_pos[i+1]).magnitude/Particle_size);
								}

								if(Particle_end - Particle_start < 1 ){Particle_end = Particle_start+1;}
							
							}

							//ParticleList[ParticleList.Length-1].position  =  Spline_to_Conform_Curve_pos[0];

					}//end variable step

				}//end normal interpolate
			}//END INTERPOLATE

				p2.SetParticles(ParticleList,p11.particleCount);

		}

	}
}
}