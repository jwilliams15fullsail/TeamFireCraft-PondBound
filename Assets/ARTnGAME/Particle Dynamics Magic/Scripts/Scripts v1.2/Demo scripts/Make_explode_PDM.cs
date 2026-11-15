using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

public class Make_explode_PDM : MonoBehaviour {

	void Start () {
		HERO_transform = HERO.transform;
		thisTransform=this.gameObject.transform;

		Particles = GetComponentInChildren(typeof(ParticleSystem)) as ParticleSystem;
		Attractor_script = GetComponent("AttractParticles") as AttractParticles;

		wait = Time.fixedTime;

	}

	PlaceParticleOnSpline[] TentaclesS;

	public List<GameObject> Tentacles;

	ParticleSystem Particles;
	AttractParticles Attractor_script;

	Transform HERO_transform;
	Transform thisTransform;
	public GameObject HERO;

	bool appeared = false;

	float wait;
	
	void Update () {

			ParticleSystem.MainModule main = Particles.main;//v2.3

		if(Attractor_script!=null){
	
		if( Vector3.Distance(HERO_transform.position, thisTransform.position) < 5f & !appeared){

			Attractor_script.dumpen = 0.8f;


					main.startLifetime = 5f;//v2.3

		appeared = true;

			wait = Time.fixedTime;

			Tentacles[0].SetActive(true);


		}


		if(appeared & (Time.fixedTime-wait>3) ){

			Attractor_script.enabled=false;

			//v2.1
			ParticleSystem.EmissionModule em = Particles.emission;
			em.enabled = false;
			//Particles.enableEmission=false;
			Particles.Clear();
			Particles.Stop();
		}else if (appeared){

					//startLifetime = startLifetime+0.1f;//v2.3 
					ParticleSystem.MinMaxCurve Curve = main.startLifetime;
					Curve.constant = Curve.constant+0.1f;//v2.3 
		}




	}

	}
}
}