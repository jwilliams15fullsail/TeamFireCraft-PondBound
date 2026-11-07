using UnityEngine;
using System;
using FishNet.Object;

public class CheckpointManager : NetworkBehaviour
{
    // Pass BOTH the checkpoint AND the GameObject of the specific player who touched it
    public static event Action<CheckpointManager, GameObject> OnCheckpointActivated;

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
        if (!IsServerInitialized) return;

        // Verify the object has a NetworkObject attached
        NetworkObject playerNetworkObject = other.GetComponentInParent<NetworkObject>();
        if (playerNetworkObject != null)
        {
            // Fire the static event passing along WHICH player touched it
            OnCheckpointActivated?.Invoke(this, playerNetworkObject.gameObject);

            // Uniquely turn the flag green ONLY on the screen of the player who touched it
            TargetSetCheckpointVisual(playerNetworkObject.Owner, true);
        }
    }

    public void DeactivateForPlayer(GameObject player)
    {
        if (!IsServerInitialized) return;

        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject != null)
        {
            // Reset the flag color back to original ONLY for this player
            TargetSetCheckpointVisual(playerNetworkObject.Owner, false);
        }
    }

    // TargetRpc ensures this logic runs ONLY on the computer of the player who owns this connection
    [TargetRpc]
    private void TargetSetCheckpointVisual(FishNet.Connection.NetworkConnection connection, bool activated)
    {
        Renderer flagRenderer = GetComponent<Renderer>();
        if (flagRenderer == null) return;

        // Custom local color settings for individual feedback
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
