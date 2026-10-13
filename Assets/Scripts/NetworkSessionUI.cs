using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

// Minimal Host / Join UI for a 1-4 player local/LAN session.
[RequireComponent(typeof(NetworkManager))]
public class NetworkSessionUI : MonoBehaviour
{
    const int MaxPlayers = 4;

    string address = "127.0.0.1";
    string status = "";

    NetworkManager manager;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
        manager.NetworkConfig.ConnectionApproval = true;
        manager.ConnectionApprovalCallback = ApproveConnection;
        manager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int playerIndex = manager.ConnectedClientsIds.Count;
        bool hasRoom = playerIndex < MaxPlayers;
        response.Approved = hasRoom;
        response.CreatePlayerObject = hasRoom;
        response.Reason = hasRoom ? "" : $"Session is full ({MaxPlayers} players max).";

        // Spawn frogs side by side so they never start inside each other.
        response.Position = new Vector3(-3f + 2f * playerIndex, 1f, 0f);
        response.Rotation = Quaternion.identity;
    }

    void OnClientDisconnect(ulong clientId)
    {
        // On a client, our own disconnect means we were rejected or the host left.
        if (!manager.IsServer && clientId == manager.LocalClientId)
            status = string.IsNullOrEmpty(manager.DisconnectReason) ? "Disconnected." : manager.DisconnectReason;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 260, 240), GUI.skin.box);

        if (!manager.IsClient && !manager.IsServer)
        {
            GUILayout.Label("Address");
            address = GUILayout.TextField(address);

            if (GUILayout.Button("Host"))
            {
                SetAddress();
                status = manager.StartHost() ? "" : "Failed to start host.";
            }
            if (GUILayout.Button("Join"))
            {
                SetAddress();
                status = manager.StartClient() ? "Connecting..." : "Failed to start client.";
            }
        }
        else
        {
            string mode = manager.IsHost ? "Host" : manager.IsServer ? "Server" : "Client";
            GUILayout.Label($"Mode: {mode}");
            if (manager.IsServer)
                GUILayout.Label($"Players: {manager.ConnectedClientsIds.Count}/{MaxPlayers}");
            else if (manager.IsConnectedClient)
                status = "";

            if (GUILayout.Button("Leave"))
                manager.Shutdown();
        }

        if (!string.IsNullOrEmpty(status))
            GUILayout.Label(status);

        GUILayout.Label("WASD move, mouse look\nHold/release Space: charged jump\nLMB tongue, hold RMB reel\nE stick/release, Q break free\nTab: free the mouse");
        GUILayout.EndArea();
    }

    void SetAddress()
    {
        var transport = manager.GetComponent<UnityTransport>();
        if (transport != null)
            transport.SetConnectionData(address, 7777);
    }
}
