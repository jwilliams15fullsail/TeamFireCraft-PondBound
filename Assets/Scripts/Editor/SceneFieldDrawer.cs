using UnityEditor;
using UnityEngine;

// Shows a SceneField as a slot you drag a scene file into, and warns when that scene
// isn't in the Build Profiles scene list (Netcode can only load scenes that are).
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        return NotInBuild(property) ? line * 2 + EditorGUIUtility.standardVerticalSpacing : line;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty asset = property.FindPropertyRelative("sceneAsset");
        SerializedProperty sceneName = property.FindPropertyRelative("sceneName");

        EditorGUI.BeginProperty(position, label, property);
        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        asset.objectReferenceValue = EditorGUI.ObjectField(line, label, asset.objectReferenceValue, typeof(SceneAsset), false);

        // Keep the name in step with the file, including after the scene is renamed.
        string current = asset.objectReferenceValue != null ? asset.objectReferenceValue.name : "";
        if (sceneName.stringValue != current)
            sceneName.stringValue = current;

        if (NotInBuild(property))
        {
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.HelpBox(EditorGUI.IndentedRect(line), "Not in the Build Profiles scene list, so it can't be loaded.", MessageType.Warning);
        }
        EditorGUI.EndProperty();
    }

    static bool NotInBuild(SerializedProperty property)
    {
        Object scene = property.FindPropertyRelative("sceneAsset").objectReferenceValue;
        if (scene == null)
            return false;
        string path = AssetDatabase.GetAssetPath(scene);
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            if (buildScene.enabled && buildScene.path == path)
                return false;
        return true;
    }
}