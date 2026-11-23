using System;
using System.IO;
using UnityEngine;

// What the save holds: the level the team got to, the last checkpoint they reached there, and
// which objectives were done when they reached it.
[Serializable]
public class SaveData
{
    public string sceneName;
    public int checkpoint; // CheckpointManager Number, 0 = the start of the level
    public int objectivesDone; // ObjectiveSequence: bit i set = objective i was done (older saves: 0)
}

// One save slot, kept in PlayerPrefs (the registry on Windows, the browser's storage on Web) and
// written to disk the moment it changes, so it's still there after quitting, a crash, or closing
// the browser tab. Only the host saves, at every checkpoint (PlayerSpawner). The main menu's
// Continue button loads it (PlayMenu) and the level's PlayerSpawner starts everyone there.
// Older builds kept it in save.json in the save folder; that file is read once and moved over.
public static class SaveSystem
{
    const string Key = "PondBound.Save";

    public static bool HasSave => PlayerPrefs.HasKey(Key) || LegacyFileExists();
    // A save Continue can use: one made at a checkpoint, not just the start of a level.
    public static bool HasCheckpointSave => Load() is SaveData save && save.checkpoint > 0 && !string.IsNullOrEmpty(save.sceneName);

    // Set by Continue, read by the level's PlayerSpawner as the level starts.
    public static SaveData Continuing { get; set; }

    // Enter Play Mode Options skip the domain reload, so clear this at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun() => Continuing = null;

    public static void Save(SaveData data)
    {
        if (data == null)
            return;
        try
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save(); // now, not on quit: a crash or a closed tab would lose it
            Debug.Log($"Game saved: {data.sceneName}, checkpoint {data.checkpoint}.");
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
            if (PlayerPrefs.HasKey(Key))
                return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key));
            if (!LegacyFileExists())
                return null;
            SaveData old = JsonUtility.FromJson<SaveData>(File.ReadAllText(LegacyFilePath));
            if (old != null)
                Save(old); // moved over; the file stays as a backup
            return old;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't load the saved game: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        try
        {
            if (LegacyFileExists())
                File.Delete(LegacyFilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't delete the old save file: {e.Message}");
        }
    }

    static string LegacyFilePath => Path.Combine(Application.persistentDataPath, "save.json");

    // The save folder's name comes from the Company and Product Name, and a bad one throws.
    static bool LegacyFileExists()
    {
        try
        {
            return File.Exists(LegacyFilePath);
        }
        catch
        {
            return false;
        }
    }
}