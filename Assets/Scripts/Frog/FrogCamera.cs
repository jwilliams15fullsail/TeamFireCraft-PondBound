using UnityEngine;
using UnityEngine.InputSystem;

// Local third-person orbit camera. Pure presentation: it only tells the host which
// way "forward" is (Yaw) and where the crosshair points (AimRay).
public class FrogCamera : MonoBehaviour
{
    [SerializeField] float distance = 3.5f;
    [SerializeField] float minDistance = 0.6f;
    [SerializeField] float pivotHeight = 0.4f;
    [SerializeField] float mouseSensitivity = 0.12f; // degrees per pixel
    [SerializeField] float stickSensitivity = 160f;  // degrees per second at full tilt
    [SerializeField] float minPitch = -35f;
    [SerializeField] float maxPitch = 70f;
    [SerializeField] float followSmoothTime = 0.06f;
    [SerializeField] float collisionRadius = 0.2f;
    [Tooltip("Untick the Frog layer so teammates don't push the camera around.")]
    [SerializeField] LayerMask collisionMask = ~0;

    public float Yaw => yaw;
    public Ray AimRay => new Ray(transform.position, transform.forward);

    Transform target;
    bool hadTarget;
    InputAction lookAction;
    float yaw;
    float pitch = 15f;
    float currentDistance;
    Vector3 pivot, pivotVelocity;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Player/Look", true);
        currentDistance = distance;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hadTarget = true;
        pivot = target.position + Vector3.up * pivotHeight;
        yaw = target.eulerAngles.y;
        LockCursor(true);
    }

    void Update()
    {
        if (target == null)
        {
            // Our frog is gone (Leave / disconnect): give the mouse back for the menu.
            if (hadTarget)
            {
                hadTarget = false;
                LockCursor(false);
            }
            return;
        }

        // Tab frees the mouse (to click Leave), Tab again captures it.
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            LockCursor(Cursor.lockState != CursorLockMode.Locked);
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (Cursor.lockState == CursorLockMode.Locked && Application.isFocused)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            bool fromStick = lookAction.activeControl != null && lookAction.activeControl.device is Gamepad;
            look *= fromStick ? stickSensitivity * Time.deltaTime : mouseSensitivity;
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
        }

        pivot = Vector3.SmoothDamp(pivot, target.position + Vector3.up * pivotHeight, ref pivotVelocity, followSmoothTime);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f); // no roll: stable horizon while climbing
        Vector3 back = rotation * Vector3.back;

        // Pull in instantly when a wall is in the way, ease back out afterwards.
        float wanted = distance;
        if (Physics.SphereCast(pivot, collisionRadius, back, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
            wanted = Mathf.Max(minDistance, hit.distance);
        currentDistance = wanted < currentDistance ? wanted : Mathf.MoveTowards(currentDistance, wanted, 4f * Time.deltaTime);

        transform.SetPositionAndRotation(pivot + back * currentDistance, rotation);
    }

    void OnGUI()
    {
        if (target != null && Cursor.lockState == CursorLockMode.Locked)
            GUI.Label(new Rect(Screen.width * 0.5f - 4f, Screen.height * 0.5f - 10f, 20f, 20f), "+");
    }

    static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
