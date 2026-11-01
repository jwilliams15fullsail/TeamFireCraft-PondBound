using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// A door that slides between closed and open. The host decides: pressure plates and wall
// buttons call HoldOpen there. Open/closed is a SyncVar, so every player sees the
// same door, including anyone who joins later. Each machine slides its own copy toward
// that state, so no position has to be streamed.
[RequireComponent(typeof(NetworkObject))]
public class DoorController : NetworkBehaviour
{
    public Vector3 openOffset = new Vector3(0, -3, 0);
    public float openSpeed = 1.5f;
    [Tooltip("How many plates or buttons must be on at the same time to open this door.")]
    public int switchesNeeded = 1;

    readonly SyncVar<bool> isOpen = new();
    readonly HashSet<Object> holders = new(); // host only: the plates and buttons that are on right now

    Vector3 closedPos;
    Vector3 openPos;
    Rigidbody body;

    public bool IsOpen => isOpen.Value;

    void Awake()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;
        body = GetComponent<Rigidbody>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Joined while the door was already open: start open instead of sliding there.
        if (isOpen.Value)
            MoveTo(openPos, snap: true);
    }

    // Host only. Calls on other machines are ignored, so plates and buttons can't
    // disagree about the door: only the host's copy of them counts.
    public void HoldOpen(Object holder, bool hold)
    {
        if (!IsServerInitialized)
            return;
        if (hold)
            holders.Add(holder);
        else
            holders.Remove(holder);
        isOpen.Value = holders.Count >= switchesNeeded;
    }

    void FixedUpdate()
    {
        Vector3 target = isOpen.Value ? openPos : closedPos;
        Vector3 current = body != null ? body.position : transform.position;
        float t = 1f - Mathf.Exp(-openSpeed * Time.fixedDeltaTime); // same feel at any frame rate
        MoveTo(Vector3.Lerp(current, target, t), snap: false);
    }

    void MoveTo(Vector3 position, bool snap)
    {
        if (body == null)
            transform.position = position;
        else if (snap)
            body.position = transform.position = position;
        else
            body.MovePosition(position); // kinematic: pushes frogs out of the way properly
    }
}