using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[System.Serializable]
public class PatrolPoint : MonoBehaviour {
		
		public Vector3 position;

		void Awake () {
			position = transform.position;
		}

}
}