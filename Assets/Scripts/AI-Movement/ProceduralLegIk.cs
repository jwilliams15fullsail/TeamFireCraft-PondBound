using System.Collections.Generic;
using UnityEngine;

public class ProceduralLegIK : MonoBehaviour
{
    [System.Serializable]
    public class LegData
    {
        public string legName;
        [Header("Bone Hierarchy (Root -> Mid -> Tip)")]
        public Transform hipBone;       // Upper leg bone (thigh)
        public Transform kneeBone;      // Middle leg bone (shin)
        public Transform footTip;       // End of the leg (foot)
        public Transform poleTarget;    // Empty transform placed above/outward to bend the knee upward

        [Header("Gait & Grounding")]
        public Transform homeTarget;    // Empty transform marking where this foot rests relative to body
        public int oppositeLegIndex = -1; // Index of neighboring leg that must stay grounded while this steps

        [HideInInspector] public Vector3 currentWorldPos;
        [HideInInspector] public Vector3 oldWorldPos;
        [HideInInspector] public Vector3 newWorldPos;
        [HideInInspector] public float stepProgress = 1f;
        [HideInInspector] public bool IsStepping => stepProgress < 1f;
    }

    [Header("Creature Legs Setup")]
    [SerializeField] private List<LegData> legs = new List<LegData>();

    [Header("Step & Raycast Settings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float stepDistance = 0.45f;     // How far body moves before foot takes a step
    [SerializeField] private float stepHeight = 0.3f;      // How high the foot lifts during a step
    [SerializeField] private float stepSpeed = 4f;          // How fast the foot moves to its new spot
    [SerializeField] private float raycastHeightOffset = 1.0f;
    [SerializeField] private float raycastMaxDistance = 2.5f;
    [SerializeField] private float footGroundOffset = 0.03f;// Prevents foot tip from sinking into floor

    [Header("Optional Body Tilt")]
    [SerializeField] private Transform bodyMesh;            // Assign child mesh if you want body to tilt with slopes
    [SerializeField] private float bodyAdaptSpeed = 8f;

    [Header("IK Blending & Pause Settings")]
    [Range(0f, 1f)][SerializeField] private float ikWeight = 1f;
    [SerializeField] private float blendSpeed = 4f;
    private float targetIKWeight = 1f;

    private void Start()
    {
        // Initialize all feet to their starting ground positions
        for (int i = 0; i < legs.Count; i++)
        {
            LegData leg = legs[i];
            if (leg.homeTarget == null || leg.footTip == null) continue;

            Vector3 startGround = GetGroundPoint(leg.homeTarget.position);
            leg.currentWorldPos = startGround;
            leg.oldWorldPos = startGround;
            leg.newWorldPos = startGround;
            leg.stepProgress = 1f;
        }
    }

    // LateUpdate runs AFTER EnemyAI movement and Animations, cleanly overriding leg bones
    private void LateUpdate()
    {
        // Smoothly fade IK weight in or out
        ikWeight = Mathf.MoveTowards(ikWeight, targetIKWeight, Time.deltaTime * blendSpeed);

        // If IK is completely paused, let the Legacy/Animator clip control the legs freely
        if (ikWeight <= 0.001f) return;


        for (int i = 0; i < legs.Count; i++)
        {
            UpdateLegGait(i);
            SolveTwoBoneIK(legs[i]);
        }

        AdaptBodyToGround();
    }

    private void UpdateLegGait(int index)
    {
        LegData leg = legs[index];
        if (leg.homeTarget == null) return;

        // Find where the foot ideally wants to be right now
        Vector3 desiredGroundPos = GetGroundPoint(leg.homeTarget.position);

        // Check if our designated opposite/neighbor leg is currently mid-air
        bool oppositeIsStepping = false;
        if (leg.oppositeLegIndex >= 0 && leg.oppositeLegIndex < legs.Count)
        {
            oppositeIsStepping = legs[leg.oppositeLegIndex].IsStepping;
        }

        // Trigger a new step if far enough from desired spot and neighbor is planted
        if (!leg.IsStepping && !oppositeIsStepping && Vector3.Distance(leg.currentWorldPos, desiredGroundPos) > stepDistance)
        {
            leg.stepProgress = 0f;
            leg.oldWorldPos = leg.currentWorldPos;
            leg.newWorldPos = desiredGroundPos;
        }

        // Animate the foot along a parabolic arc toward the new ground point
        if (leg.IsStepping)
        {
            leg.stepProgress += Time.deltaTime * stepSpeed;
            float progress = Mathf.Clamp01(leg.stepProgress);

            // Continuously update target in case the spider is moving fast
            leg.newWorldPos = GetGroundPoint(leg.homeTarget.position);

            Vector3 footPos = Vector3.Lerp(leg.oldWorldPos, leg.newWorldPos, progress);
            footPos.y += Mathf.Sin(progress * Mathf.PI) * stepHeight; // Smooth vertical lift arc

            leg.currentWorldPos = footPos;
        }
    }

    private Vector3 GetGroundPoint(Vector3 homePosition)
    {
        Vector3 rayOrigin = homePosition + Vector3.up * raycastHeightOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastMaxDistance, groundMask))
        {
            return hit.point + Vector3.up * footGroundOffset;
        }
        return homePosition;
    }

    // Analytical Two-Bone IK Solver (Works on Legacy & Generic rigs)
    private void SolveTwoBoneIK(LegData leg)
    {
        // Capture the animation clip's current bone rotations before IK overrides them
        Quaternion animHipRot = leg.hipBone.rotation;
        Quaternion animKneeRot = leg.kneeBone.rotation;

        Vector3 a = leg.hipBone.position;
        Vector3 b = leg.kneeBone.position;
        Vector3 c = leg.footTip.position;
        Vector3 target = leg.currentWorldPos;
        Vector3 pole = leg.poleTarget != null ? leg.poleTarget.position : (a + Vector3.up);

        float upperLength = Vector3.Distance(a, b);
        float lowerLength = Vector3.Distance(b, c);
        float totalLength = upperLength + lowerLength;

        Vector3 ac = target - a;
        float dist = Mathf.Clamp(ac.magnitude, 0.01f, totalLength * 0.99f);

        // Align the plane of the leg (Hip -> Knee -> Foot) with the Pole Target
        Vector3 currentPlaneNormal = Vector3.Cross(c - a, b - a);
        Vector3 targetPlaneNormal = Vector3.Cross(ac, pole - a);

        if (currentPlaneNormal.sqrMagnitude > 0.0001f && targetPlaneNormal.sqrMagnitude > 0.0001f)
        {
            Quaternion planeAlign = Quaternion.FromToRotation(currentPlaneNormal, targetPlaneNormal);
            leg.hipBone.rotation = planeAlign * leg.hipBone.rotation;
        }

        // Recalculate joint positions after plane alignment
        b = leg.kneeBone.position;
        c = leg.footTip.position;

        // Law of Cosines for interior angles
        float cosUpper = Mathf.Clamp((upperLength * upperLength + dist * dist - lowerLength * lowerLength) / (2f * upperLength * dist), -1f, 1f);
        float desiredUpperAngle = Mathf.Acos(cosUpper) * Mathf.Rad2Deg;

        float currentUpperAngle = Vector3.Angle(c - a, b - a);
        Vector3 bendAxis = Vector3.Cross(ac, pole - a).normalized;
        if (bendAxis.sqrMagnitude < 0.0001f) bendAxis = transform.right;

        // Rotate Hip to match the desired upper angle
        leg.hipBone.rotation = Quaternion.AngleAxis(desiredUpperAngle - currentUpperAngle, bendAxis) * leg.hipBone.rotation;

        // Rotate Knee so the Foot Tip lands directly on the target
        b = leg.kneeBone.position;
        c = leg.footTip.position;
        Quaternion kneeAlign = Quaternion.FromToRotation((c - b).normalized, (target - b).normalized);
        leg.kneeBone.rotation = kneeAlign * leg.kneeBone.rotation;

        // Smoothly blend between the raw animation rotation and the solved IK rotation
        if (ikWeight < 0.999f)
        {
            leg.hipBone.rotation = Quaternion.Slerp(animHipRot, leg.hipBone.rotation, ikWeight);
            leg.kneeBone.rotation = Quaternion.Slerp(animKneeRot, leg.kneeBone.rotation, ikWeight);
        }
    }

    private void AdaptBodyToGround()
    {
        if (bodyMesh == null || legs.Count < 4) return;

        // Calculate average foot height to subtly raise/lower body on uneven rocks
        float avgY = 0f;
        for (int i = 0; i < legs.Count; i++) avgY += legs[i].currentWorldPos.y;
        avgY /= legs.Count;

        Vector3 targetLocalPos = bodyMesh.localPosition;
        float heightDelta = avgY - transform.position.y;
        targetLocalPos.y = Mathf.Lerp(targetLocalPos.y, heightDelta, Time.deltaTime * bodyAdaptSpeed);
        bodyMesh.localPosition = targetLocalPos;
    }

    // Public method called by SpiderAnimationController to pause/resume IK
    public void SetIKActive(bool active)
    {
        targetIKWeight = active ? 1f : 0f;
    }

    // Helper to snap all feet to the ground immediately when IK turns back on
    public void ResetFeetToGround()
    {
        for (int i = 0; i < legs.Count; i++)
        {
            if (legs[i].homeTarget == null) continue;
            Vector3 groundPos = GetGroundPoint(legs[i].homeTarget.position);
            legs[i].currentWorldPos = groundPos;
            legs[i].oldWorldPos = groundPos;
            legs[i].newWorldPos = groundPos;
            legs[i].stepProgress = 1f;

            // Stagger odd-indexed legs slightly backward so they reach stepDistance first,
            // preventing adjacent legs from stepping simultaneously after an attack or taunt
            if (i % 2 == 1)
            {
                legs[i].currentWorldPos -= transform.forward * (stepDistance * 0.45f);
            }

            legs[i].oldWorldPos = legs[i].currentWorldPos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (LegData leg in legs)
        {
            if (leg.homeTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(leg.homeTarget.position, 0.08f);
            }
            if (Application.isPlaying)
            {
                Gizmos.color = leg.IsStepping ? Color.red : Color.cyan;
                Gizmos.DrawSphere(leg.currentWorldPos, 0.07f);
            }
        }
    }
}