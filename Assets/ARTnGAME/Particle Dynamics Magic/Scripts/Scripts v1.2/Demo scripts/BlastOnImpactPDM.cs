using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class BlastOnImpactPDM : MonoBehaviour {
	
	void Start () {

		Mover = gameObject.transform.parent.GetComponent("MoveItemOverGround") as MoveItemOverGround;
		Place_ring = gameObject.GetComponentInChildren(typeof(PlaceParticleOnGround)) as PlaceParticleOnGround;

		delay=Time.fixedTime; 

		Flame = gameObject.GetComponentInChildren(typeof(ParticleSystem)) as ParticleSystem;
	}

	ParticleSystem Flame;

	MoveItemOverGround Mover;
	PlaceParticleOnGround Place_ring;
	
	
	public GameObject HERO;

	public float PROJ_SPEED = 5;

	private float delay;

	public float blast_time=1f;

	void Update () {

		
	}

	void OnCollisionEnter(Collision collision) {

		if(Time.fixedTime - delay > blast_time){

			if(Mover!=null){
				Mover.enabled=false;
			}
			if(Place_ring!=null){
				Place_ring.enabled=true;
			}
			if(Flame!=null){
				
				//Flame.gravityModifier = -0.76f;
				ParticleSystem.MainModule main = Flame.main;
				main.gravityModifier =  -0.76f;//v2.3
				main.startSize=1.5f;
			}
			transform.position = new Vector3(transform.position.x, -23,transform.position.z);
			this.GetComponent<Rigidbody>().useGravity=false;
			this.GetComponent<Rigidbody>().isKinematic=true;
		}
	}


}
}