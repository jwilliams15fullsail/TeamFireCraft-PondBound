using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Tools > PondBound > Optimize Performance (PC + Web). One click, safe to run again.
// It doesn't bake occlusion culling: every bake tested on this level hid objects that were in view.
// 1. River Studio's depth camera re-renders only when the player has moved 15 m (it was every frame).
// 2. Vent smoke stops casting shadows and pauses while off screen.
// 3. GPU Resident Drawer off (it gained nothing here) and its extra shader variants stripped.
// 4. Windows uses Direct3D 11 first (Direct3D 12 crashed the GPU in this level). Restart the Editor after.
public static class OptimizePerformanceTool
{
    const string LevelPath = "Assets/Scenes/LabLevelShowcase.unity";
    const string DepthRendererType = "DepthRendererCT_URP";
    const float DepthUpdateMetres = 15f;

    [MenuItem("Tools/PondBound/Optimize Performance (PC + Web)")]
    static void RunMenu()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Optimize Performance: stop Play mode first.");
            return;
        }
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            Debug.LogError("Optimize Performance: save and leave Prefab Mode first.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        Run(LevelPath, true);
    }

    public static void Run(string levelPath, bool setGraphicsApi)
    {
        var report = new List<string>();
        Scene scene = EditorSceneManager.OpenScene(levelPath, OpenSceneMode.Single);

        ThrottleDepthRenderers(scene, report);
        CheapenSmoke(scene, report);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        DisableResidentDrawer(report);
        if (setGraphicsApi)
            PreferDirect3D11(report);
        AssetDatabase.SaveAssets();

        Debug.Log("Optimize Performance finished:\n" + string.Join("\n", report));
    }

    static T[] InScene<T>(Scene scene) where T : Component =>
        scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();

    static void Keep(Object changed)
    {
        EditorUtility.SetDirty(changed);
        if (PrefabUtility.IsPartOfPrefabInstance(changed))
            PrefabUtility.RecordPrefabInstancePropertyModifications(changed);
    }

    static void ThrottleDepthRenderers(Scene scene, List<string> report)
    {
        int found = 0;
        foreach (MonoBehaviour script in InScene<MonoBehaviour>(scene))
        {
            if (script == null || script.GetType().Name != DepthRendererType)
                continue;
            var data = new SerializedObject(script);
            SerializedProperty perDistance = data.FindProperty("updatePerDistance");
            SerializedProperty distance = data.FindProperty("updateDistance");
            SerializedProperty previous = data.FindProperty("previousPos");
            if (perDistance == null || distance == null || previous == null)
            {
                report.Add($"Depth camera: '{script.name}' has different fields, left as it is.");
                continue;
            }
            perDistance.boolValue = true;
            distance.floatValue = DepthUpdateMetres;
            // Far away, so it always renders once when the level starts.
            previous.vector3Value = new Vector3(100000f, 0f, 0f);
            data.ApplyModifiedPropertiesWithoutUndo();
            Keep(script);
            found++;
        }
        report.Add($"Depth camera: {found} now update every {DepthUpdateMetres} m instead of every frame.");
    }

    static void CheapenSmoke(Scene scene, List<string> report)
    {
        int count = 0;
        foreach (ParticleSystem system in InScene<ParticleSystem>(scene))
        {
            bool changed = false;
            if (system.TryGetComponent(out ParticleSystemRenderer renderer) && renderer.shadowCastingMode != ShadowCastingMode.Off)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                Keep(renderer);
                changed = true;
            }
            ParticleSystem.MainModule main = system.main;
            if (main.cullingMode == ParticleSystemCullingMode.AlwaysSimulate)
            {
                main.cullingMode = ParticleSystemCullingMode.PauseAndCatchup;
                Keep(system);
                changed = true;
            }
            if (changed)
                count++;
        }
        report.Add($"Particles: made {count} systems cheaper.");
    }

    static void DisableResidentDrawer(List<string> report)
    {
        var pipelines = new HashSet<UniversalRenderPipelineAsset>();
        if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset main)
            pipelines.Add(main);
        for (int i = 0; i < QualitySettings.count; i++)
            if (QualitySettings.GetRenderPipelineAssetAt(i) is UniversalRenderPipelineAsset asset)
                pipelines.Add(asset);

        foreach (UniversalRenderPipelineAsset asset in pipelines)
        {
            if (asset.gpuResidentDrawerMode == GPUResidentDrawerMode.Disabled)
                continue;
            asset.gpuResidentDrawerMode = GPUResidentDrawerMode.Disabled;
            EditorUtility.SetDirty(asset);
            report.Add($"{asset.name}: GPU Resident Drawer off.");
        }

        // BatchRendererGroup variants: 1 = Strip All. Only the Resident Drawer and Entities Graphics use them.
        var settings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
        SerializedProperty stripping = settings.FindProperty("m_BrgStripping");
        if (stripping != null && stripping.intValue != 1)
        {
            stripping.intValue = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            report.Add("Shaders: BatchRendererGroup variants stripped (faster builds, fewer variants).");
        }
    }

    static void PreferDirect3D11(List<string> report)
    {
        var wanted = new[] { GraphicsDeviceType.Direct3D11, GraphicsDeviceType.Direct3D12 };
        const BuildTarget target = BuildTarget.StandaloneWindows64;
        if (!PlayerSettings.GetUseDefaultGraphicsAPIs(target) && PlayerSettings.GetGraphicsAPIs(target).SequenceEqual(wanted))
        {
            report.Add("Windows: already Direct3D 11 first.");
            return;
        }
        PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
        PlayerSettings.SetGraphicsAPIs(target, wanted);
        report.Add("Windows: Direct3D 11 first. RESTART the Editor to use it.");
    }
}
