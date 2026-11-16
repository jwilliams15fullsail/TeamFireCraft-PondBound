using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class ShiftUV : MonoBehaviour {

		public Vector2 offsetVector;

		void Start () {
		}

		void OnSignal () {
			GetComponent<Renderer>().material.mainTextureOffset += offsetVector;
		}

}
}