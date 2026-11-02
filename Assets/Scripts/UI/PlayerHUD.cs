using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The local player's HUD: health, hunger and hydration bars, plus the level exit's
// "frogs at the exit" counter. Put one on a Screen Space canvas in each level.
// Purely local: it reads your own frog's FrogStats, which the host keeps up to date.
public class PlayerHUD : MonoBehaviour
{
    [Header("Bars (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] Image healthFill;
    [SerializeField] Image hungerFill;
    [SerializeField] Image hydrationFill;
    [SerializeField] float barSpeed = 1.5f;              // how fast a bar slides to its new value (full bars per second)
    [SerializeField, Range(0f, 1f)] float lowWarning = 0.25f; // a bar pulses below this

    [Header("Level exit (optional)")]
    [Tooltip("Hidden automatically in levels without a Level Exit.")]
    [SerializeField] TMP_Text exitCounter;

    [Header("Charge jump meter (optional)")]
    [Tooltip("The meter's root object. Shown only while charging a jump.")]
    [SerializeField] GameObject chargeMeter;
    [Tooltip("Image Type: Filled. Fills with the jump's power.")]
    [SerializeField] Image chargeFill;
    [Tooltip("Optional: how high and how far this charge will go.")]
    [SerializeField] TMP_Text chargeText;
    [Tooltip("Optional: shows RUN or HOP.")]
    [SerializeField] TMP_Text moveModeText;

    FrogStats stats;
    FrogMotor motor;
    FrogNetworkDriver driver;
    JumpPreview jumpPreview;


    LevelExit[] exits;

    void Start()
    {
        // Include switched-off ones: FishNet keeps level network objects off on joining
        // players until the host spawns them. BusiestExit skips any not spawned yet.
        exits = FindObjectsByType<LevelExit>(FindObjectsInactive.Include);


        // The meter is a bar that fills from left to right. It needs a Filled image with a
        // Source Image to show a fill at all.
        if (chargeFill != null)
        {
            chargeFill.type = Image.Type.Filled;
            chargeFill.fillMethod = Image.FillMethod.Horizontal;
            chargeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            if (chargeFill.sprite == null)
            {
                // No Source Image set: use a plain white square so the fill still shows.
                chargeFill.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
                Debug.LogWarning("PlayerHUD: Charge Fill has no Source Image, so it's a plain bar. Set one (UISprite) to style it.", chargeFill);
            }
        }
    }

    void Update()
    {
        if (stats == null)
        {
            stats = FindLocalFrogStats();
            if (stats != null)
            {
                motor = stats.GetComponent<FrogMotor>();
                driver = stats.GetComponent<FrogNetworkDriver>();
                jumpPreview = stats.GetComponent<JumpPreview>();
            }
        }

        if (stats != null && stats.IsSpawned)
        {
            float dt = Time.unscaledDeltaTime;
            UpdateBar(healthFill, stats.HealthFraction, dt);
            UpdateBar(hungerFill, stats.HungerFraction, dt);
            UpdateBar(hydrationFill, stats.HydrationFraction, dt);
        }
        UpdateChargeMeter();
        UpdateMoveMode();
        UpdateExitCounter();
    }

    // While charging: fills with the jump's power (swinging up and back down with Charge
    // Ping Pong) and says how high the jump goes and how far away the marker is. Uses this
    // machine's own button and charge, so it matches the preview exactly.
    void UpdateChargeMeter()
    {
        if (chargeMeter == null)
            return;
        bool ready = stats != null && stats.IsSpawned;
        bool supported = ready && (motor.Mode.Value == FrogMode.Grounded || motor.Mode.Value == FrogMode.Climbing);
        bool charging = supported && driver.LocalJumpHeld;
        if (chargeMeter.activeSelf != charging)
            chargeMeter.SetActive(charging);
        if (!charging)
            return;

        float charge01 = driver.LocalCharge;
        if (chargeFill != null)
            chargeFill.fillAmount = motor.Tuning.chargeCurve.Evaluate(charge01);
        if (chargeText == null)
            return;
        float height = motor.JumpApex(charge01, driver.LocalJumpTarget, stats.JumpHeightMultiplier);
        // Distance to the marker you placed (it stays put), or else to where the arc lands.
        Vector3? landing = driver.HasMarker ? driver.MarkerPoint
            : jumpPreview != null && jumpPreview.HasLanding ? jumpPreview.LandingPoint
            : (Vector3?)null;
        if (landing.HasValue)
        {
            float distance = Vector3.ProjectOnPlane(landing.Value - motor.transform.position, Vector3.up).magnitude;
            chargeText.text = $"Height {height:0.0} m   Distance {distance:0.0} m";
        }
        else
        {
            chargeText.text = $"Height {height:0.0} m";
        }
    }

    void UpdateMoveMode()
    {
        if (moveModeText == null || driver == null)
            return;
        moveModeText.text = motor.Tuning.hopLocomotion && driver.HopModeOn ? "HOP" : "RUN";
    }


    // Your own frog (the one you own), once PlayerSpawner has spawned it.
    public static FrogStats FindLocalFrogStats()
    {
        foreach (FrogStats frog in FindObjectsByType<FrogStats>())
        {
            if (frog.IsOwner)
                return frog;
        }
        return null;
    }

    void UpdateBar(Image fill, float value, float dt)
    {
        if (fill == null)
            return;
        value = Mathf.Clamp01(value);
        fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, value, barSpeed * dt);

        // Running low: pulse so it gets noticed.
        Color color = fill.color;
        color.a = value < lowWarning ? 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 5f)) : 1f;
        fill.color = color;
    }

    void UpdateExitCounter()
    {
        if (exitCounter == null)
            return;
        LevelExit exit = BusiestExit();
        bool show = exit != null;
        if (exitCounter.gameObject.activeSelf != show)
            exitCounter.gameObject.SetActive(show);
        if (!show)
            return;
        exitCounter.text = exit.SecondsLeft > 0f
             ? $"Leaving in {exit.SecondsLeft:0}..."
            : $"Frogs at the exit: {exit.PlayersInside} / {exit.PlayersNeeded}";


        // With several exits, show the one counting down, otherwise the one with the most
        // frogs in it (the first one while they're all empty).
        LevelExit BusiestExit()
        {
            LevelExit best = null;
            foreach (LevelExit candidate in exits)
            {
                if (candidate == null || !candidate.IsSpawned)
                    continue;
                if (best == null || Busyness(candidate) > Busyness(best))
                    best = candidate;
            }
            return best;
        }

        static float Busyness(LevelExit exit) => exit.SecondsLeft > 0f ? 1000f : exit.PlayersInside;
    }
}