using UnityEngine;

// Turns a UI image round and round (the lobby's loading spinner). Uses unscaled time, so it
// still spins while the game is paused.
public class UISpin : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 360f;

    void Update()
    {
        transform.Rotate(0f, 0f, -degreesPerSecond * Time.unscaledDeltaTime);
    }
}