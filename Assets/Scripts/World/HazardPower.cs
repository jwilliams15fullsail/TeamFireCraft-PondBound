using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// Switches off every hazard under this object (TimedLaser, TimedVent) while at least one switch
// holds it off. The host decides: pressure plates and wall buttons call HoldOff there. Off or on
// is a SyncVar, so every player sees the same hazards, including anyone who joins later.
// Switching off completes an objective whose Completed By is this object (both gas valves turned).
[RequireComponent(typeof(NetworkObject))]
public class HazardPower : NetworkBehaviour
{
    [Tooltip("How many switches must be on at the same time to turn these hazards off (2 = both valves).")]
    public int switchesNeeded = 1;

    readonly SyncVar<bool> isOff = new();
    readonly HashSet<Object> holders = new(); // host only: the switches holding the hazards off

    public bool IsOff => isOff.Value;

    // Host only. Calls on other machines are ignored, so switches can't disagree about
    // the hazards: only the host's copy of them counts.
    public void HoldOff(Object holder, bool hold)
    {
        if (!IsServerInitialized)
            return;
        if (hold)
            holders.Add(holder);
        else
            holders.Remove(holder);
        bool wasOff = isOff.Value;
        isOff.Value = holders.Count >= Mathf.Max(1, switchesNeeded);
        if (isOff.Value && !wasOff)
            ObjectiveSequence.Report(this, namedOnly: true);
    }
}