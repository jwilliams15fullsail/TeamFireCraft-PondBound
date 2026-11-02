using UnityEngine;

// Marks this food as an electric power-up.
// The frog receives electricity only after eating it.
[RequireComponent(typeof(TongueTarget))]
public class ElectricFirefly : MonoBehaviour
{
    [Header("Electric Power-Up")]
    [Min(0.1f)]
    public float stunDuration = 2f;
}

