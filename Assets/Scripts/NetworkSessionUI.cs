using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lives on the FishNet NetworkManager. Sends players back to the menu when they lose the
// host, and draws the small in-game panel: mode, players, join code, Leave.
// Games are started from the main menu (PlayMenu); frogs are spawned by each level's
// PlayerSpawner. If a level with its own NetworkManager is opened directly in the editor,
// the old Host / Join box still shows so the level can be tested on its own.
// Player limits are set on the transport by GameSession (4 online, 1 offline).
[RequireComponent(typeof(NetworkManager))]
public class NetworkSessionUI : MonoBehaviour
{
    [Tooltip("Show the small Mode / Players / Leave box during a game (handy while testing). The pause menu shows the same info.")]
    [SerializeField] bool showInGamePanel;
    string address = "127.0.0.1";
    string status = "";

    NetworkManager manager;
    bool wasConnected;

    // Start, not Awake: the NetworkManager creates its ClientManager in its own Awake.
    void Start()
    {
        manager = GetComponent<NetworkManager>();
        manager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    void OnDestroy()
    {
        if (manager != null && manager.ClientManager != null)
            manager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
            wasConnected = true;
        if (args.ConnectionState != LocalConnectionState.Stopped)
            return;

        bool lostHost = wasConnected;
        wasConnected = false;
        // Our own Leave, or the host shutting itself down: nothing went wrong.
        if (GameSession.IsLeaving || manager.IsServerStarted)
            return;
        // Never got in: the menu shows the join error itself.
        if (!lostHost)
        {
            status = "Couldn't connect.";
            return;
        }

        const string reason = "Disconnected from the host.";
        if (SceneManager.GetActiveScene().name != GameSession.MenuScene && GameSession.Current == null)
        {
            status = reason; // testing a level directly: stay here
            return;
        }
        GameSession.LastMessage = reason;
        GameSession.LeaveToMenu();
    }

    void OnGUI()
    {
        if (manager == null)
            return;
        bool running = manager.ServerManager.Started || manager.ClientManager.Started;
        if (!running && SceneManager.GetActiveScene().name == GameSession.MenuScene)
            return; // the menu has its own buttons
        if (running && !showInGamePanel)
            return; // in a game: the pause menu has the join code, Leave and the controls

        GUILayout.BeginArea(new Rect(10, 10, 260, 260), GUI.skin.box);

        if (!running)
        {
            GUILayout.Label("Testing this level directly");
            GUILayout.Label("Address");
            address = GUILayout.TextField(address);

            if (GUILayout.Button("Host") && SetAddress())
            {
                bool started = manager.ServerManager.StartConnection();
                if (started)
                    manager.ClientManager.StartConnection();
                status = started ? "" : "Failed to start host.";
            }
            if (GUILayout.Button("Join") && SetAddress())
                status = manager.ClientManager.StartConnection() ? "Connecting..." : "Failed to start client.";
        }
        else
        {
            string mode = GameSession.IsOffline ? "Offline" : manager.IsHostStarted ? "Host" : manager.IsServerStarted ? "Server" : "Client";
            GUILayout.Label($"Mode: {mode}");
            if (manager.IsServerStarted && !GameSession.IsOffline)
                GUILayout.Label($"Players: {manager.ServerManager.Clients.Count}/{GameSession.MaxPlayers}");
            else if (manager.IsClientStarted)
                status = "";
            if (GameSession.Current != null)
                GUILayout.Label($"Join code: {GameSession.Current.Code}");

            if (GUILayout.Button("Leave"))
            {
                if (SceneManager.GetActiveScene().name == GameSession.MenuScene || GameSession.Current != null || GameSession.IsOffline)
                    GameSession.LeaveToMenu();
                else
                    GameSession.StopNetwork(manager); // testing a level directly: just stop
            }
        }

        if (!string.IsNullOrEmpty(status))
            GUILayout.Label(status);

        GUILayout.Label("WASD move, mouse look\nHold/release Space: charged jump\nLMB tongue, hold RMB reel\nE stick/release, Q break free\nTab: free the mouse");
        GUILayout.EndArea();
    }

    // Direct connection for testing: the host listens on every network card, so other
    // PCs on the same network can join with this PC's address.
    bool SetAddress()
    {
        UnityTransport transport = GameSession.TransportOf(manager);
        if (transport == null)
            return false;
        transport.SetConnectionData(address, 7777, "0.0.0.0");
        transport.SetMaximumClients(GameSession.MaxPlayers);
        return true;
    }
}