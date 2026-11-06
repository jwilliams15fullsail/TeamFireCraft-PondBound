using UnityEngine;
using System;
using FishNet.Object;

public class CheckpointManager : NetworkBehaviour
{
    // Pass BOTH the checkpoint AND the GameObject of the specific player who touched it
    public static event Action<CheckpointManager, GameObject> OnCheckpointActivated;

    // private bool isActivated = false;
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

        // Verify the object has a NetworkObject attached
        NetworkObject playerNetworkObject = other.GetComponentInParent<NetworkObject>();
        if (playerNetworkObject != null)
        {
            // Fire the static event passing along WHICH player touched it
            OnCheckpointActivated?.Invoke(this, playerNetworkObject.gameObject);

            // Uniquely turn the flag green ONLY on the screen of the player who touched it
            TargetSetCheckpointVisual(playerNetworkObject.Owner, true);
        }

        //if (other.CompareTag("Frog") && !isActivated)
        //{
        //    isActivated = true;

        //    // Change color on the server and tell all clients to do the same
        //    ObserversSetCheckpointState(true);

        //    // Fire the event. The ?. ensures it only runs if something is listening
        //    OnCheckpointActivated?.Invoke(this);
        //}
    }

    //public void DeactivateCheckpoint()
    //{
    //    // Ensure only the server can dictate deactivation state
    //    if (!IsServerStarted) return;

    //    isActivated = false;
    //    if (flagRenderer != null)
    //    {
    //        flagRenderer.material.color = originalColor;
    //    }
    //}

    public void DeactivateForPlayer(GameObject player)
    {
        if (!IsServerStarted) return;

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

    //// This RPC ensures the visual color changes happen perfectly across all players screens
    //[ObserversRpc]
    //private void ObserversSetCheckpointState(bool activated)
    //{
    //    if (flagRenderer == null) return;

    //    if (activated)
    //    {
    //        flagRenderer.material.color = Color.green;
    //    }
    //    else
    //    {
    //        flagRenderer.material.color = originalColor;
    //    }
    //}
}
