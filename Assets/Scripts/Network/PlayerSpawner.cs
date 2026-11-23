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
// Each checkpoint also saves which objectives were done when the team got there, so starting
// again from it shows the next objective that isn't done (ObjectiveSequence).
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

    [Header("Starting at a checkpoint")]
    [Tooltip("After a restart or Continue, how long (seconds) to keep looking for the saved checkpoint if it isn't ready the moment the level loads.")]
    [SerializeField, Min(0.5f)] float findCheckpointTimeout = 5f;

    NetworkManager manager;
    int spawnCount;
    CheckpointManager checkpoint; // host only: the furthest checkpoint the team has reached (null: none yet)
    int pendingNumber;            // host only: the checkpoint to start at, not found yet (0: none)
    float pendingSince;

    // This run's furthest checkpoint, kept across level restarts (the spawner itself is rebuilt
    // every time the level loads). Cleared when a level starts fresh.
    static string reachedScene;
    static int reachedNumber;
    static int reachedObjectives; // the objectives done when the team reached that checkpoint

    // Enter Play Mode Options skip the domain reload, so clear these at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun()
    {
        reachedScene = null;
        reachedNumber = 0;
        reachedObjectives = 0;
    }

    // The level's spawner (one per level), for LevelReset and FrogStats.
    public static PlayerSpawner Current { get; private set; }
    public bool ResetLevelOnDeath => resetLevelOnDeath;

    // The team's furthest checkpoint (0 = none yet, the level's start). Also counts a checkpoint
    // reached earlier in this run (before a restart) and one the level is still starting at.
    public int CheckpointNumber
    {
        get
        {
            int number = checkpoint != null ? checkpoint.Number : 0;
            if (reachedScene == gameObject.scene.name)
                number = Mathf.Max(number, reachedNumber);
            return Mathf.Max(number, pendingNumber);
        }
    }

    // The objectives that were done when the team reached that checkpoint (bit i = objective i).
    public int ObjectivesAtCheckpoint => reachedScene == gameObject.scene.name ? reachedObjectives : 0;

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
        WarnAboutDuplicateNumbers();
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

    // Host only: still looking for the checkpoint to start at (it wasn't ready as the level
    // loaded). Update, not FixedUpdate, so it still runs if the game is paused.
    void Update()
    {
        if (pendingNumber <= 0 || manager == null || !manager.IsServerStarted)
            return;
        if (TryStartAtPending())
            return;
        if (Time.realtimeSinceStartup - pendingSince > findCheckpointTimeout)
        {
            Debug.LogWarning($"PlayerSpawner: couldn't find checkpoint {pendingNumber} in '{gameObject.scene.name}', so the team starts at the beginning. Check that checkpoint's Number.");
            pendingNumber = 0;
        }
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
        if (reached == null || reached.gameObject.scene != gameObject.scene)
            return;
        if (checkpoint != null && reached.Number <= checkpoint.Number)
            return;
        if (checkpoint != null)
            checkpoint.DeactivateCheckpoint();
        checkpoint = reached;
        checkpoint.ActivateCheckpoint();

        // Remembered for this run, so a restart comes back here even though this spawner is
        // rebuilt, along with the objectives done so far.
        string sceneName = gameObject.scene.name;
        int objectives = ObjectiveSequence.DoneIn(sceneName);
        if (reachedScene != sceneName || checkpoint.Number >= reachedNumber)
            reachedObjectives = objectives;
        reachedNumber = reachedScene == sceneName ? Mathf.Max(reachedNumber, checkpoint.Number) : checkpoint.Number;
        reachedScene = sceneName;

        foreach (NetworkConnection connection in manager.ServerManager.Clients.Values)
        {
            NetworkObject frog = connection.FirstObject; // the frog SpawnFrog gave them
            if (frog != null && frog.TryGetComponent(out FrogMotor motor))
                motor.SetSpawnPoint(checkpoint.SpawnPoint(connection.ClientId));
        }
        SaveSystem.Save(new SaveData { sceneName = sceneName, checkpoint = checkpoint.Number, objectivesDone = objectives });
    }

    // Two checkpoints with the same Number: only the first one touched counts, and a restart
    // may pick either. Each needs its own Number, in route order.
    void WarnAboutDuplicateNumbers()
    {
        var seen = new System.Collections.Generic.Dictionary<int, CheckpointManager>();
        foreach (CheckpointManager each in FindObjectsByType<CheckpointManager>(FindObjectsInactive.Include))
        {
            if (each.gameObject.scene != gameObject.scene)
                continue;
            if (seen.TryGetValue(each.Number, out CheckpointManager other))
                Debug.LogError($"Checkpoints '{other.name}' and '{each.name}' both have Number {each.Number}. Give every checkpoint its own Number in route order (1, 2, 3...), or only one of them will ever work.", each);
            else
                seen[each.Number] = each;
        }
    }

    // Host only, as the level starts. After Continue on the main menu or a restart: start from
    // the saved checkpoint. Otherwise (a new game, or the team moved on to this level): start
    // at the level's spawn points.
    void StartFromSave()
    {
        SaveData save = SaveSystem.Continuing;
        SaveSystem.Continuing = null;
        if (save == null || save.sceneName != gameObject.scene.name || save.checkpoint <= 0)
        {
            // A fresh start here: forget any checkpoint and objectives from an earlier game.
            reachedScene = null;
            reachedNumber = 0;
            reachedObjectives = 0;
            ObjectiveSequence.StartWith(gameObject.scene.name, 0);
            Debug.Log($"Starting '{gameObject.scene.name}' at the beginning.");
            return;
        }
        // The objectives done when the team reached that checkpoint are done again.
        ObjectiveSequence.StartWith(gameObject.scene.name, save.objectivesDone);
        pendingNumber = save.checkpoint;
        pendingSince = Time.realtimeSinceStartup;
        if (!TryStartAtPending())
            Debug.Log($"PlayerSpawner: checkpoint {pendingNumber} isn't ready yet, still looking.");
    }

    // Host only: the checkpoint to start at is in the level now. It becomes the team's spawn
    // point, and any frog that already appeared at the beginning is moved there.
    bool TryStartAtPending()
    {
        CheckpointManager found = FindCheckpoint(pendingNumber);
        if (found == null)
            return false;
        pendingNumber = 0;
        ReachCheckpoint(found);
        foreach (NetworkConnection connection in manager.ServerManager.Clients.Values)
        {
            NetworkObject frog = connection.FirstObject;
            if (frog != null && frog.TryGetComponent(out FrogMotor motor))
                motor.Respawn();
        }
        Debug.Log($"Starting at checkpoint {found.Number} ('{found.name}').");
        return true;
    }

    // This level's checkpoint with that Number, switched on or not (an active one if there are two).
    CheckpointManager FindCheckpoint(int number)
    {
        CheckpointManager best = null;
        foreach (CheckpointManager each in FindObjectsByType<CheckpointManager>(FindObjectsInactive.Include))
        {
            if (each.gameObject.scene != gameObject.scene || each.Number != number)
                continue;
            if (best == null || (!best.gameObject.activeInHierarchy && each.gameObject.activeInHierarchy))
                best = each;
        }
        return best;
    }

    void SpawnFrog(NetworkConnection connection)
    {
        // One frog each. FirstObject is the first object this player owns.
        if (connection == null || !connection.IsActive || connection.FirstObject != null)
            return;
        if (pendingNumber > 0)
            TryStartAtPending(); // one more look, so this frog appears at the checkpoint
        // At the team's checkpoint once there is one, otherwise at this level's spawn points.
        Transform point = spawnPoints.Length > 0 ? spawnPoints[spawnCount % spawnPoints.Length] : transform;
        Pose spawn = checkpoint != null ? checkpoint.SpawnPoint(connection.ClientId) : new Pose(point.position, point.rotation);
        spawnCount++;
        NetworkObject frog = Instantiate(frogPrefab, spawn.position, spawn.rotation);
        manager.ServerManager.Spawn(frog, connection, gameObject.scene);
    }
}