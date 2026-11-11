using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM{
public class ExplosionControl : MonoBehaviour {


		public GameObject[] trails;
		public ParticleSystem emitter;//public ParticleEmitter emitter; //v2.3
		public LineRenderer[] lineRenderer;
		public GameObject lightDecal;

		public float autoDisableAfter = 2.0f;

		void Awake ()
		{
			for (int i = 0; i < lineRenderer.Length; i++) {
				float lineWidth = Random.Range(0.25f,0.5f);

				//lineRenderer[i].SetWidth (lineWidth, lineWidth);
				lineRenderer[i].startWidth = lineRenderer[i].endWidth = lineWidth; //v2.3

				lineRenderer[i].SetPosition (0, Vector3.zero);

				Vector3 dir = Random.onUnitSphere;
				dir.y = Mathf.Abs (dir.y);

				lineRenderer[i].SetPosition (1, dir * Random.Range (8.0f, 12.0f));
			}
		}

		void OnEnable()
		{
			lightDecal.transform.localScale = Vector3.one;

			lightDecal.SetActive (true);

			for (int i = 0; i < trails.Length; i++) {
				trails[i].transform.localPosition = Vector3.zero;
				trails[i].SetActive (true);
				(trails[i].GetComponent<ExplosionTrail> ()).enabled = true;
			}

			for(int i = 0; i < lineRenderer.Length; i++) {
				lineRenderer[i].transform.localPosition = Vector3.zero;
				lineRenderer[i].gameObject.SetActive (true);
				lineRenderer[i].enabled = true;
			}

			//v2.3
			//emitter.emit = true;
			//emitter.enabled = true;
			ParticleSystem.EmissionModule emitterModule = emitter.emission;
			emitterModule.enabled = true;
			//emitter.Emit(

			emitter.gameObject.SetActive (true);

			//v2.3
			//Invoke("DisableEmitter", emitter.maxEnergy);
			Invoke("DisableEmitter", emitter.main.startLifetime.constant);

			Invoke("DisableStuff", autoDisableAfter);
		}

		void DisableEmitter() {
			//v2.3
			//emitter.emit = false;
			//emitter.enabled = false;
			ParticleSystem.EmissionModule emitterModule = emitter.emission;
			emitterModule.enabled = false;
		}

		void DisableStuff()
		{
			gameObject.SetActive(false);
		}



}
}