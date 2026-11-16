using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[RequireComponent(typeof(Health))]
public class TerminalHack : MonoBehaviour {


		private Health health;
		private Animation animationComp;

		//health = GetComponent.<Health> ();
		//animationComp = GetComponentInChildren.<Animation> ();

		void Start () {

			//v2.3
			health = GetComponent<Health> ();
			animationComp = GetComponentInChildren<Animation> ();

			UpdateHackingProgress ();
			enabled = false;
		}

		void OnTriggerStay (Collider other) {
			if (other.gameObject.tag == "Player")
				health.OnDamage (Time.deltaTime, Vector3.zero);
		}

		void OnHacking () {
			enabled = true;
			UpdateHackingProgress ();
		}

		void OnHackingCompleted () {
			GetComponent<AudioSource>().Play ();
			animationComp.Stop ();
			enabled = false;
		}

		void UpdateHackingProgress () {
			animationComp.clip.SampleAnimation (animationComp.gameObject, (1 - health.health / health.maxHealth) * animationComp.clip.length);
		}

		void Update () {;
			UpdateHackingProgress ();

			if (health.health == 0 || health.health == health.maxHealth) {
				UpdateHackingProgress ();
				enabled = false;
			}
		}
}
}