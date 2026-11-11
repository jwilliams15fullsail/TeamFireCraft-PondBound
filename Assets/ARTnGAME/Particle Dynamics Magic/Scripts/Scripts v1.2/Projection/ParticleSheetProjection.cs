using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
public class ParticleSheetProjection : MonoBehaviour {

	void Start () {

			if(p2==null){
				p2=this.gameObject.GetComponent<ParticleSystem>();

				if(p2!=null){
					ParticleSystem.MainModule main = p2.main;//v2.3
					main.maxParticles=particle_count;
				p2.Emit(particle_count);
				}

			}

			if(p2!=null){
		aaa = new ParticleSystem.Particle[p2.particleCount];
			}else{
				Debug.Log ("Please attach script to a particle system");
			}

		Registered_paint_positions = new List<Vector3>();
			Registered_paint_rotations = new List<Vector3>();

			keep_extend=extend;

			KTiles_X=Tiles_X;
			KTiles_Y=Tiles_Y;

			keep_count = particle_count;
	}

	public int particle_count = 100;
		private int keep_count;
	
	bool got_positions=false;
	bool got_random_offsets=false;
	public bool go_random=false;

	Vector3[] positions;
	Vector2[] rand_offsets;
	int[] tile;

	private ParticleSystem.Particle[] aaa;
	public ParticleSystem p2;

		private bool randomize=false;
	public float extend=1f;
	private List<Vector3> Registered_paint_positions; 
		private List<Vector3> Registered_paint_rotations; //v1.3

		private float keep_extend;

	public float Y_offset=0f;

	public bool fix_initial = false;
	private bool let_loose = false;
	public bool letloose = false;
	private int place_start_pos;

	public bool Gravity_Mode=false;

		public int Tiles_X=4;
		public int Tiles_Y=4;
		
		private int KTiles_X=4;
		private int KTiles_Y=4;

		public bool Origin_at_Projector=false; //v1.3
		public GameObject Projector_OBJ;//v1.3
		public bool follow_normals=false;//v1.3
		public bool Transition=false;//v1.3
		public float return_speed=0.005f;//v1.3

	void Update () {

			if(p2==null){return;}



		let_loose = letloose;
		if(!Application.isPlaying){
			
				if(p2.main.maxParticles!=particle_count){
					ParticleSystem.MainModule main = p2.main;//v2.3
					main.maxParticles=particle_count;
				}

				p2.Emit(particle_count);
				aaa = new ParticleSystem.Particle[p2.particleCount];


			//let_loose = false;
			
		}

			//v1.3
			if(p2!=null){
				if(!Transition){
					if(p2.main.maxParticles!=particle_count){
						ParticleSystem.MainModule main = p2.main;//v2.3
						main.maxParticles=particle_count;
						p2.Emit(particle_count);
					}
				}
				if(Transition){
					if(p2.particleCount!=p2.main.maxParticles){
						//p2.maxParticles=particle_count;
						p2.Emit(p2.main.maxParticles);
					}
					place_start_pos=1;
				}
			}
			if(Registered_paint_positions==null){
				Registered_paint_positions = new List<Vector3>();
			}
			if(Registered_paint_rotations==null){
				Registered_paint_rotations = new List<Vector3>();
			}





			if( KTiles_X!=Tiles_X | KTiles_Y!=Tiles_Y){
				Start ();
			}

			if(keep_count != particle_count){

				ParticleSystem.MainModule main = p2.main;//v2.3
				main.maxParticles=particle_count;
				keep_count = particle_count;
				got_random_offsets=false;
				got_positions=false;
			}

			if(keep_extend!=extend){

				keep_extend=extend;
				got_random_offsets=false;
				got_positions=false;
			}

		List<Vector3> ray_dest_positions = new List<Vector3>(); 

		if(p2.particleCount < particle_count){

		}

			if(!let_loose){
				if(Application.isPlaying){
					p2.Clear();
				}

			}

			if(p2.main.maxParticles != particle_count){
				ParticleSystem.MainModule main = p2.main;//v2.3
				main.maxParticles = particle_count;
			}

		p2.Emit(particle_count);

		
			int tileCount = Tiles_X*Tiles_Y-1;  //15;

		p2.GetParticles(aaa);

		if(!got_positions){
			
				positions = new Vector3[aaa.Length];
			
				tile = new int[aaa.Length];
			got_positions = true;
			
			for(int i=0;i<aaa.Length;i++){
				positions[i] = aaa[i].position;
				tile[i] = Random.Range(0,15);
			}
		}


			//ORIGIN v1.3
			Vector3 Position_at_origin = this.transform.position;
			Quaternion Rotation_at_origin =  this.transform.parent.rotation;
			if(Origin_at_Projector){
				if(Projector_OBJ!=null){
					Rotation_at_origin = Projector_OBJ.transform.rotation;
					Position_at_origin = Projector_OBJ.transform.position;
				}else{
					Debug.Log ("Please insert the projector gameobject to the script");
				}
			}


		// PROJECTION

			int max_positions = (int)p2.main.maxParticles/2;//v2.1
		max_positions = particle_count/2;
		
		ray_dest_positions.Clear();
		
		int GET_X = (int)Mathf.Sqrt(max_positions);

		if(!got_random_offsets){
			
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
						ray_dest_positions.Add(Rotation_at_origin*new Vector3(Position_at_origin.x - X_AXIS, Position_at_origin.y-1000,Position_at_origin.z - Z_AXIS  ));
						ray_dest_positions.Add(Rotation_at_origin*new Vector3(Position_at_origin.x + X_AXIS, Position_at_origin.y-1000,Position_at_origin.z - Z_AXIS ));
				}

				if(go_random){

						ray_dest_positions.Add(Rotation_at_origin*new Vector3(Position_at_origin.x - rand_offsets[count_2].x, Position_at_origin.y-1000,Position_at_origin.z - rand_offsets[count_2].y  ));
					count_2=count_2+1;
				}

			}
		}
		
		if(!fix_initial){
			Registered_paint_positions.Clear();
			Registered_paint_rotations.Clear();
		}

		if(Registered_paint_positions!=null){
			if(Registered_paint_positions.Count > (particle_count/2) ){
				//do nothing
			}else{

				for (int k=0;k<ray_dest_positions.Count;k++)
				{
										
					//Vector3 ORIGIN = this.transform.position;
					Vector3 ORIGIN = Position_at_origin;//v1.3
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


	


		//END PROJECTION
		
		int count_positions=0;
		for(int i=0;i<aaa.Length;i++){

			aaa[i].remainingLifetime = tileCount + 1 - tile[i];
			aaa[i].startLifetime = tileCount;

			if(!let_loose){
				aaa[i].angularVelocity =0;
				aaa[i].rotation =0;
				aaa[i].velocity=Vector3.zero;
			}

			aaa[i].startColor = Color.white;

			if(!let_loose | (place_start_pos<1)){
				
					if(count_positions < Registered_paint_positions.Count){
						aaa[i].position = Registered_paint_positions[count_positions] + new Vector3(i*0.005f,Y_offset,i*0.007f);
					}

					//v1.3
					if(follow_normals){
						
						aaa[i].rotation = 90;
						
						float FIX_for_Z = 90;
						
						Quaternion FIX_ROT = Quaternion.identity;// emitter.transform.rotation;
						Vector3 FIXED_NORMAL = FIX_ROT*Registered_paint_rotations[count_positions];
						
						if(FIXED_NORMAL.z >=0 & FIXED_NORMAL.x >=0){
							FIX_for_Z = FIX_for_Z - Vector3.Angle(new Vector3(1,0,0),new Vector3(FIXED_NORMAL.x,0,FIXED_NORMAL.z) ); //- Mathf.Abs((vertices[i].z-normals[i].z));
						}
						if(FIXED_NORMAL.z <0 & FIXED_NORMAL.x >=0){
							FIX_for_Z = FIX_for_Z + Vector3.Angle(new Vector3(1,0,0),new Vector3(FIXED_NORMAL.x,0,FIXED_NORMAL.z) ); //- Mathf.Abs((vertices[i].z-normals[i].z));
						}
						
						if(FIXED_NORMAL.z >=0 & FIXED_NORMAL.x <0){
							FIX_for_Z = FIX_for_Z + Vector3.Angle(new Vector3(1,0,0),new Vector3(FIXED_NORMAL.x,0,FIXED_NORMAL.z) )+180; //- Mathf.Abs((vertices[i].z-normals[i].z));
						}
						if(FIXED_NORMAL.z <0 & FIXED_NORMAL.x <0){
							FIX_for_Z = FIX_for_Z - Vector3.Angle(new Vector3(1,0,0),new Vector3(FIXED_NORMAL.x,0,FIXED_NORMAL.z) )+180; //- Mathf.Abs((vertices[i].z-normals[i].z));
						}
						
						float DOT = Vector3.Dot(new Vector3(0.0000001f,0.0000001f,1),FIXED_NORMAL);
						float ACOS = 0f; 
						if(DOT >1 | DOT <-1){
						}else{
							ACOS= Mathf.Acos(DOT);
						}
						
						aaa[i].rotation = ACOS+FIX_for_Z;
						aaa[i].axisOfRotation =  Vector3.Cross( new Vector3(0.00000001f,0.00000001f,1), FIXED_NORMAL);
						
					}


			}

			//Gravity
			if(let_loose & Gravity_Mode){

					if(count_positions < Registered_paint_positions.Count){
						aaa[i].position = Vector3.Slerp(aaa[i].position, Registered_paint_positions[count_positions]+ new Vector3(i*0.005f,Y_offset,i*0.007f),return_speed);
					}

				aaa[i].velocity= Vector3.Slerp(aaa[i].velocity,Vector3.zero,0.05f);
			}

			if(count_positions>Registered_paint_positions.Count-2){
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