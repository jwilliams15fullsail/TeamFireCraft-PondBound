using System.Collections.Generic;
using UnityEngine;

// Frogs (and anything in Also Track) press trails into a goo floor, and the goo slowly closes up
// over them again. Put it on the goo floor, next to its Mesh Renderer that uses PondBound/GooSurface.
// Only a visual: every screen draws its own trails from where the frogs are, so nothing is sent
// over the network.
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class GooTrails : MonoBehaviour
{
    [Tooltip("Trail detail: how many texels across the longer side of the goo.")]
    [Range(64, 512)] public int resolution = 256;
    [Tooltip("How wide (m) a frog's trail through the goo is.")]
    [Min(0.05f)] public float trailWidth = 0.5f;
    [Tooltip("How close (m) to the goo the bottom of a frog must be to push through it.")]
    [Min(0f)] public float touchHeight = 0.12f;
    [Tooltip("Seconds for a trail to close up most of the way.")]
    [Min(0.1f)] public float healTime = 4f;
    [Tooltip("Other things that leave trails, such as boxes. Frogs are found by themselves.")]
    public List<Transform> alsoTrack = new List<Transform>();
    [Tooltip("For Also Track: how far (m) below each object's centre its bottom is.")]
    [Min(0f)] public float alsoTrackDepth = 0.25f;

    static readonly int TrailTexId = Shader.PropertyToID("_TrailTex");
    static readonly int TrailRectId = Shader.PropertyToID("_TrailRect");

    MeshRenderer surface;
    Texture2D trailTexture;
    float[] trail;
    byte[] pixels;
    int width, height;
    Rect area; // the mesh's footprint in its own X/Z
    FrogMotor[] frogs = new FrogMotor[0];
    float nextFrogSearch;
    bool changed;

    void Start()
    {
        surface = GetComponent<MeshRenderer>();
        Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
        area = new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);

        // Square texels: the longer side gets Resolution texels
        Vector3 scale = transform.lossyScale;
        float metresX = area.width * Mathf.Abs(scale.x);
        float metresZ = area.height * Mathf.Abs(scale.z);
        float longest = Mathf.Max(metresX, metresZ);
        width = Mathf.Max(8, Mathf.RoundToInt(resolution * metresX / longest));
        height = Mathf.Max(8, Mathf.RoundToInt(resolution * metresZ / longest));

        trail = new float[width * height];
        pixels = new byte[width * height];
        trailTexture = new Texture2D(width, height, TextureFormat.R8, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "GooTrails"
        };
        trailTexture.SetPixelData(pixels, 0);
        trailTexture.Apply(false);

        var block = new MaterialPropertyBlock();
        surface.GetPropertyBlock(block);
        block.SetTexture(TrailTexId, trailTexture);
        block.SetVector(TrailRectId, new Vector4(area.x, area.y, area.width, area.height));
        surface.SetPropertyBlock(block);
    }

    void OnDestroy()
    {
        if (trailTexture != null)
            Destroy(trailTexture);
    }

    void Update()
    {
        if (trail == null)
            return;
        if (Time.time >= nextFrogSearch)
        {
            frogs = FindObjectsByType<FrogMotor>(FindObjectsInactive.Exclude); // frogs join and leave
            nextFrogSearch = Time.time + 1f;
        }

        // The goo closes up: 95% healed after Heal Time
        float keep = Mathf.Exp(-Time.deltaTime * 3f / healTime);
        for (int i = 0; i < trail.Length; i++)
        {
            if (trail[i] <= 0f)
                continue;
            trail[i] = trail[i] * keep < 0.004f ? 0f : trail[i] * keep;
            changed = true;
        }

        foreach (FrogMotor frog in frogs)
            if (frog != null)
                Push(frog.transform.position, frog.Radius);
        foreach (Transform thing in alsoTrack)
            if (thing != null)
                Push(thing.position, alsoTrackDepth);

        if (!changed)
            return;
        for (int i = 0; i < trail.Length; i++)
            pixels[i] = (byte)(trail[i] * 255f + 0.5f);
        trailTexture.SetPixelData(pixels, 0);
        trailTexture.Apply(false);
        changed = false;
    }
    // A single splash, such as a drop from a pipe landing here: a ring ringWidth metres across.
    public void Splash(Vector3 point, float ringWidth)
    {
        if (trail != null)
            Push(point, 0f, ringWidth);
    }

    // Presses a round dent into the goo under something whose bottom is depthBelow under centre.
    void Push(Vector3 centre, float depthBelow, float ringWidth = -1f)
    {
        Vector3 local = transform.InverseTransformPoint(centre);
        float surfaceY = transform.TransformPoint(new Vector3(local.x, 0f, local.z)).y;
        float gap = centre.y - depthBelow - surfaceY; // how far its bottom is above the goo
        if (gap > touchHeight || gap < -1f)
            return;
        float u = (local.x - area.x) / area.width;
        float v = (local.z - area.y) / area.height;
        if (u < 0f || u > 1f || v < 0f || v > 1f)
            return;

        float strength = gap <= 0f ? 1f : 1f - gap / Mathf.Max(touchHeight, 0.001f);
        float metresPerTexel = area.width * Mathf.Abs(transform.lossyScale.x) / width;
        float radius = Mathf.Max(1f, (ringWidth > 0f ? ringWidth : trailWidth) * 0.5f / metresPerTexel);
        int cx = Mathf.FloorToInt(u * width);
        int cy = Mathf.FloorToInt(v * height);
        int reach = Mathf.CeilToInt(radius);
        for (int y = Mathf.Max(0, cy - reach); y <= Mathf.Min(height - 1, cy + reach); y++)
        {
            for (int x = Mathf.Max(0, cx - reach); x <= Mathf.Min(width - 1, cx + reach); x++)
            {
                float d2 = ((x - cx) * (x - cx) + (y - cy) * (y - cy)) / (radius * radius);
                if (d2 >= 1f)
                    continue;
                float dent = strength * (1f - d2) * (1f - d2);
                int i = y * width + x;
                if (dent > trail[i])
                {
                    trail[i] = dent;
                    changed = true;
                }
            }
        }
    }
}