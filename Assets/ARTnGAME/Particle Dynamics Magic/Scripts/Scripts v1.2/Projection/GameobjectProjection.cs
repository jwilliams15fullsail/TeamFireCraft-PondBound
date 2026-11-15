using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
	public class GameobjectProjection : MonoBehaviour {

	void Start () {

			if(p2==null){
				p2=this.gameObject.GetComponent<ParticleSystem>();

				if(p2!=null){

					ParticleSystem.MainModule main = p2.main;//v2.3
					main.maxParticles = particle_count;
				p2.Emit(particle_count);
				}

			}
	
		if(p2 !=null){
			aaa = new ParticleSystem.Particle[p2.particleCount];
		}

		Particle_Num = particle_count;

		Registered_paint_positions = new List<Vector3>();
		Registered_paint_rotations = new List<Vector3>();

			//v1.2.2
			Updated_gameobject_positions = new List<Vector3>();

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

			starting_extend=extend;
		}

			colliders_last_status = Remove_colliders;



	}

	public bool Preview_mode=false;

	public int particle_count = 100;

	bool got_positions=false;
	bool got_random_offsets=false;
	public bool go_random=false;

	Vector3[] positions;
	Vector2[] rand_offsets;
	int[] tile;

	private ParticleSystem.Particle[] aaa;
	public ParticleSystem p2;

	[HideInInspector]
	public bool randomize=false;
	public float extend=1f;
	private float starting_extend;

		private List<Vector3> Registered_paint_positions; 
		private List<Vector3> Registered_paint_rotations; 
		//v1.2.2
		private List<Vector3> Updated_gameobject_positions;

		private List<GameObject> Gameobj_instances;

	public GameObject Gameobj;


	public float Y_offset=0f;

	public bool fix_initial = false;
	private bool let_loose = false;
	public bool letloose = false;
	private int place_start_pos;

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
		//v1.2.2
		public bool Look_at_direction=false;

		private bool colliders_last_status=false;

	void Update () {

		if(Parent_OBJ==null | Gameobj == null | p2 == null){
				Debug.Log ("Please add a pool gameobject and a gameobject to be emitted");
			return;
		}

			//v1.2.2
			if(Gameobj_instances !=null & Updated_gameobject_positions !=null){

				for(int i=0;i<Gameobj_instances.Count;i++){

					Updated_gameobject_positions[i] = Gameobj_instances[i].transform.position;

				}

			}


			if(Application.isPlaying){Preview_mode=false;}


				
				
					if(Remove_colliders & colliders_last_status ==false){
						for(int i=Parent_OBJ.transform.childCount-1;i>=0;i--){

							if(Parent_OBJ.transform.GetChild(i).gameObject.GetComponent<Collider>()!=null){
								Parent_OBJ.transform.GetChild(i).gameObject.GetComponent<Collider>().enabled=false;
							}
						}
						Debug.Log ("did once");
						colliders_last_status = true;
					}
					if(!Remove_colliders & colliders_last_status==true){
						for(int i=Parent_OBJ.transform.childCount-1;i>=0;i--){

							if(Parent_OBJ.transform.GetChild(i).gameObject.GetComponent<Collider>()!=null){
								Parent_OBJ.transform.GetChild(i).gameObject.GetComponent<Collider>().enabled=true;
							}
						}
						Debug.Log ("did once 1");
						colliders_last_status = false;
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



			if(p2==null){
				p2=this.gameObject.GetComponent<ParticleSystem>();
			}

		if(aaa == null){

			aaa = new ParticleSystem.Particle[p2.particleCount];

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

		if(Particle_Num != particle_count){ 
			got_random_offsets = false;
			got_positions=false;
			Particle_Num = particle_count;
				p2.Clear();

				ParticleSystem.MainModule main = p2.main;//v2.3
				main.maxParticles = particle_count;
		}

			if(p2.main.maxParticles != particle_count){
				got_random_offsets = false;
				got_positions=false;
				Particle_Num = p2.main.maxParticles;
				p2.Clear();
				particle_count = p2.main.maxParticles;
			}

		let_loose = letloose;
		if(!Application.isPlaying){ 
			
				aaa = new ParticleSystem.Particle[p2.particleCount];

			//let_loose = false;

			if(noise ==null){
				noise = new PerlinPDM ();
			}
			
		}
		

		List<Vector3> ray_dest_positions = new List<Vector3>(); 


		if(p2.particleCount < particle_count){

		}
		p2.Emit(particle_count);

		int tileCount=15;

		p2.GetParticles(aaa);

		if(!got_positions){
			positions = new Vector3[particle_count];
			tile = new int[particle_count];
			got_positions = true;
			
			for(int i=0;i<aaa.Length;i++){

				positions[i] = aaa[i].position;
				tile[i] = Random.Range(0,15);
			}
		}


		// PROJECTION

			int max_positions = (int)p2.main.maxParticles/2;//v2.1
		max_positions = particle_count/2;
		
		ray_dest_positions.Clear();
		
		int GET_X = (int)Mathf.Sqrt(max_positions);

		bool enter_me=false;
		if(starting_extend != extend){
			enter_me=true;
			starting_extend=extend;
		}

		if(!got_random_offsets | enter_me){
			
			got_random_offsets=true;
			
			rand_offsets=new Vector2[GET_X*GET_X];

			int count_me=0;
			for (int m=0;m<GET_X;m++){
				for (int n=0;n<GET_X;n++){

					rand_offsets[count_me] = new Vector2(Random.Range(0,extend*(GET_X/2)*m)+(1.5f*count_me), Random.Range(0,extend*(GET_X/2)*n)+(1.1f*count_me)  );
						count_me=count_me+1;
				}
			}
			
		}
		int count_2=0;
		for (int m=0;m<GET_X;m++){
			for (int n=0;n<GET_X;n++){
				
				float X_AXIS = (extend*(GET_X/2)*m);
				float Z_AXIS = (extend*(GET_X/2)*n);
				if(randomize){
					X_AXIS = X_AXIS+Random.Range(0,extend*(GET_X/2)*m);
					Z_AXIS = Z_AXIS+Random.Range(0,extend*(GET_X/2)*n);
				}

				if(!go_random){
					ray_dest_positions.Add(this.transform.parent.rotation*new Vector3(this.transform.position.x - X_AXIS, this.transform.position.y-1000,this.transform.position.z - Z_AXIS  ));
					ray_dest_positions.Add(this.transform.parent.rotation*new Vector3(this.transform.position.x + X_AXIS, this.transform.position.y-1000,this.transform.position.z - Z_AXIS ));
				}

				if(go_random){

					ray_dest_positions.Add(this.transform.parent.rotation*new Vector3(this.transform.position.x - rand_offsets[count_2].x, this.transform.position.y-1000,this.transform.position.z - rand_offsets[count_2].y  ));
					

					count_2=count_2+1;
				}

			
				}
		}
		

		if(!fix_initial){
			Registered_paint_positions.Clear();
			Registered_paint_rotations.Clear();
		}

		if(Registered_paint_positions!=null){
			if(Registered_paint_positions.Count > (particle_count/2) ){			//do nothing
			}else{

				for (int k=0;k<ray_dest_positions.Count;k++)
				{
					
					
					Vector3 ORIGIN = this.transform.position;
					Vector3 DEST = ray_dest_positions[k];
					
					RaycastHit hit = new RaycastHit();
					if (Physics.Raycast(ORIGIN,DEST, out hit, Mathf.Infinity))
					{
						
						if(Registered_paint_positions!=null){
							
							if(Registered_paint_positions.Count > (particle_count/2) ){
								//do nothing
							}else{
								Registered_paint_positions.Add(hit.point);

								
									Registered_paint_rotations.Add (hit.normal);
																
							}
						}
					}
				}

			}
		}


	
		if(Preview_mode | Application.isPlaying){

		if(Registered_paint_positions!=null){
		

			
				if(!follow_particles){

					for(int i=0;i<Registered_paint_positions.Count;i++){

						if(Gameobj_instances.Count < (particle_count/2)){
							GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[i],Quaternion.identity)as GameObject;

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
								//v1.2.2
								Updated_gameobject_positions.Add (TEMP.transform.position);
						}
					}
				}

				if(follow_particles){

					for(int i=0;i<aaa.Length-1;i=i+2){
						
						if(Gameobj_instances.Count < (particle_count/2) & Registered_paint_positions.Count > (i+2) ){
							GameObject TEMP = Instantiate(Gameobj,Registered_paint_positions[i],Quaternion.identity)as GameObject;
							
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
								//v1.2.2
								Updated_gameobject_positions.Add (TEMP.transform.position);
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
				
				}else{
							//v1.2.2
							if(Look_at_direction){
								//Do nothing
							}
							else{
								Gameobj_instances[i].transform.rotation = Quaternion.identity;
							}
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
								Quaternion rot = Quaternion.identity;

								if((i-Registered_paint_rotations.Count)<Registered_paint_rotations.Count){
									rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Registered_paint_rotations[i-Registered_paint_rotations.Count]);
								}else{
									rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up,Registered_paint_rotations[0]);
								}
							
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
								//v1.2.2
								if(Look_at_direction){
									//Do nothing
								}
								else{
							Gameobj_instances[i].transform.rotation = Quaternion.identity;
								}
						}
					}

			}
		}
	}


		int count_positions=0;

		for(int i=0;i<aaa.Length;i++){

		}


		if(1==1){
		for(int i=0;i<aaa.Length;i++){

			aaa[i].remainingLifetime = tileCount + 1 - tile[i];
			aaa[i].startLifetime = tileCount;

			//restore size, color, lifetime from vortexes
			if(!let_loose){
				aaa[i].angularVelocity =0;
				aaa[i].rotation =0;
				aaa[i].velocity=Vector3.zero;
			}
						
			aaa[i].startColor = Color.white;
			
			if(!let_loose | (place_start_pos<1)){
				
					if(Registered_paint_positions.Count > count_positions){
						aaa[i].position = Registered_paint_positions[count_positions] + new Vector3(i*0.005f,Y_offset,i*0.007f);
					}
				
			}

			//Gravity
			if(let_loose & Gravity_Mode){
				
					if(Registered_paint_positions.Count > count_positions){
						aaa[i].position = Vector3.Slerp(aaa[i].position, Registered_paint_positions[count_positions]+ new Vector3(i*0.005f,Y_offset,i*0.007f),Return_speed);
					}
			
				aaa[i].velocity= Vector3.Slerp(aaa[i].velocity,Vector3.zero,0.05f);
			}

			if(follow_particles){
					if(Gameobj_instances.Count > i ){ //v1.2.2 fix (-1 to 0)
						Gameobj_instances[i].transform.position = aaa[i].position;

						//v1.2.2
							Vector3 Motion_vec = Gameobj_instances[i].transform.position - Updated_gameobject_positions[i];
							Quaternion New_rot = Quaternion.identity;
							//if(Motion_vec != Vector3.zero){
							if(Motion_vec.magnitude >0.1f){ //v1.3.5
								New_rot = Quaternion.LookRotation(1*Motion_vec);
								Gameobj_instances[i].transform.rotation = Quaternion.Slerp(Gameobj_instances[i].transform.rotation,New_rot,Time.deltaTime*16);
								//Debug.DrawLine(Gameobj_instances[i].transform.position,Updated_gameobject_positions[i]);
							}


						//remove colliders
						if(Gameobj_instances[i].GetComponent<Collider>() !=null){
							if(Remove_colliders){

								Gameobj_instances[i].GetComponent<Collider>().enabled = false;
								
							}
							else if(!Remove_colliders){
								
								Gameobj_instances[i].GetComponent<Collider>().enabled = true;
								
							}
						}

					}
			}

			if(count_positions>Registered_paint_positions.Count-1){ //v1.2.2 fix, -2 to -1
				count_positions=0;
			}else{count_positions=count_positions+1;}

		
		}

		if(place_start_pos <1){
			place_start_pos = place_start_pos+1;
		}

		
		p2.SetParticles(aaa,aaa.Length);
		}
	}
}
}