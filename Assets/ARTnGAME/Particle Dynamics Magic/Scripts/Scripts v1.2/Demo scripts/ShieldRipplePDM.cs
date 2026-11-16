using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class ShieldRipplePDM : MonoBehaviour {
	
	void Start () {
			//v2.0
			if(Noise == null){
		Noise= GetComponent("ProceduralNoisePDM") as ProceduralNoisePDM;
			}
	}

	private float time_collision;

	ProceduralNoisePDM Noise;

	void Update () {
		if(Time.fixedTime-time_collision > 2){
			Noise.scale=0.10f;
			Noise.speed=1f;
		}
	}

	void OnCollisionEnter(Collision collision) {

			//v2.0
			if(Noise == null){
				Noise= GetComponent("ProceduralNoisePDM") as ProceduralNoisePDM;
			}

		Noise.scale=0.15f;
		Noise.speed=2.9f;

		time_collision = Time.fixedTime;

	}

}
}