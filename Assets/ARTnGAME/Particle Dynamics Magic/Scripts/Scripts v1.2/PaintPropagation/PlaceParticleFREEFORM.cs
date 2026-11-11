using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
public class PlaceParticleFREEFORM : MonoBehaviour {
	
	void Start () {

		Grab_time=Time.fixedTime;
	}

	void Awake () {
		p11 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

			if(p11==null){
				Debug.Log ("Please attach the script to a particle system");
			}

		Flammable_objects = GameObject.FindGameObjectsWithTag("Flammable");

		Flamer_objects = GameObject.FindGameObjectsWithTag("Flamer");

		Registered_enflamed_positions = new List<Vector2>();

	}


	 GameObject[] Flammable_objects;

	 GameObject[] Flamer_objects;

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
	public List<Vector3> Registered_initial_positions;
	[HideInInspector]
	public List<Vector3> Registered_initial_rotation;
	[HideInInspector]
	public List<Vector3> Registered_initial_scale;

	ParticleSystem.Particle[] ParticleList;

	private float Grab_time;
	public float Delay=1;
	public bool Optimize=false;

	public bool relaxed = true;
	
	public bool draw_in_sequence;

	void Update () {

		if(p11 == null){return;}

			if(Registered_paint_positions!=null){
		for (int i=Registered_paint_positions.Count-1;i>=0 ;i--){
			if(Emitter_objects[i] == null)
			{
				
				Registered_paint_positions.RemoveAt(i);
				Registered_initial_positions.RemoveAt(i);
				Registered_initial_rotation.RemoveAt(i);
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

		if(Time.fixedTime-Grab_time>Delay | !Optimize){

			Grab_time=Time.fixedTime;

		Flamer_objects = GameObject.FindGameObjectsWithTag("Flamer");
		Flammable_objects = GameObject.FindGameObjectsWithTag("Flammable");

		if(Application.isPlaying){
		for(int i=0;i<Flammable_objects.Length;i++){

					if(Emitter_objects!=null){
			for(int j=0;j<Emitter_objects.Count;j++){
				if( Emitter_objects[j] != null & Flammable_objects[i]!= null  ){
				if( Vector3.Distance(Emitter_objects[j].transform.position,Flammable_objects[i].transform.position) <6f &
				   Vector3.Distance(Emitter_objects[j].transform.position,Flammable_objects[i].transform.position) >3f ){ 


					Vector3 point = Emitter_objects[j].transform.position; 

					point = Flammable_objects[i].transform.InverseTransformPoint(point);
					
					Vector3 nearestVertex = Vector3.zero;
					
					RaycastHit hit1 = new RaycastHit();
					if(Physics.Raycast(Emitter_objects[j].transform.position, (Flammable_objects[i].transform.position - Emitter_objects[j].transform.position ), out hit1,Mathf.Infinity))
					{

							if(hit1.collider.gameObject.tag == "Flammable"){

								nearestVertex = hit1.point;
							}else{nearestVertex = Vector3.zero;}
					}

					Vector3 result = Flammable_objects[i].transform.TransformPoint(nearestVertex);
					result =nearestVertex;

					if(result != Vector3.zero){
					if(Emitter_objects.Count > (p11.main.maxParticles/2) ){ //v2.1 if(Emitter_objects.Count > (p11.emission.rate.curveScalar/2) ){ //v2.3
						//do nothing
					}else{

						int is_close_to_other_point_on_object=0;

						for(int k=0;k<Registered_enflamed_positions.Count;k++){
							Vector2 ADD_ITEM1 = Registered_enflamed_positions[k];

							if(ADD_ITEM1.x == i){

								if(Vector3.Distance( Registered_paint_positions[(int)ADD_ITEM1.y-1],result )<3f){
									is_close_to_other_point_on_object=1;
								}
							}
						}

						if(is_close_to_other_point_on_object==0){ 

							Registered_paint_positions.Add(result);

									Emitter_objects.Add(hit1.collider.gameObject);
									Registered_initial_positions.Add(hit1.collider.gameObject.transform.position);
									Registered_initial_scale.Add (hit1.collider.gameObject.transform.localScale);
									Registered_initial_rotation.Add(hit1.collider.gameObject.transform.eulerAngles);

							Vector2 ADD_ITEM = new Vector2(i,Registered_paint_positions.Count);

							Registered_enflamed_positions.Add(ADD_ITEM);

						}
					}
				}

				}
			}
					}
			} 

			if(Flamer_objects!=null){
			for(int j=0;j<Flamer_objects.Length;j++){

					if( Vector3.Distance(Flamer_objects[j].transform.position,Flammable_objects[i].transform.position) <6f &
					   Vector3.Distance(Flamer_objects[j].transform.position,Flammable_objects[i].transform.position) >3f ){ 
						
						Vector3 point = Flamer_objects[j].transform.position; 
						
						point = Flammable_objects[i].transform.InverseTransformPoint(point);

						Vector3 nearestVertex = Vector3.zero;

						RaycastHit hit1 = new RaycastHit();
						if(Physics.Raycast(Flamer_objects[j].transform.position, (Flammable_objects[i].transform.position - Flamer_objects[j].transform.position ), out hit1,Mathf.Infinity))
						{
							if(hit1.collider.gameObject.tag == "Flammable"){
								
								nearestVertex = hit1.point;
							}else{nearestVertex = Vector3.zero;}
						}

						Vector3 result = Flammable_objects[i].transform.TransformPoint(nearestVertex);
						result =nearestVertex;
						
						if(result != Vector3.zero){
										if(Emitter_objects.Count > (p11.main.maxParticles/2) ){//v2.1
								//do nothing
							}else{
								
								int is_close_to_other_point_on_object=0;

								for(int k=0;k<Registered_enflamed_positions.Count;k++){
									Vector2 ADD_ITEM1 = Registered_enflamed_positions[k];
									
									if(ADD_ITEM1.x == i){

										if(Vector3.Distance( Registered_paint_positions[(int)ADD_ITEM1.y-1],result )<3f){
											is_close_to_other_point_on_object=1;
										}
									}
								}

								if(is_close_to_other_point_on_object==0){

									Registered_paint_positions.Add(result);

									Emitter_objects.Add(hit1.collider.gameObject);
									Registered_initial_positions.Add(hit1.collider.gameObject.transform.position);
									Registered_initial_scale.Add (hit1.collider.gameObject.transform.localScale);
									Registered_initial_rotation.Add(hit1.collider.gameObject.transform.eulerAngles);
									
									Vector2 ADD_ITEM = new Vector2(i,Registered_paint_positions.Count);
									Registered_enflamed_positions.Add(ADD_ITEM);
								}
							}
						}
					}
				
			}
		 }


				}} //end check for flammables

	}

		if(Input.GetMouseButtonDown(1))
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
							Registered_initial_positions.Add(hit.collider.gameObject.transform.position);
							Registered_initial_scale.Add (hit.collider.gameObject.transform.localScale);
							Registered_initial_rotation.Add(hit.collider.gameObject.transform.eulerAngles);
						}
					}else if(Erase_mode){
						
						
						for (int i=0;i< Registered_paint_positions.Count;i++){
							
							if( Vector3.Distance(hit.point,Registered_paint_positions[i]) < (0.5f* brush_size))
							{
								Emitter_objects.RemoveAt(i);
								Registered_paint_positions.RemoveAt(i);
								Registered_initial_positions.RemoveAt(i);
								Registered_initial_rotation.RemoveAt(i);
								Registered_initial_scale.RemoveAt(i);
								break;
							}
							
						}
						
					}
					
				}
			}
		}
	}


		if(1==1 ){

				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);
				
					int counter_regsitered = 0;
					for (int i=0; i < ParticleList.Length;i++)
					{
						
					if(Registered_paint_positions!=null){
					if(Registered_paint_positions!=null & Registered_paint_positions.Count > 0 & draw_in_sequence){

						if(Emitter_objects[counter_regsitered].activeInHierarchy )
						{

						if(((counter_regsitered+1)*(ParticleList.Length/Registered_paint_positions.Count)) > i){

							Vector3 FIND_moved_toZERO = (Registered_paint_positions[counter_regsitered]-Emitter_objects[counter_regsitered].gameObject.transform.position) -(Registered_initial_positions[counter_regsitered] - Emitter_objects[counter_regsitered].gameObject.transform.position);;
							Vector3 FIND_rotated = Quaternion.Euler( -Registered_initial_rotation[counter_regsitered]+Emitter_objects[counter_regsitered].gameObject.transform.eulerAngles)*(FIND_moved_toZERO);

							Vector3 FIND_scaled = new Vector3(  FIND_rotated.x*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.x / Registered_initial_scale[counter_regsitered].x),FIND_rotated.y*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.y / Registered_initial_scale[counter_regsitered].y),FIND_rotated.z*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.z / Registered_initial_scale[counter_regsitered].z)  );


							Vector3 FIND_re_translated = FIND_scaled+Emitter_objects[counter_regsitered].gameObject.transform.position;
							Vector3 FIND_moved_pos = FIND_re_translated;

							Registered_paint_positions[counter_regsitered] = FIND_moved_pos;
							
							Registered_initial_positions[counter_regsitered]=(Emitter_objects[counter_regsitered].gameObject.transform.position);
							
							Registered_initial_rotation[counter_regsitered]=(Emitter_objects[counter_regsitered].gameObject.transform.eulerAngles);

							Registered_initial_scale[counter_regsitered]=(Emitter_objects[counter_regsitered].gameObject.transform.localScale);

							float FIND_Y = ParticleList[i].position.y;

							
								FIND_Y = FIND_moved_pos.y;
							

							if(!relaxed){

										if(Emitter_objects[counter_regsitered].activeInHierarchy){
								ParticleList[i].position  = new Vector3(FIND_moved_pos.x,FIND_Y,FIND_moved_pos.z) ; 
										}

							}

							if(relaxed){
										if(Emitter_objects[counter_regsitered].activeInHierarchy){
								if(ParticleList[i].remainingLifetime > 0.9f*ParticleList[i].startLifetime){
									ParticleList[i].position  = new Vector3(FIND_moved_pos.x,FIND_Y,FIND_moved_pos.z) ; 
								}
										}
							}

						}else{  
							if(counter_regsitered < Registered_paint_positions.Count-1 ){
								counter_regsitered= counter_regsitered+1;
							}else{counter_regsitered=0;}
						}
					}
				}

					if(Registered_paint_positions!=null & Registered_paint_positions.Count > 0 & !draw_in_sequence){ 

												
							Vector3 FIND_moved_toZERO = (Registered_paint_positions[counter_regsitered]-Emitter_objects[counter_regsitered].gameObject.transform.position) -(Registered_initial_positions[counter_regsitered] - Emitter_objects[counter_regsitered].gameObject.transform.position);;
							Vector3 FIND_rotated = Quaternion.Euler( -Registered_initial_rotation[counter_regsitered]+Emitter_objects[counter_regsitered].gameObject.transform.eulerAngles)*(FIND_moved_toZERO);
							
							Vector3 FIND_scaled = new Vector3(  FIND_rotated.x*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.x / Registered_initial_scale[counter_regsitered].x),FIND_rotated.y*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.y / Registered_initial_scale[counter_regsitered].y),FIND_rotated.z*(Emitter_objects[counter_regsitered].gameObject.transform.localScale.z / Registered_initial_scale[counter_regsitered].z)  );
							
							
							Vector3 FIND_re_translated = FIND_scaled+Emitter_objects[counter_regsitered].gameObject.transform.position;
							Vector3 FIND_moved_pos = FIND_re_translated;

							Registered_paint_positions[counter_regsitered] = FIND_moved_pos;
							
							Registered_initial_positions[counter_regsitered]=(Emitter_objects[counter_regsitered].gameObject.transform.position);
							
							Registered_initial_rotation[counter_regsitered]=(Emitter_objects[counter_regsitered].gameObject.transform.eulerAngles);
							
							Registered_initial_scale[counter_regsitered]=(Emitter_objects[counter_regsitered].gameObject.transform.localScale);
							

							float FIND_Y = ParticleList[i].position.y;
							
							
								FIND_Y = FIND_moved_pos.y;
							

						if(!relaxed){
								if(Emitter_objects[counter_regsitered].activeInHierarchy){
							ParticleList[i].position  = new Vector3(FIND_moved_pos.x,FIND_Y,FIND_moved_pos.z) ; 
								}
						}

						if(relaxed){
								if(Emitter_objects[counter_regsitered].activeInHierarchy){
							if(ParticleList[i].remainingLifetime > 0.9f*ParticleList[i].startLifetime){
								ParticleList[i].position  = new Vector3(FIND_moved_pos.x,FIND_Y,FIND_moved_pos.z) ; 
							}
								}
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