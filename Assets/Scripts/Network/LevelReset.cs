using FishNet;
using UnityEngine;

// Restarts the level from the team's last checkpoint: doors, plates, switches, boxes, hazards
// and timers all go back to how the level starts, and the frogs come back at the checkpoint
// (or at the level's spawn points if no checkpoint has been reached yet). The objectives that
// were done when the team reached that checkpoint stay done.
// Two ways in:
// - A frog dies or falls off the map, when PlayerSpawner's Reset Level On Death is on (TryRestart).
// - The pause menu's Restart from Checkpoint button (Restart), whatever that setting is.
// FishNet won't reload a level that's already loaded, so the host goes through a tiny empty
// scene (LevelReload, with a LevelReloader in it) and from there loads the level again.
// Host only. Everyone in the game goes back together.
public static class LevelReset
{
    public const string ReloadSceneName = "LevelReload";

    static bool restarting;
    public static bool IsRestarting => restarting;

    // True on the machine that can restart: the host (offline, that's you).
    public static bool CanRestart => InstanceFinder.IsServerStarted && PlayerSpawner.Current != null && !restarting;

    // Enter Play Mode Options skip the domain reload, so clear this at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun() => restarting = false;

    // Host only, for deaths and falls. True when the level is restarting (now, or already), so
    // the caller shouldn't respawn the frog itself. False when this level doesn't reset on death.
    public static bool TryRestart(PlayerSpawner spawner)
    {
        if (spawner == null || !spawner.ResetLevelOnDeath)
            return false;
        return Restart(spawner);
    }

    // Host only: restart the level from the team's last checkpoint now. False if it can't
    // (not the host, or no PlayerSpawner in this level).
    public static bool Restart(PlayerSpawner spawner)
    {
        if (spawner == null || !InstanceFinder.IsServerStarted)
            return false;
        if (restarting)
            return true;
        restarting = true;
        string sceneName = spawner.gameObject.scene.name;
        int number = spawner.CheckpointNumber; // 0 = the level's start
        SaveSystem.Continuing = new SaveData
        {
            sceneName = sceneName,
            checkpoint = number,
            objectivesDone = number > 0 ? spawner.ObjectivesAtCheckpoint : 0,
        };
        Debug.Log(number > 0
            ? $"Restarting '{sceneName}' from checkpoint {number}."
            : $"Restarting '{sceneName}' from the beginning (no checkpoint reached yet).");
        GameSession.LoadLevel(ReloadSceneName);
        return true;
    }

    // LevelReloader: the level is being loaded again, so the next death can restart it.
    public static void Finished() => restarting = false;
}