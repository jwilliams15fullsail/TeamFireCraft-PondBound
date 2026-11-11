using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class ChainLightning_PDM : MonoBehaviour
{
	public Transform target;
	private GameObject[] target1;
	public int zigs = 100;
	public float speed = 1f;
	public float scale = 1f;
	public Light startLight;
	public Light endLight;
	
	PerlinPDM noise;
	float oneOverZigs;
	
	//private Particle[] particles;
		ParticleSystem.Particle[] particles; //v2.3

		//v1.4
		public bool Energized = false; //Activate externally to signal a parent has send lighting to this object
		public bool is_parent = false;
		public int current_depth; //filled externally, a non parent must search for targets if lower than max_depth and re-transmit it minus one;
		public int max_depth; //filled externally, a non parent must search for targets if lower than max_depth and re-transmit
		private int current_target_count;
		public int max_target_count=3;
	
	void Start()
	{
		if(zigs ==0){zigs=1;}
		oneOverZigs = 1f / (float)zigs;


		//GetComponent<ParticleEmitter>().emit = false;
		//GetComponent<ParticleEmitter>().Emit(zigs);
		ParticleSystem.EmissionModule emitModule = GetComponent<ParticleSystem> ().emission;
		emitModule.enabled = false;//v2.3
		GetComponent<ParticleSystem>().Emit(zigs);

		//particles = GetComponent<ParticleEmitter>().particles;
			particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().particleCount];
			GetComponent<ParticleSystem>().GetParticles(particles);

		target1 = GameObject.FindGameObjectsWithTag("Conductor");

		if(endLight){endLight.enabled=false; endLight.gameObject.SetActive(false);}
	}

	public bool Random_target;
	public float Affect_dist = 10f;

	private float Time_count;
	public float Change_target_delay=0.5f;

	public float Particle_energy=1f;

	public int optimize_factor=5;

	void Update ()
	{		

		if (noise == null)
			{noise = new PerlinPDM();}

		if(is_parent | (!is_parent & Energized & current_depth < max_depth) ){

			target1 = GameObject.FindGameObjectsWithTag("Conductor");
			
		if(target1 !=null){
			if(target1.Length > 0 ){

				int Choose = Random.Range(0,target1.Length);
				if(Random_target){

					if(Time.fixedTime-Time_count > Change_target_delay ){

						if(Vector3.Distance(target1[Choose].transform.position, this.transform.position) < Affect_dist){
							target= target1[Choose].transform;
						}else{

									//GetComponent<ParticleEmitter>().ClearParticles();
									GetComponent<ParticleSystem> ().Clear ();
								
								}
						Time_count = Time.fixedTime;

								//v1.4
								//disable after N targets are found and hit
								if(!is_parent & Energized & current_depth < max_depth & current_target_count > max_target_count ){
									Energized = false;
								}
								current_target_count++;

					}
					if(target!=null){
						if(Vector3.Distance(target.position, this.transform.position) > Affect_dist){target= null;

									//GetComponent<ParticleEmitter>().ClearParticles();
									GetComponent<ParticleSystem> ().Clear ();
								}
					}
				}
				else{

					target=null;
					//GetComponent<ParticleEmitter>().ClearParticles();
							GetComponent<ParticleSystem> ().Clear ();

					int count_each=0;
					foreach(GameObject TRANS in target1){


						if( Vector3.Distance(TRANS.transform.position, this.transform.position) < Affect_dist){

							target= TRANS.transform;
						
						}
						count_each=count_each+1;
					}

							//v1.4
							//disable after N targets are found and hit
							if(!is_parent & Energized & current_depth < max_depth & current_target_count > max_target_count ){
								Energized = false;
							}
							current_target_count++;
				}

		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;

		if(target!=null){

		for (int i=0; i < particles.Length; i++)
		{
			Vector3 position = Vector3.Lerp(transform.position, target.position, oneOverZigs * (float)i);
			Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
										noise.Noise(timey + position.x, timey + position.y, timey + position.z),
										noise.Noise(timez + position.x, timez + position.y, timez + position.z));
			position += (offset * scale * ((float)i * oneOverZigs));
			
			particles[i].position = position;
			particles[i].startColor = Color.white;
						//particles[i].energy = Particle_energy;
								particles[i].startLifetime = Particle_energy;
		}

							//v1.4
							ChainLightning_PDM next_in_Chain = target.gameObject.GetComponent(typeof(ChainLightning_PDM)) as ChainLightning_PDM;
								//Energize target
							if(next_in_Chain ==null){

								next_in_Chain = target.gameObject.GetComponentInChildren(typeof(ChainLightning_PDM)) as ChainLightning_PDM;

							}

							if(next_in_Chain !=null){
								if(!next_in_Chain.is_parent){

									next_in_Chain.Energized = true;
									next_in_Chain.max_depth = max_depth;
									next_in_Chain.current_depth = current_depth+1;
								}
							}
		
		//GetComponent<ParticleEmitter>().particles = particles;
							GetComponent<ParticleSystem>().SetParticles(particles, GetComponent<ParticleSystem>().particleCount);
		
		if (GetComponent<ParticleSystem>().particleCount >= 2)
		{
			if (startLight)
				startLight.transform.position = particles[0].position;
			
			int get_in=1;
			get_in=Random.Range(1,optimize_factor);
			if (endLight){ 
							if(get_in==1 & target!=null){
								endLight.enabled=true;
								endLight.gameObject.SetActive(true);
								endLight.transform.position = particles[particles.Length - 1].position;
							}else{
								endLight.enabled=false;
							}
			}
		}else{
			endLight.enabled=false;
		}

		}else{
							for (int i=0; i < particles.Length; i++)
							{
								particles[i].position = Vector3.zero;
							}
							//GetComponent<ParticleEmitter>().particles = particles;
							GetComponent<ParticleSystem>().SetParticles(particles, GetComponent<ParticleSystem>().particleCount);
		}

	if(endLight & target==null){endLight.enabled=false; endLight.gameObject.SetActive(false);}

   }//END check if targets > 0
  }
		}//END check if parent
		else{
				for (int i=0; i < particles.Length; i++)
				{
					particles[i].position = Vector3.zero;
				}
				//GetComponent<ParticleEmitter>().particles = particles;
				GetComponent<ParticleSystem>().SetParticles(particles, GetComponent<ParticleSystem>().particleCount);
		}

 }

}

}