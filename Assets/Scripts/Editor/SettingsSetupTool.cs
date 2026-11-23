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
// - gives the game font a shader that masks can clip (its old shader ignored masks, so lists
//   spilled out of their boxes and logged "doesn't have _Stencil property");
// - allows window resizing and sets a sensible first window size.
public static class SettingsSetupTool
{
    const string FontPath = "Assets/TextMesh Pro/Fonts/atop-font/Atop-R99O3 SDF.asset";
    const string DropdownMaterialPath = "Assets/TextMesh Pro/Fonts/atop-font/Atop-R99O3 SDF Dropdown.mat";
    // Draws exactly like the old GUI/Text Shader (colour x the atlas's alpha), but supports masks.
    const string MaskableShader = "TextMeshPro/Mobile/Bitmap";
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

        FixFontMasking(report);

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

    static void FixFontMasking(List<string> report)
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Shader maskable = Shader.Find(MaskableShader);
        if (font == null || font.material == null || maskable == null)
        {
            report.Add("Font: couldn't find the Atop font or the TextMeshPro bitmap shader.");
            return;
        }

        // Every text using the font shares this material. The dropdown lists have their own copy,
        // made by the first version of this tool with a shader that turned their text black.
        var materials = new List<Material> { font.material };
        var dropdownCopy = AssetDatabase.LoadAssetAtPath<Material>(DropdownMaterialPath);
        if (dropdownCopy != null)
            materials.Add(dropdownCopy);

        int changed = 0;
        foreach (Material material in materials)
        {
            if (material.shader == maskable)
                continue;
            material.shader = maskable;
            EditorUtility.SetDirty(material);
            changed++;
        }
        report.Add(changed > 0 ? $"Font: masks now work ({changed} materials fixed)." : "Font: masks already work.");
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