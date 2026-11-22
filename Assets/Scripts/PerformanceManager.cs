using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps the game at a steady frame rate on PC and in the browser, and applies the player's
// quality settings (GraphicsOptions, changed from the Settings menu). It makes itself when the
// game starts, so it needs no scene setup (drop one in a scene only if you want to change its settings).
// - Frame pacing: PC syncs to the monitor (VSync) unless the player picks a cap or Unlimited; the
//   Web build lets the browser drive, or locks to the cap the player picks.
// - Render scale: the player's Render Scale setting is the most it uses. With Auto Resolution on it
//   lowers the scale when frames run slower than the target, and raises it again when there's room.
// - Shadows, anti-aliasing, post-processing and ambient occlusion follow the Settings menu.
//   Two of them are done indirectly, on purpose:
//   Shadows Off shrinks the shadow distance to almost nothing, so no camera's own shadow setting
//   is overwritten (and can be lost when scenes load), and URP keeps handling baked shadows.
//   Post-processing Off silences the costly effects (bloom, blur, vignette, grain...) and keeps
//   tonemapping and colour. Switching the camera's post-processing off would also kill anti-aliasing.
// - Everything it changes on the URP asset is put back when the game stops, so the Editor's
//   asset files don't keep the player's settings.
// - Web build only: turns off film grain and high-quality bloom.
// - F3 (or Show FPS in Settings) shows the frame rate, frame time and render scale.
// Everything it does is logged with a [Perf] tag, so the browser console shows what's running.
public class PerformanceManager : MonoBehaviour
{
    // Saved frame cap: 0 = VSync / browser, -1 = no cap (PC), anything else = that many fps.
    public const string FrameCapKey = "FrameCap";

    public static PerformanceManager Instance { get; private set; }

    [Header("Frame pacing")]
    [Tooltip("PC default before the player picks one: 0 syncs to the monitor (VSync). Any other number caps the frame rate there instead.")]
    [SerializeField] int pcFrameCap = 0;
    [Tooltip("Web default before the player picks one: 0 lets the browser pick the rate (smoothest when it can reach 60). 30 locks to an even 30 fps on slower machines.")]
    [SerializeField] int webFrameCap = 0;

    [Header("Adaptive resolution")]
    [Tooltip("Lets the player's Auto Resolution setting work at all.")]
    [SerializeField] bool adaptiveResolution = true;
    [Tooltip("Off by default: Editor frame times aren't what players get, and the URP asset is shared.")]
    [SerializeField] bool adaptiveInEditor = false;
    [Tooltip("Frame time to stay under, in milliseconds, when nothing caps the rate. 30 fps = 33.3 ms, so aim a little lower.")]
    [SerializeField] float targetFrameMs = 30f;
    [SerializeField, Range(0.3f, 1f)] float minRenderScale = 0.5f;
    [Tooltip("How much the render scale moves per step.")]
    [SerializeField, Range(0.02f, 0.2f)] float scaleStep = 0.1f;

    [Header("Web build")]
    [Tooltip("Turn off film grain and high-quality bloom in the browser.")]
    [SerializeField] bool lighterPostOnWeb = true;
    [Tooltip("Test the Web tweaks in the Editor.")]
    [SerializeField] bool simulateWebInEditor = false;

    [Header("Readout")]
    [SerializeField] bool showReadout = false;

    const float WindowSeconds = 0.5f;   // frame times are averaged over this long
    const float WarmupSeconds = 2f;     // ignore loading hitches after a scene loads
    const float HitchMs = 250f;         // a frame this long is a hitch or a hidden tab, not slowness
    const float NativeScale = 0.95f;    // URP draws anything above this at full resolution
    const float ShadowsOffDistance = 0.5f;

    // The URP asset's own values, kept so they can be put back.
    struct AssetDefaults
    {
        public float renderScale;
        public float shadowDistance;
        public int cascades;
        public int mainShadowResolution;
        public int additionalShadowResolution;
        public UpscalingFilterSelection upscaling;
    }

    UniversalRenderPipelineAsset urpAsset;
    AssetDefaults defaults;
    bool haveDefaults;
    readonly List<(ScriptableRendererFeature feature, bool wasActive)> aoFeatures = new();
    float currentScale = 1f;
    float appliedMaxScale = -1f; // the Render Scale pick that was last applied

    float windowTime, windowMs;
    int windowFrames;
    bool windowHadHitch;
    float warmupUntil;
    float holdUpUntil;   // no raising the scale before this time
    float lastRaiseTime = -100f;
    int fastWindows;

    float readoutMs = 16.7f;
    Camera lastCamera;
    Volume effectsOffVolume;
    float nextCameraCheck;

    bool IsWeb => Application.platform == RuntimePlatform.WebGLPlayer || (Application.isEditor && simulateWebInEditor);
    int DefaultFrameCap => IsWeb ? webFrameCap : pcFrameCap;
    int FrameCap => ClampCap(PlayerPrefs.GetInt(FrameCapKey, DefaultFrameCap), IsWeb);
    bool AdaptiveOn => adaptiveResolution && GraphicsOptions.AutoResolution && (!Application.isEditor || adaptiveInEditor);
    float MaxScale => GraphicsOptions.RenderScale;
    float MinScale => Mathf.Min(minRenderScale, MaxScale);

    // True when the current render pipeline has screen-space ambient occlusion to switch.
    public bool HasAmbientOcclusion => aoFeatures.Count > 0;

    // The saved frame cap, for the Settings menu.
    public static int SavedFrameCap
    {
        get
        {
            if (Instance != null)
                return Instance.FrameCap;
            bool web = Application.platform == RuntimePlatform.WebGLPlayer;
            return ClampCap(PlayerPrefs.GetInt(FrameCapKey, 0), web);
        }
    }

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
        GraphicsOptions.Changed += OnOptionsChanged;
        showReadout |= GraphicsOptions.ShowFps;
        ApplyFramePacing();
        GrabRenderAsset();
        CreateEffectsOffVolume();
        if (IsWeb && lighterPostOnWeb)
            LightenVolumes(); // sceneLoaded doesn't run for the first scene
        CheckCamera();
        warmupUntil = Time.unscaledTime + WarmupSeconds;
        Debug.Log($"[Perf] platform={Application.platform} api={SystemInfo.graphicsDeviceType} gpu={SystemInfo.graphicsDeviceName} " +
                  $"quality={QualitySettings.names[QualitySettings.GetQualityLevel()]} vSync={QualitySettings.vSyncCount} " +
                  $"targetFrameRate={Application.targetFrameRate} renderScale={currentScale:F2} adaptive={AdaptiveOn} " +
                  $"preset={GraphicsOptions.CurrentPreset} shadows={GraphicsOptions.ShadowQuality} aa={GraphicsOptions.AntiAliasingMode} " +
                  $"compute={SystemInfo.supportsComputeShaders} web={IsWeb}");
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GraphicsOptions.Changed -= OnOptionsChanged;
        RestoreRenderAsset();
        if (effectsOffVolume != null)
        {
            VolumeProfile profile = effectsOffVolume.sharedProfile;
            Destroy(effectsOffVolume.gameObject);
            Destroy(profile);
        }
        Instance = null;
    }

    void OnApplicationQuit() => RestoreRenderAsset();

    static int ClampCap(int fps, bool web) => fps < 0 ? (web ? 0 : -1) : fps;

    // Sets the frame cap (0 = VSync / browser, -1 = no cap on PC) and saves it.
    public static void SetFrameCap(int fps)
    {
        bool web = Instance != null ? Instance.IsWeb : Application.platform == RuntimePlatform.WebGLPlayer;
        PlayerPrefs.SetInt(FrameCapKey, ClampCap(fps, web));
        PlayerPrefs.Save();
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
        else if (cap == 0)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            // A cap, or -1 for as fast as it can go.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = cap > 0 ? cap : -1;
        }
        Debug.Log($"[Perf] frame pacing: cap={(cap > 0 ? cap.ToString() : cap < 0 ? "unlimited" : "none")} vSync={QualitySettings.vSyncCount} targetFrameRate={Application.targetFrameRate}");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        warmupUntil = Time.unscaledTime + WarmupSeconds;
        ResetWindow();
        if (IsWeb && lighterPostOnWeb)
            LightenVolumes();
        lastCamera = null; // check the new scene's camera straight away
        nextCameraCheck = 0f;
    }

    void OnOptionsChanged()
    {
        showReadout = GraphicsOptions.ShowFps;
        // Moving the slider (or picking a preset) starts again from exactly that scale.
        if (!Mathf.Approximately(appliedMaxScale, MaxScale))
        {
            currentScale = MaxScale;
            fastWindows = 0;
        }
        ApplyAssetSettings();
        ApplyPostProcessing();
        if (lastCamera != null)
            ApplyCameraSettings(lastCamera);
        warmupUntil = Time.unscaledTime + 1f; // settle before judging the new settings
        ResetWindow();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            GraphicsOptions.ShowFps = !showReadout;
#else
        if (Input.GetKeyDown(KeyCode.F3))
            GraphicsOptions.ShowFps = !showReadout;
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

        if (slow && currentScale > MinScale + 0.001f)
        {
            // A raise that made it slow again: wait longer before trying again.
            bool undo = Time.unscaledTime - lastRaiseTime < 3f;
            SetScale(currentScale - scaleStep, averageMs);
            holdUpUntil = Time.unscaledTime + (undo ? 20f : 5f);
            fastWindows = 0;
            return;
        }

        fastWindows = fast ? fastWindows + 1 : 0;
        if (fastWindows >= 4 && Time.unscaledTime >= holdUpUntil && currentScale < MaxScale - 0.001f)
        {
            SetScale(currentScale + scaleStep * 0.5f, averageMs);
            lastRaiseTime = Time.unscaledTime;
            fastWindows = 0;
        }
    }

    void SetScale(float scale, float averageMs)
    {
        float next = Mathf.Clamp(scale, MinScale, MaxScale);
        if (Mathf.Approximately(next, currentScale))
            return;
        Debug.Log($"[Perf] render scale {currentScale:F2} -> {next:F2} (average frame {averageMs:F1} ms, {1000f / averageMs:F0} fps)");
        currentScale = next;
        ApplyScale();
    }

    void GrabRenderAsset()
    {
        RestoreRenderAsset();
        urpAsset = UniversalRenderPipeline.asset;
        if (urpAsset == null)
            return;

        defaults = new AssetDefaults
        {
            renderScale = urpAsset.renderScale,
            shadowDistance = urpAsset.shadowDistance,
            cascades = urpAsset.shadowCascadeCount,
            mainShadowResolution = urpAsset.mainLightShadowmapResolution,
            additionalShadowResolution = urpAsset.additionalLightsShadowmapResolution,
            upscaling = urpAsset.upscalingFilter,
        };
        haveDefaults = true;

        aoFeatures.Clear();
        foreach (ScriptableRendererData data in urpAsset.rendererDataList)
        {
            if (data == null)
                continue;
            foreach (ScriptableRendererFeature feature in data.rendererFeatures)
            {
                if (feature is ScreenSpaceAmbientOcclusion)
                    aoFeatures.Add((feature, feature.isActive));
            }
        }
        currentScale = MaxScale; // a new asset starts at the player's pick
        ApplyAssetSettings();
    }

    // The player's settings on the URP asset: render scale, shadows, ambient occlusion.
    void ApplyAssetSettings()
    {
        if (urpAsset == null || !haveDefaults)
            return;

        // The player's pick is the most it uses. With Auto Resolution on, a scale it has already
        // lowered stays lowered (it climbs back by itself when there's room); otherwise it's exact.
        currentScale = AdaptiveOn ? Mathf.Clamp(currentScale, MinScale, MaxScale) : MaxScale;
        appliedMaxScale = MaxScale;
        ApplyScale();

        switch (GraphicsOptions.ShadowQuality)
        {
            case GraphicsOptions.Shadows.Off:
                // Has to stay past the camera's near plane, or URP switches shadows off completely.
                float near = lastCamera != null ? lastCamera.nearClipPlane : 0f;
                urpAsset.shadowDistance = Mathf.Max(ShadowsOffDistance, near + 0.05f);
                urpAsset.shadowCascadeCount = 1;
                urpAsset.mainLightShadowmapResolution = 256;
                urpAsset.additionalLightsShadowmapResolution = 512;
                break;
            case GraphicsOptions.Shadows.Low:
                urpAsset.shadowDistance = defaults.shadowDistance * 0.35f;
                urpAsset.shadowCascadeCount = 1;
                urpAsset.mainLightShadowmapResolution = Mathf.Min(defaults.mainShadowResolution, 1024);
                urpAsset.additionalLightsShadowmapResolution = Mathf.Min(defaults.additionalShadowResolution, 512);
                break;
            case GraphicsOptions.Shadows.Medium:
                urpAsset.shadowDistance = defaults.shadowDistance * 0.6f;
                urpAsset.shadowCascadeCount = Mathf.Min(defaults.cascades, 2);
                urpAsset.mainLightShadowmapResolution = Mathf.Min(defaults.mainShadowResolution, 2048);
                urpAsset.additionalLightsShadowmapResolution = Mathf.Min(defaults.additionalShadowResolution, 1024);
                break;
            default:
                RestoreShadows();
                break;
        }

        foreach (var (feature, wasActive) in aoFeatures)
        {
            if (feature != null)
                feature.SetActive(wasActive && GraphicsOptions.AmbientOcclusion);
        }

        Debug.Log($"[Perf] settings: preset={GraphicsOptions.CurrentPreset} scale={currentScale:F2} auto={AdaptiveOn} shadows={GraphicsOptions.ShadowQuality} " +
                  $"(distance {urpAsset.shadowDistance:F0} m, {urpAsset.shadowCascadeCount} cascades) aa={GraphicsOptions.AntiAliasingMode} " +
                  $"post={GraphicsOptions.PostProcessing} ao={(HasAmbientOcclusion ? GraphicsOptions.AmbientOcclusion.ToString() : "n/a")}");
    }

    // Set the upscaler as well: URP's Automatic turns blocky (nearest pixel) at exactly 50%.
    void ApplyScale()
    {
        urpAsset.renderScale = currentScale;
        bool sharpen = !IsWeb && currentScale < NativeScale;
        urpAsset.upscalingFilter = sharpen ? UpscalingFilterSelection.FSR : UpscalingFilterSelection.Linear;
    }

    void RestoreShadows()
    {
        urpAsset.shadowDistance = defaults.shadowDistance;
        urpAsset.shadowCascadeCount = defaults.cascades;
        urpAsset.mainLightShadowmapResolution = defaults.mainShadowResolution;
        urpAsset.additionalLightsShadowmapResolution = defaults.additionalShadowResolution;
    }

    // Puts the URP asset back, so the Editor doesn't keep the player's settings.
    void RestoreRenderAsset()
    {
        if (urpAsset != null && haveDefaults)
        {
            urpAsset.renderScale = defaults.renderScale;
            urpAsset.upscalingFilter = defaults.upscaling;
            RestoreShadows();
        }
        foreach (var (feature, wasActive) in aoFeatures)
        {
            if (feature != null)
                feature.SetActive(wasActive);
        }
        aoFeatures.Clear();
        urpAsset = null;
        haveDefaults = false;
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
        var data = cam.GetUniversalAdditionalCameraData();
        if (data == null)
            return;
        ApplyCameraSettings(cam);
        ApplyPostProcessing();
        ApplyAssetSettings(); // Shadows Off depends on this camera's near plane
    }

    // The player's settings on the game camera. Anti-aliasing needs the camera's Post Processing
    // box ticked, so that box and Render Shadows are left as the scene set them.
    void ApplyCameraSettings(Camera cam)
    {
        var data = cam.GetUniversalAdditionalCameraData();
        if (data == null)
            return;

        GraphicsOptions.AntiAliasing aa = GraphicsOptions.AntiAliasingMode;
        if (aa == GraphicsOptions.AntiAliasing.TAA && IsWeb)
            aa = GraphicsOptions.AntiAliasing.SMAA;
        data.antialiasing = aa switch
        {
            GraphicsOptions.AntiAliasing.FXAA => AntialiasingMode.FastApproximateAntialiasing,
            GraphicsOptions.AntiAliasing.SMAA => AntialiasingMode.SubpixelMorphologicalAntiAliasing,
            GraphicsOptions.AntiAliasing.TAA => AntialiasingMode.TemporalAntiAliasing,
            _ => AntialiasingMode.None,
        };
        Debug.Log($"[Perf] {cam.name}: aa={data.antialiasing} camera post={data.renderPostProcessing}");
    }

    // A volume that outranks the scene's and sets the costly effects to zero.
    void CreateEffectsOffVolume()
    {
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Post-processing Off";
        profile.Add<Bloom>().intensity.Override(0f);
        profile.Add<Vignette>().intensity.Override(0f);
        profile.Add<FilmGrain>().intensity.Override(0f);
        profile.Add<ChromaticAberration>().intensity.Override(0f);
        profile.Add<LensDistortion>().intensity.Override(0f);
        profile.Add<MotionBlur>().intensity.Override(0f);
        profile.Add<PaniniProjection>().distance.Override(0f);
        profile.Add<DepthOfField>().mode.Override(DepthOfFieldMode.Off);
        profile.Add<ScreenSpaceLensFlare>().intensity.Override(0f);

        var go = new GameObject("Post-processing Off");
        go.transform.SetParent(transform, false);
        effectsOffVolume = go.AddComponent<Volume>();
        effectsOffVolume.isGlobal = true;
        effectsOffVolume.priority = 10000f;
        effectsOffVolume.sharedProfile = profile;
        ApplyPostProcessing();
    }

    void ApplyPostProcessing()
    {
        if (effectsOffVolume == null)
            return;
        effectsOffVolume.enabled = !GraphicsOptions.PostProcessing;
        if (lastCamera == null)
            return;

        // It only counts on a layer the camera reads volumes from.
        var data = lastCamera.GetUniversalAdditionalCameraData();
        int mask = data != null ? data.volumeLayerMask.value : 1;
        for (int layer = 0; layer < 32; layer++)
        {
            if ((mask & (1 << layer)) == 0)
                continue;
            effectsOffVolume.gameObject.layer = layer;
            break;
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
        int cap = FrameCap;
        string capText = cap > 0 ? cap.ToString() : cap < 0 ? "off" : IsWeb ? "browser" : "vsync";
        string text = $"{1000f / Mathf.Max(readoutMs, 0.01f):F0} fps  {readoutMs:F1} ms  scale {currentScale:F2}  cap {capText}";
        Matrix4x4 before = GUI.matrix;
        float size = Mathf.Max(1f, Screen.height / 1080f); // readable at 1440p and 4K
        GUI.matrix = Matrix4x4.Scale(new Vector3(size, size, 1f));
        GUI.Label(new Rect(10, 10, 420, 24), text);
        GUI.matrix = before;
    }
}
