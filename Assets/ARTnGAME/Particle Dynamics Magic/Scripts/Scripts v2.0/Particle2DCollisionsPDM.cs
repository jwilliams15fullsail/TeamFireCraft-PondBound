using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
	//public class PlaceParticleOnSpline : MonoBehaviour {	//Particle2DCollisionsPDM
public class Particle2DCollisionsPDM : MonoBehaviour {	
	
	public void Start () {
			This_transf = this.transform;


			if(p2 == null){
				p2 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
			}

			if(p2 == null){

				Debug.Log ("Please attach the script to a particle system");
			}

		if(p2 == null ){
			
			return;
			
		}


	}
		Transform This_transf;
	public ParticleSystem p2;
			
	//public Particle[] particles;
	ParticleSystem.Particle[] ParticleList;
		public bool extend_life = false;
		public float keep_alive_factor = 0.5f;

		public Vector2 Bounce_range = new Vector2(1,1);

		public float Bounce_factor = 10;
		public float Min_col_dist = 0.01f;
		public bool Friction= false;
		public float Friction_loss = 0.9f;
		public float Size_loss = 0.9f;
		public float Min_size = 0.1f;

		public bool Size_on_col = false;
		public bool Size_on_vel = false;

		[HideInInspector]
		public bool Add_colliders = false;//add colliders per particle
		[HideInInspector]
		public List<Transform> Colliders = new List<Transform>();

		public bool GUI_on = false;

	void LateUpdate () {

		if(p2 == null ){

				p2 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

				if(p2==null){
					Debug.Log ("Please attach the script to a particle system");
					return;
				}

		}



			ParticleSystem p11=p2;

				
				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);
				

					for (int i=0; i < ParticleList.Length;i++)
					{

				if(Add_colliders & Application.isPlaying){

					if(Colliders.Count < ParticleList.Length){

						GameObject AA = new GameObject("Collider");
						GameObject BB = (GameObject)Instantiate(AA,ParticleList[i].position,Quaternion.identity);
						//CircleCollider2D New_col = new CircleCollider2D();
						BB.AddComponent(typeof(CircleCollider2D));
						Colliders.Add(BB.transform);
						//BB.collider2D.r
					}else{
						Colliders[i].position = ParticleList[i].position;
					}

				}
						
						if(extend_life){
							
							//ParticleList[i].lifetime = tileCount + 1 - Sheet_tile;//tile[count_particles];
													
							if(ParticleList[i].remainingLifetime < ParticleList[i].startLifetime*keep_alive_factor){
								ParticleList[i].startLifetime = 16;
								ParticleList[i].remainingLifetime = 16 ;
							}
						}


				//ParticleList[i].position  = new Vector3(ParticleList[i].position.x,ParticleList[i].position.y,This_transf.position.z + (Particle_Z_dist*i));
				ParticleList[i].position  = new Vector3(ParticleList[i].position.x,ParticleList[i].position.y,This_transf.position.z );

				ParticleList[i].velocity = new Vector3(ParticleList[i].velocity.x,ParticleList[i].velocity.y,0);

				//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				Vector2 Origin = new Vector2(ParticleList[i].position.x, ParticleList[i].position.y);
				Vector2 Direction = new Vector2(ParticleList[i].velocity.x, ParticleList[i].velocity.y);

				//RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
				RaycastHit2D hit = Physics2D.Raycast(Origin, Direction);

				//RaycastHit2D hit = Physics2D.Raycast(cameraPosition, mousePosition, distance (optional));
				if(hit.collider != null){
					//isHit = false;
					//Destroy(GameObject.Find(hit.collider.gameObject.name));
					//Debug.Log ("HIT"+hit.collider.name);
				//	if(Vector3.Distance(hit.point,ParticleList[i].position) < 0.2f){
					if(Vector2.Distance(new Vector2(hit.point.x,hit.point.y),new Vector2(ParticleList[i].position.x,ParticleList[i].position.y)) < Min_col_dist){
						Vector3 Reflection =  Vector3.Reflect( ParticleList[i].velocity,hit.normal);

						float Bounce_range_factor = 1;
						if(Bounce_range != new Vector2(1,1)){
							Bounce_range_factor = Random.Range(Bounce_range.x, Bounce_range.y);
						}

						if(Friction){
							ParticleList[i].velocity = Reflection * Bounce_factor * Friction_loss * Bounce_range_factor;
						}else{
							ParticleList[i].velocity = Reflection * Bounce_factor* Bounce_range_factor;
						}

						if(Size_on_col & Size_loss < 1 & ParticleList[i].startSize > Min_size ){
							ParticleList[i].startSize = Size_loss * ParticleList[i].startSize;
						}

						if(Size_on_vel & Size_loss < 1 & ParticleList[i].startSize < p2.main.startSize.constant){
							ParticleList[i].startSize = ParticleList[i].velocity.magnitude * 0.1f;
						}
					}
				}

			





							//	ParticleList[i].position  = Vector3.Slerp(ParticleList[i].position ,  Spline_to_Conform_Curve_pos[counter], Return_Speed);
						
//						//v1.8
//						if(Look_at_spline){
//							ParticleList[i].axisOfRotation = Rot_axis;
//							if(counter+1 < Spline_to_Conform_Curve_pos.Count){
//								Vector3 Diff = Spline_to_Conform_Curve_pos[counter] - Spline_to_Conform_Curve_pos[counter+1];
//								float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
//								if(Diff.z > 0){
//									angle = - angle;
//								}
//								ParticleList[i].rotation = Rotation_offset + angle;
//							}else{
//								
//								Vector3 Diff = Spline_to_Conform_Curve_pos[counter] - Spline_to_Conform_Curve_pos[0];
//								float angle = Mathf.Abs(Vector3.Angle(Rot_angle_axis,Diff));
//								ParticleList[i].rotation = Rotation_offset + angle;
//								
//							}
//						}					


					}
				

				p2.SetParticles(ParticleList,p11.particleCount);

		}

		void OnGUI(){
			if(GUI_on){
				//if(GUI.Button(new Rect(5,5,50,20),"
				Friction_loss = GUI.HorizontalSlider(new Rect(5,5,50,20), Friction_loss,0,1);
				Bounce_factor = GUI.HorizontalSlider(new Rect(5,5+1*30,50,20), Bounce_factor,0,1);
				Size_loss = GUI.HorizontalSlider(new Rect(5,5+2*30,50,20), Size_loss,0,1);
			}
		}

	}
}
