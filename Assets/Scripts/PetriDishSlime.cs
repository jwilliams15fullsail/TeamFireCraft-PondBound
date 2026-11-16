using UnityEngine;

// Defines how slippery a slime surface is.
// FrogMotor will read these values when a frog stands on slime.
public class PetriDishSlime : MonoBehaviour
{
    [Header("Slime Movement")]

    [Tooltip("Multiplier for ground acceleration. Lower means slower acceleration.")]
    [Range(0.05f, 1f)]
    public float accelerationMultiplier = 0.05f;

    [Tooltip("Multiplier for ground braking. Higher values make the frog stop faster.")]
    [Range(1f, 10f)]
    public float brakingMultiplier = 10f;

    [Header("Slime Jumping")]

    [Tooltip("Multiplier for jump height and reach while standing on slime. Lower values make jumping harder.")]
    [Range(0.1f, 1f)]
    public float jumpMultiplier = 0.5f;
}
