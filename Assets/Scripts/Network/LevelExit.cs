using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

// Level exit. When every player in the session is standing inside this trigger, a short
// countdown starts. Then the host loads the next level for everyone or, with no next level
// picked, every player gets the victory screen. Someone stepping out (or a new player joining)
// cancels the countdown. Only the host decides; every player sees the same "frogs here" count
// and countdown on the label and the HUD.
// Needs a NetworkObject and a trigger collider on the same GameObject.
[RequireComponent(typeof(NetworkObject))]
public class LevelExit : NetworkBehaviour
{
    [Tooltip("The level everyone moves to. It needs a PlayerSpawner and must be in the Build Profiles scene list. Leave it empty on the last level, and everyone gets the victory screen instead.")]
    [SerializeField] SceneField nextLevel;
    [SerializeField] float countdown = 3f;
    [Tooltip("Optional: a TextMeshPro (3D) text above the exit.")]
    [SerializeField] TMP_Text label;
    [SerializeField] bool labelFacesCamera = true;
    [Tooltip("Write each step (a frog in or out, the countdown, the win) to the Console, to find out why the exit isn't working.")]
    [SerializeField] bool logSteps = true;

    readonly SyncVar<int> playersInside = new();
    readonly SyncVar<int> playersNeeded = new();
    readonly SyncVar<float> secondsLeft = new(-1f);
    readonly SyncVar<bool> won = new();

    // Read by the HUD on every machine.
    public int PlayersInside => playersInside.Value;
    public int PlayersNeeded => playersNeeded.Value;
    public float SecondsLeft => secondsLeft.Value;

    readonly Dictionary<NetworkObject, int> touching = new(); // host only: frog -> its colliders inside
    float timer = -1f;
    bool loading;
    bool shownWin;

    void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized)
            return;
        NetworkObject frog = PlayerFrog(other);
        if (frog == null)
            return;
        touching.TryGetValue(frog, out int count);
        touching[frog] = count + 1;
        if (count == 0 && logSteps)
            Debug.Log($"LevelExit '{name}': frog '{frog.name}' is in.", this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServerInitialized)
            return;
        NetworkObject frog = PlayerFrog(other);
        if (frog == null || !touching.TryGetValue(frog, out int count))
            return;
        if (count <= 1)
        {
            touching.Remove(frog);
            if (logSteps)
                Debug.Log($"LevelExit '{name}': frog '{frog.name}' left.", this);
        }
        else
        {
            touching[frog] = count - 1;
        }
    }

    // The player frog a collider belongs to, or null for anything else (cubes, flies...).
    static NetworkObject PlayerFrog(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body == null)
            return null;
        FrogMotor motor = body.GetComponentInParent<FrogMotor>();
        return motor != null ? motor.NetworkObject : null;
    }

    void Update()
    {
        if (IsServerInitialized && !loading)
            CheckPlayers();
        ShowLabel();
        // Every machine shows its own victory screen, once.
        if (won.Value && !shownWin)
        {
            shownWin = true;
            ShowVictory();
        }
    }

    void CheckPlayers()
    {
        // Everyone connected counts, including someone still loading in: we wait for them.
        int needed = ServerManager.Clients.Count;
        int inside = 0;
        foreach (NetworkConnection connection in ServerManager.Clients.Values)
        {
            NetworkObject frog = connection.FirstObject; // the frog PlayerSpawner gave them
            if (frog != null && touching.ContainsKey(frog))
                inside++;
        }
        if (inside != playersInside.Value && logSteps)
            Debug.Log($"LevelExit '{name}': {inside} / {needed} frog(s) at the exit ({touching.Count} frog(s) touching it).", this);

        if (needed > 0 && inside == needed)
        {
            if (timer < 0f && logSteps)
                Debug.Log($"LevelExit '{name}': all {needed} frog(s) are here, leaving in {countdown:0.#} s.", this);
            timer = timer < 0f ? countdown : timer - Time.deltaTime;
            if (timer <= 0f)
                FinishLevel();
        }
        else
        {
            timer = -1f;
        }

        // SyncVars only send when the value changes, and whole seconds keep that rare.
        playersInside.Value = inside;
        playersNeeded.Value = needed;
        secondsLeft.Value = timer < 0f ? -1f : Mathf.Ceil(timer);
    }

    void FinishLevel()
    {
        loading = true; // once only
        if (nextLevel == null || !nextLevel.IsSet)
        {
            if (logSteps)
                Debug.Log($"LevelExit '{name}': no Next Level set, so the team has won.", this);
            won.Value = true; // the last level: nowhere to go, so every player gets the victory screen
        }
        else
        {
            if (logSteps)
                Debug.Log($"LevelExit '{name}': loading the next level, '{nextLevel.SceneName}'.", this);
            GameSession.LoadLevel(nextLevel.SceneName);
        }
    }

    // Through the GameManager when there is one (it came from the main menu), otherwise straight
    // to this level's victory screen.
    void ShowVictory()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.WinGame();
            return;
        }
        Debug.LogWarning("LevelExit: there's no GameManager (it lives in MainMenuScene, so start Play mode from the main menu). Showing the victory screen directly.", this);
        if (VictoryUI.Current != null)
            VictoryUI.Current.Show();
        else
            Debug.LogWarning("LevelExit: this level has no VictoryUI either, so there's no victory screen to show.", this);
    }

    void ShowLabel()
    {
        if (label == null)
            return;
        if (!IsSpawned)
            label.text = "";
        else if (secondsLeft.Value > 0f)
            label.text = $"Leaving in {secondsLeft.Value:0}...";
        else
            label.text = $"Frogs here: {playersInside.Value} / {playersNeeded.Value}";

        Camera view = Camera.main;
        if (labelFacesCamera && view != null)
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - view.transform.position);
    }

    // Shows the exit area in the Scene view so it's easy to place.
    void OnDrawGizmos()
    {
        if (!TryGetComponent(out BoxCollider box))
            return;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
    }
}