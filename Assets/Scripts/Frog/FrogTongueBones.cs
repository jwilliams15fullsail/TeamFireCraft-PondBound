using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// The frog model's own tongue as a stretchy, bendy tongue (replaces the line).
// On start it cleans up the tongue's skin weights on a private copy of the mesh, so
// the tongue separates cleanly from the mouth instead of leaving a stub behind. Then,
// every frame the tongue is out, it lays the tongue bones along a curve from the
// mouth to the tip that FrogTongue's synced state describes: sagging when slack,
// straight when pulled tight, wobbling as it snaps back. Purely visual; runs on
// every machine.
[DefaultExecutionOrder(100)] // after the other LateUpdates that move the model
public class FrogTongueBones : MonoBehaviour
{
    [SerializeField] FrogTongue tongue;
    [Tooltip("The frog's Skinned Mesh Renderer (Frog1_mesh).")]
    [SerializeField] SkinnedMeshRenderer frogMesh;
    [Tooltip("The tongue's root bone (lang_root).")]
    [SerializeField] Transform tongueRoot;
    [Tooltip("The bone at the tongue's tip (leng_tile).")]
    [SerializeField] Transform tongueTip;
    [Tooltip("Tongue thickness while stretched. 1 = as thick as in the open-mouth pose.")]
    [SerializeField] float thickness = 1f;
    [SerializeField] float maxSag = 0.5f;
    [SerializeField] float recoilWobble = 0.12f;
    [Tooltip("Vertices weighted at least this much to the tongue become pure tongue; the rest stay with the mouth.")]
    [SerializeField, Range(0.1f, 0.9f)] float tongueWeightSplit = 0.5f;

    FrogTuning tuning;
    Transform[] chain;       // root ... tip
    float[] restFractions;   // where each bone sits along the tongue, 0 = root, 1 = tip
    Mesh cleanMesh;
    Vector3 tip;
    float tipDistance, recoil;
    TongueState lastState;

    void Awake()
    {
        var bones = new List<Transform>();
        for (Transform t = tongueTip; t != null; t = t.parent)
        {
            bones.Add(t);
            if (t == tongueRoot)
                break;
        }
        bones.Reverse();
        chain = bones.ToArray();

        restFractions = new float[chain.Length];
        float total = 0f;
        for (int i = 1; i < chain.Length; i++)
        {
            total += Vector3.Distance(chain[i - 1].position, chain[i].position);
            restFractions[i] = total;
        }
        for (int i = 1; i < chain.Length; i++)
            restFractions[i] = total > 0f ? restFractions[i] / total : i / (chain.Length - 1f);

        CleanUpTongueWeights();
    }

    void Start()
    {
        tuning = tongue.GetComponent<FrogMotor>().Tuning;
    }

    void OnDestroy()
    {
        if (cleanMesh != null)
            Destroy(cleanMesh);
    }

    // Most tongue vertices are also weighted to the mouth, head and even shoulder
    // bones. Stretching the tongue would then drag half of them along and leave the
    // rest in the mouth (the "second tongue"). Each shared vertex is given wholly to
    // whichever side owns most of it.
    void CleanUpTongueWeights()
    {
        if (frogMesh == null)
            return;

        Transform[] skinBones = frogMesh.bones;
        var isTongue = new bool[skinBones.Length];
        for (int b = 0; b < skinBones.Length; b++)
            isTongue[b] = System.Array.IndexOf(chain, skinBones[b]) >= 0;

        cleanMesh = Instantiate(frogMesh.sharedMesh);
        cleanMesh.name = frogMesh.sharedMesh.name + " (clean tongue)";
        BoneWeight[] weights = cleanMesh.boneWeights;
        for (int v = 0; v < weights.Length; v++)
        {
            BoneWeight w = weights[v];
            int[] index = { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 };
            float[] weight = { w.weight0, w.weight1, w.weight2, w.weight3 };

            float tongueShare = 0f;
            for (int k = 0; k < 4; k++)
                if (isTongue[index[k]])
                    tongueShare += weight[k];
            if (tongueShare <= 0.001f || tongueShare >= 0.999f)
                continue; // already all tongue or no tongue

            bool keepTongue = tongueShare >= tongueWeightSplit;
            float sum = 0f;
            for (int k = 0; k < 4; k++)
            {
                if (isTongue[index[k]] != keepTongue)
                    weight[k] = 0f;
                sum += weight[k];
            }
            for (int k = 0; k < 4; k++)
                weight[k] = sum > 0f ? weight[k] / sum : 0f;

            w.weight0 = weight[0];
            w.weight1 = weight[1];
            w.weight2 = weight[2];
            w.weight3 = weight[3];
            weights[v] = w;
        }
        cleanMesh.boneWeights = weights;
        frogMesh.sharedMesh = cleanMesh;
        frogMesh.updateWhenOffscreen = true; // a long tongue reaches far outside the frog's normal bounds
    }

    void LateUpdate()
    {
        if (!tongue.IsSpawned)
            return;

        TongueSync sync = tongue.Sync.Value;
        Vector3 mouth = tongueRoot.position;
        float dt = Time.deltaTime;

        // How far the animation (Lang / MouthOpen) already sticks the tongue out.
        float animatedLength = Vector3.Distance(tongueRoot.position, tongueTip.position);

        if (sync.State != lastState)
        {
            if (sync.State == TongueState.Extending)
                tipDistance = animatedLength;
            if (sync.State == TongueState.Retracting)
                recoil = 1f;
            lastState = sync.State;
        }

        float tension = 0f;
        bool attached = false;
        switch (sync.State)
        {
            case TongueState.Idle:
                return; // the Animator's pose: resting tongue or the mouth closing

            case TongueState.Extending:
                tipDistance = Mathf.Min(tipDistance + tuning.tongueExtendSpeed * dt, tuning.tongueRange);
                tip = mouth + sync.Direction * tipDistance;
                break;

            case TongueState.Attached:
                if (sync.Target.TryGet(out NetworkObject held))
                    tip = held.transform.TransformPoint(sync.LocalAnchor);
                tipDistance = Vector3.Distance(mouth, tip);
                tension = sync.Tension;
                attached = true;
                break;

            case TongueState.Retracting:
                Vector3 back = tip - mouth;
                tipDistance = Mathf.Min(tipDistance, back.magnitude) - tuning.tongueRetractSpeed * dt;
                if (tipDistance <= animatedLength)
                    return; // back to the animated length: let the animation finish the job
                tip = mouth + (back.sqrMagnitude > 0.0001f ? back.normalized : sync.Direction) * tipDistance;
                break;
        }

        LayAlongCurve(mouth, tip, tension, attached, dt);
    }

    void LayAlongCurve(Vector3 mouth, Vector3 end, float tension, bool attached, float dt)
    {
        float length = Vector3.Distance(mouth, end);

        // Slack tongues droop; taut tongues go straight.
        float sag = attached ? Mathf.Lerp(Mathf.Min(maxSag, length * 0.15f), 0f, tension) : length * 0.03f;
        Vector3 control = (mouth + end) * 0.5f + Vector3.down * sag;

        recoil = Mathf.MoveTowards(recoil, 0f, dt * 4f);
        Vector3 along = length > 0.001f ? (end - mouth) / length : tongueRoot.up;
        Vector3 side = Vector3.Cross(along, Vector3.up);
        if (side.sqrMagnitude < 0.01f)
            side = Vector3.right;
        side.Normalize();

        tongueRoot.localScale = Vector3.one * thickness;

        // Walk from the root to the tip: point each bone at the next curve point, then
        // move the next bone onto it. The skin between bones stretches to follow.
        for (int i = 0; i < chain.Length - 1; i++)
        {
            float t = restFractions[i + 1];
            float u = 1f - t;
            Vector3 next = u * u * mouth + 2f * u * t * control + t * t * end; // quadratic Bezier
            next += side * (Mathf.Sin(t * Mathf.PI * 3f + Time.time * 40f) * recoil * recoilWobble * t);

            Vector3 current = chain[i + 1].position - chain[i].position;
            Vector3 wanted = next - chain[i].position;
            if (current.sqrMagnitude > 1e-8f && wanted.sqrMagnitude > 1e-8f)
                chain[i].rotation = Quaternion.FromToRotation(current, wanted) * chain[i].rotation;
            chain[i + 1].position = next;
        }
    }
}