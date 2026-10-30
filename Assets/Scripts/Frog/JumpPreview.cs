using System.Collections.Generic;
using UnityEngine;

// Your own frog only: while you charge a jump, on the ground or on a wall, draws the arc
// the frog will fly and puts a marker where it comes down. It flies the exact launch that
// gets sent to the host (FrogNetworkDriver.LocalJumpVelocity), stepping it the way the
// physics engine does with a ball the frog's size, so the frog lands on the marker.
// Put it on the Frog prefab with a LineRenderer (and optionally a flat marker) as children.
[RequireComponent(typeof(FrogMotor))]
public class JumpPreview : MonoBehaviour
{
    [SerializeField] LineRenderer arc;
    [Tooltip("Optional: a flat marker (a thin disc or quad with no collider) placed where the jump lands.")]
    [SerializeField] Transform landingMarker;
    [SerializeField] LayerMask hitMask = ~0;
    [Tooltip("Seconds of flight the arc follows before giving up (long falls).")]
    [SerializeField] float maxFlightTime = 3f;

    // Where the jump being aimed will touch down (valid while HasLanding).
    public bool HasLanding { get; private set; }
    public Vector3 LandingPoint { get; private set; }

    FrogMotor motor;
    FrogNetworkDriver driver;
    readonly List<Vector3> points = new();
    readonly RaycastHit[] hits = new RaycastHit[8];

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        driver = GetComponent<FrogNetworkDriver>();
        Show(false);
    }

    void LateUpdate()
    {
        FrogMode mode = motor.IsSpawned ? motor.Mode.Value : FrogMode.Disabled;
        bool supported = mode == FrogMode.Grounded || mode == FrogMode.Climbing;
        if (!driver.IsOwner || !supported || !driver.LocalJumpHeld)
        {
            HasLanding = false;
            Show(false);
            return;
        }

        // Step the flight the way the physics engine does (gravity first, then move, once per
        // physics step), sweeping a ball the frog's size, until it touches something.
        Vector3 velocity = driver.LocalJumpVelocity;
        float dt = Time.fixedDeltaTime;
        float radius = motor.Radius * 0.95f;
        points.Clear();
        Vector3 position = transform.position;
        points.Add(position);
        HasLanding = false;
        RaycastHit landing = default;
        for (float t = 0f; t < maxFlightTime && !HasLanding; t += dt)
        {
            velocity += Physics.gravity * dt;
            Vector3 next = position + velocity * dt;
            HasLanding = Sweep(position, next, radius, out landing);
            position = HasLanding ? position + (next - position).normalized * landing.distance : next;
            points.Add(position);
        }
        if (HasLanding)
            LandingPoint = landing.point;

        Show(true);
        if (arc != null)
        {
            arc.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                arc.SetPosition(i, points[i]);
        }
        // The marker: the spot you placed with the mouse, which stays put while the charge
        // swings (the arc shows whether it reaches). Without one, where the arc comes down.
        if (landingMarker != null)
        {
            bool placed = driver.HasMarker;
            bool show = placed || HasLanding;
            landingMarker.gameObject.SetActive(show);
            if (show)
            {
                Vector3 point = placed ? driver.MarkerPoint : landing.point;
                Vector3 normal = placed ? driver.MarkerNormal : landing.normal;
                landingMarker.SetPositionAndRotation(point + normal * 0.01f, Quaternion.FromToRotation(Vector3.up, normal));
            }
        }
    }

    // First thing a frog-sized ball moving between two points of the arc would touch,
    // ignoring this frog's own collider.
    bool Sweep(Vector3 from, Vector3 to, float radius, out RaycastHit best)
    {
        Vector3 delta = to - from;
        float length = delta.magnitude;
        best = default;
        if (length < 0.0001f)
            return false;
        int count = Physics.SphereCastNonAlloc(from, radius, delta / length, hits, length, hitMask, QueryTriggerInteraction.Ignore);
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].rigidbody == motor.Body)
                continue;
            if (!found || hits[i].distance < best.distance)
            {
                best = hits[i];
                found = true;
            }
        }
        return found;
    }

    void Show(bool show)
    {
        if (arc != null && arc.enabled != show)
            arc.enabled = show;
        if (!show && landingMarker != null && landingMarker.gameObject.activeSelf)
            landingMarker.gameObject.SetActive(false);
    }
}