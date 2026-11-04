using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FrogTongueAudio : MonoBehaviour
{
    [Header("Tongue Sounds")]
    [SerializeField] AudioClip[] fireSounds;
    [SerializeField] AudioClip[] attachSounds;
    [SerializeField] AudioClip[] retractSounds;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)]
    float tongueVolume = 0.6f;

    AudioSource audioSource;
    FrogTongue tongue;

    TongueState previousState = TongueState.Idle;
    bool initialized;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        tongue = GetComponent<FrogTongue>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        if (tongue == null || !tongue.IsSpawned)
            return;

        TongueState currentState = tongue.Sync.Value.State;

        if (!initialized)
        {
            previousState = currentState;
            initialized = true;
            return;
        }

        if (currentState == previousState)
            return;

        switch (currentState)
        {
            case TongueState.Extending:
                PlayRandomSound(fireSounds);
                break;

            case TongueState.Attached:
                PlayRandomSound(attachSounds);
                break;

            case TongueState.Retracting:
                PlayRandomSound(retractSounds);
                break;
        }

        previousState = currentState;
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip != null)
            audioSource.PlayOneShot(clip, tongueVolume);
    }
}