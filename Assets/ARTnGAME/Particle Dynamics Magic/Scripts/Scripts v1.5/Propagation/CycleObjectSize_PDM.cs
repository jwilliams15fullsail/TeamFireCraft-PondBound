using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

	public class CycleObjectSize_PDM : MonoBehaviour {
			
	void Start () {

			Thistransform = this.gameObject.transform;

			Thistransform.localScale = start_scale;
			Random_factor = Random.Range(5,max_scale);
	}

	void Awake(){
			Thistransform = this.gameObject.transform;
			Thistransform.localScale = start_scale;
	}

		public bool dont_grow=false; //use to only vary start size !!!!!

		public bool reduce_size=false;
		public bool pulsate_size=false;

	private Transform Thistransform;

		public Vector3 start_scale=new Vector3(0.001f,0.001f,0.001f);
		public float speed=0.1f;
	public float max_scale=26f;
	public float mean_scale=22f;
		float Random_factor;

	void Update () {

			if(!dont_grow){
				if(Thistransform.localScale.x < mean_scale* Random_factor * 0.1f){

					Thistransform.localScale = new Vector3(Thistransform.localScale.x+speed,Thistransform.localScale.y+speed,Thistransform.localScale.z+speed);

				}else{

				}
			}else{

				Thistransform.localScale = mean_scale* Random_factor * 0.1f * new Vector3(1,1,1);


			}

	}

	

}
}