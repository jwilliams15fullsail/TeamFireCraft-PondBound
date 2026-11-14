using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
namespace Artngame.BrunetonsAtmosphere
{
    public class RotateLight : MonoBehaviour
    {

        public float speed = 5.0f;

        private Vector3 lastMousePos;

        private bool rotate;

        void Update()
        {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
           // if (rotate)
            {
                Vector3 DTT = (lastMousePos - (Vector3)Mouse.current.position.ReadValue())
              * speed * Time.deltaTime;

                if (Mouse.current.leftButton.isPressed &&
                    Keyboard.current.leftCtrlKey.isPressed)
                {
                    transform.Rotate(new Vector3(-DTT.y, -DTT.x, 0f));
                }
            }
            lastMousePos = Mouse.current.position.ReadValue();
#else
            //if (rotate)
            {
                Vector3 DTT = (lastMousePos - Input.mousePosition) * speed * Time.deltaTime ;

                if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
                {
                    transform.Rotate(new Vector3(-DTT.y, -DTT.x, 0));
                }
            }
            lastMousePos = Input.mousePosition;
#endif


            //if (Input.GetMouseButtonDown(0)) rotate = true;
            //if (Input.GetMouseButtonUp(0)) rotate = false;

            //Vector3 delta = lastMousePos - Input.mousePosition;

            //if (rotate)
            //{
            //    transform.Rotate(new Vector3(delta.y * Time.deltaTime * -speed, delta.x * Time.deltaTime * -speed, 0));
            //}

            //lastMousePos = Input.mousePosition;
        }
    }
}
