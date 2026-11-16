using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;


// A flat water surface that rises to flood an area: a room, a pool, a sewer. Far cheaper than
// particle water, so it works for whole rooms. The host decides the level and syncs it, so every
// player sees the same water, including anyone who joins late.
// Put it on an object on the floor in the middle of the area. Give it a child Plane with the
// WaterSurface material as its Surface: this script sizes that plane and moves it up and down.
// Optionally the water first runs down a Stream in a pipe, then pours in at the Inflow Point.
[RequireComponent(typeof(NetworkObject))]
public class RisingWater : NetworkBehaviour
{
    [Tooltip("A child Plane with the WaterSurface material. It is resized to Size and moved up and down.")]
    [SerializeField] Transform surface;
    [Tooltip("Floor area the water covers, in metres (X by Z), centred on this object.")]
    [SerializeField] Vector2 size = new Vector2(16f, 20f);
    [Tooltip("How deep the water gets when full, in metres above this object.")]
    [SerializeField, Min(0.1f)] float depth = 1.2f;
    [Tooltip("Seconds to fill from empty to full.")]
    [SerializeField, Min(0.1f)] float fillTime = 30f;

    [Header("When it rises")]
    [Tooltip("Optional door used as a valve: the water comes in while it is open. Open it with wall buttons or pressure plates. Leave empty to start on its own.")]
    [SerializeField] DoorController valve;
    [Tooltip("With a valve: drain again while the valve is closed.")]
    [SerializeField] bool drainWhenClosed;
    [Tooltip("Without a valve: seconds after the level starts before the water starts to come in.")]
    [SerializeField, Min(0f)] float startDelay = 3f;

    [Header("Where it comes from (optional)")]
    [Tooltip("A Plane inside the pipe the water comes from, using a WaterSurface material with Flowing ticked and its blue (Z) axis pointing at the room. The water runs down it first, then rises with the room's water.")]
    [SerializeField] Transform stream;
    [Tooltip("Seconds the water takes to run down the Stream. The room starts filling when it arrives.")]
    [SerializeField, Min(0f)] float arriveTime = 3f;
    [Tooltip("Where the water pours into the room, such as the pipe's mouth. The water spreads out across the floor from here, and ripples rush outward from here while it flows in.")]
    [SerializeField] Transform inflowPoint;
    [Tooltip("Particle water (StickyFluid) that splashes out at the mouth while the room fills.")]
    [SerializeField] StickyFluid pour;
    [Tooltip("Stop the Pour once the water is this deep, in metres (when the pipe's mouth is under water).")]
    [SerializeField, Min(0f)] float stopPourAt = 0.5f;

    [Header("Optional")]
    [Tooltip("A Box Collider (Is Trigger) on this object. It is sized to the water, so a StatRefillZone beside it refills frogs in the water.")]
    [SerializeField] BoxCollider waterVolume;

    readonly SyncVar<float> level = new(new SyncTypeSettings(0.1f));  // metres above this object, set by the host
    readonly SyncVar<float> reach = new(new SyncTypeSettings(0.1f));  // 0-1: how far the water has run down the Stream
    readonly SyncVar<bool> flowing = new(new SyncTypeSettings(0.1f)); // water is coming in right now

    Rigidbody surfaceBody;
    Renderer surfaceRenderer, streamRenderer;
    MaterialPropertyBlock surfaceProps, streamProps;
    float shown, reachShown; // what this machine draws, eased toward the host's values
    float elapsed;           // host only: seconds since the level started
    float streamBaseY, streamFullSpeed, streamFullFoam, streamSpeed, streamTravel, inflow;
    float farthest; // from the Inflow Point to the far corner of the water

    const float CoverDepth = 0.15f; // the water has spread across the whole floor by this depth

    public float Level => shown;
    public float FillFraction => shown / depth;
    public bool IsFull => level.Value >= depth - 0.001f;


    static readonly List<RisingWater> all = new List<RisingWater>();

    // The highest water surface (world y) at point's spot, among all floods in the scene. False
    // when there's no water there: outside every flood, below its floor, or still dry.
    public static bool SurfaceAt(Vector3 point, out float surfaceY)
    {
        surfaceY = float.MinValue;
        foreach (RisingWater water in all)
        {
            if (water == null || water.shown < 0.01f)
                continue;
            Vector3 local = water.transform.InverseTransformPoint(point);
            if (Mathf.Abs(local.x) > water.size.x * 0.5f || Mathf.Abs(local.z) > water.size.y * 0.5f || local.y < -0.5f)
                continue;
            surfaceY = Mathf.Max(surfaceY, water.transform.TransformPoint(new Vector3(0f, water.shown, 0f)).y);
        }
        return surfaceY > float.MinValue;
    }

    void Awake()
    {
        if (stream != null)
        {
            streamBaseY = stream.position.y;
            streamRenderer = stream.GetComponent<Renderer>();
            streamProps = new MaterialPropertyBlock();
            if (streamRenderer != null)
            {
                Material m = streamRenderer.sharedMaterial;
                streamFullSpeed = m.HasFloat("_FlowSpeed") ? m.GetFloat("_FlowSpeed") : 0f;
                streamFullFoam = m.HasFloat("_FlowFoam") ? m.GetFloat("_FlowFoam") : 0f;
                streamRenderer.enabled = false; // dry until the water starts
            }
        }
        if (inflowPoint != null)
        {
            Vector3 from = inflowPoint.position;
            for (int i = 0; i < 4; i++)
            {
                Vector3 corner = transform.TransformPoint(new Vector3((i % 2 == 0 ? -0.5f : 0.5f) * size.x, 0f, (i < 2 ? -0.5f : 0.5f) * size.y));
                farthest = Mathf.Max(farthest, new Vector2(corner.x - from.x, corner.z - from.z).magnitude);
            }
        }
        if (surface == null)
            return;
        surfaceBody = surface.GetComponent<Rigidbody>();
        surfaceRenderer = surface.GetComponent<Renderer>();
        surfaceProps = new MaterialPropertyBlock();
        surface.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f); // a Unity Plane is 10 x 10 m
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        shown = level.Value; // joined late: start where the water is instead of from empty
        reachShown = stream == null ? 1f : reach.Value;
    }


    // Frogs ask every flood in the scene for the water they're in (SurfaceAt).
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        all.Add(this);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        all.Remove(this);
    }

    void Update()
    {
        float rate = depth / fillTime;
        float reachRate = 1f / Mathf.Max(arriveTime, 0.01f);

        if (IsServerInitialized)
        {
            elapsed += Time.deltaTime;
            bool open = valve != null ? valve.IsOpen : elapsed >= startDelay;
            // First the water runs down the Stream, then the room starts to fill.
            float nextReach = stream == null ? 1f : open ? Mathf.MoveTowards(reach.Value, 1f, reachRate * Time.deltaTime) : reach.Value;
            if (nextReach != reach.Value)
                reach.Value = nextReach;
            bool arrived = nextReach >= 1f;
            float target = open ? (arrived ? depth : level.Value) : (valve != null && drainWhenClosed ? 0f : level.Value);
            float next = Mathf.MoveTowards(level.Value, target, rate * Time.deltaTime);
            if (next != level.Value)
                level.Value = next;
            bool nowFlowing = open && next < depth;
            if (nowFlowing != flowing.Value)
                flowing.Value = nowFlowing;
            shown = next;
            reachShown = nextReach;
        }
        else
        {
            shown = Mathf.Abs(level.Value - shown) > 0.25f ? level.Value : Mathf.MoveTowards(shown, level.Value, rate * 1.2f * Time.deltaTime);
            reachShown = stream == null ? 1f : Mathf.MoveTowards(reachShown, reach.Value, reachRate * 1.2f * Time.deltaTime);
        }

        bool pouringIn = flowing.Value && reachShown >= 1f;
        if (pour != null)
            pour.emitting = pouringIn && shown < stopPourAt;

        UpdateSurface(pouringIn);
        UpdateStream();

        if (waterVolume != null)
        {
            float top = shown + 0.1f; // a little above the surface, so frogs riding on top count as in the water
            waterVolume.enabled = shown > 0.05f;
            waterVolume.size = new Vector3(size.x, top, size.y);
            waterVolume.center = new Vector3(0f, top * 0.5f, 0f);
        }
    }

    void UpdateSurface(bool pouringIn)
    {
        if (surface == null)
            return;
        if (surfaceBody == null)
            surface.position = transform.TransformPoint(new Vector3(0f, shown, 0f));
        if (surfaceRenderer == null)
            return;
        surfaceRenderer.enabled = shown > 0.01f;
        if (inflowPoint != null)
        {
            // The water spreads out across the floor from where it pours in. Ripples keep rushing out
            // from there while it pours, then calm down when it stops.
            inflow = Mathf.MoveTowards(inflow, pouringIn ? 1f : 0f, Time.deltaTime * 0.5f);
            Vector3 into = transform.position - inflowPoint.position;
            into.y = 0f;
            surfaceProps.SetVector("_InflowPoint", inflowPoint.position);
            surfaceProps.SetVector("_InflowDir", into.sqrMagnitude > 0.0001f ? into.normalized : Vector3.forward);
            surfaceProps.SetFloat("_InflowStrength", inflow);
            surfaceProps.SetFloat("_Spread", shown < CoverDepth ? farthest * shown / CoverDepth + 0.5f : 100000f);
            surfaceRenderer.SetPropertyBlock(surfaceProps);
        }
    }

    void UpdateStream()
    {
        if (stream == null)
            return;
        // It rides just above the room's water once that is higher than it.
        float waterY = transform.TransformPoint(new Vector3(0f, shown, 0f)).y + 0.01f;
        Vector3 at = stream.position;
        stream.position = new Vector3(at.x, Mathf.Max(streamBaseY, waterY), at.z);
        if (streamRenderer == null)
            return;
        // Ease the flow in and out, keeping its own offset so the ripples never jump.
        streamSpeed = Mathf.MoveTowards(streamSpeed, flowing.Value ? streamFullSpeed : 0f, streamFullSpeed * Time.deltaTime);
        streamTravel += streamSpeed * Time.deltaTime;
        float length = 10f * stream.lossyScale.z; // a Unity Plane is 10 m long
        streamProps.SetFloat("_FlowSpeed", 0f);
        streamProps.SetFloat("_FlowOffset", streamTravel);
        streamProps.SetFloat("_FlowFoam", streamFullSpeed > 0f ? streamFullFoam * streamSpeed / streamFullSpeed : 0f);
        streamProps.SetVector("_ReachStart", stream.TransformPoint(new Vector3(0f, 0f, -5f))); // its upstream end
        streamProps.SetFloat("_Reach", reachShown >= 1f ? 100000f : reachShown * length);
        streamRenderer.SetPropertyBlock(streamProps);
        streamRenderer.enabled = reachShown > 0f || shown > 0.01f;
    }

    void FixedUpdate()
    {
        // A kinematic Rigidbody on the surface carries frogs standing on it, like a lift.
        if (surfaceBody != null)
            surfaceBody.MovePosition(transform.TransformPoint(new Vector3(0f, shown, 0f)));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.9f);
        Gizmos.DrawWireCube(new Vector3(0f, depth * 0.5f, 0f), new Vector3(size.x, depth, size.y));
    }
}