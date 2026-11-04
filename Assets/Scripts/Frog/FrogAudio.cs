using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FrogAudio : MonoBehaviour
{
    [Header("Movement Sounds")]
    [SerializeField] AudioClip[] hopSounds;
    [SerializeField] AudioClip[] landingSounds;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)]
    float movementVolume = 0.6f;

    [SerializeField]
    float minimumSoundInterval = 0.45f;

    AudioSource audioSource;
    float lastSoundTime = -10f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    public void PlayHop()
    {
        PlayRandomSound(hopSounds);
    }

    public void PlayLanding()
    {
        PlayRandomSound(landingSounds);
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        if (Time.time - lastSoundTime < minimumSoundInterval)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip, movementVolume);
        lastSoundTime = Time.time;
    }
}