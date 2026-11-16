using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// River Studio's water shader was written for the built-in render pipeline. It lights the water
// with _LightColor0 and _WorldSpaceLightPos0, which URP never sets, so in URP the water gets no
// light. Unity also resets both values to "no light" every time it draws the sky, and the sky is
// drawn before see-through water, so this hands the light to the water's own renderer instead.
// It runs right before every frame is drawn: Game view and Scene view, Play mode and editing.
// Put it on the River object and pick the lamp that should light the water.
[ExecuteAlways]
public class RiverLight : MonoBehaviour
{
    [Tooltip("The light that lights the water, such as a sewer wall lamp (or the sun).")]
    public Light lamp;
    [Tooltip("The water (WATER AUTO). For a point or spot lamp, its light comes from the lamp toward this.")]
    public Transform water;
    [Tooltip("How bright the water's lighting is. The lamp's own Intensity is ignored: the shader has no falloff.")]
    [Min(0f)] public float intensity = 1f;

    static readonly int LightColorId = Shader.PropertyToID("_LightColor0");
    static readonly int LightPositionId = Shader.PropertyToID("_WorldSpaceLightPos0");
    MaterialPropertyBlock block;

    void OnEnable() => RenderPipelineManager.beginContextRendering += BeforeDrawing;

    void OnDisable()
    {
        RenderPipelineManager.beginContextRendering -= BeforeDrawing;
        if (water != null && water.TryGetComponent(out Renderer surface))
            surface.SetPropertyBlock(null); // the water goes back to unlit
    }

    void BeforeDrawing(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (lamp == null || water == null || !water.TryGetComponent(out Renderer surface))
            return;
        Vector3 toLight = lamp.type == LightType.Directional
            ? -lamp.transform.forward
            : (lamp.transform.position - water.position).normalized;
        Color color = lamp.isActiveAndEnabled ? lamp.color * intensity : Color.black;
        block ??= new MaterialPropertyBlock();
        surface.GetPropertyBlock(block);
        block.SetVector(LightColorId, new Vector4(color.r, color.g, color.b, 1f));
        block.SetVector(LightPositionId, new Vector4(toLight.x, toLight.y, toLight.z, 0f));
        surface.SetPropertyBlock(block);
    }
}
