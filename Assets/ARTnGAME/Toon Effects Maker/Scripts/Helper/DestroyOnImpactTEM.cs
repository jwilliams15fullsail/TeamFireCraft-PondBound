using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

public class DestroyOnImpactTEM : MonoBehaviour {
	
	void Start () {
			timer = Time.fixedTime;
	}
		public bool Radial_force=false;
		public float radius = 5.0f;
		public float power = 10.0f;

	private float time_collision;

	public GameObject To_Destroy;
	public GameObject To_Spawn;
	public float destroy_time=1f;
	private bool Collided = false;

		public bool FullDestroy = true;

		public bool Destroy_after_time=false;
		float timer;
		public float max_time=10f;

		public bool Disable_physics=true;
		public bool Disable_particle=true;

	void Update () {
		if(Time.fixedTime-time_collision > destroy_time & Collided){

			if(Time.fixedTime-time_collision > destroy_time+0.2f ){
				if(To_Destroy!=null){
					To_Destroy.SetActive(false);

						if(FullDestroy){
							Destroy (To_Destroy);
						}
				}
			}

//			if(To_Spawn!=null){
//				To_Spawn.SetActive(true);
//			}
		}
			if(Destroy_after_time){
				if(Time.fixedTime - timer > max_time){
					if(To_Destroy!=null){
						To_Destroy.SetActive(false);
						
						if(FullDestroy){
							Destroy (To_Destroy);
						}
					}
				}
			}
	}

	void OnCollisionEnter(Collision collision) {

		time_collision = Time.fixedTime;

		Collided=true;

		MeshRenderer MESH = this.GetComponent("MeshRenderer") as MeshRenderer;
		if(MESH !=null){
			MESH.enabled=false;
		}

			if(Disable_particle){
				ParticleSystem[] Ball_particles = To_Destroy.GetComponentsInChildren<ParticleSystem>(false);
				if(Ball_particles!=null){
					for(int i=0;i<Ball_particles.Length;i++){

						//v2.3 PDM
						//Ball_particles[i].enableEmission = false;
						ParticleSystem.EmissionModule emitModule = Ball_particles [i].emission;
						emitModule.enabled = false;
					}
				}
			}
			if(Disable_physics){
				if(this.GetComponent<Collider>() !=null){
					this.GetComponent<Collider>().enabled = false;
				}
				if(this.GetComponent<Rigidbody>() !=null){
					this.GetComponent<Rigidbody>().isKinematic = true;
					this.GetComponent<Rigidbody>().useGravity = false;
				}
			}

			if(To_Spawn!=null){
				To_Spawn.SetActive(true);
			}

			if(Radial_force){

				Vector3 explosionPos = transform.position;
				Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
				foreach (Collider hit in colliders) {
					if (hit && hit.GetComponent<Rigidbody>()){
						hit.GetComponent<Rigidbody>().AddExplosionForce(power, explosionPos, radius, 3.0f);
					}
					
				}

			}

			///////
	}

}
}