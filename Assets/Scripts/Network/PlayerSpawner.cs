using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Spawns each player's frog once that player has this level loaded, so frogs never
// appear in the main menu. Put one in every level frogs play in. Only the host spawns.
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;

    NetworkManager manager;
    bool listening;
    int spawnCount;
    readonly HashSet<ulong> inLevel = new(); // clients that finished loading this level

    void Start()
    {
        manager = NetworkManager.Singleton;
        if (manager == null)
            Debug.LogWarning("No NetworkManager. Start the game from MainMenuScene.");
        else if (manager.IsServer)
            Listen(); // arrived from the menu as host
    }

    void OnDestroy()
    {
        if (listening && manager != null && manager.SceneManager != null)
            manager.SceneManager.OnSceneEvent -= OnSceneEvent;
    }

    void Update()
    {
        if (manager == null || !manager.IsServer)
            return;

        Listen(); // in case the host was started from this level
        foreach (ulong clientId in manager.ConnectedClientsIds)
        {
            bool ready = clientId == NetworkManager.ServerClientId || inLevel.Contains(clientId); // the host is already here
            if (ready && manager.ConnectedClients[clientId].PlayerObject == null)
                SpawnFrog(clientId);
        }
    }

    void Listen()
    {
        if (listening)
            return;
        manager.SceneManager.OnSceneEvent += OnSceneEvent;
        listening = true;
    }

    void OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadEventCompleted: // everyone who was connected when the host loaded this level
                if (sceneEvent.SceneName == gameObject.scene.name)
                    inLevel.UnionWith(sceneEvent.ClientsThatCompleted);
                break;
            case SceneEventType.SynchronizeComplete: // someone joined while the game was already running
                inLevel.Add(sceneEvent.ClientId);
                break;
        }
    }

    void SpawnFrog(ulong clientId)
    {
        Transform point = spawnPoints.Length > 0 ? spawnPoints[spawnCount % spawnPoints.Length] : transform;
        spawnCount++;
        NetworkObject prefab = manager.NetworkConfig.PlayerPrefab.GetComponent<NetworkObject>();
        manager.SpawnManager.InstantiateAndSpawn(prefab, clientId, destroyWithScene: true, isPlayerObject: true,
            position: point.position, rotation: point.rotation);
    }
}