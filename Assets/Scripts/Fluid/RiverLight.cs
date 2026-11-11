using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// River Studio's water shader was written for the built-in render pipeline. It lights the water
// with _LightColor0 and _WorldSpaceLightPos0, which URP never sets, so in URP the water gets no
// light and looks almost black under its foam. This hands it one light right before every frame
// is drawn: in the Game view and the Scene view, in Play mode and while editing.
// Put it on the River object and pick the lamp that should light the water.
[ExecuteAlways]
public class RiverLight : MonoBehaviour
{
    [Tooltip("The light that lights the water, such as a sewer wall lamp (or the sun).")]
    public Light lamp;
    [Tooltip("For a point or spot lamp: the water (WATER AUTO), so its light comes from the lamp toward the water.")]
    public Transform water;
    [Tooltip("How bright the water's lighting is. The lamp's own Intensity is ignored: the shader has no falloff.")]
    [Min(0f)] public float intensity = 1f;

    static readonly int LightColorId = Shader.PropertyToID("_LightColor0");
    static readonly int LightPositionId = Shader.PropertyToID("_WorldSpaceLightPos0");

    void OnEnable() => RenderPipelineManager.beginContextRendering += BeforeDrawing;
    void OnDisable() => RenderPipelineManager.beginContextRendering -= BeforeDrawing;

    void BeforeDrawing(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (lamp == null)
            return;
        Vector3 toLight = lamp.type == LightType.Directional || water == null
            ? -lamp.transform.forward
            : (lamp.transform.position - water.position).normalized;
        Color color = lamp.isActiveAndEnabled ? lamp.color * intensity : Color.black;
        Shader.SetGlobalVector(LightColorId, new Vector4(color.r, color.g, color.b, 1f));
        Shader.SetGlobalVector(LightPositionId, new Vector4(toLight.x, toLight.y, toLight.z, 0f));
    }
}