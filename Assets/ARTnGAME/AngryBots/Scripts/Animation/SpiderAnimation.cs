using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class SpiderAnimation : MonoBehaviour {


		public MovementMotor motor;
		public AnimationClip activateAnim;
		public AnimationClip forwardAnim;
		public AnimationClip backAnim;
		public AnimationClip leftAnim;
		public AnimationClip rightAnim;
		public AudioSource audioSource;
		public SignalSender footstepSignals;
		public bool skiddingSounds;
		public bool footstepSounds;

		private Transform tr;
		private float lastFootstepTime = 0;
		private float lastAnimTime = 0;

		void OnEnable () {
			
			tr = motor.transform;

			GetComponent<Animation>()[activateAnim.name].enabled = true;
			GetComponent<Animation>()[activateAnim.name].weight = 1;
			GetComponent<Animation>()[activateAnim.name].time = 0;
			GetComponent<Animation>()[activateAnim.name].speed = 1;

			GetComponent<Animation>()[forwardAnim.name].layer = 1;
			GetComponent<Animation>()[forwardAnim.name].enabled = true;
			GetComponent<Animation>()[forwardAnim.name].weight = 0;
			GetComponent<Animation>()[backAnim.name].layer = 1;
			GetComponent<Animation>()[backAnim.name].enabled = true;
			GetComponent<Animation>()[backAnim.name].weight = 0;
			GetComponent<Animation>()[leftAnim.name].layer = 1;
			GetComponent<Animation>()[leftAnim.name].enabled = true;
			GetComponent<Animation>()[leftAnim.name].weight = 0;
			GetComponent<Animation>()[rightAnim.name].layer = 1;
			GetComponent<Animation>()[rightAnim.name].enabled = true;
			GetComponent<Animation>()[rightAnim.name].weight = 0;

		}

		void OnDisable () {
			GetComponent<Animation>()[activateAnim.name].enabled = true;
			GetComponent<Animation>()[activateAnim.name].weight = 1;
			GetComponent<Animation>()[activateAnim.name].normalizedTime = 1;
			GetComponent<Animation>()[activateAnim.name].speed = -1;
			GetComponent<Animation>().CrossFade (activateAnim.name, 0.3f, PlayMode.StopAll);
		}

		void Update () {
			Vector3 direction = motor.movementDirection;
			direction.y = 0;

			float walkWeight = direction.magnitude;

			GetComponent<Animation>()[forwardAnim.name].speed = walkWeight;
			GetComponent<Animation>()[rightAnim.name].speed = walkWeight;
			GetComponent<Animation>()[backAnim.name].speed = walkWeight;
			GetComponent<Animation>()[leftAnim.name].speed = walkWeight;

			float angle = Mathf.DeltaAngle (
				HorizontalAngle (tr.forward),
				HorizontalAngle (direction)
			);

			if (walkWeight > 0.01f) {
				float w;
				if (angle < -90) {
					w = Mathf.InverseLerp (-180, -90, angle);
					GetComponent<Animation>()[forwardAnim.name].weight = 0;
					GetComponent<Animation>()[rightAnim.name].weight = 0;
					GetComponent<Animation>()[backAnim.name].weight = 1 - w;
					GetComponent<Animation>()[leftAnim.name].weight = 1;
				}
				else if (angle < 0) {
					w = Mathf.InverseLerp (-90, 0, angle);
					GetComponent<Animation>()[forwardAnim.name].weight = w;
					GetComponent<Animation>()[rightAnim.name].weight = 0;
					GetComponent<Animation>()[backAnim.name].weight = 0;
					GetComponent<Animation>()[leftAnim.name].weight = 1 - w;
				}
				else if (angle < 90) {
					w = Mathf.InverseLerp (0, 90, angle);
					GetComponent<Animation>()[forwardAnim.name].weight = 1 - w;
					GetComponent<Animation>()[rightAnim.name].weight = w;
					GetComponent<Animation>()[backAnim.name].weight = 0;
					GetComponent<Animation>()[leftAnim.name].weight = 0;
				}
				else {
					w = Mathf.InverseLerp (90, 180, angle);
					GetComponent<Animation>()[forwardAnim.name].weight = 0;
					GetComponent<Animation>()[rightAnim.name].weight = 1 - w;
					GetComponent<Animation>()[backAnim.name].weight = w;
					GetComponent<Animation>()[leftAnim.name].weight = 0;
				}
			}

			if (skiddingSounds) {
				if (walkWeight > 0.2f && !audioSource.isPlaying)
					audioSource.Play ();
				else if (walkWeight < 0.2f && audioSource.isPlaying)
					audioSource.Pause ();
			}

			if (footstepSounds && walkWeight > 0.2f) {
				float newAnimTime = Mathf.Repeat (GetComponent<Animation>()[forwardAnim.name].normalizedTime * 4 + 0.1f, 1);
				if (newAnimTime < lastAnimTime) {
					if (Time.time > lastFootstepTime + 0.1f) {
						footstepSignals.SendSignals (this);
						lastFootstepTime = Time.time;
					}
				}
				lastAnimTime = newAnimTime;
			}
		}

		static float HorizontalAngle (Vector3 direction) {
			return Mathf.Atan2 (direction.x, direction.z) * Mathf.Rad2Deg;
		}

}
}