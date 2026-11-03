using UnityEngine;

// Defines how slippery a slime surface is.
// FrogMotor will read these values when a frog stands on slime.
public class PetriDishSlime : MonoBehaviour
{
    [Header("Slime Movement")]

    [Tooltip("Multiplier for ground acceleration. Lower means slower acceleration.")]
    [Range(0.05f, 1f)]
    public float accelerationMultiplier = 0.35f;

    [Tooltip("Multiplier for ground braking. Lower means more sliding.")]
    [Range(0.01f, 1f)]
    public float brakingMultiplier = 0.08f;
}
