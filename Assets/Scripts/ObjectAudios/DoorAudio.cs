using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(DoorController))]
public class DoorAudio : MonoBehaviour
{
    [Header("Door Sounds")]
    [SerializeField] AudioClip[] openingSounds;
    [SerializeField] AudioClip[] closingSounds;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)] float volume = 0.6f;

    AudioSource audioSource;
    DoorController door;

    bool previousState;
    bool initialized;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        door = GetComponent<DoorController>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        if (door == null || !door.IsSpawned)
            return;

        bool currentState = door.IsOpen;

        if (!initialized)
        {
            previousState = currentState;
            initialized = true;
            return;
        }

        if (currentState == previousState)
            return;

        if (currentState)
            PlayRandomSound(openingSounds);
        else
            PlayRandomSound(closingSounds);

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