using System;
using UnityEngine;

// The player's quality and performance choices, saved between launches. The Settings menu
// (GraphicsSettingsMenu) changes them; PerformanceManager applies them to the URP asset and
// the game camera. A preset (Low / Medium / High) sets everything at once; changing any one
// value by hand switches the preset to Custom.
public static class GraphicsOptions
{
    public enum Preset { Low, Medium, High, Custom }
    public enum Shadows { Off, Low, Medium, High }
    public enum AntiAliasing { Off, FXAA, SMAA, TAA }

    const string PresetKey = "Gfx.Preset";
    const string RenderScaleKey = "Gfx.RenderScale";
    const string AutoResolutionKey = "Gfx.AutoResolution";
    const string ShadowsKey = "Gfx.Shadows";
    const string AntiAliasingKey = "Gfx.AntiAliasing";
    const string PostProcessingKey = "Gfx.PostProcessing";
    const string AmbientOcclusionKey = "Gfx.AmbientOcclusion";
    const string ShowFpsKey = "Gfx.ShowFps";

    public const float MinRenderScale = 0.5f;
    public const float MaxRenderScale = 1f;

    // Raised after any change, so PerformanceManager and every open menu can update.
    public static event Action Changed;

    public static bool IsWeb => Application.platform == RuntimePlatform.WebGLPlayer;

    // TAA smears in the browser and costs a lot there, so the Web build doesn't offer it.
    public static bool TaaAllowed => !IsWeb;

    // A new player starts on High on PC and Medium in the browser.
    static Preset DefaultPreset => IsWeb ? Preset.Medium : Preset.High;

    public static Preset CurrentPreset
    {
        get => (Preset)PlayerPrefs.GetInt(PresetKey, (int)DefaultPreset);
        private set => PlayerPrefs.SetInt(PresetKey, (int)value);
    }

    // The resolution the 3D view is drawn at, 0.5 to 1 of the screen. With Auto Resolution on
    // this is the most it uses; it drops below this only while frames run slow.
    public static float RenderScale
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(RenderScaleKey, PresetRenderScale(DefaultPreset)), MinRenderScale, MaxRenderScale);
        set => SetCustom(() => PlayerPrefs.SetFloat(RenderScaleKey, Mathf.Clamp(value, MinRenderScale, MaxRenderScale)));
    }

    public static bool AutoResolution
    {
        get => PlayerPrefs.GetInt(AutoResolutionKey, 1) == 1;
        set => SetCustom(() => PlayerPrefs.SetInt(AutoResolutionKey, value ? 1 : 0));
    }

    public static Shadows ShadowQuality
    {
        get => (Shadows)Mathf.Clamp(PlayerPrefs.GetInt(ShadowsKey, (int)PresetShadows(DefaultPreset)), 0, 3);
        set => SetCustom(() => PlayerPrefs.SetInt(ShadowsKey, (int)value));
    }

    public static AntiAliasing AntiAliasingMode
    {
        get
        {
            var mode = (AntiAliasing)Mathf.Clamp(PlayerPrefs.GetInt(AntiAliasingKey, (int)PresetAntiAliasing(DefaultPreset)), 0, 3);
            return mode == AntiAliasing.TAA && !TaaAllowed ? AntiAliasing.SMAA : mode;
        }
        set => SetCustom(() => PlayerPrefs.SetInt(AntiAliasingKey, (int)value));
    }

    // Bloom, color grading and the other screen effects. Off is faster but the game looks flatter.
    public static bool PostProcessing
    {
        get => PlayerPrefs.GetInt(PostProcessingKey, 1) == 1;
        set => SetCustom(() => PlayerPrefs.SetInt(PostProcessingKey, value ? 1 : 0));
    }

    // Screen-space ambient occlusion (soft shading in corners). Only the PC pipeline has it.
    public static bool AmbientOcclusion
    {
        get => PlayerPrefs.GetInt(AmbientOcclusionKey, PresetAmbientOcclusion(DefaultPreset) ? 1 : 0) == 1;
        set => SetCustom(() => PlayerPrefs.SetInt(AmbientOcclusionKey, value ? 1 : 0));
    }

    // Not part of the presets: it's a readout, not a quality setting.
    public static bool ShowFps
    {
        get => PlayerPrefs.GetInt(ShowFpsKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(ShowFpsKey, value ? 1 : 0);
            Changed?.Invoke();
        }
    }

    // Frame cap: 0 = VSync on PC / the browser's own pacing on the Web, -1 = no cap (PC only),
    // anything else = that many frames per second. Not part of the presets either.
    public static int FrameCap
    {
        get => PerformanceManager.SavedFrameCap;
        set
        {
            PerformanceManager.SetFrameCap(value);
            Changed?.Invoke();
        }
    }

    public static void ApplyPreset(Preset preset)
    {
        if (preset == Preset.Custom)
            return;
        PlayerPrefs.SetFloat(RenderScaleKey, PresetRenderScale(preset));
        PlayerPrefs.SetInt(AutoResolutionKey, 1);
        PlayerPrefs.SetInt(ShadowsKey, (int)PresetShadows(preset));
        PlayerPrefs.SetInt(AntiAliasingKey, (int)PresetAntiAliasing(preset));
        PlayerPrefs.SetInt(PostProcessingKey, 1);
        PlayerPrefs.SetInt(AmbientOcclusionKey, PresetAmbientOcclusion(preset) ? 1 : 0);
        CurrentPreset = preset;
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    static float PresetRenderScale(Preset preset) => preset switch
    {
        Preset.Low => 0.65f,
        Preset.Medium => 0.85f,
        _ => 1f,
    };

    static Shadows PresetShadows(Preset preset) => preset switch
    {
        Preset.Low => Shadows.Low,
        Preset.Medium => Shadows.Medium,
        _ => Shadows.High,
    };

    static AntiAliasing PresetAntiAliasing(Preset preset) => preset switch
    {
        Preset.Low => AntiAliasing.FXAA,
        Preset.Medium => IsWeb ? AntiAliasing.FXAA : AntiAliasing.SMAA,
        _ => TaaAllowed ? AntiAliasing.TAA : AntiAliasing.SMAA,
    };

    static bool PresetAmbientOcclusion(Preset preset) => preset == Preset.High && !IsWeb;

    static void SetCustom(Action save)
    {
        save();
        CurrentPreset = Preset.Custom;
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}