using System.Collections.Generic;
using FishNet;
using UnityEngine;

// A trigger that refills health, hunger and hydration for every frog inside it (a spring,
// a resting lily pad...). The host does the refilling; the bars update for everyone
// through FrogStats. Needs a trigger collider on the same GameObject; no NetworkObject.
// It can also celebrate when your own frog gets there (a chime and sparkles when it reaches
// the water): each player hears and sees that for their own frog.
public class StatRefillZone : MonoBehaviour
{
    [SerializeField] float healthPerSecond = 15f;
    [SerializeField] float hungerPerSecond = 15f;
    [SerializeField] float hydrationPerSecond = 25f;
    [Tooltip("Fill every bar to full the moment a frog steps in, instead of over time.")]
    [SerializeField] bool refillInstantly;

    [Header("Celebration (optional)")]
    [Tooltip("Played when your own frog gets here, e.g. a cheer, a splash or a happy chime.")]
    [SerializeField] AudioClip celebrationSound;
    [SerializeField, Range(0f, 1f)] float celebrationVolume = 0.8f;
    [Tooltip("Optional: a particle effect in the scene (sparkles, a splash). It's moved to your frog and played.")]
    [SerializeField] ParticleSystem celebrationEffect;
    [Tooltip("Celebrate only the first time your frog gets here, not every time it comes back.")]
    [SerializeField] bool celebrateOnce = true;

    readonly Dictionary<FrogStats, int> inside = new(); // frog -> its colliders inside
    readonly List<FrogStats> gone = new();
    AudioSource celebrationSource;
    bool celebrated;
    float nextCelebration;

    static bool IsHost => InstanceFinder.IsServerStarted;

    void OnTriggerEnter(Collider other)
    {
        if (!TryGetFrog(other, out FrogStats frog))
            return;
        if (frog.IsOwner)
            Celebrate(frog.transform.position);
        if (!IsHost)
            return;
        inside.TryGetValue(frog, out int count);
        inside[frog] = count + 1;
        if (refillInstantly && count == 0)
            frog.RefillAll();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsHost || !TryGetFrog(other, out FrogStats frog) || !inside.TryGetValue(frog, out int count))
            return;
        if (count <= 1)
            inside.Remove(frog);
        else
            inside[frog] = count - 1;
    }

    // Your own frog got here: play the celebration on this machine only.
    void Celebrate(Vector3 at)
    {
        if ((celebrateOnce && celebrated) || Time.time < nextCelebration)
            return;
        celebrated = true;
        nextCelebration = Time.time + 3f; // a frog's several colliders entering count once
        if (celebrationSound != null)
        {
            if (celebrationSource == null)
            {
                celebrationSource = gameObject.AddComponent<AudioSource>();
                celebrationSource.playOnAwake = false;
                celebrationSource.spatialBlend = 0f; // your own moment: heard clearly, not in 3D
            }
            celebrationSource.PlayOneShot(celebrationSound, celebrationVolume);
        }
        if (celebrationEffect != null)
        {
            celebrationEffect.transform.position = at;
            celebrationEffect.Play(true);
        }
    }

    void FixedUpdate()
    {
        if (!IsHost || refillInstantly || inside.Count == 0)
            return;
        float dt = Time.fixedDeltaTime;
        foreach (FrogStats frog in inside.Keys)
        {
            if (frog == null)
                gone.Add(frog); // left the game while standing here
            else
                frog.Restore(healthPerSecond * dt, hungerPerSecond * dt, hydrationPerSecond * dt);
        }
        foreach (FrogStats frog in gone)
            inside.Remove(frog);
        gone.Clear();
    }

    static bool TryGetFrog(Collider other, out FrogStats frog)
    {
        frog = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<FrogStats>() : null;
        return frog != null;
    }

    // Shows the refill area in the Scene view so it's easy to place.
    void OnDrawGizmos()
    {
        if (!TryGetComponent(out Collider area))
            return;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        Gizmos.DrawCube(area.bounds.center, area.bounds.size);
    }
}