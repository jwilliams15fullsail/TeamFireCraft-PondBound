using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class DestroyObject : MonoBehaviour {

		public GameObject objectToDestroy;

		void OnSignal () {
			Spawner.Destroy (objectToDestroy);
		}

}
}