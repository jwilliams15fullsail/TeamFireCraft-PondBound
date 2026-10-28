using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Connects the local frog to a Cinemachine camera: a Spyro / Ratchet & Clank style orbit
// that trails the frog and swings in behind it while it hops (Orbital Follow recentering).
// Lives on the Main Camera, next to the Cinemachine Brain. Pure presentation: it tells the
// host which way "forward" is (Yaw) and where the crosshair points (AimRay). It reads
// mouse / stick look itself, so the Settings sensitivity and the pause menu still apply.
[RequireComponent(typeof(Camera))]
public class FrogCamera : MonoBehaviour
{
    [Tooltip("The Cinemachine Camera that follows the frog (it needs Orbital Follow). Found automatically if empty.")]
    [SerializeField] CinemachineCamera followCamera;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.12f; // degrees per pixel
    [SerializeField] float stickSensitivity = 160f;  // degrees per second at full tilt
    [SerializeField] bool invertY;

    [Header("Zoom (scroll wheel)")]
    [SerializeField] float minRadius = 2.5f;
    [SerializeField] float maxRadius = 8f;
    [SerializeField] float zoomStep = 0.5f;          // metres per scroll notch
    [SerializeField] float zoomSmoothTime = 0.15f;

    [Header("Swing in behind the frog")]
    [Tooltip("Only swing behind the frog while it moves (Spyro style). Standing still, the camera stays where you put it.")]
    [SerializeField] bool recenterOnlyWhileMoving = true;
    [SerializeField] float movingSpeed = 0.4f;       // m/s that counts as moving

    [Header("Own frog")]
    [Tooltip("Hide our own frog (its shadow stays) when the camera is pushed this close.")]
    [SerializeField] float hideFrogWithin = 0.6f;
    [Tooltip("The frog moved this far in one frame (a respawn): cut to it instead of flying across the level.")]
    [SerializeField] float teleportDistance = 5f;

    public float Yaw => transform.eulerAngles.y;

    // The crosshair ray. It starts level with the frog rather than at the lens, so
    // anything between the camera and the frog can't steal the tongue's aim.
    public Ray AimRay
    {
        get
        {
            Vector3 forward = transform.forward;
            float skip = target != null ? Mathf.Max(0f, Vector3.Dot(target.position - transform.position, forward)) : 0f;
            return new Ray(transform.position + forward * skip, forward);
        }
    }

    CinemachineOrbitalFollow orbit;
    Transform target;
    Renderer[] frogRenderers;
    ShadowCastingMode[] frogShadowModes;
    bool frogHidden;
    bool hadTarget;
    InputAction lookAction;
    float sensitivityScale = 1f; // from the Settings menu's Mouse Sensitivity slider
    float radius, radiusVelocity;
    Vector3 lastTargetPosition;
    float smoothSpeed;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Player/Look", true);
        FindFollowCamera();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hadTarget = true;
        frogRenderers = target.GetComponentsInChildren<Renderer>(true);
        frogShadowModes = new ShadowCastingMode[frogRenderers.Length];
        for (int i = 0; i < frogRenderers.Length; i++)
            frogShadowModes[i] = frogRenderers[i].shadowCastingMode;
        frogHidden = false;
        lastTargetPosition = target.position;

        FindFollowCamera();
        if (followCamera != null)
        {
            followCamera.Follow = target;
            followCamera.LookAt = target;
            if (orbit != null)
                orbit.HorizontalAxis.Value = Mathf.DeltaAngle(0f, target.eulerAngles.y); // start right behind it
            followCamera.PreviousStateIsValid = false; // cut there, no fly-in
        }
        ReadSettings();
        LockCursor(true);
    }

    void FindFollowCamera()
    {
        if (followCamera == null)
            followCamera = FindAnyObjectByType<CinemachineCamera>();
        if (followCamera != null && orbit == null)
        {
            orbit = followCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbit != null)
                radius = orbit.Radius;
        }
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

        // Tab frees the mouse, Tab again captures it. Not while paused: the pause menu owns it then.
        if (!PauseMenu.IsOpen && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            LockCursor(Cursor.lockState != CursorLockMode.Locked);

        // While the mouse is free a menu may be changing the settings: pick them up.
        if (Cursor.lockState != CursorLockMode.Locked)
            ReadSettings();

        if (orbit == null)
            return;
        float dt = Time.deltaTime;

        if (Cursor.lockState == CursorLockMode.Locked && Application.isFocused)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            bool fromStick = lookAction.activeControl != null && lookAction.activeControl.device is Gamepad;
            look *= (fromStick ? stickSensitivity * dt : mouseSensitivity) * sensitivityScale;

            // Only touch the axes when there really is look input: Cinemachine treats any
            // change of value as input and holds off swinging behind the frog.
            if (look.x != 0f)
                orbit.HorizontalAxis.Value = Mathf.Repeat(orbit.HorizontalAxis.Value + look.x + 180f, 360f) - 180f;
            if (look.y != 0f)
            {
                Vector2 pitchRange = orbit.VerticalAxis.Range;
                orbit.VerticalAxis.Value = Mathf.Clamp(orbit.VerticalAxis.Value + (invertY ? look.y : -look.y), pitchRange.x, pitchRange.y);
            }

            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    radius = Mathf.Clamp(radius - Mathf.Sign(scroll) * zoomStep, minRadius, maxRadius);
            }
        }
        orbit.Radius = Mathf.SmoothDamp(orbit.Radius, radius, ref radiusVelocity, zoomSmoothTime);

        // Respawn teleport: cut to the frog. Otherwise note how fast it's moving.
        Vector3 moved = target.position - lastTargetPosition;
        lastTargetPosition = target.position;
        if (moved.magnitude > teleportDistance)
            followCamera.PreviousStateIsValid = false;
        moved.y = 0f;
        float speed = dt > 0f ? moved.magnitude / dt : 0f;
        smoothSpeed = Mathf.Lerp(smoothSpeed, speed, 1f - Mathf.Exp(-6f * dt));

        // Spyro style: swing in behind the frog only while it's going somewhere.
        if (recenterOnlyWhileMoving)
            orbit.HorizontalAxis.Recentering.Enabled = smoothSpeed > movingSpeed;

        // Pushed right up against the frog (by a wall): hide it so it doesn't fill the screen.
        float distance = Vector3.Distance(transform.position, target.position);
        if (!frogHidden && distance < hideFrogWithin)
            SetFrogHidden(true);
        else if (frogHidden && distance > hideFrogWithin + 0.1f)
            SetFrogHidden(false);
    }

    // The Settings menu saves Mouse Sensitivity as a percentage (1 to 100, default 50).
    // 50% is the normal speed, 100% twice as fast, 25% half as fast.
    void ReadSettings()
    {
        sensitivityScale = settingsController.GetMouseSensitivity() / settingsController.DefaultMouseSensitivity;
    }

    void SetFrogHidden(bool hidden)
    {
        frogHidden = hidden;
        for (int i = 0; i < frogRenderers.Length; i++)
            if (frogRenderers[i] != null)
                frogRenderers[i].shadowCastingMode = hidden ? ShadowCastingMode.ShadowsOnly : frogShadowModes[i];
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