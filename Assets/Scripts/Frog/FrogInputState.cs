using System;
using Unity.Netcode;
using UnityEngine;

// The continuous part of a player's input. The owner writes it every frame and the
// host reads it during simulation. Button presses are sent separately as RPCs so a
// quick tap can never fall between two network ticks.
public struct FrogInputState : INetworkSerializable, IEquatable<FrogInputState>
{
    public Vector2 Move;   // WASD / left stick, each axis -1..1
    public float ViewYaw;  // camera yaw in degrees, so "forward" means "where my camera looks"
    public bool Reel;      // right mouse / left trigger held

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Move);
        serializer.SerializeValue(ref ViewYaw);
        serializer.SerializeValue(ref Reel);
    }

    public bool Equals(FrogInputState other)
    {
        return Move == other.Move && Mathf.Approximately(ViewYaw, other.ViewYaw) && Reel == other.Reel;
    }

    // Camera-relative directions on the horizontal plane.
    public Vector3 ViewForward => Quaternion.Euler(0f, ViewYaw, 0f) * Vector3.forward;
    public Vector3 ViewRight => Quaternion.Euler(0f, ViewYaw, 0f) * Vector3.right;
    public Vector3 WorldMove => Vector3.ClampMagnitude(ViewForward * Move.y + ViewRight * Move.x, 1f);
}
