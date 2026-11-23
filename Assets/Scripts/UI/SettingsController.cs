using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Display and Controls part of the Settings panel, in the main menu and the pause menu:
// resolution, display mode, brightness and mouse sensitivity. DisplayOptions and BrightnessOverlay
// do the work, so the saved values also apply before the menu has ever been opened.
// The menu always shows what the screen is really doing, also after Alt+Enter or a window resize.
public class settingsController : MonoBehaviour
{
    // Saved as a percentage: 1 to 100, 50 = the normal camera speed. FrogCamera reads it too.
    // A new key name, so values saved by the old 0.1 to 10 slider are ignored.
    public const string MouseSensitivityKey = "MouseSensitivityPercent";
    public const float DefaultMouseSensitivity = 50f;

    [Header("UI Controls")]
    [SerializeField] private Slider mouseSensSlider;
    [Tooltip("0 to 100. 50 is the game's normal brightness.")]
    [SerializeField] private Slider brightnessSlider;
    [Tooltip("Hidden in the Web build, where the browser sets the size.")]
    [SerializeField] private TMP_Dropdown resolutionDropD;
    [Tooltip("Optional: Fullscreen / Exclusive Fullscreen / Windowed.")]
    [SerializeField] private TMP_Dropdown displayModeDropD;

    private List<Vector2Int> sizes = new();
    private List<DisplayOptions.Mode> modes = new();
    private Vector2Int shownSize;
    private DisplayOptions.Mode shownMode;

    // The saved sensitivity (1 to 100), or 50 when nothing usable was saved.
    public static float GetMouseSensitivity()
    {
        float saved = PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity);
        return saved >= 1f && saved <= 100f ? saved : DefaultMouseSensitivity;
    }

    private void Awake()
    {
        // Set the ranges before listening, so changing a range isn't saved as the player's choice.
        if (mouseSensSlider != null)
        {
            mouseSensSlider.minValue = 5f;
            mouseSensSlider.maxValue = 100f;
            mouseSensSlider.wholeNumbers = true;
            mouseSensSlider.SetValueWithoutNotify(GetMouseSensitivity());
            mouseSensSlider.onValueChanged.AddListener(SetSensitivity);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 100f;
            brightnessSlider.wholeNumbers = true;
            brightnessSlider.SetValueWithoutNotify(BrightnessOverlay.Percent);
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        if (displayModeDropD != null)
        {
            modes = DisplayOptions.Modes();
            var names = new List<string>();
            foreach (DisplayOptions.Mode mode in modes)
                names.Add(DisplayOptions.NameOf(mode));
            displayModeDropD.ClearOptions();
            displayModeDropD.AddOptions(names);
            displayModeDropD.onValueChanged.AddListener(SetDisplayMode);
        }

        if (resolutionDropD != null)
        {
            resolutionDropD.onValueChanged.AddListener(SetResolution);
            if (!DisplayOptions.CanChangeResolution)
                HideRow(resolutionDropD.transform);
        }
    }

    // Every time the panel opens: the other copy of the menu may have changed the values.
    private void OnEnable()
    {
        if (mouseSensSlider != null)
            mouseSensSlider.SetValueWithoutNotify(GetMouseSensitivity());
        if (brightnessSlider != null)
            brightnessSlider.SetValueWithoutNotify(BrightnessOverlay.Percent);
        RefreshDisplay();
    }

    private void OnDisable() => PlayerPrefs.Save();

    private void Update()
    {
        if (DisplayOptions.CurrentSize == shownSize && DisplayOptions.CurrentMode == shownMode)
            return;
        // Rebuilding an open list would close it under the mouse.
        if (resolutionDropD != null && resolutionDropD.IsExpanded)
            return;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        shownSize = DisplayOptions.CurrentSize;
        shownMode = DisplayOptions.CurrentMode;

        if (displayModeDropD != null)
        {
            displayModeDropD.SetValueWithoutNotify(Mathf.Max(0, modes.IndexOf(shownMode)));
            displayModeDropD.RefreshShownValue();
        }

        if (resolutionDropD == null || !DisplayOptions.CanChangeResolution)
            return;

        sizes = DisplayOptions.Sizes(shownMode);
        if (!sizes.Contains(shownSize))
            sizes.Insert(0, shownSize); // a window dragged to its own size

        var names = new List<string>();
        foreach (Vector2Int size in sizes)
            names.Add($"{size.x} x {size.y}");
        resolutionDropD.ClearOptions();
        resolutionDropD.AddOptions(names);
        resolutionDropD.SetValueWithoutNotify(sizes.IndexOf(shownSize));
        resolutionDropD.RefreshShownValue();
    }

    public void SetSensitivity(float mouseSensValue)
    {
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensValue);
    }

    public void SetBrightness(float brightnessValue)
    {
        BrightnessOverlay.Percent = brightnessValue;
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < sizes.Count)
            DisplayOptions.SetSize(sizes[index]);
    }

    public void SetDisplayMode(int index)
    {
        if (index >= 0 && index < modes.Count)
            DisplayOptions.SetMode(modes[index]);
    }

    private static void HideRow(Transform control)
    {
        Transform row = control.parent;
        bool inRow = row != null && row.GetComponent<HorizontalLayoutGroup>() != null;
        (inRow ? row : control).gameObject.SetActive(false);
    }
}
