using System.Collections.Generic;
using UnityEngine;

// Drives the frog's Animator from replicated gameplay state (FrogMotor mode, charge,
// input, tongue state) plus how the body actually moved on this screen. Runs on every
// machine and only reads, so no animation data is sent over the network and the
// physics body is never moved by animation.
public class FrogAnimatorDriver : MonoBehaviour
{
    [SerializeField] Animator animator;

    [Header("Locomotion")]
    [Tooltip("Body speed (m/s) at which the run clip plays at normal speed.")]
    [SerializeField] float runClipSpeed = 3.5f;
    [Tooltip("Climbing speed (m/s) at which the run clip plays at normal speed on walls.")]
    [SerializeField] float climbClipSpeed = 2f;
    [SerializeField] float minRunPlayback = 0.6f;
    [SerializeField] float maxRunPlayback = 1.6f;
    [Tooltip("Turn rate (degrees per second) that fully plays a rotate clip.")]
    [SerializeField] float fullTurnRate = 270f;
    [SerializeField] float parameterDamping = 0.1f;

    [Header("Jump clip timing (normalized 0-1, read from the clip preview)")]
    [SerializeField, Range(0f, 1f)] float crouchTime = 0f;       // deepest crouch (both clips)
    [Tooltip("Running jump (Jump2): feet leave the ground.")]
    [SerializeField, Range(0f, 1f)] float liftoffTime = 0.26f;
    [Tooltip("Running jump (Jump2): feet touch the ground again.")]
    [SerializeField, Range(0f, 1f)] float touchdownTime = 0.7f;
    [Tooltip("Standing jump (Jump1): feet leave the ground.")]
    [SerializeField, Range(0f, 1f)] float idleLiftoffTime = 0.25f;
    [Tooltip("Standing jump (Jump1): feet touch the ground again.")]
    [SerializeField, Range(0f, 1f)] float idleTouchdownTime = 0.65f;
    [SerializeField] float takeoffDuration = 0.12f;              // seconds from crouch to liftoff pose
    [SerializeField] float landingDuration = 0.25f;              // seconds from touchdown to clip end
    [Tooltip("How fast the jump pose may move through the clip (clip lengths per second). Stops pops.")]
    [SerializeField] float phaseFollowRate = 3f;
    [Tooltip("Animator state that plays the running jump; its forward travel is cancelled.")]
    [SerializeField] string runJumpStateName = "JumpRun";

    [Header("Keep the jump clips in place (physics moves the frog)")]
    [Tooltip("Bone the jump clips lift (DEF_Spine_Root). Its rise is cancelled in the air.")]
    [SerializeField] Transform liftBone;
    [SerializeField] float liftBlendSpeed = 8f;
    [Tooltip("Bones whose average marks where the frog stands; the running jump's travel is measured from them.")]
    [SerializeField] string[] footBonePrefixes = { "DEF_foot", "DEF_hand_finger" };

    [Header("Tongue (Head layer)")]
    [SerializeField] string headLayerName = "Head";
    [Tooltip("Point in the Lang clip where the mouth starts to open.")]
    [SerializeField, Range(0f, 1f)] float tongueOpenStart = 0.35f;
    [Tooltip("Point in the Lang clip held while the tongue is out (mouth open, tongue out).")]
    [SerializeField, Range(0f, 1f)] float tongueHoldTime = 0.5f;
    [SerializeField] float tonguePhaseRate = 1.5f;               // clip lengths per second
    [SerializeField] float headBlendSpeed = 8f;

    static readonly int SpeedId = Animator.StringToHash("Speed");
    static readonly int TurnId = Animator.StringToHash("Turn");
    static readonly int MoveAnimSpeedId = Animator.StringToHash("MoveAnimSpeed");
    static readonly int InJumpId = Animator.StringToHash("InJump");
    static readonly int RunJumpId = Animator.StringToHash("RunJump");
    static readonly int JumpPhaseId = Animator.StringToHash("JumpPhase");
    static readonly int TonguePhaseId = Animator.StringToHash("TonguePhase");
    static readonly int TongueAttachedId = Animator.StringToHash("TongueAttached");

    enum JumpStep { None, Charging, Takeoff, Air, Landing }

    FrogMotor motor;
    FrogNetworkDriver driver;
    FrogTongue tongue;
    Transform model;
    Transform[] footBones;
    int runJumpStateHash;

    Vector3 lastPosition, velocity;
    Quaternion lastRotation;
    float turnRate;
    FrogMode lastMode;
    bool wasCharging;

    JumpStep jumpStep;
    bool runJump;
    float stepTimer, jumpPhase, targetPhase, airProgress, peakUpSpeed;

    Vector3 modelBasePosition, restFeet;
    float restLiftHeight, liftWeight;

    int headLayer = -1;
    bool tongueShowing;
    float tonguePhase, headWeight;

    float Liftoff => runJump ? liftoffTime : idleLiftoffTime;
    float Touchdown => runJump ? touchdownTime : idleTouchdownTime;

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        driver = GetComponent<FrogNetworkDriver>();
        tongue = GetComponent<FrogTongue>();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        runJumpStateHash = Animator.StringToHash(runJumpStateName);
    }

    void Start()
    {
        model = animator.transform;
        modelBasePosition = model.localPosition;

        var feet = new List<Transform>();
        foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
        {
            foreach (string prefix in footBonePrefixes)
            {
                if (t.name.StartsWith(prefix))
                {
                    feet.Add(t);
                    break;
                }
            }
        }
        footBones = feet.ToArray();

        // The standing pose, measured before any animation plays.
        restFeet = FeetInModelSpace();
        if (liftBone != null)
            restLiftHeight = model.InverseTransformPoint(liftBone.position).y;

        headLayer = animator.GetLayerIndex(headLayerName);
        if (headLayer >= 0)
            animator.SetLayerWeight(headLayer, 0f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || !motor.IsSpawned)
            return;

        MeasureMotion(dt);

        FrogMode mode = motor.Mode.Value;
        bool supported = mode == FrogMode.Grounded || mode == FrogMode.Climbing;

        UpdateLocomotion(mode, supported, dt);
        UpdateJump(mode, supported, dt);
        UpdateTongue(dt);

        lastMode = mode;
    }

    // Runs after the Animator has posed the frog. The jump clips move the body on
    // their own (Jump2 travels 1.4 m forward and stays there), but physics decides
    // where the frog really is, so the model is shifted back by however far the clip
    // moved it. Feet stay where the physics body is, and nothing slides back later.
    void LateUpdate()
    {
        if (model == null)
            return;

        Vector3 offset = Vector3.zero;

        // Height: cancel the clip's hop while airborne.
        bool airborne = jumpStep == JumpStep.Takeoff || jumpStep == JumpStep.Air;
        liftWeight = Mathf.MoveTowards(liftWeight, airborne ? 1f : 0f, liftBlendSpeed * Time.deltaTime);
        if (liftBone != null)
            offset.y = liftWeight * Mathf.Max(0f, model.InverseTransformPoint(liftBone.position).y - restLiftHeight);

        // Travel: cancel the running jump's forward movement whenever its pose is
        // showing, including the blend back to Locomotion.
        if (RunJumpPoseShowing() && footBones.Length > 0)
        {
            Vector3 travel = FeetInModelSpace() - restFeet;
            offset.x = travel.x;
            offset.z = travel.z;
        }

        model.localPosition = modelBasePosition - model.localRotation * Vector3.Scale(offset, model.localScale);
    }

    bool RunJumpPoseShowing()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == runJumpStateHash)
            return true;
        return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == runJumpStateHash;
    }

    Vector3 FeetInModelSpace()
    {
        Vector3 sum = Vector3.zero;
        foreach (Transform t in footBones)
            sum += model.InverseTransformPoint(t.position);
        return footBones.Length > 0 ? sum / footBones.Length : Vector3.zero;
    }

    // Velocity and turn rate from the (interpolated) transform, so remote frogs are
    // animated against exactly what this player sees.
    void MeasureMotion(float dt)
    {
        Vector3 rawVelocity = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;
        velocity = Vector3.Lerp(velocity, rawVelocity, 1f - Mathf.Exp(-15f * dt));

        // Signed turn around the body's own up axis (works on walls too).
        Quaternion delta = transform.rotation * Quaternion.Inverse(lastRotation);
        lastRotation = transform.rotation;
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
            angle -= 360f;
        float rawTurn = float.IsNaN(axis.x) ? 0f : angle * Vector3.Dot(axis, transform.up) / dt;
        turnRate = Mathf.Lerp(turnRate, rawTurn, 1f - Mathf.Exp(-10f * dt));
    }

    void UpdateLocomotion(FrogMode mode, bool supported, float dt)
    {
        // Ground and walls have different top speeds; each one counts as a full run.
        bool climbing = mode == FrogMode.Climbing;
        float topSpeed = climbing ? motor.Tuning.climbSpeed : motor.Tuning.moveSpeed;
        float clipSpeed = climbing ? climbClipSpeed : runClipSpeed;

        float speed = Vector3.ProjectOnPlane(velocity, transform.up).magnitude;
        float speed01 = supported ? Mathf.Clamp01(speed / Mathf.Max(0.01f, topSpeed)) : 0f;
        float turn = supported ? Mathf.Clamp(turnRate / fullTurnRate, -1f, 1f) : 0f;

        animator.SetFloat(SpeedId, speed01, parameterDamping, dt);
        animator.SetFloat(TurnId, turn, parameterDamping, dt);

        // Speed up or slow down the run cycle so the feet match the body's speed.
        float playback = speed > 0.2f ? Mathf.Clamp(speed / Mathf.Max(0.01f, clipSpeed), minRunPlayback, maxRunPlayback) : 1f;

        // Facing the camera, the frog can back up: play the run cycle in reverse then,
        // so it doesn't moonwalk.
        if (speed > 0.2f && Vector3.Dot(Vector3.ProjectOnPlane(velocity, transform.up), transform.forward) < -0.5f * speed)
            playback = -playback;
        animator.SetFloat(MoveAnimSpeedId, playback);
    }

    // One jump clip covers crouch, takeoff, flight and landing. Instead of playing it
    // at a fixed speed, JumpPhase scrubs it to match the physics: charge sets the
    // crouch depth, vertical speed picks the flight pose, landing plays the rest.
    // The shown pose follows its target at a limited rate, so nothing ever pops.
    void UpdateJump(FrogMode mode, bool supported, float dt)
    {
        bool charging = motor.IsCharging.Value || (driver.IsOwner && driver.LocalJumpHeld && supported);
        float charge = motor.ChargeFraction();
        if (driver.IsOwner && driver.LocalJumpHeld)
            charge = Mathf.Max(charge, Mathf.Clamp01(driver.LocalJumpHeldTime / motor.Tuning.fullChargeTime));

        // Hop locomotion: while this frog hops along the ground, the running jump plays
        // straight from the shared hop clock, so it lines up with the body on every screen.
        // Jump2 ends in its own start pose, so chained hops loop without a seam.
        if (supported && jumpStep != JumpStep.Takeoff && jumpStep != JumpStep.Air && motor.TryGetHopPhase(out float hop))
        {
            jumpStep = JumpStep.None;
            runJump = motor.Tuning.hopClip == HopClip.Jump2Running;
            jumpPhase = targetPhase = hop;
            wasCharging = charging;
            animator.SetBool(InJumpId, true);
            animator.SetBool(RunJumpId, runJump);
            animator.SetFloat(JumpPhaseId, hop);
            return;
        }


        bool leftSupport = mode == FrogMode.Airborne && lastMode != FrogMode.Airborne;
        float upSpeed = Vector3.Dot(velocity, Vector3.up);
        JumpStep before = jumpStep;

        switch (jumpStep)
        {
            case JumpStep.None:
            case JumpStep.Charging:
                if (leftSupport)
                {
                    // Same rule FrogMotor uses when it launches: a held direction means a leap.
                    // Walking off a ledge counts as running if the frog was moving.
                    runJump = wasCharging
                        ? driver.CurrentInput.Move.sqrMagnitude > 0.04f
                        : Vector3.ProjectOnPlane(velocity, Vector3.up).magnitude > 1f;
                    BeginAir(wasCharging ? JumpStep.Takeoff : JumpStep.Air);
                }
                else if (charging && supported)
                {
                    jumpStep = JumpStep.Charging;
                    runJump = false;
                    targetPhase = Mathf.Lerp(0f, crouchTime, charge);
                }
                else
                    jumpStep = JumpStep.None;
                break;

            case JumpStep.Takeoff:
                stepTimer += dt;
                targetPhase = Mathf.Lerp(crouchTime, Liftoff, stepTimer / takeoffDuration);
                if (supported && stepTimer > takeoffDuration)
                    BeginLanding();
                else if (stepTimer >= takeoffDuration)
                    jumpStep = JumpStep.Air;
                break;

            case JumpStep.Air:
                // Rising fast = just after liftoff, falling fast = about to touch down.
                peakUpSpeed = Mathf.Max(peakUpSpeed, upSpeed, 2f);
                airProgress = Mathf.Max(airProgress, Mathf.InverseLerp(peakUpSpeed, -peakUpSpeed, upSpeed));
                targetPhase = Mathf.Lerp(Liftoff, Touchdown, airProgress);
                if (supported)
                    BeginLanding();
                break;

            case JumpStep.Landing:
                stepTimer += dt;
                targetPhase = Mathf.Lerp(Touchdown, 1f, stepTimer / landingDuration);
                if (leftSupport)
                    BeginAir(JumpStep.Air);
                else if (stepTimer >= landingDuration && Mathf.Approximately(jumpPhase, 1f))
                    jumpStep = JumpStep.None;
                break;
        }

        // Entering the jump state starts right at the target; after that, glide to it.
        jumpPhase = before == JumpStep.None ? targetPhase : Mathf.MoveTowards(jumpPhase, targetPhase, phaseFollowRate * dt);

        wasCharging = charging;
        animator.SetBool(InJumpId, jumpStep != JumpStep.None);
        animator.SetBool(RunJumpId, runJump);
        animator.SetFloat(JumpPhaseId, jumpPhase);
    }

    void BeginAir(JumpStep step)
    {
        jumpStep = step;
        stepTimer = 0f;
        airProgress = 0f;
        peakUpSpeed = 0f;
        targetPhase = step == JumpStep.Air ? Liftoff : crouchTime;
    }

    void BeginLanding()
    {
        jumpStep = JumpStep.Landing;
        stepTimer = 0f;
        targetPhase = Touchdown;
    }

    // While the tongue is out, hold the Lang clip at the point where the mouth is open
    // and the tongue is out; when it comes back, play the rest of the clip to close.
    // The Head layer only covers head and tongue bones, so the legs keep running.
    void UpdateTongue(float dt)
    {
        if (headLayer < 0)
            return;

        bool tongueOut = tongue.Sync.Value.State != TongueState.Idle;
        if (tongueOut && !tongueShowing)
        {
            tongueShowing = true;
            tonguePhase = tongueOpenStart;
        }
        if (tongueShowing)
        {
            tonguePhase = Mathf.MoveTowards(tonguePhase, tongueOut ? tongueHoldTime : 1f, tonguePhaseRate * dt);
            if (!tongueOut && tonguePhase >= 1f)
                tongueShowing = false;
        }

        headWeight = Mathf.MoveTowards(headWeight, tongueShowing ? 1f : 0f, headBlendSpeed * dt);
        animator.SetLayerWeight(headLayer, headWeight);
        animator.SetFloat(TonguePhaseId, tonguePhase);
        animator.SetBool(TongueAttachedId, tongue.Sync.Value.State == TongueState.Attached);
    }
}