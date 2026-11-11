using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class DestroyOnImpactPDM : MonoBehaviour {
	
	void Start () {

	}

	private float time_collision;

	ProceduralNoisePDM Noise;

	public GameObject To_Destroy;
	public GameObject To_Spawn;
	public float destroy_time=1f;
	private bool Collided = false;

	void Update () {
		if(Time.fixedTime-time_collision > destroy_time & Collided){

			if(Time.fixedTime-time_collision > destroy_time+0.2f ){
				if(To_Destroy!=null){
					To_Destroy.SetActive(false);
				}
			}

			if(To_Spawn!=null){
				To_Spawn.SetActive(true);
			}
		}
	}

	void OnCollisionEnter(Collision collision) {

		time_collision = Time.fixedTime;

		Collided=true;

		MeshRenderer MESH = this.GetComponent("MeshRenderer") as MeshRenderer;
		MESH.enabled=false;
	}

}
}