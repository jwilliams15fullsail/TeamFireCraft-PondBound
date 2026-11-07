using UnityEngine;

public class TestObserver : MonoBehaviour
{
    public static TestObserver instance { get; private set; }


    public Vector3 initialSpawnPosition;
    private CheckpointManager activeCheckpoint;
    [SerializeField] private GameObject playerObject;
    [SerializeField] public Rigidbody playerRigidbody;

    private FrogMotor frogMotor;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
    }

    // Method the networked Frog script will call when it spawns
    public void RegisterLocalPlayer(GameObject localPlayer, Rigidbody rb, Vector3 spawnPoint)
    {
        playerObject = localPlayer;
        playerRigidbody = rb;
        initialSpawnPosition = spawnPoint;

        if (localPlayer != null)
        {
            frogMotor = localPlayer.GetComponent<FrogMotor>();
        }

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

    private void HandleCheckpointTouched(CheckpointManager newCheckpoint, GameObject interactingPlayer)
    {
        // CRITICAL MULTIPLAYER FILTER: Only process this event if it was triggered by our local player
        if (interactingPlayer != playerObject || frogMotor == null) return;

        if (activeCheckpoint != null && activeCheckpoint != newCheckpoint)
        {
            // Tell the server to deactivate visuals specifically for our local player
            activeCheckpoint.DeactivateForPlayer(playerObject);
            Debug.Log("[Test Observer] Cleared the previous checkpoint.");
        }
        activeCheckpoint = newCheckpoint;
        frogMotor.spawnPosition = newCheckpoint.transform.position;
        Debug.Log($"[Test Observer] New Checkpoint set at: {newCheckpoint.transform.position}");
    }
}
