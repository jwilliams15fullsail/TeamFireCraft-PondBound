using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using System;
namespace Artngame.RiverStudio
{
    public class RotateLight : MonoBehaviour
    {

        public float speed = 50.0f;

		public Vector3 axis = new Vector3(1,0,0);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		public Key decrementKey = Key.NumpadMinus;
		public Key incrementKey = Key.NumpadPlus;
#else
		public KeyCode decrementKey = KeyCode.KeypadMinus;
		public KeyCode incrementKey = KeyCode.KeypadPlus;
#endif

		// Use this for initialization
		void Start()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (decrementKey == Key.None)
            {
                decrementKey = Key.NumpadMinus;
            }
            if (incrementKey == Key.None)
            {
                incrementKey = Key.NumpadPlus;
            }
#endif
        }
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Safe key check:
        /// - ignores Key.None
        /// - avoids ArgumentOutOfRangeException
        /// </summary>
        private static bool IsKeyHeld(Key key)
        {
            if (Keyboard.current == null)
                return false;

            if (key == Key.None)
                return false;

            // Critical fix: validate enum value safely
            if (!Enum.IsDefined(typeof(Key), key))
                return false;

            return Keyboard.current[key].isPressed;
        }
#endif
        void Update()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Safety: keyboard may not exist (mobile, headless, cloud)
            if (Keyboard.current == null)
                return;

            float dt = Time.deltaTime * speed;
            Vector3 v = new Vector3(dt, dt, dt);

            // Decrement
            if (IsKeyHeld(decrementKey))
            {
                Vector3 r = Vector3.Scale(-axis, v);
                transform.Rotate(r);
            }

            // Increment
            if (IsKeyHeld(incrementKey))
            {
                Vector3 r = Vector3.Scale(axis, v);
                transform.Rotate(r);
            }
#else
			float dt = Time.deltaTime * speed;

			Vector3 v = new Vector3(dt,dt,dt);

			if (Input.GetKey(decrementKey))
            {
				v.x *= -axis.x;
				v.y *= -axis.y;
				v.z *= -axis.z;

				transform.Rotate(v);
            }

			if (Input.GetKey(incrementKey))
            {
				v.x *= axis.x;
				v.y *= axis.y;
				v.z *= axis.z;
				
				transform.Rotate(v);
            }
#endif
        }
	}
}