using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	
public class SpawnAtCheckpoint : MonoBehaviour {

		public Transform checkpoint;

		void OnSignal () {
			transform.position = checkpoint.position;
			transform.rotation = checkpoint.rotation;

			ResetHealthOnAll ();
		}

		void ResetHealthOnAll () {
			Health[] healthObjects = FindObjectsOfType (typeof(Health)) as Health[];
			foreach (Health health in healthObjects) {//for (var health : Health in healthObjects) {
				health.dead = false;
				health.health = health.maxHealth;
			}
		}

}
}