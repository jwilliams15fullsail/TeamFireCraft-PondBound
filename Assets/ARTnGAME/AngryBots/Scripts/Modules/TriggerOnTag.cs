using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class TriggerOnTag : MonoBehaviour {


		public string triggerTag = "Player";
		public SignalSender enterSignals;
		public SignalSender exitSignals;

		void OnTriggerEnter (Collider other) {
			if (other.isTrigger)
				return;

			if (other.gameObject.tag == triggerTag || triggerTag == "") {
				enterSignals.SendSignals (this);
			}
		}

		void OnTriggerExit (Collider other) {
			if (other.isTrigger)
				return;

			if (other.gameObject.tag == triggerTag || triggerTag == "") {
				exitSignals.SendSignals (this);
			}
		}
}
}