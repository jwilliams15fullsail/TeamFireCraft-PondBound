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

        Vector3 spawnPosition = spawnPoints.Length > 0 ? spawnPoints[spawnCount % spawnPoints.Length].position : transform.position;
        Quaternion spawnRotation = spawnPoints.Length > 0 ? spawnPoints[spawnCount % spawnPoints.Length].rotation : transform.rotation;

        // --- INTERCEPT SYSTEM: SAVE DATA RESTORATION ---
        // Instead of reading a static variable that gets erased, read directly from the save file
        // We only intercept if SaveSystem.Continuing was set by the menus Continue button
        if (SaveSystem.Continuing != null)
        {
            SaveData diskSave = SaveSystem.Load();

            // Verify that the file matches the current scene layout and contains a valid checkpoint
            if (diskSave != null && diskSave.sceneName == gameObject.scene.name && diskSave.checkpoint > 0)
            {
                // Find all active checkpoints in this level scene
                CheckpointManager[] checkpoints = FindObjectsByType<CheckpointManager>();
                CheckpointManager targetCheckpoint = null;

                foreach (CheckpointManager cp in checkpoints)
                {
                    if (cp.Number == diskSave.checkpoint)
                    {
                        targetCheckpoint = cp;
                        break;
                    }
                }

                if (targetCheckpoint != null)
                {
                    // Pull the calculated multi-player safe Pose layout directly from that checkpoint object
                    Pose pose = targetCheckpoint.SpawnPoint(connection.ClientId);
                    spawnPosition = pose.position;
                    spawnRotation = pose.rotation;
                    Debug.Log($"[Save Spawner] Intercepted default spawn. Routing player connection {connection.ClientId} to Checkpoint #{SaveSystem.Continuing.checkpoint}");
                }
            }
        }
        // ------------------------------------------------

        spawnCount++;

        // Instantiate and network-spawn the frog at our validated coordinates
        NetworkObject frog = Instantiate(frogPrefab, spawnPosition, spawnRotation);

        // Update the motor's running fallback coordinates so it respawns at this checkpoint if it dies
        FrogMotor motor = frog.GetComponent<FrogMotor>();
        if (motor != null)
        {
            motor.spawnPosition = spawnPosition;
        }

        manager.ServerManager.Spawn(frog, connection, gameObject.scene);

        // Force clients to sync visuals if we are executing a special save file positioning logic
        var netTransform = frog.GetComponent<FishNet.Component.Transforming.NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport();
        }
    }
}