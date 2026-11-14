using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class SimpleRotate : MonoBehaviour {

		public float speed = 4.0f;

		void OnBecameVisible () {
			enabled = true;	
		}

		void OnBecameInvisible () {
			enabled = false;	
		}

		void Update () {
			transform.Rotate(0.0f, 0.0f, Time.deltaTime * speed);
		}
}
}