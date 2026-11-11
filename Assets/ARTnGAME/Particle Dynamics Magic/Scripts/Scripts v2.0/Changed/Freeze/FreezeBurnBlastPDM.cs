using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

	public class FreezeBurnBlastPDM : MonoBehaviour {
			
	void Start () {

		Freezer = this.gameObject.GetComponent(typeof(FreezeBurnControlPDM)) as FreezeBurnControlPDM;
			timer = Time.fixedTime;
	}

		//v1.8
		public FreezeBurnControlPDM Freezer;
		ParticleSystem BlasterP;
		public GameObject Blaster;
		GameObject Blast_it;
		public float destroy_on_freeze = 4;
		public bool Destroy_obj = true;
		public float Destroy_after = 0.7f;

		float timer; 
		bool emission_done = false;

	void Update () {

			if(Freezer == null){
				Freezer = this.gameObject.GetComponent(typeof(FreezeBurnControlPDM)) as FreezeBurnControlPDM;
			}
			if(Freezer != null & !emission_done){
				if(Freezer.freeze_ammount > destroy_on_freeze){
					if(Blaster!=null){
						Blast_it = (GameObject)Instantiate(Blaster,transform.position,Quaternion.identity);
						BlasterP = Blast_it.GetComponent<ParticleSystem>();
						if(BlasterP != null){

							//v2.1
							ParticleSystem.EmissionModule em = BlasterP.emission;
							em.enabled = true;
							//BlasterP.enableEmission = true;
							emission_done= true;
							timer = Time.fixedTime;
							//Destroy(this.gameObject);
							this.gameObject.GetComponent<Renderer>().enabled = false;
						}

					}
				}
			}

			if(emission_done){
				if(Destroy_obj & (Time.fixedTime - timer > Destroy_after)){
					Destroy(Blast_it);
					Destroy(this.gameObject);
				}
			}

	}	

}
}