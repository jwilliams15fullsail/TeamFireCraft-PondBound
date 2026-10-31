using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

public class ShieldRippleTEM : MonoBehaviour {
	
	void Start () {
			Noise= GetComponent("ProceduralNoiseTEM") as ProceduralNoiseTEM;
	}

	private float time_collision;

		ProceduralNoiseTEM Noise;

	void Update () {
		if(Time.fixedTime-time_collision > 2){
			Noise.scale=0.10f;
			Noise.speed=1f;
		}
	}

	void OnCollisionEnter(Collision collision) {

		Noise.scale=0.15f;
		Noise.speed=2.9f;

		time_collision = Time.fixedTime;

	}

}
}