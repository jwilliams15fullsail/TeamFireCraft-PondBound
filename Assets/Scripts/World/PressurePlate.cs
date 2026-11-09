using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// Holds its door open while enough mass rests on it: drop or tongue-drag a box onto it.
// Anything with a Rigidbody counts by its mass; frogs only if Frogs Count is ticked.
// Runs on the host, which simulates every box; the door then syncs to everyone.
// Needs a trigger collider that reaches a little above the plate's top surface.
// It can also hold a group of hazards off: fill in Hazards instead of, or as well as, Door.
// Starts Locked: it ignores all weight until a WallButton that has it as Unlocks Plate is on.
// Its color tells every player what it's doing: locked, ready for weight, or pressed.
// Needs a NetworkObject on this GameObject or a parent (a plate riding on a LooseCube uses the cube's).
public class PressurePlate : NetworkBehaviour
{
    public DoorController door;
    [Tooltip("Optional: lasers, vents or other hazards to keep off while the plate is pressed.")]
    public HazardPower hazards;
    [Tooltip("Total mass (Rigidbody Mass) needed to hold the door open.")]

    public float minMass = 2f;
    [Tooltip("Let frogs standing on the plate add their own mass too.")]
    public bool frogsCount;
    [Tooltip("Stays pressed after the first time enough weight is on it, like a wall button with Press Once.")]
    public bool pressOnce;

    [Tooltip("Optional: objective to complete when the plate becomes pressed.")]
    public ObjectiveSequence completesObjective;

    [Header("Lock")]
    [Tooltip("Ignores all weight until a WallButton with this plate as its Unlocks Plate is switched on.")]
    [SerializeField] bool startsLocked;

    [Header("Colors")]
    [Tooltip("Optional: the renderers to tint. Empty: every renderer on this object and its children.")]
    [SerializeField] Renderer[] tinted;
    [SerializeField] Color lockedColor = new Color(0.9f, 0.15f, 0.1f);
    [SerializeField] Color readyColor = new Color(1f, 0.75f, 0.1f);
    [SerializeField] Color pressedColor = new Color(0.2f, 0.9f, 0.25f);

    readonly SyncVar<bool> isPressed = new();
    readonly SyncVar<bool> isUnlocked = new();

    readonly Dictionary<Rigidbody, int> bodies = new(); // body -> its colliders inside
    readonly List<Rigidbody> gone = new();
    MaterialPropertyBlock block;
    int shown = -1; // the color showing: 0 locked, 1 ready, 2 pressed

    // Host only: what's on the plate right now (handy for debugging or a gauge).
    public float MassOnPlate { get; private set; }
    public bool IsLocked => startsLocked && !isUnlocked.Value;
    public bool IsPressed => isPressed.Value;

    void Awake()
    {
        if (tinted == null || tinted.Length == 0)
            tinted = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
    }

    // Host only (WallButton): switch the plate on or off. Locked again, it lets go of its door.
    public void SetUnlocked(bool unlocked)
    {
        if (IsServerInitialized)
            isUnlocked.Value = unlocked;
    }

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
        if (!IsServerInitialized)
            return null;
        Rigidbody body = other.attachedRigidbody;
        if (body == null || (!frogsCount && body.GetComponent<FrogMotor>() != null))
            return null;
        return body;
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized)
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

        // Press Once: after the first press it stays pressed, whatever happens to the weight.
        bool nowPressed = (pressOnce && isPressed.Value) || (!IsLocked && mass >= minMass);
        if (nowPressed == isPressed.Value)
            return;
        isPressed.Value = nowPressed;
        if (door != null)
            door.HoldOpen(this, nowPressed);
        if (hazards != null)
            hazards.HoldOff(this, nowPressed);

        if (nowPressed)
            ObjectiveSequence.Report(this);
    }

    // Every machine tints its own copy from the synced state, so everyone sees the same color.
    void Update()
    {
        int state = IsLocked ? 0 : isPressed.Value ? 2 : 1;
        if (state == shown)
            return;
        shown = state;
        Color color = state == 0 ? lockedColor : state == 1 ? readyColor : pressedColor;
        foreach (Renderer r in tinted)
        {
            if (r == null)
                continue;
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color); // URP Lit
            block.SetColor("_Color", color);     // Built-in / other shaders
            r.SetPropertyBlock(block);
        }
    }
}