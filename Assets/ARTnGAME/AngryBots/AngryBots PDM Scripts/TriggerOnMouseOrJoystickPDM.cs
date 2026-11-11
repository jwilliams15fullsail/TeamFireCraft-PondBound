using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Artngame.PDM{
public class TriggerOnMouseOrJoystickPDM : MonoBehaviour {


		public SignalSender mouseDownSignals ;
		public SignalSender mouseUpSignals ;

		public bool is_ice = false;

		private bool state  = false;

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
        private Joystick[] joysticks; //private var joysticks : Joystick[]; //v2.5

        void Start() //function Start () //v2.5
        { 
            joysticks = FindObjectsOfType (typeof(Joystick)) as Joystick[];	
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
			//if (this.gameObject.GetComponent<ParticleSystem>() !=null){
			//	if (Input.GetMouseButton (0) && !is_ice){

			//		//v2.1
			//		var em = this.gameObject.GetComponent<ParticleSystem>().emission; //ParticleSystem.EmissionModule
			//		em.enabled = true;
			//		//this.gameObject.GetComponent.<ParticleSystem>().enableEmission=true;
			//	}else if(!is_ice){

			//		//v2.1
			//		var em1 = this.gameObject.GetComponent<ParticleSystem>().emission; //ParticleSystem.EmissionModule
			//		em1.enabled = false;
			//		//this.gameObject.GetComponent.<ParticleSystem>().enableEmission=false;
			//	}

			//	if (Input.GetMouseButton (1) && is_ice){

			//		//v2.1
			//		var em2 = this.gameObject.GetComponent<ParticleSystem>().emission; //ParticleSystem.EmissionModule
			//		em2.enabled = true;
			//		//this.gameObject.GetComponent.<ParticleSystem>().enableEmission=true;
			//	}else if(is_ice){

			//		//v2.1
			//		var em3 = this.gameObject.GetComponent<ParticleSystem>().emission; //ParticleSystem.EmissionModule
			//		em3.enabled = false;
			//		//this.gameObject.GetComponent.<ParticleSystem>().enableEmission=false;
			//	}
			//}
			if (this.gameObject.GetComponent<ParticleSystem>() != null)
			{
				bool mouse0 = false;
				bool mouse1 = false;

#if ENABLE_LEGACY_INPUT_MANAGER

    mouse0 = Input.GetMouseButton(0);
    mouse1 = Input.GetMouseButton(1);

#elif ENABLE_INPUT_SYSTEM

				if (UnityEngine.InputSystem.Mouse.current != null)
				{
					mouse0 = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
					mouse1 = UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
				}

#endif

				var em = this.gameObject.GetComponent<ParticleSystem>().emission;

				if (!is_ice)
				{
					em.enabled = mouse0;
				}
				else
				{
					em.enabled = mouse1;
				}
			}


		}
}
}