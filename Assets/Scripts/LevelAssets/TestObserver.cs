using UnityEngine;
using UnityEngine.InputSystem;

public class TestObserver : MonoBehaviour
{
    public static TestObserver instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private InputActionReference respawnKey;
    [SerializeField] private float respawnHeightOffset = 1f;

    [Header("Respawn Restrictions")]
    [SerializeField] private bool isImmobile = false;
    [SerializeField] private float immobilityThreshold = 0.05f;

    private Vector3 initialSpawnPosition;
    private CheckpointManager activeCheckpoint;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Rigidbody playerRigidbody;

    public bool isPlayerDead { get; set; } = false;


    private void Awake()
    {
        //  Single player option    //
        //playerObject = GameObject.FindWithTag("Frog");
        //if (playerObject != null)
        //{
        //    playerRigidbody = playerObject.GetComponent<Rigidbody>();
        //    initialSpawnPosition = playerObject.transform.position;
        //}
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
    }

    // Method the networked Frog script will call when it spawns
    public void RegisterLocalPlayer(GameObject localPlayer, Rigidbody rb, Vector3 spawnPoint)
    {
        playerObject = localPlayer;
        playerRigidbody = rb;
        initialSpawnPosition = spawnPoint;

        Debug.Log($"[Test Observer] Successfully registered local player: {localPlayer.name}");
    }

    // Subscribe to CheckpointManager on creation
    private void OnEnable()
    {
        CheckpointManager.OnCheckpointActivated += HandleCheckpointTouched;
    }

    // Unsubscribe if this object is destroyed
    private void OnDisable()
    {
        CheckpointManager.OnCheckpointActivated -= HandleCheckpointTouched;
    }

    private void Update()
    {
        if (respawnKey.action.WasPressedThisFrame() && CanRespawn())
        {
            RespawnPlayer();
        }
    }

    private void HandleCheckpointTouched(CheckpointManager newCheckpoint)
    {
        if (activeCheckpoint != null && activeCheckpoint != newCheckpoint)
        {
            activeCheckpoint.DeactivateCheckpoint();
            Debug.Log("[Test Observer] Cleared the previous checkpoint.");
        }
        activeCheckpoint = newCheckpoint;
        Debug.Log($"[Test Observer] New Checkpoint set at: {newCheckpoint.transform.position}");
    }

    private bool CanRespawn()
    {
        if (isPlayerDead) return true;

        if (!isImmobile && playerRigidbody != null)
        {
            bool isMoving = playerRigidbody.linearVelocity.magnitude > immobilityThreshold;
            if (isMoving)
            {
                Debug.Log("[Test Observer] Cannot respawn, player is moving.");
                return false;
            }
            return true;
        }
        // Block respawning by default if no restrictions met
        return false;
    }

    private void RespawnPlayer()
    {
        if (playerObject == null) { return; }

        // Prevent weird physics interactions when teleporting player back to checkpoint
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        // Ternary operator checking if we have an activeCheckpoint or not
        Vector3 spawnPosition = activeCheckpoint != null
            ? activeCheckpoint.transform.position + (Vector3.up * respawnHeightOffset)
            : initialSpawnPosition;

        // Teleport player to the checkpoint position
        playerObject.transform.position = spawnPosition;
        // Reset death state after respawn
        isPlayerDead = false;
        Debug.Log($"[Test Observer] Player respawned at position {spawnPosition}");
    }
}
