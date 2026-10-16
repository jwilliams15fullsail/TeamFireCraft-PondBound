using UnityEngine;

public class TestObserver : MonoBehaviour
{
    private CheckpointManager activeCheckpoint;

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
}
