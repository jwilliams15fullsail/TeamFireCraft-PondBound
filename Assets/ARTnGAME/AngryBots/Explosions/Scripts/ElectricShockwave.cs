using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM{
public class ElectricShockwave : MonoBehaviour {

		public float autoDisableAfter = 2.0f;

		void OnEnable ()
		{
			DeactivateCoroutine (autoDisableAfter);
		}


		IEnumerator DeactivateCoroutine (float t)
		{
			yield return new WaitForSeconds(t);

			gameObject.SetActive (false);
		}

}
}