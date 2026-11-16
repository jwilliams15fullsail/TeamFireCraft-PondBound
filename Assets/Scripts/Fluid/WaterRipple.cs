using Artngame.CommonTools.FluidSim2D;
using UnityEngine;

// Makes ripples on River Studio's water where this object touches it: a splash when it lands in
// the water and a trail while it moves through it. Put it on the Frog prefab and on floating boxes.
// With Always on, it ripples nonstop: put one at the sewer tube's mouth for the churn of the stream
// pouring in. Only a visual, so it runs on every screen and sends nothing over the network.
public class WaterRipples : MonoBehaviour
{
    [Tooltip("River Studio's ripple maker (River > RIPPLE CAUSTICS GENERATOR). Found by itself if empty.")]
    public RippleCausticsGeneratorSM ripples;
    [Tooltip("River Studio's water (River > WATER AUTO). Found by itself if empty.")]
    public Renderer water;
    [Tooltip("How big (m across) each ripple starts.")]
    [Min(0.05f)] public float size = 1.2f;
    [Tooltip("How hard each ripple pushes the water. Higher shows more clearly.")]
    [Range(1, 6)] public int strength = 3;
    [Tooltip("Landing in the water makes a ripple this many times bigger.")]
    [Min(1f)] public float splashSize = 2f;
    [Tooltip("How close (m) to the water's surface counts as touching it.")]
    [Min(0f)] public float touchDistance = 0.35f;
    [Tooltip("Moving this far (m) through the water leaves the next ripple of the trail.")]
    [Min(0.01f)] public float trailSpacing = 0.3f;
    [Tooltip("Ripple nonstop, even when still and away from flood water (for where a stream pours in).")]
    public bool always;
    [Tooltip("With Always on: seconds between ripples.")]
    [Min(0.02f)] public float interval = 0.12f;
    [Tooltip("With Always on: ripples land anywhere within this distance (m), so the water churns.")]
    [Min(0f)] public float scatter = 0.8f;

    static readonly int DepthCameraId = Shader.PropertyToID("_DepthCameraPos");
    static readonly int TerrainScaleId = Shader.PropertyToID("_TerrainScale");
    static readonly int TilingId = Shader.PropertyToID("_LocalWavesTiling");

    bool searched;
    bool wasTouching;
    Vector3 lastRipple;
    float timer;

    void Update()
    {
        if (!FindWater() || !water.enabled)
        {
            wasTouching = false; // dry room: River Studio's water is hidden
            return;
        }

        Vector3 at = transform.position;
        if (always)
        {
            timer += Time.deltaTime;
            if (timer < interval)
                return;
            timer = 0f;
            Vector2 offset = Random.insideUnitCircle * scatter;
            Ripple(at + new Vector3(offset.x, 0f, offset.y), size);
            return;
        }

        bool touching = RisingWater.SurfaceAt(at, out float surfaceY) && Mathf.Abs(at.y - surfaceY) <= touchDistance;
        if (touching && !wasTouching)
        {
            Ripple(at, size * splashSize); // splash: just landed in the water
            lastRipple = at;
        }
        else if (touching && Vector3.ProjectOnPlane(at - lastRipple, Vector3.up).magnitude >= trailSpacing)
        {
            Ripple(at, size);
            lastRipple = at;
        }
        wasTouching = touching;
    }

    // The water reads its ripples from the depth camera's square of the room, so a world point
    // turns into a spot in the ripple texture the same way the water's shader does it.
    void Ripple(Vector3 point, float across)
    {
        Material material = water.sharedMaterial;
        Vector4 centre = material.GetVector(DepthCameraId);
        Vector4 tiling = material.GetVector(TilingId);
        float scale = 0.015f * 1.12f * material.GetFloat(TerrainScaleId);
        Vector2 uv = new Vector2((point.x - centre.x) * scale * tiling.x + tiling.z,
                                 (point.z - centre.z) * scale * tiling.y + tiling.w);
        ripples.AddRipple(uv, across * 0.5f * scale * Mathf.Abs(tiling.x), strength);
    }

    // Looks for River Studio once, so a frog in a level without it costs nothing.
    bool FindWater()
    {
        if (searched)
            return ripples != null && water != null;
        searched = true;
        if (ripples == null)
            ripples = FindAnyObjectByType<RippleCausticsGeneratorSM>();
        if (water == null)
        {
            foreach (Renderer candidate in FindObjectsByType<Renderer>(FindObjectsInactive.Include))
            {
                if (candidate.sharedMaterial != null && candidate.sharedMaterial.HasProperty(TilingId) && candidate.sharedMaterial.HasProperty(DepthCameraId))
                {
                    water = candidate;
                    break;
                }
            }
        }
        return ripples != null && water != null;
    }
}