using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnTrigger : MonoBehaviour {

		public  bool onlyPlayOnce = true;

		private  bool playedOnce = false;

		void OnTriggerEnter () {
			if (playedOnce && onlyPlayOnce)
				return;

			GetComponent<AudioSource>().Play ();
			playedOnce = true;
		}
}
}