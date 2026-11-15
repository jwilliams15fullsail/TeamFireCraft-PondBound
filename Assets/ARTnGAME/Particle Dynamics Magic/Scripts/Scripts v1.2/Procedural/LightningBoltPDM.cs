using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class LightningBoltPDM : MonoBehaviour
{
	public Transform target;
	public int zigs = 100;
	public float speed = 1f;
	public float scale = 1f;
	public Light startLight;
	public Light endLight;
	
	PerlinPDM noise;
	float oneOverZigs;
	
	//private Particle[] particles;
		ParticleSystem.Particle[] particles; //v2.3
	
	void Start()
	{
		oneOverZigs = 1f / (float)zigs;

//		GetComponent<ParticleEmitter>().emit = false;
//		GetComponent<ParticleEmitter>().Emit(zigs);
			ParticleSystem.EmissionModule emitModule = GetComponent<ParticleSystem> ().emission;
			emitModule.enabled = false;//v2.3
			GetComponent<ParticleSystem>().Emit(zigs);

		//v2.3
		//particles = GetComponent<ParticleEmitter>().particles;
		particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().particleCount];
		GetComponent<ParticleSystem>().GetParticles(particles);

	}
	
	void Update ()
	{
		if (noise == null)
			noise = new PerlinPDM();
			
		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		for (int i=0; i < particles.Length; i++)
		{
			Vector3 position = Vector3.Lerp(transform.position, target.position, oneOverZigs * (float)i);
			Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
										noise.Noise(timey + position.x, timey + position.y, timey + position.z),
										noise.Noise(timez + position.x, timez + position.y, timez + position.z));
			position += (offset * scale * ((float)i * oneOverZigs));
			
			particles[i].position = position;
				particles[i].startColor = Color.white;

			//v2.3
			//particles[i].energy = 1f;
			particles[i].startLifetime = 1f;
		}
		
		//v2.3
		//GetComponent<ParticleEmitter>().particles = particles;
			GetComponent<ParticleSystem>().SetParticles(particles, GetComponent<ParticleSystem>().particleCount);
		
			if (GetComponent<ParticleSystem>().particleCount >= 2)
		{
			if (startLight)
				startLight.transform.position = particles[0].position;
			if (endLight)
				endLight.transform.position = particles[particles.Length - 1].position;
		}
	}	
}

}