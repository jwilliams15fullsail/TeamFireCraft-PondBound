using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hover and click sounds for a menu button.
// They play from a shared object that is never switched off, so buttons that hide their own panel
// (Settings, Back, Resume) or load a scene are still heard.
[RequireComponent(typeof(Button))]
public class MenuButtonAudio : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Effects")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.7f;

    [SerializeField] private AudioMixerGroup sfxGroup;

    private static GameObject player;
    private static readonly List<AudioSource> sources = new(); // one per mixer group

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForNewRun()
    {
        player = null;
        sources.Clear();
    }

    public void OnPointerEnter(PointerEventData eventData) => Play(hoverSound);

    public void OnPointerClick(PointerEventData eventData) => Play(clickSound);

    private void Play(AudioClip clip)
    {
        if (clip != null)
            GetSource(sfxGroup).PlayOneShot(clip, volume);
    }

    private static AudioSource GetSource(AudioMixerGroup group)
    {
        if (player == null)
        {
            player = new GameObject("Menu Button Sounds");
            DontDestroyOnLoad(player);
            sources.Clear();
        }

        foreach (AudioSource existing in sources)
            if (existing.outputAudioMixerGroup == group)
                return existing;

        var source = player.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = group;
        sources.Add(source);
        return source;
    }
}