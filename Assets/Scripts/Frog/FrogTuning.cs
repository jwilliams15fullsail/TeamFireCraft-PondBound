using UnityEngine;


// How a frog on the ground or in the air turns its body.
public enum FrogFacing : byte { MoveDirection, CameraWhenMoving, AlwaysCamera }

// Shared tuning values for every frog. Every frog prefab points at the same asset,
// so a designer can tweak feel in one place (even during Play Mode).
[CreateAssetMenu(menuName = "PondBound/Frog Tuning", fileName = "FrogTuning")]
public class FrogTuning : ScriptableObject
{
    [Header("Ground movement")]
    public float moveSpeed = 3.5f;
    public float groundAcceleration = 30f;
    public float airAcceleration = 6f;
    public float turnSpeed = 540f;            // degrees per second
    [Tooltip("MoveDirection: turn toward where you walk. CameraWhenMoving: face the camera while moving. AlwaysCamera: always face the camera, like a shooter.")]
    public FrogFacing facing = FrogFacing.AlwaysCamera;
    public float maxSlopeAngle = 50f;
    public float groundProbeDistance = 0.12f;
    public float coyoteTime = 0.12f;          // grace period after walking off a ledge


    [Header("Hop locomotion (Jump2 moves the frog)")]
    [Tooltip("Move by hopping, carried by the Jump2 animation's own forward travel. Off = the old run.")]
    public bool hopLocomotion = true;
    [Tooltip("Seconds per hop at full stick. Jump2's own length is 0.9; bigger = slower, calmer hops.")]
    public float hopDuration = 1.0f;
    [Tooltip("Seconds per hop at a light stick tilt.")]
    public float slowHopDuration = 1.4f;
    [Tooltip("Metres per hop at full stick. Jump2's feet travel 1.43.")]
    public float hopStride = 1.43f;
    [Tooltip("Stride at a light stick tilt, as a fraction of Hop Stride.")]
    [Range(0.2f, 1f)] public float minHopStride = 0.5f;
    [Tooltip("Skip this much of the opening crouch so a hop reacts the moment you push.")]
    [Range(0f, 0.2f)] public float hopStartPhase = 0.06f;
    [Tooltip("Let go before this point of a hop (still crouching) and it's cancelled instead of leaping.")]
    [Range(0f, 0.3f)] public float hopCancelBefore = 0.14f;
    [Tooltip("Facing further than this from the stick direction: turn on the spot before hopping.")]
    public float hopMaxStartAngle = 70f;
    [Tooltip("How tightly the body follows the hop's speed (m/s per second).")]
    public float hopAcceleration = 80f;
    [Tooltip("Extra delay (s) for the hop animation on players' screens other than the host's, on top of the network delay. Raise it if remote feet slide forward, lower it if they slide back.")]
    public float remoteHopDelay = 0.04f;
    [Tooltip("Jump2's forward travel (0 to 1) across the clip (0 to 1), measured from the clip.")]
    public AnimationCurve hopTravel = DefaultHopTravel();

    // Feet travel of the Jump2 clip, sampled every 4% of its length (1.434 m in total).
    static AnimationCurve DefaultHopTravel()
    {
        float[] travel =
        {
            0f, 0f, 0f, 0.0063f, 0.0209f, 0.0314f, 0.0628f, 0.1192f, 0.1960f, 0.2894f, 0.3724f, 0.4310f, 0.4874f,
            0.5635f, 0.6653f, 0.7810f, 0.8905f, 0.9533f, 0.9819f, 0.9979f, 1f, 1f, 1f, 1f, 1f, 1f,
        };
        // Straight lines between samples: never overshoots, so the frog never steps backwards.
        float step = 1f / (travel.Length - 1);
        var keys = new Keyframe[travel.Length];
        for (int i = 0; i < travel.Length; i++)
        {
            float inSlope = i > 0 ? (travel[i] - travel[i - 1]) / step : 0f;
            float outSlope = i < travel.Length - 1 ? (travel[i + 1] - travel[i]) / step : 0f;
            keys[i] = new Keyframe(i * step, travel[i], inSlope, outSlope);
        }
        return new AnimationCurve(keys);
    }

    [Header("Charged jump")]
    public float fullChargeTime = 1f;
    public float minJumpSpeed = 3.5f;         // a quick tap
    public float maxJumpSpeed = 9f;           // a full charge
    public AnimationCurve chargeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(0f, 1f)] public float forwardJumpRatio = 0.5f; // 0 = straight up, 1 = flat leap
    public float wallJumpUpBias = 0.8f;       // how much "up" is mixed into a wall jump

    [Header("Sticking and climbing")]
    public float stickReach = 0.3f;           // how far from the body a surface can be grabbed
    public float climbSpeed = 2f;
    public float gripStrength = 25f;          // newtons the toe pads can hold
    public float gripSlipTolerance = 0.35f;   // seconds of overload before the frog lets go
    public float reattachCooldown = 0.3f;
    public float maxSurfaceTurn = 60f;        // biggest normal change per step while following bumps and outside edges
    public float maxCornerTurn = 100f;        // biggest angle when climbing into an inside corner (90 = square room corner)
    public float cornerLookAhead = 0.15f;     // how far ahead (m) to look for an inside corner or the floor
    [Range(-1f, 1f)] public float minWallUpDot = -0.25f; // below this = ceiling, not allowed yet
    [Range(-1f, 1f)] public float maxWallUpDot = 0.75f;  // above this = floor, just walk on it
    
    [Tooltip("Jumping or falling into a climbable wall grabs it without pressing Stick.")]
    public bool autoClimb = true;
    [Tooltip("How far ahead (m) a frog in the air looks for a wall to grab. It looks further when flying fast.")]
    public float autoClimbReach = 0.25f;
    [Tooltip("Speed (m/s) into the wall needed to grab it when the stick isn't pushing toward it.")]
    public float autoClimbMinApproach = 1f;
    [Tooltip("Seconds after letting go of a wall before grabbing in the air works again.")]
    public float autoClimbCooldown = 0.5f;


    [Header("Tongue")]
    public float tongueRange = 7f;
    public float tongueExtendSpeed = 35f;
    public float tongueRetractSpeed = 45f;
    public float tongueRadius = 0.06f;
    public float tongueStiffness = 60f;       // spring strength (N per metre of stretch)
    public float tongueDamping = 6f;
    public float tongueMaxTension = 60f;      // force cap so the spring can never explode
    public float tongueBraceForce = 60f;      // pull (N) a grounded frog's feet absorb, so it can drag heavy things
    public float tongueBreakDistance = 10f;
    public float reelSpeed = 3f;              // metres per second of rest length removed
    public float minRestLength = 0.6f;
    public float foodReelSpeed = 8f;
    public float eatDistance = 0.45f;
    public float blockedBreakTime = 0.15f;    // tongue snaps if scenery blocks it this long


    [Header("Survival")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;            // hunger doubles as stamina: effort burns it
    public float maxHydration = 100f;
    public float hungerDrainPerSecond = 0.5f;
    public float hydrationDrainPerSecond = 0.8f;
    public float jumpHungerCost = 3f;         // for a full-charge jump; small hops cost less
    public float hopHungerCost = 0.3f;        // each step of hop locomotion
    public float climbHungerPerSecond = 1f;
    public float starvingDamagePerSecond = 2f; // while hydration is empty (empty hunger weakens jumps instead)
    public float healthRegenPerSecond = 0.5f; // while hunger and hydration are both above half
    [Tooltip("Jump height when hunger is empty, as a fraction of normal (0.5 = half as high).")]
    [Range(0.1f, 1f)] public float hungryJumpHeight = 0.5f;
    [Tooltip("Jumps start getting lower below this much hunger (0.25 = a quarter full). 0 = only once it's empty.")]
    [Range(0f, 1f)] public float tiredBelowHunger = 0.25f;
}
