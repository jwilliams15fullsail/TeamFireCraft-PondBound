using UnityEngine;

public class SpiderAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Dedicated source for looping walk/run skittering")]
    public AudioSource movementSource;
    [Tooltip("Dedicated source for one-shot attacks, taunts, and death")]
    public AudioSource actionSource;

    [Header("Walking / Skittering")]
    public AudioClip[] stepClips;
    [Range(0.8f, 1.2f)] public float walkPitch = 1.0f;
    [Range(0.8f, 1.4f)] public float runPitch = 1.2f;

    [Header("Action SFX")]
    public AudioClip biteClip;
    public AudioClip screechClip;
    public AudioClip deathClip;

    public void StartMovementLoop(bool isRunning)
    {
        if (stepClips.Length == 0 || movementSource == null) return;

        // Adjust pitch based on walk vs. run speed
        movementSource.pitch = isRunning ? runPitch : walkPitch;

        // Only start playing if it isn't already playing
        if (!movementSource.isPlaying)
        {
            movementSource.clip = stepClips[Random.Range(0, stepClips.Length)];
            movementSource.loop = true;
            movementSource.Play();
        }
    }

    public void StopMovementLoop()
    {
        if (movementSource != null && movementSource.isPlaying)
        {
            movementSource.Stop();
        }
    }

    public void PlayBite() => PlayActionSound(biteClip);
    public void PlayScreech() => PlayActionSound(screechClip);
    public void PlayDeath() => PlayActionSound(deathClip);

    private void PlayActionSound(AudioClip clip)
    {
        if (clip == null || actionSource == null) return;
        actionSource.pitch = Random.Range(0.95f, 1.05f);
        actionSource.PlayOneShot(clip);
    }
}