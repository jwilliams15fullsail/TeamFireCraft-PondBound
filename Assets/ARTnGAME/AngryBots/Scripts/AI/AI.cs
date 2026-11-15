using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#pragma strict

namespace Artngame.PDM {	
public class AI : MonoBehaviour {

		// Public member data
		public MonoBehaviour behaviourOnSpotted;
		public AudioClip soundOnSpotted;
		public MonoBehaviour behaviourOnLostTrack;

		// Private memeber data
		private Transform character;
		private Transform player;
		private bool insideInterestArea = true;

		void Awake () {
			character = transform;
			player = GameObject.FindWithTag ("Player").transform;
		}

		void OnEnable () {
			behaviourOnLostTrack.enabled = true;
			behaviourOnSpotted.enabled = false;
		}

		void OnTriggerEnter (Collider other) {
			if (other.transform == player && CanSeePlayer ()) {
				OnSpotted ();
			}
		}

		public void OnEnterInterestArea () {
			insideInterestArea = true;
		}

		public void OnExitInterestArea () {
			insideInterestArea = false;
			OnLostTrack ();
		}

		public void OnSpotted () {
			if (!insideInterestArea)
				return;
			if (!behaviourOnSpotted.enabled) {
				behaviourOnSpotted.enabled = true;
				behaviourOnLostTrack.enabled = false;

				if (GetComponent<AudioSource>() && soundOnSpotted) {
					GetComponent<AudioSource>().clip = soundOnSpotted;
					GetComponent<AudioSource>().Play ();
				}
			}
		}

		public void OnLostTrack () {
			if (!behaviourOnLostTrack.enabled) {
				behaviourOnLostTrack.enabled = true;
				behaviourOnSpotted.enabled = false;
			}
		}

		public bool CanSeePlayer () {
			Vector3 playerDirection = (player.position - character.position);
			RaycastHit hit;
			Physics.Raycast (character.position, playerDirection, out hit, playerDirection.magnitude);
			if (hit.collider && hit.collider.transform == player) {
				return true;
			}
			return false;
		}

//		// Use this for initialization
//		void Start () {
//			
//		}
//		
//		// Update is called once per frame
//		void Update () {
//			
//		}

}
}
