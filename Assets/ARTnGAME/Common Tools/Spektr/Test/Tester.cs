using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Artngame.CommonTools.Spektr
{
    public class Tester : MonoBehaviour
    {
        void Update()
        {
            var lr = GetComponent<Spektr.LightningRenderer>();
            if (lr == null) return;

            lr.throttle = IsMouseHeld() ? 0.4f : 0.045f;
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

        void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 200, 200), "Press mouse button to burst.");
        }
    }
}