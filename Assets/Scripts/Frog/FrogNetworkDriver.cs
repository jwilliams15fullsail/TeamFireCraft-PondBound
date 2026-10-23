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

    Rigidbody frogRigidbody;
    FrogMotor motor;
    FrogTongue tongue;
    FrogCamera frogCamera;
    InputAction moveAction, jumpAction, tongueAction, reelAction, stickAction, breakFreeAction;
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

        var state = new FrogInputState
        {
            Move = active ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero,
            ViewYaw = frogCamera != null ? Mathf.Round(frogCamera.Yaw) : transform.eulerAngles.y,
            Reel = active && reelAction.IsPressed(),
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

    void ReleaseJumpLocally()
    {
        LocalJumpHeld = false;
        JumpReleased();
    }

    // ---- Owner -> host commands. The host only queues them; FrogMotor and
    // ---- FrogTongue apply them inside FixedUpdate (one simulation path).
    // ---- [ServerRpc] only accepts calls from this frog's owner by default.

    [ServerRpc]
    void SubmitInput(FrogInputState state) => input.Value = state;

    [ServerRpc]
    void JumpPressed() => motor.PressJump();

    [ServerRpc]
    void JumpReleased() => motor.ReleaseJump();

    [ServerRpc]
    void TonguePressed(Vector3 aimOrigin, Vector3 aimDirection) => tongue.PressTongue(aimOrigin, aimDirection);

    [ServerRpc]
    void StickPressed() => motor.PressStick();

    [ServerRpc]
    void BreakFreePressed() => FrogTongue.ReleaseEverythingHolding(NetworkObject);
}