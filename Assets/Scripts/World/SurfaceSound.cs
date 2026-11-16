using UnityEngine;

// What a surface sounds like when a frog hops on it or lands on it: goo, wood, metal, water...
// Put it on the floor object itself (anything with a collider), or on a trigger box laid over an
// area, like a goo puddle or a wooden walkway, to cover everything inside it. FrogAudio finds
// the one under the frog; without one it plays its own default sounds.
public class SurfaceSound : MonoBehaviour
{
    [Tooltip("Played as the frog pushes off (a hop or a jump). One is picked at random.")]
    public AudioClip[] steps;
    [Tooltip("Played as the frog lands. Empty: uses Steps.")]
    public AudioClip[] landings;
    [Range(0f, 1f)] public float volume = 0.7f;
    [Tooltip("Each sound gets a random pitch in this range, so repeats don't sound identical.")]
    public Vector2 pitchRange = new Vector2(0.92f, 1.08f);
    [Tooltip("When a trigger box and a floor overlap, the higher priority wins (a goo puddle over a floor).")]
    public int priority;
}