using System.Collections.Generic;
using FishNet;
using UnityEngine;

// An area that completes an objective when a frog goes into it, like "Get into the lab room".
// Put it on a GameObject with a trigger collider (Is Trigger ticked) covering the doorway or the
// room, then drag that GameObject into the objective's Completed By (ObjectiveSequence).
// Every Frog: wait until every player's frog is inside at the same time, not just the first one.
// Only the host decides; the objective list updates for everyone. Reaching it before the
// objective comes up still counts: the objective is simply done when its turn comes.
// No NetworkObject needed.
[RequireComponent(typeof(Collider))]
public class ObjectiveArea : MonoBehaviour
{
    [Tooltip("Off: the first frog in completes it. On: every player's frog has to be inside at once.")]
    [SerializeField] bool everyFrog;
    [Tooltip("Optional: also hide this object once the objective is done (say, a glowing floor marker).")]
    [SerializeField] GameObject hideWhenDone;

    readonly Dictionary<FrogMotor, int> inside = new(); // frog -> its colliders in the trigger
    readonly List<FrogMotor> gone = new();
    bool reported;

    void Reset()
    {
        // Added from the Inspector: make the collider a trigger straight away.
        if (TryGetComponent(out Collider area))
            area.isTrigger = true;
    }

    void Awake()
    {
        if (TryGetComponent(out Collider area) && !area.isTrigger)
            Debug.LogWarning($"ObjectiveArea '{name}': tick Is Trigger on its collider, or frogs will bump into it instead of going through.", this);
    }

    static bool IsHost => InstanceFinder.IsServerStarted;

    void OnTriggerEnter(Collider other)
    {
        if (reported || !IsHost || !TryGetFrog(other, out FrogMotor frog))
            return;
        inside.TryGetValue(frog, out int count);
        inside[frog] = count + 1;
        Check();
    }

    void OnTriggerExit(Collider other)
    {
        if (reported || !IsHost || !TryGetFrog(other, out FrogMotor frog) || !inside.TryGetValue(frog, out int count))
            return;
        if (count <= 1)
            inside.Remove(frog);
        else
            inside[frog] = count - 1;
    }

    // Every Frog: a frog that was already inside when another joins counts too.
    void FixedUpdate()
    {
        if (everyFrog && !reported && IsHost && inside.Count > 0)
            Check();
    }

    void Check()
    {
        int here = 0;
        foreach (KeyValuePair<FrogMotor, int> pair in inside)
        {
            if (pair.Key == null)
                gone.Add(pair.Key); // despawned while inside (the player left)
            else if (!pair.Key.IsDead)
                here++;
        }
        foreach (FrogMotor frog in gone)
            inside.Remove(frog);
        gone.Clear();

        int needed = everyFrog ? Mathf.Max(1, InstanceFinder.ServerManager.Clients.Count) : 1;
        if (here < needed)
            return;

        reported = true;
        inside.Clear();
        // Only counts for an objective whose Completed By is this object, so walking through
        // never completes some other objective by accident.
        ObjectiveSequence.Report(this, namedOnly: true);
        if (hideWhenDone != null)
            hideWhenDone.SetActive(false);
    }

    // Player frogs are the only things with a FrogMotor.
    static bool TryGetFrog(Collider other, out FrogMotor frog)
    {
        frog = null;
        return other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out frog) && !frog.IsDead;
    }
}
