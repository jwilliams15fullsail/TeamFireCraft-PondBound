using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Keeps every menu and the HUD intact at any resolution or screen shape. It makes itself, so it
// needs no scene setup, and changes nothing at 16:9, the shape the UI was designed at.
// - Canvases always fit their whole 1920 x 1080 layout on screen, so nothing is cut off.
// - HUD pieces stay pinned to the real screen edges.
// - Full-screen panels (menus, cutscene, loading, death, victory) keep their 16:9 layout in the
//   middle. Their artwork's edge colours carry on out to the screen edge, so there are no empty bars.
// A panel counts as full-screen when it sits directly on the canvas and is either 1920 x 1080 or
// stretched over the whole screen with a 16:9 picture on it.
public class ScreenFitter : MonoBehaviour
{
    const float RescanSeconds = 1f; // finds menus that are spawned later

    static ScreenFitter instance;

    readonly HashSet<Transform> fitted = new();
    float nextScan;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (instance != null)
            return;
        var go = new GameObject("Screen Fitter");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScreenFitter>();
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Scan();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this)
            instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fitted.RemoveWhere(t => t == null);
        Scan();
    }

    void Update()
    {
        if (Time.unscaledTime >= nextScan)
            Scan();
    }

    void Scan()
    {
        nextScan = Time.unscaledTime + RescanSeconds;
        foreach (CanvasScaler scaler in FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include))
            Fit(scaler, fitted);
    }

    public static void Fit(CanvasScaler scaler, HashSet<Transform> done)
    {
        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            return;
        if (!scaler.TryGetComponent(out Canvas canvas) || canvas.renderMode == RenderMode.WorldSpace)
            return;
        Transform root = scaler.transform;
        if (root.parent != null && root.parent.GetComponentInParent<Canvas>(true) != null)
            return; // a canvas inside a canvas follows its parent's scale

        if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand)
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        Vector2 design = scaler.referenceResolution;
        foreach (Transform child in root)
        {
            if (done.Contains(child) || !(child is RectTransform panel))
                continue;
            done.Add(child);
            FitPanel(panel, (RectTransform)root, design);
        }
    }

    static void FitPanel(RectTransform panel, RectTransform screen, Vector2 design)
    {
        var picture = panel.GetComponent<Image>();
        bool hasPicture = picture != null && picture.sprite != null && picture.type == Image.Type.Simple;

        bool fixedSize = panel.anchorMin == panel.anchorMax
            && Mathf.Abs(panel.sizeDelta.x - design.x) < 2f
            && Mathf.Abs(panel.sizeDelta.y - design.y) < 2f;
        bool stretchedPicture = hasPicture
            && panel.anchorMin == Vector2.zero && panel.anchorMax == Vector2.one
            && panel.sizeDelta.sqrMagnitude < 4f
            && Mathf.Abs(picture.sprite.rect.width / picture.sprite.rect.height - design.x / design.y) < 0.05f;
        if (!fixedSize && !stretchedPicture)
            return;

        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = design;

        if (hasPicture)
            ScreenEdgeFill.AddTo(picture, screen);
    }
}
