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

   
    LevelExit[] exits;

    void Start()
    {
        // Include switched-off ones: FishNet keeps level network objects off on joining
        // players until the host spawns them. BusiestExit skips any not spawned yet.
        exits = FindObjectsByType<LevelExit>(FindObjectsInactive.Include);
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

    // While charging: fills with the jump's power and says how high and far it will go.
    // Reads the button on this machine, so it reacts the instant Space goes down.
    void UpdateChargeMeter()
    {
        if (chargeMeter == null)
            return;
        bool ready = stats != null && stats.IsSpawned;
        bool supported = ready && (motor.Mode.Value == FrogMode.Grounded || motor.Mode.Value == FrogMode.Climbing);
        bool charging = ready && (motor.IsCharging.Value || (driver.LocalJumpHeld && supported));
        if (chargeMeter.activeSelf != charging)
            chargeMeter.SetActive(charging);
        if (!charging)
            return;

        float charge01 = motor.ChargeFraction();
        if (driver.LocalJumpHeld)
            charge01 = Mathf.Max(charge01, Mathf.Clamp01(driver.LocalJumpHeldTime / motor.Tuning.fullChargeTime));
        if (chargeFill != null)
            chargeFill.fillAmount = motor.Tuning.chargeCurve.Evaluate(charge01);
        if (chargeText == null)
            return;
        if (motor.Mode.Value == FrogMode.Climbing)
        {
            chargeText.text = "Wall jump";
            return;
        }
        float heightScale = stats.JumpHeightMultiplier;
        Vector2 reach = motor.GroundJumpReach(charge01, heightScale);
        float share = motor.AimToward(driver.LocalJumpTarget, charge01, heightScale).magnitude;
        chargeText.text = share > 0.01f
            ? $"Height {reach.x:0.0} m   Distance {reach.y * share:0.0} m"
            : $"Height {reach.x:0.0} m";
    }

    void UpdateMoveMode()
    {
        if (moveModeText == null || driver == null)
            return;
        moveModeText.text = motor.Tuning.hopLocomotion && driver.HopModeOn ? "HOP" : "RUN";
    }


    // Your own frog (the one you own), once PlayerSpawner has spawned it.
    static FrogStats FindLocalFrogStats()
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
            ? $"Next level in {exit.SecondsLeft:0}..."
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