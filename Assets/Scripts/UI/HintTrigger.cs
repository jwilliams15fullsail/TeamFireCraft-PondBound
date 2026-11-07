using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class HintTrigger : NetworkBehaviour
{
    [SerializeField] private string hintId;
    [TextArea]
    [SerializeField] private string hintText;

    // Server-side: each player can see each hint only once per session.
    private static readonly HashSet<string> SeenHints = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized)
            return;

        NetworkObject playerObject = other.GetComponentInParent<NetworkObject>();
        if (playerObject == null || playerObject.Owner == null)
            return;

        string playerKey = playerObject.Owner.ClientId.ToString();
        string seenKey = $"{playerKey}:{hintId}";

        if (!SeenHints.Add(seenKey))
            return;

        ShowHintForTarget(playerObject.Owner, hintText);
    }

    private void ShowHintForTarget(NetworkConnection connection, string text)
    {
      
    }
}