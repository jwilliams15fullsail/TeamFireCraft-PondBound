using Artngame.CommonTools.DepthRenderer;
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
    [Tooltip("Optional: River Studio's depth camera (DEPTH RENDERERS). Keeps its cut-off at the water's surface, so frogs and boxes in the water bend the flow around them.")]
    public DepthRendererCT_URP obstacleCamera;
    [Tooltip("How far (m) something must stick out of the water to block the flow.")]
    [Min(0f)] public float obstacleHeight = 0.05f;

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

        if (obstacleCamera != null)
        {
            // River Studio's flow treats anything in the top 80% of its depth camera's range as solid.
            // Ending that range just above the water means only things sticking out of the water,
            // like floating frogs and boxes, block the flow, and the floor under the water never does.
            float toWater = obstacleCamera.depthCameraHeight - at.y - obstacleHeight;
            float near = obstacleCamera.depthCameraNearPlane;
            obstacleCamera.depthCameraFarPlane = near + Mathf.Max(0.01f, toWater - near) / 0.8f;
        }
    }
}