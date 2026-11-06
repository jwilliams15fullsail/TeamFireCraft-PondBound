using UnityEngine;
using System;
using FishNet.Object;

public class CheckpointManager : NetworkBehaviour
{
    // A static event that passes the specific checkpoint that was just activated
    public static event Action<CheckpointManager> OnCheckpointActivated;

    private bool isActivated = false;
    private Color originalColor;
    private Renderer flagRenderer;

    private void Awake()
    {
        flagRenderer = GetComponent<Renderer>();
        if (flagRenderer != null)
        {
            originalColor = flagRenderer.material.color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only run checkpoint logic on the Server/Host to prevent cheating/desync
        if (!IsServerStarted) return;

        if (other.CompareTag("Frog") && !isActivated)
        {
            isActivated = true;

            // Change color on the server and tell all clients to do the same
            ObserversSetCheckpointState(true);

            // Fire the event. The ?. ensures it only runs if something is listening
            OnCheckpointActivated?.Invoke(this);
        }
    }

    public void DeactivateCheckpoint()
    {
        // Ensure only the server can dictate deactivation state
        if (!IsServerStarted) return;

        isActivated = false;
        if (flagRenderer != null)
        {
            flagRenderer.material.color = originalColor;
        }
    }

    // This RPC ensures the visual color changes happen perfectly across all players screens
    [ObserversRpc]
    private void ObserversSetCheckpointState(bool activated)
    {
        if (flagRenderer == null) return;

        if (activated)
        {
            flagRenderer.material.color = Color.green;
        }
        else
        {
            flagRenderer.material.color = originalColor;
        }
    }
}
