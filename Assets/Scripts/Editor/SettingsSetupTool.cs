using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > Settings Menu > 3. Connect All Settings. One click, safe to run again:
// - points each menu's volume script at the sliders in Settings (NEW);
// - switches off the old duplicate settingsController components;
// - gives the dropdown lists a copy of the font material that masks can clip (the game font's
//   own shader ignores masks, so long lists spilled out of their box);
// - allows window resizing and sets a sensible first window size.
public static class SettingsSetupTool
{
    const string MenuPath = "Assets/Prefabs/UI/Settings (NEW).prefab";
    const string PausePath = "Assets/Prefabs/UI/Pause Menu UI.prefab";
    const string MainMenuPath = "Assets/Scenes/MainMenuScene.unity";

    [MenuItem("Tools/Settings Menu/3. Connect All Settings")]
    static void RunMenu()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Settings Setup: stop Play mode first.");
            return;
        }
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            Debug.LogError("Settings Setup: save and leave Prefab Mode first, then run this again.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        Run();
    }

    public static void Run()
    {
        var report = new List<string>();

        // The shared menu first, so the pause menu and main menu pick the change up.
        GameObject settings = PrefabUtility.LoadPrefabContents(MenuPath);
        try
        {
            if (FixDropdownClipping(settings, report))
                PrefabUtility.SaveAsPrefabAsset(settings, MenuPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(settings);
        }

        GameObject pause = PrefabUtility.LoadPrefabContents(PausePath);
        try
        {
            if (Connect(new[] { pause }, "Pause menu", report))
                PrefabUtility.SaveAsPrefabAsset(pause, PausePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(pause);
        }

        var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        if (Connect(scene.GetRootGameObjects(), "Main menu", report))
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        SetPlayerSettings(report);
        AssetDatabase.SaveAssets();

        Debug.Log("Settings Setup finished:\n" + string.Join("\n", report));
    }

    static bool Connect(GameObject[] roots, string where, List<string> report)
    {
        Transform menu = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t =>
            PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject) &&
            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject) == MenuPath);
        if (menu == null)
        {
            report.Add($"{where}: no Settings (NEW) found, nothing changed.");
            return false;
        }

        bool changed = false;
        Slider[] sliders = menu.GetComponentsInChildren<Slider>(true);
        foreach (var volume in roots.SelectMany(r => r.GetComponentsInChildren<VolumeSettings>(true)))
        {
            var data = new SerializedObject(volume);
            foreach (var (slot, name) in new[] { ("masterSlider", "Master Volume"), ("musicSlider", "Music Volume"), ("sfxSlider", "SFX Volume") })
            {
                Slider slider = sliders.FirstOrDefault(s => s.name.EndsWith(name));
                SerializedProperty property = data.FindProperty(slot);
                if (slider == null || property == null)
                {
                    report.Add($"{where}: couldn't connect {name}.");
                    continue;
                }
                if (property.objectReferenceValue == slider)
                    continue;
                property.objectReferenceValue = slider;
                report.Add($"{where}: connected {name}.");
                changed = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        foreach (var controller in roots.SelectMany(r => r.GetComponentsInChildren<settingsController>(true)))
        {
            if (controller.transform == menu || !controller.enabled)
                continue;
            controller.enabled = false;
            EditorUtility.SetDirty(controller);
            if (PrefabUtility.IsPartOfPrefabInstance(controller))
                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            report.Add($"{where}: switched off the duplicate settingsController on '{controller.name}'.");
            changed = true;
        }

        if (!changed)
            report.Add($"{where}: already connected.");
        return changed;
    }

    static bool FixDropdownClipping(GameObject root, List<string> report)
    {
        Shader maskable = Shader.Find("UI/Default");
        int changed = 0;
        foreach (var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            TMP_Text text = dropdown.itemText;
            if (text == null || text.font == null)
                continue;
            Material current = text.fontSharedMaterial;
            if (current == null || current.shader == null || current.shader.name != "GUI/Text Shader")
                continue; // already fine

            // Saved next to the font. Same look, but this shader supports masks.
            string fontPath = AssetDatabase.GetAssetPath(text.font);
            string copyPath = fontPath.Substring(0, fontPath.LastIndexOf('.')) + " Dropdown.mat";
            var copy = AssetDatabase.LoadAssetAtPath<Material>(copyPath);
            if (copy == null)
            {
                copy = new Material(current) { shader = maskable };
                AssetDatabase.CreateAsset(copy, copyPath);
            }
            text.fontSharedMaterial = copy;
            EditorUtility.SetDirty(text);
            changed++;
        }

        report.Add(changed > 0 ? $"Dropdowns: fixed clipping on {changed} lists." : "Dropdowns: already clip correctly.");
        return changed > 0;
    }

    static void SetPlayerSettings(List<string> report)
    {
        PlayerSettings.resizableWindow = true;
        PlayerSettings.defaultIsNativeResolution = true;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        report.Add("Player: fullscreen at the monitor's size, resizable 1280 x 720 window.");
    }
}
