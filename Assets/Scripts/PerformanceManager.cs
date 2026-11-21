using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps the game at a steady frame rate on PC and in the browser. It makes itself when the game
// starts, so it needs no scene setup (drop one in a scene only if you want to change its settings).
// - Frame pacing: PC syncs to the monitor (VSync); the Web build lets the browser drive, or locks
//   to 30 fps when Web Frame Cap is 30.
// - Adaptive resolution: when frames run slower than the target it lowers the URP render scale,
//   and raises it again when there's room. It never goes above the scale set in the URP asset,
//   and puts the asset back the way it was when the game stops.
// - Web build only: swaps TAA for FXAA on the camera and turns off film grain and high-quality bloom.
// - F3 shows the frame rate, frame time and render scale.
// Everything it does is logged with a [Perf] tag, so the browser console shows what's running.
public class PerformanceManager : MonoBehaviour
{
    // Saved frame cap (0 = VSync / browser). A Settings dropdown can call SetFrameCap.
    public const string FrameCapKey = "FrameCap";

    public static PerformanceManager Instance { get; private set; }

    [Header("Frame pacing")]
    [Tooltip("PC: 0 syncs to the monitor (VSync). Any other number caps the frame rate there instead.")]
    [SerializeField] int pcFrameCap = 0;
    [Tooltip("Web: 0 lets the browser pick the rate (smoothest when it can reach 60). 30 locks to an even 30 fps on slower machines.")]
    [SerializeField] int webFrameCap = 0;

    [Header("Adaptive resolution")]
    [SerializeField] bool adaptiveResolution = true;
    [Tooltip("Off by default: Editor frame times aren't what players get, and the URP asset is shared.")]
    [SerializeField] bool adaptiveInEditor = false;
    [Tooltip("Frame time to stay under, in milliseconds, when nothing caps the rate. 30 fps = 33.3 ms, so aim a little lower.")]
    [SerializeField] float targetFrameMs = 30f;
    [SerializeField, Range(0.3f, 1f)] float minRenderScale = 0.5f;
    [Tooltip("How much the render scale moves per step.")]
    [SerializeField, Range(0.02f, 0.2f)] float scaleStep = 0.1f;

    [Header("Web build")]
    [Tooltip("Swap TAA for FXAA and turn off film grain and high-quality bloom in the browser.")]
    [SerializeField] bool lighterPostOnWeb = true;
    [Tooltip("Test the Web tweaks in the Editor.")]
    [SerializeField] bool simulateWebInEditor = false;

    [Header("Readout")]
    [SerializeField] bool showReadout = false;

    const float WindowSeconds = 0.5f;   // frame times are averaged over this long
    const float WarmupSeconds = 2f;     // ignore loading hitches after a scene loads
    const float HitchMs = 250f;         // a frame this long is a hitch or a hidden tab, not slowness

    UniversalRenderPipelineAsset urpAsset;
    float originalScale = -1f;
    float currentScale = 1f;

    float windowTime, windowMs;
    int windowFrames;
    bool windowHadHitch;
    float warmupUntil;
    float holdUpUntil;   // no raising the scale before this time
    float lastRaiseTime = -100f;
    int fastWindows;

    float readoutMs = 16.7f;
    Camera lastCamera;
    float nextCameraCheck;

    bool IsWeb => Application.platform == RuntimePlatform.WebGLPlayer || (Application.isEditor && simulateWebInEditor);
    int FrameCap => PlayerPrefs.GetInt(FrameCapKey, IsWeb ? webFrameCap : pcFrameCap);
    bool AdaptiveOn => adaptiveResolution && (!Application.isEditor || adaptiveInEditor);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance != null || FindAnyObjectByType<PerformanceManager>() != null)
            return;
        new GameObject("PerformanceManager").AddComponent<PerformanceManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyFramePacing();
        GrabRenderAsset();
        warmupUntil = Time.unscaledTime + WarmupSeconds;
        Debug.Log($"[Perf] platform={Application.platform} api={SystemInfo.graphicsDeviceType} gpu={SystemInfo.graphicsDeviceName} " +
                  $"quality={QualitySettings.names[QualitySettings.GetQualityLevel()]} vSync={QualitySettings.vSyncCount} " +
                  $"targetFrameRate={Application.targetFrameRate} renderScale={currentScale:F2} adaptive={AdaptiveOn} " +
                  $"compute={SystemInfo.supportsComputeShaders} web={IsWeb}");
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RestoreRenderAsset();
        Instance = null;
    }

    void OnApplicationQuit() => RestoreRenderAsset();

    // Sets the frame cap (0 = VSync / browser) and saves it.
    public static void SetFrameCap(int fps)
    {
        PlayerPrefs.SetInt(FrameCapKey, Mathf.Max(0, fps));
        if (Instance != null)
            Instance.ApplyFramePacing();
    }

    void ApplyFramePacing()
    {
        int cap = FrameCap;
        if (IsWeb)
        {
            // The browser paces frames itself; VSync does nothing there.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = cap > 0 ? cap : -1;
        }
        else if (cap <= 0)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = cap;
        }
        Debug.Log($"[Perf] frame pacing: cap={(cap > 0 ? cap.ToString() : "none")} vSync={QualitySettings.vSyncCount} targetFrameRate={Application.targetFrameRate}");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        warmupUntil = Time.unscaledTime + WarmupSeconds;
        ResetWindow();
        if (IsWeb && lighterPostOnWeb)
            LightenVolumes();
        lastCamera = null; // check the new scene's camera straight away
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            showReadout = !showReadout;
#else
        if (Input.GetKeyDown(KeyCode.F3))
            showReadout = !showReadout;
#endif
        float ms = Time.unscaledDeltaTime * 1000f;
        readoutMs = Mathf.Lerp(readoutMs, ms, 0.1f);

        if (Time.unscaledTime >= nextCameraCheck)
        {
            nextCameraCheck = Time.unscaledTime + 1f;
            CheckCamera();
        }

        if (urpAsset != UniversalRenderPipeline.asset)
            GrabRenderAsset(); // the quality level changed

        if (!AdaptiveOn || urpAsset == null || Time.unscaledTime < warmupUntil || !Application.isFocused)
            return;

        windowTime += Time.unscaledDeltaTime;
        windowMs += ms;
        windowFrames++;
        if (ms > HitchMs)
            windowHadHitch = true;
        if (windowTime < WindowSeconds)
            return;

        float average = windowMs / windowFrames;
        bool hitch = windowHadHitch;
        ResetWindow();
        if (hitch)
            return;
        AdjustScale(average);
    }

    void AdjustScale(float averageMs)
    {
        int cap = FrameCap;
        bool capped = cap > 0 || (!IsWeb && QualitySettings.vSyncCount > 0);
        // With a cap the frame time sits at the cap even with spare time, so "fast" means
        // "keeping up with the cap" and "slow" means "missing it".
        float budget = cap > 0 ? 1000f / cap : targetFrameMs;
        bool slow = averageMs > Mathf.Max(budget, targetFrameMs) * 1.08f;
        bool fast = cap > 0 ? averageMs <= budget * 1.03f : averageMs < targetFrameMs * (capped ? 0.7f : 0.65f);

        if (slow && currentScale > minRenderScale + 0.001f)
        {
            // A raise that made it slow again: wait longer before trying again.
            bool undo = Time.unscaledTime - lastRaiseTime < 3f;
            SetScale(currentScale - scaleStep, averageMs);
            holdUpUntil = Time.unscaledTime + (undo ? 20f : 5f);
            fastWindows = 0;
            return;
        }

        fastWindows = fast ? fastWindows + 1 : 0;
        if (fastWindows >= 4 && Time.unscaledTime >= holdUpUntil && currentScale < originalScale - 0.001f)
        {
            SetScale(currentScale + scaleStep * 0.5f, averageMs);
            lastRaiseTime = Time.unscaledTime;
            fastWindows = 0;
        }
    }

    void SetScale(float scale, float averageMs)
    {
        float next = Mathf.Clamp(scale, minRenderScale, originalScale);
        if (Mathf.Approximately(next, currentScale))
            return;
        Debug.Log($"[Perf] render scale {currentScale:F2} -> {next:F2} (average frame {averageMs:F1} ms, {1000f / averageMs:F0} fps)");
        currentScale = next;
        urpAsset.renderScale = next;
    }

    void GrabRenderAsset()
    {
        RestoreRenderAsset();
        urpAsset = UniversalRenderPipeline.asset;
        if (urpAsset == null)
            return;
        originalScale = urpAsset.renderScale;
        currentScale = originalScale;
    }

    // Puts the URP asset back, so the Editor doesn't keep a lowered render scale.
    void RestoreRenderAsset()
    {
        if (urpAsset != null && originalScale > 0f)
            urpAsset.renderScale = originalScale;
        urpAsset = null;
        originalScale = -1f;
    }

    void ResetWindow()
    {
        windowTime = 0f;
        windowMs = 0f;
        windowFrames = 0;
        windowHadHitch = false;
    }

    void CheckCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || cam == lastCamera)
            return;
        lastCamera = cam;
        if (!IsWeb || !lighterPostOnWeb)
            return;
        var data = cam.GetUniversalAdditionalCameraData();
        if (data != null && data.antialiasing == AntialiasingMode.TemporalAntiAliasing)
        {
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            Debug.Log($"[Perf] {cam.name}: TAA -> FXAA for the Web build");
        }
    }

    // Works on each Volume's own copy of its profile, so the profile asset isn't changed.
    void LightenVolumes()
    {
        foreach (Volume volume in FindObjectsByType<Volume>(FindObjectsInactive.Include))
        {
            if (volume.sharedProfile == null)
                continue;
            VolumeProfile profile = volume.profile;
            if (profile.TryGet(out FilmGrain grain) && grain.active)
            {
                grain.active = false;
                Debug.Log($"[Perf] {volume.name}: film grain off for the Web build");
            }
            if (profile.TryGet(out Bloom bloom) && bloom.active)
            {
                bloom.highQualityFiltering.Override(false);
                bloom.downscale.Override(BloomDownscaleMode.Quarter);
                Debug.Log($"[Perf] {volume.name}: bloom set to low-cost filtering, quarter size");
            }
        }
    }

    void OnGUI()
    {
        if (!showReadout)
            return;
        string text = $"{1000f / Mathf.Max(readoutMs, 0.01f):F0} fps  {readoutMs:F1} ms  scale {currentScale:F2}  cap {(FrameCap > 0 ? FrameCap.ToString() : "-")}";
        GUI.Label(new Rect(10, 10, 420, 24), text);
    }
}