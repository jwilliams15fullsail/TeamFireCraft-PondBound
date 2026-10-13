using Unity.Netcode;
using UnityEngine;

// Host-side sticking and climbing. FrogMotor decides when to call it, so there is
// still only one simulation path per frog.
public class FrogClimb : NetworkBehaviour
{
    [SerializeField] LayerMask surfaceMask = ~0;

    // 0 = relaxed, 1 = at the grip limit, above 1 = slipping. Every machine reads it for visuals.
    public readonly NetworkVariable<float> GripStrain = new();

    public Vector3 SurfaceNormal { get; private set; } = Vector3.up;
    public bool IsAttached => surface != null;

    FrogMotor motor;
    Rigidbody rb;
    ClimbableSurface surface;
    Rigidbody surfaceBody;
    Vector3 externalForce;  // tongue pulls on this frog during the current step
    float overloadTime;
    float cooldownUntil;

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
            if (CastForSurface(dir.normalized, motor.Tuning.stickReach, out RaycastHit hit) && (!found || hit.distance < best.distance))
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

        // Input in the plane of the surface: W = up the wall, D = right along it.
        Vector3 surfaceUp = Vector3.ProjectOnPlane(Vector3.up, n);
        if (surfaceUp.sqrMagnitude < 0.001f)
            surfaceUp = Vector3.ProjectOnPlane(input.ViewForward, n);
        surfaceUp.Normalize();
        Vector3 surfaceRight = Vector3.Cross(n, surfaceUp);
        Vector3 climbDir = charging ? Vector3.zero : Vector3.ClampMagnitude(surfaceUp * input.Move.y + surfaceRight * input.Move.x, 1f);

        // Inside corner: another climbable face directly ahead. Switch onto it.
        if (climbDir.sqrMagnitude > 0.01f
            && CastForSurface(climbDir.normalized, 0.05f, out RaycastHit ahead)
            && Vector3.Angle(ahead.normal, n) <= t.maxSurfaceTurn)
        {
            n = SurfaceNormal = ahead.normal;
        }

        // Find the surface under the belly again (it may be curved or moving).
        if (!CastForSurface(-n, t.stickReach, out RaycastHit hit) || Vector3.Angle(hit.normal, n) > t.maxSurfaceTurn)
        {
            // Climbed past the top edge: pop up and over instead of just dropping.
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

        // Lie flat on the surface, head toward the climb direction (head-up when idle).
        motor.FaceTowards(climbDir, n, dt, surfaceUp);
        return true;
    }

    bool CastForSurface(Vector3 direction, float reach, out RaycastHit hit)
    {
        float castRadius = motor.Radius * 0.8f;
        float distance = (motor.Radius - castRadius) + reach;
        if (!Physics.SphereCast(rb.position, castRadius, direction, out hit, distance, surfaceMask, QueryTriggerInteraction.Ignore))
            return false;
        if (hit.rigidbody == rb || hit.collider.GetComponentInParent<ClimbableSurface>() == null)
            return false;

        // Walls and steep slopes only: floors are for walking, ceilings come later.
        float upDot = Vector3.Dot(hit.normal, Vector3.up);
        return upDot >= motor.Tuning.minWallUpDot && upDot <= motor.Tuning.maxWallUpDot;
    }
}
