using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.SceneManagement;

// A checkpoint. When a player's frog touches it, it becomes the team's new spawn point: frogs
// that die or fall off come back here, players who join later appear here, and the game is
// saved here (PlayerSpawner does all three). Only the host decides; every player sees the flag
// turn green. Number the checkpoints along the route (1, 2, 3...). The spawn point only moves
// forward, so a frog going back to an earlier checkpoint doesn't drag everyone back with it.
// Needs a NetworkObject and a trigger collider on the same GameObject.
[RequireComponent(typeof(NetworkObject))]
public class CheckpointManager : NetworkBehaviour
{
    // Host only: a player's frog touched this checkpoint.
    // public static event Action<CheckpointManager> OnCheckpointActivated;

    [Tooltip("Where this checkpoint comes along the level: 1 for the first, 2 for the next, and so on. Each checkpoint in a level needs its own number.")]
    [SerializeField, Min(1)] int number = 1;
    [Tooltip("Optional: where frogs come back, one per player (or just one). Empty: Respawn Height Offset above this checkpoint.")]
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float respawnHeightOffset = 1f;

    readonly SyncVar<bool> isActivated = new();

    public int Number => number;

    private Color originalColor;
    private Renderer flagRenderer;

    // A static variable that keeps track of the highest checkpoint number reached in this level session.
    // Starts at 0 (the map beginning).
    private static int highestActiveCheckpointNum = 0;

    private void OnDestroy()
    {
        // When the scene unloads or you quit to the main menu, reset this back to 0 
        highestActiveCheckpointNum = 0;
    }

    private void Awake()
    {
        flagRenderer = GetComponent<Renderer>();
        if (flagRenderer != null)
        {
            originalColor = flagRenderer.material.color;
        }
        isActivated.OnChange += OnActivatedChanged;
    }

    // Reset the static tracker whenever a new scene/run loads so old memory doesn't leak
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticTracker() => highestActiveCheckpointNum = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || isActivated.Value || other.attachedRigidbody == null)
            return;

        // If the player steps on a checkpoint that is LOWER than or EQUAL to the one they already got, ignore it
        if (this.number <= highestActiveCheckpointNum)
        {
            Debug.Log($"[Checkpoint Guard] Checkpoint {this.number} ignored. Players have already reached Checkpoint {highestActiveCheckpointNum}.");
            return;
        }

        // Player frogs are the only things with a FrogMotor
        FrogMotor frog = other.attachedRigidbody.GetComponent<FrogMotor>();
        if (frog != null && !frog.IsDead)
        {
            // Update the server's tracking master sequence number to this new highest value
            highestActiveCheckpointNum = this.number;

            // Loop through all checkpoints in the scene and safely reset any older ones to clean up colors
            CheckpointManager[] allCheckpoints = FindObjectsByType<CheckpointManager>(FindObjectsInactive.Exclude);
            foreach (CheckpointManager cp in allCheckpoints)
            {
                if (cp != this && cp.Number != this.number)
                {
                    cp.DeactivateCheckpoint(); // Reset other flags globally via network sync
                }
            }
            // Lock the state on the server so it cannot be double-triggered
            ActivateCheckpoint();

            // BROADCAST: Tell EVERY player's machine that a new global checkpoint has been established
            ObserversHandleCheckpointGlobal();

            // -------- OR ------- //

            // Fetch the unique network connection of the player who hit this flag
            //NetworkObject playerNetObj = frog.GetComponent<NetworkObject>();
            //if (playerNetObj != null)
            //{
            //    // Send a target instruction ONLY to the machine of the player who touched it
            //    TargetHandleCheckpointTouched(playerNetObj.Owner, frog.gameObject);
            //}
        }
    }

    // TargetRpc ensures this block runs exclusively on the computer of the player who hit the flag
    [TargetRpc]
    private void TargetHandleCheckpointTouched(FishNet.Connection.NetworkConnection conn, GameObject localPlayer)
    {
        if (localPlayer == null) return;

        FrogMotor frogMotor = localPlayer.GetComponent<FrogMotor>();
        if (frogMotor == null) return;

        // Update the local player's running respawn coordinate reference
        Vector3 checkpointPos = transform.position;
        frogMotor.spawnPosition = checkpointPos;
        Debug.Log($"[Checkpoint] Local player spawn position updated to: {checkpointPos}");

        // OFFLINE PROGRESSION SAVE: Write coordinates directly to the user device's local memory registry
        PlayerPrefs.SetFloat("SavedCheckpointX", checkpointPos.x);
        PlayerPrefs.SetFloat("SavedCheckpointY", checkpointPos.y);
        PlayerPrefs.SetFloat("SavedCheckpointZ", checkpointPos.z);
        PlayerPrefs.Save(); // Forces Unity to write the cached updates instantly onto disk storage space safely

        Debug.Log($"[Save System] Local save state updated on disk storage.");
    }

    // ObserversRpc ensures this block executes on EVERY client computer currently connected to the server
    [ObserversRpc]
    private void ObserversHandleCheckpointGlobal()
    {
        // 1. Find all active frogs in this local client's instance of the game
        FrogMotor[] allFrogMotors = FindObjectsByType<FrogMotor>(FindObjectsInactive.Exclude);

        Vector3 checkpointPos = transform.position;

        foreach (FrogMotor frog in allFrogMotors)
        {
            if (frog != null)
            {
                frog.spawnPosition = checkpointPos;
            }
        }
        Debug.Log($"[Global Checkpoint] All local player spawn positions shifted globally to: {checkpointPos}");

        // Create a data package matching your SaveData schema layout
        SaveData myNewSave = new SaveData();
        myNewSave.sceneName = this.gameObject.scene.name;
        myNewSave.checkpoint = this.number;     // Cache this flag index integer

        // Execute save sequence via your custom SaveSystem class
        SaveSystem.Save(myNewSave);
        Debug.Log($"[Save System] Successfully saved current progress to JSON file! Level: {myNewSave.sceneName}, Checkpoint: {myNewSave.checkpoint}");
}

    // Where a player's frog comes back: its own spot when there are several.
    public Pose SpawnPoint(int player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Mathf.Abs(player) % spawnPoints.Length];
            return new Pose(point.position, point.rotation);
        }
        return new Pose(transform.position + Vector3.up * respawnHeightOffset, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
    }

    // Host only (PlayerSpawner calls these). Every machine sees the flag change.
    public void ActivateCheckpoint() => isActivated.Value = true;
    public void DeactivateCheckpoint() => isActivated.Value = false;

    private void OnActivatedChanged(bool previous, bool next, bool asServer)
    {
        if (flagRenderer != null)
        {
            flagRenderer.material.color = next ? Color.green : originalColor;
        }
    }
}