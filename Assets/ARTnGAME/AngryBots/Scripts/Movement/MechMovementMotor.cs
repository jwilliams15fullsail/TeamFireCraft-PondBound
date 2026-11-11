using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[RequireComponent(typeof(Rigidbody))]
	public class MechMovementMotor : MovementMotor {

		public float walkingSpeed = 3.0f;
		public float turningSpeed = 100.0f;
		public float aimingSpeed = 150.0f;

		public Transform head;

		//private var wallNormal : Vector3 = Vector3.zero;
		private Vector3 wallHit;
		private bool facingInRightDirection = false;
		private Quaternion headRotation = Quaternion.identity;

		void FixedUpdate () {
			
			Vector3 adjustedMovementDirection = movementDirection;

			// If the movement direction points into a wall as defined by the wall normal,
			// then change the movement direction to be perpendicular to the wall,
			// so the character "glides" along the wall.
			/*if (Vector3.Dot (movementDirection, wallNormal) < 0) {
			// Keep the vector length prior to adjustment
			var vectorLength : float = movementDirection.magnitude;
			// Project the movement vector onto the plane defined by the wall normal
			adjustedMovementDirection =
				movementDirection - Vector3.Project (movementDirection, wallNormal) * 0.9;
			// Apply the original length of the vector
			adjustedMovementDirection = adjustedMovementDirection.normalized * vectorLength;
		}*/

			/*Debug.DrawRay(transform.position, adjustedMovementDirection, Color.yellow);
			Debug.DrawRay(transform.position, movementDirection, Color.green);
			Debug.DrawRay(transform.position, wallNormal, Color.red);*/

			// Make the character rotate towards the target rotation
			float rotationAngle;
			if (adjustedMovementDirection != Vector3.zero)
				rotationAngle = AngleAroundAxis (transform.forward, adjustedMovementDirection, Vector3.up) * 0.3f;
			else
				rotationAngle = 0;
			Vector3 targetAngularVelocity = Vector3.up * Mathf.Clamp (rotationAngle, -turningSpeed * Mathf.Deg2Rad, turningSpeed * Mathf.Deg2Rad);
			GetComponent<Rigidbody>().angularVelocity = Vector3.MoveTowards (GetComponent<Rigidbody>().angularVelocity, targetAngularVelocity, Time.deltaTime * turningSpeed * Mathf.Deg2Rad * 3);

			/*
		if ((transform.position - wallHit).magnitude > 2) {
			wallNormal = Vector3.zero;
		}*/

			float angle = Vector3.Angle (transform.forward, adjustedMovementDirection);
			if (facingInRightDirection && angle > 25)
				facingInRightDirection = false;
			if (!facingInRightDirection && angle < 5)
				facingInRightDirection = true;

			// Handle the movement of the character
			Vector3 targetVelocity;
			if (facingInRightDirection)
				targetVelocity = transform.forward * walkingSpeed + GetComponent<Rigidbody>().linearVelocity.y * Vector3.up;
			else
				targetVelocity = GetComponent<Rigidbody>().linearVelocity.y * Vector3.up;

			GetComponent<Rigidbody>().linearVelocity = Vector3.MoveTowards (GetComponent<Rigidbody>().linearVelocity, targetVelocity, Time.deltaTime * walkingSpeed * 3);
			//transform.position += targetVelocity * Time.deltaTime * walkingSpeed * 3;
		}

		void LateUpdate () {
			// Target with head
			if (facingDirection != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation (facingDirection);
				headRotation = Quaternion.RotateTowards (
					headRotation,
					targetRotation,
					aimingSpeed * Time.deltaTime
				);
				head.rotation = headRotation * Quaternion.Inverse (transform.rotation) * head.rotation;
			}
		}

		/*
	function OnCollisionStay (collisionInfo : Collision) {
		if (collisionInfo.gameObject.tag == "Player")
			return;
		
		// Record the first wall normal
		for (var contact : ContactPoint in collisionInfo.contacts) {
			// Discard normals that are not mostly horizontal
			if (Mathf.Abs(contact.normal.y) < 0.7) {
				wallNormal = contact.normal;
				wallNormal.y = 0;
				wallHit = transform.position;
				break;
			}
		}
		
		// Only keep the horizontal components
		wallNormal.y = 0;
	}
	*/

		// The angle between dirA and dirB around axis
		static float AngleAroundAxis (Vector3 dirA,Vector3 dirB,Vector3 axis) {
			// Project A and B onto the plane orthogonal target axis
			dirA = dirA - Vector3.Project (dirA, axis);
			dirB = dirB - Vector3.Project (dirB, axis);

			// Find (positive) angle between A and B
			float angle = Vector3.Angle (dirA, dirB);

			// Return angle multiplied with 1 or -1
			return angle * (Vector3.Dot (axis, Vector3.Cross (dirA, dirB)) < 0 ? -1 : 1);
		}
}
}