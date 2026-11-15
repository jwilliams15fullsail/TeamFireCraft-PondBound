using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Artngame.PDM {
public class TextAdventureManager : MonoBehaviour {

		public Transform player;
		public MoodBox[] playableMoodBoxes;

		public float timePerChar = 0.125f;

		private int currentMoodBox = 0;
		private int textAnimation = 0;
		private float timer = 0.0f;
		private Vector3 camOffset = Vector3.zero;

		void Start () {
			if (!player)
				player = GameObject.FindWithTag ("Player").transform;	

			GameObject leftIcon = new GameObject ("Left Arrow", typeof(Text));
			GameObject rightIcon = new GameObject ("Right Arrow", typeof(Text));

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
            leftIcon.name = "<";//leftIcon.guiText.text = "<"; //v0.1
#else
			leftIcon.GetComponent<Text>().text = "< backspace";		
#endif

            leftIcon.GetComponent<Text>().font = GetComponent<Text>().font;
			leftIcon.GetComponent<Text>().material = GetComponent<Text>().material;
            //leftIcon.GetComponent<Text>().anchor = TextAnchor.UpperLeft;
            leftIcon.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            leftIcon.gameObject.layer = ( LayerMask.NameToLayer ("Adventure"));

			//v2.3
			//leftIcon.transform.position.x = 0.01f;
			//leftIcon.transform.position.y = 0.1f;
			leftIcon.transform.position = new Vector3( 0.01f, 0.1f,leftIcon.transform.position.z);


#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
            rightIcon.name = ">";//rightIcon.guiText.text = ">"; //v0.1
#else
			rightIcon.GetComponent<Text>().text = "space >";		
#endif
            rightIcon.GetComponent<Text>().font = GetComponent<Text>().font;
			rightIcon.GetComponent<Text>().material = GetComponent<Text>().material;
            //rightIcon.GetComponent<Text>().anchor = TextAnchor.UpperRight;
            rightIcon.GetComponent<Text>().alignment = TextAnchor.UpperRight;

            rightIcon.gameObject.layer = ( LayerMask.NameToLayer ("Adventure"));

			//v2.3
			//rightIcon.transform.position.x = 0.99f;
			//rightIcon.transform.position.y = 0.1f;		
			rightIcon.transform.position = new Vector3(0.99f, 0.1f,leftIcon.transform.position.z);
		}

		void OnEnable () {	
			textAnimation = 0;
			timer = timePerChar;

			camOffset = Camera.main.transform.position - player.position;

			BeamToBox (currentMoodBox);

			if (player) {
				PlayerMoveController ctrler = player.GetComponent<PlayerMoveController> ();
				ctrler.enabled = false;		
			}

			GetComponent<Text>().enabled = true;
		}

		void OnDisable () {
			// and back to normal player control

			if (player) {
				PlayerMoveController ctrler = player.GetComponent<PlayerMoveController> ();
				ctrler.enabled = true;		
			}

			GetComponent<Text>().enabled = false;	
		}

		void Update () {
			GetComponent<Text>().text = "AngryBots \n \n";
			GetComponent<Text>().text += playableMoodBoxes[currentMoodBox].data.adventureString.Substring (0, textAnimation);

			Debug.Log (GetComponent<Text>().text);

			if (textAnimation >= playableMoodBoxes[currentMoodBox].data.adventureString.Length ) {

			}
			else {
				timer -= Time.deltaTime;
				if (timer <= 0.0f) {
					textAnimation++;
					timer = timePerChar;
				}
			}

			CheckInput ();
		}

		void BeamToBox (int index) {
			if (index > playableMoodBoxes.Length)
				return;

			player.position = playableMoodBoxes[index].transform.position;
			Camera.main.transform.position = player.position + camOffset;
			textAnimation = 0;
			timer = timePerChar;
		}

		void CheckInput () {
			int input = 0;

			#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY

			foreach (Touch touch in Input.touches) {//for (var touch : Touch in Input.touches) { //v2.3

			if (touch.phase == TouchPhase.Ended && touch.phase != TouchPhase.Canceled) {
			if (touch.position.x < Screen.width / 2)
			input = -1;
			else 
			input = 1;
			}
			}
			#else
			if (Input.GetKeyUp (KeyCode.Space))
				input = 1;
			else if (Input.GetKeyUp (KeyCode.Backspace))
				input = -1;
			#endif

			if (input != 0) {
				if (textAnimation < playableMoodBoxes[currentMoodBox].data.adventureString.Length) {
					textAnimation = playableMoodBoxes[currentMoodBox].data.adventureString.Length;
					input = 0;
				}
			}

			if (input != 0) {
				if ((currentMoodBox - playableMoodBoxes.Length) == -1 && input < 0) 
					input = 0;
				if (currentMoodBox == 0 && input < 0)
					input = 0;

				if (input > 0) { //v2.3
					currentMoodBox = (input + currentMoodBox) % playableMoodBoxes.Length;
					BeamToBox (currentMoodBox);
				}
			}
		}
}
}