using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Artngame.PDM {
	[RequireComponent(typeof(BoxCollider))]
public class EndOfLevel : MonoBehaviour {

		public float timeToTriggerLevelEnd = 2.0f;
		public string endSceneName = "3-4_Pain";


		IEnumerator OnTriggerEnter (Collider other) {
			if (other.tag == "Player") {

				FadeOutAudio ();	

				PlayerMoveController playerMove = other.gameObject.GetComponent<PlayerMoveController> ();
				playerMove.enabled = false;

				yield return true;

				float timeWaited = 0.0f;
				FreeMovementMotor playerMotor = other.gameObject.GetComponent<FreeMovementMotor> ();
				while (playerMotor.walkingSpeed > 0.0f) {
					playerMotor.walkingSpeed -= Time.deltaTime * 6.0f;
					if (playerMotor.walkingSpeed < 0.0f)
						playerMotor.walkingSpeed = 0.0f;
					timeWaited += Time.deltaTime;
					yield return true;
				}
				playerMotor.walkingSpeed = 0.0f;		

				yield return new WaitForSeconds ( Mathf.Clamp (timeToTriggerLevelEnd - timeWaited, 0.0f, timeToTriggerLevelEnd));
				Camera.main.gameObject.SendMessage ("WhiteOut");

				yield return new WaitForSeconds (2.0f);

				//v2.1
				SceneManager.LoadScene(endSceneName);
				//Application.LoadLevel (endSceneName);
			}
		}

		IEnumerator FadeOutAudio () {
			AudioListener al = Camera.main.gameObject.GetComponent<AudioListener> ();
			if (al) {
				while (AudioListener.volume > 0.0f) {//while (al.volume > 0.0f) {
					AudioListener.volume -= Time.deltaTime / timeToTriggerLevelEnd;//al.volume -= Time.deltaTime / timeToTriggerLevelEnd;
					yield return true;	
				}	
			}		
		}
}
}