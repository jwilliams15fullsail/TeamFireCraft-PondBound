using UnityEngine;
using UnityEngine.Audio;

// Sounds for lasers and vents: a start sound when the hazard begins (a laser charging up, a vent
// starting to hiss), a loop while it's dangerous (beam hum, gas hiss), and a stop sound when it
// ends. Put it on a TimedLaser or TimedVent, or on the parent of a whole group (the ceiling
// lasers): it follows every TimedLaser and TimedVent on this object and under it, so a group
// makes one sound instead of one per beam. Follows the hazards' shared clock, so every player
// hears the same thing, and goes quiet when a switch turns the hazards off.
public class HazardSound : MonoBehaviour
{
    [Tooltip("Played when the hazard starts: the warning beam or the first puff of gas.")]
    [SerializeField] AudioClip startSound;
    [Tooltip("Loops while the hazard can hurt: laser hum, gas hiss.")]
    [SerializeField] AudioClip loop;
    [Tooltip("Played when it switches off.")]
    [SerializeField] AudioClip stopSound;
    [SerializeField, Range(0f, 1f)] float volume = 0.7f;

    [Header("3D sound")]
    [Tooltip("Full volume within this distance (m).")]
    [SerializeField, Min(0f)] float minDistance = 2f;
    [Tooltip("Silent beyond this distance (m).")]
    [SerializeField, Min(0.1f)] float maxDistance = 15f;
    [Tooltip("Optional: the mixer group, e.g. SFX, so the Settings sliders control it.")]
    [SerializeField] AudioMixerGroup output;

    TimedLaser[] lasers;
    TimedVent[] vents;
    AudioSource loopSource, shotSource;
    int state; // 0 off, 1 warning, 2 dangerous

    void Awake()
    {
        lasers = GetComponentsInChildren<TimedLaser>(true);
        vents = GetComponentsInChildren<TimedVent>(true);
        loopSource = MakeSource(true);
        shotSource = MakeSource(false);
        loopSource.clip = loop;
    }

    AudioSource MakeSource(bool looping)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = looping;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.volume = volume;
        source.outputAudioMixerGroup = output;
        return source;
    }

    void Update()
    {
        int now = 0;
        foreach (TimedLaser laser in lasers)
            if (laser != null)
                now = Mathf.Max(now, laser.BeamDangerous ? 2 : laser.BeamShowing ? 1 : 0);
        foreach (TimedVent vent in vents)
            if (vent != null)

                now = Mathf.Max(now, vent.IsBlasting ? 2 : vent.IsSmoking ? 1 : 0);
        if (now == state)
            return;

        if (state == 0 && startSound != null)
            shotSource.PlayOneShot(startSound);
        if (now == 2 && loop != null && !loopSource.isPlaying)
            loopSource.Play();
        if (now < 2 && loopSource.isPlaying)
            loopSource.Stop();
        if (now == 0 && stopSound != null)
            shotSource.PlayOneShot(stopSound);
        state = now;
    }
}