using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[RequireComponent(typeof(Rigidbody))]
	public class HoverMovementMotor : MovementMotor {

		//public var movement : MoveController;
		public float flyingSpeed = 5.0f;
		public float flyingSnappyness = 2.0f;
		public float turningSpeed = 3.0f;
		public float turningSnappyness = 3.0f;
		public float bankingAmount = 1.0f;

		void FixedUpdate () {
			// Handle the movement of the character
			Vector3 targetVelocity = movementDirection * flyingSpeed;
			Vector3 deltaVelocity = targetVelocity - GetComponent<Rigidbody>().linearVelocity;
			GetComponent<Rigidbody>().AddForce (deltaVelocity * flyingSnappyness, ForceMode.Acceleration);

			// Make the character rotate towards the target rotation
			Vector3 facingDir = facingDirection != Vector3.zero ? facingDirection : movementDirection;
			if (facingDir != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation (facingDir, Vector3.up);
				Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);
				Vector3 axis;//= Vector3.zero;
				float angle;//=0;

				deltaRotation.ToAngleAxis (out angle, out axis);

				Vector3 deltaAngularVelocity = axis * Mathf.Clamp (angle, -turningSpeed, turningSpeed) - GetComponent<Rigidbody>().angularVelocity;

				float banking = Vector3.Dot (movementDirection, -transform.right);

				GetComponent<Rigidbody>().AddTorque (deltaAngularVelocity * turningSnappyness + transform.forward * banking * bankingAmount);
			}
		}

		void OnCollisionStay (Collision collisionInfo) {
			// Move up if colliding with static geometry
			if (collisionInfo.rigidbody == null)
				GetComponent<Rigidbody>().linearVelocity += Vector3.up * Time.deltaTime * 50;
		}
}
}