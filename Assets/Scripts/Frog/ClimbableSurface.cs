using UnityEngine;

// Marks a collider (or any of its children) as something frogs can stick to.
// Nothing is climbable unless it has this component.
public class ClimbableSurface : MonoBehaviour
{
    [Tooltip("1 = normal grip, below 1 = slippery, above 1 = extra sticky.")]
    [Min(0f)] public float gripMultiplier = 1f;
}
