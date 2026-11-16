using UnityEngine;
using UnityEngine.Audio;

// The background sound of an area: a looping bed (sewer drips and rumble, lab hum) that fades in
// while your frog is inside this object's trigger box and fades out when it leaves, so walking
// from room to room crossfades between them. It can also play random one-off sounds (a pipe
// creak, a drip, distant machinery) at random spots inside the box, heard in 3D.
// Purely local: each player hears the zone their own frog is in.
// Needs a trigger collider (a Box Collider with Is Trigger) covering the area.
[RequireComponent(typeof(Collider))]
public class AmbientZone : MonoBehaviour
{
    [Header("Background loop")]
    [Tooltip("The looping background sound for this area.")]
    [SerializeField] AudioClip loop;
    [SerializeField, Range(0f, 1f)] float loopVolume = 0.4f;
    [Tooltip("Seconds to fade in when your frog walks in, and out when it leaves.")]
    [SerializeField, Min(0.01f)] float fadeTime = 1.5f;

    [Header("Random one-off sounds (optional)")]
    [SerializeField] AudioClip[] oneShots;
    [SerializeField, Range(0f, 1f)] float oneShotVolume = 0.6f;
    [Tooltip("Seconds between one-off sounds (shortest, longest).")]
    [SerializeField] Vector2 oneShotGap = new Vector2(6f, 15f);
    [Tooltip("How far a one-off sound can be heard (m).")]
    [SerializeField] float oneShotRange = 20f;

    [Header("Mixer")]
    [Tooltip("Optional: the mixer group, e.g. SFX or an Ambience group, so the Settings sliders control it.")]
    [SerializeField] AudioMixerGroup output;

    Collider area;
    AudioSource bed, spot;
    Transform localFrog;
    float nextFind, nextOneShot;

    void Awake()
    {
        area = GetComponent<Collider>();
        area.isTrigger = true;

        bed = gameObject.AddComponent<AudioSource>();
        bed.clip = loop;
        bed.loop = true;
        bed.playOnAwake = false;
        bed.spatialBlend = 0f; // all around you while you're in the area
        bed.volume = 0f;
        bed.outputAudioMixerGroup = output;

        spot = new GameObject("Ambient One-Shot").AddComponent<AudioSource>();
        spot.transform.SetParent(transform, false);
        spot.playOnAwake = false;
        spot.spatialBlend = 1f;
        spot.rolloffMode = AudioRolloffMode.Linear;
        spot.minDistance = 1f;
        spot.maxDistance = oneShotRange;
        spot.outputAudioMixerGroup = output;

        nextOneShot = Time.time + Random.Range(oneShotGap.x, oneShotGap.y);
    }

    void Update()
    {
        // Our own frog (looked up once a second until it has spawned).
        if (localFrog == null && Time.time >= nextFind)
        {
            nextFind = Time.time + 1f;
            FrogStats frog = PlayerHUD.FindLocalFrogStats();
            localFrog = frog != null ? frog.transform : null;
        }

        bool inside = localFrog != null && Contains(localFrog.position);
        float target = inside ? loopVolume : 0f;
        bed.volume = Mathf.MoveTowards(bed.volume, target, loopVolume / fadeTime * Time.deltaTime);
        if (loop != null)
        {
            if (bed.volume > 0f && !bed.isPlaying)
                bed.Play();
            else if (bed.volume <= 0f && bed.isPlaying)
                bed.Stop();
        }

        if (inside && oneShots != null && oneShots.Length > 0 && Time.time >= nextOneShot)
        {
            nextOneShot = Time.time + Random.Range(oneShotGap.x, oneShotGap.y);
            AudioClip clip = oneShots[Random.Range(0, oneShots.Length)];
            if (clip != null)
            {
                Bounds b = area.bounds;
                spot.transform.position = new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), Random.Range(b.min.z, b.max.z));
                spot.pitch = Random.Range(0.9f, 1.1f);
                spot.PlayOneShot(clip, oneShotVolume);
            }
        }
    }

    bool Contains(Vector3 point) => (area.ClosestPoint(point) - point).sqrMagnitude < 0.0001f;
}