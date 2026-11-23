using FishNet;
using FishNet.Managing.Timing;
using UnityEngine;

// The clock the timed hazards (TimedLaser, TimedVent) run on: the shared network clock, so every
// player sees the same cycle. Offline, time spent with the pause menu open is left out, so the
// hazards carry on from exactly where they were when you resume. Online the game never pauses,
// so it is just the network clock. It makes itself; no scene setup needed.
[DefaultExecutionOrder(-1000)]
public class HazardClock : MonoBehaviour
{
    static HazardClock instance;
    static double pausedTotal; // seconds of offline pause so far
    static double lastRaw = -1;

    // Seconds on the hazard clock.
    public static double Now => Raw - pausedTotal;

    static double Raw
    {
        get
        {
            TimeManager clock = InstanceFinder.TimeManager;
            return clock != null ? clock.TicksToTime(clock.GetPreciseTick(TickType.Tick)) : Time.timeAsDouble;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun()
    {
        pausedTotal = 0;
        lastRaw = -1;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (instance != null)
            return;
        var go = new GameObject("Hazard Clock");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<HazardClock>();
    }

    void Update()
    {
        double raw = Raw;
        if (!GameSession.IsOffline)
            pausedTotal = 0; // online: everyone shares the plain network clock
        else if (Time.timeScale == 0f && lastRaw >= 0 && raw > lastRaw)
            pausedTotal += raw - lastRaw;
        lastRaw = raw;
    }
}
