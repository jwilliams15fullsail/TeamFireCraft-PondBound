using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class MechAttackMoveController : MonoBehaviour {


		// Public member data
		public MovementMotor motor;
		public Transform head;

		public float targetDistanceMin = 3.0f;
		public float targetDistanceMax = 4.0f;

		public MonoBehaviour[] weaponBehaviours;
		public float fireFrequency = 2;

		// Private memeber data
		private AI ai;

		private Transform character;

		private Transform player;

		private bool inRange = false;
		private float nextRaycastTime = 0;
		private float lastRaycastSuccessfulTime  = 0;
		private float noticeTime = 0;

		private bool firing = false;
		private float lastFireTime = -1;
		private int nextWeaponToFire = 0;

		void Awake () {
			character = motor.transform;
			player = GameObject.FindWithTag ("Player").transform;
			ai = transform.parent.GetComponentInChildren<AI> ();
		}

		void OnEnable () {
			inRange = false;
			nextRaycastTime = Time.time + 1;
			lastRaycastSuccessfulTime = Time.time;
			noticeTime = Time.time;
		}

		void OnDisable () {
			Shoot (false);
		}

		void Shoot (bool state) {
			firing = state;
		}

		void Fire () {
			if (weaponBehaviours[nextWeaponToFire]) {
				weaponBehaviours[nextWeaponToFire].SendMessage ("Fire");
				nextWeaponToFire = (nextWeaponToFire + 1) % weaponBehaviours.Length;
				lastFireTime = Time.time;
			}
		}

		void Update () {
			// Calculate the direction from the player to this character
			Vector3 playerDirection = (player.position - character.position);
			playerDirection.y = 0;
			float playerDist = playerDirection.magnitude;
			playerDirection /= playerDist;

			// Set this character to face the player,
			// that is, to face the direction from this character to the player
			motor.facingDirection = playerDirection;

			// For a short moment after noticing player,
			// only look at him but don't walk towards or attack yet.
			if (Time.time < noticeTime + 1.5f) {
				motor.movementDirection = Vector3.zero;
				return;
			}

			if (inRange && playerDist > targetDistanceMax)
				inRange = false;
			if (!inRange && playerDist < targetDistanceMin)
				inRange = true;

			if (inRange)
				motor.movementDirection = Vector3.zero;
			else
				motor.movementDirection = playerDirection;

			if (Time.time > nextRaycastTime) {
				nextRaycastTime = Time.time + 1;
				if (ai.CanSeePlayer ()) {
					lastRaycastSuccessfulTime = Time.time;
					if (IsAimingAtPlayer ())
						Shoot (true);
					else
						Shoot (false);
				}
				else {
					Shoot (false);
					if (Time.time > lastRaycastSuccessfulTime + 5) {
						ai.OnLostTrack ();
					}
				}
			}

			if (firing) {
				if (Time.time > lastFireTime + 1 / fireFrequency) {
					Fire ();
				}
			}
		}

		bool IsAimingAtPlayer () {
			Vector3 playerDirection= (player.position - head.position);
			playerDirection.y = 0;
			return Vector3.Angle (head.forward, playerDirection) < 15;
		}
}
}