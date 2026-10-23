using System.Collections.Generic;
using FishNet;
using UnityEngine;

// Holds its door open while enough mass rests on it: drop or tongue-drag a box onto it.
// Anything with a Rigidbody counts by its mass; frogs only if Frogs Count is ticked.
// Runs on the host, which simulates every box; the door then syncs to everyone.
// Needs a trigger collider that reaches a little above the plate's top surface.
public class PressurePlate : MonoBehaviour
{
    public DoorController door;
    [Tooltip("Total mass (Rigidbody Mass) needed to hold the door open.")]
    public float minMass = 2f;
    [Tooltip("Let frogs standing on the plate add their own mass too.")]
    public bool frogsCount;

    readonly Dictionary<Rigidbody, int> bodies = new(); // body -> its colliders inside
    readonly List<Rigidbody> gone = new();
    bool pressed;

    // Host only: what's on the plate right now (handy for debugging or a gauge).
    public float MassOnPlate { get; private set; }

    static bool IsHost => InstanceFinder.IsServerStarted;

    void OnTriggerEnter(Collider other)
    {
        Rigidbody body = CountedBody(other);
        if (body == null)
            return;
        bodies.TryGetValue(body, out int count);
        bodies[body] = count + 1;
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody body = CountedBody(other);
        if (body == null || !bodies.TryGetValue(body, out int count))
            return;
        if (count <= 1)
            bodies.Remove(body);
        else
            bodies[body] = count - 1;
    }

    Rigidbody CountedBody(Collider other)
    {
        if (!IsHost)
            return null;
        Rigidbody body = other.attachedRigidbody;
        if (body == null || (!frogsCount && body.GetComponent<FrogMotor>() != null))
            return null;
        return body;
    }

    void FixedUpdate()
    {
        if (!IsHost)
            return;

        float mass = 0f;
        foreach (Rigidbody body in bodies.Keys)
        {
            if (body == null)
                gone.Add(body); // destroyed or despawned while sitting here
            else
                mass += body.mass;
        }
        foreach (Rigidbody body in gone)
            bodies.Remove(body);
        gone.Clear();
        MassOnPlate = mass;

        bool nowPressed = mass >= minMass;
        if (nowPressed == pressed)
            return;
        pressed = nowPressed;
        if (door != null)
            door.SetOpen(pressed);
    }
}