using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class EmitParticles : MonoBehaviour {

		void OnSignal () 
		{
			//GetComponent<ParticleEmitter>().emit = true;

			ParticleSystem.EmissionModule emitModule = GetComponent<ParticleSystem> ().emission;
			emitModule.enabled = true;
			GetComponent<ParticleSystem> ().Emit (GetComponent<ParticleSystem> ().main.maxParticles);//v2.3
		}
}
}