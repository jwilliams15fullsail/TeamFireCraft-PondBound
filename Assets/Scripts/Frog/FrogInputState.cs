using System;
using UnityEngine;

// The continuous part of a player's input. The owner sends it to the host whenever it
// changes (at most once per tick) and the host keeps it in a SyncVar, so every machine
// can read it. Button presses go separately as RPCs so a quick tap can never be lost.
// FishNet writes this struct over the network automatically (its public fields).
public struct FrogInputState : IEquatable<FrogInputState>
{
    public Vector2 Move;   // WASD / left stick, each axis -1..1
    public float ViewYaw;  // camera yaw in degrees, so "forward" means "where my camera looks"
    public bool Reel;      // right mouse / left trigger held
    public bool Hop;       // hop mode on (Sprint switches it, or holds it, see FrogTuning)
    public Vector2 JumpTarget; // landing marker while charging: world x/z offset from the frog, metres

    public bool Equals(FrogInputState other)
    {
        return Move == other.Move && Mathf.Approximately(ViewYaw, other.ViewYaw) && Reel == other.Reel
            && Hop == other.Hop && JumpTarget == other.JumpTarget;
    }

    // Camera-relative directions on the horizontal plane.
    public Vector3 ViewForward => Quaternion.Euler(0f, ViewYaw, 0f) * Vector3.forward;
    public Vector3 ViewRight => Quaternion.Euler(0f, ViewYaw, 0f) * Vector3.right;
    public Vector3 WorldMove => Vector3.ClampMagnitude(ViewForward * Move.y + ViewRight * Move.x, 1f);
}