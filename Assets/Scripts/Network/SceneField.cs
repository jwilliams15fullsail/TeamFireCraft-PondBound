using UnityEngine;

// A scene picked in the Inspector by dragging the scene file in, instead of typing its
// name. Only the name is used at runtime, so the scene must be in the Build Profiles
// scene list. The drawer that shows it is in Assets/Scripts/Editor/SceneFieldDrawer.cs.
[System.Serializable]
public class SceneField
{
#pragma warning disable CS0169 // only the editor reads it (SceneFieldDrawer)
    [SerializeField] Object sceneAsset;
#pragma warning restore CS0169
    [SerializeField] string sceneName = "";

    public string SceneName => sceneName;
    public bool IsSet => !string.IsNullOrEmpty(sceneName);
}