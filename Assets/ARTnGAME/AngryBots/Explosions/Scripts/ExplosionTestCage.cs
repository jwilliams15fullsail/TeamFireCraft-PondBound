using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM{
public class ExplosionTestCage : MonoBehaviour {

		public GameObject explPrefab;

		void OnTriggerEnter(Collider other) {
			if(other.GetComponent<Collider>().tag == "Player") {
				//GameObject go = Instantiate(explPrefab, transform.position, transform.rotation);	
				Instantiate(explPrefab, transform.position, transform.rotation); //v2.3	
			}	
		}
}
}