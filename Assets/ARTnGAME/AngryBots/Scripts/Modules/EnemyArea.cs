using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class EnemyArea : MonoBehaviour {

		public List<GameObject> affected = new List<GameObject> ();

		//v2.3
		void Start(){
			ActivateAffected (false);
		}

		void OnTriggerEnter (Collider other) {
			if (other.tag == "Player")
				ActivateAffected (true);
		}

		void OnTriggerExit (Collider other) {
			if (other.tag == "Player")
				ActivateAffected (false);
		}

		IEnumerator ActivateAffected (bool state) {
			foreach (GameObject go in affected) {//for (var go : GameObject in affected) {
				if (go == null)
					continue;
				go.SetActive (state);
				yield return true; //v2.3
			}
			foreach (Transform tr in transform) {//for (var tr : Transform in transform) {
				tr.gameObject.SetActive (state);
				yield return true; //v2.3
			}
		}

}
}