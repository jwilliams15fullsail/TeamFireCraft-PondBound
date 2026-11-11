using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Artngame.PDM
{
    public class DragTransformPDM : MonoBehaviour
    {
        public Color mouseOverColor = Color.blue;
        private Color originalColor;

        public bool Colorize = true;

        private Renderer cachedRenderer;
        private Camera cam;

        private bool isDragging;
        private float zDepth;

        void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            cam = Camera.main;
        }

        void Start()
        {
            if (Colorize && cachedRenderer != null &&
                cachedRenderer.sharedMaterial.HasProperty("_Color"))
            {
                originalColor = cachedRenderer.sharedMaterial.color;
            }
        }

        void Update()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector2 mousePos = GetMousePosition();
            Ray ray = cam.ScreenPointToRay(mousePos);

            // -----------------------
            // HOVER (replacement for OnMouseEnter/Exit)
            // -----------------------
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.transform == transform)
                {
                    if (Colorize)
                        SetColor(mouseOverColor);

                    // -----------------------
                    // START DRAG
                    // -----------------------
                    if (!isDragging && IsMouseDown())
                    {
                        isDragging = true;
                        zDepth = cam.WorldToScreenPoint(transform.position).z;
                    }
                }
                else
                {
                    if (!isDragging && Colorize)
                        SetColor(originalColor);
                }
            }
            else
            {
                if (!isDragging && Colorize)
                    SetColor(originalColor);
            }

            // -----------------------
            // DRAG (replacement for OnMouseDown coroutine)
            // -----------------------
            if (isDragging)
            {
                if (IsMouseHeld())
                {
                    Vector2 pos = GetMousePosition();
                    Vector3 screenPoint = new Vector3(pos.x, pos.y, zDepth);

                    Vector3 worldPos = cam.ScreenToWorldPoint(screenPoint);
                    transform.position = worldPos;
                }
                else
                {
                    isDragging = false;

                    if (Colorize)
                        SetColor(originalColor);
                }
            }
        }

        void SetColor(Color c)
        {
            if (cachedRenderer != null &&
                cachedRenderer.material.HasProperty("_Color"))
            {
                cachedRenderer.material.color = c;
            }
        }

        bool IsMouseDown()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);

#elif ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   Mouse.current.leftButton.wasPressedThisFrame;
#else
            return false;
#endif
        }

        bool IsMouseHeld()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0);

#elif ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   Mouse.current.leftButton.isPressed;
#else
            return false;
#endif
        }

        Vector2 GetMousePosition()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;

#elif ENABLE_INPUT_SYSTEM
            return Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : Vector2.zero;
#else
            return Vector2.zero;
#endif
        }
    }
}

/*
using System.Collections;
using System;
using UnityEngine;
using Artngame.PDM;

namespace Artngame.PDM {

public class DragTransformPDM: MonoBehaviour {


	public Color mouseOverColor = Color.blue;
	private Color originalColor ;

		//v.2.0
		public bool Colorize = true;

	void Start() {
			if(Colorize){
                if (GetComponent<Renderer>().sharedMaterial.HasProperty("_Color"))//v2.5
                {
                    originalColor = GetComponent<Renderer>().sharedMaterial.color;
                }
			}
	}
	void OnMouseEnter() {
			if(Colorize){
                if (GetComponent<Renderer>().material.HasProperty("_Color"))//v2.5
                {
                    GetComponent<Renderer>().material.color = mouseOverColor;
                }
			}
	}

	void OnMouseExit() {
			if(Colorize){
                if (GetComponent<Renderer>().material.HasProperty("_Color"))//v2.5
                {
                    GetComponent<Renderer>().material.color = originalColor;
                }
			}
	}

	IEnumerator  OnMouseDown() {
		Vector3 screenSpace = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
		while (Input.GetMouseButton(0))
		{
			Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
			Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
			transform.position = curPosition;

			yield return 1;
		}
	}

}

}
*/