using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class MechAnimationTest : MonoBehaviour {

		#if !UNITY_FLASH

		public float turning = 0;
		public float walking = 0;
		public float turnOffset = 0.0f;

		public Rigidbody rigid;
		public AnimationClip idle;
		public AnimationClip walk;
		public AnimationClip turnLeft;
		public AnimationClip turnRight;
		public SignalSender footstepSignals;

		void OnEnable () {

			GetComponent<Animation>()[idle.name].layer = 0;
			GetComponent<Animation>()[idle.name].weight = 1;
			GetComponent<Animation>()[idle.name].enabled = true;

			GetComponent<Animation>()[walk.name].layer = 1;
			GetComponent<Animation>()[turnLeft.name].layer = 1;
			GetComponent<Animation>()[turnRight.name].layer = 1;

			GetComponent<Animation>()[walk.name].weight = 1;
			GetComponent<Animation>()[turnLeft.name].weight = 0;
			GetComponent<Animation>()[turnRight.name].weight = 0;

			GetComponent<Animation>()[walk.name].enabled = true;
			GetComponent<Animation>()[turnLeft.name].enabled = true;
			GetComponent<Animation>()[turnRight.name].enabled = true;

			//animation[walk.name].speed = 0.93;

			//animation.Play ();
		}

		void FixedUpdate () {
			GetComponent<Animation>()[walk.name].speed = Mathf.Lerp (1, GetComponent<Animation>()[walk.name].length / GetComponent<Animation>()[turnLeft.name].length, Mathf.Abs (turning));

			GetComponent<Animation>()[turnLeft.name].time = GetComponent<Animation>()[walk.name].time + turnOffset;
			GetComponent<Animation>()[turnRight.name].time = GetComponent<Animation>()[walk.name].time + turnOffset;

			rigid.linearVelocity = rigid.transform.forward * 2.5f * walking;
			rigid.angularVelocity = Vector3.up * turning * 100 * Mathf.Deg2Rad;

			float turningWeight = rigid.angularVelocity.y * Mathf.Rad2Deg / 100.0f;
			float forwardWeight = rigid.linearVelocity.magnitude / 2.5f;

			GetComponent<Animation>()[turnLeft.name].weight = Mathf.Clamp01 (-turningWeight);
			GetComponent<Animation>()[turnRight.name].weight = Mathf.Clamp01 (turningWeight);
			GetComponent<Animation>()[walk.name].weight = Mathf.Clamp01 (forwardWeight);
		}

		void OnGUI () {
			GUILayout.Label ("Walking (0 to 1): "+walking.ToString("0.00"));
			walking = GUILayout.HorizontalSlider (walking, 0, 1, GUILayout.Width (100));
			if (GUI.changed) {
				turning = Mathf.Clamp (Mathf.Abs (turning), 0, 1 - walking) * Mathf.Sign (turning);
				GUI.changed = false;
			}

			GUILayout.Label ("Turning (-1 to 1): "+turning.ToString("0.00"));
			turning = GUILayout.HorizontalSlider (turning, -1, 1, GUILayout.Width (100));
			if (Mathf.Abs (turning) < 0.1f)
				turning = 0;
			if (GUI.changed) {
				walking = Mathf.Clamp (walking, 0, 1 - Mathf.Abs (turning));
				GUI.changed = false;
			}

			GUILayout.Label ("Offset to turning anims (-0.5 to 0.5): "+turnOffset.ToString("0.00"));
			turnOffset = GUILayout.HorizontalSlider (turnOffset, -0.5f, 0.5f, GUILayout.Width (100));
			if (Mathf.Abs (turnOffset) < 0.05f)
				turnOffset = 0;
		}
		#endif
}
}