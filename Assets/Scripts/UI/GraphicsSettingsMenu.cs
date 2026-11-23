using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Quality and Performance part of the Settings panel. Put it on the Settings panel (next to
// settingsController) in the main menu and the pause menu, and drag in the controls you made.
// Every slot is optional: leave one empty and that setting just isn't shown.
// Choices are saved (GraphicsOptions) and PerformanceManager applies them straight away.
public class GraphicsSettingsMenu : MonoBehaviour
{
    [Header("Quality")]
    [Tooltip("Low / Medium / High / Custom. Picking one sets everything below.")]
    [SerializeField] TMP_Dropdown presetDropdown;
    [Tooltip("Off / Low / Medium / High.")]
    [SerializeField] TMP_Dropdown shadowsDropdown;
    [Tooltip("Off / FXAA / SMAA / TAA (TAA isn't offered in the Web build).")]
    [SerializeField] TMP_Dropdown antiAliasingDropdown;
    [Tooltip("Bloom, color grading and the other screen effects.")]
    [SerializeField] Toggle postProcessingToggle;
    [Tooltip("Soft shading in corners. Greyed out when the current pipeline doesn't have it (the Web build).")]
    [SerializeField] Toggle ambientOcclusionToggle;

    [Header("Performance")]
    [Tooltip("50% to 100%: how sharp the 3D view is drawn. Lower is faster.")]
    [SerializeField] Slider renderScaleSlider;
    [Tooltip("Optional text next to the slider, e.g. \"85%\".")]
    [SerializeField] TMP_Text renderScaleLabel;
    [Tooltip("Lowers the render scale by itself while the game runs slow.")]
    [SerializeField] Toggle autoResolutionToggle;
    [Tooltip("PC: VSync / 30 / 60 / 120 / 144 / Unlimited. Web: Browser / 30 / 60.")]
    [SerializeField] TMP_Dropdown frameRateDropdown;
    [Tooltip("Shows the frame rate in the top-left corner (same as F3).")]
    [SerializeField] Toggle showFpsToggle;

    static readonly List<string> presetNames = new() { "Low", "Medium", "High", "Custom" };
    static readonly List<string> shadowNames = new() { "Off", "Low", "Medium", "High" };

    readonly List<GraphicsOptions.AntiAliasing> aaModes = new();
    readonly List<int> frameCaps = new();
    bool refreshing;

    void Awake()
    {
        if (presetDropdown != null)
        {
            presetDropdown.ClearOptions();
            presetDropdown.AddOptions(presetNames);
            presetDropdown.onValueChanged.AddListener(i => { if (!refreshing) GraphicsOptions.ApplyPreset((GraphicsOptions.Preset)i); });
        }

        if (shadowsDropdown != null)
        {
            shadowsDropdown.ClearOptions();
            shadowsDropdown.AddOptions(shadowNames);
            shadowsDropdown.onValueChanged.AddListener(i => { if (!refreshing) GraphicsOptions.ShadowQuality = (GraphicsOptions.Shadows)i; });
        }

        if (antiAliasingDropdown != null)
        {
            aaModes.Clear();
            aaModes.Add(GraphicsOptions.AntiAliasing.Off);
            aaModes.Add(GraphicsOptions.AntiAliasing.FXAA);
            aaModes.Add(GraphicsOptions.AntiAliasing.SMAA);
            if (GraphicsOptions.TaaAllowed)
                aaModes.Add(GraphicsOptions.AntiAliasing.TAA);
            var names = new List<string>();
            foreach (var mode in aaModes)
                names.Add(mode == GraphicsOptions.AntiAliasing.Off ? "Off" : mode.ToString());
            antiAliasingDropdown.ClearOptions();
            antiAliasingDropdown.AddOptions(names);
            antiAliasingDropdown.onValueChanged.AddListener(i => { if (!refreshing && i >= 0 && i < aaModes.Count) GraphicsOptions.AntiAliasingMode = aaModes[i]; });
        }

        if (postProcessingToggle != null)
            postProcessingToggle.onValueChanged.AddListener(on => { if (!refreshing) GraphicsOptions.PostProcessing = on; });

        if (ambientOcclusionToggle != null)
            ambientOcclusionToggle.onValueChanged.AddListener(on => { if (!refreshing) GraphicsOptions.AmbientOcclusion = on; });

        if (renderScaleSlider != null)
        {
            // Whole percent steps, so the saved value doesn't change with every pixel of drag.
            renderScaleSlider.minValue = GraphicsOptions.MinRenderScale * 100f;
            renderScaleSlider.maxValue = GraphicsOptions.MaxRenderScale * 100f;
            renderScaleSlider.wholeNumbers = true;
            renderScaleSlider.onValueChanged.AddListener(percent =>
            {
                ShowScaleLabel(percent);
                if (!refreshing)
                    GraphicsOptions.RenderScale = percent / 100f;
            });
        }

        if (autoResolutionToggle != null)
            autoResolutionToggle.onValueChanged.AddListener(on => { if (!refreshing) GraphicsOptions.AutoResolution = on; });

        if (frameRateDropdown != null)
        {
            frameCaps.Clear();
            var names = new List<string>();
            if (GraphicsOptions.IsWeb)
            {
                frameCaps.AddRange(new[] { 0, 30, 60 });
                names.AddRange(new[] { "Browser (smoothest)", "30 fps", "60 fps" });
            }
            else
            {
                frameCaps.AddRange(new[] { 0, 30, 60, 120, 144, -1 });
                names.AddRange(new[] { "VSync", "30 fps", "60 fps", "120 fps", "144 fps", "Unlimited" });
            }
            frameRateDropdown.ClearOptions();
            frameRateDropdown.AddOptions(names);
            frameRateDropdown.onValueChanged.AddListener(i => { if (!refreshing && i >= 0 && i < frameCaps.Count) GraphicsOptions.FrameCap = frameCaps[i]; });
        }

        if (showFpsToggle != null)
            showFpsToggle.onValueChanged.AddListener(on => { if (!refreshing) GraphicsOptions.ShowFps = on; });
    }

    // Every time the panel opens, and whenever a setting changes (a preset changes several at once).
    void OnEnable()
    {
        GraphicsOptions.Changed += Refresh;
        Refresh();
    }

    void OnDisable() => GraphicsOptions.Changed -= Refresh;

    void Refresh()
    {
        refreshing = true;

        if (presetDropdown != null)
            SetDropdown(presetDropdown, (int)GraphicsOptions.CurrentPreset);
        if (shadowsDropdown != null)
            SetDropdown(shadowsDropdown, (int)GraphicsOptions.ShadowQuality);
        if (antiAliasingDropdown != null)
            SetDropdown(antiAliasingDropdown, Mathf.Max(0, aaModes.IndexOf(GraphicsOptions.AntiAliasingMode)));
        if (postProcessingToggle != null)
            postProcessingToggle.SetIsOnWithoutNotify(GraphicsOptions.PostProcessing);
        if (ambientOcclusionToggle != null)
        {
            bool available = PerformanceManager.Instance == null || PerformanceManager.Instance.HasAmbientOcclusion;
            ambientOcclusionToggle.interactable = available;
            ambientOcclusionToggle.SetIsOnWithoutNotify(available && GraphicsOptions.AmbientOcclusion);
        }
        if (renderScaleSlider != null)
        {
            float percent = Mathf.Round(GraphicsOptions.RenderScale * 100f);
            renderScaleSlider.SetValueWithoutNotify(percent);
            ShowScaleLabel(percent);
        }
        if (autoResolutionToggle != null)
            autoResolutionToggle.SetIsOnWithoutNotify(GraphicsOptions.AutoResolution);
        if (frameRateDropdown != null)
        {
            int index = frameCaps.IndexOf(GraphicsOptions.FrameCap);
            SetDropdown(frameRateDropdown, index >= 0 ? index : 0);
        }
        if (showFpsToggle != null)
            showFpsToggle.SetIsOnWithoutNotify(GraphicsOptions.ShowFps);

        refreshing = false;
    }

    static void SetDropdown(TMP_Dropdown dropdown, int index)
    {
        dropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, dropdown.options.Count - 1));
        dropdown.RefreshShownValue();
    }

    void ShowScaleLabel(float percent)
    {
        if (renderScaleLabel != null)
            renderScaleLabel.text = $"{percent:0}%";
    }
}