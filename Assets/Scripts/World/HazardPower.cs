using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// Switches off every hazard under this object (TimedLaser, TimedVent) while at least one switch
// holds it off. The host decides: pressure plates and wall buttons call HoldOff there. Off or on
// is a SyncVar, so every player sees the same hazards, including anyone who joins later.
[RequireComponent(typeof(NetworkObject))]
public class HazardPower : NetworkBehaviour
{
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
        isOff.Value = holders.Count > 0;
    }
}