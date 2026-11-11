using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Artngame.PDM {
	[System.Serializable]
public class TriggerOnMouseOrJoystick : MonoBehaviour {

		[SerializeField]
		public SignalSender mouseDownSignals = new SignalSender();
		public SignalSender mouseUpSignals = new SignalSender();

		private bool state = false;

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
        //private var joysticks : Joystick[];
        private Joystick[] joysticks; //private var joysticks : Joystick[]; //v2.5

        void Start() //function Start () //v2.5
        {
            joysticks = FindObjectsOfType(typeof(Joystick)) as Joystick[];          

            //function Start () {
            //joysticks = FindObjectsOfType (Joystick) as Joystick[];	
        }
		#endif

		void Update () {
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			if (state == false && joysticks[0].tapCount > 0) {
			mouseDownSignals.SendSignals (this);
			state = true;
			}
			else if (joysticks[0].tapCount <= 0) {
			mouseUpSignals.SendSignals (this);
			state = false;
			}	
#else
#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)
			// On consoles use the right trigger to fire
			var fireAxis : float = Input.GetAxis("TriggerFire");
			if (state == false && fireAxis >= 0.2) {
			mouseDownSignals.SendSignals (this);
			state = true;
			}
			else if (state == true && fireAxis < 0.2) {
			mouseUpSignals.SendSignals (this);
			state = false;
			}
#else
			//if (state == false && Input.GetMouseButtonDown (0)) {
			//	mouseDownSignals.SendSignals (this);
			//	state = true;
			//}

			//else if (state == true && Input.GetMouseButtonUp (0)) {
			//	mouseUpSignals.SendSignals (this);
			//	state = false;
			//}
			bool mouseDown = false;
			bool mouseUp = false;

#if ENABLE_LEGACY_INPUT_MANAGER

mouseDown = Input.GetMouseButtonDown(0);
mouseUp   = Input.GetMouseButtonUp(0);

#elif ENABLE_INPUT_SYSTEM

			if (Mouse.current != null)
			{
				mouseDown = Mouse.current.leftButton.wasPressedThisFrame;
				mouseUp = Mouse.current.leftButton.wasReleasedThisFrame;
			}

#endif

			if (state == false && mouseDown)
			{
				mouseDownSignals.SendSignals(this);
				state = true;
			}
			else if (state == true && mouseUp)
			{
				mouseUpSignals.SendSignals(this);
				state = false;
			}
#endif
#endif
		}

}
}