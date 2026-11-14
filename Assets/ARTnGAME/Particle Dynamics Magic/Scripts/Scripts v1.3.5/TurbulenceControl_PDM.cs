using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

	public class TurbulenceControl_PDM : MonoBehaviour {
			
	public GameObject TARGET_NODE;//the node target to move to and from
	public GameObject ATTRACTOR;
	public GameObject TURBULANT_SYSTEM;

		private Vector3 start_pos;
	
	void Start () {

			start_pos = ATTRACTOR.transform.position;
		
	}

		private bool going_back=false;
		public float attractor_speed = 1f;
	void Update () {

		//MOVE ATTRACTOR
			if(!going_back){
				int speed_variant= Random.Range(1,3);

				ATTRACTOR.transform.position = Vector3.Lerp(ATTRACTOR.transform.position, TARGET_NODE.transform.position,attractor_speed*speed_variant*0.9f*Time.deltaTime);

				if(Vector3.Distance(ATTRACTOR.transform.position, TARGET_NODE.transform.position) < 1 ){
					going_back= true;
				}

			}else if (going_back){
				int speed_variant= Random.Range(2,5);
				int freq_variant= Random.Range(2,15);
				ATTRACTOR.transform.position = new Vector3(0,ATTRACTOR.transform.position.y*0.01f*Mathf.Cos(freq_variant*Time.fixedTime),0 )+Vector3.Lerp(ATTRACTOR.transform.position, new Vector3(start_pos.x,start_pos.y,start_pos.z),attractor_speed*speed_variant*0.8f*Time.deltaTime);

				if(Vector3.Distance(ATTRACTOR.transform.position, start_pos) < 1 ){
					going_back= false;
				}

			}

	}
	}
}