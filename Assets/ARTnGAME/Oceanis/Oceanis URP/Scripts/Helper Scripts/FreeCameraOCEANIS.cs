using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Artngame.Oceanis// UnityEngine.Rendering
{
    /// <summary>
    /// Utility Free Camera component.
    /// </summary>
    //[HelpURL(Documentation.baseURL + Documentation.releaseVersion + Documentation.subURL + "Free-Camera" + Documentation.endURL)]
    [ExecuteAlways]
    public class FreeCameraOCEANIS : MonoBehaviour
    {
        const float k_MouseSensitivityMultiplier = 0.01f;

        /// <summary>
        /// Rotation speed when using a controller.
        /// </summary>
        public float m_LookSpeedController = 120f;
        /// <summary>
        /// Rotation speed when using the mouse.
        /// </summary>
        public float m_LookSpeedMouse = 4.0f;
        /// <summary>
        /// Movement speed.
        /// </summary>
        public float m_MoveSpeed = 10.0f;
        /// <summary>
        /// Value added to the speed when incrementing.
        /// </summary>
        public float m_MoveSpeedIncrement = 2.5f;
        /// <summary>
        /// Scale factor of the turbo mode.
        /// </summary>
        public float m_Turbo = 10.0f;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER



#if !USE_INPUT_SYSTEM
        private static string kMouseX = "Mouse X";
        private static string kMouseY = "Mouse Y";
        private static string kRightStickX = "Controller Right Stick X";
        private static string kRightStickY = "Controller Right Stick Y";
        private static string kVertical = "Vertical";
        private static string kHorizontal = "Horizontal";

        private static string kYAxis = "YAxis";
        private static string kSpeedAxis = "Speed Axis";
#endif

#if USE_INPUT_SYSTEM
        InputAction lookAction;
        InputAction moveAction;
        InputAction speedAction;
        InputAction yMoveAction;
#endif

        void OnEnable()
        {
            RegisterInputs();
        }

        void RegisterInputs()
        {
#if USE_INPUT_SYSTEM
            var map = new InputActionMap("Free Camera");

            lookAction = map.AddAction("look", binding: "<Mouse>/delta");
            moveAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
            speedAction = map.AddAction("speed", binding: "<Gamepad>/dpad");
            yMoveAction = map.AddAction("yMove");

            lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=15, y=15)");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d")
                .With("Right", "<Keyboard>/rightArrow");
            speedAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/home")
                .With("Down", "<Keyboard>/end");
            yMoveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/pageUp")
                .With("Down", "<Keyboard>/pageDown")
                .With("Up", "<Keyboard>/e")
                .With("Down", "<Keyboard>/q")
                .With("Up", "<Gamepad>/rightshoulder")
                .With("Down", "<Gamepad>/leftshoulder");

            moveAction.Enable();
            lookAction.Enable();
            speedAction.Enable();
            yMoveAction.Enable();
#endif

            //#if UNITY_EDITOR && !USE_INPUT_SYSTEM
            //            List<InputManagerEntry> inputEntries = new List<InputManagerEntry>();

            //            // Add new bindings
            //            inputEntries.Add(new InputManagerEntry { name = kRightStickX, kind = InputManagerEntry.Kind.Axis, axis = InputManagerEntry.Axis.Fourth, sensitivity = 1.0f, gravity = 1.0f, deadZone = 0.2f });
            //            inputEntries.Add(new InputManagerEntry { name = kRightStickY, kind = InputManagerEntry.Kind.Axis, axis = InputManagerEntry.Axis.Fifth, sensitivity = 1.0f, gravity = 1.0f, deadZone = 0.2f, invert = true });

            //            inputEntries.Add(new InputManagerEntry { name = kYAxis, kind = InputManagerEntry.Kind.KeyOrButton, btnPositive = "page up", altBtnPositive = "joystick button 5", btnNegative = "page down", altBtnNegative = "joystick button 4", gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });
            //            inputEntries.Add(new InputManagerEntry { name = kYAxis, kind = InputManagerEntry.Kind.KeyOrButton, btnPositive = "q", btnNegative = "e", gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });

            //            inputEntries.Add(new InputManagerEntry { name = kSpeedAxis, kind = InputManagerEntry.Kind.KeyOrButton, btnPositive = "home", btnNegative = "end", gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });
            //            inputEntries.Add(new InputManagerEntry { name = kSpeedAxis, kind = InputManagerEntry.Kind.Axis, axis = InputManagerEntry.Axis.Seventh, gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });

            //            InputRegistering.RegisterInputs(inputEntries);
            //#endif
        }

        float inputRotateAxisX, inputRotateAxisY;
        float inputChangeSpeed;
        float inputVertical, inputHorizontal, inputYAxis;
        bool leftShiftBoost, leftShift, fire1;

        void UpdateInputs()
        {
            inputRotateAxisX = 0.0f;
            inputRotateAxisY = 0.0f;
            leftShiftBoost = false;
            fire1 = false;

#if USE_INPUT_SYSTEM
            var lookDelta = lookAction.ReadValue<Vector2>();
            inputRotateAxisX = lookDelta.x * m_LookSpeedMouse * k_MouseSensitivityMultiplier;
            inputRotateAxisY = lookDelta.y * m_LookSpeedMouse * k_MouseSensitivityMultiplier;

            leftShift = Keyboard.current?.leftShiftKey?.isPressed ?? false;
            fire1 = Mouse.current?.leftButton?.isPressed == true || Gamepad.current?.xButton?.isPressed == true;

            inputChangeSpeed = speedAction.ReadValue<Vector2>().y;

            var moveDelta = moveAction.ReadValue<Vector2>();
            inputVertical = moveDelta.y;
            inputHorizontal = moveDelta.x;
            inputYAxis = yMoveAction.ReadValue<Vector2>().y;
#else

            // --- Mouse look when RMB held ---
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                leftShiftBoost = true;

                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                inputRotateAxisX = mouseDelta.x * m_LookSpeedMouse * 0.1f;
                inputRotateAxisY = mouseDelta.y * m_LookSpeedMouse * 0.1f;
            }
            else
            {
                inputRotateAxisX = 0f;
                inputRotateAxisY = 0f;
            }

            // --- Gamepad right stick look (always added) ---
            if (Gamepad.current != null)
            {
                Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
                inputRotateAxisX += rightStick.x * m_LookSpeedController * k_MouseSensitivityMultiplier;
                inputRotateAxisY += rightStick.y * m_LookSpeedController * k_MouseSensitivityMultiplier;
            }

            // --- Keyboard ---
            leftShift = Keyboard.current != null &&
                        Keyboard.current.leftShiftKey.isPressed;

            // --- Fire1 (LMB / Gamepad Right Trigger) ---
            fire1 =
                (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
                (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.1f);

            // --- Speed axis (Shift / Gamepad shoulder example) ---
            inputChangeSpeed =
                (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed ? 1f : 0f) +
                (Gamepad.current != null && Gamepad.current.rightShoulder.isPressed ? 1f : 0f);

            // --- Movement axes (WASD + left stick) ---
            inputVertical =
                (Keyboard.current.wKey.isPressed ? 1f : 0f) -
                (Keyboard.current.sKey.isPressed ? 1f : 0f);

            inputHorizontal =
                (Keyboard.current.dKey.isPressed ? 1f : 0f) -
                (Keyboard.current.aKey.isPressed ? 1f : 0f);

            if (Gamepad.current != null)
            {
                Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
                inputHorizontal += leftStick.x;
                inputVertical += leftStick.y;
            }

            // --- Y axis (example: Q / E or triggers) ---
            inputYAxis =
                (Keyboard.current.eKey.isPressed ? 1f : 0f) -
                (Keyboard.current.qKey.isPressed ? 1f : 0f);

            if (Gamepad.current != null)
            {
                inputYAxis +=
                    Gamepad.current.rightTrigger.ReadValue() -
                    Gamepad.current.leftTrigger.ReadValue();
            }

            //if (Input.GetMouseButton(1))
            //{
            //    leftShiftBoost = true;
            //    inputRotateAxisX = Input.GetAxis(kMouseX) * m_LookSpeedMouse;
            //    inputRotateAxisY = Input.GetAxis(kMouseY) * m_LookSpeedMouse;
            //}
            //inputRotateAxisX += (Input.GetAxis(kRightStickX) * m_LookSpeedController * k_MouseSensitivityMultiplier);
            //inputRotateAxisY += (Input.GetAxis(kRightStickY) * m_LookSpeedController * k_MouseSensitivityMultiplier);

            //leftShift = Input.GetKey(KeyCode.LeftShift);
            //fire1 = Input.GetAxis("Fire1") > 0.0f;

            //inputChangeSpeed = Input.GetAxis(kSpeedAxis);

            //inputVertical = Input.GetAxis(kVertical);
            //inputHorizontal = Input.GetAxis(kHorizontal);
            //inputYAxis = Input.GetAxis(kYAxis);
#endif
        }

        void Update()
        {
            // If the debug menu is running, we don't want to conflict with its inputs.
            //if (DebugManager.instance.displayRuntimeUI)
            //    return;

            UpdateInputs();

            if (inputChangeSpeed != 0.0f)
            {
                m_MoveSpeed += inputChangeSpeed * m_MoveSpeedIncrement;
                if (m_MoveSpeed < m_MoveSpeedIncrement) m_MoveSpeed = m_MoveSpeedIncrement;
            }

            bool moved = inputRotateAxisX != 0.0f || inputRotateAxisY != 0.0f || inputVertical != 0.0f || inputHorizontal != 0.0f || inputYAxis != 0.0f;
            if (moved)
            {
                float rotationX = transform.localEulerAngles.x;
                float newRotationY = transform.localEulerAngles.y + inputRotateAxisX;

                // Weird clamping code due to weird Euler angle mapping...
                float newRotationX = (rotationX - inputRotateAxisY);
                if (rotationX <= 90.0f && newRotationX >= 0.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
                if (rotationX >= 270.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

                transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

                float moveSpeed = Time.deltaTime * m_MoveSpeed;
                if (fire1 || leftShiftBoost && leftShift)
                    moveSpeed *= m_Turbo;
                transform.position += transform.forward * moveSpeed * inputVertical;
                transform.position += transform.right * moveSpeed * inputHorizontal;
                transform.position += Vector3.up * moveSpeed * inputYAxis;
            }
        }

#else
		public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
		public RotationAxes axes = RotationAxes.MouseXAndY;
		public float sensitivityX = 15F;
		public float sensitivityY = 15F;
		
		public float minimumX = -360F;
		public float maximumX = 360F;
		
		public float minimumY = -60F;
		public float maximumY = 60F;
		
		public float rotationY = 0F;

		bool m_takeMouseInput = true;

		void Start () 
		{

			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		
		}

		void OnGUI()
		{
			
			if(Event.current == null) return;
			
			if(Event.current.isMouse)
				m_takeMouseInput = true;
			else
				m_takeMouseInput = false;
			
		}
		
		void Update () 
		{
#if ENABLE_LEGACY_INPUT_MANAGER
            float speed = m_MoveSpeed;

			if(Input.GetKey(KeyCode.Space)) speed *= 10.0f;
			
			Vector3 move = new Vector3(0,0,0);
			
			//move left
			if(Input.GetKey(KeyCode.A))
				move = new Vector3(-1,0,0) * Time.deltaTime * speed;
			
			//move right
			if(Input.GetKey(KeyCode.D))
				move = new Vector3(1,0,0) * Time.deltaTime * speed;
			
			//move forward
			if(Input.GetKey(KeyCode.W))
				move = new Vector3(0,0,1) * Time.deltaTime * speed;
			
			//move back
			if(Input.GetKey(KeyCode.S))
				move = new Vector3(0,0,-1) * Time.deltaTime * speed;
			
			//move up
			if(Input.GetKey(KeyCode.E))
				move = new Vector3(0,-1,0) * Time.deltaTime * speed;
			
			//move down
			if(Input.GetKey(KeyCode.Q))
				move = new Vector3(0,1,0) * Time.deltaTime * speed;


			transform.Translate(move);

            if (m_takeMouseInput && Input.GetMouseButton(1)) // Input.GetMouseButtonDown(1))
			{

				if (axes == RotationAxes.MouseXAndY)
				{
					float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
					
					rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
					rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
					
					transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
				}
				else if (axes == RotationAxes.MouseX)
				{
					transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
				}
				else
				{
					rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
					rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
					
					transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
				}
			}

#endif
        }
#endif

        //        /// <summary>
        //        /// Rotation speed when using a controller.
        //        /// </summary>
        //        public float m_LookSpeedController = 120f;
        //        /// <summary>
        //        /// Rotation speed when using the mouse.
        //        /// </summary>
        //        public float m_LookSpeedMouse = 10.0f;
        //        /// <summary>
        //        /// Movement speed.
        //        /// </summary>
        //        public float m_MoveSpeed = 10.0f;
        //        /// <summary>
        //        /// Value added to the speed when incrementing.
        //        /// </summary>
        //        public float m_MoveSpeedIncrement = 2.5f;
        //        /// <summary>
        //        /// Scale factor of the turbo mode.
        //        /// </summary>
        //        public float m_Turbo = 10.0f;

        //        private static string kMouseX = "Mouse X";
        //        private static string kMouseY = "Mouse Y";
        //        private static string kRightStickX = "Controller Right Stick X";
        //        private static string kRightStickY = "Controller Right Stick Y";
        //        private static string kVertical = "Vertical";
        //        private static string kHorizontal = "Horizontal";

        //        private static string kYAxis = "YAxis";
        //        private static string kSpeedAxis = "Speed Axis";

        //        void OnEnable()
        //        {
        //            RegisterInputs();
        //        }

        //        void RegisterInputs()
        //        {
        //#if UNITY_EDITOR
        //            List<InputManagerEntry> inputEntries = new List<InputManagerEntry>();

        //            // Add new bindings
        //            inputEntries.Add(new InputManagerEntry { name = kRightStickX, kind = InputManagerEntry.Kind.Axis, axis = InputManagerEntry.Axis.Fourth, sensitivity = 1.0f, gravity = 1.0f, deadZone = 0.2f });
        //            inputEntries.Add(new InputManagerEntry { name = kRightStickY, kind = InputManagerEntry.Kind.Axis, axis = InputManagerEntry.Axis.Fifth, sensitivity = 1.0f, gravity = 1.0f, deadZone = 0.2f, invert = true });

        //            inputEntries.Add(new InputManagerEntry { name = kYAxis, kind = InputManagerEntry.Kind.KeyOrButton, btnPositive = "page up", altBtnPositive = "joystick button 5", btnNegative = "page down", altBtnNegative = "joystick button 4", gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });

        //            inputEntries.Add(new InputManagerEntry { name = kSpeedAxis, kind = InputManagerEntry.Kind.KeyOrButton, btnPositive = "home", btnNegative = "end", gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });
        //            inputEntries.Add(new InputManagerEntry { name = kSpeedAxis, kind = InputManagerEntry.Kind.Axis, axis = InputManagerEntry.Axis.Seventh, gravity = 1000.0f, deadZone = 0.001f, sensitivity = 1000.0f });

        //            InputRegistering.RegisterInputs(inputEntries);
        //#endif
        //        }

        //        void Update()
        //        {
        //            // If the debug menu is running, we don't want to conflict with its inputs.
        //            //if (DebugManager.instance.displayRuntimeUI)
        //            //    return;

        //            float inputRotateAxisX = 0.0f;
        //            float inputRotateAxisY = 0.0f;
        //            if (Input.GetMouseButton(1))
        //            {
        //                inputRotateAxisX = Input.GetAxis(kMouseX) * m_LookSpeedMouse;
        //                inputRotateAxisY = Input.GetAxis(kMouseY) * m_LookSpeedMouse;
        //            }
        //            inputRotateAxisX += (Input.GetAxis(kRightStickX) * m_LookSpeedController * Time.deltaTime);
        //            inputRotateAxisY += (Input.GetAxis(kRightStickY) * m_LookSpeedController * Time.deltaTime);

        //            float inputChangeSpeed = Input.GetAxis(kSpeedAxis);
        //            if (inputChangeSpeed != 0.0f)
        //            {
        //                m_MoveSpeed += inputChangeSpeed * m_MoveSpeedIncrement;
        //                if (m_MoveSpeed < m_MoveSpeedIncrement) m_MoveSpeed = m_MoveSpeedIncrement;
        //            }

        //            float inputVertical = Input.GetAxis(kVertical);
        //            float inputHorizontal = Input.GetAxis(kHorizontal);
        //            float inputYAxis = Input.GetAxis(kYAxis);

        //            bool moved = inputRotateAxisX != 0.0f || inputRotateAxisY != 0.0f || inputVertical != 0.0f || inputHorizontal != 0.0f || inputYAxis != 0.0f;
        //            if (moved)
        //            {
        //                float rotationX = transform.localEulerAngles.x;
        //                float newRotationY = transform.localEulerAngles.y + inputRotateAxisX;

        //                // Weird clamping code due to weird Euler angle mapping...
        //                float newRotationX = (rotationX - inputRotateAxisY);
        //                if (rotationX <= 90.0f && newRotationX >= 0.0f)
        //                    newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
        //                if (rotationX >= 270.0f)
        //                    newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

        //                transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

        //                float moveSpeed = Time.deltaTime * m_MoveSpeed;
        //                if (Input.GetMouseButton(1))
        //                    moveSpeed *= Input.GetKey(KeyCode.LeftShift) ? m_Turbo : 1.0f;
        //                else
        //                    moveSpeed *= Input.GetAxis("Fire1") > 0.0f ? m_Turbo : 1.0f;
        //                transform.position += transform.forward * moveSpeed * inputVertical;
        //                transform.position += transform.right * moveSpeed * inputHorizontal;
        //                transform.position += Vector3.up * moveSpeed * inputYAxis;
        //            }
        //        }
    }
}
