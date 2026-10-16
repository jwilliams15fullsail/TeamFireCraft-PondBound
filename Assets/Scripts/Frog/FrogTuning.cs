using UnityEngine;

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
    public float maxSlopeAngle = 50f;
    public float groundProbeDistance = 0.12f;
    public float coyoteTime = 0.12f;          // grace period after walking off a ledge

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

    [Header("Tongue")]
    public float tongueRange = 7f;
    public float tongueExtendSpeed = 35f;
    public float tongueRetractSpeed = 45f;
    public float tongueRadius = 0.06f;
    public float tongueStiffness = 60f;       // spring strength (N per metre of stretch)
    public float tongueDamping = 6f;
    public float tongueMaxTension = 60f;      // force cap so the spring can never explode
    public float tongueBreakDistance = 10f;
    public float reelSpeed = 3f;              // metres per second of rest length removed
    public float minRestLength = 0.6f;
    public float foodReelSpeed = 8f;
    public float eatDistance = 0.45f;
    public float blockedBreakTime = 0.15f;    // tongue snaps if scenery blocks it this long
}
