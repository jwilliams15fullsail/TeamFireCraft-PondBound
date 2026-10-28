using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Settings panel, in the main menu and the pause menu. Every value is saved, and
// Unity itself remembers the resolution and screen mode between launches.
// Resolution and screen mode only change in a built game: the editor's Game view ignores them.
public class settingsController : MonoBehaviour
{
    // Saved as a percentage: 1 to 100, 50 = the normal camera speed. FrogCamera reads it too.
    // A new key name, so values saved by the old 0.1 to 10 slider are ignored.
    public const string MouseSensitivityKey = "MouseSensitivityPercent";
    public const float DefaultMouseSensitivity = 50f;

    [Header("UI Controls")]
    [SerializeField] private Slider mouseSensSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown resolutionDropD;
    [Tooltip("Optional: Fullscreen / Exclusive fullscreen / Windowed.")]
    [SerializeField] private TMP_Dropdown displayModeDropD;
    [SerializeField] private Camera playerCamera;

    static readonly FullScreenMode[] displayModes =
    {
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.Windowed,
    };
    static readonly List<string> displayModeNames = new() { "Fullscreen", "Exclusive Fullscreen", "Windowed" };

    // Every size this monitor supports, biggest first, each size once.
    private readonly List<Vector2Int> resolutions = new();

    private void Start()
    {
        mouseSensSlider.minValue = 1f;
        mouseSensSlider.maxValue = 100f;
        mouseSensSlider.wholeNumbers = true;
        mouseSensSlider.SetValueWithoutNotify(GetMouseSensitivity());
        mouseSensSlider.onValueChanged.AddListener(SetSensitivity);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 100f;
        volumeSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("Volume", 100f));
        volumeSlider.onValueChanged.AddListener(SetVolume);
        SetVolume(volumeSlider.value);

        brightnessSlider.minValue = 0f;
        brightnessSlider.maxValue = 100f;
        brightnessSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("Brightness", 100f));
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        SetBrightness(brightnessSlider.value);

        SetupResolutions();
        SetupDisplayModes();
    }

    // The saved sensitivity (1 to 100), or 50 when nothing usable was saved.
    public static float GetMouseSensitivity()
    {
        float saved = PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity);
        return saved >= 1f && saved <= 100f ? saved : DefaultMouseSensitivity;
    }

    public void SetSensitivity(float mouseSensValue)
    {
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensValue);
    }

    public void SetVolume(float volumeValue)
    {
        AudioListener.volume = volumeValue / 100f;
        PlayerPrefs.SetFloat("Volume", volumeValue);
    }

    public void SetBrightness(float brightnessValue)
    {
        PlayerPrefs.SetFloat("Brightness", brightnessValue);

        float amount = brightnessValue / 100f;

        if (playerCamera != null)
        {
            playerCamera.backgroundColor =
                Color.Lerp(Color.black, Color.white, amount);
        }
    }

    // Lists the sizes the monitor really supports instead of a fixed list, so the
    // player's own screen shape (16:9, 16:10, ultrawide...) is always there.
    private void SetupResolutions()
    {
        resolutions.Clear();
        foreach (Resolution r in Screen.resolutions)
        {
            var size = new Vector2Int(r.width, r.height);
            if (!resolutions.Contains(size)) // the same size at several refresh rates
                resolutions.Add(size);
        }
        if (resolutions.Count == 0)
            resolutions.Add(new Vector2Int(Screen.width, Screen.height));
        resolutions.Sort((a, b) => b.x != a.x ? b.x.CompareTo(a.x) : b.y.CompareTo(a.y));

        var labels = new List<string>();
        int current = 0;
        for (int i = 0; i < resolutions.Count; i++)
        {
            labels.Add($"{resolutions[i].x} x {resolutions[i].y}");
            if (resolutions[i].x == Screen.width && resolutions[i].y == Screen.height)
                current = i;
        }

        resolutionDropD.ClearOptions();
        resolutionDropD.AddOptions(labels);
        resolutionDropD.SetValueWithoutNotify(current);
        resolutionDropD.RefreshShownValue();
        resolutionDropD.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count)
            return;

        Vector2Int size = resolutions[index];
        Debug.Log($"Setting resolution to {size.x} x {size.y}");
        Screen.SetResolution(size.x, size.y, Screen.fullScreenMode);
    }

    private void SetupDisplayModes()
    {
        if (displayModeDropD == null)
            return;

        int current = System.Array.IndexOf(displayModes, Screen.fullScreenMode);
        displayModeDropD.ClearOptions();
        displayModeDropD.AddOptions(displayModeNames);
        displayModeDropD.SetValueWithoutNotify(current >= 0 ? current : 0);
        displayModeDropD.RefreshShownValue();
        displayModeDropD.onValueChanged.AddListener(SetDisplayMode);
    }

    public void SetDisplayMode(int index)
    {
        if (index < 0 || index >= displayModes.Length)
            return;

        Screen.fullScreenMode = displayModes[index];
    }
}