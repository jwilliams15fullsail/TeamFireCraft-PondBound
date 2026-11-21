using System.Collections;
using FishNet;
using UnityEngine;

// Lives in the LevelReload scene: a moment after it loads, the host loads the level again,
// and the level's PlayerSpawner starts everyone at the checkpoint saved in SaveSystem.Continuing.
// See LevelReset.
public class LevelReloader : MonoBehaviour
{
    [Tooltip("Seconds to wait in the empty scene before loading the level again (lets every player arrive).")]
    [SerializeField, Min(0f)] float delay = 0.3f;

    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(delay);
        if (!InstanceFinder.IsServerStarted)
            yield break; // only the host loads levels; everyone else follows
        SaveData save = SaveSystem.Continuing;
        LevelReset.Finished();
        if (save == null || string.IsNullOrEmpty(save.sceneName))
        {
            Debug.LogWarning("LevelReloader: no level to go back to, returning to the menu.");
            GameSession.LeaveToMenu();
            yield break;
        }
        GameSession.LoadLevel(save.sceneName);
    }
}