using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class SelfIlluminationBlink : MonoBehaviour {

		public float blink = 0.0f;

		void OnWillRenderObject () {
			GetComponent<Renderer>().sharedMaterial.SetFloat ("_SelfIllumStrength", blink);	
		}

		public void Blink () {
			blink = 1.0f - blink;
		}
}
}