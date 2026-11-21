using UnityEngine;

// Restarts the level from the team's last checkpoint when a frog dies (or falls off the map):
// doors, plates, switches, boxes, hazards, timers and objectives all go back to how the level
// starts, and the frogs come back at the checkpoint. Turned on per level with PlayerSpawner's
// Reset Level On Death.
// FishNet won't reload a level that's already loaded, so the host goes through a tiny empty
// scene (LevelReload, with a LevelReloader in it) and from there loads the level again.
// Host only. Everyone in the game goes back together.
public static class LevelReset
{
    public const string ReloadSceneName = "LevelReload";

    static bool restarting;
    public static bool IsRestarting => restarting;

    // Enter Play Mode Options skip the domain reload, so clear this at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun() => restarting = false;

    // Host only. True when the level is restarting (now, or already), so the caller shouldn't
    // respawn the frog itself. False when this level doesn't reset on death.
    public static bool TryRestart(PlayerSpawner spawner)
    {
        if (spawner == null || !spawner.ResetLevelOnDeath)
            return false;
        if (restarting)
            return true;
        restarting = true;
        SaveSystem.Continuing = new SaveData
        {
            sceneName = spawner.gameObject.scene.name,
            checkpoint = spawner.CheckpointNumber, // 0 = the level's start
        };
        GameSession.LoadLevel(ReloadSceneName);
        return true;
    }

    // LevelReloader: the level is being loaded again, so the next death can restart it.
    public static void Finished() => restarting = false;
}