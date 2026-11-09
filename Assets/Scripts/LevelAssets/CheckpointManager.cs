using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// A checkpoint. When a player's frog touches it, it becomes the team's new spawn point: frogs
// that die or fall off come back here, players who join later appear here, and the game is
// saved here (PlayerSpawner does all three). Only the host decides; every player sees the flag
// turn green. Number the checkpoints along the route (1, 2, 3...). The spawn point only moves
// forward, so a frog going back to an earlier checkpoint doesn't drag everyone back with it.
// Needs a NetworkObject and a trigger collider on the same GameObject.
[RequireComponent(typeof(NetworkObject))]
public class CheckpointManager : NetworkBehaviour
{
    // Host only: a player's frog touched this checkpoint.
    public static event Action<CheckpointManager> OnCheckpointActivated;

    [Tooltip("Where this checkpoint comes along the level: 1 for the first, 2 for the next, and so on. Each checkpoint in a level needs its own number.")]
    [SerializeField, Min(1)] int number = 1;
    [Tooltip("Optional: where frogs come back, one per player (or just one). Empty: Respawn Height Offset above this checkpoint.")]
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float respawnHeightOffset = 1f;

    readonly SyncVar<bool> isActivated = new();

    public int Number => number;

    private Color originalColor;
    private Renderer flagRenderer;

    private void Awake()
    {
        flagRenderer = GetComponent<Renderer>();
        if (flagRenderer != null)
        {
            originalColor = flagRenderer.material.color;
        }
        isActivated.OnChange += OnActivatedChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || isActivated.Value || other.attachedRigidbody == null)
            return;
        // Player frogs are the only things with a FrogMotor.
        FrogMotor frog = other.attachedRigidbody.GetComponent<FrogMotor>();
        if (frog != null && !frog.IsDead)
            OnCheckpointActivated?.Invoke(this);
    }

    // Where a player's frog comes back: its own spot when there are several.
    public Pose SpawnPoint(int player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Mathf.Abs(player) % spawnPoints.Length];
            return new Pose(point.position, point.rotation);
        }
        return new Pose(transform.position + Vector3.up * respawnHeightOffset, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
    }

    // Host only (PlayerSpawner calls these). Every machine sees the flag change.
    public void ActivateCheckpoint() => isActivated.Value = true;
    public void DeactivateCheckpoint() => isActivated.Value = false;

    private void OnActivatedChanged(bool previous, bool next, bool asServer)
    {
        if (flagRenderer != null)
        {
            flagRenderer.material.color = next ? Color.green : originalColor;
        }
    }
}