using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamite3D.RealIvy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Bakes every Real Ivy in the open scenes into a mesh asset stored next to its prefab,
// then strips the editable Real Ivy data so it stops bloating the scene file.
public static class IvyBakeTool
{
    const string FallbackFolder = "Assets/Level Assets/IvyAsset";

    [MenuItem("Tools/PondBound/Bake All Ivy Into Prefabs")]
    static void BakeAllIvy()
    {
        IvyInfo[] ivies = Object.FindObjectsByType<IvyInfo>(FindObjectsInactive.Include);
        if (ivies.Length == 0)
        {
            EditorUtility.DisplayDialog("Bake Ivy", "No Real Ivy objects found in the open scenes.", "OK");
            return;
        }

        bool go = EditorUtility.DisplayDialog("Bake Ivy",
            "Bake " + ivies.Length + " ivy objects into mesh assets and remove their Real Ivy editing data?\n\n" +
            "They will no longer be editable in the Real Ivy window. Back up the scene first.",
            "Bake", "Cancel");
        if (!go) return;

        // Instances that share one prefab can't each write their own mesh into it, so unpack those first.
        foreach (var group in ivies.Where(PrefabUtility.IsPartOfPrefabInstance)
                                   .GroupBy(i => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(i.gameObject))
                                   .Where(g => g.Count() > 1))
        {
            foreach (IvyInfo shared in group)
            {
                GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(shared.gameObject);
                PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        var leftovers = new List<Object>();
        var instanceRoots = new List<GameObject>();
        var emptyIvies = new List<GameObject>();
        int baked = 0;

        try
        {
            for (int i = 0; i < ivies.Length; i++)
            {
                IvyInfo info = ivies[i];
                GameObject ivyGO = info.gameObject;
                EditorUtility.DisplayProgressBar("Bake Ivy", ivyGO.name + " (" + (i + 1) + "/" + ivies.Length + ")", (float)i / ivies.Length);

                MeshFilter meshFilter = ivyGO.GetComponent<MeshFilter>();
                UnityEngine.Mesh sceneMesh = meshFilter != null ? meshFilter.sharedMesh : null;
                CollectLeftovers(info, sceneMesh, leftovers);

                bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(ivyGO);
                string prefabPath = isPrefab ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(ivyGO) : null;
                string folder = isPrefab ? Path.GetDirectoryName(prefabPath).Replace('\\', '/') : FallbackFolder;
                string baseName = isPrefab ? Path.GetFileNameWithoutExtension(prefabPath) : ivyGO.name;

                // 1. Save the generated mesh as its own asset.
                UnityEngine.Mesh meshAsset = null;
                if (sceneMesh != null && EditorUtility.IsPersistent(sceneMesh))
                {
                    meshAsset = sceneMesh;
                }
                else if (sceneMesh != null)
                {
                    EnsureFolder(folder);
                    meshAsset = Object.Instantiate(sceneMesh);
                    meshAsset.name = baseName + " Mesh";
                    AssetDatabase.CreateAsset(meshAsset, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + baseName + " Mesh.asset"));
                }
                else
                {
                    emptyIvies.Add(ivyGO);
                }

                if (isPrefab)
                {
                    // 2. Drop the scene overrides that point at scene-only data.
                    GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(ivyGO);
                    string relativePath = RelativePath(instanceRoot.transform, ivyGO.transform);
                    if (meshFilter != null) PrefabUtility.RevertObjectOverride(meshFilter, InteractionMode.AutomatedAction);
                    PrefabUtility.RevertObjectOverride(info, InteractionMode.AutomatedAction);

                    // 3. Put the mesh into the prefab and remove the Real Ivy component from it.
                    GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
                    Transform target = relativePath.Length == 0 ? contents.transform : contents.transform.Find(relativePath);
                    MeshFilter prefabFilter = target != null ? target.GetComponent<MeshFilter>() : null;
                    if (prefabFilter != null) prefabFilter.sharedMesh = meshAsset;
                    foreach (IvyInfo prefabInfo in contents.GetComponentsInChildren<IvyInfo>(true))
                        Object.DestroyImmediate(prefabInfo);
                    PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                    PrefabUtility.UnloadPrefabContents(contents);

                    instanceRoots.Add(instanceRoot);
                }
                else
                {
                    if (meshFilter != null)
                    {
                        Undo.RecordObject(meshFilter, "Bake Ivy");
                        meshFilter.sharedMesh = meshAsset;
                    }
                    Undo.DestroyObjectImmediate(info);
                }

                EditorSceneManager.MarkSceneDirty(ivyGO.scene);
                baked++;
            }

            // 4. Clear overrides left pointing at the removed component, then free the scene-only data.
            if (instanceRoots.Count > 0)
                PrefabUtility.RemoveUnusedOverrides(instanceRoots.ToArray(), InteractionMode.AutomatedAction);

            foreach (Object leftover in leftovers.Distinct())
            {
                if (leftover != null && !EditorUtility.IsPersistent(leftover))
                    Object.DestroyImmediate(leftover);
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        foreach (GameObject empty in emptyIvies)
            Debug.LogWarning("[Bake Ivy] '" + empty.name + "' has no mesh (an empty ivy). You can delete it.", empty);

        Debug.Log("[Bake Ivy] Baked " + baked + " ivy objects, " + emptyIvies.Count + " were empty. Save the scene now.");
        EditorUtility.DisplayDialog("Bake Ivy",
            "Baked " + baked + " ivy objects (" + emptyIvies.Count + " empty, see Console).\n\nSave the scene now (Ctrl+S).", "OK");
    }

    static void CollectLeftovers(IvyInfo info, UnityEngine.Mesh sceneMesh, List<Object> leftovers)
    {
        if (sceneMesh != null) leftovers.Add(sceneMesh);

        InfoPool pool = info.infoPool;
        if (pool == null) return;
        leftovers.Add(pool);

        if (pool.ivyContainer != null)
        {
            leftovers.Add(pool.ivyContainer);
            if (pool.ivyContainer.branches != null)
                leftovers.AddRange(pool.ivyContainer.branches.Where(b => b != null).Cast<Object>());
        }
        if (pool.meshBuilder != null)
        {
            leftovers.Add(pool.meshBuilder);
            if (pool.meshBuilder.ivyMesh != null) leftovers.Add(pool.meshBuilder.ivyMesh);
        }
        if (pool.growth != null) leftovers.Add(pool.growth);
    }

    static string RelativePath(Transform root, Transform child)
    {
        var parts = new List<string>();
        for (Transform t = child; t != null && t != root; t = t.parent)
            parts.Insert(0, t.name);
        return string.Join("/", parts);
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }
}