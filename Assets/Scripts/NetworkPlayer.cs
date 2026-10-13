using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// Owner-authoritative player: each client moves its own capsule and
// NetworkTransform (Owner authority) replicates it to everyone else.
[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotationSpeed = 720f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -20f;

    static readonly Color[] PlayerColors = { new Color(0.2f, 0.5f, 1f), new Color(1f, 0.35f, 0.25f) };

    CharacterController controller;
    float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        // Tint by client id so the two players are easy to tell apart.
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = PlayerColors[(int)(OwnerClientId % (ulong)PlayerColors.Length)];

        if (IsOwner)
        {
            // Offset spawn so players don't overlap.
            controller.enabled = false;
            transform.position = new Vector3(OwnerClientId % 2 == 0 ? -2f : 2f, 1f, 0f);
            controller.enabled = true;

            var follow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
            if (follow != null)
                follow.Target = transform;
        }
        else
        {
            // Remote players are driven by NetworkTransform, not physics.
            controller.enabled = false;
        }
    }

    void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        Vector2 input = ReadMove();
        Vector3 move = new Vector3(input.x, 0f, input.y);

        // Move relative to the camera's facing.
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            move = forward * input.y + right * input.x;
        }
        move = Vector3.ClampMagnitude(move, 1f);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            if (JumpPressed())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;

        controller.Move((move * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);

        // Respawn if we fall off the map.
        if (transform.position.y < -20f)
        {
            controller.enabled = false;
            transform.position = new Vector3(0f, 2f, 0f);
            controller.enabled = true;
            verticalVelocity = 0f;
        }
    }

    // Only read input while the game window has focus, so two editor
    // instances (Multiplayer Play Mode) don't both react to the same keys.
    static bool HasFocus => Application.isFocused;

    static Vector2 ReadMove()
    {
        if (!HasFocus)
            return Vector2.zero;

        Vector2 v = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
        }
        var pad = Gamepad.current;
        if (pad != null)
            v += pad.leftStick.ReadValue();
        return Vector2.ClampMagnitude(v, 1f);
    }

    static bool JumpPressed()
    {
        if (!HasFocus)
            return false;

        var kb = Keyboard.current;
        var pad = Gamepad.current;
        return (kb != null && kb.spaceKey.wasPressedThisFrame)
            || (pad != null && pad.buttonSouth.wasPressedThisFrame);
    }
}
