using System;
using System.Linq;
using System.Text.RegularExpressions;
using Artngame.PANGAEA.Rockgen.Unity;
using UnityEngine;
using static UnityEngine.GUILayout;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

//v0.1
using Artngame.CommonTools.UnityFBXExporter;

namespace Artngame.PANGAEA.RockGen.Unity
{
public static class RockGeneratorGUI
{
    static readonly int MAX_LABEL_WIDTH;
    static readonly int CHARACTER_WIDTH;

    static readonly Vector3 DEFAULT_POS = new Vector3(2.5f, 2.5f, 2.5f);

    static RockGeneratorGUI()
    {
        CHARACTER_WIDTH = 7;

        var settings = new RockGenerationSettings();

        MAX_LABEL_WIDTH = new[] {
            nameof(settings.StockDensity),
            nameof(settings.TargetTriangleCount),
            nameof(settings.Distortion),
        }.Max(n => n.Length) * CHARACTER_WIDTH;
    }

    public static event Action<string> fileExported;

    static Quaternion rotation = Quaternion.identity;

    public static bool OnGUI(RockGenerator generator)
    {
        var groupStyle  = GUI.skin.box;
        var newSettings = new RockGenerationSettings(generator.Settings);

        using (var vs = new VerticalScope(groupStyle))
        {
            newSettings.StockDensity = SettingSlider(nameof(newSettings.StockDensity),
                                                     newSettings.StockDensity, 2, 16, "N1");
            newSettings.TargetTriangleCount = Mathf.RoundToInt(
                SettingSlider(nameof(newSettings.TargetTriangleCount),
                              newSettings.TargetTriangleCount, 50, 2000,
                              "N0",
                              logScale: true
                )
            );
        }

        using (var vs = new VerticalScope(groupStyle))
        {
            using (var gcs = new GUIChangedScope())
            {
                var Uniformity = Mathf.Clamp01(1 - newSettings.GridSettings.Randomness);
                Uniformity = SettingSlider(nameof(Uniformity), Uniformity, 0, 1);

                if (GUI.changed)
                {
                    newSettings.GridSettings = new VoronoiGridSettings(newSettings.GridSettings) {
                        Randomness = 1 - Uniformity
                    };
                }
            }

            newSettings.Distortion = SettingSlider(nameof(newSettings.Distortion),
                                                   newSettings.Distortion, -1, 1);
            newSettings.PatternSize = SettingSlider(nameof(newSettings.PatternSize),
                                                    newSettings.PatternSize, .5f, 2.5f);
        }

        using (var vs = new VerticalScope(groupStyle))
        using (var gcs = new GUIChangedScope())
        {
            var scale = newSettings.Scale;
            Label("Shape:");
            var newX = SettingSlider("X", Mathf.Clamp((float) scale.X, .5f, 2), 0.5f, 2);
            var newY = SettingSlider("Y", Mathf.Clamp((float) scale.Y, .5f, 2), 0.5f, 2);
            var newZ = (float) scale.Z;

            if (GUI.changed)
            {
                var m = UnityEngine.Matrix4x4.TRS(DEFAULT_POS,
                                                  rotation,
                                                  new Vector3(newX, newY, newZ));
                newSettings.Transform = Convert.FromUnityMatrix(m);
            }
        }

        if (Button("Make Rock"))
        {
            var newGridSettings = new VoronoiGridSettings(newSettings.GridSettings);
            newGridSettings.Randomness += 1e-6f;

            newSettings.Transform = Convert.FromUnityMatrix(
                UnityEngine.Matrix4x4.TRS(DEFAULT_POS + Random.insideUnitSphere,
                                          rotation = Random.rotation,
                                          Convert.ToVector3(newSettings.Scale))
            );

            newSettings.GridSettings = newGridSettings;
        }

        var settingsChanged = GUI.changed;
        if (settingsChanged)
            generator.Settings = newSettings;

        if (Button("Save this one"))
        {
                //var path = FileSaver.SaveMesh(generator.LatestMesh, "rock");
                // OnFileExported(path);


                //v0.1
#if UNITY_EDITOR
                string newPath = GetNewPath(generator.unityMeshFilter.gameObject, null);

                if (newPath != null && newPath.Length != 0)
                {
                    bool isSuccess = FBXExporter.ExportGameObjToFBX(generator.unityMeshFilter.gameObject, newPath, false, false);

                    if (isSuccess)
                    {
                        return true;// newPath;
                    }
                    else
                    {
                        //EditorUtility.DisplayDialog("Warning", "The extension probably wasn't an FBX file, could not export.", "Okay");
                    }
                }
#endif
                //END v0.1

            }

        return settingsChanged;
    }


        //v0.1
#if UNITY_EDITOR
        private static string GetNewPath(GameObject gameObject, string oldPath = null)
        {
            // NOTE: This must return a path with the starting "Assets/" or else textures won't copy right

            string name = gameObject.name;

            string newPath = null;
            if (oldPath == null)
                newPath = EditorUtility.SaveFilePanelInProject("Export FBX File", name + ".fbx", "fbx", "Export " + name + " GameObject to a FBX file");
            else
            {
                if (oldPath.StartsWith("/Assets"))
                {
                    oldPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"), 7) + oldPath;
                    oldPath = oldPath.Remove(oldPath.LastIndexOf('/'), oldPath.Length - oldPath.LastIndexOf('/'));
                }
                newPath = EditorUtility.SaveFilePanel("Export FBX File", oldPath, name + ".fbx", "fbx");
            }

            int assetsIndex = newPath.IndexOf("Assets");

            if (assetsIndex < 0)
                return null;

            if (assetsIndex > 0)
                newPath = newPath.Remove(0, assetsIndex);

            return newPath;
        }
#endif


        static float SettingSlider(string varName,
                               float  value, float min, float max,
                               string format    = "N",
                               bool   autoLabel = true,
                               bool   logScale  = false)
    {
        BeginHorizontal();
        if (autoLabel)
        {
            var label = Regex.Replace(varName, @"([A-Z]+?)", " $1");
            Label(label, MaxWidth(MAX_LABEL_WIDTH));
        }

        if (logScale)
        {
            value = Mathf.Log(value);
            min   = Mathf.Log(min);
            max   = Mathf.Log(max);
        }

        float newVal = HorizontalSlider(value, min, max, ExpandWidth(true));

        if (logScale)
        {
            newVal = Mathf.Exp(newVal);
        }

        Label(newVal.ToString(format), Width(5 * CHARACTER_WIDTH));
        EndHorizontal();
        return newVal;
    }

    static void OnFileExported(string path)
    {
        fileExported?.Invoke(path);
    }
}
}
