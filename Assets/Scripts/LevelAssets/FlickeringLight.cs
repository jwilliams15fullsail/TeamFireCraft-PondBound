using UnityEngine;

// A failing light: it burns steadily, then now and then stutters through a burst of flicker,
// sometimes dropping out completely for a moment. The Light and the glowing (emissive) part of
// the lamp's model dim together, and an optional sound plays as each burst starts.
// Purely visual, so each player's copy flickers on its own; nobody can tell.
// The lamp's material needs Emission turned on for the glow to flicker.
[DisallowMultipleComponent]
public class FlickeringLight : MonoBehaviour
{
    [Header("What flickers")]
    [Tooltip("The lights to flicker. Empty: every Light on this object and its children.")]
    [SerializeField] Light[] lights;
    [Tooltip("The lamp's glowing parts. Their material needs Emission turned on.")]
    [SerializeField] Renderer[] emissiveRenderers;
    [Tooltip("Which material on those renderers glows (0 = the first).")]
    [SerializeField, Min(0)] int materialIndex;

    [Header("Flicker")]
    [Tooltip("Brightness between bursts (1 = the light's own intensity and glow).")]
    [SerializeField, Range(0f, 1f)] float steadyBrightness = 1f;
    [Tooltip("Average seconds of steady light between bursts.")]
    [SerializeField, Min(0.1f)] float timeBetweenBursts = 4f;
    [Tooltip("How long a burst of flicker lasts (shortest, longest seconds).")]
    [SerializeField] Vector2 burstLength = new Vector2(0.2f, 0.8f);
    [Tooltip("The dimmest it gets while flickering.")]
    [SerializeField, Range(0f, 1f)] float flickerMinBrightness = 0.05f;
    [Tooltip("How many times a second the brightness jumps during a burst.")]
    [SerializeField, Min(1f)] float flickerRate = 20f;
    [Tooltip("Chance a burst ends with the light dropping out completely.")]
    [SerializeField, Range(0f, 1f)] float blackoutChance = 0.15f;
    [Tooltip("How long a drop-out lasts (shortest, longest seconds).")]
    [SerializeField] Vector2 blackoutLength = new Vector2(0.1f, 0.6f);

    [Header("Sound (optional)")]
    [Tooltip("Plays a Flicker Sound as each burst starts. Make it a 3D sound (Spatial Blend 1).")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("One is picked at random for each burst: buzzes, clicks, sparks.")]
    [SerializeField] AudioClip[] flickerSounds;
    [SerializeField] Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [Tooltip("Optional: a looping hum (Loop on, Play On Awake on). Its volume follows the brightness.")]
    [SerializeField] AudioSource hum;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    float[] baseIntensities;
    Color[] baseEmissions;
    MaterialPropertyBlock block;
    float baseHumVolume;
    float nextBurst, burstEnd, blackoutEnd, nextJump, flickerLevel;

    void Awake()
    {
        if (lights == null || lights.Length == 0)
            lights = GetComponentsInChildren<Light>();
        baseIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            baseIntensities[i] = lights[i] != null ? lights[i].intensity : 0f;

        emissiveRenderers ??= new Renderer[0];
        baseEmissions = new Color[emissiveRenderers.Length];
        for (int i = 0; i < emissiveRenderers.Length; i++)
        {
            Renderer r = emissiveRenderers[i];
            Material m = r != null && materialIndex < r.sharedMaterials.Length ? r.sharedMaterials[materialIndex] : null;
            baseEmissions[i] = m != null && m.HasProperty(EmissionColor) ? m.GetColor(EmissionColor) : Color.black;
        }
        block = new MaterialPropertyBlock();
        if (hum != null)
            baseHumVolume = hum.volume;
    }

    void OnEnable()
    {
        // Each lamp starts at a different point, so a row of them doesn't flicker in step.
        nextBurst = Time.time + Random.Range(0f, timeBetweenBursts * 1.5f);
        burstEnd = blackoutEnd = 0f;
    }

    // Switched off in the Inspector or by a script: back to steady light.
    void OnDisable() => Apply(steadyBrightness);

    void Update()
    {
        float now = Time.time;
        float level;
        if (now < burstEnd)
        {
            if (now >= nextJump)
            {
                flickerLevel = Random.Range(flickerMinBrightness, steadyBrightness);
                nextJump = now + Random.Range(0.5f, 1.5f) / flickerRate;
            }
            level = flickerLevel;
        }
        else if (now < blackoutEnd)
        {
            level = 0f;
        }
        else
        {
            level = steadyBrightness;
            if (now >= nextBurst)
                StartBurst(now);
        }
        Apply(level);
    }

    void StartBurst(float now)
    {
        burstEnd = now + Random.Range(burstLength.x, burstLength.y);
        nextJump = now;
        float end = burstEnd;
        if (Random.value < blackoutChance)
        {
            blackoutEnd = burstEnd + Random.Range(blackoutLength.x, blackoutLength.y);
            end = blackoutEnd;
        }
        nextBurst = end + timeBetweenBursts * Random.Range(0.5f, 1.5f);

        if (audioSource != null && flickerSounds != null && flickerSounds.Length > 0)
        {
            AudioClip clip = flickerSounds[Random.Range(0, flickerSounds.Length)];
            if (clip != null)
            {
                audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
                audioSource.PlayOneShot(clip);
            }
        }
    }

    void Apply(float level)
    {
        if (lights != null && baseIntensities != null)
            for (int i = 0; i < lights.Length; i++)
                if (lights[i] != null)
                    lights[i].intensity = baseIntensities[i] * level;

        if (emissiveRenderers != null && baseEmissions != null && block != null)
            for (int i = 0; i < emissiveRenderers.Length; i++)
            {
                Renderer r = emissiveRenderers[i];
                if (r == null || materialIndex >= r.sharedMaterials.Length)
                    continue;
                r.GetPropertyBlock(block, materialIndex);
                block.SetColor(EmissionColor, baseEmissions[i] * level);
                r.SetPropertyBlock(block, materialIndex);
            }

        if (hum != null)
            hum.volume = baseHumVolume * level;
    }
}