using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class StaticFollower : MonoBehaviour {

		public Transform target;

		private Vector3 relativePos;

		void Awake () {
			relativePos = transform.position - target.position;
		}

		void LateUpdate () {
			transform.position = target.position + relativePos;
		}
}
}