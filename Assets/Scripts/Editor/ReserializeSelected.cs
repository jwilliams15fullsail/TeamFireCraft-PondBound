using System.Linq;
using UnityEditor;

// Rewrites the selected assets in this Unity version's format, which clears
// "below the supported minimum" warnings. Right-click assets in the Project window.
public static class ReserializeSelected
{
    [MenuItem("Assets/Reserialize Selected")]
    static void Run()
    {
        string[] paths = Selection.assetGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
    }

    [MenuItem("Assets/Reserialize Selected", true)]
    static bool CanRun() => Selection.assetGUIDs.Length > 0;
}