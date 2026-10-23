using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

// Health, hunger (doubles as stamina) and hydration. The host runs the numbers and
// every machine can read them; the HUD shows your own.
// Hunger and hydration drain over time, hunger faster while climbing and on every jump.
// An empty bar drains health; being well fed and watered slowly heals. At zero health
// the frog respawns with full bars. Eating a fly adds its nutrition to hunger.
[RequireComponent(typeof(FrogMotor))]
public class FrogStats : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new();
    public readonly SyncVar<float> Hunger = new();
    public readonly SyncVar<float> Hydration = new();

    public float HealthFraction => Health.Value / Tuning.maxHealth;
    public float HungerFraction => Hunger.Value / Tuning.maxHunger;
    public float HydrationFraction => Hydration.Value / Tuning.maxHydration;

    FrogMotor motor;
    FrogTuning Tuning => motor.Tuning;
    float health, hunger, hydration; // the host's exact values; the SyncVars are rounded copies
    bool wasCharging;
    float lastCharge;

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

        hunger = Mathf.Max(0f, hunger - burn);
        hydration = Mathf.Max(0f, hydration - t.hydrationDrainPerSecond * dt);

        // Each empty bar hurts; with both bars over half full the frog slowly heals.
        int emptyBars = (hunger <= 0f ? 1 : 0) + (hydration <= 0f ? 1 : 0);
        if (emptyBars > 0)
            health -= t.starvingDamagePerSecond * emptyBars * dt;
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