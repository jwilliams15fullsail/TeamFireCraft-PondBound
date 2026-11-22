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
// Reset Level On Death: a frog dying or falling off restarts the whole level from that
// checkpoint instead (see LevelReset).
public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("The Frog prefab (FishNet NetworkObject).")]
    [SerializeField] NetworkObject frogPrefab;
    [SerializeField] Transform[] spawnPoints;

    [Header("Death and falling")]
    [Tooltip("When a frog dies or falls off, restart the whole level (doors, switches, boxes, timers, objectives) from the team's last checkpoint. Off: only that frog comes back.")]
    [SerializeField] bool resetLevelOnDeath = true;
    [Tooltip("A frog below this height has fallen off the level (keep it below the lowest floor). Set the Frog prefab's own Respawn Height lower than this, e.g. -1000, so this one decides.")]
    [SerializeField] float fallHeight = -50f;

    NetworkManager manager;
    int spawnCount;
    CheckpointManager checkpoint; // host only: the furthest checkpoint the team has reached (null: none yet)

    // The level's spawner (one per level), for LevelReset and FrogStats.
    public static PlayerSpawner Current { get; private set; }
    public bool ResetLevelOnDeath => resetLevelOnDeath;
    // The team's furthest checkpoint (0 = none yet, the level's start).
    public int CheckpointNumber => checkpoint != null ? checkpoint.Number : 0;

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
    void OnEnable()
    {
        Current = this;
        CheckpointManager.OnCheckpointActivated += ReachCheckpoint;
    }

    void OnDisable()
    {
        if (Current == this)
            Current = null;
        CheckpointManager.OnCheckpointActivated -= ReachCheckpoint;
    }

    // Host only: a frog that falls off the level restarts it (or, with Reset Level On Death off,
    // just comes back at the spawn point).
    void FixedUpdate()
    {
        if (manager == null || !manager.IsServerStarted || LevelReset.IsRestarting)
            return;
        foreach (NetworkConnection connection in manager.ServerManager.Clients.Values)
        {
            NetworkObject frog = connection.FirstObject;
            if (frog == null || frog.transform.position.y >= fallHeight || !frog.TryGetComponent(out FrogMotor motor))
                continue;
            if (!LevelReset.TryRestart(this))
                motor.Respawn();
            return;
        }
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
            return; // a new game: nothing is saved until the team reaches a checkpoint
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