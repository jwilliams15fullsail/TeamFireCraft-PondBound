using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// A door that slides and/or swings between closed and open. The host decides: pressure plates
// and wall buttons call HoldOpen there. Open/closed is a SyncVar, so every player sees the
// same door, including anyone who joins later. Each machine moves its own copy toward that
// state, so no position has to be streamed.
// Sliding: Open Offset. Swinging: Open Rotation turns it round its pivot (world axes, degrees),
// so a hatch or gate on a hinge goes on an empty placed at the hinge, with the model under it.
// Opening completes an objective whose Completed By is this door (a gate that needs two plates).
[RequireComponent(typeof(NetworkObject))]
public class DoorController : NetworkBehaviour
{
    [Tooltip("How far it slides to open (m, world axes). 0, 0, 0 for a door that only swings.")]
    public Vector3 openOffset = new Vector3(0, -3, 0);
    [Tooltip("How far it turns round its pivot to open (degrees, world axes): 0, 90, 0 swings it a quarter turn round Y.")]
    public Vector3 openRotation = Vector3.zero;
    public float openSpeed = 1.5f;
    [Tooltip("How many plates or buttons must be on at the same time to open this door.")]
    public int switchesNeeded = 1;

    readonly SyncVar<bool> isOpen = new();
    readonly HashSet<Object> holders = new(); // host only: the plates and buttons that are on right now

    Vector3 closedPos;
    Vector3 openPos;
    Quaternion closedRot;
    Quaternion openRot;
    Rigidbody body;

    public bool IsOpen => isOpen.Value;

    void Awake()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;
        closedRot = transform.rotation;
        openRot = Quaternion.Euler(openRotation) * closedRot;
        body = GetComponent<Rigidbody>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Joined while the door was already open: start open instead of moving there.
        if (isOpen.Value)
            MoveTo(openPos, openRot, snap: true);
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
        bool wasOpen = isOpen.Value;
        isOpen.Value = holders.Count >= switchesNeeded;
        if (isOpen.Value && !wasOpen)
            ObjectiveSequence.Report(this, namedOnly: true);
    }

    void FixedUpdate()
    {
        bool open = isOpen.Value;
        Vector3 currentPos = body != null ? body.position : transform.position;
        Quaternion currentRot = body != null ? body.rotation : transform.rotation;
        float t = 1f - Mathf.Exp(-openSpeed * Time.fixedDeltaTime); // same feel at any frame rate
        MoveTo(Vector3.Lerp(currentPos, open ? openPos : closedPos, t),
               Quaternion.Slerp(currentRot, open ? openRot : closedRot, t), snap: false);
    }

    void MoveTo(Vector3 position, Quaternion rotation, bool snap)
    {
        if (body == null)
        {
            transform.SetPositionAndRotation(position, rotation);
        }
        else if (snap)
        {
            body.position = position;
            body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            // Kinematic: pushes frogs out of the way properly.
            body.MovePosition(position);
            body.MoveRotation(rotation);
        }
    }
}