using System.Collections.Generic;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// A countdown for a room. When it runs out, the Lasers (a HazardPower with TimedLaser beams
// under it, like the Lasers prefab) switch on and come down from the ceiling. Until then they
// are held off: no beams, no damage. If the players get out in time (Stop When Open's door
// opens, or something calls Stop), the timer stops and the lasers never come.
// The host starts and stops it; the end time is a SyncVar on the shared network clock, so
// every player sees the same countdown and the same lasers, including anyone who joins late.
// TimerUI on the HUD shows it.
// Needs a NetworkObject. For Starts When: Frog Enters, also a trigger collider on this object.
[RequireComponent(typeof(NetworkObject))]
public class RoomTimer : NetworkBehaviour
{
    public enum StartMode { LevelStart, FrogEnters, Manual }

    [Header("Countdown")]
    [Tooltip("Seconds the players get.")]
    [SerializeField, Min(1f)] float duration = 120f;
    [Tooltip("Level Start: as soon as the level begins. Frog Enters: when a frog walks into this object's trigger. Manual: when something calls Begin.")]
    [SerializeField] StartMode startsWhen = StartMode.FrogEnters;
    [Tooltip("Optional: the countdown stops for good once this door opens (the way out).")]
    [SerializeField] DoorController stopWhenOpen;
    [Tooltip("Shown above the time on the HUD.")]
    [SerializeField] string label = "Lasers in";

    [Header("Lasers")]
    [Tooltip("The lasers: a HazardPower with TimedLaser beams under it (the Lasers prefab). Place it at the ceiling.")]
    [SerializeField] HazardPower lasers;
    [Tooltip("Seconds the lasers show at the ceiling, as a warning, before they start coming down.")]
    [SerializeField, Min(0f)] float warningTime = 2f;
    [Tooltip("How far they come down (m). Down to just above the floor.")]
    [SerializeField, Min(0f)] float dropDistance = 4f;
    [Tooltip("Seconds to come all the way down.")]
    [SerializeField, Min(0.1f)] float dropTime = 6f;

    // The shared clock time the countdown ends (-1 = not started). Stopped = got out in time.
    readonly SyncVar<double> endsAt = new(-1);
    readonly SyncVar<bool> stopped = new();

    static readonly List<RoomTimer> all = new();
    Vector3 lasersStart;
    bool lasersHeldOff; // host: this timer is holding the lasers off

    public string Label => label;
    public bool IsRunning => endsAt.Value >= 0 && !stopped.Value && Now < endsAt.Value;
    public bool HasRunOut => endsAt.Value >= 0 && !stopped.Value && Now >= endsAt.Value;
    public bool IsStopped => stopped.Value;
    public float SecondsLeft => endsAt.Value < 0 ? duration : Mathf.Max(0f, (float)(endsAt.Value - Now));

    // The timer to show on the HUD: one that's counting down, or one that has just run out.
    public static RoomTimer Shown
    {
        get
        {
            RoomTimer runOut = null;
            foreach (RoomTimer timer in all)
            {
                if (timer.IsRunning)
                    return timer;
                if (timer.HasRunOut)
                    runOut = timer;
            }
            return runOut;
        }
    }

    double Now
    {
        get
        {
            TimeManager clock = TimeManager;
            return clock != null ? clock.TicksToTime(clock.GetPreciseTick(TickType.Tick)) : Time.timeAsDouble;
        }
    }

    void Awake()
    {
        if (lasers != null)
            lasersStart = lasers.transform.position;
    }

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (startsWhen == StartMode.LevelStart)
            Begin();
    }

    // Host only: start the countdown (once).
    public void Begin()
    {
        if (IsServerInitialized && endsAt.Value < 0 && !stopped.Value)
            endsAt.Value = Now + duration;
    }

    // Host only: the players made it. The lasers never come (or, if already down, stay).
    public void Stop()
    {
        if (IsServerInitialized && !HasRunOut)
            stopped.Value = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (startsWhen != StartMode.FrogEnters || !IsServerInitialized || other.attachedRigidbody == null)
            return;
        if (other.attachedRigidbody.GetComponent<FrogMotor>() != null)
            Begin();
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized)
            return;
        if (stopWhenOpen != null && stopWhenOpen.IsOpen)
            Stop();
        // No beams until the time runs out; then they switch on (unless a switch holds them off too).
        // Checked every step, because the lasers may finish spawning after this timer.
        bool holdOff = !HasRunOut;
        if (lasers != null && lasers.IsServerInitialized && lasersHeldOff != holdOff)
        {
            lasers.HoldOff(this, holdOff);
            lasersHeldOff = holdOff;
        }
    }

    // Every machine moves its own copy of the lasers from the shared clock, so they come down
    // in step for everyone without streaming their position.
    void Update()
    {
        if (lasers == null || !HasRunOut)
            return;
        float t = Mathf.Clamp01(((float)(Now - endsAt.Value) - warningTime) / dropTime);
        lasers.transform.position = lasersStart + Vector3.down * (dropDistance * Mathf.SmoothStep(0f, 1f, t));
    }
}