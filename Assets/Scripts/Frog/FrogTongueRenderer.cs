using Unity.Netcode;
using UnityEngine;

// Draws the tongue on every machine from FrogTongue.Sync. Purely cosmetic:
// the curve, sag, thinning and recoil wobble never feed back into gameplay.
[RequireComponent(typeof(LineRenderer))]
public class FrogTongueRenderer : MonoBehaviour
{
    [SerializeField] FrogTongue tongue;
    [Tooltip("Optional: a point on the model's lips. Defaults to the gameplay mouth.")]
    [SerializeField] Transform visualMouth;
    [SerializeField] int segments = 20;
    [SerializeField] float width = 0.06f;
    [SerializeField] float maxSag = 0.5f;
    [SerializeField] float recoilWobble = 0.12f;

    LineRenderer line;
    FrogTuning tuning;
    Vector3 tip;
    float tipDistance;
    float recoil;
    TongueState lastState;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = segments;
        line.enabled = false;
    }

    void Start()
    {
        tuning = tongue.GetComponent<FrogMotor>().Tuning;
    }

    void LateUpdate()
    {
        if (!tongue.IsSpawned)
        {
            line.enabled = false;
            return;
        }

        TongueSync sync = tongue.Sync.Value;
        Vector3 mouth = (visualMouth != null ? visualMouth : tongue.Mouth).position;
        float dt = Time.deltaTime;

        // Rebuild the motion locally from state changes instead of streaming the tip position.
        if (sync.State != lastState)
        {
            if (sync.State == TongueState.Extending)
            {
                tipDistance = 0f;
                tip = mouth;
            }
            if (sync.State == TongueState.Retracting)
                recoil = 1f;
            lastState = sync.State;
        }

        float tension = 0f;
        switch (sync.State)
        {
            case TongueState.Idle:
                line.enabled = false;
                return;

            case TongueState.Extending:
                tipDistance = Mathf.Min(tipDistance + tuning.tongueExtendSpeed * dt, tuning.tongueRange);
                tip = mouth + sync.Direction * tipDistance;
                break;

            case TongueState.Attached:
                if (sync.Target.TryGet(out NetworkObject held))
                    tip = held.transform.TransformPoint(sync.LocalAnchor);
                tipDistance = Vector3.Distance(mouth, tip);
                tension = sync.Tension;
                break;

            case TongueState.Retracting:
                Vector3 back = tip - mouth;
                tipDistance = Mathf.Max(0f, Mathf.Min(tipDistance, back.magnitude) - tuning.tongueRetractSpeed * dt);
                tip = mouth + (back.sqrMagnitude > 0.0001f ? back.normalized : sync.Direction) * tipDistance;
                break;
        }

        Draw(mouth, tension, sync.State == TongueState.Attached, dt);
    }

    void Draw(Vector3 mouth, float tension, bool attached, float dt)
    {
        line.enabled = true;
        float length = Vector3.Distance(mouth, tip);

        // Slack tongues droop; taut tongues go straight and thin.
        float sag = attached ? Mathf.Lerp(Mathf.Min(maxSag, length * 0.15f), 0f, tension) : length * 0.03f;
        Vector3 control = (mouth + tip) * 0.5f + Vector3.down * sag;

        recoil = Mathf.MoveTowards(recoil, 0f, dt * 4f);
        Vector3 along = length > 0.001f ? (tip - mouth) / length : Vector3.forward;
        Vector3 side = Vector3.Cross(along, Vector3.up);
        if (side.sqrMagnitude < 0.01f)
            side = Vector3.right;
        side.Normalize();

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            float u = 1f - t;
            Vector3 point = u * u * mouth + 2f * u * t * control + t * t * tip; // quadratic Bezier
            point += side * (Mathf.Sin(t * Mathf.PI * 3f + Time.time * 40f) * recoil * recoilWobble * t);
            line.SetPosition(i, point);
        }
        line.widthMultiplier = width * Mathf.Lerp(1f, 0.55f, tension);
    }
}
