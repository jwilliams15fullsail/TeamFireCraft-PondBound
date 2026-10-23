using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class settingsController : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private Slider mouseSensSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown resolutionDropD;
    [SerializeField] private Camera playerCamera;

    private readonly Vector2Int[] resolutionOptions =
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1366, 768),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };

    private void Start()
    {
        // Mouse sensitivity as a percentage: 50% is the normal camera speed.
        mouseSensSlider.minValue = 1f;
        mouseSensSlider.maxValue = 100f;
        mouseSensSlider.wholeNumbers = true;
        mouseSensSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("MouseSensitivity", 50f));
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
    }

    public void SetSensitivity(float mouseSensValue)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensValue);
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

    private void SetupResolutions()
    {
        resolutionDropD.ClearOptions();
        resolutionDropD.AddOptions(new List<string>
        {
            "1920 x 1080",
            "1366 x 768",
            "2560 x 1440",
            "3840 x 2160"
        });

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        int currentIndex = savedIndex >= 0 &&
                           savedIndex < resolutionOptions.Length
            ? savedIndex
            : 0;

        if (savedIndex < 0)
        {
            for (int i = 0; i < resolutionOptions.Length; i++)
            {
                if (Screen.width == resolutionOptions[i].x &&
                    Screen.height == resolutionOptions[i].y)
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        resolutionDropD.SetValueWithoutNotify(currentIndex);
        resolutionDropD.RefreshShownValue();
        resolutionDropD.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutionOptions.Length)
            return;

        Vector2Int resolution = resolutionOptions[index];

        Debug.Log($"Setting resolution to {resolution.x} x {resolution.y}");

        Screen.SetResolution(
            resolution.x,
            resolution.y,
            Screen.fullScreenMode
        );

        PlayerPrefs.SetInt("ResolutionIndex", index);
    }
}
