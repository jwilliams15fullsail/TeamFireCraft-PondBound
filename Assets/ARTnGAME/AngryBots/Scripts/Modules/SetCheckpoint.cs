using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class SetCheckpoint : MonoBehaviour {

		public Transform spawnTransform;

		void OnTriggerEnter (Collider other) {
			SpawnAtCheckpoint checkpointKeeper = other.GetComponent<SpawnAtCheckpoint> () as SpawnAtCheckpoint;
			checkpointKeeper.checkpoint = spawnTransform;
		}
}
}