using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(WallButton))]
public class WallButtonAudio : MonoBehaviour
{
    [Header("Button Sounds")]
    [SerializeField] AudioClip[] activateSounds;
    [SerializeField] AudioClip[] deactivateSounds;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)]
    float volume = 0.6f;

    AudioSource audioSource;
    WallButton button;

    bool previousState;
    bool initialized;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<WallButton>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        if (button == null || !button.IsSpawned)
            return;

        bool currentState = button.IsActive;

        if (!initialized)
        {
            previousState = currentState;
            initialized = true;
            return;
        }

        if (currentState == previousState)
            return;

        if (currentState)
            PlayRandomSound(activateSounds);
        else
            PlayRandomSound(deactivateSounds);

        previousState = currentState;
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}