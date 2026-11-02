using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Timing;
using UnityEngine;

// A laser that fires along this object's blue (Z) arrow and stops at the first thing it
// hits, so walls block it. Each cycle: off, then a thin harmless beam (the warning), then
// the full beam, which hurts any frog touching it. It can also sweep from side to side.
// Every machine reads the timing from the shared network clock, so the beam looks the same
// for everyone with no network messages. The host does the damage; the bars update for
// everyone through FrogStats. Draws with the LineRenderer; no collider or NetworkObject.
// A HazardPower on a parent object can switch it off.s
[RequireComponent(typeof(LineRenderer))]
public class TimedLaser : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [Tooltip("Set this and Warning Time to 0 for a beam that stays on.")]
    [SerializeField] float offTime = 2f;
    [Tooltip("A thin, harmless beam shows this long first, so players can see it coming.")]
    [SerializeField] float warningTime = 0.75f;
    [SerializeField] float onTime = 2f;
    [Tooltip("Shifts this laser's timing, so a row of lasers can take turns.")]
    [SerializeField] float cycleOffset;

    [Header("Sweep")]
    [Tooltip("How far the beam swings to each side, in degrees. 0 keeps it still.")]
    [SerializeField] float sweepAngle;
    [Tooltip("Seconds for one swing there and back.")]
    [SerializeField] float sweepTime = 4f;

    [Header("Beam")]
    [SerializeField] float maxLength = 10f;
    [SerializeField] float width = 0.04f;
    [SerializeField] float warningWidth = 0.01f;
    [Tooltip("What stops the beam. Keep Frog unticked so a frog can't shield the one behind it.")]
    [SerializeField] LayerMask blockers = ~(1 << 6);
    [Tooltip("The layer frogs are on.")]
    [SerializeField] LayerMask frogLayer = 1 << 6;

    [Header("Damage")]
    [SerializeField] float damagePerHit = 25f;
    [Tooltip("Seconds before the same frog can be hurt again.")]
    [SerializeField] float hitCooldown = 1f;

    readonly Collider[] touching = new Collider[8];
    readonly Dictionary<FrogStats, float> nextHit = new(); // frog -> when it can be hurt again
    LineRenderer line;
    Quaternion restRotation;
    HazardPower power; // optional: a parent that can switch this laser offs

    // Seconds on the shared clock, the same on every machine.
    double Clock
    {
        get
        {
            TimeManager clock = InstanceFinder.TimeManager;
            double now = clock != null ? clock.TicksToTime(clock.GetPreciseTick(TickType.Tick)) : Time.timeAsDouble;
            return now + cycleOffset;
        }
    }

    float CycleTime => (float)(Clock % (offTime + warningTime + onTime));
    bool SwitchedOff => power != null && power.IsOff;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.enabled = false;
        restRotation = transform.localRotation;
        power = GetComponentInParent<HazardPower>();
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0f)
            return; // paused offline: the network clock keeps counting, but the beam should freeze
         if (SwitchedOff)
                  {
                       line.enabled = false;
                     return;
               }
        Aim();
        float cycle = CycleTime;
        line.enabled = cycle >= offTime;
        if (!line.enabled)
            return;
        line.widthMultiplier = cycle >= offTime + warningTime ? width : warningWidth;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, BeamEnd(transform.forward));
    }

    void FixedUpdate()
    {
        if (SwitchedOff || !InstanceFinder.IsServerStarted || CycleTime < offTime + warningTime)
            return;
        Aim();
        int count = Physics.OverlapCapsuleNonAlloc(transform.position, BeamEnd(transform.forward), width * 0.5f, touching, frogLayer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (!TryGetFrog(touching[i], out FrogStats frog))
                continue;
            nextHit.TryGetValue(frog, out float next);
            if (Time.fixedTime < next)
                continue;
            nextHit[frog] = Time.fixedTime + hitCooldown;
            frog.Damage(damagePerHit);
        }
    }

    // Swings the laser to where the clock says it should be.
    void Aim()
    {
        if (sweepAngle == 0f)
            return;
        float swing = Mathf.Sin((float)(Clock / sweepTime % 1.0) * 2f * Mathf.PI);
        transform.localRotation = restRotation * Quaternion.Euler(0f, sweepAngle * swing, 0f);
    }

    Vector3 BeamEnd(Vector3 direction)
    {
        Vector3 start = transform.position;
        return Physics.Raycast(start, direction, out RaycastHit hit, maxLength, blockers, QueryTriggerInteraction.Ignore)
            ? hit.point
            : start + direction * maxLength;
    }

    static bool TryGetFrog(Collider other, out FrogStats frog)
    {
        frog = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<FrogStats>() : null;
        return frog != null;
    }

    // Shows the beam and how far it swings in the Scene view so it's easy to place.
    void OnDrawGizmos()
    {
        Vector3 start = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, BeamEnd(transform.forward));
        if (sweepAngle == 0f)
            return;
        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.4f);
        Gizmos.DrawLine(start, BeamEnd(Quaternion.AngleAxis(-sweepAngle, transform.up) * transform.forward));
        Gizmos.DrawLine(start, BeamEnd(Quaternion.AngleAxis(sweepAngle, transform.up) * transform.forward));
    }
}