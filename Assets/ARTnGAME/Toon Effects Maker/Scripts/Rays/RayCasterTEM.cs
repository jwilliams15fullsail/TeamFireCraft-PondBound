using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.TEM {

[RequireComponent(typeof(LineRenderer))]
public class RayCasterTEM : MonoBehaviour
{

	//texture atlas anim
	public bool Animated_tex = false;
	public int TexColumns = 5;
	public int TexRows = 5;
	public float FPS = 10f;

	public Color Start_color = Color.white;
	public Color End_color = Color.white;

	public List<GameObject> Reflections = new List<GameObject>();
	List<LineRenderer> ReflectionsL = new List<LineRenderer>();

	public GameObject RotateObject;

	Vector2 mouse;
	RaycastHit hit;
	public float range = 100.0f;
	LineRenderer line;
	public Material lineMaterial;

	public Vector2 Start_end_width = new Vector2(1,1);

	void Start()
	{
		line = GetComponent<LineRenderer>();

		//line.SetVertexCount(2);
			line.positionCount = 2; //v2.3 PDM

		line.GetComponent<Renderer>().material = lineMaterial;
		//line.SetWidth(Start_end_width.x, Start_end_width.y);

			//v2.3 PDM
			line.startWidth = Start_end_width.x;
			line.endWidth = Start_end_width.y;

		if(RotateObject != null){
			Particles = RotateObject.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
		}

		//
		if(Animated_tex){
			GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1f / TexColumns, 1f / TexRows));
			Timing = Time.fixedTime;
		}
		this_transform = this.transform;
	}

	ParticleSystem Particles;

	public bool reflect = false;
	public int Reflect_steps = 5;
	public float Reflect_dist = 1000;

	public bool Always_on = false;
	public Transform Pointer;
	Transform this_transform;
	public bool Reflect_no_target=false;
	public float max_ray_len=15;
	

	Vector3 First_hit;

		public bool Apply_force = false;
		public float shotForce = 200;

	void Update()
	{
		int counter = 0;
			if(Input.GetMouseButton(0) | Always_on)
			{

			if(1==1){
				
				for(int i = Reflections.Count -1 ; i>=0;i--){
					Destroy(Reflections[i].gameObject);						
				}
				Reflections.Clear();
				ReflectionsL.Clear();
				
				
			}


				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			Vector3 Start_Position = this_transform.position;
			if(Always_on){
				if(Pointer != null){
					ray = new Ray(Pointer.position,Pointer.forward);
					Start_Position = Pointer.position;
				}else{
					ray = new Ray(this_transform.position,this_transform.forward);
				}
			}

				if(Physics.Raycast(ray, out hit, range))
				{
					//line.SetVertexCount(2);
					line.positionCount = 2; //v2.3 PDM

					line.enabled = true;
				line.SetPosition(counter, Start_Position);
					counter++;

					line.SetPosition(counter, hit.point );
					counter++;

					if(Apply_force){
						if(hit.collider !=null){
							if(hit.collider.GetComponent<Rigidbody>() !=null){
								Rigidbody shot = hit.collider.GetComponent<Rigidbody>();
								shot.AddForce((hit.point - transform.position).normalized * shotForce);
							}
						}
					}

				First_hit = hit.point;

				//line.SetColors(Start_color,End_color);
					line.startColor = Start_color;
					line.endColor = End_color;

					if(RotateObject!=null){
						RotateObject.transform.LookAt(hit.point );
					}
					
					if(Particles != null){
						//Particles.enableEmission = true;
						ParticleSystem.EmissionModule emitModule = Particles.emission; //v2.3 PDM
						emitModule.enabled = true;
					}

				if(reflect){
					Vector3 Hit_point = hit.point;
					
					Vector3 moveDir = Vector3.Reflect( (hit.point - Start_Position).normalized, hit.normal );
					

					int found = -1;
					
					
					for(int i=0;i<Reflect_steps;i++){
						if (Physics.Raycast (Hit_point, moveDir, out hit, Reflect_dist) )
						{
							
								found = i;
							
							if(Reflections.Count < (i+1)){
								GameObject LineMore = new GameObject("LineReflected");
								LineMore.transform.parent = this_transform;
								LineMore.AddComponent(typeof(LineRenderer));
								LineRenderer Reflection = LineMore.GetComponent(typeof(LineRenderer))as LineRenderer;

								Reflection.material = line.material;
								//Reflection.SetWidth(Start_end_width.x, Start_end_width.y);
									//v2.3 PDM
									Reflection.startWidth = Start_end_width.x;
									Reflection.endWidth = Start_end_width.y;

								//Reflection.SetColors(Start_color,End_color);
									Reflection.startColor = Start_color;
									Reflection.endColor = End_color;

								//Reflection.SetVertexCount(2);
									Reflection.positionCount = 2; //v2.3 PDM

								Reflection.SetPosition(0, Hit_point);
								Reflection.SetPosition(1, hit.point);
								Reflections.Add(LineMore);
								ReflectionsL.Add(Reflection);
							}else{
								//ReflectionsL[i].SetVertexCount(2);
									ReflectionsL[i].positionCount = 2; //v2.3 PDM

								ReflectionsL[i].SetPosition(0, Hit_point);
								ReflectionsL[i].SetPosition(1, hit.point);

								ReflectionsL[i].material = line.material;
								//ReflectionsL[i].SetWidth(Start_end_width.x, Start_end_width.y);
									//v2.3 PDM
									ReflectionsL[i].startWidth = Start_end_width.x;
									ReflectionsL[i].endWidth = Start_end_width.y;

								//ReflectionsL[i].SetColors(Start_color,End_color);
									ReflectionsL[i].startColor = Start_color;
									ReflectionsL[i].endColor = End_color;
							}
						
							moveDir = Vector3.Reflect( (hit.point - Hit_point).normalized, hit.normal ) ;
							Hit_point = hit.point;							

							

						}else{

							break;
						}

					}



					if(ReflectionsL.Count >0){
						ReflectionsL[0].SetPosition(0, First_hit);
					}
					if(found != -1){
						if(Reflect_no_target){

							

							if(Reflections.Count < found+2){
								GameObject LineMore = new GameObject("LineReflected");
								LineMore.transform.parent = this_transform;
								LineMore.AddComponent(typeof(LineRenderer));
								LineRenderer Reflection = LineMore.GetComponent(typeof(LineRenderer))as LineRenderer;
								
								Reflection.material = line.material;
								//Reflection.SetWidth(Start_end_width.x, Start_end_width.y);
									//v2.3 PDM
									Reflection.startWidth = Start_end_width.x;
									Reflection.endWidth = Start_end_width.y;

								//Reflection.SetColors(Start_color,End_color);
									Reflection.startColor = Start_color;
									Reflection.endColor = End_color;
								
								//Reflection.SetVertexCount(2);
									Reflection.positionCount = 2; //v2.3 PDM

								Reflection.SetPosition(0, Hit_point);
								Reflection.SetPosition(1, Hit_point + max_ray_len*hit.normal);
								Reflections.Add(LineMore);
								ReflectionsL.Add(Reflection);
							}else{
									
								//ReflectionsL[found+1].SetVertexCount(2);
									ReflectionsL[found+1].positionCount = 2; //v2.3 PDM

								ReflectionsL[found+1].SetPosition(0, Hit_point);
								ReflectionsL[found+1].SetPosition(1, Hit_point + max_ray_len*hit.normal);
								//ReflectionsL[found+1].SetColors(Start_color,End_color);
									ReflectionsL[found+1].startColor = Start_color;
									ReflectionsL[found+1].endColor = End_color;
							}
						}
					}else{

					}
				}
			  }
			else{
				if(!Always_on){
					for(int i = Reflections.Count -1 ; i>=0;i--){
						Destroy(Reflections[i].gameObject);						
					}
					Reflections.Clear();
					ReflectionsL.Clear();
					
					line.enabled = false;
					
					if(Particles != null){
						//Particles.enableEmission = false;
							ParticleSystem.EmissionModule emitModule = Particles.emission; //v2.3 PDM
							emitModule.enabled = false;
					}
				}
			}
				
			}
		else{
			
			for(int i = Reflections.Count -1 ; i>=0;i--){
				Destroy(Reflections[i].gameObject);
				
			}
			Reflections.Clear();
			ReflectionsL.Clear();
			
			line.enabled = false;
			
			if(Particles != null){
				//Particles.enableEmission = false;
					ParticleSystem.EmissionModule emitModule = Particles.emission; //v2.3 PDM
					emitModule.enabled = false;
			}
		}
	}

	int frame_counter=0;
	float Timing;

	void LateUpdate () {

		if(Animated_tex){				
			if(Time.fixedTime - Timing > (1/FPS)){

				float Y_coord = (int)(frame_counter/TexRows);
				float X_coord = frame_counter;
								
				GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2( ((float)X_coord/TexColumns), ((float)Y_coord/TexRows)  ));
				if(frame_counter > (TexRows*TexColumns)){
					frame_counter=0;
				}else{
					frame_counter++;
				}

				Timing = Time.fixedTime;
			}
		}
	}
}

}