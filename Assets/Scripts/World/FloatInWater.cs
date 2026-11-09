using FishNet;
using UnityEngine;

// Floats a Rigidbody in a RisingWater flood: it rises with the water, bobs, and dips when frogs
// or boxes land on it. Its shape comes from its Box Collider. Runs on the host, which simulates
// the boxes; the object needs a NetworkTransform (LooseCube has one) so everyone sees it float.
// Put a PressurePlate on a trigger on top of it and it becomes a floating switch.
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FloatInWater : MonoBehaviour
{
    [Tooltip("The water it floats in.")]
    public RisingWater water;
    [Tooltip("How much of its height sits under water with nothing on it. Lower floats higher and dips less under weight.")]
    [Range(0.05f, 0.95f)] public float sinkDepth = 0.35f;
    [Tooltip("How quickly bobbing, drifting and spinning settle down in the water.")]
    [Min(0f)] public float waterDrag = 4f;
    [Tooltip("Pulls it back toward where it started, so it doesn't drift away. 0 = drifts freely.")]
    [Min(0f)] public float stayPut = 1f;

    Rigidbody body;
    BoxCollider box;
    Vector3 home;

    static bool IsHost => InstanceFinder.IsServerStarted;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        home = transform.position;
    }

    void FixedUpdate()
    {
        if (!IsHost || water == null)
            return;

        float surface = water.transform.TransformPoint(new Vector3(0f, water.Level, 0f)).y;
        Vector3 c = box.center;
        Vector3 s = box.size * 0.5f;
        float height = box.size.y * transform.lossyScale.y;

        // One spring per bottom corner. Together they hold its weight at Sink Depth, and a
        // lopsided load pushes one side deeper, which pushes back harder and levels it again.
        float stiffness = body.mass * Physics.gravity.magnitude / (4f * sinkDepth * height);
        float wet = 0f;
        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = transform.TransformPoint(new Vector3(c.x + (i % 2 == 0 ? -s.x : s.x), c.y - s.y, c.z + (i < 2 ? -s.z : s.z)));
            float depth = Mathf.Clamp(surface - corner.y, 0f, height);
            if (depth <= 0f)
                continue;
            body.AddForceAtPosition(Vector3.up * (stiffness * depth), corner, ForceMode.Force);
            wet += depth / height * 0.25f;
        }
        if (wet <= 0f)
            return; // out of the water: plain gravity

        // Water drag, stronger the deeper it sits
        float soak = wet / sinkDepth;
        body.AddForce(-body.linearVelocity * (waterDrag * soak), ForceMode.Acceleration);
        body.AddTorque(-body.angularVelocity * (waterDrag * soak), ForceMode.Acceleration);
        if (stayPut > 0f)
        {
            Vector3 offset = transform.position - home;
            offset.y = 0f;
            body.AddForce(-offset * (stayPut * soak), ForceMode.Acceleration);
        }
    }
}