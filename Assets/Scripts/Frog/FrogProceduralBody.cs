using UnityEngine;

// Procedural "acting" for a frog model that has no animations or bones.
// Runs on every machine, reads only replicated state plus how the body moved on
// this screen, and only touches the Visual hierarchy, never the physics body.
public class FrogProceduralBody : MonoBehaviour
{
    [SerializeField] Transform visual; // offsets and tilts (pivot under the belly)
    [SerializeField] Transform squash; // scale only

    [Header("Breathing")]
    [SerializeField] float breathRate = 0.7f;   // breaths per second
    [SerializeField] float breathAmount = 0.03f;

    [Header("Hop-crawl")]
    [SerializeField] float hopLength = 0.35f;   // metres travelled per little hop
    [SerializeField] float hopHeight = 0.08f;
    [SerializeField] float hopPitch = 10f;

    [Header("Jump")]
    [SerializeField] float chargeSquash = 0.35f; // how flat a full charge makes the frog
    [SerializeField] float chargePitch = 12f;    // nose up while aiming
    [SerializeField] float airStretch = 0.25f;
    [SerializeField] float maxAirPitch = 45f;
    [SerializeField] float launchKick = 3f;
    [SerializeField] float landingKickPerSpeed = 0.6f;

    [Header("Lean and strain")]
    [SerializeField] float leanPerAccel = 1.2f;  // degrees per m/s^2
    [SerializeField] float maxLean = 15f;
    [SerializeField] float tongueLean = 10f;
    [SerializeField] float strainShake = 0.015f;

    [Header("Squash spring")]
    [SerializeField] float springStiffness = 180f;
    [SerializeField] float springDamping = 12f;

    FrogMotor motor;
    FrogClimb climb;
    FrogTongue tongue;
    FrogNetworkDriver driver;

    Vector3 visualBasePosition;
    Vector3 squashBaseScale;
    Vector3 lastPosition, velocity, smoothedAccel;
    FrogMode lastMode;
    float hopPhase, hopAmount, airAmount;
    float squashValue, squashSpeed;
    float pitch, roll;

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        climb = GetComponent<FrogClimb>();
        tongue = GetComponent<FrogTongue>();
        driver = GetComponent<FrogNetworkDriver>();
        visualBasePosition = visual.localPosition;
        squashBaseScale = squash.localScale;
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || !motor.IsSpawned)
            return;

        // Velocity from how the (interpolated) body moved on this screen, so remote frogs
        // are animated against exactly what this player sees.
        Vector3 previousVelocity = velocity;
        Vector3 rawVelocity = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;
        velocity = Vector3.Lerp(velocity, rawVelocity, 1f - Mathf.Exp(-20f * dt));
        smoothedAccel = Vector3.Lerp(smoothedAccel, (velocity - previousVelocity) / dt, 1f - Mathf.Exp(-8f * dt));

        FrogMode mode = motor.Mode.Value;
        bool supported = mode == FrogMode.Grounded || mode == FrogMode.Climbing;
        Vector3 up = transform.up;

        // ---- One-off kicks: launch stretch and landing squash ----
        if (mode != lastMode)
        {
            if (mode == FrogMode.Airborne && Vector3.Dot(velocity, Vector3.up) > 1f)
                squashSpeed += launchKick;
            if (lastMode == FrogMode.Airborne && supported)
                squashSpeed -= Mathf.Max(0f, -Vector3.Dot(previousVelocity, up)) * landingKickPerSpeed;
            lastMode = mode;
        }

        float charge = driver.IsOwner && driver.LocalJumpHeld && supported ? driver.LocalCharge : motor.ChargeFraction();

        // ---- Hop-crawl: a little hop every hopLength metres while walking ----
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        float planarSpeed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
        float moveFactor = supported ? Mathf.Clamp01(planarSpeed / motor.Tuning.moveSpeed) : 0f;
        hopAmount = Mathf.Lerp(hopAmount, moveFactor, 1f - Mathf.Exp(-10f * dt));
        if (supported && planarSpeed > 0.15f)
            hopPhase += planarSpeed / hopLength * dt;
        float hopWave = Mathf.Abs(Mathf.Sin(hopPhase * Mathf.PI)); // 0 -> 1 -> 0 per hop
        float hopOffset = hopWave * hopHeight * hopAmount;
        float hopTilt = -Mathf.Cos(hopPhase * Mathf.PI) * hopPitch * hopAmount; // nose up rising, down falling

        // ---- Squash spring: crouch while charging, small squish at each hop contact ----
        float squashTarget = -chargeSquash * charge - 0.08f * hopAmount * (1f - hopWave);
        squashSpeed += ((squashTarget - squashValue) * springStiffness - squashSpeed * springDamping) * dt;
        squashValue = Mathf.Clamp(squashValue + squashSpeed * dt, -0.6f, 0.6f);

        float breath = Mathf.Sin(Time.time * breathRate * 2f * Mathf.PI) * breathAmount * (1f - hopAmount);
        airAmount = Mathf.Lerp(airAmount, mode == FrogMode.Airborne ? Mathf.Clamp01(velocity.magnitude / 8f) : 0f, 1f - Mathf.Exp(-12f * dt));

        float s = squashValue + breath;
        float a = airAmount * airStretch;
        squash.localScale = Vector3.Scale(squashBaseScale, new Vector3(1f - s * 0.5f - a * 0.3f, 1f + s - a * 0.3f, 1f - s * 0.5f + a));

        // ---- Tilt: lean into acceleration on support, follow the arc in the air ----
        float targetPitch, targetRoll = 0f;
        if (mode == FrogMode.Airborne)
        {
            float vertical = Vector3.Dot(velocity, Vector3.up);
            float forward = Mathf.Max(1f, Mathf.Abs(Vector3.Dot(velocity, transform.forward)));
            targetPitch = Mathf.Clamp(-Mathf.Atan2(vertical, forward) * Mathf.Rad2Deg * 0.6f, -maxAirPitch, maxAirPitch);
        }
        else
        {
            Vector3 localAccel = transform.InverseTransformDirection(smoothedAccel);
            targetPitch = Mathf.Clamp(localAccel.z * leanPerAccel, -maxLean, maxLean) + hopTilt - chargePitch * charge;
            targetRoll = Mathf.Clamp(-localAccel.x * leanPerAccel, -maxLean, maxLean);
        }
        TongueSync tongueSync = tongue.Sync.Value;
        if (tongueSync.State == TongueState.Attached)
            targetPitch += tongueLean * tongueSync.Tension; // strain forward against the pull

        pitch = Mathf.Lerp(pitch, targetPitch, 1f - Mathf.Exp(-12f * dt));
        roll = Mathf.Lerp(roll, targetRoll, 1f - Mathf.Exp(-12f * dt));

        // ---- Grip strain: trembling when close to slipping off a wall ----
        Vector3 shake = Vector3.zero;
        float strain = mode == FrogMode.Climbing ? climb.GripStrain.Value : 0f;
        if (strain > 0.7f)
        {
            float amount = strainShake * Mathf.Clamp01((strain - 0.7f) / 0.3f);
            shake = new Vector3(Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f, 0f, Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f) * 2f * amount;
        }

        visual.localPosition = visualBasePosition + Vector3.up * hopOffset + shake;
        visual.localRotation = Quaternion.Euler(pitch, 0f, roll);
    }
}
