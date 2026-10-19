using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public enum FrogMode : byte { Grounded, Airborne, Climbing, Disabled }

// Host-side frog physics: ground probe, movement, charged jumps and the movement
// state machine. Clients never run the simulation; they see the result through
// NetworkTransform and the NetworkVariables below.
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class FrogMotor : NetworkBehaviour
{
    [SerializeField] FrogTuning tuning;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] float respawnHeight = -20f;

    // Replicated state. The host writes it, every machine reads it for visuals.
    public readonly NetworkVariable<FrogMode> Mode = new(FrogMode.Airborne);
    public readonly NetworkVariable<bool> IsCharging = new();
    public readonly NetworkVariable<double> ChargeStartTime = new();

    public FrogTuning Tuning => tuning;
    public Rigidbody Body => rb;
    public float Radius => sphere.radius;
    public Vector3 GroundNormal { get; private set; } = Vector3.up;

    Rigidbody rb;
    SphereCollider sphere;
    FrogNetworkDriver driver;
    FrogClimb climb;
    NetworkTransform networkTransform;

    bool jumpHeld, jumpPressQueued, jumpReleaseQueued, stickQueued;
    float lastGroundedTime = -10f;
    float ignoreGroundUntil;
    Rigidbody support;
    Vector3 spawnPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();
        driver = GetComponent<FrogNetworkDriver>();
        climb = GetComponent<FrogClimb>();
        networkTransform = GetComponent<NetworkTransform>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            spawnPosition = transform.position;
    }

    // Jump charge as 0..1. Works on every machine because it uses network time.
    public float ChargeFraction()
    {
        if (!IsCharging.Value)
            return 0f;
        double held = NetworkManager.ServerTime.Time - ChargeStartTime.Value;
        return Mathf.Clamp01((float)held / tuning.fullChargeTime);
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
        if (!IsServer || !IsSpawned)
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
        UpdateJump(input);

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
            LeaveSurface();
        else if (climb.TryAttach(input))
        {
            IsCharging.Value = false;
            Mode.Value = FrogMode.Climbing;
        }
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
            ChargeStartTime.Value = NetworkManager.ServerTime.Time;
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
        IsCharging.Value = false;

        // Holding a direction = leap that way. No direction = straight up.
        Vector3 aim = input.Move.sqrMagnitude > 0.04f ? input.WorldMove.normalized : Vector3.zero;

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
            Vector3 target = Vector3.ProjectOnPlane(wish, up).normalized * (wish.magnitude * tuning.moveSpeed);
            Vector3 change = Vector3.ClampMagnitude(target - planar, tuning.groundAcceleration * dt);
            rb.AddForce(change, ForceMode.VelocityChange);

            // The body is frictionless, so cancel gravity's slide when standing still on a slope.
            if (!hasInput)
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
        // jump it always faces the camera, whatever the setting.
        bool faceCamera = IsCharging.Value
            || tuning.facing == FrogFacing.AlwaysCamera
            || (tuning.facing == FrogFacing.CameraWhenMoving && hasInput);
        FaceTowards(faceCamera ? input.ViewForward : wish, up, dt, Vector3.zero);
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
        IsCharging.Value = false;
        Mode.Value = FrogMode.Airborne;
        rb.linearVelocity = Vector3.zero;
        // Teleport (instead of setting the position) so clients don't watch the frog fly back.
        networkTransform.Teleport(spawnPosition, Quaternion.identity, transform.localScale);
    }
}
