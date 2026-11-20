using System.Collections.Generic;
using UnityEngine;

// Your own frog only: while you charge a jump, on the ground or on a wall, draws the arc
// the frog will fly and puts a marker where it comes down. It flies the exact launch that
// gets sent to the host (FrogNetworkDriver.LocalJumpVelocity), stepping it the way the
// physics engine does with a ball the frog's size, so the frog lands on the marker.
// Put it on the Frog prefab with a LineRenderer (and optionally a flat marker) as children.
[RequireComponent(typeof(FrogMotor))]
public class JumpPreview : MonoBehaviour
{
    [SerializeField] LineRenderer arc;
    [Tooltip("Optional: a flat marker (a thin disc or quad with no collider) placed where the jump lands.")]
    [SerializeField] Transform landingMarker;
    [SerializeField] LayerMask hitMask = ~0;
    [Tooltip("Seconds of flight the arc follows before giving up (long falls).")]
    [SerializeField] float maxFlightTime = 3f;
    [Tooltip("Seconds the landing marker takes to glide to a new spot (0 = jump straight there).")]
    [SerializeField] float markerSmoothTime = 0.05f;
    [Tooltip("How quickly the marker turns to lie flat on a new surface (a wall, a slope).")]
    [SerializeField] float markerTurnSpeed = 20f;
    [Tooltip("Marker colour on the ground, and on a wall the frog will cling to.")]
    [SerializeField] Color floorColor = new Color(0.03f, 1f, 0f);
    [SerializeField] Color wallColor = new Color(0.1f, 0.6f, 1f);
    [Tooltip("Colour when the path to the marker would hit something on the way.")]
    [SerializeField] Color blockedColor = new Color(1f, 0.2f, 0.15f);

    const int PathPoints = 40; // points the arc is drawn with along a planned jump path

    [Header("Arc look")]
    [Tooltip("How thick the arc is (m) with the camera close by. It gets thicker as the camera pulls back, so big jumps stay easy to read.")]
    [SerializeField, Min(0.005f)] float arcWidth = 0.05f;
    [Tooltip("The arc is drawn at Arc Width up to this camera distance (m), and thicker beyond it.")]
    [SerializeField, Min(0.5f)] float arcWidthDistance = 4f;
    [Tooltip("How see-through the arc is (1 = solid). It takes the marker's colour.")]
    [SerializeField, Range(0.1f, 1f)] float arcOpacity = 0.85f;
    [Tooltip("How far (m) the marker floats above the surface, so the floor never covers it.")]
    [SerializeField, Min(0.001f)] float markerLift = 0.03f;

    // Where the jump being aimed will touch down (valid while HasLanding).
    public bool HasLanding { get; private set; }
    public Vector3 LandingPoint { get; private set; }

    FrogMotor motor;
    FrogNetworkDriver driver;
    readonly List<Vector3> points = new();
    readonly RaycastHit[] hits = new RaycastHit[8];
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    Renderer markerRenderer;
    MaterialPropertyBlock markerColors;
    Vector3 markerPosition, markerVelocity; // the marker's own world pose (it rides on the frog)
    Quaternion markerRotation;

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        driver = GetComponent<FrogNetworkDriver>();
        markerRenderer = landingMarker != null ? landingMarker.GetComponent<Renderer>() : null;
        // The marker draws after other see-through things (goo, water, smoke), so they can't hide it.
        if (markerRenderer != null)
            markerRenderer.material.renderQueue = 3100;
        if (arc != null)
        {
            arc.widthCurve = new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.15f, 1f), new Keyframe(1f, 1f));
            arc.numCapVertices = 4; // rounded ends
        }
        Show(false);
    }

    // The arc's thickness and colour: thicker when the camera is further away, and the colour
    // of the marker (on the ground, or on a wall the frog will cling to).
    void StyleArc(Color color)
    {
        Camera view = Camera.main;
        float distance = view != null ? Vector3.Distance(view.transform.position, transform.position) : arcWidthDistance;
        arc.widthMultiplier = arcWidth * Mathf.Max(1f, distance / arcWidthDistance);
        color.a = arcOpacity;
        arc.startColor = color;
        arc.endColor = color;
    }

    void LateUpdate()
    {
        FrogMode mode = motor.IsSpawned ? motor.Mode.Value : FrogMode.Disabled;
        bool supported = mode == FrogMode.Grounded || mode == FrogMode.Climbing;
        if (!driver.IsOwner || !supported || !driver.LocalJumpHeld)
        {
            HasLanding = false;
            Show(false);
            return;
        }

        points.Clear();
        HasLanding = false;
        RaycastHit landing = default;
        if (driver.HasPath)
        {
            // A jump to the marker: the planned path, exactly as the frog will fly it.
            for (int i = 0; i <= PathPoints; i++)
                points.Add(FrogMotor.PathPoint(driver.PathStart, driver.PathControl1, driver.PathControl2, driver.PathEnd, i / (float)PathPoints));
            HasLanding = true;
            LandingPoint = driver.MarkerPoint;
        }
        else
        {
            // Step the flight the way the physics engine does (gravity first, then move, once per
            // physics step), sweeping a ball the frog's size, until it touches something.
            Vector3 velocity = driver.LocalJumpVelocity;
            float dt = Time.fixedDeltaTime;
            float radius = motor.Radius * 0.95f;
            Vector3 position = transform.position;
            points.Add(position);
            for (float t = 0f; t < maxFlightTime && !HasLanding; t += dt)
            {
                velocity += Physics.gravity * dt;
                Vector3 next = position + velocity * dt;
                HasLanding = Sweep(position, next, radius, out landing);
                position = HasLanding ? position + (next - position).normalized * landing.distance : next;
                points.Add(position);
            }
            if (HasLanding)
                LandingPoint = landing.point;
        }

        // Red when the path would hit something; blue onto a wall; green onto a floor or top.
        Color color = driver.HasPath && driver.PathBlocked ? blockedColor
            : driver.HasMarker && driver.MarkerOnWall ? wallColor : floorColor;
        Show(true);
        if (arc != null)
        {
            arc.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                arc.SetPosition(i, points[i]);
            StyleArc(color);
        }
        // The marker: the spot you aim at with the crosshair (the arc shows whether this charge
        // reaches it), or without one, where the arc comes down. It glides between spots and
        // turns smoothly to lie flat on walls and slopes, and changes colour on a wall the frog
        // will cling to.
        if (landingMarker != null)
        {
            bool placed = driver.HasMarker;
            if (!placed && !HasLanding)
            {
                landingMarker.gameObject.SetActive(false);
                return;
            }
            Vector3 normal = placed ? driver.MarkerNormal : landing.normal;
            Vector3 point = (placed ? driver.MarkerPoint : landing.point) + normal * markerLift;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            if (!landingMarker.gameObject.activeSelf)
            {
                landingMarker.gameObject.SetActive(true); // just appeared: start right there
                markerPosition = point;
                markerRotation = rotation;
                markerVelocity = Vector3.zero;
            }
            else
            {
                markerPosition = Vector3.SmoothDamp(markerPosition, point, ref markerVelocity, markerSmoothTime);
                markerRotation = Quaternion.Slerp(markerRotation, rotation, 1f - Mathf.Exp(-markerTurnSpeed * Time.deltaTime));
            }
            landingMarker.SetPositionAndRotation(markerPosition, markerRotation);
            TintMarker(color);
        }
    }

    // First thing a frog-sized ball moving between two points of the arc would touch,
    // ignoring this frog's own collider.
    bool Sweep(Vector3 from, Vector3 to, float radius, out RaycastHit best)
    {
        Vector3 delta = to - from;
        float length = delta.magnitude;
        best = default;
        if (length < 0.0001f)
            return false;
        int count = Physics.SphereCastNonAlloc(from, radius, delta / length, hits, length, hitMask, QueryTriggerInteraction.Ignore);
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].rigidbody == motor.Body)
                continue;
            if (!found || hits[i].distance < best.distance)
            {
                best = hits[i];
                found = true;
            }
        }
        return found;
    }

    void Show(bool show)
    {
        if (arc != null && arc.enabled != show)
            arc.enabled = show;
        if (!show && landingMarker != null && landingMarker.gameObject.activeSelf)
            landingMarker.gameObject.SetActive(false);
    }

    // The arc and the landing marker: the camera leaves these showing when it hides your frog.
    public bool IsPreviewRenderer(Renderer r) =>
        (arc != null && r == arc) || (landingMarker != null && r.transform.IsChildOf(landingMarker));

    void TintMarker(Color color)
    {
        if (markerRenderer == null)
            return;
        markerColors ??= new MaterialPropertyBlock();
        markerRenderer.GetPropertyBlock(markerColors);
        markerColors.SetColor(BaseColorId, color);
        markerRenderer.SetPropertyBlock(markerColors);
    }
}