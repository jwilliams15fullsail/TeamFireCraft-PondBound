using UnityEngine;
using UnityEngine.UI;

// The tongue crosshair in the middle of the screen, in three states:
// - Idle: nothing grabbable under it.
// - In Range: the tongue would reach and grab what's under it (the "Grab with tongue" prompt shows).
// - Locked: your tongue is stuck to something.
// It aims exactly like the tongue: the camera's crosshair ray finds the point, then the tongue's
// path from the mouth to that point, up to Tongue Range. Put it on the HUD canvas.
// It replaces the old "+" crosshair; the landing-marker cursor while aiming a jump stays.
public class TongueCrosshair : MonoBehaviour
{
    public enum State { Idle, InRange, Locked }

    // FrogCamera stops drawing its own "+" while one of these is showing.
    public static bool Showing { get; private set; }

    [Header("Crosshair")]
    [Tooltip("An Image in the middle of the screen (anchor and pivot centre).")]
    [SerializeField] Image crosshair;
    [SerializeField] Sprite idleSprite;
    [SerializeField] Sprite inRangeSprite;
    [SerializeField] Sprite lockedSprite;

    [Header("Prompt (optional)")]
    [Tooltip("Shown only while In Range, e.g. the \"LMB Grab with tongue\" image.")]
    [SerializeField] GameObject grabPrompt;

    [Header("Feel")]
    [Tooltip("How much bigger the crosshair pops when its state changes (1 = no pop).")]
    [SerializeField, Range(1f, 1.6f)] float pop = 1.25f;
    [Tooltip("Seconds for the pop to settle.")]
    [SerializeField, Min(0.01f)] float popTime = 0.15f;

    public State Current { get; private set; }

    static readonly RaycastHit[] hits = new RaycastHit[16];

    FrogNetworkDriver frog;
    FrogTongue tongue;
    FrogMotor motor;
    FrogElectricCharge charge;
    Rigidbody body;
    FrogCamera frogCamera;
    float nextSearch;
    float popLeft;

    void Awake()
    {
        Show(false);
        SetState(State.Idle, true);
    }

    void OnDisable()
    {
        Showing = false;
    }

    void LateUpdate()
    {
        if (frog == null && !FindLocalFrog())
        {
            Show(false);
            return;
        }

        // Hidden while the mouse is free (menus), while aiming a jump (the marker is the cursor),
        // and while dead.
        bool visible = Cursor.lockState == CursorLockMode.Locked
            && frogCamera != null && !frogCamera.AimingMarker
            && !motor.IsDead;
        Show(visible);
        if (!visible)
            return;

        SetState(Classify(), false);

        if (popLeft > 0f)
        {
            popLeft = Mathf.Max(0f, popLeft - Time.unscaledDeltaTime);
            float t = popLeft / popTime;
            crosshair.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, pop, t * t);
        }
    }

    State Classify()
    {
        TongueState tongueState = tongue.Sync.Value.State;
        if (tongueState == TongueState.Attached)
            return State.Locked;
        if (tongueState != TongueState.Idle)
            return Current == State.Locked ? State.Idle : Current; // mid-flight: keep what it was showing

        // 1. Where the crosshair points (the tongue fires at this point).
        Ray aim = frogCamera.AimRay;
        Vector3 aimPoint = aim.origin + aim.direction * 100f;
        if (Cast(aim.origin, aim.direction, 0f, 100f, out RaycastHit aimHit))
            aimPoint = aimHit.point;

        // 2. The tongue's own path from the mouth to that point.
        Vector3 mouth = tongue.Mouth != null ? tongue.Mouth.position : frog.transform.position;
        Vector3 path = aimPoint - mouth;
        float length = path.magnitude;
        if (length < 0.01f)
            return State.Idle;
        FrogTuning tuning = motor.Tuning;
        if (!Cast(mouth, path / length, tuning.tongueRadius, tuning.tongueRange, out RaycastHit hit))
            return State.Idle; // nothing within reach

        if (hit.collider.GetComponentInParent<TongueTarget>() != null)
            return State.InRange;
        // An electrified tongue also stuns enemies.
        if (charge != null && charge.HasCharge.Value && hit.collider.GetComponentInParent<EnemyStun>() != null)
            return State.InRange;
        return State.Idle;
    }

    // The nearest hit, skipping your own frog. Triggers are ignored, like the tongue does.
    bool Cast(Vector3 origin, Vector3 direction, float radius, float distance, out RaycastHit best)
    {
        int count = radius > 0f
            ? Physics.SphereCastNonAlloc(origin, radius, direction, hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            : Physics.RaycastNonAlloc(origin, direction, hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        best = default;
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            if (body != null && hits[i].rigidbody == body)
                continue;
            if (!found || hits[i].distance < best.distance)
            {
                best = hits[i];
                found = true;
            }
        }
        return found;
    }

    void SetState(State state, bool force)
    {
        if (state == Current && !force)
            return;
        Current = state;
        if (crosshair != null)
        {
            Sprite sprite = state switch
            {
                State.Locked => lockedSprite,
                State.InRange => inRangeSprite,
                _ => idleSprite,
            };
            if (sprite != null)
                crosshair.sprite = sprite;
        }
        if (grabPrompt != null && grabPrompt.activeSelf != (state == State.InRange))
            grabPrompt.SetActive(state == State.InRange);
        if (!force)
            popLeft = popTime;
    }

    void Show(bool show)
    {
        Showing = show && crosshair != null;
        if (crosshair != null && crosshair.enabled != show)
            crosshair.enabled = show;
        if (!show && grabPrompt != null && grabPrompt.activeSelf)
            grabPrompt.SetActive(false);
        if (!show)
        {
            popLeft = 0f;
            if (crosshair != null)
                crosshair.rectTransform.localScale = Vector3.one;
        }
    }

    // Your own frog, once it has spawned.
    bool FindLocalFrog()
    {
        if (Time.unscaledTime < nextSearch)
            return false;
        nextSearch = Time.unscaledTime + 0.5f;
        foreach (FrogNetworkDriver candidate in FindObjectsByType<FrogNetworkDriver>(FindObjectsInactive.Exclude))
        {
            if (!candidate.IsSpawned || !candidate.IsOwner)
                continue;
            frog = candidate;
            tongue = candidate.GetComponent<FrogTongue>();
            motor = candidate.GetComponent<FrogMotor>();
            charge = candidate.GetComponent<FrogElectricCharge>();
            body = candidate.GetComponent<Rigidbody>();
            frogCamera = Camera.main != null ? Camera.main.GetComponent<FrogCamera>() : null;
            if (tongue == null || motor == null)
            {
                frog = null;
                return false;
            }
            return true;
        }
        return false;
    }
}
