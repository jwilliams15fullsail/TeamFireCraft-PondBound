using System;
using FishNet.Component.Transforming;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public enum FrogMode : byte { Grounded, Airborne, Climbing, Disabled }

// The shared hop clock for hop locomotion. The host starts and chains hops and writes
// this once per hop; every machine works out the same Jump2 frame from it, so the
// animation stays in step with the body without streaming any animation data.
// FishNet writes this struct over the network automatically (its public fields).
public struct HopSync : IEquatable<HopSync>
{
    public double StartTime;   // network time at which this hop's phase was 0
    public float Duration;     // seconds for the whole hop
    public float PrevDuration; // the hop chained just before this one (0 = started from standing)
    public double StopTime;    // network time hopping stops (double.MaxValue while it carries on)

    public bool Equals(HopSync other)
    {
        return StartTime == other.StartTime && Duration == other.Duration
            && PrevDuration == other.PrevDuration && StopTime == other.StopTime;
    }
}

// Host-side frog physics: ground probe, movement, charged jumps and the movement
// state machine. Clients never run the simulation; they see the result through
// NetworkTransform and the SyncVars below.
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class FrogMotor : NetworkBehaviour
{
    [SerializeField] FrogTuning tuning;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] float respawnHeight = -20f;

    // FishNet's NetworkTransform shows other frogs this many ticks late (its Interpolation setting).
    const int TransformInterpolationTicks = 2;

    // Replicated state. The host writes it, every machine reads it for visuals.
    // Send rate 0.001 s rounds up to every tick, so jumps and hops show without extra delay.
    // (Exactly 0 would count as "use the default", which is only every 0.1 s.)
    public readonly SyncVar<FrogMode> Mode = new(FrogMode.Airborne, new SyncTypeSettings(0.001f));
    public readonly SyncVar<bool> IsCharging = new(new SyncTypeSettings(0.001f));
    public readonly SyncVar<double> ChargeStartTime = new(new SyncTypeSettings(0.001f));
    public readonly SyncVar<HopSync> Hop = new(new SyncTypeSettings(0.001f));

    public FrogTuning Tuning => tuning;
    public Rigidbody Body => rb;
    public float Radius => sphere.radius;
    public Vector3 GroundNormal { get; private set; } = Vector3.up;

    // The host's clock (seconds), the same on every machine: FishNet's tick plus how far
    // into the next tick we are, so it moves smoothly every frame.
    public double NetworkTime => TimeManager.TicksToTime(TimeManager.GetPreciseTick(TickType.Tick));

    Rigidbody rb;
    SphereCollider sphere;
    FrogNetworkDriver driver;
    FrogClimb climb;
    FrogStats stats;
    NetworkTransform networkTransform;

    bool jumpHeld, jumpPressQueued, jumpReleaseQueued, stickQueued;
    float lastGroundedTime = -10f;
    float ignoreGroundUntil;
    Rigidbody support;
    Vector3 spawnPosition;
    bool autoClimbBlocked;  // let go of a wall with Stick: no grabbing in the air until we land

    bool hopping;                           // host only: the hop being simulated
    float hopPhase, hopDuration, hopStride;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();
        driver = GetComponent<FrogNetworkDriver>();
        climb = GetComponent<FrogClimb>();
        stats = GetComponent<FrogStats>();
        networkTransform = GetComponent<NetworkTransform>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnPosition = transform.position;
    }

    // Jump charge as 0..1. Works on every machine because it uses network time.
    public float ChargeFraction()
    {
        if (!IsCharging.Value)
            return 0f;
        double held = NetworkTime - ChargeStartTime.Value;
        return Mathf.Clamp01((float)held / tuning.fullChargeTime);
    }

    // Where this frog is in its current hop (0..1), on any machine; false when not hopping.
    // The host reads its own simulation. Everyone else runs the shared hop clock as far
    // behind as their copy of the frog is drawn (half a round trip for the update to
    // arrive, plus the NetworkTransform's interpolation), so the feet stay planted.
    public bool TryGetHopPhase(out float phase)
    {
        phase = hopPhase;
        if (IsServerInitialized)
            return hopping;

        HopSync hop = Hop.Value;
        double clock = NetworkTime
            - TimeManager.HalfRoundTripTime / 1000.0
            - TransformInterpolationTicks * TimeManager.TickDelta
            - tuning.remoteHopDelay;
        if (hop.Duration <= 0f || clock >= hop.StopTime)
            return false;
        if (clock >= hop.StartTime)
        {
            phase = Mathf.Min(1f, (float)((clock - hop.StartTime) / hop.Duration)); // next hop late: hold the landing
            return true;
        }
        // Our clock is behind the host's: still finishing the hop before this one.
        if (hop.PrevDuration <= 0f)
            return false;
        phase = 1f - (float)((hop.StartTime - clock) / hop.PrevDuration);
        return phase >= 0f;
    }

    // ---- Commands (host only). Queued now, applied in FixedUpdate. ----

    public void PressJump()
    {
        jumpHeld = true;
        jumpPressQueued = true;
    }

    public void ReleaseJump()
    {
        jumpHeld = false;
        jumpReleaseQueued = true;
    }

    public void PressStick() => stickQueued = true;

    // ---- Simulation (host only) ----

    void FixedUpdate()
    {
       
        if (!IsServerInitialized || !IsSpawned)
            return;

        float dt = Time.fixedDeltaTime;
        FrogInputState input = driver.CurrentInput;

        if (rb.position.y < respawnHeight)
        {
            Respawn();
            return;
        }
        if (Mode.Value == FrogMode.Disabled)
            return;

        HandleStick(input);
        if (Mode.Value != FrogMode.Climbing)
            UpdateGroundState();
        HandleAutoClimb(input);
        UpdateJump(input);
        if (Mode.Value != FrogMode.Grounded)
            EndHop(); // stuck to a wall, jumped or fell: the hop is over

        if (Mode.Value == FrogMode.Climbing)
        {
            if (!climb.Simulate(input, IsCharging.Value, dt))
                LeaveSurface();
        }
        else
        {
            Move(input, dt);
        }
    }

    void HandleStick(FrogInputState input)
    {
        if (!stickQueued)
            return;
        stickQueued = false;

        if (Mode.Value == FrogMode.Climbing)
        {
            LeaveSurface();
            autoClimbBlocked = true; // let go on purpose: don't grab the wall again on the way down
        }
        else if (climb.TryAttach(input))
        {
            StartClimbing();
        }
    }

    // Jumped (or fell) into a climbable wall: grab it without pressing Stick.
    void HandleAutoClimb(FrogInputState input)
    {
        if (Mode.Value == FrogMode.Grounded)
            autoClimbBlocked = false;
        if (!tuning.autoClimb || autoClimbBlocked || Mode.Value != FrogMode.Airborne)
            return;
        if (climb.TryAutoAttach(input))
            StartClimbing();
    }

    void StartClimbing()
    {
        IsCharging.Value = false;
        Mode.Value = FrogMode.Climbing;
    }

    void LeaveSurface()
    {
        climb.Detach();
        IsCharging.Value = false;
        Mode.Value = FrogMode.Airborne;
    }

    void UpdateGroundState()
    {
        bool hitGround = ProbeGround(out RaycastHit hit);
        if (hitGround)
        {
            lastGroundedTime = Time.time;
            GroundNormal = hit.normal;
            support = hit.rigidbody;
        }

        if (Mode.Value == FrogMode.Airborne && hitGround)
        {
            Mode.Value = FrogMode.Grounded;
        }
        else if (Mode.Value == FrogMode.Grounded && Time.time - lastGroundedTime > tuning.coyoteTime)
        {
            // Hysteresis: only become airborne after the ground has been missing for a moment.
            Mode.Value = FrogMode.Airborne;
            IsCharging.Value = false;
            GroundNormal = Vector3.up;
            support = null;
        }
    }

    bool ProbeGround(out RaycastHit hit)
    {
        hit = default;
        if (Time.time < ignoreGroundUntil)
            return false;

        // A slightly smaller sphere cast straight down from the body's centre.
        float castRadius = Radius * 0.9f;
        float distance = (Radius - castRadius) + tuning.groundProbeDistance;
        if (!Physics.SphereCast(rb.position, castRadius, Vector3.down, out hit, distance, groundMask, QueryTriggerInteraction.Ignore))
            return false;
        if (hit.rigidbody == rb)
            return false;
        return Vector3.Angle(hit.normal, Vector3.up) <= tuning.maxSlopeAngle;
    }

    void UpdateJump(FrogInputState input)
    {
        bool onSupport = Mode.Value == FrogMode.Grounded || Mode.Value == FrogMode.Climbing;

        // Holding jump while landing starts a charge too (a simple input buffer).
        if ((jumpPressQueued || jumpHeld) && onSupport && !IsCharging.Value)
        {
            IsCharging.Value = true;
            ChargeStartTime.Value = NetworkTime;
        }
        jumpPressQueued = false;

        if (jumpReleaseQueued)
        {
            jumpReleaseQueued = false;
            if (IsCharging.Value)
                Launch(input);
        }
    }

    void Launch(FrogInputState input)
    {
        float charge = tuning.chargeCurve.Evaluate(ChargeFraction());
        float speed = Mathf.Lerp(tuning.minJumpSpeed, tuning.maxJumpSpeed, charge);
        // A hungry frog jumps lower. Height grows with the square of launch speed.
        if (stats != null)
            speed *= Mathf.Sqrt(stats.JumpHeightMultiplier);
        IsCharging.Value = false;

        // Holding a direction = leap that way. No direction = straight up.
        bool aiming = input.Move.sqrMagnitude > 0.04f;
        Vector3 aim = aiming ? input.WorldMove.normalized : Vector3.zero;

        Vector3 surfaceNormal;
        Vector3 direction;
        if (Mode.Value == FrogMode.Climbing)
        {
            // Wall jump: let go first, then push out from the wall and up.
            surfaceNormal = climb.SurfaceNormal;
            climb.Detach();
            direction = (surfaceNormal + Vector3.up * tuning.wallJumpUpBias + aim * 0.5f).normalized;
        }
        else
        {
            surfaceNormal = GroundNormal;
            // On the ground the leap goes the way the frog faces (the stick turns it while charging).
            if (aiming)
                aim = Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, surfaceNormal).normalized;
            direction = (surfaceNormal * (1f - tuning.forwardJumpRatio) + aim * tuning.forwardJumpRatio).normalized;
        }

        // Remove any velocity pushing into the surface, then launch exactly once.
        float into = Mathf.Min(0f, Vector3.Dot(rb.linearVelocity, surfaceNormal));
        rb.AddForce(direction * speed - surfaceNormal * into, ForceMode.VelocityChange);

        Mode.Value = FrogMode.Airborne;
        ignoreGroundUntil = Time.time + 0.15f;
        support = null;
    }

    void Move(FrogInputState input, float dt)
    {
        bool grounded = Mode.Value == FrogMode.Grounded;
        Vector3 up = grounded ? GroundNormal : Vector3.up;
        Vector3 wish = IsCharging.Value ? Vector3.zero : input.WorldMove; // feet planted while charging
        bool hasInput = wish.sqrMagnitude > 0.0001f;

        if (grounded)
        {
            // Bounded acceleration toward the wanted velocity, measured relative to the support
            // (moving platforms) so tongue pulls and collisions are not wiped out instantly.
            Vector3 supportVelocity = support != null ? support.GetPointVelocity(rb.position) : Vector3.zero;
            Vector3 planar = Vector3.ProjectOnPlane(rb.linearVelocity - supportVelocity, up);
            Vector3 target;
            float acceleration = tuning.groundAcceleration;
            if (tuning.hopLocomotion)
            {
                target = HopVelocity(wish, up, dt);
                if (hopping)
                    acceleration = tuning.hopAcceleration;
            }
            else
            {
                target = Vector3.ProjectOnPlane(wish, up).normalized * (wish.magnitude * tuning.moveSpeed);
            }
            Vector3 change = Vector3.ClampMagnitude(target - planar, acceleration * dt);
            rb.AddForce(change, ForceMode.VelocityChange);

            // The body is frictionless, so cancel gravity's slide when standing still on a slope.
            if (target.sqrMagnitude < 0.0001f)
                rb.AddForce(-Vector3.ProjectOnPlane(Physics.gravity, up), ForceMode.Acceleration);
        }
        else if (hasInput)
        {
            // Air control can steer and slow a leap but never pushes past moveSpeed.
            Vector3 dir = wish.normalized;
            float along = Vector3.Dot(rb.linearVelocity, dir);
            float add = Mathf.Clamp(tuning.moveSpeed * wish.magnitude - along, 0f, tuning.airAcceleration * dt);
            rb.AddForce(dir * add, ForceMode.VelocityChange);
        }

        // Which way the body faces is set by FrogTuning > Facing. While aiming a charged
        // jump it always faces the camera, whatever the setting. Hopping always turns
        // toward the stick: a hop can only go the way the frog faces.
        // Which way the body faces is set by FrogTuning > Facing. Hopping always turns
        // toward the stick: a hop can only go the way the frog faces. While charging a
        // jump the frog keeps its facing; holding a direction turns it to aim the leap.
        bool faceCamera = !IsCharging.Value && !tuning.hopLocomotion
            && (tuning.facing == FrogFacing.AlwaysCamera || (tuning.facing == FrogFacing.CameraWhenMoving && hasInput));
        Vector3 faceDirection = IsCharging.Value ? input.WorldMove : faceCamera ? input.ViewForward : wish;
        FaceTowards(faceDirection, up, dt, Vector3.zero);
    }

    // ---- Hop locomotion (host only) ----
    // The body moves forward exactly as far as Jump2 carries the feet at the same moment
    // of the hop, so the animation never slides: root motion worked out on the host and
    // kept in step on every screen through the Hop variable.

    Vector3 HopVelocity(Vector3 wish, Vector3 up, float dt)
    {
        float amount = Mathf.Clamp01(wish.magnitude);
        bool wantsMove = amount > 0.1f;
        Vector3 forward = Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, up);
        if (forward.sqrMagnitude < 0.0001f)
            return Vector3.zero;
        forward.Normalize();

        if (!hopping)
        {
            // Standing: hop once facing roughly the right way, otherwise turn on the spot first.
            if (!wantsMove || Vector3.Angle(forward, Vector3.ProjectOnPlane(wish, up)) > tuning.hopMaxStartAngle)
                return Vector3.zero;
            BeginHop(amount, tuning.hopStartPhase, 0f, chained: false);
        }
        else if (!wantsMove && hopPhase < tuning.hopCancelBefore)
        {
            EndHop(); // let go while still crouching: don't leap
            return Vector3.zero;
        }

        float from = hopPhase;
        float stride = hopStride;
        hopPhase += dt / hopDuration;
        float travel;
        if (hopPhase < 1f)
        {
            travel = HopTravel(hopPhase) - HopTravel(from);
        }
        else
        {
            travel = 1f - HopTravel(from); // finish this hop
            float secondsIntoNext = (hopPhase - 1f) * hopDuration;
            if (wantsMove)
                BeginHop(amount, 0f, secondsIntoNext, chained: true); // Jump2 ends in its own start pose
            else
                EndHop();
        }
        return forward * (travel * stride / dt);
    }

    float HopTravel(float phase) => tuning.hopTravel.Evaluate(Mathf.Clamp01(phase));

    void BeginHop(float amount, float startPhase, float extraSeconds, bool chained)
    {
        float previousDuration = chained ? hopDuration : 0f;
        hopping = true;
        hopDuration = Mathf.Max(0.2f, Mathf.Lerp(tuning.slowHopDuration, tuning.hopDuration, amount));
        hopStride = tuning.hopStride * Mathf.Lerp(tuning.minHopStride, 1f, amount);
        hopPhase = startPhase + extraSeconds / hopDuration;
        Hop.Value = new HopSync
        {
            StartTime = NetworkTime - hopPhase * hopDuration,
            Duration = hopDuration,
            PrevDuration = previousDuration,
            StopTime = double.MaxValue,
        };
    }

    void EndHop()
    {
        if (!hopping)
            return;
        hopping = false;
        HopSync hop = Hop.Value;
        hop.StopTime = NetworkTime;
        Hop.Value = hop;
    }

    // Turns the body with a speed limit (never snaps the physics body).
    public void FaceTowards(Vector3 direction, Vector3 up, float dt, Vector3 fallback)
    {
        Vector3 forward = Vector3.ProjectOnPlane(direction, up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(fallback, up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(rb.rotation * Vector3.up, up); // just left a wall
        if (forward.sqrMagnitude < 0.0001f)
            return;

        Quaternion target = Quaternion.LookRotation(forward, up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, tuning.turnSpeed * dt));
    }

    public void Respawn()
    {
        if (Mode.Value == FrogMode.Climbing)
            climb.Detach();
        EndHop();
        IsCharging.Value = false;
        Mode.Value = FrogMode.Airborne;
        rb.linearVelocity = Vector3.zero;
        rb.position = spawnPosition;
        rb.rotation = Quaternion.identity;
        transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        // Tell the NetworkTransform this move is a teleport, so clients don't watch the frog fly back.
        networkTransform.Teleport();
    }
}