using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class MovementMotor : MonoBehaviour {

	[HideInInspector]
	public Vector3 movementDirection;

	// Simpler motors might want to drive movement based on a target purely
	[HideInInspector]
	public Vector3 movementTarget;

	// The direction the character wants to face towards, in world space.
	[HideInInspector]
	public Vector3 facingDirection;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
}
