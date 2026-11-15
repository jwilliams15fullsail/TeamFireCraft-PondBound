using System.Collections.Generic;
using UnityEngine;

// Water dripping from a leaky pipe: a drop swells here, sags, lets go, stretches as it falls, and
// splashes into tiny drops where it lands. Landing in a puddle with GooTrails rings it, and landing
// in a RisingWater flood splashes on the water's surface. Put it on an empty (scale 1) just under a
// pipe joint. Only a visual: every screen drips on its own, so nothing is sent over the network.
public class PipeDrip : MonoBehaviour
{
    [Tooltip("The drops' material: clear, shiny water (URP Lit, Transparent, Smoothness about 0.95).")]
    public Material dropMaterial;
    [Tooltip("How big (m across) a drop is when it lets go.")]
    [Min(0.005f)] public float dropSize = 0.035f;
    [Tooltip("Seconds from one drop to the next.")]
    [Min(0.1f)] public float interval = 1.6f;
    [Tooltip("How much each wait varies, as a share of Interval, so the dripping isn't a steady beat.")]
    [Range(0f, 1f)] public float randomness = 0.35f;
    [Tooltip("How far (m) a drop can fall before it's gone.")]
    [Min(0.1f)] public float maxFall = 15f;
    [Tooltip("What the drops land on.")]
    public LayerMask landOn = ~0;

    [Header("Splash")]
    [Tooltip("How many tiny drops jump up where a drop lands.")]
    [Range(0, 12)] public int splashDrops = 5;
    [Tooltip("How fast (m/s) the tiny drops fly out.")]
    [Min(0f)] public float splashSpeed = 1.2f;
    [Tooltip("How wide (m) the ring is when a drop lands in a puddle with GooTrails.")]
    [Min(0.02f)] public float rippleSize = 0.25f;
    [Tooltip("Optional drip sounds. A random one plays where each drop lands.")]
    public AudioClip[] dripSounds;
    [Range(0f, 1f)] public float volume = 0.4f;

    class Drop
    {
        public Transform body;
        public Vector3 velocity;
        public float age;
        public float life; // tiny splash drops only
        public bool tiny;
    }

    static Mesh sphere;
    readonly List<Drop> drops = new List<Drop>();
    readonly Stack<Transform> spare = new Stack<Transform>();
    Transform forming;
    float formAge, formTime;

    void OnEnable()
    {
        if (sphere == null)
        {
            GameObject temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere = temporary.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temporary);
        }
        StartNextDrop();
    }

    void OnDisable()
    {
        foreach (Drop drop in drops)
            Recycle(drop.body);
        drops.Clear();
        if (forming != null)
            Recycle(forming);
        forming = null;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // The drop at the pipe swells and sags until it's heavy enough to let go
        formAge += dt;
        float grown = Mathf.Clamp01(formAge / formTime);
        float size = dropSize * Mathf.Pow(grown, 0.6f);
        forming.position = transform.position + Vector3.down * (size * 0.5f * (0.6f + 0.4f * grown));
        forming.rotation = Quaternion.identity;
        forming.localScale = new Vector3(size, size * (1f + 0.35f * grown), size);
        if (grown >= 1f)
        {
            drops.Add(new Drop { body = forming });
            StartNextDrop();
        }

        for (int i = drops.Count - 1; i >= 0; i--)
        {
            Drop drop = drops[i];
            drop.age += dt;
            Vector3 from = drop.body.position;
            drop.velocity += Physics.gravity * dt;
            Vector3 to = from + drop.velocity * dt;

            if (drop.tiny)
            {
                // Tiny splash drops shrink away
                float left = 1f - drop.age / drop.life;
                if (left <= 0f || Hit(from, to, out _, out _))
                {
                    Recycle(drop.body);
                    drops.RemoveAt(i);
                    continue;
                }
                drop.body.position = to;
                drop.body.localScale = Vector3.one * (dropSize * 0.35f * left);
                continue;
            }

            if (Hit(from, to, out Vector3 point, out Collider landed))
            {
                Splash(point, landed);
                Recycle(drop.body);
                drops.RemoveAt(i);
                continue;
            }
            if (transform.position.y - to.y > maxFall)
            {
                Recycle(drop.body);
                drops.RemoveAt(i);
                continue;
            }

            // Falling drops stretch out along their path the faster they go
            float stretch = Mathf.Lerp(1.1f, 1.8f, Mathf.Clamp01(drop.velocity.magnitude / 6f));
            drop.body.position = to;
            drop.body.rotation = Quaternion.FromToRotation(Vector3.up, drop.velocity.normalized);
            drop.body.localScale = new Vector3(dropSize * 0.85f, dropSize * stretch, dropSize * 0.85f);
        }
    }

    void StartNextDrop()
    {
        forming = TakeBody();
        formAge = 0f;
        formTime = interval * (1f + Random.Range(-randomness, randomness));
        forming.localScale = Vector3.zero;
    }

    // Where the drop lands between from and to: a surface, or the top of flood water.
    bool Hit(Vector3 from, Vector3 to, out Vector3 point, out Collider landed)
    {
        point = to;
        landed = null;
        float best = float.MaxValue;
        Vector3 path = to - from;
        float length = path.magnitude;
        if (length > 0f && Physics.Raycast(from, path / length, out RaycastHit hit, length, landOn, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            landed = hit.collider;
            best = hit.distance;
        }
        if (RisingWater.SurfaceAt(to, out float surfaceY) && from.y >= surfaceY && to.y < surfaceY)
        {
            float t = (from.y - surfaceY) / Mathf.Max(from.y - to.y, 1e-5f);
            if (t * length < best)
            {
                point = Vector3.Lerp(from, to, t);
                landed = null;
                best = t * length;
            }
        }
        return best < float.MaxValue;
    }

    void Splash(Vector3 point, Collider landed)
    {
        GooTrails puddle = landed != null ? landed.GetComponentInParent<GooTrails>() : null;
        if (puddle != null)
            puddle.Splash(point, rippleSize);

        for (int i = 0; i < splashDrops; i++)
        {
            Vector2 sideways = Random.insideUnitCircle.normalized * Random.Range(0.3f, 1f);
            Vector3 velocity = new Vector3(sideways.x, Random.Range(0.8f, 1.6f), sideways.y) * splashSpeed;
            Transform tiny = TakeBody();
            tiny.position = point + Vector3.up * 0.01f;
            tiny.rotation = Quaternion.identity;
            tiny.localScale = Vector3.one * (dropSize * 0.35f);
            drops.Add(new Drop { body = tiny, velocity = velocity, life = Random.Range(0.25f, 0.45f), tiny = true });
        }

        if (dripSounds != null && dripSounds.Length > 0)
        {
            AudioClip clip = dripSounds[Random.Range(0, dripSounds.Length)];
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, point, volume);
        }
    }

    Transform TakeBody()
    {
        Transform body = spare.Count > 0 ? spare.Pop() : MakeBody();
        body.gameObject.SetActive(true);
        return body;
    }

    Transform MakeBody()
    {
        var body = new GameObject("Drop", typeof(MeshFilter), typeof(MeshRenderer));
        body.transform.SetParent(transform, false);
        body.GetComponent<MeshFilter>().sharedMesh = sphere;
        var renderer = body.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = dropMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return body.transform;
    }

    void Recycle(Transform body)
    {
        if (body == null)
            return;
        body.gameObject.SetActive(false);
        spare.Push(body);
    }
}