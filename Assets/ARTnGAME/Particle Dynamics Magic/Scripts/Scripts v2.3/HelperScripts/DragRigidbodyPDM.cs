using System;
using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Artngame.PDM
{
	public class DragRigidbodyPDM : MonoBehaviour
    {
        const float k_Spring = 50.0f;
        const float k_Damper = 5.0f;
        const float k_Drag = 10.0f;
        const float k_AngularDrag = 5.0f;
        const float k_Distance = 0.2f;
        const bool k_AttachToCenterOfMass = false;

        private SpringJoint m_SpringJoint;


        private void Update()
        {
            //Debug.Log("Update running");

            bool mouseButtonDown = false;
            Vector2 mousePos = Vector2.zero;

            // Prefer OLD input system if available
#if ENABLE_LEGACY_INPUT_MANAGER

mouseButtonDown = Input.GetMouseButtonDown(0);
mousePos = Input.mousePosition;

#elif ENABLE_INPUT_SYSTEM

            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                mouseButtonDown =
                    UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;

                mousePos =
                    UnityEngine.InputSystem.Mouse.current.position.ReadValue();

                //Debug.Log("Update running 1");
            }

#endif

            // Make sure the user pressed the mouse down
            if (!mouseButtonDown)
            {
                return;
            }

            var mainCamera = FindCamera();

            // We need to actually hit an object
            RaycastHit hit;

            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            if (!Physics.Raycast(
                    ray.origin,
                    ray.direction,
                    out hit,
                    100,
                    Physics.DefaultRaycastLayers))
            {
                return;
            }
            /*
            // Make sure the user pressed the mouse down
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            var mainCamera = FindCamera();

            // We need to actually hit an object
            RaycastHit hit = new RaycastHit();
            if (
                !Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition).origin,
                                 mainCamera.ScreenPointToRay(Input.mousePosition).direction, out hit, 100,
                                 Physics.DefaultRaycastLayers))
            {
                return;
            }
            */

            // We need to hit a rigidbody that is not kinematic
            if (!hit.rigidbody || hit.rigidbody.isKinematic)
            {
                return;
            }

            if (!m_SpringJoint)
            {
                var go = new GameObject("Rigidbody dragger");
                Rigidbody body = go.AddComponent<Rigidbody>();
                m_SpringJoint = go.AddComponent<SpringJoint>();
                body.isKinematic = true;
            }

            m_SpringJoint.transform.position = hit.point;
            m_SpringJoint.anchor = Vector3.zero;

            m_SpringJoint.spring = k_Spring;
            m_SpringJoint.damper = k_Damper;
            m_SpringJoint.maxDistance = k_Distance;
            m_SpringJoint.connectedBody = hit.rigidbody;

            //StartCoroutine("DragObject", hit.distance);
            StartCoroutine(DragObject(hit.distance));
        }


        //private IEnumerator DragObject(float distance)
        //{
        //    var oldDrag = m_SpringJoint.connectedBody.linearDamping;
        //    var oldAngularDrag = m_SpringJoint.connectedBody.angularDamping;
        //    m_SpringJoint.connectedBody.linearDamping = k_Drag;
        //    m_SpringJoint.connectedBody.angularDamping = k_AngularDrag;
        //    var mainCamera = FindCamera();
        //    while (Input.GetMouseButton(0))
        //    {
        //        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //        m_SpringJoint.transform.position = ray.GetPoint(distance);
        //        yield return null;
        //    }
        //    if (m_SpringJoint.connectedBody)
        //    {
        //        m_SpringJoint.connectedBody.linearDamping = oldDrag;
        //        m_SpringJoint.connectedBody.angularDamping = oldAngularDrag;
        //        m_SpringJoint.connectedBody = null;
        //    }
        //}
        private IEnumerator DragObject(float distance)
        {
            var oldDrag = m_SpringJoint.connectedBody.linearDamping;
            var oldAngularDrag = m_SpringJoint.connectedBody.angularDamping;

            m_SpringJoint.connectedBody.linearDamping = k_Drag;
            m_SpringJoint.connectedBody.angularDamping = k_AngularDrag;

            var mainCamera = FindCamera();

            while (true)
            {
                bool mouseHeld = false;
                Vector3 mousePos = Vector3.zero;

                // Prefer OLD input system if available
#if ENABLE_LEGACY_INPUT_MANAGER

        mouseHeld = Input.GetMouseButton(0);
        mousePos = Input.mousePosition;

#elif ENABLE_INPUT_SYSTEM

                if (UnityEngine.InputSystem.Mouse.current != null)
                {
                    mouseHeld =
                        UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;

                    Vector2 pos =
                        UnityEngine.InputSystem.Mouse.current.position.ReadValue();

                    mousePos = new Vector3(pos.x, pos.y, 0f);
                }

#endif

                if (!mouseHeld)
                {
                    break;
                }

                Ray ray = mainCamera.ScreenPointToRay(mousePos);

                // m_SpringJoint.transform.position = ray.GetPoint(distance);
                m_SpringJoint.connectedBody.position = ray.GetPoint(distance);

                yield return null;
            }

            if (m_SpringJoint.connectedBody)
            {
                m_SpringJoint.connectedBody.linearDamping = oldDrag;
                m_SpringJoint.connectedBody.angularDamping = oldAngularDrag;
                m_SpringJoint.connectedBody = null;
            }
        }


        private Camera FindCamera()
        {
            if (GetComponent<Camera>())
            {
                return GetComponent<Camera>();
            }

            return Camera.main;
        }
    }
}
