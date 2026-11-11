using UnityEngine;

// Keeps this object at a RisingWater flood's water height, so River Studio's water rises as the
// room floods. Put it on WATER AUTO, and on RIPPLE CAUSTICS GENERATOR so the caustics follow too.
public class FollowFloodLevel : MonoBehaviour
{
    [Tooltip("The flood to follow (SewerFlood).")]
    public RisingWater water;
    [Tooltip("Optional: a renderer shown only while there's water (WATER AUTO's own Mesh Renderer).")]
    public Renderer showWhenWet;
    [Tooltip("Optional: the flood's own surface, hidden so only River Studio's water shows (SewerFlood > Surface).")]
    public Renderer hideSurface;

    void LateUpdate()
    {
        if (water == null)
            return;
        Vector3 at = transform.position;
        at.y = water.transform.TransformPoint(new Vector3(0f, water.Level, 0f)).y;
        transform.position = at;
        if (showWhenWet != null)
            showWhenWet.enabled = water.Level > 0.01f; // nothing to show while the room is dry
        if (hideSurface != null)
            hideSurface.enabled = false; // RisingWater switches it back on every frame; this runs after it
    }
}