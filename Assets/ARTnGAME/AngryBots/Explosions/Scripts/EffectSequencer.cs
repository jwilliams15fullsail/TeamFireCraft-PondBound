using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM{
	[System.Serializable]
	public class ExplosionPart {
		public GameObject gameObject = null;
		public float delay = 0.0f;	
		public bool hqOnly = false;
		public float yOffset = 0.0f;
	}


public class EffectSequencer : MonoBehaviour {


		public ExplosionPart[] ambientEmitters ;
		public ExplosionPart[] explosionEmitters ;
		public ExplosionPart[] smokeEmitters ;

		public ExplosionPart[] miscSpecialEffects;

		IEnumerator Start () {	
			//ExplosionPart go;
			float maxTime  = 0;

			foreach (ExplosionPart go in ambientEmitters) {
				StartCoroutine(InstantiateDelayed(go));
				//if (go.gameObject.GetComponent<ParticleEmitter>())
				if (go.gameObject.GetComponent<ParticleSystem>())
					//maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleEmitter>().maxEnergy);
					maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleSystem>().main.startLifetime.constant); //v2.3
			}
			foreach (ExplosionPart go in explosionEmitters) {
				StartCoroutine(InstantiateDelayed(go));	
				if (go.gameObject.GetComponent<ParticleSystem>())
					//maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleEmitter>().maxEnergy);
					maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleSystem>().main.startLifetime.constant); //v2.3
			}
			foreach (ExplosionPart go in smokeEmitters) {
				StartCoroutine(InstantiateDelayed(go));
				if (go.gameObject.GetComponent<ParticleSystem>())
					//maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleEmitter>().maxEnergy);
					maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleSystem>().main.startLifetime.constant); //v2.3
			}

			if (GetComponent<AudioSource>() && GetComponent<AudioSource>().clip)
				maxTime = Mathf.Max (maxTime, GetComponent<AudioSource>().clip.length);

			yield return true;

			foreach (ExplosionPart go in miscSpecialEffects) {
				StartCoroutine(InstantiateDelayed(go));
				if (go.gameObject.GetComponent<ParticleSystem>())
					//maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleEmitter>().maxEnergy);
					maxTime = Mathf.Max (maxTime, go.delay + go.gameObject.GetComponent<ParticleSystem>().main.startLifetime.constant); //v2.3
			}

			Destroy (gameObject, maxTime + 0.5f);
		}

		IEnumerator InstantiateDelayed (ExplosionPart go) {
			if (go.hqOnly && QualityManager.quality < QualityManager.Quality.High)
				//return;
				yield break;

			yield return new WaitForSeconds (go.delay);
			Instantiate (go.gameObject, transform.position + Vector3.up * go.yOffset, transform.rotation);
		}

}
}