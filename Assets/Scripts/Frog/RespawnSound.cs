using UnityEngine;

// A sound (and optional effect) when a frog comes back: after the death screen, or after it fell
// off the level or into a kill zone and was put back at its checkpoint. Works on every machine
// from what every machine already sees, so it needs no network messages: the frog leaves the
// dead state, or jumps a long way in one frame (the respawn teleport).
// Your own frog's sound plays clearly (2D); other players' frogs are heard where they reappear.
// Put it on the Frog prefab, next to FrogMotor.
[RequireComponent(typeof(FrogMotor))]
public class RespawnSound : MonoBehaviour
{
    [Tooltip("Played when the frog reappears: a pop, a splash, a ribbit.")]
    [SerializeField] AudioClip sound;
    [SerializeField, Range(0f, 1f)] float volume = 0.8f;
    [Tooltip("Optional: a particle effect played where the frog reappears (a puff, a ripple). Set its Simulation Space to World if it's under the frog.")]
    [SerializeField] ParticleSystem effect;
    [Tooltip("A frog that moves further than this (m) in one frame has been put back at its spawn point.")]
    [SerializeField, Min(1f)] float teleportDistance = 4f;

    FrogMotor motor;
    AudioSource source;
    Vector3 lastPosition;
    bool wasDead;
    bool tracking;

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 2f;
        source.maxDistance = 25f;
    }

    void LateUpdate()
    {
        if (!motor.IsSpawned)
        {
            tracking = false;
            return;
        }
        bool dead = motor.IsDead;
        Vector3 position = transform.position;
        bool teleported = (position - lastPosition).sqrMagnitude > teleportDistance * teleportDistance;
        if (tracking && !dead && (wasDead || teleported))
            Play();
        tracking = true;
        wasDead = dead;
        lastPosition = position;
    }

    void Play()
    {
        if (sound != null)
        {
            source.spatialBlend = motor.IsOwner ? 0f : 1f;
            source.PlayOneShot(sound, volume);
        }
        if (effect != null)
        {
            effect.transform.position = transform.position;
            effect.Play(true);
        }
    }
}