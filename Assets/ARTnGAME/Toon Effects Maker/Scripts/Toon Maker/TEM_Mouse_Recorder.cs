using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
public class TEM_Mouse_Recorder : MonoBehaviour {

	void Start () {

		if(Recorded_Points==null){
			Recorded_Points = new List<Vector3>();
		}
		if(DirectVector==null){
			DirectVector = new List<Vector3>();
		}
		this_transform = transform;

		Virtual_transform_Main = transform.position;
		Virtual_transform_Path = transform.position;
		
	}

	public bool Reset_to_record_pos = false;
	public bool Register_new_start = false;

	Transform this_transform;

	Vector3 Virtual_transform_Main = Vector3.zero;
	Vector3 Virtual_transform_Path = Vector3.zero;

	public float Min_dist = 1.5f;

	public List<Vector3> DirectVector;
	public bool DirectToVector = false;
	public float Direct_speed=1f;
	public bool RecordVector = false;
	float last_dir_change_time;

	public bool Debug_on=false;

	[HideInInspector]
	public float Dir_change_time=0.5f;

	//[HideInInspector]
	public List<Vector3> Recorded_Points;

	public bool Record = false;

	public bool PlayPath = false;
	public bool Loop = false;

	public bool MoveRelative = false; //move based on new particle position, in relation to the one when recording started
	public Vector3 RecordStartPoint;

	void MoveToLayer(Transform root, int layer) {
		root.gameObject.layer = layer;
		foreach(Transform child in root)
			MoveToLayer(child, layer);
	}

	public bool Clear_record = false;
	public bool Clear_path_record = false;
	public float PlaySpeed = 50f;

	int traverse_points_counter;
	int traverse_dir_points_counter;

	[HideInInspector]
	public bool CosTurb = false;

	public bool UseLerp = false;
	Vector3 startPlayPos = Vector3.zero;

	[HideInInspector]
	public bool normalize_speed=false;


	void Update () {

		if(Recorded_Points == null){
			Recorded_Points = new List<Vector3>();
			this_transform = transform;
		}

		if(DirectVector == null){
			DirectVector = new List<Vector3>();
		}

		if(Record){
			RecordVector = false;

			if( RecordStartPoint == Vector3.zero){
				RecordStartPoint = this_transform.position;
			}

			if(1==1){

				if(Recorded_Points.Count == 0){
					Recorded_Points.Add(this_transform.position);
				}else{
					if(Vector3.Distance(Recorded_Points[Recorded_Points.Count-1],this_transform.position) > Min_dist){

						Recorded_Points.Add(this_transform.position);

					}
				}
			}
		}

		if(RecordVector){

			Record=false;

			if( RecordStartPoint == Vector3.zero){
				RecordStartPoint = this_transform.position;
			}
						
			if((Input.GetMouseButton(0) & Application.isPlaying) | !Application.isPlaying){
		
				if(DirectVector.Count == 0){
					DirectVector.Add(this_transform.position);
				}else{
					if(Vector3.Distance(DirectVector[DirectVector.Count-1],this_transform.position) > Min_dist){
						DirectVector.Add(this_transform.position);						
					}
				}
			}
		}

		if(Debug_on){

			for(int i=0;i<Recorded_Points.Count;i++){

				if(i == Recorded_Points.Count - 1){
					Debug.DrawLine(Recorded_Points[i],Recorded_Points[0]);
				}else{
					Debug.DrawLine(Recorded_Points[i],Recorded_Points[i+1]);
				}
			}
			for(int i=0;i<DirectVector.Count;i++){
				
				if(i == DirectVector.Count - 1){
					Debug.DrawLine(DirectVector[i],DirectVector[0],Color.blue);
				}else{
					Debug.DrawLine(DirectVector[i],DirectVector[i+1],Color.blue);
				}
			}
		}

		if(PlayPath & Application.isPlaying){

			Record = false;
			RecordVector = false;

			if(startPlayPos == Vector3.zero){
				startPlayPos = this_transform.position;//keep position when start playing effect
			}

			Vector3 Distance = Vector3.zero;
			if(MoveRelative){
				Distance = startPlayPos - RecordStartPoint;
			}			

			if(traverse_dir_points_counter < DirectVector.Count | traverse_points_counter < Recorded_Points.Count){		

				float Dist = 1;

				if(normalize_speed){
					if(traverse_points_counter > 0){
						Dist = (Recorded_Points[traverse_points_counter] - this_transform.position).magnitude;
						if(Dist <= 0){
							Dist = 0.01f;
						}
					}else{
						Dist = (this_transform.position -Recorded_Points[traverse_points_counter]).magnitude;
						if(Dist <= 0){
							Dist = 0.01f;
						}
					}
					//Debug.Log (Dist);
				}

				if(CosTurb){
					this_transform.position = Vector3.Lerp(this_transform.position, (Mathf.Abs (Mathf.Cos(Time.fixedTime*18.5f))*new Vector3(0.5f,0.5f,0.5f))+Recorded_Points[traverse_points_counter],PlaySpeed*Time.deltaTime);

				}else{

					if(UseLerp){

						if(traverse_points_counter < Recorded_Points.Count){
							Virtual_transform_Main = Vector3.Slerp(Virtual_transform_Main, Recorded_Points[traverse_points_counter],PlaySpeed*Time.deltaTime);
						}
						if(traverse_dir_points_counter < DirectVector.Count){
							Virtual_transform_Path = Vector3.Slerp(Virtual_transform_Path, DirectVector[traverse_dir_points_counter],Direct_speed*Time.deltaTime);
						}
						Vector3 Adder = Vector3.zero;

						Adder = Distance + Virtual_transform_Main;
						if(DirectToVector){
							Adder = Adder + Virtual_transform_Path - RecordStartPoint;
						}

						this_transform.position = Vector3.Slerp(this_transform.position,Adder,20*PlaySpeed*Time.deltaTime);

					}else{

						if(traverse_points_counter < Recorded_Points.Count){
							Virtual_transform_Main = Vector3.Slerp(Virtual_transform_Main, Recorded_Points[traverse_points_counter],PlaySpeed*Time.deltaTime);
						}
						if(traverse_dir_points_counter < DirectVector.Count){
							Virtual_transform_Path = Vector3.Slerp(Virtual_transform_Path, DirectVector[traverse_dir_points_counter],Direct_speed*Time.deltaTime);
						}
						Vector3 Adder = Vector3.zero;
				
						Adder = Distance + Virtual_transform_Main;
						if(DirectToVector){
							Adder = Adder + Virtual_transform_Path - RecordStartPoint;
						}

						this_transform.position = Adder;
					}
				}

				if(traverse_points_counter < Recorded_Points.Count){
					if(Vector3.Distance(Virtual_transform_Main, Recorded_Points[traverse_points_counter]) < 0.05f){
						traverse_points_counter++;
					}
				}

				if(traverse_dir_points_counter < DirectVector.Count){
					if(Vector3.Distance(Virtual_transform_Path, DirectVector[traverse_dir_points_counter]) < 0.05f){
						traverse_dir_points_counter++;
					}
				}

			}else{
				if(!Loop){
					PlayPath = false;
				}
				traverse_points_counter = 0;
				traverse_dir_points_counter = 0;

				Virtual_transform_Main = RecordStartPoint;
				Virtual_transform_Path = RecordStartPoint;
			}

		}else{
			startPlayPos = Vector3.zero;
		}

		if(Clear_record){

			Record = false;
			RecordVector = false;

			Recorded_Points.Clear();
			Clear_record=false;
			RecordStartPoint = Vector3.zero;

			traverse_points_counter = 0;
			Virtual_transform_Main = RecordStartPoint;
		}

		if(Clear_path_record){
			
			Record = false;
			RecordVector = false;			

			Clear_path_record=false;
			
			DirectVector.Clear();
			traverse_dir_points_counter = 0;			

			Virtual_transform_Path = RecordStartPoint;
		}

		if(Reset_to_record_pos){
			Reset_to_record_pos=false;
			this_transform.position = RecordStartPoint;
		}

		if(Register_new_start){
			Register_new_start=false;
			RecordStartPoint = this_transform.position;
		}
	}
}
