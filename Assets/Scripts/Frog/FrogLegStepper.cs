using UnityEngine;
using UnityEngine.Animations.Rigging;

// Procedural foot placement for a RIGGED frog (needs the Animation Rigging package).
// Each leg has a Two Bone IK constraint whose Target this script moves. A foot stays
// planted until it falls too far behind, then steps along a short arc. Diagonal pairs
// step together, and a group only lifts off while the other group is planted.
public class FrogLegStepper : MonoBehaviour
{
    [System.Serializable]
    public class Leg
    {
        public string name;
        public Transform ikTarget; // the Two Bone IK constraint's Target
        public Transform home;     // resting foot spot, a child of the frog body
        public int group;          // 0 or 1. Front-left + back-right = 0, front-right + back-left = 1

        [System.NonSerialized] public Vector3 planted, stepFrom, stepTo, normal;
        [System.NonSerialized] public float stepT = 1f; // 1 = planted
    }

    [SerializeField] Leg[] legs;
    [SerializeField] Rig legRig;
    [SerializeField] FrogMotor motor;
    [Tooltip("Untick the Frog layer so feet never land on the frog's own collider.")]
    [SerializeField] LayerMask footMask = ~0;
    [SerializeField] float stepDistance = 0.18f;
    [SerializeField] float stepHeight = 0.08f;
    [SerializeField] float stepDuration = 0.12f;
    [SerializeField] float probeUp = 0.25f;
    [SerializeField] float probeDown = 0.4f;
    [SerializeField] float maxReach = 0.35f; // safety: snap the step if a foot is left this far behind

    Vector3 lastPosition, velocity;

    void Start()
    {
        lastPosition = transform.position;
        foreach (Leg leg in legs)
        {
            leg.planted = leg.home.position;
            leg.normal = transform.up;
        }
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        velocity = Vector3.Lerp(velocity, (transform.position - lastPosition) / dt, 1f - Mathf.Exp(-15f * dt));
        lastPosition = transform.position;

        FrogMode mode = motor.Mode.Value;
        bool supported = mode == FrogMode.Grounded || mode == FrogMode.Climbing;

        // Airborne: fade IK out so the legs return to the model's own pose, fade back in on landing.
        legRig.weight = Mathf.MoveTowards(legRig.weight, supported ? 1f : 0f, dt * (supported ? 5f : 10f));
        if (!supported)
        {
            foreach (Leg leg in legs)
            {
                leg.planted = leg.home.position;
                leg.stepT = 1f;
                leg.ikTarget.SetPositionAndRotation(leg.home.position, leg.home.rotation);
            }
            return;
        }

        Vector3 up = transform.up; // floor normal on the ground, wall normal while climbing
        foreach (Leg leg in legs)
        {
            FindFoothold(leg, up, out Vector3 desired, out Vector3 normal);
            desired += velocity * stepDuration; // aim a little ahead so feet don't trail

            if (leg.stepT >= 1f)
            {
                float behind = Vector3.Distance(leg.planted, desired);
                bool otherGroupLifted = AnyStepping(1 - leg.group);
                if ((behind > stepDistance && !otherGroupLifted) || behind > maxReach)
                {
                    leg.stepFrom = leg.planted;
                    leg.stepTo = desired;
                    leg.normal = normal;
                    leg.stepT = 0f;
                }
            }

            Vector3 foot;
            if (leg.stepT < 1f)
            {
                leg.stepT = Mathf.Min(1f, leg.stepT + dt / stepDuration);
                foot = Vector3.Lerp(leg.stepFrom, leg.stepTo, leg.stepT) + up * (Mathf.Sin(leg.stepT * Mathf.PI) * stepHeight);
                if (leg.stepT >= 1f)
                    leg.planted = leg.stepTo;
            }
            else
            {
                foot = leg.planted;
            }

            Vector3 footForward = Vector3.ProjectOnPlane(transform.forward, leg.normal);
            if (footForward.sqrMagnitude < 0.0001f)
                footForward = transform.forward;
            leg.ikTarget.SetPositionAndRotation(foot, Quaternion.LookRotation(footForward, leg.normal));
        }
    }

    bool AnyStepping(int group)
    {
        foreach (Leg leg in legs)
        {
            if (leg.group == group && leg.stepT < 1f)
                return true;
        }
        return false;
    }

    // Probe toward the surface the body is on (not always world-down), so it works on walls.
    void FindFoothold(Leg leg, Vector3 up, out Vector3 point, out Vector3 normal)
    {
        Vector3 origin = leg.home.position + up * probeUp;
        if (Physics.Raycast(origin, -up, out RaycastHit hit, probeUp + probeDown, footMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            normal = hit.normal;
        }
        else
        {
            point = leg.home.position; // nothing to stand on: tuck toward the rest pose
            normal = up;
        }
    }
}
