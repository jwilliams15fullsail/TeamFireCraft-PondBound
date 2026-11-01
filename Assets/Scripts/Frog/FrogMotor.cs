using System;
using FishNet.Component.Transforming;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public enum FrogMode : byte { Grounded, Airborne, Climbing, Disabled }

// The shared hop clock for hop mode. The host starts and chains hops and writes
// this once per hop; every machine works out the same jump-clip frame from it, so the
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
    // A dead frog is in Disabled mode: the host stops simulating it until Respawn.
    public bool IsDead => Mode.Value == FrogMode.Disabled;
    // The host's clock (seconds), the same on every machine: FishNet's tick plus how far
    // into the next tick we are, so it moves smoothly every frame.
    public double NetworkTime => TimeManager.TicksToTime(TimeManager.GetPreciseTick(TickType.Tick));

    Rigidbody rb;
    SphereCollider sphere;
    FrogNetworkDriver driver;
    FrogClimb climb;
    FrogStats stats;
    NetworkTransform networkTransform;
    FrogTongue tongue;

    bool jumpHeld, jumpPressQueued, jumpReleaseQueued, stickQueued;
    float lastGroundedTime = -10f;
    float ignoreGroundUntil;
    Rigidbody support;
    Vector3 spawnPosition;
    bool autoClimbBlocked;  // let go of a wall with Stick: no grabbing in the air until we land
    float airSpeedLimit;    // sideways speed at takeoff: air steering never goes past it
    Vector3 releaseVelocity; // the launch the owner worked out when letting go of jump
    bool followingArc;      // a charged jump in flight: no air steering, so it follows its preview

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
        tongue = GetComponent<FrogTongue>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnPosition = transform.position;
    }
    // Charge (0..1) after holding jump this many seconds. With Charge Ping Pong it swings up
    // to full and back down again while held, so the player picks the moment to let go.
    public float ChargeFromSeconds(float held)
    {
        float t = held / tuning.fullChargeTime;
        return tuning.chargePingPong ? Mathf.PingPong(t, 1f) : Mathf.Clamp01(t);
    }

    // This frog's charge as the host has it (0..1). Works on every machine because it uses
    // network time; other players' frogs crouch with it. The owner shows its own (LocalCharge).
    public float ChargeFraction()
    {
        if (!IsCharging.Value)
            return 0f;
        return ChargeFromSeconds((float)(NetworkTime - ChargeStartTime.Value));
    }

    // Height and forward distance (m, landing on flat ground) of a ground jump with this
    // much charge (0..1). heightScale shrinks both for a hungry frog (1 = normal).
    public Vector2 GroundJumpReach(float charge01, float heightScale)
    {
        float charge = tuning.chargeCurve.Evaluate(charge01);
        return new Vector2(
            Mathf.Lerp(tuning.minJumpHeight, tuning.maxJumpHeight, charge),
            Mathf.Lerp(tuning.minJumpDistance, tuning.maxJumpDistance, charge)) * heightScale;
    }

    // How high (m, above the frog) a jump toward target peaks: the charge's height, but always
    // at least Jump Clearance above the target, so it comes down on it. Only a target higher
    // than any jump can reach is out of reach.
    public float JumpApex(float charge01, Vector3 target, float heightScale)
    {
        float charged = GroundJumpReach(charge01, heightScale).x;
        float highest = tuning.maxJumpHeight * heightScale;
        return Mathf.Min(Mathf.Max(charged, target.y + tuning.jumpClearance), highest);
    }

    // The launch velocity of a jump with this much charge (0..1) toward target: an offset
    // from the frog to where its centre should come down, at any height (zero = straight
    // up). It always comes down exactly on the target; the charge only sets how high the
    // arc goes (JumpApex). It counts time the way the physics engine does, so the frog
    // really lands there. wallNormal: jumping off a wall (zero on the ground), which always
    // pushes away from it.
    public Vector3 JumpVelocity(float charge01, Vector3 target, float heightScale, Vector3 wallNormal)
    {
        float gravity = -Physics.gravity.y;
        float up = Mathf.Sqrt(2f * gravity * Mathf.Max(0.01f, JumpApex(charge01, target, heightScale)));
        // Physics adds gravity before moving each step, so a body flies as if launched half
        // a step's worth of gravity slower than the textbook arc.
        float stepUp = up - 0.5f * gravity * Time.fixedDeltaTime;
        Vector3 velocity = Vector3.up * up;

        Vector3 flat = new Vector3(target.x, 0f, target.z);
        float distance = flat.magnitude;
        if (distance > 0.05f)
        {
            float under = stepUp * stepUp - 2f * gravity * target.y; // below 0: higher than the peak
            float time = under > 0f ? (stepUp + Mathf.Sqrt(under)) / gravity : stepUp / gravity;
            velocity += flat / distance * (distance / time);
        }

        if (wallNormal.sqrMagnitude > 0.5f)
        {
            float away = Vector3.Dot(velocity, wallNormal);
            if (away < tuning.wallJumpPush)
                velocity += wallNormal * (tuning.wallJumpPush - away);
        }
        return velocity;
    }

    // The fastest launch any jump can have: the furthest jump on the lowest arc, or the
    // highest one, plus the wall push and slack for a hungry frog's lower arcs. The host
    // never launches faster than this.
    float MaxJumpSpeed()
    {
        float gravity = -Physics.gravity.y;
        float low = Mathf.Sqrt(2f * gravity * Mathf.Max(0.01f, tuning.minJumpHeight));
        float high = Mathf.Sqrt(2f * gravity * tuning.maxJumpHeight);
        float across = tuning.maxJumpDistance * gravity / (2f * low);
        return (Mathf.Max(high, new Vector2(low, across).magnitude) + tuning.wallJumpPush) * 1.5f;
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

    // target: where the player put the landing marker (world x/z offset from the frog, metres).
    // velocity: the launch the owner worked out from what its preview showed (its charge,
    // crosshair and wall), so the frog flies exactly the arc that player saw.
    public void ReleaseJump(Vector3 velocity)
    {
        jumpHeld = false;
        jumpReleaseQueued = true;
        releaseVelocity = Vector3.ClampMagnitude(velocity, MaxJumpSpeed());
    }
    public void PressStick() => stickQueued = true;

    // ---- Simulation (host only) ----

    void FixedUpdate()
    {
        if (!IsServerInitialized || !IsSpawned)
            return;

        float dt = Time.fixedDeltaTime;
        FrogInputState input = driver.CurrentInput;

        if (IsDead)
                 {
                     SettleBody(dt);
                    return;
                }
              if (rb.position.y < respawnHeight)
                   {
                       Respawn();
                       return;
                 }

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
        followingArc = false;
    }

    void LeaveSurface()
    {
        climb.Detach();
        IsCharging.Value = false;
        Mode.Value = FrogMode.Airborne;
        airSpeedLimit = FlatSpeed();
        followingArc = false;
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
            followingArc = false;
            StickLanding();
        }
        else if (Mode.Value == FrogMode.Grounded && Time.time - lastGroundedTime > tuning.coyoteTime)
        {
            // Hysteresis: only become airborne after the ground has been missing for a moment.
            Mode.Value = FrogMode.Airborne;
            IsCharging.Value = false;
            GroundNormal = Vector3.up;
            support = null;
            airSpeedLimit = FlatSpeed(); // walked off a ledge: steering can't speed the fall up
        }
    }

    // Landing drops most of the jump's sideways speed (Landing Keep Speed), so the frog
    // stays on the platform it lands on instead of sliding off it. Running or hopping
    // picks the speed up again from there.
    void StickLanding()
    {
        Vector3 supportVelocity = support != null ? support.GetPointVelocity(rb.position) : Vector3.zero;
        Vector3 relative = rb.linearVelocity - supportVelocity;
        Vector3 vertical = Vector3.Project(relative, Vector3.up);
        rb.linearVelocity = supportVelocity + vertical + (relative - vertical) * tuning.landingKeepSpeed;
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

        // On a ledge the cast touches the corner, and the corner's normal leans out over the
        // drop, so the frog would slide off. Look straight down just inside the edge for the
        // flat top instead, and stand on it while the centre isn't too far past the edge.
        Vector3 toContact = Vector3.ProjectOnPlane(hit.point - rb.position, Vector3.up);
        if (toContact.sqrMagnitude > 0.0001f
            && Physics.Raycast(hit.point + toContact.normalized * 0.02f + Vector3.up * 0.05f, Vector3.down,
                               out RaycastHit top, 0.1f, groundMask, QueryTriggerInteraction.Ignore)
            && top.rigidbody != rb
            && Vector3.Angle(top.normal, hit.normal) > 5f)
        {
            if (toContact.magnitude > Radius * tuning.ledgeSupport)
                return false; // hanging too far over: fall
            hit.normal = top.normal;
        }
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
                Launch();
        }
    }

    void Launch()
    {
        IsCharging.Value = false;
        if (Mode.Value == FrogMode.Climbing)
        {
            climb.Detach(); // let go of the wall first
            rb.linearVelocity = releaseVelocity;
        }
        else
        {
            // Replaces any run speed, so the jump lands exactly where the preview showed.
            Vector3 supportVelocity = support != null ? support.GetPointVelocity(rb.position) : Vector3.zero;
            rb.linearVelocity = supportVelocity + releaseVelocity;
        }

        airSpeedLimit = FlatSpeed();
        followingArc = tuning.followJumpArc;
        Mode.Value = FrogMode.Airborne;
        ignoreGroundUntil = Time.time + 0.15f;
        support = null;
    }

    float FlatSpeed() => Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up).magnitude;

    // Hop mode needs both the tuning switch and the player's Sprint toggle.
    bool HopMode(FrogInputState input) => tuning.hopLocomotion && input.Hop;

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
            if (HopMode(input))
            {
                target = HopVelocity(wish, up, dt);
            }
            else
            {
                EndHop(); // switched back to running mid-hop
                target = RunVelocity(wish, up);
            }
            // Speeding up uses Ground Acceleration, slowing down or stopping uses Ground Braking.
            float acceleration = hopping ? tuning.hopAcceleration
                : target.sqrMagnitude > planar.sqrMagnitude ? tuning.groundAcceleration : tuning.groundBraking;
            Vector3 change = Vector3.ClampMagnitude(target - planar, acceleration * dt);
            rb.AddForce(change, ForceMode.VelocityChange);

            // The body is frictionless, so cancel gravity's slide when standing still on a slope.
            if (target.sqrMagnitude < 0.0001f)
                rb.AddForce(-Vector3.ProjectOnPlane(Physics.gravity, up), ForceMode.Acceleration);
        }
        else if (hasInput && !followingArc)
        {
            // Air steering can turn and slow a leap but never stretch it past its takeoff
            // speed (a straight-up jump or a fall may drift a little), so jumps land where
            // the preview shows.
            Vector3 planar = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
            Vector3 target = wish * Mathf.Max(airSpeedLimit, tuning.airDriftSpeed);
            Vector3 change = Vector3.ClampMagnitude(target - planar, tuning.airAcceleration * dt);
            rb.AddForce(change, ForceMode.VelocityChange);
        }

        // The frog turns toward the stick (on the spot when it points far away). While
        // charging a jump it turns to face the landing marker (or, with mouse aiming off,
        // the stick); with neither it keeps its facing.
        Vector3 face = wish;
        if (IsCharging.Value)
            face = tuning.aimJumpsWithMouse ? new Vector3(input.JumpTarget.x, 0f, input.JumpTarget.y) : input.WorldMove;
        else if (followingArc)
            face = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up); // look where it's flying
        FaceTowards(face, up, dt, Vector3.zero);
    }

    // Running: the frog only ever moves the way it faces, so it never skates sideways.
    // Pushing further than Turn In Place Angle from its facing turns it on the spot
    // first; speed builds up as it comes round. Speed follows how far the stick is pushed.
    Vector3 RunVelocity(Vector3 wish, Vector3 up)
    {
        Vector3 forward = Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, up);
        Vector3 wishFlat = Vector3.ProjectOnPlane(wish, up);
        if (forward.sqrMagnitude < 0.0001f || wishFlat.sqrMagnitude < 0.0001f)
            return Vector3.zero;
        float facing = Mathf.InverseLerp(tuning.turnInPlaceAngle, 0f, Vector3.Angle(forward, wishFlat));
        return forward.normalized * (tuning.moveSpeed * Mathf.Clamp01(wish.magnitude) * facing);
    }

    // ---- Hop mode (host only) ----
    // The body moves forward exactly as far as the hop clip carries the feet at the same
    // moment of the hop, so the animation never slides: root motion worked out on the
    // host and kept in step on every screen through the Hop variable.

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
                BeginHop(amount, 0f, secondsIntoNext, chained: true); // both hop clips end in their start pose
            else
                EndHop();
        }
        return forward * (travel * stride / dt);
    }

    float HopTravel(float phase) => tuning.HopTravelCurve.Evaluate(Mathf.Clamp01(phase));

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
      // Host only. Health ran out: stop simulating this frog and let go of everything. The body
   // drops where it is and stays there until Respawn. Every machine sees the mode change and
   // plays the death animation.
   public void Die()
   {
       if (Mode.Value == FrogMode.Climbing)
           climb.Detach();
       EndHop();
       IsCharging.Value = false;
       followingArc = false;
       Mode.Value = FrogMode.Disabled;
       tongue.LetGo();
   }

    // Dead: no steering. The body falls and levels out, and once it's on something it brakes
   // to a stop there (it's frictionless, so otherwise it would slide away).
  void SettleBody(float dt)
   {
            bool onGround = ProbeGround(out RaycastHit hit);
            FaceTowards(Vector3.zero, onGround ? hit.normal : Vector3.up, dt, Vector3.zero);
           if (!onGround)
                 return;
          Vector3 supportVelocity = hit.rigidbody != null ? hit.rigidbody.GetPointVelocity(rb.position) : Vector3.zero;
           Vector3 planar = Vector3.ProjectOnPlane(rb.linearVelocity - supportVelocity, hit.normal);
            rb.AddForce(Vector3.ClampMagnitude(-planar, tuning.groundBraking * dt), ForceMode.VelocityChange);
           rb.AddForce(-Vector3.ProjectOnPlane(Physics.gravity, hit.normal), ForceMode.Acceleration);
       }

    public void Respawn()
    {
        if (Mode.Value == FrogMode.Climbing)
            climb.Detach();
        EndHop();
        IsCharging.Value = false;
        Mode.Value = FrogMode.Airborne;
        airSpeedLimit = 0f;
        followingArc = false;
        jumpPressQueued = jumpReleaseQueued = stickQueued = false; // anything pressed while dead
        rb.linearVelocity = Vector3.zero;
        rb.position = spawnPosition;
        rb.rotation = Quaternion.identity;
        transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        // Tell the NetworkTransform this move is a teleport, so clients don't watch the frog fly back.
        networkTransform.Teleport();
    }
}