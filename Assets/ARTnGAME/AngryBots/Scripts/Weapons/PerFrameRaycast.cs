using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class PerFrameRaycast : MonoBehaviour {

		private RaycastHit hitInfo;
		private Transform tr;

		void Awake () {
			tr = transform;
		}

		void Update () {
			// Cast a ray to find out the end point of the laser
			hitInfo = new RaycastHit ();
			Physics.Raycast (tr.position, tr.forward, out hitInfo);
		}

		public RaycastHit GetHitInfo () {
			return hitInfo;
		}
}
}