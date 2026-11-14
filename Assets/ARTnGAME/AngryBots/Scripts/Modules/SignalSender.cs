using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {
	
	[System.Serializable]
	public class ReceiverItem {
		public GameObject receiver;
		public string action = "OnSignal";
		public float delay;

		public IEnumerator SendWithDelay (MonoBehaviour sender) {
			 
			//yield WaitForSeconds (delay); //v2.3
			yield return new WaitForSeconds(delay);

			if (receiver)
				receiver.SendMessage (action);
			else
				Debug.LogWarning ("No receiver of signal \""+action+"\" on object "+sender.name+" ("+sender.GetType().Name+")", sender);
		}
	}
	[System.Serializable]
	public class SignalSender  { //v2.3 MUST NOT INHERIT FROM MONO, otherwise wont expose ReceiverItem properties in other scripts

		[SerializeField]
		public bool onlyOnce;
		[SerializeField]			
		public ReceiverItem[] receivers;

		private bool hasFired = false;

		public void SendSignals (MonoBehaviour sender) {
			if (hasFired == false || onlyOnce == false) {
				for (int i = 0; i < receivers.Length; i++) {
					sender.StartCoroutine (receivers[i].SendWithDelay(sender));
				}
				hasFired = true;
			}
		}

	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}
}
}
