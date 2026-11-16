using UnityEngine;

// The frog's hop and landing sounds. Each one depends on what the frog is standing on: a
// SurfaceSound on the floor (or on a trigger box over an area, like a goo puddle) picks the
// clips; anywhere else uses the default Hop and Landing Sounds below.
// FrogProceduralBody calls PlayHop and PlayLanding on every machine, so everyone hears it.
[RequireComponent(typeof(AudioSource))]
public class FrogAudio : MonoBehaviour
{
    [Header("Default sounds (no SurfaceSound under the frog)")]
    [SerializeField] AudioClip[] hopSounds;
    [SerializeField] AudioClip[] landingSounds;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)]
    float movementVolume = 0.6f;
    [SerializeField]
    float minimumSoundInterval = 0.45f;
    [SerializeField] Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Surface check")]
    [Tooltip("How far below the frog's centre to look for the surface (m).")]
    [SerializeField] float surfaceProbe = 0.6f;
    [Tooltip("What counts as a surface. Leave Frog unticked.")]
    [SerializeField] LayerMask surfaceLayers = ~(1 << 6);

    AudioSource audioSource;
    float lastSoundTime = -10f;
    readonly RaycastHit[] hits = new RaycastHit[8];

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    public void PlayHop()
    {
        if (Time.time - lastSoundTime < minimumSoundInterval)
            return;
        SurfaceSound surface = FindSurface();
        if (surface != null)
            Play(surface.steps, surface.volume, surface.pitchRange);
        else
            Play(hopSounds, movementVolume, pitchRange);
    }

    public void PlayLanding()
    {
        if (Time.time - lastSoundTime < minimumSoundInterval)
            return;
        SurfaceSound surface = FindSurface();
        if (surface != null)
        {
            bool hasLandings = surface.landings != null && surface.landings.Length > 0;
            Play(hasLandings ? surface.landings : surface.steps, surface.volume, surface.pitchRange);
        }
        else
        {
            Play(landingSounds, movementVolume, pitchRange);
        }
    }

    // The SurfaceSound under the frog: on what it stands on, or on a trigger box it stands in.
    // The highest priority wins, then the nearest.
    SurfaceSound FindSurface()
    {
        Vector3 from = transform.position + Vector3.up * 0.1f;
        int count = Physics.RaycastNonAlloc(from, Vector3.down, hits, surfaceProbe + 0.1f, surfaceLayers, QueryTriggerInteraction.Collide);
        SurfaceSound best = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            SurfaceSound surface = hits[i].collider.GetComponentInParent<SurfaceSound>();
            if (surface == null)
                continue;
            bool better = best == null || surface.priority > best.priority
                || (surface.priority == best.priority && hits[i].distance < bestDistance);
            if (better)
            {
                best = surface;
                bestDistance = hits[i].distance;
            }
        }
        // Standing inside a trigger box the ray started in (a deep goo puddle): check for that too.
        if (best == null)
        {
            Collider[] around = Physics.OverlapSphere(transform.position, 0.05f, surfaceLayers, QueryTriggerInteraction.Collide);
            foreach (Collider area in around)
            {
                SurfaceSound surface = area.isTrigger ? area.GetComponentInParent<SurfaceSound>() : null;
                if (surface != null && (best == null || surface.priority > best.priority))
                    best = surface;
            }
        }
        return best;
    }

    void Play(AudioClip[] clips, float volume, Vector2 pitch)
    {
        if (clips == null || clips.Length == 0)
            return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
            return;

        audioSource.Stop(); // one movement sound at a time
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(pitch.x, pitch.y);
        audioSource.Play();
        lastSoundTime = Time.time;
    }
}