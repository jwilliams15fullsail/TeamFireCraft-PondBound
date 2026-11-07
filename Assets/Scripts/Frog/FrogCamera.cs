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
    [Tooltip("The Cinemachine Camera that follows the frog (Orbital Follow, or Third Person Follow). Found automatically if empty.")]
    [SerializeField] CinemachineCamera followCamera;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.12f; // degrees per pixel
    [SerializeField] float stickSensitivity = 160f;  // degrees per second at full tilt
    [SerializeField] bool invertY;

    [Header("Follow point (keeps the camera steady through hops)")]
    [Tooltip("How far (m) the frog can wander sideways before the camera follows, so small wobbles never shake it.")]
    [SerializeField, Min(0f)] float deadZone = 0.15f;
    [Tooltip("Seconds the camera takes to catch up sideways, and with the frog's height in jumps and drops. Smaller = tighter, bigger = smoother.")]
    [SerializeField] float followSmoothTime = 0.1f;
    [Tooltip("Seconds the camera takes to settle at the frog's new height after it lands, or while it climbs.")]
    [SerializeField] float heightSmoothTime = 0.3f;
    [Tooltip("Hops lower than this (m) leave the camera's height alone. Higher hops, charged jumps and drops are followed.")]
    [SerializeField, Min(0f)] float hopHeadroom = 1.5f;

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
    [Tooltip("Extra camera distance (m) while charging a jump, so you can see round the frog and where it will land. Walls still pull the camera in.")]
    [SerializeField] float aimZoomOut = 1f;
    [Tooltip("Where the camera looks while you place the landing marker: 0 = at the frog, 1 = right at the marker.")]
    [SerializeField, Range(0f, 1f)] float aimLookAtMarker = 1f;
    [Tooltip("Seconds to ease the camera out (and onto the marker) when you start charging, and back after the jump.")]
    [SerializeField] float aimBlendTime = 0.35f;
    [Tooltip("Seconds the camera takes to glide after the landing marker as it moves (and hops up onto or down off platforms).")]
    [SerializeField] float aimFocusSmoothTime = 0.15f;
    [Tooltip("While you place the landing marker, the camera turns toward it only as far as keeps the frog on screen: 1 = the frog may reach the edge, 0.5 = it stays in the middle half.")]
    [SerializeField, Range(0.1f, 1f)] float keepFrogOnScreen = 0.7f;


    [Header("Third Person Follow (only if the Frog Follow Camera uses it)")]
    [Tooltip("How far the camera can tilt, in degrees: up (negative) and down (positive).")]
    [SerializeField] Vector2 pitchLimits = new Vector2(-30f, 70f);
    [Tooltip("How far the camera looks down when your frog spawns (degrees).")]
    [SerializeField] float startPitch = 15f;
    [Tooltip("While aiming a jump the camera tilts down to at least this (degrees), a view from above that follows the landing marker.")]
    [SerializeField, Range(0f, 89f)] float aimPitch = 45f;
    [Tooltip("While you place the landing marker the camera slides toward it (by Aim Look At Marker), but never more than this (m), so the frog stays on screen.")]
    [SerializeField] float aimMaxSlide = 1.5f;
   

    public float Yaw => transform.eulerAngles.y;

    // Which way the camera sits behind the frog (degrees, like a compass). The landing marker
    // moves left / right / forward / back as seen from it, and swings round with it when it orbits.
    public float AimYaw => orbit != null ? orbit.HorizontalAxis.Value : thirdPerson != null ? lookYaw : Yaw;

    // Set by our frog while it charges a jump: the camera pulls back so more of the level is
    // on screen.
    public bool AimingJump { get; set; }

    // Set by our frog while it aims a jump with the landing marker: the camera goes overhead
    // and follows the marker, and mouse / stick look steers the aim (JumpAimRay) instead of the
    // camera. The aim starts where the crosshair points.
    public bool AimingMarker
    {
        get => aimingMarker;
        set
        {
            if (value && !aimingMarker)
            {
                Vector3 forward = transform.forward;
                jumpAimYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                jumpAimPitch = -Mathf.Asin(Mathf.Clamp(forward.y, -1f, 1f)) * Mathf.Rad2Deg;
            }
            aimingMarker = value;
        }
    }

    // Aiming a jump: the ray the landing marker sits on. It starts at the frog's head (where the
    // crosshair line passes over the frog) and points where the mouse steered it, so the camera
    // can float overhead to follow the marker without moving the aim.
    public Ray JumpAimRay
    {
        get
        {
            float head = thirdPerson != null ? shoulderOffset.y : composerOffset.y;
            Vector3 from = target != null ? target.position + Vector3.up * head : transform.position;
            return new Ray(from, Quaternion.Euler(jumpAimPitch, jumpAimYaw, 0f) * Vector3.forward);
        }
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
    float radius;                // the Orbital Follow's Radius, as set in the Inspector
    FrogMotor frogMotor;
    Transform followPoint;       // what the Cinemachine Camera follows and looks at (see UpdateFollowPoint)
    Vector3 followVelocity;
    float heightVelocity;
    bool followingJump;          // following this jump up (a charged jump, or higher than Hop Headroom)
    LayerMask solidLayers = Physics.DefaultRaycastLayers;
    CinemachineThirdPersonFollow thirdPerson; // used instead of the orbit when the Frog Follow Camera has Third Person Follow
    float cameraDistance;        // Third Person Follow's Camera Distance, as set in the Inspector
    float lookYaw, lookPitch;    // where Third Person Follow points the camera (degrees, pitch + = looking down)
    Vector3 shoulderOffset;      // Third Person Follow's Shoulder Offset, as set in the Inspector
    float aimRoom = 1f, aimRoomVelocity; // how much room the camera has behind its pivot (1 = all of it), smoothed
    bool aimingMarker;
    float jumpAimYaw, jumpAimPitch; // where the jump is aimed while AimingMarker (degrees, pitch + = down)
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
        frogMotor = target.GetComponent<FrogMotor>();
        frogRenderers = System.Array.FindAll(target.GetComponentsInChildren<Renderer>(true), r => frogPreview == null || !frogPreview.IsPreviewRenderer(r)); // the jump arc and marker stay visible
        frogShadowModes = new ShadowCastingMode[frogRenderers.Length];
        for (int i = 0; i < frogRenderers.Length; i++)
            frogShadowModes[i] = frogRenderers[i].shadowCastingMode;
        frogHidden = false;
        lastTargetPosition = target.position;
        zoomBlend = markerBlend = 0f;
        lookYaw = target.eulerAngles.y; // Third Person Follow starts right behind the frog
        lookPitch = startPitch;
        SnapFollowPoint();

        FindFollowCamera();
        if (followCamera != null)
        {
            followCamera.Follow = followPoint;
            followCamera.LookAt = followPoint;
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
        if (thirdPerson == null)
        {
            thirdPerson = followCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPerson != null)
            {
                cameraDistance = thirdPerson.CameraDistance;
                shoulderOffset = thirdPerson.ShoulderOffset;
                solidLayers = thirdPerson.AvoidObstacles.CollisionFilter; // the follow point stays out of what the camera collides with
            }
        }

        // Camera collision must keep the frog in sight, not the spot the camera looks at:
        // while aiming that's the landing marker, and the line to it grazes platform edges
        // and the floor, which would drag the camera down toward it. It checks from the
        // height the camera normally looks at, so a camera pushed in close (in a corner)
        // keeps looking where you aimed instead of tipping up toward the frog's head.
        var deoccluder = followCamera.GetComponent<CinemachineDeoccluder>();
        if (deoccluder != null)
        {
            deoccluder.AvoidObstacles.UseFollowTarget.Enabled = true;
            deoccluder.AvoidObstacles.UseFollowTarget.YOffset = composerOffset.y;
            solidLayers = deoccluder.CollideAgainst; // the follow point stays out of what the camera collides with
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

        if (orbit == null && thirdPerson == null)
            return;
        float dt = Time.deltaTime;

        if (Cursor.lockState == CursorLockMode.Locked && Application.isFocused)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            bool fromStick = lookAction.activeControl != null && lookAction.activeControl.device is Gamepad;
            look *= (fromStick ? stickSensitivity * dt : mouseSensitivity) * sensitivityScale;

            if (AimingMarker)
            {
                // Aiming a jump: the mouse steers the aim (where the landing marker goes), not the
                // camera. Third Person Follow turns with it, so the marker stays ahead of you.
                jumpAimYaw = Mathf.Repeat(jumpAimYaw + look.x, 360f);
                jumpAimPitch = Mathf.Clamp(jumpAimPitch + (invertY ? look.y : -look.y), -80f, 89f);
                if (thirdPerson != null)
                    lookYaw = jumpAimYaw;
            }
            else if (thirdPerson != null)
            {
                lookYaw = Mathf.Repeat(lookYaw + look.x, 360f);
                lookPitch = Mathf.Clamp(lookPitch + (invertY ? look.y : -look.y), pitchLimits.x, pitchLimits.y);
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

        }

        // Charging a jump: ease the camera back so the frog's surroundings are on screen. The
        // Deoccluder on the Frog Follow Camera then pulls it in front of any wall or object
        // between it and the frog, and eases it back out once there's room again.
        zoomBlend = Mathf.MoveTowards(zoomBlend, AimingJump ? 1f : 0f, dt / Mathf.Max(0.01f, aimBlendTime));
        float zoomOut = aimZoomOut * Mathf.SmoothStep(0f, 1f, zoomBlend);
        if (orbit != null)
            orbit.Radius = radius + zoomOut;
        else
            thirdPerson.CameraDistance = cameraDistance + zoomOut;

        // Respawn teleport: cut to the frog. Otherwise note how fast it's moving.
        Vector3 moved = target.position - lastTargetPosition;
        lastTargetPosition = target.position;
        if (moved.magnitude > teleportDistance)
            followCamera.PreviousStateIsValid = false;
        moved.y = 0f;
        float speed = dt > 0f ? moved.magnitude / dt : 0f;
        smoothSpeed = Mathf.Lerp(smoothSpeed, speed, 1f - Mathf.Exp(-6f * dt));

        // Spyro style: swing in behind the frog only while it's going somewhere.
        if (recenterOnlyWhileMoving && orbit != null)
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
        if (target == null)
            return;
        UpdateFollowPoint(Time.deltaTime);
        UpdateMarkerLook(Time.deltaTime);
        if (thirdPerson != null)
            UpdateThirdPersonAim();
    }

    // The Cinemachine Camera follows this point instead of the frog itself. It trails the frog
    // sideways (small wobbles inside the Dead Zone are ignored) and ignores small hops, but it
    // follows charged jumps, big hops and drops, and settles at the new height once the frog
    // lands, and it never sinks into what the frog hops onto. FollowRotation sets which way it faces.
    void UpdateFollowPoint(float dt)
    {
        Vector3 frog = target.position;
        Vector3 point = followPoint.position;
        if ((frog - point).sqrMagnitude > teleportDistance * teleportDistance)
        {
            SnapFollowPoint(); // respawned
            return;
        }

        Vector3 away = new Vector3(frog.x - point.x, 0f, frog.z - point.z);
        float distance = away.magnitude;
        Vector3 goal = distance > deadZone ? point + away * (1f - deadZone / distance) : point;
        Vector3 flat = Vector3.SmoothDamp(new Vector3(point.x, 0f, point.z), new Vector3(goal.x, 0f, goal.z), ref followVelocity, followSmoothTime);

        // Small hops keep the height the frog took off from, so hopping around doesn't bob the
        // camera. A charged jump, a hop higher than Hop Headroom, or a drop is followed closely,
        // and once the frog lands (or while it climbs) the height settles more gently.
        bool airborne = frogMotor != null && frogMotor.Mode.Value == FrogMode.Airborne;
        if (!airborne)
            followingJump = false;
        else if (zoomBlend > 0f || markerBlend > 0f || frog.y > point.y + hopHeadroom)
            followingJump = true;
        bool holdHeight = airborne && !followingJump && frog.y > point.y;
        float height = Mathf.SmoothDamp(point.y, holdHeight ? point.y : frog.y, ref heightVelocity, airborne ? followSmoothTime : heightSmoothTime);

        // Keep the point in the open, in sight of the frog. Left inside the box the frog just
        // hopped onto (or a corner it's going round), the camera would treat that box as a wall
        // between it and the frog and jump in front of it.
        Vector3 next = new Vector3(flat.x, height, flat.z);
        if (Physics.Linecast(frog, next, out RaycastHit hit, solidLayers, QueryTriggerInteraction.Ignore))
            next = Vector3.MoveTowards(hit.point, frog, 0.1f);
        followPoint.position = next;

        followPoint.rotation = FollowRotation();
    }

    void SnapFollowPoint()
    {
        if (followPoint == null)
        {
            followPoint = new GameObject("Frog Camera Follow Point").transform;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(followPoint.gameObject, gameObject.scene); // unloads with this camera
        }
        followPoint.position = target.position;
        followPoint.rotation = FollowRotation();
        followVelocity = Vector3.zero;
        heightVelocity = 0f;
    }

    // Which way the follow point faces. Third Person Follow points the camera exactly this way
    // (your mouse look); the orbit only uses it to swing in behind the frog (the frog's heading,
    // never its tilt on a wall).
    Quaternion FollowRotation()
    {
        if (thirdPerson != null)
        {
            // Aiming a jump: tilt down toward Aim Pitch, a view from above that follows the marker.
            float overhead = Mathf.SmoothStep(0f, 1f, zoomBlend);
            return Quaternion.Euler(Mathf.Lerp(lookPitch, Mathf.Max(lookPitch, aimPitch), overhead), lookYaw, 0f);
        }
        Vector3 heading = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        return heading.sqrMagnitude > 0.01f ? Quaternion.LookRotation(heading) : followPoint.rotation; // keep the last heading while it faces straight up or down a wall
    }
    // Third Person Follow while you place a landing marker: the camera's pivot slides part of the
    // way toward the marker (at most Aim Max Slide), so the frog and where it will land are both
    // on screen. It only slides as far as the camera has room: when a wall or shelf pulls the
    // camera in close (on a stool by a desk), it slides less, so the frog never ends up behind it.
    void UpdateThirdPersonAim()
    {
        // Room behind the frog: how far back the camera could go from the frog's own pivot. Measured
        // from there, not from the slid pivot, or sliding would change the room it's based on and
        // the camera would bounce against things on the desk.
        Vector3 pivot = followPoint.position + Quaternion.Euler(0f, lookYaw, 0f) * shoulderOffset;
        float distance = Mathf.Max(0.01f, thirdPerson.CameraDistance);
        float room = 1f;
        if (Physics.SphereCast(pivot, thirdPerson.AvoidObstacles.CameraRadius, -followPoint.forward, out RaycastHit hit, distance, solidLayers, QueryTriggerInteraction.Ignore))
            room = hit.distance / distance;
        aimRoom = Mathf.SmoothDamp(aimRoom, room, ref aimRoomVelocity, 0.15f);

        Vector3 slide = Vector3.ClampMagnitude((markerFocus - followPoint.position) * aimLookAtMarker, aimMaxSlide * aimRoom);
        slide *= Mathf.SmoothStep(0f, 1f, markerBlend);
        thirdPerson.ShoulderOffset = shoulderOffset + Quaternion.Inverse(Quaternion.Euler(0f, lookYaw, 0f)) * slide;
    }

    void OnDestroy()
    {
        if (followPoint != null)
            Destroy(followPoint.gameObject);
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
            if (orbit != null)
            {
                orbit.HorizontalAxis.CancelRecentering();
                orbit.VerticalAxis.CancelRecentering();
            }
        }
        else if (markerBlend <= 0f)
        {
            markerFocus = followPoint.position;
            markerFocusVelocity = Vector3.zero;
        }
        if (composer == null)
            return;

        float blend = Mathf.SmoothStep(0f, 1f, markerBlend) * aimLookAtMarker;
        Vector3 frogPoint = followPoint.position + followPoint.rotation * composerOffset;
        Vector3 look = frogPoint + (markerFocus - followPoint.position) * blend;

        // Turn toward it only so far that the frog stays on screen.
        Vector3 toFrog = frogPoint - transform.position;
        Vector3 toLook = look - transform.position;
        float limit = lens.fieldOfView * 0.5f * keepFrogOnScreen;
        if (Vector3.Angle(toFrog, toLook) > limit)
            look = transform.position + Vector3.RotateTowards(toFrog, toLook, limit * Mathf.Deg2Rad, 0f).normalized * toLook.magnitude;

        composer.TargetOffset = composerOffset + Quaternion.Inverse(followPoint.rotation) * (look - frogPoint);
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
        if (target != null && Cursor.lockState == CursorLockMode.Locked && !AimingMarker) // the landing marker is the cursor while aiming
            GUI.Label(new Rect(Screen.width * 0.5f - 4f, Screen.height * 0.5f - 10f, 20f, 20f), "+");
    }

    static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}