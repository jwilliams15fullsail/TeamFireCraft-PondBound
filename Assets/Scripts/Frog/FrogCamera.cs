using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Local over-the-shoulder camera, like a third-person shooter. Pure presentation: it
// only tells the host which way "forward" is (Yaw) and where the crosshair points (AimRay).
// Follow: tight sideways, softer up and down (hops don't jolt the view), never more than
// maxLag behind. Look is raw (no smoothing) so aiming stays precise.
// Zoom: scroll wheel, plus an optional, subtle dynamic pull-back.
// Collision: neither the camera nor its near clip plane goes inside level geometry.
[RequireComponent(typeof(Camera))]
public class FrogCamera : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.12f; // degrees per pixel
    [SerializeField] float stickSensitivity = 160f;  // degrees per second at full tilt
    [SerializeField] float minPitch = -35f;
    [SerializeField] float maxPitch = 70f;

    [Header("Follow")]
    [SerializeField] float pivotHeight = 0.4f;
    [SerializeField] float horizontalFollowTime = 0.04f;
    [SerializeField] float verticalFollowTime = 0.12f;
    [Tooltip("The camera never trails the frog by more than this (metres), even during tongue yanks or falls.")]
    [SerializeField] float maxLag = 0.5f;

    [Header("Shoulder")]
    [SerializeField] float shoulderOffset = 0.55f;   // metres to the side; the frog sits left of the crosshair
    [SerializeField] bool startOnRightShoulder = true;
    [SerializeField] float shoulderSwapTime = 0.15f; // V or right stick click swaps sides

    [Header("Zoom (scroll wheel)")]
    [SerializeField] float startZoom = 3.5f;
    [SerializeField] float minZoom = 1.5f;
    [SerializeField] float maxZoom = 7f;
    [SerializeField] float zoomStep = 0.5f;          // metres per scroll notch
    [SerializeField] float stickZoomSpeed = 4f;      // metres per second, optional Player/Zoom action
    [SerializeField] float zoomSmoothTime = 0.12f;

    [Header("Dynamic zoom (added on top)")]
    [Tooltip("0 = fixed distance like most shooters, 1 = full pull-back values below.")]
    [SerializeField, Range(0f, 1f)] float dynamicZoomStrength = 0.35f;
    [SerializeField] float chargeExtra = 0.8f;       // at full jump charge
    [SerializeField] float airborneExtra = 0.6f;
    [SerializeField] float climbExtra = 0.7f;        // see more of the wall
    [SerializeField] float tongueExtraPerMetre = 0.15f;
    [SerializeField] float maxTongueExtra = 1.5f;
    [SerializeField] float speedExtra = 0.5f;        // at fastSpeed
    [SerializeField] float fastSpeed = 6f;
    [SerializeField] float dynamicSmoothTime = 0.4f;

    [Header("Collision")]
    [Tooltip("Tick level geometry (Default, Obstacle...). Untick Frog and Player so frogs don't push the camera around.")]
    [SerializeField] LayerMask collisionMask = 1 << 0;
    [SerializeField] float collisionRadius = 0.2f;   // grown automatically to cover the near clip plane
    [SerializeField] float minDistance = 0.35f;
    [SerializeField] float recoverDelay = 0.15f;     // wait before easing back out, stops flicker at edges
    [SerializeField] float recoverSpeed = 4f;        // metres per second when easing back out
    [Tooltip("Hide our own frog (shadow stays) when the camera is pushed this close.")]
    [SerializeField] float hideFrogWithin = 0.6f;

    public float Yaw => yaw;

    // The crosshair ray. It starts level with the frog rather than at the lens, so
    // anything between the camera and the frog can't steal the tongue's aim.
    public Ray AimRay
    {
        get
        {
            Vector3 forward = transform.forward;
            float skip = target != null ? Mathf.Max(0f, Vector3.Dot(pivot - transform.position, forward)) : 0f;
            return new Ray(transform.position + forward * skip, forward);
        }
    }

    Camera cam;
    Transform target;
    FrogMotor motor;
    FrogTongue tongue;
    Renderer[] frogRenderers;
    ShadowCastingMode[] frogShadowModes;
    bool frogHidden;
    bool hadTarget;
    InputAction lookAction, zoomAction;
    float yaw;
    float pitch = 15f;
    float sensitivityScale = 1f; // from the Settings menu's Mouse Sensitivity slider
    Vector3 pivot, horizontalVelocity;
    float verticalVelocity;
    bool rightShoulder;
    float shoulderSide, shoulderSideVelocity; // -1 left .. +1 right, eased
    float shoulderRoom;                       // how far sideways walls let us go
    float zoom, smoothZoom, zoomVelocity;
    float dynamicExtra, dynamicVelocity;
    float currentDistance, blockedUntil, shoulderBlockedUntil;
    Vector3 lastTargetPosition;
    float smoothSpeed;

    void Awake()
    {
        cam = GetComponent<Camera>();
        zoom = smoothZoom = currentDistance = startZoom;
        rightShoulder = startOnRightShoulder;
        shoulderSide = rightShoulder ? 1f : -1f;
        shoulderRoom = shoulderOffset;
    }

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Player/Look", true);
        zoomAction = InputSystem.actions.FindAction("Player/Zoom", false); // optional, for gamepads
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hadTarget = true;
        motor = target.GetComponent<FrogMotor>();
        tongue = target.GetComponent<FrogTongue>();
        frogRenderers = target.GetComponentsInChildren<Renderer>(true);
        frogShadowModes = new ShadowCastingMode[frogRenderers.Length];
        for (int i = 0; i < frogRenderers.Length; i++)
            frogShadowModes[i] = frogRenderers[i].shadowCastingMode;
        frogHidden = false;

        lastTargetPosition = target.position;
        pivot = SafePivot();
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        yaw = target.eulerAngles.y;
        ReadSettings();
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

        // Tab frees the mouse (to click Leave), Tab again captures it. Not while paused:
        // the pause menu owns the mouse then.
        if (!PauseMenu.IsOpen && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            LockCursor(Cursor.lockState != CursorLockMode.Locked);

        // While the mouse is free a menu may be changing the settings: pick them up.
        if (Cursor.lockState != CursorLockMode.Locked)
            ReadSettings();
    }

    // The Settings menu saves Mouse Sensitivity (0.1 to 10, default 2) in PlayerPrefs.
    void ReadSettings()
    {
        sensitivityScale = PlayerPrefs.GetFloat("Sensitivity", 2f) / 2f;
    }

    void LateUpdate()
    {
        if (target == null)
            return;
        float dt = Time.deltaTime;

        if (Cursor.lockState == CursorLockMode.Locked && Application.isFocused)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            bool fromStick = lookAction.activeControl != null && lookAction.activeControl.device is Gamepad;
            look *= (fromStick ? stickSensitivity * dt : mouseSensitivity) * sensitivityScale;
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
            ReadZoomInput(dt);
            ReadShoulderSwap();
        }

        // Zoom: the player's choice plus the dynamic extra, each eased on its own.
        smoothZoom = Mathf.SmoothDamp(smoothZoom, zoom, ref zoomVelocity, zoomSmoothTime);
        dynamicExtra = Mathf.SmoothDamp(dynamicExtra, DynamicExtra(dt) * dynamicZoomStrength, ref dynamicVelocity, dynamicSmoothTime);
        float desired = smoothZoom + dynamicExtra;

        FollowPivot();
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f); // no roll: stable horizon while climbing
        Vector3 back = rotation * Vector3.back;
        Vector3 right = rotation * Vector3.right;

        // Shoulder: slide the orbit point sideways, stopping short of walls.
        shoulderSide = Mathf.SmoothDamp(shoulderSide, rightShoulder ? 1f : -1f, ref shoulderSideVelocity, shoulderSwapTime);
        Vector3 side = right * (shoulderSide >= 0f ? 1f : -1f);
        shoulderRoom = PullInOrRecover(shoulderRoom, ClearDistance(pivot, side, shoulderOffset), ref shoulderBlockedUntil, dt);
        Vector3 shoulder = pivot + side * Mathf.Min(shoulderRoom, shoulderOffset * Mathf.Abs(shoulderSide));

        // Back: pull in instantly when something is in the way, ease out after a moment.
        float wanted = Mathf.Max(minDistance, ClearDistance(shoulder, back, desired));
        currentDistance = PullInOrRecover(currentDistance, wanted, ref blockedUntil, dt);

        transform.SetPositionAndRotation(shoulder + back * currentDistance, rotation);

        // Pushed right up against the frog: hide it so it doesn't fill the screen.
        if (!frogHidden && currentDistance < hideFrogWithin)
            SetFrogHidden(true);
        else if (frogHidden && currentDistance > hideFrogWithin + 0.1f)
            SetFrogHidden(false);
    }

    // Tight sideways, softer vertically, and never further behind than maxLag.
    void FollowPivot()
    {
        Vector3 goal = SafePivot();
        Vector3 flat = Vector3.SmoothDamp(new Vector3(pivot.x, 0f, pivot.z), new Vector3(goal.x, 0f, goal.z),
            ref horizontalVelocity, horizontalFollowTime);
        float y = Mathf.SmoothDamp(pivot.y, goal.y, ref verticalVelocity, verticalFollowTime);
        pivot = new Vector3(flat.x, y, flat.z);

        Vector3 lag = pivot - goal;
        if (lag.sqrMagnitude > maxLag * maxLag)
            pivot = goal + lag.normalized * maxLag;
    }

    float PullInOrRecover(float current, float wanted, ref float waitUntil, float dt)
    {
        if (wanted < current)
        {
            waitUntil = Time.time + recoverDelay;
            return wanted;
        }
        return Time.time >= waitUntil ? Mathf.MoveTowards(current, wanted, recoverSpeed * dt) : current;
    }

    void ReadZoomInput(float dt)
    {
        // Scroll wheel: one notch = one step, whatever the platform reports per notch.
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
                zoom -= Mathf.Sign(scroll) * zoomStep;
        }
        // Optional Player/Zoom action (e.g. D-pad up/down): held = continuous zoom.
        if (zoomAction != null)
            zoom -= zoomAction.ReadValue<float>() * stickZoomSpeed * dt;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    void ReadShoulderSwap()
    {
        bool swap = (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
                 || (Gamepad.current != null && Gamepad.current.rightStickButton.wasPressedThisFrame);
        if (swap)
            rightShoulder = !rightShoulder;
    }

    // How much further back to sit right now, from the frog's replicated state.
    float DynamicExtra(float dt)
    {
        Vector3 moved = target.position - lastTargetPosition;
        lastTargetPosition = target.position;
        moved.y = 0f;
        float speed = dt > 0f ? moved.magnitude / dt : 0f;
        smoothSpeed = Mathf.Lerp(smoothSpeed, speed, 1f - Mathf.Exp(-5f * dt)); // ignore single-frame spikes
        float extra = speedExtra * Mathf.Clamp01(smoothSpeed / fastSpeed);

        if (motor != null && motor.IsSpawned)
        {
            FrogMode mode = motor.Mode.Value;
            if (mode == FrogMode.Airborne)
                extra += airborneExtra;
            else if (mode == FrogMode.Climbing)
                extra += climbExtra;
            extra += chargeExtra * motor.ChargeFraction();
        }

        if (tongue != null && tongue.IsSpawned)
        {
            TongueSync sync = tongue.Sync.Value;
            if (sync.State == TongueState.Attached && sync.Target.TryGet(out NetworkObject held))
            {
                float length = Vector3.Distance(target.position, held.transform.TransformPoint(sync.LocalAnchor));
                extra += Mathf.Min(maxTongueExtra, length * tongueExtraPerMetre);
            }
        }
        return extra;
    }

    // The orbit point above the frog, lowered if a ceiling or ledge is in the way.
    Vector3 SafePivot()
    {
        Vector3 from = target.position;
        if (Physics.SphereCast(from, 0.1f, Vector3.up, out RaycastHit hit, pivotHeight, collisionMask, QueryTriggerInteraction.Ignore))
            return from + Vector3.up * Mathf.Max(0f, hit.distance - 0.05f);
        return from + Vector3.up * pivotHeight;
    }

    // How far the camera can move from origin along dir before it (or its near clip
    // plane) touches geometry.
    float ClearDistance(Vector3 origin, Vector3 dir, float desired)
    {
        if (desired <= 0f)
            return 0f;
        float radius = Mathf.Max(collisionRadius, NearPlaneExtent());
        // A cast that starts inside a collider can't see it: fall back to a thin probe.
        if (Physics.CheckSphere(origin, radius, collisionMask, QueryTriggerInteraction.Ignore))
            radius = 0.05f;
        if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, desired, collisionMask, QueryTriggerInteraction.Ignore))
            return hit.distance;
        return desired;
    }

    // Distance from the camera to a corner of its near clip plane.
    float NearPlaneExtent()
    {
        float near = cam.nearClipPlane;
        float halfHeight = near * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * cam.aspect;
        return Mathf.Sqrt(near * near + halfHeight * halfHeight + halfWidth * halfWidth);
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