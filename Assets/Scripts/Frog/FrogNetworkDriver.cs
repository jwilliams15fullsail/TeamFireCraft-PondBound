using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;


// Collects the local player's input and hands it to the host.
// Only the host simulates frogs. Every other machine only displays them.
public class FrogNetworkDriver : NetworkBehaviour
{
    const float MaxPickDistance = 200f; // how far the aim cursor looks for a surface
    const int PathChecks = 24;          // sections a planned jump path is checked in for walls in the way

    [Tooltip("While aiming, write on screen what the aim cursor is pointing at and why the marker can or can't go there. For tracking down spots the marker won't move onto.")]
    [SerializeField] bool debugAim;
    string lastPick = "";

    // The host's copy of this player's input, shared with everyone (animation reads it).
    readonly SyncVar<FrogInputState> input = new(new SyncTypeSettings(0.001f));

    // Read by the host's FrogMotor / FrogTongue every physics step.
    public FrogInputState CurrentInput => input.Value;

    // Owner only: lets the visuals crouch the instant Space goes down, instead of waiting a
    // round trip for the host. The held time only counts while the frog is on the ground or
    // a wall, like the host's charge.
    public bool LocalJumpHeld { get; private set; }
    public float LocalJumpHeldTime { get; private set; }

    // Owner only: the charge (0..1) right now, as this player sees it on the meter.
    public float LocalCharge => motor.ChargeFromSeconds(LocalJumpHeldTime);

    // Owner only: the stick right now and whether hop mode is on, for the HUD and the
    // jump preview (the host's copy in CurrentInput arrives a moment later).
    public Vector2 LocalMove { get; private set; }
    public bool HopModeOn { get; private set; }

    // Owner only: where the next jump should come down, as an offset from the frog to the
    // spot its centre should reach (zero = straight up). With Aim Jumps With Mouse it is
    // the landing marker you place while charging.
    public Vector3 LocalJumpTarget { get; private set; }

    // Owner only: the landing marker, a spot on any surface: a floor, a desk or cabinet top, a
    // wall or a side too steep to stand on, at any height. HasMarker is false when there is
    // none (straight up, or not aiming). MarkerOnWall: the frog will land there and cling.
    public bool HasMarker { get; private set; }
    public Vector3 MarkerPoint { get; private set; }
    public Vector3 MarkerNormal { get; private set; }
    public bool MarkerOnWall { get; private set; }

    // Owner only: the path the next jump flies when there is a marker. The frog leaves its
    // surface (floor or wall) along that surface's normal, arcs over anything in the way and
    // comes down onto the marker's surface. The preview draws exactly this, and the host moves
    // the frog along exactly this. PathBlocked: no clear path was found (it would hit something).
    public bool HasPath { get; private set; }
    public bool PathBlocked { get; private set; }
    public Vector3 PathStart { get; private set; }
    public Vector3 PathControl1 { get; private set; }
    public Vector3 PathControl2 { get; private set; }
    public Vector3 PathEnd { get; private set; }
    public float PathDuration { get; private set; }
    // How high (m) the path rises above where the frog starts, for the HUD.
    public float PathHeight { get; private set; }

    // Owner only, while aiming: the aim cursor (screen pixels, from the bottom left). It moves with
    // the mouse; the marker follows it onto every surface it passes over that the frog can land
    // on. CursorOnMarker is false while it crosses something it can't (a gap, an overhang), and
    // the camera then draws it so the player can see where it is.
    public Vector2 AimCursor { get; private set; }
    public bool CursorOnMarker { get; private set; }

    // Owner only: the launch the next jump will have when it has no path (straight up, or
    // jumps with mouse aiming off). The preview draws this arc, and it is sent to the host.
    public Vector3 LocalJumpVelocity
    {
        get
        {
            // On a wall the frog lies flat against it, so its up is the wall's normal: a jump
            // from a wall always pushes away from it.
            Vector3 wall = motor.Mode.Value == FrogMode.Climbing ? transform.up : Vector3.zero;
            return motor.JumpVelocity(LocalCharge, LocalJumpTarget, aimHeightScale, wall);
        }
    }

    Rigidbody frogRigidbody;
    FrogMotor motor;
    FrogTongue tongue;
    FrogStats stats;
    FrogCamera frogCamera;
    InputAction moveAction, jumpAction, tongueAction, reelAction, reelWheelAction, stickAction, breakFreeAction, hopAction;
    FrogInputState lastSent;
    uint lastSentTick;
    readonly RaycastHit[] aimHits = new RaycastHit[16];
    // This frame's jump height scale (hunger, slime underfoot), worked out once per frame: it
    // costs a ground probe.
    float aimHeightScale = 1f;
    bool aimStarted; // this charge has placed its first marker

    void Awake()
    {
        frogRigidbody = GetComponent<Rigidbody>();
        motor = GetComponent<FrogMotor>();
        tongue = GetComponent<FrogTongue>();
        stats = GetComponent<FrogStats>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
            return;

        // Project-wide actions (Edit > Project Settings > Input System Package).
        var actions = InputSystem.actions;
        moveAction = actions.FindAction("Player/Move", true);
        jumpAction = actions.FindAction("Player/Jump", true);
        tongueAction = actions.FindAction("Player/Tongue", true);
        reelAction = actions.FindAction("Player/Reel", true);
        reelWheelAction = actions.FindAction("Player/ReelWheel", true);
        stickAction = actions.FindAction("Player/Stick", true);
        breakFreeAction = actions.FindAction("Player/BreakFree", true);
        hopAction = actions.FindAction("Player/Sprint", true);
        HopModeOn = motor.Tuning.hopByDefault;

        frogCamera = Camera.main != null ? Camera.main.GetComponent<FrogCamera>() : null;
        if (frogCamera != null)
            frogCamera.SetTarget(transform);
    }

    void Update()
    {
        if (!IsSpawned || !IsOwner || moveAction == null)
            return;

        // Ignore input while the window is unfocused (two Multiplayer Play Mode
        // players on one PC) or while the cursor is free to click UI.
        bool active = Application.isFocused && Cursor.lockState == CursorLockMode.Locked;
        bool supported = motor.Mode.Value == FrogMode.Grounded || motor.Mode.Value == FrogMode.Climbing;

        // Sprint switches hop mode on and off (or hops only while held, if Hop Toggle is off).
        if (!motor.Tuning.hopToggle)
            HopModeOn = active && hopAction.IsPressed();
        else if (active && hopAction.WasPressedThisFrame())
            HopModeOn = !HopModeOn;
        LocalMove = active ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;
        float viewYaw = frogCamera != null ? Mathf.Round(frogCamera.Yaw) : transform.eulerAngles.y;
        if (frogCamera != null)
            frogCamera.AimingJump = LocalJumpHeld && supported;
        UpdateJumpTarget(viewYaw, supported);

        var state = new FrogInputState
        {
            Move = LocalMove,
            ViewYaw = viewYaw,
            Reel = active && reelAction.IsPressed(),
            Hop = HopModeOn,
            // Only while charging (the host turns the frog toward it), in 2 cm steps, so
            // moving the camera around doesn't send input every tick.
            JumpTarget = LocalJumpHeld
                ? new Vector2(Mathf.Round(LocalJumpTarget.x * 50f), Mathf.Round(LocalJumpTarget.z * 50f)) / 50f
                : Vector2.zero,
        };
        // Send changes at most once per tick. A change held back this tick goes out next tick.
        if (!state.Equals(lastSent) && TimeManager.LocalTick != lastSentTick)
        {
            lastSent = state;
            lastSentTick = TimeManager.LocalTick;
            SubmitInput(state);
        }

        if (!active)
        {
            if (LocalJumpHeld)
                ReleaseJumpLocally();
            return;
        }

        if (LocalJumpHeld)
            LocalJumpHeldTime = supported ? LocalJumpHeldTime + Time.deltaTime : 0f;

        if (jumpAction.WasPressedThisFrame())
        {
            LocalJumpHeld = true;
            LocalJumpHeldTime = 0f;
            JumpPressed();
        }
        if (jumpAction.WasReleasedThisFrame() && LocalJumpHeld)
            ReleaseJumpLocally();

        if (tongueAction.WasPressedThisFrame())
        {
            Ray aim = frogCamera != null ? frogCamera.AimRay : new Ray(transform.position, transform.forward);
            TonguePressed(aim.origin, aim.direction);
        }

        // Mouse wheel: up reels the tongue in, down lets it out. Each scroll goes to the host at
        // once, like a button press, so no notch gets lost.
        float notches = Mathf.Clamp(reelWheelAction.ReadValue<float>(), -3f, 3f);
        if (notches != 0f && tongue.Sync.Value.State == TongueState.Attached)
            ReelWheel(notches * motor.Tuning.reelPerNotch);

        if (stickAction.WasPressedThisFrame())
            StickPressed();
        if (breakFreeAction.WasPressedThisFrame())
            BreakFreePressed();
    }

    // Where the next jump should come down. Mouse aiming, while charging: the landing marker
    // you move with the mouse (AimLandingMarker). Without mouse aiming: the way the frog faces
    // (on a wall, the way the stick points), as far as the charge reaches, or straight up.
    void UpdateJumpTarget(float viewYaw, bool supported)
    {
        FrogTuning tuning = motor.Tuning;
        aimHeightScale = (stats != null ? stats.JumpHeightMultiplier : 1f) * motor.LocalSlimeJumpMultiplier;
        if (tuning.aimJumpsWithMouse && frogCamera != null)
        {
            bool aiming = LocalJumpHeld && supported;
            frogCamera.AimingMarker = aiming; // the camera goes overhead and follows the marker
            if (aiming)
            {
                AimLandingMarker();
            }
            else
            {
                aimStarted = false;
                HasMarker = MarkerOnWall = HasPath = false;
            }
            return;
        }

        HasMarker = MarkerOnWall = HasPath = false;
        if (LocalMove.sqrMagnitude <= 0.04f)
        {
            LocalJumpTarget = Vector3.zero;
            return;
        }
        Vector3 direction = motor.Mode.Value == FrogMode.Climbing
            ? Quaternion.Euler(0f, viewYaw, 0f) * new Vector3(LocalMove.x, 0f, LocalMove.y)
            : transform.forward;
        float reach = motor.GroundJumpReach(LocalCharge, aimHeightScale).y; // as far as this charge goes
        LocalJumpTarget = Vector3.ProjectOnPlane(direction, Vector3.up).normalized * reach;
    }

    // ---- The landing marker ----
    // It works like a mouse cursor laid on the world: it starts where the crosshair points, and
    // moving the mouse slides it across the screen onto whatever surface is under it there, a
    // floor, the front of a desk, the desk's top, the side of a cabinet, at any angle or height,
    // as long as it's within Max Aim Range of the frog. It stays put in the world while the
    // camera moves, and hold Free Look (right mouse) to swing the camera round for a better view.

    void AimLandingMarker()
    {
        Camera lens = frogCamera.Lens;
        Vector2 delta = frogCamera.TakeJumpAimDelta();
        if (lens == null)
            return;

        if (!aimStarted)
        {
            aimStarted = true;
            StartCursor(lens);
        }
        else
        {
            // While the cursor sits on the marker it stays glued to it as the camera moves.
            if (HasMarker && CursorOnMarker)
            {
                Vector3 onScreen = lens.WorldToScreenPoint(MarkerPoint);
                if (onScreen.z > 0f)
                    AimCursor = onScreen;
            }
            if (delta.sqrMagnitude > 0f)
                MoveCursor(lens, delta);
        }

        // Out of reach now (the frog slid, or a hungry frog's reach shrank): no jump to it.
        if (HasMarker && !InReach(MarkerPoint + MarkerNormal * motor.Radius))
            HasMarker = false;

        if (HasMarker)
            PlanPath();
        else
        {
            HasPath = false;
            MarkerOnWall = false;
            LocalJumpTarget = Vector3.zero; // no marker: straight up
        }
    }

    // The cursor starts where the crosshair points, or else just in front of the frog.
    void StartCursor(Camera lens)
    {
        HasMarker = CursorOnMarker = false;
        AimCursor = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (TryPick(CursorRay(lens, AimCursor), out RaycastHit hit) == Pick.Ok)
        {
            SetMarker(hit);
            return;
        }
        Vector3 frogOnScreen = lens.WorldToScreenPoint(transform.position);
        if (frogOnScreen.z <= 0f)
            return;
        Vector2 inFront = (Vector2)frogOnScreen + new Vector2(0f, Screen.height * 0.08f);
        if (TryPick(CursorRay(lens, inFront), out hit) == Pick.Ok)
        {
            AimCursor = inFront;
            SetMarker(hit);
        }
    }

    // Moves the cursor by delta (pixels). Onto a surface the frog can reach: the marker goes
    // there. Over nothing it can land on (the gap between a desk and a cabinet, the floor under
    // an overhang, the underside of a shelf): the cursor carries on across, and the marker waits
    // at the last good spot until the cursor reaches the next surface. Past Max Aim Range: the
    // cursor stops at the edge of the reach, so it never runs off where the marker can't follow.
    void MoveCursor(Camera lens, Vector2 delta)
    {
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        for (float part = 1f; part >= 0.12f; part *= 0.5f)
        {
            Vector2 next = AimCursor + delta * part;
            next.x = Mathf.Clamp(next.x, screen.xMin, screen.xMax - 1f);
            next.y = Mathf.Clamp(next.y, screen.yMin, screen.yMax - 1f);
            switch (TryPick(CursorRay(lens, next), out RaycastHit hit))
            {
                case Pick.Ok:
                    AimCursor = next;
                    SetMarker(hit);
                    return;
                case Pick.Unusable:
                    AimCursor = next;
                    CursorOnMarker = false;
                    return;
                case Pick.OutOfReach:
                    continue; // try a shorter step toward the edge of the reach
            }
        }
    }

    void SetMarker(RaycastHit hit)
    {
        HasMarker = true;
        CursorOnMarker = true;
        MarkerPoint = hit.point;
        MarkerNormal = hit.normal;
        MarkerOnWall = Vector3.Angle(hit.normal, Vector3.up) > motor.Tuning.maxSlopeAngle;
    }

    enum Pick { Ok, Unusable, OutOfReach }

    // The ray under the cursor, starting near the frog instead of at the lens. The camera sits a
    // few metres behind the frog and gets pushed up against things (the back of a cabinet, a
    // glass panel, a lamp, the outside of a wall it can't draw), and a ray from the lens would
    // stop on those before ever reaching the desk or cabinet the player is pointing at. Anything
    // nearer the camera than the frog's reach is skipped.
    Ray CursorRay(Camera lens, Vector2 screenPoint)
    {
        Ray ray = lens.ScreenPointToRay(screenPoint);
        float frogDepth = Vector3.Dot(transform.position - ray.origin, ray.direction);
        float skip = Mathf.Max(0f, frogDepth - motor.Tuning.maxAimRange - motor.Radius);
        return new Ray(ray.GetPoint(skip), ray.direction);
    }

    // A surface the frog can land on or cling to, within reach. Ceilings don't count, and on
    // something to stand on the frog's centre must be over solid ground (not hanging off an
    // edge, where it would slide off).
    // Other frogs don't count: the cursor looks past them to what they're standing on.
    Pick TryPick(Ray ray, out RaycastHit hit)
    {
        if (!Cast(ray, MaxPickDistance, out hit, skipFrogs: true))
            return Report(Pick.Unusable, "nothing under the cursor", hit);
        FrogTuning tuning = motor.Tuning;
        if (Vector3.Dot(hit.normal, Vector3.up) < tuning.minWallUpDot)
            return Report(Pick.Unusable, "a ceiling or underside", hit);
        Vector3 centre = hit.point + hit.normal * motor.Radius;
        if (!InReach(centre))
            return Report(Pick.OutOfReach, $"out of reach ({Vector3.Distance(centre, transform.position):0.0} m)", hit);
        bool floor = Vector3.Angle(hit.normal, Vector3.up) <= tuning.maxSlopeAngle;
        if (floor && !HasFooting(centre))
            return Report(Pick.Unusable, "the frog would hang off an edge there", hit);
        return Report(Pick.Ok, floor ? "landing spot" : "wall to cling to", hit);
    }

    // Debug Aim: remembers what the cursor last found and why, for the on-screen readout.
    Pick Report(Pick result, string why, RaycastHit hit)
    {
        if (debugAim)
            lastPick = hit.collider != null
                ? $"Cursor: {hit.collider.name}  {why}  (normal {hit.normal.x:0.0} {hit.normal.y:0.0} {hit.normal.z:0.0})"
                : $"Cursor: {why}";
        return result;
    }

    void OnGUI()
    {
        if (!debugAim || !IsOwner || !LocalJumpHeld)
            return;
        GUI.Label(new Rect(10f, Screen.height * 0.5f, 900f, 24f), lastPick);
        GUI.Label(new Rect(10f, Screen.height * 0.5f + 20f, 900f, 24f),
            HasMarker ? $"Marker on: {(MarkerOnWall ? "wall" : "floor / top")}   path {(PathBlocked ? "BLOCKED" : "clear")}" : "No marker");
    }

    // Within reach: Max Aim Range up or across, and any drop down to Max Drop Height (FrogTuning.InAimReach).
    bool InReach(Vector3 centre) => motor.Tuning.InAimReach(centre - transform.position, aimHeightScale);

    static float Square(float x) => x * x;

    // Whether the frog's centre, resting there, is right over ground it can stand on.
    bool HasFooting(Vector3 centre)
    {
        if (!Cast(new Ray(centre + Vector3.up * 0.05f, Vector3.down), motor.Radius + 0.25f, out RaycastHit below))
            return false;
        return Vector3.Angle(below.normal, Vector3.up) <= motor.Tuning.maxSlopeAngle;
    }

    // ---- The jump path ----
    // A smooth curve (cubic Bezier) from the frog to the marker. It leaves the frog's surface
    // along that surface's normal (straight up off a floor, straight out from a wall) and arrives
    // on the marker's surface along its normal, rising as high as it needs to clear anything in
    // the way: the edge of a desk, the lip of a cabinet, the corner round a wall. The charge
    // makes it rise higher.
    void PlanPath()
    {
        FrogTuning tuning = motor.Tuning;
        Vector3 start = transform.position;
        Vector3 end = MarkerPoint + MarkerNormal * motor.Radius;
        // The surface the frog is on: a wall it clings to (its belly faces it), or the floor.
        Vector3 startNormal = motor.Mode.Value == FrogMode.Climbing ? transform.up : Vector3.up;
        Vector3 leave = (startNormal + Vector3.up).normalized;
        Vector3 arrive = (MarkerNormal + Vector3.up).normalized;

        float distance = Vector3.Distance(start, end);
        float charge = tuning.chargeCurve.Evaluate(LocalCharge);
        float lift = Mathf.Max(0.25f, distance * 0.35f) * Mathf.Lerp(1f, 1.6f, charge);

        // The lowest arc (for this charge) that clears everything; if none does, the lowest one,
        // drawn red, so the player can see it would hit something.
        bool found = false;
        Vector3 c1 = start + leave * lift, c2 = end + arrive * lift;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            float a = lift * (1f + 0.5f * attempt);
            Vector3 tryC1 = start + leave * a;
            Vector3 tryC2 = end + arrive * a;
            if (PathClear(start, tryC1, tryC2, end))
            {
                c1 = tryC1;
                c2 = tryC2;
                found = true;
                break;
            }
        }

        HasPath = true;
        PathBlocked = !found;
        PathStart = start;
        PathControl1 = c1;
        PathControl2 = c2;
        PathEnd = end;
        LocalJumpTarget = end - start;

        float length = 0f, highest = start.y;
        Vector3 previous = start;
        for (int i = 1; i <= PathChecks; i++)
        {
            Vector3 point = FrogMotor.PathPoint(start, c1, c2, end, i / (float)PathChecks);
            length += Vector3.Distance(previous, point);
            highest = Mathf.Max(highest, point.y);
            previous = point;
        }
        PathHeight = highest - start.y;
        PathDuration = Mathf.Max(tuning.minPathJumpTime, length / Mathf.Max(0.5f, tuning.pathJumpSpeed));
    }

    // Whether a ball a little smaller than the frog can fly the whole path without touching
    // anything (other than this frog).
    bool PathClear(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float radius = motor.Radius * 0.8f;
        Vector3 previous = p0;
        for (int i = 1; i <= PathChecks; i++)
        {
            Vector3 point = FrogMotor.PathPoint(p0, p1, p2, p3, i / (float)PathChecks);
            Vector3 step = point - previous;
            if (step.sqrMagnitude > 0.000001f
                && SphereCast(new Ray(previous, step), radius, step.magnitude, out _))
                return false;
            previous = point;
        }
        return true;
    }


    // Nearest hit along a ray, ignoring this frog's own collider.
    // skipFrogs: other players' frogs don't count either.
    bool Cast(Ray ray, float distance, out RaycastHit best, bool skipFrogs = false)
    {
        int count = Physics.RaycastNonAlloc(ray, aimHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        bool found = false;
        best = default;
        for (int i = 0; i < count; i++)
        {
            Rigidbody body = aimHits[i].rigidbody;
            if (body == frogRigidbody || (skipFrogs && body != null && body.GetComponent<FrogMotor>() != null))
                continue;
            if (!found || aimHits[i].distance < best.distance)
            {
                best = aimHits[i];
                found = true;
            }
        }
        return found;
    }


    // Nearest hit sweeping a ball `radius` wide along a ray, ignoring this frog's own
    // collider and anything the ball starts out touching.
    bool SphereCast(Ray ray, float radius, float distance, out RaycastHit best)
    {
        int count = Physics.SphereCastNonAlloc(ray, radius, aimHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        bool found = false;
        best = default;
        for (int i = 0; i < count; i++)
        {
            if (aimHits[i].rigidbody == frogRigidbody || aimHits[i].distance <= 0f)
                continue;
            if (!found || aimHits[i].distance < best.distance)
            {
                best = aimHits[i];
                found = true;
            }
        }
        return found;
    }

    void ReleaseJumpLocally()
    {
        LocalJumpHeld = false;
        if (HasPath)
            JumpPathReleased(PathStart, PathControl1, PathControl2, PathEnd, MarkerNormal, PathDuration);
        else
            JumpReleased(LocalJumpVelocity, Vector3.zero, transform.position + LocalJumpTarget);
        HasPath = false;
    }

    // ---- Owner -> host commands. The host only queues them; FrogMotor and
    // ---- FrogTongue apply them inside FixedUpdate (one simulation path).
    // ---- [ServerRpc] only accepts calls from this frog's owner by default.

    [ServerRpc]
    void SubmitInput(FrogInputState state) => input.Value = state;

    [ServerRpc]
    void JumpPressed() => motor.PressJump();

    [ServerRpc]
    void JumpReleased(Vector3 velocity, Vector3 wallNormal, Vector3 wallTarget) => motor.ReleaseJump(velocity, wallNormal, wallTarget);

    [ServerRpc]
    void JumpPathReleased(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, Vector3 endNormal, float duration) =>
        motor.ReleasePathJump(start, control1, control2, end, endNormal, duration);

    [ServerRpc]
    void TonguePressed(Vector3 aimOrigin, Vector3 aimDirection) => tongue.PressTongue(aimOrigin, aimDirection);
    [ServerRpc]
    void ReelWheel(float metres) => tongue.ReelBy(metres);

    [ServerRpc]
    void StickPressed() => motor.PressStick();

    [ServerRpc]
    void BreakFreePressed() => FrogTongue.ReleaseEverythingHolding(NetworkObject);

    // Public: the death screen calls it on your own frog when its countdown ends.
    [ServerRpc]
    public void RequestRespawn() => stats.Revive();
}