using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

// Sticky, slime-like fluid simulated on the GPU with SPH (StickyFluid.compute)
// and drawn with the PondBound/StickyFluidParticle shader.
// The goo lives inside this object's box, in its local space.
// Visual only: every player runs their own copy, so don't drive gameplay from it.
[DisallowMultipleComponent]
public class StickyFluid : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct Particle
    {
        public Vector3 position; public float density;
        public Vector3 velocity; public float pressure;
        public Vector3 normal; public float pad0;
        public Vector3 force; public float pad1;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FluidCollider
    {
        public Vector3 a; public float radius;
        public Vector3 b; public float type;
        public Vector3 velocity; public float pad;
        public Quaternion rotation;
    }

    const int ParticleStride = 64;
    const int ColliderStride = 64;
    const int Threads = 256;     // THREADS in StickyFluid.compute
    const int MaxPerCell = 32;   // MAX_PER_CELL in StickyFluid.compute
    const int MaxColliders = 32;

    [Header("Setup")]
    [SerializeField] ComputeShader simulation;
    [SerializeField] Material material;

    [Header("Container")]
    [Tooltip("Size of the box the goo lives in, centred on this object. Read when the goo is built (Play or Reset Fluid).")]
    [SerializeField] Vector3 boxSize = new Vector3(1.5f, 1f, 1.5f);

    [Header("Particles")]
    [Tooltip("Gap between particles at rest, in metres. Smaller = finer goo, but many more particles. Read when the goo is built.")]
    [SerializeField, Range(0.02f, 0.1f)] float particleSpacing = 0.04f;
    [SerializeField, Min(1)] int maxParticles = 16384;
    [Tooltip("Fill the spawn block with goo when the goo is built.")]
    [SerializeField] bool spawnBlock = true;
    [SerializeField] Vector3 spawnCenter = new Vector3(0f, -0.25f, 0f);
    [SerializeField] Vector3 spawnSize = new Vector3(0.6f, 0.3f, 0.6f);

    [Header("Pouring (optional)")]
    [Tooltip("Pours goo out along this transform's blue (forward) axis. Keep it inside the box. Leave empty for no pouring.")]
    [SerializeField] Transform emitter;
    [Tooltip("Other scripts can switch pouring on and off with this.")]
    public bool emitting = true;
    [SerializeField, Min(0.1f)] float emitSpeed = 1f;
    [SerializeField, Min(0.01f)] float emitRadius = 0.06f;

    [Header("Goo")]
    [SerializeField] float restDensity = 1000f;
    [Tooltip("How hard the goo resists being squashed. Higher costs more sub-steps.")]
    [SerializeField, Range(20f, 400f)] float stiffness = 80f;
    [Tooltip("How syrupy it is: share of the speed difference between neighbours removed every 1/60 s.")]
    [SerializeField, Range(0f, 1f)] float thickness = 0.9f;
    [Tooltip("How strongly the goo pulls itself into blobs and strings.")]
    [SerializeField, Range(0f, 10f)] float surfaceTension = 3f;
    [SerializeField, Min(0.5f)] float maxSpeed = 5f;

    [Header("Stickiness")]
    [Tooltip("Pull towards the floor and walls, in multiples of gravity. Above 1 it can hang off a wall.")]
    [SerializeField, Range(0f, 10f)] float containerAdhesion = 5f;
    [Tooltip("How much sliding and peeling the floor and walls stop, every 1/60 s.")]
    [SerializeField, Range(0f, 1f)] float containerFriction = 0.95f;
    [Tooltip("Pull towards colliders such as frogs, in multiples of gravity.")]
    [SerializeField, Range(0f, 10f)] float colliderAdhesion = 5f;
    [SerializeField, Range(0f, 1f)] float colliderFriction = 0.95f;

    [Header("Colliders")]
    [Tooltip("Sphere, Capsule and Box colliders on these layers push and carry the goo. Mesh colliders are skipped.")]
    [SerializeField] LayerMask colliderLayers = ~0;

    [Header("Simulation")]
    [Tooltip("Most sub-steps per frame. If the goo needs more, it slows down instead of exploding.")]
    [SerializeField, Range(1, 32)] int maxStepsPerFrame = 12;

    [Header("Rendering")]
    [Tooltip("Size of each drawn blob, as a multiple of Particle Spacing.")]
    [SerializeField, Range(0.4f, 2f)] float renderRadius = 1.1f;
    [SerializeField] bool castShadows = true;

    public int ParticleCount => particleCount;

    GraphicsBuffer particleBuffer, cellCountBuffer, cellParticleBuffer, colliderBuffer;
    Particle[] staging;
    readonly FluidCollider[] colliders = new FluidCollider[MaxColliders];
    readonly Collider[] overlaps = new Collider[MaxColliders];
    Dictionary<Collider, Vector3> lastCenters = new Dictionary<Collider, Vector3>();
    Dictionary<Collider, Vector3> centers = new Dictionary<Collider, Vector3>();
    readonly List<Vector2> nozzle = new List<Vector2>();
    MaterialPropertyBlock props;
    Vector3Int gridSize;
    int cellCount, particleCount, colliderCount;
    Vector3 box;          // Box Size and Particle Spacing as they were when the goo was built
    float spacing, h, mass, emitTimer;
    int kClear, kBuild, kDensity, kNormals, kForces, kIntegrate;
    int[] kernels;
    bool ready;

    void OnEnable()
    {
        // Dedicated servers and batch mode have no GPU to run or draw the goo.
        if (!SystemInfo.supportsComputeShaders || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            enabled = false;
            return;
        }
        if (simulation == null || material == null)
        {
            Debug.LogWarning($"{name}: StickyFluid needs its Simulation compute shader and Material set.", this);
            enabled = false;
            return;
        }
        Build();
    }

    void OnDisable() => Release();

    [ContextMenu("Reset Fluid")]
    public void ResetFluid()
    {
        if (!isActiveAndEnabled) return;
        Release();
        Build();
    }

    void Build()
    {
        box = boxSize;
        spacing = particleSpacing;
        h = 2f * spacing;
        gridSize = new Vector3Int(
            Mathf.Max(1, Mathf.CeilToInt(box.x / h)),
            Mathf.Max(1, Mathf.CeilToInt(box.y / h)),
            Mathf.Max(1, Mathf.CeilToInt(box.z / h)));
        cellCount = gridSize.x * gridSize.y * gridSize.z;

        particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxParticles, ParticleStride);
        cellCountBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, cellCount, sizeof(uint));
        cellParticleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, cellCount * MaxPerCell, sizeof(uint));
        colliderBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MaxColliders, ColliderStride);
        staging = new Particle[maxParticles];
        props ??= new MaterialPropertyBlock();

        kClear = simulation.FindKernel("ClearGrid");
        kBuild = simulation.FindKernel("BuildGrid");
        kDensity = simulation.FindKernel("ComputeDensityPressure");
        kNormals = simulation.FindKernel("ComputeNormals");
        kForces = simulation.FindKernel("ComputeForces");
        kIntegrate = simulation.FindKernel("Integrate");
        kernels = new[] { kClear, kBuild, kDensity, kNormals, kForces, kIntegrate };

        // Pick the particle mass so a block spawned at Particle Spacing sits exactly at rest density.
        float sum = 0f;
        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
                for (int z = -2; z <= 2; z++)
                {
                    float r2 = (x * x + y * y + z * z) * spacing * spacing;
                    if (r2 < h * h)
                    {
                        float w = h * h - r2;
                        sum += w * w * w;
                    }
                }
        mass = restDensity / (Poly6(h) * sum);

        BuildNozzle();
        particleCount = 0;
        colliderCount = 0;
        emitTimer = 0f;
        lastCenters.Clear();
        centers.Clear();
        if (spawnBlock) SpawnBlock();
        ready = true;
    }

    void Release()
    {
        ready = false;
        particleBuffer?.Release();
        cellCountBuffer?.Release();
        cellParticleBuffer?.Release();
        colliderBuffer?.Release();
        particleBuffer = cellCountBuffer = cellParticleBuffer = colliderBuffer = null;
        particleCount = 0;
    }

    void Update()
    {
        if (!ready) return;

        float frameTime = Time.deltaTime;
        if (frameTime > 0f)
        {
            Emit(frameTime);
            GatherColliders(frameTime);

            if (particleCount > 0)
            {
                float maxStep = Mathf.Min(1f / 120f, 0.4f * h / Mathf.Sqrt(stiffness));
                int steps = Mathf.Clamp(Mathf.CeilToInt(frameTime / maxStep), 1, maxStepsPerFrame);
                float dt = Mathf.Min(frameTime / steps, maxStep);
                SetConstants(dt);
                for (int s = 0; s < steps; s++) Step();
            }
        }

        Draw();
    }

    void Step()
    {
        int particleGroups = Mathf.CeilToInt(particleCount / (float)Threads);
        simulation.Dispatch(kClear, Mathf.CeilToInt(cellCount / (float)Threads), 1, 1);
        simulation.Dispatch(kBuild, particleGroups, 1, 1);
        simulation.Dispatch(kDensity, particleGroups, 1, 1);
        simulation.Dispatch(kNormals, particleGroups, 1, 1);
        simulation.Dispatch(kForces, particleGroups, 1, 1);
        simulation.Dispatch(kIntegrate, particleGroups, 1, 1);
    }

    // Set every frame, so several goo objects can share one compute shader.
    void SetConstants(float dt)
    {
        foreach (int k in kernels)
        {
            simulation.SetBuffer(k, "_Particles", particleBuffer);
            simulation.SetBuffer(k, "_CellCounts", cellCountBuffer);
            simulation.SetBuffer(k, "_CellParticles", cellParticleBuffer);
            simulation.SetBuffer(k, "_Colliders", colliderBuffer);
        }

        float g = Physics.gravity.magnitude;
        simulation.SetInt("_ParticleCount", particleCount);
        simulation.SetInt("_CellCount", cellCount);
        simulation.SetInt("_ColliderCount", colliderCount);
        simulation.SetVector("_GridSize", new Vector4(gridSize.x, gridSize.y, gridSize.z, 0f));
        simulation.SetVector("_HalfBox", box * 0.5f);
        simulation.SetVector("_Gravity", Quaternion.Inverse(transform.rotation) * Physics.gravity);
        simulation.SetFloat("_H", h);
        simulation.SetFloat("_H2", h * h);
        simulation.SetFloat("_Mass", mass);
        simulation.SetFloat("_RestDensity", restDensity);
        simulation.SetFloat("_Stiffness", stiffness);
        simulation.SetFloat("_SurfaceTension", surfaceTension);
        simulation.SetFloat("_Poly6", Poly6(h));
        simulation.SetFloat("_SpikyGrad", 45f / (Mathf.PI * Mathf.Pow(h, 6)));
        simulation.SetFloat("_CohesionCoef", 32f / (Mathf.PI * Mathf.Pow(h, 9)));
        simulation.SetFloat("_DeltaTime", dt);
        simulation.SetFloat("_Xsph", 1f - KeptPerStep(thickness, dt));
        simulation.SetFloat("_MaxSpeed", maxSpeed);
        simulation.SetFloat("_CollisionRadius", 0.5f * spacing);
        simulation.SetFloat("_AdhesionRange", h);
        simulation.SetFloat("_ContainerAdhesion", containerAdhesion * g);
        simulation.SetFloat("_ContainerFriction", KeptPerStep(containerFriction, dt));
        simulation.SetFloat("_ColliderAdhesion", colliderAdhesion * g);
        simulation.SetFloat("_ColliderFriction", KeptPerStep(colliderFriction, dt));
    }

    static float Poly6(float radius) => 315f / (64f * Mathf.PI * Mathf.Pow(radius, 9));

    // Share of speed kept after one step of dt, when "removed" is the share removed every 1/60 s.
    static float KeptPerStep(float removed, float dt) => Mathf.Pow(1f - 0.999f * removed, 60f * dt);

    void SpawnBlock()
    {
        int nx = Mathf.Max(1, Mathf.FloorToInt(spawnSize.x / spacing));
        int ny = Mathf.Max(1, Mathf.FloorToInt(spawnSize.y / spacing));
        int nz = Mathf.Max(1, Mathf.FloorToInt(spawnSize.z / spacing));
        Vector3 start = spawnCenter - new Vector3(nx - 1, ny - 1, nz - 1) * (0.5f * spacing);
        Vector3 limit = box * 0.5f - Vector3.one * (0.5f * spacing);
        int added = 0;

        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    if (particleCount + added >= maxParticles) break;
                    Vector3 p = start + new Vector3(x, y, z) * spacing + Random.insideUnitSphere * (0.05f * spacing);
                    if (Mathf.Abs(p.x) > limit.x || Mathf.Abs(p.y) > limit.y || Mathf.Abs(p.z) > limit.z) continue;
                    staging[added++] = new Particle { position = p, density = restDensity };
                }
        Upload(added);
    }

    void BuildNozzle()
    {
        nozzle.Clear();
        int rings = Mathf.CeilToInt(emitRadius / spacing);
        for (int y = -rings; y <= rings; y++)
            for (int x = -rings; x <= rings; x++)
            {
                Vector2 p = new Vector2(x, y) * spacing;
                if (p.sqrMagnitude <= emitRadius * emitRadius) nozzle.Add(p);
            }
    }

    // Pours one disc of particles every time the stream has moved one Particle Spacing.
    void Emit(float frameTime)
    {
        if (emitter == null || !emitting || particleCount >= maxParticles) return;

        emitTimer = Mathf.Min(emitTimer + frameTime, 0.25f);
        float interval = spacing / emitSpeed;
        Quaternion toLocal = Quaternion.Inverse(transform.rotation);
        Vector3 velocity = toLocal * emitter.forward * emitSpeed;
        int added = 0;

        while (emitTimer >= interval && particleCount + added + nozzle.Count <= maxParticles)
        {
            emitTimer -= interval;
            Vector3 travelled = velocity * emitTimer;
            foreach (Vector2 offset in nozzle)
            {
                Vector3 world = emitter.position + emitter.rotation * new Vector3(offset.x, offset.y, 0f);
                Vector3 local = toLocal * (world - transform.position) + travelled;
                staging[added++] = new Particle { position = local, velocity = velocity, density = restDensity };
            }
        }
        Upload(added);
    }

    void Upload(int added)
    {
        if (added == 0) return;
        particleBuffer.SetData(staging, 0, particleCount, added);
        particleCount += added;
    }

    // Finds the colliders touching the box and sends them to the GPU in local space.
    void GatherColliders(float frameTime)
    {
        Quaternion toLocal = Quaternion.Inverse(transform.rotation);
        Vector3 origin = transform.position;
        Vector3 halfExtents = box * 0.5f + Vector3.one * (2f * spacing);
        int hits = Physics.OverlapBoxNonAlloc(origin, halfExtents, overlaps, transform.rotation, colliderLayers, QueryTriggerInteraction.Ignore);

        (lastCenters, centers) = (centers, lastCenters);
        centers.Clear();
        colliderCount = 0;

        for (int i = 0; i < hits && colliderCount < MaxColliders; i++)
        {
            Collider col = overlaps[i];
            if (col.transform.IsChildOf(transform)) continue;
            if (!Describe(col, out FluidCollider c, out Vector3 center)) continue;

            Vector3 velocity = lastCenters.TryGetValue(col, out Vector3 last) ? (center - last) / frameTime : Vector3.zero;
            centers[col] = center;

            c.a = toLocal * (c.a - origin);
            if (c.type > 0.5f && c.type < 1.5f) c.b = toLocal * (c.b - origin);
            c.rotation = toLocal * c.rotation;
            c.velocity = toLocal * velocity;
            colliders[colliderCount++] = c;
        }

        if (colliderCount > 0) colliderBuffer.SetData(colliders, 0, 0, colliderCount);
    }

    // World-space shape of a collider. Type 0 sphere, 1 capsule, 2 box.
    static bool Describe(Collider col, out FluidCollider c, out Vector3 center)
    {
        c = default;
        c.rotation = Quaternion.identity;
        Transform t = col.transform;
        Vector3 s = t.lossyScale;
        s = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

        switch (col)
        {
            case SphereCollider sphere:
                center = t.TransformPoint(sphere.center);
                c.type = 0f;
                c.a = center;
                c.radius = sphere.radius * Mathf.Max(s.x, Mathf.Max(s.y, s.z));
                return true;

            case CapsuleCollider capsule:
                {
                    center = t.TransformPoint(capsule.center);
                    int axis = capsule.direction;
                    float radiusScale = axis == 0 ? Mathf.Max(s.y, s.z) : axis == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y);
                    float radius = capsule.radius * radiusScale;
                    float halfLine = Mathf.Max(capsule.height * 0.5f * s[axis] - radius, 0f);
                    Vector3 dir = t.rotation * (axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward);
                    c.type = 1f;
                    c.a = center - dir * halfLine;
                    c.b = center + dir * halfLine;
                    c.radius = radius;
                    return true;
                }

            case BoxCollider cube:
                center = t.TransformPoint(cube.center);
                c.type = 2f;
                c.a = center;
                c.b = Vector3.Scale(cube.size, s) * 0.5f;
                c.rotation = t.rotation;
                return true;

            default:
                center = default;
                return false;
        }
    }

    void Draw()
    {
        if (particleCount == 0) return;

        props.SetBuffer("_Particles", particleBuffer);
        props.SetMatrix("_SimToWorld", Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one));
        props.SetFloat("_ParticleRadius", renderRadius * spacing);
        props.SetFloat("_RestDensity", restDensity);

        var rp = new RenderParams(material)
        {
            worldBounds = new Bounds(transform.position, Vector3.one * (box.magnitude + 2f * spacing)),
            matProps = props,
            shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
            receiveShadows = true,
            layer = gameObject.layer
        };
        Graphics.RenderPrimitives(rp, MeshTopology.Triangles, 6, particleCount);
    }

    void OnValidate()
    {
        boxSize = Vector3.Max(boxSize, Vector3.one * (4f * particleSpacing));
        spawnSize = Vector3.Max(spawnSize, Vector3.zero);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        if (spawnBlock)
        {
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.25f);
            Gizmos.DrawCube(spawnCenter, spawnSize);
        }
        if (emitter != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(emitter.position, emitRadius);
            Gizmos.DrawRay(emitter.position, emitter.forward * 0.3f);
        }
    }
}