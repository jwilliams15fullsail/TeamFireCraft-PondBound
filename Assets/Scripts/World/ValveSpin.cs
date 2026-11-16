using UnityEngine;

// Turns a valve handle while its WallButton is on, and back if it's switched off again. Put it
// next to the WallButton on the valve. The button's on/off state is synced, so every player sees
// the handle turn. Purely visual: the WallButton does the work (its Hazards slot shuts the gas).
[RequireComponent(typeof(WallButton))]
public class ValveSpin : MonoBehaviour
{
    [Tooltip("The part that turns. Empty: this object.")]
    [SerializeField] Transform handle;
    [Tooltip("The axis the handle turns round, in its own space (its green Y arrow by default).")]
    [SerializeField] Vector3 spinAxis = Vector3.up;
    [Tooltip("How far it turns when opened or closed (degrees). Negative turns the other way.")]
    [SerializeField] float turnDegrees = 270f;
    [Tooltip("Seconds the turn takes.")]
    [SerializeField, Min(0.05f)] float turnTime = 1.2f;

    [Header("Sound (optional)")]
    [Tooltip("A squeak or metal grind, played as it starts to turn.")]
    [SerializeField] AudioClip turnSound;
    [SerializeField, Range(0f, 1f)] float volume = 0.8f;

    WallButton button;
    Quaternion restRotation;
    AudioSource audioSource;
    float amount;     // 0 = as placed, 1 = fully turned
    bool wasActive;

    void Awake()
    {
        button = GetComponent<WallButton>();
        if (handle == null)
            handle = transform;
        restRotation = handle.localRotation;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 15f;
    }

    void Update()
    {
        bool active = button.IsActive;
        if (active != wasActive)
        {
            wasActive = active;
            if (turnSound != null)
                audioSource.PlayOneShot(turnSound, volume);
        }
        amount = Mathf.MoveTowards(amount, active ? 1f : 0f, Time.deltaTime / turnTime);
        float angle = turnDegrees * Mathf.SmoothStep(0f, 1f, amount);
        handle.localRotation = restRotation * Quaternion.AngleAxis(angle, spinAxis);
    }
}