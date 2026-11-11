using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class SlowBulletFire : MonoBehaviour {

		public GameObject bulletPrefab;
		public float frequency = 2;
		public float coneAngle = 1.5f;
		public AudioClip fireSound;
		public bool firing = false;

		private float lastFireTime = -1;

		void Update () {
			if (firing) {
				if (Time.time > lastFireTime + 1 / frequency) {
					Fire ();
				}
			}
		}

		void Fire () {
			// Spawn bullet
			var coneRandomRotation = Quaternion.Euler (Random.Range (-coneAngle, coneAngle), Random.Range (-coneAngle, coneAngle), 0);
			Spawner.Spawn (bulletPrefab, transform.position, transform.rotation * coneRandomRotation);

			if (GetComponent<AudioSource>() && fireSound) {
				GetComponent<AudioSource>().clip = fireSound;
				GetComponent<AudioSource>().Play ();
			}

			lastFireTime = Time.time;
		}

		void OnStartFire () {
			firing = true;
		}

		void OnStopFire () {
			firing = false;
		}
}
}