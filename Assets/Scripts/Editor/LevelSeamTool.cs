using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Level Seam Tool
// Snaps walls / floors / ceilings flush (with a tiny overlap) and finds cracks between pieces.
// Open from: Tools > Level Seam Tool > Open Window
public class LevelSeamTool : EditorWindow
{
    const string Prefix = "LevelSeamTool.";
    const float Touch = 0.0005f;       // gaps smaller than this count as "touching"
    const float ThinSize = 0.01f;      // pieces thinner than this on an axis are flat planes on that axis
    const int MaxPieceRenderers = 24;  // a "piece" root with more renderers than this is treated as a group, not a piece
    const int MaxSeams = 500;
    const int MaxListRows = 200;

    // Unreal-style collision meshes that ship as visible MeshRenderers.
    static readonly string[] CollisionPrefixes = { "UCX_", "UBX_", "UCP_", "USP_", "UMESH_" };

    // ---------------- Settings (stored in EditorPrefs so the hotkeys use them too) ----------------

    static float GetF(string key, float def) => EditorPrefs.GetFloat(Prefix + key, def);
    static void SetF(string key, float v) { if (!EditorPrefs.HasKey(Prefix + key) || EditorPrefs.GetFloat(Prefix + key) != v) EditorPrefs.SetFloat(Prefix + key, v); }
    static bool GetB(string key, bool def) => EditorPrefs.GetBool(Prefix + key, def);
    static void SetB(string key, bool v) { if (!EditorPrefs.HasKey(Prefix + key) || EditorPrefs.GetBool(Prefix + key) != v) EditorPrefs.SetBool(Prefix + key, v); }

    static float Overlap { get => GetF("Overlap", 0.01f); set => SetF("Overlap", Mathf.Max(0f, value)); }
    static float SnapDistance { get => GetF("SnapDistance", 0.25f); set => SetF("SnapDistance", Mathf.Max(0.001f, value)); }
    static float MinContact { get => GetF("MinContact", 0.02f); set => SetF("MinContact", Mathf.Max(0f, value)); }
    static float EdgeTolerance { get => GetF("EdgeTolerance", 0.05f); set => SetF("EdgeTolerance", Mathf.Max(0f, value)); }
    static float MinPieceSize { get => GetF("MinPieceSize", 0.3f); set => SetF("MinPieceSize", Mathf.Max(0f, value)); }
    static bool StaticOnly { get => GetB("StaticOnly", true); set => SetB("StaticOnly", value); }
    static bool StructuralOnly { get => GetB("StructuralOnly", true); set => SetB("StructuralOnly", value); }
    static string StructuralNames
    {
        get => EditorPrefs.GetString(Prefix + "StructuralNames", "wall,ceil,floor,panel,gate,door,window,beam,pillar,column");
        set { if (EditorPrefs.GetString(Prefix + "StructuralNames", "") != value) EditorPrefs.SetString(Prefix + "StructuralNames", value); }
    }
    static float GroupSnapDistance { get => GetF("GroupSnapDistance", 1f); set => SetF("GroupSnapDistance", Mathf.Max(0.001f, value)); }
    static float GridSize { get => GetF("GridSize", 0.5f); set => SetF("GridSize", Mathf.Max(0.001f, value)); }
    static float RotationStep { get => GetF("RotationStep", 90f); set => SetF("RotationStep", Mathf.Clamp(value, 0.1f, 180f)); }
    static float SeamMaxGap { get => GetF("SeamMaxGap", 0.05f); set => SetF("SeamMaxGap", Mathf.Max(0.001f, value)); }
    static bool FlagTouching { get => GetB("FlagTouching", false); set => SetB("FlagTouching", value); }
    static bool ShowSeams { get => GetB("ShowSeams", true); set => SetB("ShowSeams", value); }
    static float LabelDistance { get => GetF("LabelDistance", 20f); set => SetF("LabelDistance", Mathf.Max(0f, value)); }

    static event System.Action GeometryMoved;

    class Piece
    {
        public Transform root;
        public Bounds bounds;
    }

    class Seam
    {
        public Transform low;    // piece on the negative side of the gap
        public Transform high;   // piece on the positive side of the gap
        public Bounds lowBounds;
        public Bounds highBounds;
        public int axis;
        public float gap;
        public Vector3 center;
        public Vector3 size;
    }

    readonly List<Seam> seams = new List<Seam>();
    Vector2 scroll;
    bool scanned;

    // ---------------- Menu items / hotkeys ----------------

    [MenuItem("Tools/Level Seam Tool/Open Window")]
    static void Open() => GetWindow<LevelSeamTool>("Seam Tool");

    [MenuItem("Tools/Level Seam Tool/Snap To Nearest Gap #&s")]      // Shift+Alt+S
    static void SnapNearestMenu() => SnapSelection(false);

    [MenuItem("Tools/Level Seam Tool/Seal All Sides #&d")]           // Shift+Alt+D
    static void SealAllMenu() => SnapSelection(true);

    [MenuItem("Tools/Level Seam Tool/Snap Selected Together #&g")]   // Shift+Alt+G
    static void SnapTogetherMenu() => SnapSelectionTogether();

    [MenuItem("Tools/Level Seam Tool/Snap To Nearest Gap #&s", true)]
    [MenuItem("Tools/Level Seam Tool/Seal All Sides #&d", true)]
    static bool HasSelection() => Selection.transforms.Length > 0;

    [MenuItem("Tools/Level Seam Tool/Snap Selected Together #&g", true)]
    static bool HasMultiSelection() => SelectedPieces().Count > 1;

    // ---------------- Window lifecycle ----------------

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        GeometryMoved += OnGeometryMoved;
        Undo.undoRedoPerformed += OnGeometryMoved;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        GeometryMoved -= OnGeometryMoved;
        Undo.undoRedoPerformed -= OnGeometryMoved;
    }

    void OnSelectionChange() => Repaint();

    void OnGeometryMoved()
    {
        if (scanned) Scan();
        Repaint();
        SceneView.RepaintAll();
    }

    // ---------------- GUI ----------------

    void OnGUI()
    {
        var selPieces = SelectedPieces();

        EditorGUILayout.LabelField("Shared", EditorStyles.boldLabel);
        Overlap = EditorGUILayout.FloatField(new GUIContent("Overlap (m)", "How far a piece is pushed INTO its neighbour. 0.01-0.02 hides hairline cracks and light leaks."), Overlap);
        MinContact = EditorGUILayout.FloatField(new GUIContent("Min Contact (m)", "Faces must overlap at least this much side-to-side (on one axis) to count as neighbours."), MinContact);
        EdgeTolerance = EditorGUILayout.FloatField(new GUIContent("Edge Tolerance (m)", "Pieces that only meet at an edge (like a ceiling over a paper-thin wall) still count if they're this close."), EdgeTolerance);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Snap Selected Together", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select 2+ pieces. The ACTIVE one (clicked last) stays still; every other piece closes its gaps onto the nearest piece already placed.", MessageType.None);
        GroupSnapDistance = EditorGUILayout.FloatField(new GUIContent("Group Snap Distance (m)", "Largest gap closed between selected pieces."), GroupSnapDistance);

        using (new EditorGUI.DisabledScope(selPieces.Count < 2))
        {
            string anchorName = selPieces.Count >= 2 ? ResolveAnchor(selPieces).name : "-";
            if (GUILayout.Button($"Snap Selected Together   (Shift+Alt+G)\nanchor: {anchorName}", GUILayout.Height(36))) SnapSelectionTogether();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Snap To Level", EditorStyles.boldLabel);
        SnapDistance = EditorGUILayout.FloatField(new GUIContent("Snap Distance (m)", "Largest gap the level snap will close."), SnapDistance);
        MinPieceSize = EditorGUILayout.FloatField(new GUIContent("Min Piece Size (m)", "Ignore pieces smaller than this (props, decals, small trims)."), MinPieceSize);
        StaticOnly = EditorGUILayout.Toggle(new GUIContent("Static Objects Only", "Only snap against / scan objects marked Static. Turn off if your walls aren't static."), StaticOnly);
        StructuralOnly = EditorGUILayout.Toggle(new GUIContent("Structural Pieces Only", "Only walls/floors/ceilings etc. (by name) count for level snap and the seam scan, so props like cabinets are ignored. Snap Selected Together ignores this."), StructuralOnly);
        using (new EditorGUI.DisabledScope(!StructuralOnly))
            StructuralNames = EditorGUILayout.TextField(new GUIContent("Structural Names", "Comma-separated words. A piece counts if its name, or one of its mesh objects' names, contains any of them."), StructuralNames);

        using (new EditorGUI.DisabledScope(selPieces.Count == 0))
        {
            if (GUILayout.Button("Snap To Nearest Gap   (Shift+Alt+S)")) SnapSelection(false);
            if (GUILayout.Button("Seal All Sides   (Shift+Alt+D)")) SnapSelection(true);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Grid / Rotation", EditorStyles.boldLabel);
        GridSize = EditorGUILayout.FloatField(new GUIContent("Grid Size (m)", "Position rounding snaps the piece's bounds corner (not its pivot) to this grid."), GridSize);
        RotationStep = EditorGUILayout.FloatField(new GUIContent("Rotation Step (deg)", "Rotation rounds each axis to a multiple of this."), RotationStep);

        using (new EditorGUI.DisabledScope(selPieces.Count == 0))
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Round Rotation")) RoundRotation();
            if (GUILayout.Button("Round Position")) RoundPosition();
            if (GUILayout.Button("Round Both")) { RoundRotation(); RoundPosition(); }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Seam Checker", EditorStyles.boldLabel);
        SeamMaxGap = EditorGUILayout.FloatField(new GUIContent("Max Gap (m)", "Report gaps up to this size."), SeamMaxGap);
        FlagTouching = EditorGUILayout.Toggle(new GUIContent("Flag Exact Touching", "Also report faces that touch exactly with no overlap (yellow). These can still show hairline cracks."), FlagTouching);
        ShowSeams = EditorGUILayout.Toggle("Draw In Scene View", ShowSeams);
        LabelDistance = EditorGUILayout.FloatField(new GUIContent("Label Distance (m)", "Only draw gap labels within this distance of the Scene camera."), LabelDistance);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(scanned ? "Rescan" : "Scan Scene")) Scan();
            if (GUILayout.Button("Clear", GUILayout.Width(60))) { seams.Clear(); scanned = false; SceneView.RepaintAll(); }
        }

        if (scanned)
        {
            string capped = seams.Count >= MaxSeams ? $" (capped at {MaxSeams})" : "";
            EditorGUILayout.HelpBox(seams.Count == 0 ? "No seams found." : $"{seams.Count} seam(s) found{capped}. Red = gap, Yellow = touching only.", seams.Count == 0 ? MessageType.Info : MessageType.Warning);
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        int rows = Mathf.Min(seams.Count, MaxListRows);
        for (int i = 0; i < rows; i++)
        {
            var s = seams[i];
            if (s.low == null || s.high == null) continue;

            using (new EditorGUILayout.HorizontalScope())
            {
                var old = GUI.color;
                GUI.color = s.gap <= Touch ? Color.yellow : new Color(1f, 0.55f, 0.55f);
                EditorGUILayout.LabelField($"{s.gap * 100f:0.00} cm  {"XYZ"[s.axis]}   {s.low.name}  <->  {s.high.name}");
                GUI.color = old;

                if (GUILayout.Button("Show", GUILayout.Width(50))) ShowSeam(s);
                if (GUILayout.Button("Fix", GUILayout.Width(40))) { FixSeam(s); break; }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    // ---------------- Snap selected pieces to each other ----------------

    static void SnapSelectionTogether()
    {
        var pieces = SelectedPieces();
        if (pieces.Count < 2)
        {
            Debug.Log("[LevelSeamTool] Select at least two pieces to snap them together.");
            return;
        }

        Transform anchor = ResolveAnchor(pieces);
        if (!TryGetPieceBounds(anchor, out var anchorBounds))
        {
            Debug.LogWarning($"[LevelSeamTool] Anchor '{anchor.name}' has no visible MeshRenderer to snap to.");
            return;
        }

        var placed = new List<Bounds> { anchorBounds };
        var remaining = new List<Transform>();
        var remainingBounds = new List<Bounds>();
        foreach (var p in pieces)
        {
            if (p == anchor) continue;
            if (TryGetPieceBounds(p, out var pb)) { remaining.Add(p); remainingBounds.Add(pb); }
            else Debug.LogWarning($"[LevelSeamTool] '{p.name}' has no visible MeshRenderer, skipped.");
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Snap Selected Together");
        int undoGroup = Undo.GetCurrentGroup();

        float maxDist = GroupSnapDistance;
        float overlap = Overlap;
        int moved = 0, alreadyOk = 0;
        var tooFar = new List<string>();

        // Work outward from the anchor: always handle the piece closest to something already placed.
        while (remaining.Count > 0)
        {
            int bestIndex = 0, bestTarget = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < remaining.Count; i++)
            {
                for (int j = 0; j < placed.Count; j++)
                {
                    float d = BoundsDistance(remainingBounds[i], placed[j]);
                    if (d < bestDist) { bestDist = d; bestIndex = i; bestTarget = j; }
                }
            }

            Transform t = remaining[bestIndex];
            Bounds b = remainingBounds[bestIndex];
            remaining.RemoveAt(bestIndex);
            remainingBounds.RemoveAt(bestIndex);

            int outcome = TryGroupMove(b, placed[bestTarget], maxDist, overlap, out Vector3 move);
            if (outcome == 1)
            {
                Undo.RecordObject(t, "Snap Selected Together");
                t.position += move;
                b.center += move;
                moved++;
            }
            else if (outcome == 2)
            {
                tooFar.Add($"{t.name} ({bestDist * 100f:0.0} cm away)");
            }
            else
            {
                alreadyOk++;
            }

            placed.Add(b);
        }

        Undo.CollapseUndoOperations(undoGroup);

        string msg = $"[LevelSeamTool] Snap Together onto '{anchor.name}': moved {moved}, already sealed {alreadyOk}.";
        if (tooFar.Count > 0) msg += $" Too far apart (over {maxDist} m, raise Group Snap Distance): {string.Join(", ", tooFar)}";
        Debug.Log(msg);

        GeometryMoved?.Invoke();
    }

    // Closes every open gap between b and target, ending with 'overlap' of interpenetration on each closed axis.
    // Returns 0 = nothing to do, 1 = move found, 2 = too far apart.
    static int TryGroupMove(Bounds b, Bounds target, float maxDist, float overlap, out Vector3 move)
    {
        move = Vector3.zero;
        bool any = false;

        for (int axis = 0; axis < 3; axis++)
        {
            // Two flat planes lying in the same plane (e.g. in-line thin walls): never push them apart on that axis.
            if (b.size[axis] < ThinSize && target.size[axis] < ThinSize) continue;

            float plus = target.min[axis] - b.max[axis];    // target sits on the + side
            float minus = b.min[axis] - target.max[axis];   // target sits on the - side
            float sep = Mathf.Max(plus, minus);

            if (sep < -Touch) continue;                     // already overlapping on this axis
            if (sep > maxDist) { move = Vector3.zero; return 2; }

            move[axis] = plus >= minus ? sep + overlap : -(sep + overlap);
            any = true;
        }

        return any ? 1 : 0;
    }

    static Transform ResolveAnchor(List<Transform> pieces)
    {
        var active = Selection.activeTransform;
        if (active != null)
        {
            var ap = PieceRoot(active);
            if (pieces.Contains(ap)) return ap;
            foreach (var p in pieces)
                if (active.IsChildOf(p)) return p;
        }
        return pieces[0];
    }

    static float BoundsDistance(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dy = Mathf.Max(0f, Mathf.Max(a.min.y - b.max.y, b.min.y - a.max.y));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    // ---------------- Snap to level ----------------

    static void SnapSelection(bool allAxes)
    {
        var sel = SelectedPieces();
        if (sel.Count == 0) return;

        var exclude = new HashSet<Transform>(sel);
        var pieces = GatherPieces(exclude);
        var others = new List<Bounds>(pieces.Count);
        foreach (var p in pieces) others.Add(p.bounds);

        int moved = 0;
        foreach (var t in sel)
        {
            if (!TryGetPieceBounds(t, out var b)) continue;

            Vector3 move = FindSnapMove(b, others, allAxes, SnapDistance);
            if (move.sqrMagnitude < 1e-10f) continue;

            Undo.RecordObject(t, allAxes ? "Seal All Sides" : "Snap To Nearest Gap");
            t.position += move;
            moved++;
        }

        if (moved == 0)
            Debug.Log($"[LevelSeamTool] Nothing to snap: no open gap within {SnapDistance} m (check Static Objects Only / Min Piece Size, or use Snap Selected Together).");
        else
            Debug.Log($"[LevelSeamTool] Snapped {moved} piece(s).");

        GeometryMoved?.Invoke();
    }

    static Vector3 FindSnapMove(Bounds b, List<Bounds> others, bool allAxes, float maxDist)
    {
        float overlap = Overlap;
        float minContact = MinContact;
        float edgeTol = EdgeTolerance;

        Vector3 result = Vector3.zero;
        float bestGap = float.MaxValue;
        int bestAxis = -1;
        float bestMove = 0f;

        for (int axis = 0; axis < 3; axis++)
        {
            float gapPlus = float.MaxValue;   // nearest open gap toward +axis
            float gapMinus = float.MaxValue;  // nearest open gap toward -axis
            bool sealedPlus = false;          // already overlapping a neighbour on the + side
            bool sealedMinus = false;         // already overlapping a neighbour on the - side

            foreach (var c in others)
            {
                if (b.size[axis] < ThinSize && c.size[axis] < ThinSize) continue;   // coplanar flat pieces
                if (!AreNeighbours(b, c, axis, minContact, edgeTol)) continue;
                CheckGap(c.min[axis] - b.max[axis], ref gapPlus, ref sealedPlus, maxDist);
                CheckGap(b.min[axis] - c.max[axis], ref gapMinus, ref sealedMinus, maxDist);
            }

            // Never pull a piece off a neighbour it is already sealed against on the other side.
            if (sealedMinus) gapPlus = float.MaxValue;
            if (sealedPlus) gapMinus = float.MaxValue;

            float gap, move;
            if (gapPlus <= gapMinus) { gap = gapPlus; move = gapPlus + overlap; }
            else { gap = gapMinus; move = -(gapMinus + overlap); }

            if (gap == float.MaxValue) continue;

            if (allAxes)
            {
                result[axis] = move;
            }
            else if (gap < bestGap)
            {
                bestGap = gap;
                bestAxis = axis;
                bestMove = move;
            }
        }

        if (!allAxes && bestAxis >= 0) result[bestAxis] = bestMove;
        return result;
    }

    static void CheckGap(float gap, ref float nearest, ref bool sealedSide, float maxDist)
    {
        if (gap < -Touch)
        {
            if (gap > -0.1f) sealedSide = true;   // already pushed into this neighbour
            return;
        }
        if (gap <= maxDist && gap < nearest) nearest = gap;
    }

    // Neighbours across 'axis': on both other axes they must at least meet (within edgeTol),
    // and on at least one of them they must really overlap (>= minContact).
    // This lets a paper-thin wall count as touching the ceiling above it.
    static bool AreNeighbours(Bounds a, Bounds b, int axis, float minContact, float edgeTol)
    {
        bool faceContact = false;
        for (int i = 0; i < 3; i++)
        {
            if (i == axis) continue;
            float o = Mathf.Min(a.max[i], b.max[i]) - Mathf.Max(a.min[i], b.min[i]);
            if (o < -edgeTol) return false;
            if (o >= minContact) faceContact = true;
        }
        return faceContact;
    }

    // ---------------- Rounding ----------------

    static void RoundRotation()
    {
        var sel = SelectedPieces();
        if (sel.Count == 0) return;

        float step = RotationStep;
        Undo.RecordObjects(sel.ToArray(), "Round Rotation");
        foreach (var t in sel)
        {
            Vector3 e = t.eulerAngles;
            e.x = Mathf.Round(e.x / step) * step;
            e.y = Mathf.Round(e.y / step) * step;
            e.z = Mathf.Round(e.z / step) * step;
            t.rotation = Quaternion.Euler(e);
        }
        GeometryMoved?.Invoke();
    }

    static void RoundPosition()
    {
        var sel = SelectedPieces();
        if (sel.Count == 0) return;

        float grid = GridSize;
        Undo.RecordObjects(sel.ToArray(), "Round Position");
        foreach (var t in sel)
        {
            if (TryGetPieceBounds(t, out var b))
            {
                // Snap the bounds corner, so pieces with centred pivots still land on the grid edges.
                Vector3 min = b.min;
                Vector3 snapped = new Vector3(
                    Mathf.Round(min.x / grid) * grid,
                    Mathf.Round(min.y / grid) * grid,
                    Mathf.Round(min.z / grid) * grid);
                t.position += snapped - min;
            }
            else
            {
                Vector3 p = t.position;
                t.position = new Vector3(
                    Mathf.Round(p.x / grid) * grid,
                    Mathf.Round(p.y / grid) * grid,
                    Mathf.Round(p.z / grid) * grid);
            }
        }
        GeometryMoved?.Invoke();
    }

    // ---------------- Seam scan ----------------

    void Scan()
    {
        seams.Clear();
        scanned = true;

        var pieces = GatherPieces(null);
        int n = pieces.Count;

        float maxGap = SeamMaxGap;
        float minContact = MinContact;
        float edgeTol = EdgeTolerance;
        float minGap = FlagTouching ? -Touch : Touch;

        for (int i = 0; i < n && seams.Count < MaxSeams; i++)
        {
            for (int j = i + 1; j < n && seams.Count < MaxSeams; j++)
            {
                Bounds a = pieces[i].bounds, c = pieces[j].bounds;
                if (a.min.x - c.max.x > maxGap || c.min.x - a.max.x > maxGap) continue;
                if (a.min.y - c.max.y > maxGap || c.min.y - a.max.y > maxGap) continue;
                if (a.min.z - c.max.z > maxGap || c.min.z - a.max.z > maxGap) continue;

                for (int axis = 0; axis < 3; axis++)
                {
                    if (a.size[axis] < ThinSize && c.size[axis] < ThinSize) continue;   // coplanar flat pieces
                    if (!AreNeighbours(a, c, axis, minContact, edgeTol)) continue;

                    float g = c.min[axis] - a.max[axis];
                    if (g >= minGap && g <= maxGap) AddSeam(pieces[i], pieces[j], axis, g);

                    g = a.min[axis] - c.max[axis];
                    if (g >= minGap && g <= maxGap) AddSeam(pieces[j], pieces[i], axis, g);
                }
            }
        }

        seams.Sort((x, y) => y.gap.CompareTo(x.gap));   // biggest gaps first
        Repaint();
        SceneView.RepaintAll();
    }

    void AddSeam(Piece low, Piece high, int axis, float gap)
    {
        Bounds lb = low.bounds, hb = high.bounds;
        Vector3 min = Vector3.zero, max = Vector3.zero;
        for (int k = 0; k < 3; k++)
        {
            if (k == axis)
            {
                min[k] = Mathf.Min(lb.max[k], hb.min[k]);
                max[k] = Mathf.Max(lb.max[k], hb.min[k]);
            }
            else
            {
                // Edge-only contacts can have a slightly negative overlap; keep the box sensible.
                float lo = Mathf.Max(lb.min[k], hb.min[k]);
                float hi = Mathf.Min(lb.max[k], hb.max[k]);
                min[k] = Mathf.Min(lo, hi);
                max[k] = Mathf.Max(lo, hi);
            }
        }

        Vector3 size = max - min;
        for (int k = 0; k < 3; k++) size[k] = Mathf.Max(size[k], 0.01f);

        seams.Add(new Seam
        {
            low = low.root,
            high = high.root,
            lowBounds = lb,
            highBounds = hb,
            axis = axis,
            gap = gap,
            center = (min + max) * 0.5f,
            size = size
        });
    }

    void ShowSeam(Seam s)
    {
        Selection.objects = new Object[] { s.low.gameObject, s.high.gameObject };
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.Frame(new Bounds(s.center, Vector3.one * 1.5f), false);
    }

    void FixSeam(Seam s)
    {
        if (s.low == null || s.high == null) return;

        // Move the smaller piece toward the bigger one, closing the gap plus the overlap.
        bool moveLow = Volume(s.lowBounds) <= Volume(s.highBounds);
        Transform t = moveLow ? s.low : s.high;

        Vector3 dir = Vector3.zero;
        dir[s.axis] = moveLow ? 1f : -1f;

        Undo.RecordObject(t, "Fix Seam");
        t.position += dir * (s.gap + Overlap);
        GeometryMoved?.Invoke();
    }

    // ---------------- Scene drawing ----------------

    void OnSceneGUI(SceneView sv)
    {
        if (!ShowSeams || seams.Count == 0) return;

        Vector3 camPos = sv.camera != null ? sv.camera.transform.position : Vector3.zero;
        float labelDist = LabelDistance;

        var oldZ = Handles.zTest;
        var oldColor = Handles.color;
        Handles.zTest = CompareFunction.Always;

        foreach (var s in seams)
        {
            if (s.low == null || s.high == null) continue;

            Handles.color = s.gap <= Touch ? Color.yellow : Color.red;
            Handles.DrawWireCube(s.center, s.size);

            if (Vector3.Distance(camPos, s.center) <= labelDist)
                Handles.Label(s.center, $"{s.gap * 100f:0.00} cm");
        }

        Handles.zTest = oldZ;
        Handles.color = oldColor;
    }

    // ---------------- Pieces and bounds ----------------

    // What counts as one "piece" for a transform: the LOD group it belongs to, or a small prefab
    // instance just above it, otherwise the object itself. Stops the tool from moving a single
    // LOD mesh away from its siblings and collision mesh.
    static Transform PieceRoot(Transform t)
    {
        var lod = t.GetComponentInParent<LODGroup>();
        if (lod != null && Depth(t, lod.transform) <= 3 && IsSmallPiece(lod.transform))
            return lod.transform;

        var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
        if (prefabRoot != null && prefabRoot.transform != t && t.IsChildOf(prefabRoot.transform)
            && Depth(t, prefabRoot.transform) <= 2 && IsSmallPiece(prefabRoot.transform))
            return prefabRoot.transform;

        return t;
    }

    static int Depth(Transform child, Transform ancestor)
    {
        int d = 0;
        for (var p = child; p != null && p != ancestor; p = p.parent) d++;
        return d;
    }

    static bool IsSmallPiece(Transform root) => root.GetComponentsInChildren<MeshRenderer>(true).Length <= MaxPieceRenderers;

    static List<Transform> SelectedPieces()
    {
        var list = new List<Transform>();
        var seen = new HashSet<Transform>();
        foreach (var t in Selection.transforms)
        {
            var p = PieceRoot(t);
            if (seen.Add(p)) list.Add(p);
        }
        return list;
    }

    // Renderers that belong to LOD1+ (and not LOD0) of the given LOD groups.
    static HashSet<Renderer> HigherLodRenderers(IEnumerable<LODGroup> groups)
    {
        var set = new HashSet<Renderer>();
        foreach (var g in groups)
        {
            var lods = g.GetLODs();
            if (lods.Length < 2) continue;

            var lod0 = new HashSet<Renderer>();
            foreach (var r in lods[0].renderers) if (r != null) lod0.Add(r);

            for (int i = 1; i < lods.Length; i++)
                foreach (var r in lods[i].renderers)
                    if (r != null && !lod0.Contains(r)) set.Add(r);
        }
        return set;
    }

    // Only the mesh you actually see: enabled, not shadow-only, not a collision mesh, not a lower LOD.
    static bool IsVisualRenderer(MeshRenderer r, HashSet<Renderer> higherLods)
    {
        if (!r.enabled || !r.gameObject.activeInHierarchy) return false;
        if (r.shadowCastingMode == ShadowCastingMode.ShadowsOnly) return false;
        if (higherLods.Contains(r)) return false;

        string n = r.name;
        foreach (var p in CollisionPrefixes)
            if (n.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }

    static bool TryGetPieceBounds(Transform piece, out Bounds b)
    {
        b = default;
        bool has = false;
        var higherLods = HigherLodRenderers(piece.GetComponentsInChildren<LODGroup>());
        foreach (var r in piece.GetComponentsInChildren<MeshRenderer>())
        {
            if (!IsVisualRenderer(r, higherLods)) continue;
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        return has;
    }

    static List<Piece> GatherPieces(HashSet<Transform> excludeRoots)
    {
        bool staticOnly = StaticOnly;
        float minSize = MinPieceSize;
        string[] keywords = StructuralOnly ? ParseKeywords(StructuralNames) : null;

        var higherLods = HigherLodRenderers(FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude));
        var map = new Dictionary<Transform, Bounds>();
        var structural = new HashSet<Transform>();
        var order = new List<Transform>();

        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            if (!IsVisualRenderer(r, higherLods)) continue;
            if (staticOnly && !r.gameObject.isStatic) continue;
            if (excludeRoots != null && IsUnderAny(r.transform, excludeRoots)) continue;

            var root = PieceRoot(r.transform);
            if (map.TryGetValue(root, out var b))
            {
                b.Encapsulate(r.bounds);
                map[root] = b;
            }
            else
            {
                map[root] = r.bounds;
                order.Add(root);
            }

            if (keywords != null && (NameMatches(root.name, keywords) || NameMatches(r.name, keywords)))
                structural.Add(root);
        }

        var list = new List<Piece>(order.Count);
        foreach (var root in order)
        {
            if (keywords != null && !structural.Contains(root)) continue;
            var b = map[root];
            Vector3 s = b.size;
            if (Mathf.Max(s.x, Mathf.Max(s.y, s.z)) < minSize) continue;
            list.Add(new Piece { root = root, bounds = b });
        }
        return list;
    }

    static string[] ParseKeywords(string csv)
    {
        var list = new List<string>();
        foreach (var part in csv.Split(','))
        {
            var k = part.Trim();
            if (k.Length > 0) list.Add(k);
        }
        return list.ToArray();
    }

    static bool NameMatches(string name, string[] keywords)
    {
        foreach (var k in keywords)
            if (name.IndexOf(k, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        return false;
    }

    static bool IsUnderAny(Transform t, HashSet<Transform> set)
    {
        for (var p = t; p != null; p = p.parent)
            if (set.Contains(p)) return true;
        return false;
    }

    static float Volume(Bounds b) => Mathf.Max(b.size.x, 0.001f) * Mathf.Max(b.size.y, 0.001f) * Mathf.Max(b.size.z, 0.001f);
}