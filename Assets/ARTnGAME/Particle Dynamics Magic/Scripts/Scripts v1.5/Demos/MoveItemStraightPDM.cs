using UnityEngine;
using System.Collections;

namespace Artngame.PDM {

	public class MoveItemStraightPDM : MonoBehaviour {
	
	void Start () {

		if(HERO !=null){
		CAST_FORWARD_VEC=HERO.transform.forward;
		}else{CAST_FORWARD_VEC=this.gameObject.transform.forward;}

			This_transform = this.transform;
			if(target!=null){
			target_transform = target.transform;
			}

			Motion_vec = CAST_FORWARD_VEC;
			current_time = Time.fixedTime;

			GetComponent<Rigidbody>().linearVelocity = This_transform.forward.normalized * PROJ_SPEED;
	}
	
	Vector3 CAST_FORWARD_VEC;
	public GameObject HERO;
		Vector3 Previous_pos;

		public GameObject target;
		Transform This_transform;
		Transform target_transform;

	public float PROJ_SPEED = 5;
		public float PROJ_ACCELERATION = 5;

	public float Speed_x;
	public float Speed_z;
		public float TURN_SPEED=16f;
		Vector3 Motion_vec;

		Vector3 previous_motion_vec;

		public float init_time=0.5f; //time until seek starts
		float current_time;
		public bool curved=false;

		void Update1(){

			if(Time.fixedTime - current_time > init_time){
				Vector3 Pos_to_target = target_transform.position - This_transform.position;
				float Diff = Pos_to_target.magnitude;
				if(Diff ==0){Diff=0.1f;}

				if(!curved){
					this.transform.position  =  this.transform.position+Pos_to_target.normalized * Time.deltaTime *PROJ_SPEED;
				}else{
					Vector3 Intermediate = Pos_to_target + (50/Diff)*CAST_FORWARD_VEC;
					this.transform.position  =  this.transform.position+Intermediate.normalized * Time.deltaTime *PROJ_SPEED;
				}

			}else{

				this.transform.position  =  this.transform.position+CAST_FORWARD_VEC * Time.deltaTime *PROJ_SPEED;
			}
		}

		protected float proximity;

	void FixedUpdate () {
							
			proximity = proximity - GetComponent<Rigidbody>().linearVelocity.magnitude * Time.fixedDeltaTime;
						
			if(target!=null){
			
			Vector3 Pos_to_target = target_transform.position - This_transform.position;
			Vector3 velocity = Vector3.RotateTowards(GetComponent<Rigidbody>().linearVelocity, Pos_to_target, Mathf.Deg2Rad*2*TURN_SPEED * Time.fixedDeltaTime, 0);
						
			GetComponent<Rigidbody>().linearVelocity = velocity;

				if(Vector3.Distance(Previous_pos,This_transform.position) > 0.2f){
					Motion_vec = This_transform.position - Previous_pos;
					transform.forward = Vector3.Slerp(transform.forward,Motion_vec,Time.deltaTime*TURN_SPEED);
					Previous_pos = This_transform.position;
				}

			}

	}























}
}