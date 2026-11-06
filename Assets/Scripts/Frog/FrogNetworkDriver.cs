using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

// Collects the local player's input and hands it to the host.
// Only the host simulates frogs. Every other machine only displays them.
public class FrogNetworkDriver : NetworkBehaviour
{
    const float MaxAimDistance = 60f; // the crosshair finds landing spots up to this far away

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

    // Owner only: the landing marker you aim with the crosshair, a spot on a surface. It stays
    // where you aim while the charge swings; the arc shows whether this charge reaches it.
    // HasMarker is false when there is none (straight up, or not aiming). MarkerOnWall: it's on
    // a wall or a side too steep to stand on, so the frog will land there and cling to it.
    public bool HasMarker { get; private set; }
    public Vector3 MarkerPoint { get; private set; }
    public Vector3 MarkerNormal { get; private set; }
    public bool MarkerOnWall { get; private set; }

    // Owner only: the launch the next jump will have. The preview draws exactly this arc,
    // and it is what gets sent to the host on release.
    public Vector3 LocalJumpVelocity
    {
        get
        {
            float heightScale = stats != null ? stats.JumpHeightMultiplier : 1f;
            heightScale *= motor.LocalSlimeJumpMultiplier;
             // On a wall the frog lies flat against it, so its up is the wall's normal. A jump
             // aimed at a marker flies exactly to it, so it gets no extra push off the wall.
             Vector3 wall = motor.Mode.Value == FrogMode.Climbing && !HasMarker ? transform.up : Vector3.zero;
            return motor.JumpVelocity(LocalCharge, LocalJumpTarget, heightScale, wall);
        }
    }

    Rigidbody frogRigidbody;
    FrogMotor motor;
    FrogTongue tongue;
    FrogStats stats;
    FrogCamera frogCamera;
    InputAction moveAction, jumpAction, tongueAction, reelAction, stickAction, breakFreeAction, hopAction;
    FrogInputState lastSent;
    uint lastSentTick;
    readonly RaycastHit[] aimHits = new RaycastHit[8];
    readonly Collider[] aimOverlaps = new Collider[8];
    

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
        stickAction = actions.FindAction("Player/Stick", true);
        breakFreeAction = actions.FindAction("Player/BreakFree", true);
        hopAction = actions.FindAction("Player/Sprint", true);
        HopModeOn = motor.Tuning.hopByDefault;

        frogCamera = Camera.main != null ? Camera.main.GetComponent<FrogCamera>() : null;
        if (frogCamera != null)
            frogCamera.SetTarget(transform);

        if (TestObserver.instance != null)
        {
            TestObserver.instance.RegisterLocalPlayer(this.gameObject, frogRigidbody, transform.position);
        }
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
        if (stickAction.WasPressedThisFrame())
            StickPressed();
        if (breakFreeAction.WasPressedThisFrame())
            BreakFreePressed();
    }

    // Where the next jump should come down. Mouse aiming, while charging: wherever the crosshair
    // points (AimAtCrosshair). Without mouse aiming: the way the frog faces (on a wall, the way
    // the stick points), as far as the charge reaches, or straight up with no direction held.
    void UpdateJumpTarget(float viewYaw, bool supported)
    {
        FrogTuning tuning = motor.Tuning;
        if (tuning.aimJumpsWithMouse && frogCamera != null)
        {
            if (LocalJumpHeld && supported)
                AimAtCrosshair();
            else
                HasMarker = MarkerOnWall = false;
            return;
        }

        HasMarker = MarkerOnWall = false;
        if (LocalMove.sqrMagnitude <= 0.04f)
        {
            LocalJumpTarget = Vector3.zero;
            return;
        }
        Vector3 direction = motor.Mode.Value == FrogMode.Climbing
            ? Quaternion.Euler(0f, viewYaw, 0f) * new Vector3(LocalMove.x, 0f, LocalMove.y)
            : transform.forward;
        float heightScale = stats != null ? stats.JumpHeightMultiplier : 1f;
        heightScale *= motor.LocalSlimeJumpMultiplier;

        float reach = motor.GroundJumpReach(LocalCharge, heightScale).y; // as far as this charge goes
        LocalJumpTarget = Vector3.ProjectOnPlane(direction, Vector3.up).normalized * reach;
    }

    // The landing marker goes on whatever surface the crosshair points at (a floor, a box top, a
    // slope, a wall), if a jump can reach it. A wall, or a side too steep to stand on, is
    // somewhere to cling: the frog lands on it and sticks there (MarkerOnWall). Out of reach (too
    // far, too high, a ceiling, the sky): as far as a jump goes toward it, set down on whatever
    // is there, stepping back toward the frog until it fits.
    void AimAtCrosshair()
    {
        FrogTuning tuning = motor.Tuning;
        Ray aim = frogCamera.AimRay;
        Vector3 aimed = aim.origin + aim.direction * tuning.maxJumpDistance; // the sky: just that way
        if (Cast(aim, MaxAimDistance, out RaycastHit hit))
        {
            aimed = hit.point;
            float heightScale = stats != null ? stats.JumpHeightMultiplier : 1f;
            
            heightScale *= motor.LocalSlimeJumpMultiplier;
            float highest = tuning.maxJumpHeight * heightScale - tuning.jumpClearance; // highest landing a jump can clear
            Vector3 target = hit.point + hit.normal * motor.Radius - transform.position; // the frog's centre, touching the spot
            bool floor = Vector3.Angle(hit.normal, Vector3.up) <= tuning.maxSlopeAngle;
            bool wall = !floor && Vector3.Dot(hit.normal, Vector3.up) >= tuning.minWallUpDot;
            bool inReach = new Vector2(target.x, target.z).magnitude <= tuning.maxJumpDistance && target.y <= highest;
            if ((floor || wall) && inReach)
            {
                HasMarker = true;
                MarkerOnWall = wall;
                MarkerPoint = hit.point;
                MarkerNormal = hit.normal;
                LocalJumpTarget = target;
                return;
            }
        }

        MarkerOnWall = false;
        Vector3 toward = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(aimed - transform.position, Vector3.up), tuning.maxJumpDistance);
        for (int i = 10; i >= 0; i--)
        {
            if (MarkerTarget(toward * (i / 10f), out Vector3 dropped))
            {
                LocalJumpTarget = dropped;
                return;
            }
        }
    }

    // The landing marker `offset` (flat, metres) from the frog, set down on the highest
    // surface there that a jump can clear (a platform top, the floor below...): exactly where
    // a frog-sized ball dropped from the top of the jump would come to rest. target: where the
    // frog's centre should come down, relative to the frog. False, changing nothing, when the
    // spot is inside something too high to land on (a wall, the side of a tall box). A tiny
    // offset = straight up, with no marker.
    bool MarkerTarget(Vector3 offset, out Vector3 target)
    {
        target = Vector3.zero;
        if (offset.sqrMagnitude < 0.0025f)
        {
            HasMarker = false;
            return true;
        }
        // A slightly smaller ball, so a wall the frog is only touching doesn't count.
        float radius = motor.Radius * 0.9f;
        // The highest the frog's centre can get and still clear an edge: its highest jump
        // less Jump Clearance, or lower under a ceiling.
        float heightScale = stats != null ? stats.JumpHeightMultiplier : 1f;
        heightScale *= motor.LocalSlimeJumpMultiplier;

        float reach = Mathf.Max(
            0.05f,
            motor.Tuning.maxJumpHeight * heightScale - motor.Tuning.jumpClearance
        );
        if (SphereCast(new Ray(transform.position, Vector3.up), radius, reach, out RaycastHit ceiling))
            reach = ceiling.distance;
        Vector3 top = transform.position + offset + Vector3.up * reach;
        if (Blocked(top, radius))
            return false;
        if (!SphereCast(new Ray(top, Vector3.down), radius, reach + MaxAimDistance, out RaycastHit ground))
        {
            HasMarker = false;
            target = offset; // nothing below: aim level, it will fall
            return true;
        }
        HasMarker = true;
        MarkerPoint = ground.point;
        MarkerNormal = ground.normal;
        target = top + Vector3.down * ground.distance + Vector3.up * (motor.Radius - radius) - transform.position;
        return true;
    }


    // Nearest hit along a ray, ignoring this frog's own collider.
    bool Cast(Ray ray, float distance, out RaycastHit best)
    {
        int count = Physics.RaycastNonAlloc(ray, aimHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        bool found = false;
        best = default;
        for (int i = 0; i < count; i++)
        {
            if (aimHits[i].rigidbody == frogRigidbody)
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

    // Whether a ball `radius` wide at point would be inside something other than this frog.
    bool Blocked(Vector3 point, float radius)
    {
        int count = Physics.OverlapSphereNonAlloc(point, radius, aimOverlaps, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
            if (aimOverlaps[i].attachedRigidbody != frogRigidbody)
                return true;
        return false;
    }

    void ReleaseJumpLocally()
    {
        LocalJumpHeld = false;
        // Aimed at a wall: tell the host which wall, and where the frog's centre comes to rest on it.
        bool toWall = HasMarker && MarkerOnWall;
        JumpReleased(LocalJumpVelocity, toWall ? MarkerNormal : Vector3.zero, transform.position + LocalJumpTarget);
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
    void TonguePressed(Vector3 aimOrigin, Vector3 aimDirection) => tongue.PressTongue(aimOrigin, aimDirection);

    [ServerRpc]
    void StickPressed() => motor.PressStick();

    [ServerRpc]
    void BreakFreePressed() => FrogTongue.ReleaseEverythingHolding(NetworkObject);
 
  // Public: the death screen calls it on your own frog when its countdown ends.
    [ServerRpc]
     public void RequestRespawn() => stats.Revive();
}