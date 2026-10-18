using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Collects the local player's input and hands it to the host.
// Only the host simulates frogs. Every other machine only displays them.
public class FrogNetworkDriver : NetworkBehaviour
{
    readonly NetworkVariable<FrogInputState> input = new(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Read by the host's FrogMotor / FrogTongue every physics step.
    public FrogInputState CurrentInput => input.Value;

    // Owner only: lets the visuals crouch the instant Space goes down,
    // instead of waiting a round trip for the host to confirm the charge.
    public bool LocalJumpHeld { get; private set; }
    public float LocalJumpHeldTime { get; private set; }

    FrogMotor motor;
    FrogTongue tongue;
    FrogCamera frogCamera;
    InputAction moveAction, jumpAction, tongueAction, reelAction, stickAction, breakFreeAction;

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        tongue = GetComponent<FrogTongue>();
    }

    public override void OnNetworkSpawn()
    {
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
    }

    void Update()
    {
        if (!IsSpawned || !IsOwner)
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
        if (!state.Equals(input.Value))
            input.Value = state;

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
            JumpPressedRpc();
        }
        if (jumpAction.WasReleasedThisFrame() && LocalJumpHeld)
            ReleaseJumpLocally();

        if (tongueAction.WasPressedThisFrame())
        {
            Ray aim = frogCamera != null ? frogCamera.AimRay : new Ray(transform.position, transform.forward);
            TonguePressedRpc(aim.origin, aim.direction);
        }
        if (stickAction.WasPressedThisFrame())
            StickPressedRpc();
        if (breakFreeAction.WasPressedThisFrame())
            BreakFreePressedRpc();
    }

    void ReleaseJumpLocally()
    {
        LocalJumpHeld = false;
        JumpReleasedRpc();
    }

    // ---- Owner -> host commands. The host only queues them; FrogMotor and
    // ---- FrogTongue apply them inside FixedUpdate (one simulation path).

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void JumpPressedRpc() => motor.PressJump();

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void JumpReleasedRpc() => motor.ReleaseJump();

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void TonguePressedRpc(Vector3 aimOrigin, Vector3 aimDirection) => tongue.PressTongue(aimOrigin, aimDirection);

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void StickPressedRpc() => motor.PressStick();

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void BreakFreePressedRpc() => FrogTongue.ReleaseEverythingHolding(NetworkObject);
}
