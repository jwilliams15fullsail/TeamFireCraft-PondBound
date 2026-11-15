using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class PlayAnimation : MonoBehaviour {

		public string clip = "MyAnimation";

		void OnSignal () {
			GetComponent<Animation>().Play(clip);
		}
}
}