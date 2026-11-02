using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Timing;
using UnityEngine;

// A vent that blows smoke on a timer and hurts every frog inside it during the blast.
// Each cycle: off, then smoke with no damage yet (the warning), then smoke with damage.
// Every machine reads the timing from the shared network clock, so the smoke matches the
// damage with no network messages. The host does the damage; the bars update for everyone
// through FrogStats. Needs a trigger collider on the same GameObject; no NetworkObject.
// A HazardPower on a parent object can switch it off.
public class TimedVent : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [SerializeField] float offTime = 3f;
    [Tooltip("Smoke starts this long before it does damage, so players can see it coming.")]
    [SerializeField] float warningTime = 1f;
    [SerializeField] float blastTime = 2f;
    [Tooltip("Shifts this vent's cycle, so a row of vents can take turns.")]
    [SerializeField] float cycleOffset;

    [Header("Effect")]
    [SerializeField] float damagePerSecond = 25f;
    [SerializeField] ParticleSystem smoke;

    readonly Dictionary<FrogStats, int> inside = new(); // frog -> its colliders inside
    readonly List<FrogStats> gone = new();
    bool smoking;
    HazardPower power; // optional: a parent that can switch this vent off

    static bool IsHost => InstanceFinder.IsServerStarted;
    bool SwitchedOff => power != null && power.IsOff;

    // Seconds into the current cycle, the same on every machine.
    float CycleTime
    {
        get
        {
            TimeManager clock = InstanceFinder.TimeManager;
            double now = clock != null ? clock.TicksToTime(clock.GetPreciseTick(TickType.Tick)) : Time.timeAsDouble;
            return (float)((now + cycleOffset) % (offTime + warningTime + blastTime));
        }
    }

    void Start()
    {
        power = GetComponentInParent<HazardPower>();
        // The smoke only plays when this script says so.
        if (smoke != null)
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        bool shouldSmoke = !SwitchedOff && CycleTime >= offTime;
        if (smoke == null || shouldSmoke == smoking)
            return;
        smoking = shouldSmoke;
        if (smoking)
            smoke.Play(true);
        else
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsHost || !TryGetFrog(other, out FrogStats frog))
            return;
        inside.TryGetValue(frog, out int count);
        inside[frog] = count + 1;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsHost || !TryGetFrog(other, out FrogStats frog) || !inside.TryGetValue(frog, out int count))
            return;
        if (count <= 1)
            inside.Remove(frog);
        else
            inside[frog] = count - 1;
    }

    void FixedUpdate()
    {
        if (SwitchedOff || !IsHost || inside.Count == 0 || CycleTime < offTime + warningTime)
            return;
        float amount = damagePerSecond * Time.fixedDeltaTime;
        foreach (FrogStats frog in inside.Keys)
        {
            if (frog == null)
                gone.Add(frog); // left the game while standing here
            else
                frog.Damage(amount);
        }
        foreach (FrogStats frog in gone)
            inside.Remove(frog);
        gone.Clear();
    }

    static bool TryGetFrog(Collider other, out FrogStats frog)
    {
        frog = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<FrogStats>() : null;
        return frog != null;
    }

    // Shows the damage area in the Scene view so it's easy to place.
    void OnDrawGizmos()
    {
        if (!TryGetComponent(out Collider area))
            return;
        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.3f);
        Gizmos.DrawCube(area.bounds.center, area.bounds.size);
    }
}