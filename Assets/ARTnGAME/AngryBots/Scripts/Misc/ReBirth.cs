using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[RequireComponent(typeof(Camera))]
public class ReBirth : MonoBehaviour {

		void Start () {
			AudioListener al = null;
			al = Camera.main.gameObject.GetComponent<AudioListener> ();

			if (al)
				AudioListener.volume = 1.0f;

			ShaderDatabase sm = GetComponent<ShaderDatabase> ();
			sm.WhiteIn ();

			GetComponent<Camera>().backgroundColor = Color.white;
		}
}
}