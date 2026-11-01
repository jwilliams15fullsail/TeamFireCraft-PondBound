using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public enum TongueState : byte { Idle, Extending, Attached, Retracting }

// Everything a client needs to draw the tongue, packed in one variable so it always arrives together.
// FishNet writes this struct over the network automatically, including the object reference.
public struct TongueSync : IEquatable<TongueSync>
{
    public TongueState State;
    public Vector3 Direction;              // firing direction while extending
    public NetworkObject Target;           // what the tongue is stuck to (null when not attached)
    public Vector3 LocalAnchor;            // hit point in the target's local space
    public float RestLength;
    public float Tension;                  // 0..1, drives thinning and sag

    public bool Equals(TongueSync other)
    {
        return State == other.State && Direction == other.Direction && Target == other.Target
            && LocalAnchor == other.LocalAnchor && Mathf.Approximately(RestLength, other.RestLength)
            && Mathf.Approximately(Tension, other.Tension);
    }
}

// Host-side tongue: swept hit detection, a tension-only elastic constraint, reeling,
// eating and release rules. FrogTongueRenderer draws it on every machine.
public class FrogTongue : NetworkBehaviour
{
    [Tooltip("Gameplay mouth point. A child of the frog root, NOT of the visual model.")]
    [SerializeField] Transform mouth;
    [SerializeField] LayerMask hitMask = ~0;

    public readonly SyncVar<TongueSync> Sync = new(new SyncTypeSettings(0.001f));
    public Transform Mouth => mouth;

    // Host: every spawned tongue, for "one grab per target" and "break free".
    static readonly List<FrogTongue> spawned = new();

    FrogMotor motor;
    FrogNetworkDriver driver;
    FrogClimb climb;
    FrogElectricCharge electricCharge;
    Rigidbody rb;
    readonly RaycastHit[] hits = new RaycastHit[16];

    TongueState state;
    Vector3 direction;
    float tipDistance;
    float restLength;
    float blockedTime;
    TongueTarget target;
    Vector3 localAnchor;
    bool pressQueued;
    Vector3 aimOrigin, aimDirection;

    FrogTuning Tuning => motor.Tuning;

    void Awake()
    {
        motor = GetComponent<FrogMotor>();
        driver = GetComponent<FrogNetworkDriver>();
        climb = GetComponent<FrogClimb>();
        rb = GetComponent<Rigidbody>();
        electricCharge = GetComponent<FrogElectricCharge>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        spawned.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        spawned.Remove(this);
    }

    // ---- Commands (host only) ----

    public void PressTongue(Vector3 origin, Vector3 dir)
    {
            if (dir.sqrMagnitude < 0.5f || motor.IsDead)
                return; // ignore garbage aim data, and a dead frog
        pressQueued = true;
        aimOrigin = origin;
        aimDirection = dir.normalized;
    }
      // The frog died: whatever the tongue was doing, it comes back.
   public void LetGo()
   {
        pressQueued = false;
        if (state == TongueState.Extending || state == TongueState.Attached)
           StartRetract();
   }

    // Break free: drop every tongue currently holding this object.
    public static void ReleaseEverythingHolding(NetworkObject grabbed)
    {
        foreach (FrogTongue tongue in spawned)
        {
            if (tongue.target != null && tongue.target.NetObject == grabbed)
                tongue.StartRetract();
        }
    }

    static bool IsHeldByAnyone(TongueTarget candidate)
    {
        foreach (FrogTongue tongue in spawned)
        {
            if (tongue.target == candidate)
                return true;
        }
        return false;
    }

    // ---- Simulation (host only) ----

    void FixedUpdate()
    {
        if (!IsServerInitialized || !IsSpawned)
            return;

        float dt = Time.fixedDeltaTime;
        if (pressQueued)
        {
            pressQueued = false;
            OnPress();
        }

        switch (state)
        {
            case TongueState.Extending:
                Extend(dt);
                break;
            case TongueState.Attached:
                Hold(dt);
                break;
            case TongueState.Retracting:
                tipDistance -= Tuning.tongueRetractSpeed * dt;
                if (tipDistance <= 0f)
                    SetState(TongueState.Idle);
                break;
        }
    }

    void OnPress()
    {
        if (state == TongueState.Attached)
        {
            StartRetract(); // second click lets go
            return;
        }
        if (state != TongueState.Idle)
            return;

        // Fire from the mouth toward whatever is under the crosshair.
        Vector3 aimPoint = aimOrigin + aimDirection * 100f;
        if (CastIgnoringSelf(aimOrigin, aimDirection, 0f, 100f, out RaycastHit aimHit))
            aimPoint = aimHit.point;
        direction = (aimPoint - mouth.position).normalized;
        tipDistance = 0f;
        SetState(TongueState.Extending);
    }


    void Extend(float dt)
    {
        // Swept test from the old tip to the new tip, so a fast tongue can't skip past a fly.
        float step = Tuning.tongueExtendSpeed * dt;
        Vector3 from = mouth.position + direction * tipDistance;

        if (CastIgnoringSelf(from, direction, Tuning.tongueRadius, step, out RaycastHit hit))
        {
            // Check whether the tongue hit an enemy.
            EnemyStun enemy = hit.collider.GetComponentInParent<EnemyStun>();

            if (enemy != null && electricCharge != null)
            {
                // Only consume electricity when an enemy is successfully hit.
                if (electricCharge.TryConsumeCharge(out float stunDuration))
                {
                    enemy.ApplyStun(stunDuration);
                    Debug.Log($"Electrified tongue stunned {enemy.name}!");

                    StartRetract();
                    return;
                }
            }

            TongueTarget hitTarget = hit.collider.GetComponentInParent<TongueTarget>();
            if (hitTarget != null && hitTarget.NetObject.IsSpawned && !IsHeldByAnyone(hitTarget))
                Attach(hitTarget, hit.distance > 0f ? hit.point : from);
            else
                StartRetract(); // plain scenery: the first version doesn't wrap or stick
            return;
        }

        tipDistance += step;
        if (tipDistance >= Tuning.tongueRange)
            StartRetract();
    }

    void Attach(TongueTarget newTarget, Vector3 point)
    {
        target = newTarget;
        //added to attach to buttons -Jeff
        if (newTarget.TryGetComponent(out WallButton button))
        {
            button.Activate();
            StartRetract();
            return;
        }
        //added for food pile prefab -Tim
        if (newTarget.TryGetComponent(out FoodPile pile))
        {
            pile.OnTongueHit(this, point);
            return;
        }
        // Stop a firefly wandering while the tongue is holding it.
        if (target.TryGetComponent(out FireflyWander firefly))
            firefly.SetCaught(true);

        localAnchor = target.transform.InverseTransformPoint(point);
        restLength = Vector3.Distance(mouth.position, point); // no sudden yank on contact
        tipDistance = restLength;
        blockedTime = 0f;
        SetState(TongueState.Attached);
    }
    //needed for food pile -Tim
    public void ForceAttach(TongueTarget target, Vector3 point)
    {
        if (!IsServerInitialized) return;
        Attach(target, point);
    }

    void Hold(float dt)
    {

        if (target == null || !target.NetObject.IsSpawned)
        {
            StartRetract(); // target was eaten, destroyed, or its player left
            return;
        }

        Vector3 mouthPos = mouth.position;
        Vector3 anchor = target.transform.TransformPoint(localAnchor);
        Vector3 delta = anchor - mouthPos;
        float distance = delta.magnitude;
        tipDistance = distance;

        if (distance > Tuning.tongueBreakDistance)
        {
            StartRetract();
            return;
        }

        // Scenery between mouth and anchor: allow a brief graze, then snap.
        if (IsBlocked(mouthPos, anchor))
        {
            blockedTime += dt;
            if (blockedTime > Tuning.blockedBreakTime)
            {
                StartRetract();
                return;
            }
        }
        else
        {
            blockedTime = 0f;
        }

        if (target.kind == TongueTargetKind.Food)
        {
            ReelFood(anchor, distance);
            return;
        }

        if (driver.CurrentInput.Reel)
            restLength = Mathf.Max(Tuning.minRestLength, restLength - Tuning.reelSpeed * dt);

        // Tension-only spring: pulls when stretched past rest length, slack otherwise.
        float tension = 0f;
        float stretch = distance - restLength;
        if (stretch > 0f && distance > 0.001f)
        {
            Vector3 dir = delta / distance;
            Rigidbody other = target.Body;
            Vector3 otherVelocity = other != null ? other.GetPointVelocity(anchor) : Vector3.zero;
            float separating = Vector3.Dot(otherVelocity - rb.linearVelocity, dir);
            tension = Mathf.Clamp(Tuning.tongueStiffness * stretch + Tuning.tongueDamping * separating, 0f, Tuning.tongueMaxTension);

            // Equal and opposite forces. Mass decides who moves more, except that a frog
            // standing on the ground braces its feet (see BracedPull).
            Vector3 force = dir * tension;
            rb.AddForce(BracedPull(force));
            climb.AddExternalForce(force);
            if (other != null && !other.isKinematic)
            {
                other.AddForceAtPosition(-force, anchor);
                if (target.Climb != null)
                    target.Climb.AddExternalForce(-force); // a clinging teammate's grip feels it
            }
        }
        PushSync(tension / Tuning.tongueMaxTension);
    }
    // The part of the tongue's pull that actually moves this frog. On the ground its feet
    // push against the ground and soak up to tongueBraceForce of the pull along it, so a
    // light frog can drag a heavy box instead of being dragged to it. Lifts still work.
    Vector3 BracedPull(Vector3 force)
    {
        if (motor.Mode.Value != FrogMode.Grounded)
            return force;
        Vector3 along = Vector3.ProjectOnPlane(force, motor.GroundNormal);
        if (along.sqrMagnitude < 1e-6f)
            return force;
        return force - along.normalized * Mathf.Min(along.magnitude, Tuning.tongueBraceForce);
    }

    void ReelFood(Vector3 anchor, float distance)
    {
        if (distance <= Tuning.eatDistance)
        {
            // Check before consuming, because Consume despawns the food.
            bool ateElectricFirefly =
                target != null && target.TryGetComponent(out ElectricFirefly electricFirefly);

            target.Consume(this); // the host eats it exactly once

            // Preserve a newly earned charge when eating an Electric Firefly.
            if (ateElectricFirefly)
                StartRetract(false);
            else
                StartRetract();

            return;
        }

        // Food is very light, which would make a spring unstable, so drive it to the mouth directly.
        Rigidbody food = target.Body;
        if (food != null && !food.isKinematic)
            food.linearVelocity = (mouth.position - anchor).normalized * Tuning.foodReelSpeed + rb.linearVelocity;

        PushSync(1f);
    }

    void StartRetract(bool consumeElectricCharge = true)
    {
        // Let a released firefly resume wandering, if it still exists.
        if (target != null && target.TryGetComponent(out FireflyWander firefly))
            firefly.SetCaught(false);

        // An electrified tongue only gets one attempt.
        // Preserve the charge immediately after eating an Electric Firefly.
        if (consumeElectricCharge && electricCharge != null && electricCharge.HasCharge.Value)
        {
            electricCharge.TryConsumeCharge(out _);
            Debug.Log("Electric tongue charge lost after retracting!");
        }

        target = null;
        SetState(TongueState.Retracting);
    }

    void SetState(TongueState newState)
    {
        state = newState;
        if (newState != TongueState.Attached)
            target = null;
        PushSync(0f);
    }

    void PushSync(float tension01)
    {
        Sync.Value = new TongueSync
        {
            State = state,
            Direction = direction,

            LocalAnchor = localAnchor,
            RestLength = Mathf.Round(restLength * 50f) / 50f, // rounding = fewer network updates
            Tension = Mathf.Round(tension01 * 20f) / 20f,
            Target = target != null ? target.NetObject : null,
        };
    }

    bool IsBlocked(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        float length = delta.magnitude;
        if (length < 0.1f)
            return false;

        int count = Physics.RaycastNonAlloc(from, delta / length, hits, length - 0.05f, hitMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].rigidbody == rb)
                continue;
            if (hits[i].collider.GetComponentInParent<TongueTarget>() == target)
                continue;
            return true;
        }
        return false;
    }

    // Nearest hit along a ray or sphere sweep, skipping this frog's own collider.
    bool CastIgnoringSelf(Vector3 origin, Vector3 dir, float radius, float distance, out RaycastHit best)
    {
        int count = radius > 0f
            ? Physics.SphereCastNonAlloc(origin, radius, dir, hits, distance, hitMask, QueryTriggerInteraction.Ignore)
            : Physics.RaycastNonAlloc(origin, dir, hits, distance, hitMask, QueryTriggerInteraction.Ignore);

        best = default;
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].rigidbody == rb)
                continue;
            if (!found || hits[i].distance < best.distance)
            {
                best = hits[i];
                found = true;
            }
        }
        return found;
    }
}
