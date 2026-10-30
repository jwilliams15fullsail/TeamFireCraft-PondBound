using System.Collections.Generic;
using UnityEngine;

// Your own frog only: while you charge a jump on the ground, draws the arc the frog will
// fly and puts a marker where it will land. It uses the exact launch FrogMotor uses, and
// air steering can't stretch a jump, so the frog lands on the marker unless you steer.
// Put it on the Frog prefab with a LineRenderer (and optionally a flat marker) as children.
[RequireComponent(typeof(FrogMotor))]
public class JumpPreview : MonoBehaviour
{
    [SerializeField] LineRenderer arc;
    [Tooltip("Optional: a flat marker (a thin disc or quad with no collider) placed where the jump lands.")]
    [SerializeField] Transform landingMarker;
    [SerializeField] LayerMask hitMask = ~0;
    [Tooltip("Seconds of flight the arc follows before giving up (long falls).")]
    [SerializeField] float maxFlightTime = 2f;
    [SerializeField] float timeStep = 0.02f;

    FrogMotor motor;
    FrogNetworkDriver driver;
    FrogStats stats;
    readonly List<Vector3> points = new();
    readonly RaycastHit[] hits = new RaycastHit[8];

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        driver = GetComponent<FrogNetworkDriver>();
        stats = GetComponent<FrogStats>();
        Show(false);
    }

    void LateUpdate()
    {
        bool grounded = motor.IsSpawned && motor.Mode.Value == FrogMode.Grounded;
        if (!driver.IsOwner || !grounded || !(motor.IsCharging.Value || driver.LocalJumpHeld))
        {
            Show(false);
            return;
        }

        float charge01 = Mathf.Max(motor.ChargeFraction(), Mathf.Clamp01(driver.LocalJumpHeldTime / motor.Tuning.fullChargeTime));
        float heightScale = stats != null ? stats.JumpHeightMultiplier : 1f;
        Vector3 aim = motor.AimToward(driver.LocalJumpTarget, charge01, heightScale);
        Vector3 velocity = motor.GroundJumpVelocity(charge01, aim, heightScale);

        // Step along the flight under gravity until the arc hits something.
        points.Clear();
        Vector3 position = transform.position;
        points.Add(position);
        bool landed = false;
        RaycastHit landing = default;
        for (float t = 0f; t < maxFlightTime && !landed; t += timeStep)
        {
            Vector3 next = position + velocity * timeStep + 0.5f * Physics.gravity * (timeStep * timeStep);
            velocity += Physics.gravity * timeStep;
            landed = Cast(position, next, out landing);
            position = landed ? landing.point : next;
            points.Add(position);
        }

        Show(true);
        if (arc != null)
        {
            arc.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                arc.SetPosition(i, points[i]);
        }
        if (landingMarker != null)
        {
            landingMarker.gameObject.SetActive(landed);
            if (landed)
                landingMarker.SetPositionAndRotation(landing.point + landing.normal * 0.01f,
                    Quaternion.FromToRotation(Vector3.up, landing.normal));
        }
    }

    // Nearest hit between two points of the arc, ignoring this frog's own collider.
    bool Cast(Vector3 from, Vector3 to, out RaycastHit best)
    {
        Vector3 delta = to - from;
        float length = delta.magnitude;
        best = default;
        if (length < 0.0001f)
            return false;
        int count = Physics.RaycastNonAlloc(from, delta / length, hits, length, hitMask, QueryTriggerInteraction.Ignore);
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