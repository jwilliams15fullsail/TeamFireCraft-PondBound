using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// A checkpoint. When a player's frog touches it, it becomes the team's new spawn point: frogs
// that die or fall off come back here, players who join later appear here, and the game is
// saved here (PlayerSpawner does all three). Only the host decides; every player sees the flag
// turn green. Number the checkpoints along the route (1, 2, 3...). The spawn point only moves
// forward, so a frog going back to an earlier checkpoint doesn't drag everyone back with it.
// Effects: a marker (the loot drop glow) shows where it is until a frog reaches it, then goes
// away for good. The frog that reaches it gets a heal effect, a croak and a healing sound, and
// optionally full bars.
// It can also be turned on without a frog touching it: hook Activate up to a button, lever or
// door's event (host only, like the touch).
// Needs a NetworkObject and a trigger collider on the same GameObject.
[RequireComponent(typeof(NetworkObject))]
public class CheckpointManager : NetworkBehaviour
{
    // Host only: a player's frog touched this checkpoint.
    public static event Action<CheckpointManager> OnCheckpointActivated;

    // Every machine: a frog has just reached this checkpoint (for the HUD popup). The frog may be null.
    public static event Action<CheckpointManager, NetworkObject> Reached;

    [Tooltip("Where this checkpoint comes along the level: 1 for the first, 2 for the next, and so on. Each checkpoint in a level needs its own number.")]
    [SerializeField, Min(1)] int number = 1;
    [Tooltip("Optional: where frogs come back, one per player (or just one). Drag empty GameObjects placed around the checkpoint in here. Empty slots are skipped; none filled in: Respawn Height Offset above this checkpoint.")]
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float respawnHeightOffset = 1f;

    [Tooltip("Optional: what turns green when this checkpoint is reached, such as its water puddle. Empty: the renderer on this object.")]
    [SerializeField] Renderer flagRenderer;

    [Header("Effects")]
    [Tooltip("Shows where the checkpoint is (the vfx_LootDrop instance, a child of this object). Hidden for good once a frog reaches it.")]
    [SerializeField] GameObject marker;
    [Tooltip("Played on the frog that reaches it (the vfx_Heal prefab from the Project window).")]
    [SerializeField] GameObject reachedEffect;
    [Tooltip("Seconds before the reached effect is cleaned up.")]
    [SerializeField, Min(0.1f)] float reachedEffectLifetime = 4f;
    [Tooltip("The frog's happy sound (a croak).")]
    [SerializeField] AudioClip frogSound;
    [Tooltip("The healing sound.")]
    [SerializeField] AudioClip healSound;
    [SerializeField, Range(0f, 1f)] float soundVolume = 0.8f;
    [Tooltip("Refill the health, hunger and hydration of the frog that reaches it.")]
    [SerializeField] bool refillOnReach = true;

    readonly SyncVar<bool> isActivated = new();
    readonly SyncVar<bool> wasReached = new(); // stays true: the marker never comes back

    public int Number => number;

    private Color originalColor;
    FrogMotor toucher; // host: the frog whose touch is being handled right now
    bool activateWhenSpawned; // host: activated (by a loaded save) before this was networked
    AudioSource audioSource;

    private void Awake()
    {
        if (flagRenderer == null)
            flagRenderer = GetComponent<Renderer>();
        if (flagRenderer != null)
        {
            originalColor = flagRenderer.material.color;
        }
        isActivated.OnChange += OnActivatedChanged;
        wasReached.OnChange += (_, reached, _) => ShowMarker(!reached);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 25f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || isActivated.Value || other.attachedRigidbody == null)
            return;
        // Player frogs are the only things with a FrogMotor.
        FrogMotor frog = other.attachedRigidbody.GetComponent<FrogMotor>();
        if (frog == null || frog.IsDead)
            return;
        toucher = frog; // so ActivateCheckpoint knows a frog really got here (not a loaded save)
        OnCheckpointActivated?.Invoke(this);
        toucher = null;
    }

    // Host only: turn this checkpoint on without a frog touching it, e.g. from a button or a
    // lever's event. Same as reaching it: it becomes the team's spawn point if it's further along.
    public void Activate()
    {
        if (!IsServerInitialized || isActivated.Value)
            return;
        OnCheckpointActivated?.Invoke(this);
    }

    // Where a player's frog comes back: its own spot when there are several. Empty slots in
    // Spawn Points are skipped; with none filled in, it's Respawn Height Offset above this checkpoint.
    public Pose SpawnPoint(int player)
    {
        int filled = 0;
        if (spawnPoints != null)
            foreach (Transform each in spawnPoints)
                if (each != null)
                    filled++;
        if (filled > 0)
        {
            int pick = Mathf.Abs(player) % filled;
            foreach (Transform point in spawnPoints)
            {
                if (point == null)
                    continue;
                if (pick-- == 0)
                    return new Pose(point.position, point.rotation);
            }
        }
        return new Pose(transform.position + Vector3.up * respawnHeightOffset, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
    }

    // Host only (PlayerSpawner calls these). Every machine sees the flag change.
    public void ActivateCheckpoint()
    {
        if (!IsServerInitialized)
        {
            // The level has only just loaded (restart or Continue) and this checkpoint isn't on
            // the network yet: values set now wouldn't reach the other players. Done in OnStartServer.
            activateWhenSpawned = true;
            return;
        }
        isActivated.Value = true;
        wasReached.Value = true;
        if (toucher == null)
            return; // continued from a save: no fanfare
        if (refillOnReach && toucher.TryGetComponent(out FrogStats stats))
            stats.RefillAll();
        PlayReached(toucher.NetworkObject);
    }

    public void DeactivateCheckpoint()
    {
        activateWhenSpawned = false;
        if (IsServerInitialized)
            isActivated.Value = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!activateWhenSpawned)
            return;
        activateWhenSpawned = false;
        isActivated.Value = true;
        wasReached.Value = true;
    }

    // Every player sees the heal effect on the frog and hears the sounds.
    [ObserversRpc]
    void PlayReached(NetworkObject frog)
    {
        if (frogSound != null)
            audioSource.PlayOneShot(frogSound, soundVolume);
        if (healSound != null)
            audioSource.PlayOneShot(healSound, soundVolume);
        if (reachedEffect != null)
        {
            Transform on = frog != null ? frog.transform : transform;
            GameObject effect = Instantiate(reachedEffect, on.position, Quaternion.identity, on); // rides along with the frog
            Destroy(effect, reachedEffectLifetime);
        }
        ShowMarker(false);
        Reached?.Invoke(this, frog);
    }

    void ShowMarker(bool show)
    {
        if (marker != null && marker.activeSelf != show)
            marker.SetActive(show);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ShowMarker(!wasReached.Value); // joined late: already reached, so no marker
    }

#if UNITY_EDITOR
    // Scene view: the checkpoint's number floats above it, so the order is easy to check.
    void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up * (respawnHeightOffset + 1f), $"Checkpoint {number}", EditorStyles.boldLabel);
    }
#endif

    private void OnActivatedChanged(bool previous, bool next, bool asServer)
    {
        if (flagRenderer != null)
        {
            flagRenderer.material.color = next ? Color.green : originalColor;
        }
    }
}