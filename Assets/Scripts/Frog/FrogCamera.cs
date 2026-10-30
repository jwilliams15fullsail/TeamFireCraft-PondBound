using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Connects the local frog to a Cinemachine camera: a Spyro / Ratchet & Clank style orbit
// that trails the frog and swings in behind it while it hops (Orbital Follow recentering).
// Lives on the Main Camera, next to the Cinemachine Brain. Pure presentation: it tells the
// host which way "forward" is (Yaw) and where the crosshair points (AimRay). It reads
// mouse / stick look itself, so the Settings sensitivity and the pause menu still apply.
// Runs before other scripts, so its LateUpdate comes before the Cinemachine Brain's.
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
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

    [Header("Aiming a jump")]
    [Tooltip("Extra camera distance (m) while charging a jump, so you can see round the frog and where it will land. Never past Max Radius, and walls still pull the camera in.")]
    [SerializeField] float aimZoomOut = 1f;
    [Tooltip("Where the camera looks while you place the landing marker: 0 = at the frog, 1 = right at the marker.")]
    [SerializeField, Range(0f, 1f)] float aimLookAtMarker = 1f;
    [Tooltip("Seconds to ease the camera out (and onto the marker) when you start charging, and back after the jump.")]
    [SerializeField] float aimBlendTime = 0.35f;
    [Tooltip("Seconds the camera takes to glide after the landing marker as it moves (and hops up onto or down off platforms).")]
    [SerializeField] float aimFocusSmoothTime = 0.15f;
    [Tooltip("While you place the landing marker, the camera turns toward it only as far as keeps the frog on screen: 1 = the frog may reach the edge, 0.5 = it stays in the middle half.")]
    [SerializeField, Range(0.1f, 1f)] float keepFrogOnScreen = 0.7f;

    public float Yaw => transform.eulerAngles.y;

    // Which way the camera sits behind the frog (degrees, like a compass). It holds still
    // while you aim, so the landing marker moves left / right / forward / back as seen from it.
    public float AimYaw => orbit != null ? orbit.HorizontalAxis.Value : Yaw;

    // Set by our frog while it charges a jump: the camera pulls back so more of the level is
    // on screen.
    public bool AimingJump { get; set; }

    // Set by our frog while it places a landing marker: the camera holds its place and looks
    // at the marker, and mouse / stick look moves the marker (TakeJumpAimLook) instead.
    public bool AimingMarker
    {
        get => aimingMarker;
        set
        {
            if (!value)
                jumpAimLook = Vector2.zero;
            aimingMarker = value;
        }
    }

    // Look gathered while AimingMarker is on (degrees, Settings sensitivity included).
    // The frog takes it every frame.
    public Vector2 TakeJumpAimLook()
    {
        Vector2 look = jumpAimLook;
        jumpAimLook = Vector2.zero;
        return look;
    }

    // The crosshair ray. It starts level with the frog rather than at the lens, so
    // anything between the camera and the frog can't steal the tongue's (or a jump's) aim.
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
    CinemachineRotationComposer composer;
    Vector3 composerOffset;      // the composer's own Target Offset, restored after aiming
    Camera lens;
    Transform target;
    FrogNetworkDriver frogDriver;
    JumpPreview frogPreview;
    Renderer[] frogRenderers;
    ShadowCastingMode[] frogShadowModes;
    bool frogHidden;
    bool hadTarget;
    InputAction lookAction;
    float sensitivityScale = 1f; // from the Settings menu's Mouse Sensitivity slider (1 = 50%)
    float radius, radiusVelocity;
    bool aimingMarker;
    Vector2 jumpAimLook;
    float zoomBlend, markerBlend; // 0 = normal, 1 = fully into aiming
    Vector3 markerFocus, markerFocusVelocity; // where the camera looks while (and just after) aiming
    Vector3 lastTargetPosition;
    float smoothSpeed;

    void Start()
    {
        lens = GetComponent<Camera>();
        lookAction = InputSystem.actions.FindAction("Player/Look", true);
        FindFollowCamera();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hadTarget = true;
        frogDriver = target.GetComponent<FrogNetworkDriver>();
        frogPreview = target.GetComponent<JumpPreview>();
        frogRenderers = target.GetComponentsInChildren<Renderer>(true);
        frogShadowModes = new ShadowCastingMode[frogRenderers.Length];
        for (int i = 0; i < frogRenderers.Length; i++)
            frogShadowModes[i] = frogRenderers[i].shadowCastingMode;
        frogHidden = false;
        lastTargetPosition = target.position;
        zoomBlend = markerBlend = 0f;

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
        if (followCamera == null)
            return;
        if (orbit == null)
        {
            orbit = followCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbit != null)
                radius = orbit.Radius;
        }
        if (composer == null)
        {
            composer = followCamera.GetComponent<CinemachineRotationComposer>();
            if (composer != null)
                composerOffset = composer.TargetOffset;
        }

        // Camera collision must keep the frog in sight, not the spot the camera looks at:
        // while aiming that's the landing marker, and the line to it grazes platform edges
        // and the floor, which would drag the camera down toward it.
        var deoccluder = followCamera.GetComponent<CinemachineDeoccluder>();
        if (deoccluder != null)
            deoccluder.AvoidObstacles.UseFollowTarget.Enabled = true;
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

            if (AimingMarker)
            {
                jumpAimLook += look; // the frog moves its landing marker with it; the camera holds its place
            }
            else
            {
                // Only touch the axes when there really is look input: Cinemachine treats any
                // change of value as input and holds off swinging behind the frog.
                if (look.x != 0f)
                    orbit.HorizontalAxis.Value = Mathf.Repeat(orbit.HorizontalAxis.Value + look.x + 180f, 360f) - 180f;
                if (look.y != 0f)
                {
                    Vector2 pitchRange = orbit.VerticalAxis.Range;
                    orbit.VerticalAxis.Value = Mathf.Clamp(orbit.VerticalAxis.Value + (invertY ? look.y : -look.y), pitchRange.x, pitchRange.y);
                }
            }

            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    radius = Mathf.Clamp(radius - Mathf.Sign(scroll) * zoomStep, minRadius, maxRadius);
            }
        }

        // Charging a jump: ease the camera back so the frog's surroundings are on screen, but
        // never further than the furthest scroll zoom. The Deoccluder on the Frog Follow Camera
        // then pulls it in front of any wall or object between it and the frog, and eases it
        // back out once there's room again.
        zoomBlend = Mathf.MoveTowards(zoomBlend, AimingJump ? 1f : 0f, dt / Mathf.Max(0.01f, aimBlendTime));
        float zoomOut = aimZoomOut * Mathf.SmoothStep(0f, 1f, zoomBlend);
        orbit.Radius = Mathf.SmoothDamp(orbit.Radius, Mathf.Min(radius + zoomOut, maxRadius), ref radiusVelocity, zoomSmoothTime);

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

    // After everything has moved this frame and before the Cinemachine Brain places the
    // camera, so the look below matches exactly where the frog is and which way it faces.
    void LateUpdate()
    {
        if (target != null && orbit != null)
            UpdateMarkerLook(Time.deltaTime);
    }

    // Placing a landing marker: the camera holds its place (no swinging round or levelling
    // out on its own) and turns toward the marker, as far as it can while keeping the frog on
    // screen. It eases back onto the frog after the jump (while it flies there).
    void UpdateMarkerLook(float dt)
    {
        markerBlend = Mathf.MoveTowards(markerBlend, AimingMarker ? 1f : 0f, dt / Mathf.Max(0.01f, aimBlendTime));
        if (AimingMarker && frogDriver != null)
        {
            // Glide after the marker, so it hopping up onto a platform or down off one
            // doesn't jerk the camera.
            Vector3 marker = frogDriver.HasMarker ? frogDriver.MarkerPoint
                : frogPreview != null && frogPreview.HasLanding ? frogPreview.LandingPoint
                : target.position + frogDriver.LocalJumpTarget;
            markerFocus = Vector3.SmoothDamp(markerFocus, marker, ref markerFocusVelocity, aimFocusSmoothTime);
            orbit.HorizontalAxis.CancelRecentering();
            orbit.VerticalAxis.CancelRecentering();
        }
        else if (markerBlend <= 0f)
        {
            markerFocus = target.position;
            markerFocusVelocity = Vector3.zero;
        }
        if (composer == null)
            return;

        float blend = Mathf.SmoothStep(0f, 1f, markerBlend) * aimLookAtMarker;
        Vector3 frogPoint = target.position + target.rotation * composerOffset;
        Vector3 look = frogPoint + (markerFocus - target.position) * blend;

        // Turn toward it only so far that the frog stays on screen.
        Vector3 toFrog = frogPoint - transform.position;
        Vector3 toLook = look - transform.position;
        float limit = lens.fieldOfView * 0.5f * keepFrogOnScreen;
        if (Vector3.Angle(toFrog, toLook) > limit)
            look = transform.position + Vector3.RotateTowards(toFrog, toLook, limit * Mathf.Deg2Rad, 0f).normalized * toLook.magnitude;

        composer.TargetOffset = composerOffset + Quaternion.Inverse(target.rotation) * (look - frogPoint);
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