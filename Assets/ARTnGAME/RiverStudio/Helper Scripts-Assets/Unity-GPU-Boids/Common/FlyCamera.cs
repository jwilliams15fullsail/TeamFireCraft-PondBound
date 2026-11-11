using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
namespace Artngame.RiverStudio
{
    public class FlyCamera : MonoBehaviour
    {
        public float minSpeed = 0.5f;
        public float mainSpeed = 10f; // Regular speed.
        public float shiftMultiplier = 2f;  // Multiplied while shift is held.  Basically running.
        public float ctrlMultiplier = 0.5f;  // Multiplied while control is held.  Basically running.
        public float camSens = .35f;  // Camera sensitivity by mouse input.
        private Vector3 lastMouse = new Vector3(Screen.width / 2, Screen.height / 2, 0); // Kind of in the middle of the screen, rather than at the top (play).
        public bool clickToMove = true;

        void Update()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Mouse scroll wheel
            if (Mouse.current != null)
                mainSpeed += Mouse.current.scroll.ReadValue().y * mainSpeed;

            if (mainSpeed < minSpeed)
                mainSpeed = minSpeed;

            if (clickToMove)
            {
                if (Mouse.current == null ||
                    !Mouse.current.leftButton.isPressed)
                    return;

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    lastMouse = Mouse.current.position.ReadValue();
                    return;
                }
            }

            if (Mouse.current == null)
                return;

            // Mouse input (rotation)
            Vector3 currentMouse = Mouse.current.position.ReadValue();
            lastMouse = currentMouse - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(
                transform.eulerAngles.x + lastMouse.x,
                transform.eulerAngles.y + lastMouse.y,
                0);
            transform.eulerAngles = lastMouse;
            lastMouse = currentMouse;

            // Keyboard commands
            Vector3 p = getDirection();

            if (Keyboard.current != null &&
                Keyboard.current.leftShiftKey.isPressed)
                p *= mainSpeed * shiftMultiplier;
            else if (Keyboard.current != null &&
                     Keyboard.current.leftCtrlKey.isPressed)
                p *= mainSpeed * ctrlMultiplier;
            else
                p *= mainSpeed;

            p *= Time.unscaledDeltaTime;

            Vector3 newPosition = transform.position;

            if (Keyboard.current != null &&
                Keyboard.current.vKey.isPressed)
            {
                transform.Translate(p);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                transform.position = newPosition;
            }
            else
            {
                transform.Translate(p);
            }
#else
            mainSpeed += Input.GetAxis("Mouse ScrollWheel") * mainSpeed;
            if (mainSpeed < minSpeed)
                mainSpeed = minSpeed;

            if (clickToMove)
            {
                if (!Input.GetMouseButton(0))
                {
                    return;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    lastMouse = Input.mousePosition;
                    return;
                }
            }

            // Mouse input.
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;


            // Keyboard commands.
            Vector3 p = getDirection();
            if (Input.GetKey(KeyCode.LeftShift))
                p = p * mainSpeed * shiftMultiplier;
            else if (Input.GetKey(KeyCode.LeftControl))
                p = p * mainSpeed * ctrlMultiplier;
            else
                p = p * mainSpeed;

            //p = p * Time.deltaTime;
            p = p * Time.unscaledDeltaTime;
            Vector3 newPosition = transform.position;
            if (Input.GetKey(KeyCode.V))
            { //If player wants to move on X and Z axis only
                transform.Translate(p);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                transform.position = newPosition;
            }
            else
            {
                transform.Translate(p);
            }
#endif
        }

        private Vector3 getDirection()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Vector3 p_Velocity = Vector3.zero;

            if (Keyboard.current != null && Keyboard.current.wKey.isPressed)
                p_Velocity += new Vector3(0, 0, 1);

            if (Keyboard.current != null && Keyboard.current.sKey.isPressed)
                p_Velocity += new Vector3(0, 0, -1);

            if (Keyboard.current != null && Keyboard.current.aKey.isPressed)
                p_Velocity += new Vector3(-1, 0, 0);

            if (Keyboard.current != null && Keyboard.current.dKey.isPressed)
                p_Velocity += new Vector3(1, 0, 0);

            if (Keyboard.current != null && Keyboard.current.rKey.isPressed)
                p_Velocity += new Vector3(0, 1, 0);

            if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
                p_Velocity += new Vector3(0, -1, 0);

            return p_Velocity;
#else
            Vector3 p_Velocity = new Vector3();
            if (Input.GetKey(KeyCode.W))
            {
                p_Velocity += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(KeyCode.S))
            {
                p_Velocity += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(KeyCode.A))
            {
                p_Velocity += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                p_Velocity += new Vector3(1, 0, 0);
            }
            if (Input.GetKey(KeyCode.R))
            {
                p_Velocity += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(KeyCode.F))
            {
                p_Velocity += new Vector3(0, -1, 0);
            }
            return p_Velocity;
#endif
        }

        public void resetRotation(Vector3 lookAt)
        {
            transform.LookAt(lookAt);
        }
    }
}