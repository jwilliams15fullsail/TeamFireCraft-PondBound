using System;
using System.IO;
using UnityEngine;

// What the save file holds: the level the team got to, and the last checkpoint they reached there.
[Serializable]
public class SaveData
{
    public string sceneName;
    public int checkpoint; // CheckpointManager Number, 0 = the start of the level
}

// One save slot: a small JSON file in the player's save folder (Application.persistentDataPath).
// Only the host saves, when a level starts and at every checkpoint (PlayerSpawner). The main
// menu's Continue button loads it (PlayMenu).
public static class SaveSystem
{
    static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave => File.Exists(FilePath);

    // Set by Continue, read by the level's PlayerSpawner as the level starts.
    public static SaveData Continuing { get; set; }

    // Enter Play Mode Options skip the domain reload, so clear this at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun() => Continuing = null;

    public static void Save(SaveData data)
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't save the game: {e.Message}");
        }
    }

    // The saved game, or null when there isn't one (or it can't be read).
    public static SaveData Load()
    {
        try
        {
            return HasSave ? JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath)) : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't load the saved game: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (HasSave)
            File.Delete(FilePath);
    }
}