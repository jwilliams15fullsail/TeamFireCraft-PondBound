using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class MecanimAnimationController : NetworkBehaviour
{
    [Header("Animator Parameter Names")]
    [Tooltip("Float parameter controlling Idle (0), Walk (1), and Run (2)")]
    [SerializeField] private string moveStateParam = "MoveState";
    [SerializeField] private string tauntTriggerParam = "Taunt";
    [SerializeField] private string attackTriggerParam = "Attack";
    [SerializeField] private string attackIndexParam = "AttackIndex";

    [Header("Attack Variations")]
    [Tooltip("Total number of attack clips inside your Animator Controller")]
    [SerializeField] private int attackVariationCount = 1;

    [Header("Smoothing")]
    [SerializeField] private float blendDampTime = 0.12f;

    private Animator animator;
    private NetworkAnimator networkAnimator;
    private EnemyAI enemyAI;
    private WaypointMover waypointMover;

    private int moveStateHash;
    private int tauntHash;
    private int attackHash;
    private int attackIndexHash;
    private float targetMoveState = 0f;

    private void CacheComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        if (networkAnimator == null)
        {
            networkAnimator = GetComponent<NetworkAnimator>();
            if (networkAnimator == null) networkAnimator = GetComponentInChildren<NetworkAnimator>();
        }

        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        if (waypointMover == null) waypointMover = GetComponent<WaypointMover>();

        moveStateHash = Animator.StringToHash(moveStateParam);
        tauntHash = Animator.StringToHash(tauntTriggerParam);
        attackHash = Animator.StringToHash(attackTriggerParam);
        attackIndexHash = Animator.StringToHash(attackIndexParam);
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheComponents();

        if (enemyAI != null)
        {
            enemyAI.OnIdle += HandleIdle;
            enemyAI.OnWalk += HandleWalk;
            enemyAI.OnRun += HandleRun;
            enemyAI.OnTaunt += HandleTaunt;
            enemyAI.OnAttack += HandleAttack;
        }

        if (waypointMover != null)
        {
            waypointMover.OnIdle += HandleIdle;
            waypointMover.OnWalk += HandleWalk;
        }
    }

    private void OnDisable()
    {
        if (enemyAI != null)
        {
            enemyAI.OnIdle -= HandleIdle;
            enemyAI.OnWalk -= HandleWalk;
            enemyAI.OnRun -= HandleRun;
            enemyAI.OnTaunt -= HandleTaunt;
            enemyAI.OnAttack -= HandleAttack;
        }

        if (waypointMover != null)
        {
            waypointMover.OnIdle -= HandleIdle;
            waypointMover.OnWalk -= HandleWalk;
        }
    }

    private void Update()
    {
        if (animator == null) return;

        // Safe network check using InstanceFinder (never throws NullReferenceException offline)
        if (InstanceFinder.NetworkManager != null && InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted)
        {
            return;
        }

        // Smoothly damp the float parameter so rolling/walking transitions look fluid
        animator.SetFloat(moveStateHash, targetMoveState, blendDampTime, Time.deltaTime);
    }

    // --- EVENT HANDLERS ---
    private void HandleIdle() => targetMoveState = 0f;
    private void HandleWalk() => targetMoveState = 1f;
    private void HandleRun() => targetMoveState = 2f;

    private void HandleTaunt()
    {
        targetMoveState = 0f;
        FireNetworkSafeTrigger(tauntHash);
    }

    private void HandleAttack()
    {
        targetMoveState = 0f;

        if (animator != null && attackVariationCount > 1)
        {
            int randomIndex = Random.Range(0, attackVariationCount);
            animator.SetInteger(attackIndexHash, randomIndex);
        }

        FireNetworkSafeTrigger(attackHash);
    }

    private void FireNetworkSafeTrigger(int triggerHash)
    {
        if (animator == null || !HasParameter(triggerHash, AnimatorControllerParameterType.Trigger)) return;

        bool isRemoteClient = (InstanceFinder.NetworkManager != null && InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted);
        if (isRemoteClient) return;

        bool isNetworked = (InstanceFinder.NetworkManager != null && (InstanceFinder.IsServerStarted || InstanceFinder.IsClientStarted));
        if (isNetworked && networkAnimator != null)
        {
            networkAnimator.SetTrigger(triggerHash);
        }
        else
        {
            animator.SetTrigger(triggerHash);
        }
    }

    private bool HasParameter(int paramHash, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash && param.type == type) return true;
        }
        return false;
    }
}