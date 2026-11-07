using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

// Spawns each player's frog once that player has this level loaded, so frogs never
// appear in the main menu. Put one in every level frogs play in. Only the host spawns.
// Once the team reaches a checkpoint, frogs spawn there instead, it's where they come back
// after dying or falling off, and the game is saved there (Continue on the main menu).
public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("The Frog prefab (FishNet NetworkObject).")]
    [SerializeField] NetworkObject frogPrefab;
    [SerializeField] Transform[] spawnPoints;

    NetworkManager manager;
    int spawnCount;
    CheckpointManager checkpoint; // host only: the furthest checkpoint the team has reached (null: none yet)

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
        if (manager.IsServerStarted)
            StartFromSave(); // first, so anyone spawned below appears at a saved checkpoint

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

    // Checkpoints only send this on the host.
    void OnEnable() => CheckpointManager.OnCheckpointActivated += ReachCheckpoint;
    void OnDisable() => CheckpointManager.OnCheckpointActivated -= ReachCheckpoint;

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

    // Host only: a player's frog touched a checkpoint. If it's further along than the team's
    // last one, it becomes everyone's spawn point, and the game is saved there.
    void ReachCheckpoint(CheckpointManager reached)
    {
        if (checkpoint != null && reached.Number <= checkpoint.Number)
            return;
        if (checkpoint != null)
            checkpoint.DeactivateCheckpoint();
        checkpoint = reached;
        checkpoint.ActivateCheckpoint();

        foreach (NetworkConnection connection in manager.ServerManager.Clients.Values)
        {
            NetworkObject frog = connection.FirstObject; // the frog SpawnFrog gave them
            if (frog != null && frog.TryGetComponent(out FrogMotor motor))
                motor.SetSpawnPoint(checkpoint.SpawnPoint(connection.ClientId));
        }
        SaveSystem.Save(new SaveData { sceneName = gameObject.scene.name, checkpoint = checkpoint.Number });
    }

    // Host only, as the level starts. After Continue on the main menu: start from the saved
    // checkpoint. Otherwise (a new game, or the team moved on to this level): save that.
    void StartFromSave()
    {
        SaveData save = SaveSystem.Continuing;
        SaveSystem.Continuing = null;
        if (save == null || save.sceneName != gameObject.scene.name)
        {
            SaveSystem.Save(new SaveData { sceneName = gameObject.scene.name });
            return;
        }
        foreach (CheckpointManager each in FindObjectsByType<CheckpointManager>(FindObjectsInactive.Exclude))
        {
            if (each.Number == save.checkpoint)
                ReachCheckpoint(each);
        }
    }

    void SpawnFrog(NetworkConnection connection)
    {
        // One frog each. FirstObject is the first object this player owns.
        if (connection == null || !connection.IsActive || connection.FirstObject != null)
            return;
        // At the team's checkpoint once there is one, otherwise at this level's spawn points.
        Transform point = spawnPoints.Length > 0 ? spawnPoints[spawnCount % spawnPoints.Length] : transform;
        Pose spawn = checkpoint != null ? checkpoint.SpawnPoint(connection.ClientId) : new Pose(point.position, point.rotation);
        spawnCount++;
        NetworkObject frog = Instantiate(frogPrefab, spawn.position, spawn.rotation);
        manager.ServerManager.Spawn(frog, connection, gameObject.scene);
    }
}