using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// Host-side sticking and climbing. FrogMotor decides when to call it, so there is
// still only one simulation path per frog.
public class FrogClimb : NetworkBehaviour
{
    [SerializeField] LayerMask surfaceMask = ~0;

    // 0 = relaxed, 1 = at the grip limit, above 1 = slipping. Every machine reads it for visuals.
    public readonly SyncVar<float> GripStrain = new();

    public Vector3 SurfaceNormal { get; private set; } = Vector3.up;
    public bool IsAttached => surface != null;

    FrogMotor motor;
    Rigidbody rb;
    ClimbableSurface surface;
    Rigidbody surfaceBody;
    Vector3 externalForce;  // tongue pulls on this frog during the current step
    float overloadTime;
    float cooldownUntil;
    float wallUpSign = 1f;  // +1: W climbs up the wall, -1: W climbs down (camera looking over the top)

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        rb = GetComponent<Rigidbody>();
    }

    // Tongues call this whenever they pull on this frog, so grip can resist (or fail).
    public void AddExternalForce(Vector3 force)
    {
        if (IsAttached)
            externalForce += force;
    }

    public bool TryAttach(FrogInputState input)
    {
        if (Time.time < cooldownUntil)
            return false;

        // Probe in front of the frog, where the camera looks, where the stick points, and below.
        Vector3[] directions = { transform.forward, input.ViewForward, input.WorldMove, Vector3.down };
        bool found = false;
        RaycastHit best = default;
        foreach (Vector3 dir in directions)
        {
            if (dir.sqrMagnitude < 0.01f)
                continue;
            if (CastForSurface(dir.normalized, motor.Tuning.stickReach, true, out RaycastHit hit) && (!found || hit.distance < best.distance))
            {
                best = hit;
                found = true;
            }
        }
        if (!found)
            return false;

        surface = best.collider.GetComponentInParent<ClimbableSurface>();
        surfaceBody = best.rigidbody;
        SurfaceNormal = best.normal;
        rb.useGravity = false; // the pads carry our weight now (see Simulate)
        overloadTime = 0f;
        externalForce = Vector3.zero;
        return true;
    }

    public void Detach()
    {
        surface = null;
        surfaceBody = null;
        rb.useGravity = true;
        cooldownUntil = Time.time + motor.Tuning.reattachCooldown;
        externalForce = Vector3.zero;
        GripStrain.Value = 0f;
    }

    // One physics step of clinging. Returns false when the frog lost the surface or its grip.
    public bool Simulate(FrogInputState input, bool charging, float dt)
    {
        FrogTuning t = motor.Tuning;
        Vector3 n = SurfaceNormal;
        Vector3 forward = SurfaceForward(n, input);
        Vector3 climbDir = charging ? Vector3.zero : ClimbDirection(n, forward, input);

        if (climbDir.sqrMagnitude > 0.01f)
        {
            Vector3 dir = climbDir.normalized;

            // Inside corner: another climbable face just ahead (a side wall, a ledge above,
            // a climbable floor below). Switch onto it before bumping into it.
            if (CastForSurface(dir, t.cornerLookAhead, false, out RaycastHit ahead)
                && Vector3.Angle(ahead.normal, n) > 5f
                && Vector3.Angle(ahead.normal, n) <= t.maxCornerTurn)
            {
                n = SurfaceNormal = ahead.normal;
                forward = SurfaceForward(n, input);
                climbDir = ClimbDirection(n, forward, input);
            }
            // Ordinary walkable ground ahead that isn't climbable (the floor at the bottom
            // of a wall): let go and step onto it.
            else if (WalkableGroundAhead(dir, t))
            {
                return false;
            }
        }

        // Find the surface under the belly again. Around an outside edge this hits the
        // edge itself, so the normal rolls over a little each step and the frog wraps around.
        if (!CastForSurface(-n, t.stickReach, false, out RaycastHit hit) || Vector3.Angle(hit.normal, n) > t.maxSurfaceTurn)
        {
            // Climbed past an edge that can't be climbed: pop up and over instead of just dropping.
            if (Vector3.Dot(climbDir, Vector3.up) > 0.3f)
                rb.linearVelocity = Vector3.up * 2.5f - n * 1.5f;
            return false;
        }

        surface = hit.collider.GetComponentInParent<ClimbableSurface>();
        surfaceBody = hit.rigidbody;
        SurfaceNormal = n = Vector3.Slerp(n, hit.normal, 0.5f).normalized; // smooth small bumps

        // Pads: keep a small gap from the surface and move along it, limited by grip strength.
        float gripForce = t.gripStrength * surface.gripMultiplier;
        float centreToSurface = hit.distance + motor.Radius * 0.8f;
        float gapError = centreToSurface - (motor.Radius + 0.02f);
        Vector3 supportVelocity = surfaceBody != null ? surfaceBody.GetPointVelocity(rb.position) : Vector3.zero;
        Vector3 relative = rb.linearVelocity - supportVelocity;
        Vector3 targetVelocity = climbDir * t.climbSpeed - n * Mathf.Clamp(gapError * 10f, -2f, 2f);
        Vector3 wantedAccel = (targetVelocity - relative) / dt;
        Vector3 padAccel = Vector3.ClampMagnitude(wantedAccel - Physics.gravity, gripForce / rb.mass);
        rb.AddForce(Physics.gravity + padAccel, ForceMode.Acceleration);

        // Newton's third law: a loose object we cling to feels the pads pushing on it.
        if (surfaceBody != null && !surfaceBody.isKinematic)
            surfaceBody.AddForceAtPosition(-padAccel * rb.mass, hit.point);

        // Grip load = our weight + any tongue pulls. Overloaded for too long = slip off.
        Vector3 load = Physics.gravity * rb.mass + externalForce;
        externalForce = Vector3.zero;
        float strain = load.magnitude / Mathf.Max(0.01f, gripForce);
        GripStrain.Value = Mathf.Round(strain * 20f) / 20f; // 0.05 steps = fewer network updates
        overloadTime = strain > 1f ? overloadTime + dt : Mathf.Max(0f, overloadTime - dt);
        if (overloadTime > t.gripSlipTolerance)
            return false;

        // Lie flat on the surface, head toward the climb direction (or "forward" when idle).
        motor.FaceTowards(climbDir, n, dt, forward);
        return true;
    }

    // Which way W moves on this surface. On walls W climbs up (or down, when the camera
    // looks at the wall from over its top). On tops and gentle slopes W goes where the
    // camera looks. The two blend as the frog rolls over an edge, so the controls never flip.
    Vector3 SurfaceForward(Vector3 n, FrogInputState input)
    {
        float facing = Vector3.Dot(input.ViewForward, -n);
        if (Mathf.Abs(facing) > 0.2f)
            wallUpSign = facing > 0f ? 1f : -1f; // keep the last choice while side-on to the wall

        Vector3 wallForward = Vector3.ProjectOnPlane(Vector3.up, n) * wallUpSign;
        Vector3 topForward = Vector3.ProjectOnPlane(input.ViewForward, n);
        wallForward = wallForward.sqrMagnitude > 0.0001f ? wallForward.normalized : Vector3.zero;
        topForward = topForward.sqrMagnitude > 0.0001f ? topForward.normalized : Vector3.zero;

        float topness = Mathf.InverseLerp(0.5f, 0.85f, Vector3.Dot(n, Vector3.up));
        Vector3 forward = Vector3.Lerp(wallForward, topForward, topness);
        if (forward.sqrMagnitude < 0.0001f)
            forward = wallForward.sqrMagnitude > 0f ? wallForward : topForward;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(transform.forward, n);
        return forward.normalized;
    }

    static Vector3 ClimbDirection(Vector3 n, Vector3 forward, FrogInputState input)
    {
        Vector3 right = Vector3.Cross(n, forward);
        return Vector3.ClampMagnitude(forward * input.Move.y + right * input.Move.x, 1f);
    }

    bool WalkableGroundAhead(Vector3 dir, FrogTuning t)
    {
        float castRadius = motor.Radius * 0.8f;
        float distance = (motor.Radius - castRadius) + t.cornerLookAhead;
        if (!Physics.SphereCast(rb.position, castRadius, dir, out RaycastHit hit, distance, surfaceMask, QueryTriggerInteraction.Ignore))
            return false;
        if (hit.rigidbody == rb || hit.collider.GetComponentInParent<ClimbableSurface>() != null)
            return false;
        return Vector3.Angle(hit.normal, Vector3.up) <= t.maxSlopeAngle;
    }

    // attaching = true: pressing Stick only grabs walls and steep slopes (not floors).
    // attaching = false: once stuck, any climbable face counts, including flat tops.
    bool CastForSurface(Vector3 direction, float reach, bool attaching, out RaycastHit hit)
    {
        float castRadius = motor.Radius * 0.8f;
        float distance = (motor.Radius - castRadius) + reach;
        if (!Physics.SphereCast(rb.position, castRadius, direction, out hit, distance, surfaceMask, QueryTriggerInteraction.Ignore))
            return false;
        if (hit.rigidbody == rb || hit.collider.GetComponentInParent<ClimbableSurface>() == null)
            return false;

        float upDot = Vector3.Dot(hit.normal, Vector3.up);
        if (upDot < motor.Tuning.minWallUpDot)
            return false; // ceilings come later
        return !attaching || upDot <= motor.Tuning.maxWallUpDot;
    }
}