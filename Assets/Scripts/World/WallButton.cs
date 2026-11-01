using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// A button a frog's tongue presses. FrogTongue calls Activate on the host. Pressed or not is a
// SyncVar, so every player sees the same color, including anyone who joins later. Each machine
// swaps its own copy's material to match. Needs a TongueTarget on this same GameObject.
// It can open a door, switch off a group of hazards, unlock another button, or any mix of those.
public class WallButton : NetworkBehaviour
{
    public DoorController door;
    [Tooltip("Optional: lasers, vents or other hazards to keep off while this button is on.")]
    public HazardPower hazards;
    [Tooltip("Optional: a locked button that this one unlocks for good on its first press.")]
    public WallButton unlocks;
    [Tooltip("Stays on after the first press, instead of a second press switching it back off.")]
    [SerializeField] bool pressOnce;
    [Tooltip("Ignores the tongue until another button unlocks it.")]
    [SerializeField] bool startsLocked;
    [Tooltip("Shown while pressed. Until then the button shows the material on its MeshRenderer.")]
    [SerializeField] Material pressedMaterial;
    [Tooltip("Optional: shown while locked.")]
    [SerializeField] Material lockedMaterial;
    //public float resetDelay; //optional for automatic reset

    readonly SyncVar<bool> isActive = new();
    readonly SyncVar<bool> isUnlocked = new();

    Renderer buttonRenderer;
    Material unpressedMaterial;

    bool IsLocked => startsLocked && !isUnlocked.Value;

    void Awake()
    {
        buttonRenderer = GetComponentInChildren<Renderer>();
        unpressedMaterial = buttonRenderer.sharedMaterial;
    }

    public void Activate()
    {
        if (IsLocked || (pressOnce && isActive.Value))
            return;
        isActive.Value = !isActive.Value;
        if (door != null)
            door.HoldOpen(this, isActive.Value);
        if (hazards != null)
            hazards.HoldOff(this, isActive.Value);
        if (unlocks != null)
            unlocks.isUnlocked.Value = true;
    }
    //void ResetButton()
    //{
    //    isActive.Value = false;
    //    door.SetOpen(false);
    //}

    void Update()
    {
        Material wanted = unpressedMaterial;
        if (isActive.Value && pressedMaterial != null)
            wanted = pressedMaterial;
        else if (IsLocked && lockedMaterial != null)
            wanted = lockedMaterial;
        if (buttonRenderer.sharedMaterial != wanted)
            buttonRenderer.sharedMaterial = wanted;
    }
}