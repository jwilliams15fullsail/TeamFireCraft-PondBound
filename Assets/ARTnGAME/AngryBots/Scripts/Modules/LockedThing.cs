using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class LockedThing : MonoBehaviour {

		// This component will forward a signal only if all the locks are unlocked

		public Lock[] locks;
		public SignalSender conditionalSignal;

		void OnSignal () {
			bool locked = false;
			foreach (Lock lockObj in locks) {//for (var lockObj : Lock in locks) {
				if (lockObj.locked)
					locked = true;
			}

			if (locked == false)
				conditionalSignal.SendSignals (this);
		}

}
}