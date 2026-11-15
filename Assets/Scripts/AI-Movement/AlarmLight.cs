using UnityEngine;

// A red alarm light. It pulses smoothly, strobes on and off, or spins like a beacon, and the
// lamp's glowing (emissive) part flashes with it. An optional alarm sound loops while it's on,
// or beeps on every flash.
// Turn it on and off with Starts On, from a button's or a trigger's events (TurnOn / TurnOff /
// SetOn), or let it follow a HazardPower: it then flashes while those hazards are on and stops
// once a switch turns them off. HazardPower is synced, so every player sees the same alarm.
// The lamp's material needs Emission turned on for the glow to flash.
[DisallowMultipleComponent]
public class AlarmLight : MonoBehaviour
{
    public enum Style { Pulse, Strobe, Spin }

    [Header("What flashes")]
    [Tooltip("The lights. Empty: every Light on this object and its children.")]
    [SerializeField] Light[] lights;
    [Tooltip("The lamp's glowing parts. Their material needs Emission turned on.")]
    [SerializeField] Renderer[] emissiveRenderers;
    [Tooltip("Which material on those renderers glows (0 = the first).")]
    [SerializeField, Min(0)] int materialIndex;
    [Tooltip("The alarm's color, for the lights and the glow.")]
    [SerializeField] Color color = new Color(1f, 0.05f, 0.03f);
    [Tooltip("Light intensity at full flash.")]
    [SerializeField, Min(0f)] float intensity = 6f;
    [Tooltip("Glow strength at full flash (HDR: above 1 blooms).")]
    [SerializeField, Min(0f)] float emissionStrength = 4f;

    [Header("Flashing")]
    [SerializeField] Style style = Style.Pulse;
    [Tooltip("Flashes per second (Spin: turns per second).")]
    [SerializeField, Min(0.05f)] float flashesPerSecond = 1.5f;
    [Tooltip("Brightness at the bottom of each flash (0 = fully dark).")]
    [SerializeField, Range(0f, 1f)] float minBrightness = 0f;
    [Tooltip("Spin: what turns, usually a child holding a Spot Light. Empty: this object.")]
    [SerializeField] Transform spinner;

    [Header("On and off")]
    [SerializeField] bool startsOn = true;
    [Tooltip("Optional: flash while these hazards are on, and stop once a switch turns them off.")]
    [SerializeField] HazardPower followHazards;

    [Header("Sound (optional)")]
    [Tooltip("Loops the Alarm Loop while on, or plays Flash Sound each flash. Make it 3D (Spatial Blend 1).")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("A looping siren played while the alarm is on.")]
    [SerializeField] AudioClip alarmLoop;
    [Tooltip("A beep played at the start of every flash (Pulse and Strobe).")]
    [SerializeField] AudioClip flashSound;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    bool isOn;
    float onSince;
    int lastFlash = -1;
    MaterialPropertyBlock block;

    public bool IsOn => isOn;

    void Awake()
    {
        if (lights == null || lights.Length == 0)
            lights = GetComponentsInChildren<Light>();
        emissiveRenderers ??= new Renderer[0];
        block = new MaterialPropertyBlock();
        foreach (Light l in lights)
            if (l != null)
                l.color = color;
        SetOn(startsOn);
    }

    // For button, trigger and UnityEvent hookups. Only this player's copy changes; use
    // Follow Hazards when every player must see the same thing.
    public void TurnOn() => SetOn(true);
    public void TurnOff() => SetOn(false);
    public void Toggle() => SetOn(!isOn);

    public void SetOn(bool on)
    {
        if (on == isOn && lastFlash >= 0)
            return;
        isOn = on;
        onSince = Time.time;
        lastFlash = 0;
        if (audioSource != null && alarmLoop != null)
        {
            if (on)
            {
                audioSource.clip = alarmLoop;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
        if (!on)
            Apply(0f);
    }

    void Update()
    {
        if (followHazards != null)
            SetOn(!followHazards.IsOff);
        if (!isOn)
            return;

        float cycles = (Time.time - onSince) * flashesPerSecond;
        float phase = cycles - Mathf.Floor(cycles); // 0..1 through this flash
        float level;
        switch (style)
        {
            case Style.Strobe:
                level = phase < 0.5f ? 1f : 0f;
                break;
            case Style.Spin:
                level = 1f;
                Transform turn = spinner != null ? spinner : transform;
                turn.Rotate(0f, 360f * flashesPerSecond * Time.deltaTime, 0f, Space.Self);
                break;
            default: // Pulse
                level = 0.5f - 0.5f * Mathf.Cos(phase * 2f * Mathf.PI);
                break;
        }
        Apply(Mathf.Lerp(minBrightness, 1f, level));

        // A beep as each new flash starts.
        int flash = Mathf.FloorToInt(cycles);
        if (flash != lastFlash)
        {
            lastFlash = flash;
            if (style != Style.Spin && audioSource != null && flashSound != null)
                audioSource.PlayOneShot(flashSound);
        }
    }

    void Apply(float level)
    {
        foreach (Light l in lights)
            if (l != null)
                l.intensity = intensity * level;

        Color glow = color * (emissionStrength * level);
        foreach (Renderer r in emissiveRenderers)
        {
            if (r == null || materialIndex >= r.sharedMaterials.Length)
                continue;
            r.GetPropertyBlock(block, materialIndex);
            block.SetColor(EmissionColor, glow);
            r.SetPropertyBlock(block, materialIndex);
        }
    }
}