using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

// Collects the local player's input and hands it to the host.
// Only the host simulates frogs. Every other machine only displays them.
public class FrogNetworkDriver : NetworkBehaviour
{
    // The host's copy of this player's input, shared with everyone (animation reads it).
    readonly SyncVar<FrogInputState> input = new(new SyncTypeSettings(0.001f));

    // Read by the host's FrogMotor / FrogTongue every physics step.
    public FrogInputState CurrentInput => input.Value;

    // Owner only: lets the visuals crouch the instant Space goes down,
    // instead of waiting a round trip for the host to confirm the charge.
    public bool LocalJumpHeld { get; private set; }
    public float LocalJumpHeldTime { get; private set; }

    // Owner only: the stick right now and whether hop mode is on, for the HUD and the
    // jump preview (the host's copy in CurrentInput arrives a moment later).
    public Vector2 LocalMove { get; private set; }
    public bool HopModeOn { get; private set; }

    // Owner only: where the next ground jump should land, as a flat offset from the frog in
    // metres (zero = straight up). FrogMotor.AimToward cuts it to what the charge can reach.
    public Vector3 LocalJumpTarget { get; private set; }

    Rigidbody frogRigidbody;
    FrogMotor motor;
    FrogTongue tongue;
    FrogCamera frogCamera;
    InputAction moveAction, jumpAction, tongueAction, reelAction, stickAction, breakFreeAction, hopAction;

    FrogInputState lastSent;
    uint lastSentTick;

    void Awake()
    {
        frogRigidbody = GetComponent<Rigidbody>();
        motor = GetComponent<FrogMotor>();
        tongue = GetComponent<FrogTongue>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
            return;

        // Project-wide actions (Edit > Project Settings > Input System Package).
        var actions = InputSystem.actions;
        moveAction = actions.FindAction("Player/Move", true);
        jumpAction = actions.FindAction("Player/Jump", true);
        tongueAction = actions.FindAction("Player/Tongue", true);
        reelAction = actions.FindAction("Player/Reel", true);
        stickAction = actions.FindAction("Player/Stick", true);
        breakFreeAction = actions.FindAction("Player/BreakFree", true);
        hopAction = actions.FindAction("Player/Sprint", true);
        HopModeOn = motor.Tuning.hopByDefault;

        frogCamera = Camera.main != null ? Camera.main.GetComponent<FrogCamera>() : null;
        if (frogCamera != null)
            frogCamera.SetTarget(transform);

        if (TestObserver.instance != null)
        {
            TestObserver.instance.RegisterLocalPlayer(this.gameObject, frogRigidbody, transform.position);
        }
    }

    void Update()
    {
        if (!IsSpawned || !IsOwner || moveAction == null)
            return;

        // Ignore input while the window is unfocused (two Multiplayer Play Mode
        // players on one PC) or while the cursor is free to click UI.
        bool active = Application.isFocused && Cursor.lockState == CursorLockMode.Locked;

        // Sprint switches hop mode on and off (or hops only while held, if Hop Toggle is off).
        if (!motor.Tuning.hopToggle)
            HopModeOn = active && hopAction.IsPressed();
        else if (active && hopAction.WasPressedThisFrame())
            HopModeOn = !HopModeOn;
        LocalMove = active ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;
        float viewYaw = frogCamera != null ? Mathf.Round(frogCamera.Yaw) : transform.eulerAngles.y;
        UpdateJumpTarget(active, viewYaw);

        var state = new FrogInputState
        {
            Move = LocalMove,
            ViewYaw = viewYaw,
            Reel = active && reelAction.IsPressed(),
            Hop = HopModeOn,
            // 2 cm steps: the host only needs it to turn the frog, so send it less often.
            JumpTarget = new Vector2(Mathf.Round(LocalJumpTarget.x * 50f), Mathf.Round(LocalJumpTarget.z * 50f)) / 50f,
        };
        // Send changes at most once per tick. A change held back this tick goes out next tick.
        if (!state.Equals(lastSent) && TimeManager.LocalTick != lastSentTick)
        {
            lastSent = state;
            lastSentTick = TimeManager.LocalTick;
            SubmitInput(state);
        }

        if (!active)
        {
            if (LocalJumpHeld)
                ReleaseJumpLocally();
            return;
        }

        if (LocalJumpHeld)
            LocalJumpHeldTime += Time.deltaTime;

        if (jumpAction.WasPressedThisFrame())
        {
            LocalJumpHeld = true;
            LocalJumpHeldTime = 0f;
            JumpPressed();
        }
        if (jumpAction.WasReleasedThisFrame() && LocalJumpHeld)
            ReleaseJumpLocally();

        if (tongueAction.WasPressedThisFrame())
        {
            Ray aim = frogCamera != null ? frogCamera.AimRay : new Ray(transform.position, transform.forward);
            TonguePressed(aim.origin, aim.direction);
        }
        if (stickAction.WasPressedThisFrame())
            StickPressed();
        if (breakFreeAction.WasPressedThisFrame())
            BreakFreePressed();
    }

    // While charging on the ground (with Aim Jumps With Mouse on), mouse / right stick look
    // moves the landing marker instead of the camera: sideways and further / closer, as seen
    // from the camera. It starts as far as possible the way the stick points when the charge
    // begins, or under the frog (straight up) when no direction is held.
    void UpdateJumpTarget(bool active, float viewYaw)
    {
        FrogTuning tuning = motor.Tuning;
        Quaternion view = Quaternion.Euler(0f, viewYaw, 0f);
        bool aiming = tuning.aimJumpsWithMouse && active && LocalJumpHeld && motor.Mode.Value == FrogMode.Grounded;
        if (frogCamera != null)
            frogCamera.LookAimsJump = aiming;

        if (aiming)
        {
            Vector2 look = frogCamera != null ? frogCamera.TakeJumpAimLook() : Vector2.zero;
            Vector3 move = view * new Vector3(look.x, 0f, look.y) * tuning.jumpAimPerDegree;
            LocalJumpTarget = Vector3.ClampMagnitude(LocalJumpTarget + move, tuning.maxJumpDistance);
        }
        else if (LocalMove.sqrMagnitude > 0.04f)
        {
            // Without mouse aiming the frog jumps the way it faces (the host turns it toward
            // the stick while charging).
            Vector3 direction = tuning.aimJumpsWithMouse
                ? view * new Vector3(LocalMove.x, 0f, LocalMove.y).normalized
                : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            LocalJumpTarget = direction * tuning.maxJumpDistance;
        }
        else
        {
            LocalJumpTarget = Vector3.zero;
        }
    }

    void ReleaseJumpLocally()
    {
        LocalJumpHeld = false;
        JumpReleased(new Vector2(LocalJumpTarget.x, LocalJumpTarget.z));
    }

    // ---- Owner -> host commands. The host only queues them; FrogMotor and
    // ---- FrogTongue apply them inside FixedUpdate (one simulation path).
    // ---- [ServerRpc] only accepts calls from this frog's owner by default.

    [ServerRpc]
    void SubmitInput(FrogInputState state) => input.Value = state;

    [ServerRpc]
    void JumpPressed() => motor.PressJump();

    [ServerRpc]
    void JumpReleased(Vector2 target) => motor.ReleaseJump(target);

    [ServerRpc]
    void TonguePressed(Vector3 aimOrigin, Vector3 aimDirection) => tongue.PressTongue(aimOrigin, aimDirection);

    [ServerRpc]
    void StickPressed() => motor.PressStick();

    [ServerRpc]
    void BreakFreePressed() => FrogTongue.ReleaseEverythingHolding(NetworkObject);
}