using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SFXVolume";

    private void Start()
    {
        // Set defaults if these preferences haven't been saved yet.
        masterSlider.value = PlayerPrefs.GetFloat(MasterKey, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(MusicKey, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 1f);

        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSfxVolume(sfxSlider.value);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    public void SetMasterVolume(float value) => SetVolume(MasterKey, value);
    public void SetMusicVolume(float value) => SetVolume(MusicKey, value);
    public void SetSfxVolume(float value) => SetVolume(SfxKey, value);

    private void SetVolume(string parameter, float value)
    {
        // Mixer volume uses decibels. Avoid Log10(0).
        float decibels = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        mixer.SetFloat(parameter, decibels);
        PlayerPrefs.SetFloat(parameter, value);
    }
}