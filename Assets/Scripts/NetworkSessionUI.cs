using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lives on the NetworkManager. Approves joins (4 players online, only the host offline)
// and draws the small in-game panel: mode, players, join code, Leave.
// Games are started from the main menu (PlayMenu); frogs are spawned by each level's
// PlayerSpawner. If a level is opened directly in the editor, the old Host / Join box
// still shows so the level can be tested on its own.
[RequireComponent(typeof(NetworkManager))]
public class NetworkSessionUI : MonoBehaviour
{
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
        bool isHost = request.ClientNetworkId == NetworkManager.ServerClientId;
        int limit = GameSession.IsOffline ? 1 : GameSession.MaxPlayers;
        bool hasRoom = isHost || manager.ConnectedClientsIds.Count < limit;
        response.Approved = hasRoom;
        response.CreatePlayerObject = false; // PlayerSpawner does it once the player is in the level
        response.Reason = hasRoom ? "" : GameSession.IsOffline ? "That game is offline." : $"Session is full ({limit} players max).";
    }

    void OnClientDisconnect(ulong clientId)
    {
        // On a client, our own disconnect means we were turned away or the host left.
        if (manager.IsServer || clientId != manager.LocalClientId)
            return;
        string reason = string.IsNullOrEmpty(manager.DisconnectReason) ? "Disconnected from the host." : manager.DisconnectReason;
        if (SceneManager.GetActiveScene().name == GameSession.MenuScene || GameSession.Current != null)
        {
            GameSession.LastMessage = reason;
            GameSession.LeaveToMenu();
        }
        else
        {
            status = reason; // testing a level directly: stay here
        }
    }

    void OnGUI()
    {
        bool running = manager.IsClient || manager.IsServer;
        if (!running && SceneManager.GetActiveScene().name == GameSession.MenuScene)
            return; // the menu has its own buttons

        GUILayout.BeginArea(new Rect(10, 10, 260, 260), GUI.skin.box);

        if (!running)
        {
            GUILayout.Label("Testing this level directly");
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
            string mode = GameSession.IsOffline ? "Offline" : manager.IsHost ? "Host" : manager.IsServer ? "Server" : "Client";
            GUILayout.Label($"Mode: {mode}");
            if (manager.IsServer && !GameSession.IsOffline)
                GUILayout.Label($"Players: {manager.ConnectedClientsIds.Count}/{GameSession.MaxPlayers}");
            else if (manager.IsConnectedClient)
                status = "";
            if (GameSession.Current != null)
                GUILayout.Label($"Join code: {GameSession.Current.Code}");

            if (GUILayout.Button("Leave"))
            {
                if (SceneManager.GetActiveScene().name == GameSession.MenuScene || GameSession.Current != null || GameSession.IsOffline)
                    GameSession.LeaveToMenu();
                else
                    manager.Shutdown(); // testing a level directly: just stop
            }
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