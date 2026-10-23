using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

// Spawns each player's frog once that player has this level loaded, so frogs never
// appear in the main menu. Put one in every level frogs play in. Only the host spawns.
public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("The Frog prefab (FishNet NetworkObject).")]
    [SerializeField] NetworkObject frogPrefab;
    [SerializeField] Transform[] spawnPoints;

    NetworkManager manager;
    int spawnCount;

    void Start()
    {
        manager = InstanceFinder.NetworkManager;
        if (manager == null)
        {
            Debug.LogWarning("No FishNet NetworkManager in the game.");
            return;
        }
        manager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
        manager.SceneManager.OnClientPresenceChangeEnd += OnClientPresenceChangeEnd;

        // Arrived from another level as host: spawn everyone already in this scene.
        if (manager.IsServerStarted && manager.SceneManager.SceneConnections.TryGetValue(gameObject.scene, out var here))
        {
            foreach (NetworkConnection connection in here)
                SpawnFrog(connection);
        }
    }

    void OnDestroy()
    {
        if (manager == null || manager.SceneManager == null)
            return;
        manager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
        manager.SceneManager.OnClientPresenceChangeEnd -= OnClientPresenceChangeEnd;
    }

    // A player finished joining (they've loaded the scenes the server is running).
    void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (asServer)
            SpawnFrog(connection);
    }

    // A player finished loading a level the host switched to.
    void OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        if (args.Added && args.Scene == gameObject.scene && manager.IsServerStarted)
            SpawnFrog(args.Connection);
    }

    void SpawnFrog(NetworkConnection connection)
    {
        // One frog each. FirstObject is the first object this player owns.
        if (connection == null || !connection.IsActive || connection.FirstObject != null)
            return;
        Transform point = spawnPoints.Length > 0 ? spawnPoints[spawnCount % spawnPoints.Length] : transform;
        spawnCount++;
        NetworkObject frog = Instantiate(frogPrefab, point.position, point.rotation);
        manager.ServerManager.Spawn(frog, connection, gameObject.scene);
    }
}