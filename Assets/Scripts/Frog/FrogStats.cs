using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// Health, hunger (doubles as stamina) and hydration. The host runs the numbers and
// every machine can read them; the HUD shows your own.
// Hunger and hydration drain over time, hunger faster while climbing and on every jump.
// Running out of water drains health; running low on food makes jumps lower instead.
// Being well fed and watered slowly heals. At zero health the frog respawns with full
// bars. Eating a fly adds its nutrition to hunger.
[RequireComponent(typeof(FrogMotor))]
public class FrogStats : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new();
    public readonly SyncVar<float> Hunger = new();
    public readonly SyncVar<float> Hydration = new();

    public float HealthFraction => Health.Value / Tuning.maxHealth;
    public float HungerFraction => Hunger.Value / Tuning.maxHunger;
    public float HydrationFraction => Hydration.Value / Tuning.maxHydration;


    // Host only: how high this frog's jumps go, 1 = normal. Below Tired Below Hunger the
    // frog runs out of puff, down to Hungry Jump Height when hunger is empty.
    // How high (and far) this frog's jumps go, 1 = normal. Works on every machine, so the
    // owner's charge meter and jump preview match the host. Below Tired Below Hunger the
    // frog runs out of puff, down to Hungry Jump Height when hunger is empty.
    public float JumpHeightMultiplier
    {
        get
        {
            FrogTuning t = Tuning;
            float fed = Hunger.Value;
            if (fed <= 0f)
                return t.hungryJumpHeight;
            if (t.tiredBelowHunger <= 0f)
                return 1f;
            return Mathf.Lerp(t.hungryJumpHeight, 1f, fed / (t.maxHunger * t.tiredBelowHunger));
        }
    }

    FrogMotor motor;
    FrogTuning Tuning => motor.Tuning;
    float health, hunger, hydration; // the host's exact values; the SyncVars are rounded copies
    bool wasCharging;
    float lastCharge;
    double lastHopStart; // each new hop has a new start time on the hop clock

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        TongueTarget.Eaten += OnEaten;
        RefillAll();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        TongueTarget.Eaten -= OnEaten;
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized || !IsSpawned)
            return;
        FrogTuning t = Tuning;
        float dt = Time.fixedDeltaTime;

        // Effort: climbing burns hunger, and so does each jump (more for a big one).
        float burn = t.hungerDrainPerSecond * dt;
        if (motor.Mode.Value == FrogMode.Climbing)
            burn += t.climbHungerPerSecond * dt;
        bool charging = motor.IsCharging.Value;
        if (wasCharging && !charging && motor.Mode.Value == FrogMode.Airborne)
            burn += t.jumpHungerCost * Mathf.Max(0.25f, lastCharge);
        if (charging)
            lastCharge = motor.ChargeFraction();
        wasCharging = charging;
        double hopStart = motor.Hop.Value.StartTime;
        if (hopStart != lastHopStart)
        {
            lastHopStart = hopStart;
            burn += t.hopHungerCost; // hop locomotion: every hop is a little effort too
        }

        hunger = Mathf.Max(0f, hunger - burn);
        hydration = Mathf.Max(0f, hydration - t.hydrationDrainPerSecond * dt);

        // No water hurts. No food doesn't (it weakens jumps, see JumpHeightMultiplier).
        // With both bars over half full the frog slowly heals.
        if (hydration <= 0f)
            health -= t.starvingDamagePerSecond * dt;
        else if (hunger > t.maxHunger * 0.5f && hydration > t.maxHydration * 0.5f)
            health = Mathf.Min(t.maxHealth, health + t.healthRegenPerSecond * dt);

        if (health <= 0f)
        {
            motor.Respawn();
            RefillAll();
        }
        Publish();
    }

    // ---- Host only. Hazards call Damage; food, springs and rest spots call Restore. ----

    public void Damage(float amount)
    {
        if (!IsServerInitialized)
            return;
        health -= amount; // FixedUpdate respawns the frog if this reaches zero
        Publish();
    }

    public void Restore(float healthAmount, float hungerAmount, float hydrationAmount)
    {
        if (!IsServerInitialized)
            return;
        FrogTuning t = Tuning;
        health = Mathf.Min(t.maxHealth, health + healthAmount);
        hunger = Mathf.Min(t.maxHunger, hunger + hungerAmount);
        hydration = Mathf.Min(t.maxHydration, hydration + hydrationAmount);
        Publish();
    }

    public void RefillAll()
    {
        if (!IsServerInitialized)
            return;
        FrogTuning t = Tuning;
        health = t.maxHealth;
        hunger = t.maxHunger;
        hydration = t.maxHydration;
        Publish();
    }

    void OnEaten(FrogTongue eater, TongueTarget food)
    {
        if (eater.gameObject == gameObject)
            Restore(0f, food.nutrition, 0f);
    }

    // Half-point steps: the bars still look smooth, with far fewer network updates.
    void Publish()
    {
        Health.Value = Mathf.Round(health * 2f) / 2f;
        Hunger.Value = Mathf.Round(hunger * 2f) / 2f;
        Hydration.Value = Mathf.Round(hydration * 2f) / 2f;
    }
}