using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Level exit. When every player in the session is standing inside this trigger, a short
// countdown starts and the host loads the next level for everyone. Someone stepping out
// (or a new player joining) cancels the countdown. Only the host decides; every player
// sees the same "frogs here" count and countdown on the label.
// Needs a NetworkObject and a trigger collider on the same GameObject.
[RequireComponent(typeof(NetworkObject))]
public class LevelExit : NetworkBehaviour
{
    [Tooltip("The level everyone moves to. It needs a PlayerSpawner, no NetworkManager, and must be in the Build Profiles scene list.")]
    [SerializeField] SceneField nextLevel;
    [SerializeField] float countdown = 3f;
    [Tooltip("Optional: a TextMeshPro (3D) text above the exit.")]
    [SerializeField] TMP_Text label;
    [SerializeField] bool labelFacesCamera = true;

    readonly NetworkVariable<int> playersInside = new();
    readonly NetworkVariable<int> playersNeeded = new();
    readonly NetworkVariable<float> secondsLeft = new(-1f);

    // Read by the HUD on every machine.
    public int PlayersInside => playersInside.Value;
    public int PlayersNeeded => playersNeeded.Value;
    public float SecondsLeft => secondsLeft.Value;

    readonly Dictionary<NetworkObject, int> touching = new(); // host only: frog -> its colliders inside
    float timer = -1f;
    bool loading;

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;
        NetworkObject frog = PlayerFrog(other);
        if (frog == null)
            return;
        touching.TryGetValue(frog, out int count);
        touching[frog] = count + 1;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServer)
            return;
        NetworkObject frog = PlayerFrog(other);
        if (frog == null || !touching.TryGetValue(frog, out int count))
            return;
        if (count <= 1)
            touching.Remove(frog);
        else
            touching[frog] = count - 1;
    }

    // The player frog a collider belongs to, or null for anything else (cubes, flies...).
    static NetworkObject PlayerFrog(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        NetworkObject netObject = body != null ? body.GetComponent<NetworkObject>() : null;
        return netObject != null && netObject.IsPlayerObject ? netObject : null;
    }

    void Update()
    {
        if (IsServer && !loading)
            CheckPlayers();
        ShowLabel();
    }

    void CheckPlayers()
    {
        // Everyone connected counts, including someone still loading in: we wait for them.
        int needed = NetworkManager.ConnectedClientsIds.Count;
        int inside = 0;
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            NetworkObject frog = NetworkManager.ConnectedClients[clientId].PlayerObject;
            if (frog != null && touching.ContainsKey(frog))
                inside++;
        }

        if (needed > 0 && inside == needed)
        {
            timer = timer < 0f ? countdown : timer - Time.deltaTime;
            if (timer <= 0f)
                LoadNextLevel();
        }
        else
        {
            timer = -1f;
        }

        // NetworkVariables only send when the value changes, and whole seconds keep that rare.
        playersInside.Value = inside;
        playersNeeded.Value = needed;
        secondsLeft.Value = timer < 0f ? -1f : Mathf.Ceil(timer);
    }

    void LoadNextLevel()
    {
        loading = true; // once only, even if it fails (no error spam every frame)
        if (nextLevel == null || !nextLevel.IsSet)
        {
            Debug.LogError("LevelExit: no Next Level picked.", this);
            return;
        }
        SceneEventProgressStatus status = NetworkManager.SceneManager.LoadScene(nextLevel.SceneName, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
            Debug.LogError($"LevelExit: couldn't load '{nextLevel.SceneName}' ({status}). Is it in the Build Profiles scene list?", this);
    }

    void ShowLabel()
    {
        if (label == null)
            return;
        if (!IsSpawned)
            label.text = "";
        else if (secondsLeft.Value > 0f)
            label.text = $"Next level in {secondsLeft.Value:0}...";
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