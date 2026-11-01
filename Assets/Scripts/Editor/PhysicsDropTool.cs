using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Lets selected objects fall and settle live in the Scene view, without entering Play Mode.
// Select them and press Ctrl+Shift+D (Tools > Physics Drop). The drop ends by itself once they are
// at rest, or press it again to end it early. One Undo reverses the whole drop.
public static class PhysicsDropTool
{
    const string MenuPath = "Tools/Physics Drop %#d";
    const string UndoName = "Physics Drop";
    const float MaxSeconds = 12f;

    class Drop
    {
        public Transform root;
        public Rigidbody body;
        public bool addedBody;
        public bool wasKinematic;
        public bool usedGravity;
        public CollisionDetectionMode detection;
        public Vector3 startPosition;
        public Quaternion startRotation;
        public bool missed;
    }

    static readonly List<Drop> drops = new List<Drop>();
    static readonly List<Collider> tempColliders = new List<Collider>();
    static readonly List<MeshCollider> madeConvex = new List<MeshCollider>();
    static readonly List<Rigidbody> frozen = new List<Rigidbody>();
    static SimulationMode savedMode;
    static bool running;
    static double lastTime;
    static float pending;
    static float elapsed;
    static float killHeight;

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        if (running) Stop();
        else Begin(Selection.transforms);
    }

    [MenuItem(MenuPath, true)]
    static bool CanToggle()
    {
        return running || (Selection.transforms.Length > 0 && !EditorApplication.isPlayingOrWillChangePlaymode);
    }

    // Anything that could save the scene, reload scripts or leave edit mode ends the drop first.
    [InitializeOnLoadMethod]
    static void Hook()
    {
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        EditorApplication.quitting += Stop;
        EditorApplication.playModeStateChanged += state => Stop();
        EditorSceneManager.sceneSaving += (scene, path) => Stop();
        EditorSceneManager.sceneClosing += (scene, removing) => Stop();
    }

    static void Begin(Transform[] roots)
    {
        running = true;
        savedMode = UnityEngine.Physics.simulationMode;
        UnityEngine.Physics.simulationMode = SimulationMode.Script;

        // A simulation step moves the whole scene, so hold every other moving body still.
        foreach (Rigidbody body in Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude))
        {
            if (!body.isKinematic && System.Array.IndexOf(roots, body.transform) < 0)
            {
                body.isKinematic = true;
                frozen.Add(body);
            }
        }

        // Meshes without a collider would let things fall through, so give them a temporary one.
        foreach (MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude))
        {
            if (filter.GetComponent<Collider>() == null && IsSolidMesh(filter) && !IsUnder(filter.transform, roots))
                tempColliders.Add(AddTempCollider(filter, false));
        }

        foreach (Transform root in roots)
            AddDrop(root);

        // Anything this far below the lowest collider has missed everything.
        UnityEngine.Physics.SyncTransforms();
        killHeight = float.MaxValue;
        foreach (Collider collider in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude))
            killHeight = Mathf.Min(killHeight, collider.bounds.min.y);
        killHeight -= 5f;

        lastTime = EditorApplication.timeSinceStartup;
        pending = 0f;
        elapsed = 0f;
        EditorApplication.update += Tick;
    }

    static void AddDrop(Transform root)
    {
        bool solid = false;
        foreach (Collider collider in root.GetComponentsInChildren<Collider>())
        {
            if (collider.isTrigger) continue;
            solid = true;

            // A moving body can only use a convex mesh collider.
            MeshCollider mesh = collider as MeshCollider;
            if (mesh != null && !mesh.convex)
            {
                mesh.convex = true;
                madeConvex.Add(mesh);
            }
        }

        if (!solid)
        {
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                if (!IsSolidMesh(filter)) continue;
                tempColliders.Add(AddTempCollider(filter, true));
                solid = true;
            }
        }

        if (!solid)
        {
            Debug.LogWarning("Physics Drop: " + root.name + " has no collider or mesh, so it was skipped.", root);
            return;
        }

        Drop drop = new Drop { root = root, startPosition = root.position, startRotation = root.rotation };
        drop.body = root.GetComponent<Rigidbody>();
        drop.addedBody = drop.body == null;
        if (drop.addedBody)
        {
            drop.body = root.gameObject.AddComponent<Rigidbody>();
            drop.body.hideFlags = HideFlags.HideAndDontSave;
        }

        drop.wasKinematic = drop.body.isKinematic;
        drop.usedGravity = drop.body.useGravity;
        drop.detection = drop.body.collisionDetectionMode;
        drop.body.isKinematic = false;
        drop.body.useGravity = true;

        // Stops small, fast objects passing through thin surfaces.
        drop.body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        drop.body.WakeUp();
        drops.Add(drop);
    }

    static void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        pending += Mathf.Min((float)(now - lastTime), 0.1f);
        lastTime = now;

        float step = Time.fixedDeltaTime;
        while (pending >= step)
        {
            UnityEngine.Physics.Simulate(step);
            pending -= step;
            elapsed += step;
        }

        bool moving = false;
        foreach (Drop drop in drops)
        {
            if (drop.root == null || drop.body == null || drop.missed) continue;
            if (drop.body.position.y < killHeight)
            {
                drop.missed = true;
                drop.body.collisionDetectionMode = CollisionDetectionMode.Discrete;
                drop.body.isKinematic = true;
            }
            else if (!drop.body.IsSleeping())
            {
                moving = true;
            }
        }

        SceneView.RepaintAll();
        if (!moving || elapsed >= MaxSeconds) Stop();
    }

    static void Stop()
    {
        if (!running) return;
        running = false;
        EditorApplication.update -= Tick;

        foreach (Collider collider in tempColliders)
        {
            if (collider != null) Object.DestroyImmediate(collider);
        }
        tempColliders.Clear();

        foreach (MeshCollider mesh in madeConvex)
        {
            if (mesh != null) mesh.convex = false;
        }
        madeConvex.Clear();

        foreach (Rigidbody body in frozen)
        {
            if (body != null) body.isKinematic = false;
        }
        frozen.Clear();

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        foreach (Drop drop in drops)
        {
            if (drop.root == null) continue;
            Vector3 endPosition = drop.root.position;
            Quaternion endRotation = drop.root.rotation;
            RestoreBody(drop);

            // Back to the start first, so the recorded change is the whole fall and one Undo reverses it.
            drop.root.SetPositionAndRotation(drop.startPosition, drop.startRotation);
            if (drop.missed)
            {
                Debug.LogWarning("Physics Drop: " + drop.root.name + " fell past everything, so it was put back.", drop.root);
                continue;
            }

            Undo.RecordObject(drop.root, UndoName);
            drop.root.SetPositionAndRotation(endPosition, endRotation);
            PrefabUtility.RecordPrefabInstancePropertyModifications(drop.root);
        }
        Undo.FlushUndoRecordObjects();
        drops.Clear();

        UnityEngine.Physics.simulationMode = savedMode;
        SceneView.RepaintAll();
    }

    static void RestoreBody(Drop drop)
    {
        if (drop.body == null) return;
        if (drop.addedBody)
        {
            Object.DestroyImmediate(drop.body);
            return;
        }

        if (!drop.body.isKinematic)
        {
            drop.body.linearVelocity = Vector3.zero;
            drop.body.angularVelocity = Vector3.zero;
        }
        drop.body.collisionDetectionMode = drop.detection;
        drop.body.useGravity = drop.usedGravity;
        drop.body.isKinematic = drop.wasKinematic;
    }

    static MeshCollider AddTempCollider(MeshFilter filter, bool convex)
    {
        MeshCollider collider = filter.gameObject.AddComponent<MeshCollider>();
        collider.hideFlags = HideFlags.HideAndDontSave;
        collider.convex = convex;
        collider.sharedMesh = filter.sharedMesh;
        return collider;
    }

    // Skips empty filters, hidden renderers and the lower-detail copies in an LOD group.
    static bool IsSolidMesh(MeshFilter filter)
    {
        MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
        if (filter.sharedMesh == null || renderer == null || !renderer.enabled) return false;

        LODGroup group = filter.GetComponentInParent<LODGroup>();
        if (group == null) return true;
        LOD[] lods = group.GetLODs();
        return lods.Length == 0 || System.Array.IndexOf(lods[0].renderers, renderer) >= 0;
    }

    static bool IsUnder(Transform transform, Transform[] roots)
    {
        foreach (Transform root in roots)
        {
            if (transform.IsChildOf(root)) return true;
        }
        return false;
    }
}