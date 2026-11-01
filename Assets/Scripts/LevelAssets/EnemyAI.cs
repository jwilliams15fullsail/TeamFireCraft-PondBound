using UnityEngine;
using System.Collections;
using FishNet.Object;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : NetworkBehaviour
{
    public enum AIState { patrolling, chasing, searching, returning }

    [SerializeField] float spiderDamage = 10f;

    [Header("State Machine")]
    [SerializeField] AIState currentState = AIState.patrolling;

    [Header("FOV Settings")]
    [SerializeField] float viewRadius = 13f;
    [SerializeField] float viewAngle = 90f;
    [SerializeField] LayerMask targetMask;      // What the AI targets/responds to
    [SerializeField] LayerMask obstacleMask;    // Set to environment/obstacle layer

    [Header("Search Settings")]
    [SerializeField] float searchDuration = 8f; // How long the AI searches
    [SerializeField] float stopDistance = 0.7f;    // How far away from the target it stops moving
    private Vector3 lastPlayerPosition;
    private float searchTimer;

    [Header("AI Movement")]
    [SerializeField] float patrolSpeed = 2.8f;
    [SerializeField] float chaseSpeed = 4.5f;
    [SerializeField] float returnSpeed = 2.9f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float pauseTime = 2f;

    private Transform player;
    private WaypointMover pathing;
    private Rigidbody rigidBody;
    private EnemyStun enemyStun;

    /////   ANIMATION INTEGRATION   /////
    private Animator animator;
    private Animation animation;
    private static readonly int speedHash = Animator.StringToHash("speed");
    private static readonly int attackHash = Animator.StringToHash("attack");
    private static readonly int attackIndexHash = Animator.StringToHash("attackIndex");
    private bool isAttacking = false;
    private float stateLockTimer = 0f;  // Stops state flickering for animations
    private bool isTaunting = false;
    private bool hasTauntedThisChase = false; // Ensure spider only taunts once when it spots the player
    // -------------------------------

    /////   NETWORKING   /////
    // Track the last played animation name to prevent spamming network calls every frame
    private string lastNetworkedClip = "";
    // -----------------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathing = GetComponent<WaypointMover>();
        rigidBody = GetComponent<Rigidbody>();
        enemyStun = GetComponent<EnemyStun>();
        animator = GetComponent<Animator>();
        animation = GetComponent<Animation>();

        isTaunting = false;
        hasTauntedThisChase = false;

        // !!CRUCIAL FOR RIGIDBODY!! Prevents falling over from physics
        rigidBody.freezeRotation = true;
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        // NETWORK SAFETY: Only the Host/Server machine should process AI states and calculations
        if (!IsServerStarted) return;
        // -----------------------------------------

        // Do not chase, patrol, search, or attack while stunned.
        if (enemyStun != null && enemyStun.IsStunned)
            return;
        if (stateLockTimer > 0f) { stateLockTimer -= Time.deltaTime; }

        LookForPlayer();

        switch (currentState)
        {
            case AIState.patrolling:
                PerformPatrol();
                break;
            case AIState.chasing:
                PerformChase();
                break;
            case AIState.searching:
                PerformSearch();
                break;
            case AIState.returning:
                PerformReturn();
                break;
        }
        
    }

    void LookForPlayer()
    {
        // Collection of all targets within enemy FOV
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (targetsInRadius.Length > 0)
        {
            // First to enter enemy FOV is the target
            Transform target = targetsInRadius[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // STICKY TARGETING: if we are already chasing, don't lose player immediately upon exiting FOV
            bool isCloseEnoughToTrack = (currentState == AIState.chasing && distanceToTarget <= stopDistance + 3f);
            bool isInFOVCone = Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2;

            // Check if player is within our FOV
            if (isInFOVCone || isCloseEnoughToTrack)
            {
                // Fix Raycast collission with own body
                Vector3 rayOrigin = transform.position + (directionToTarget * 1f);
                // Check for obstacles with Raycast
                if (!Physics.Raycast(rayOrigin, directionToTarget, distanceToTarget, obstacleMask))
                {
                    player = target;        // Continuously update players last known position
                    lastPlayerPosition = player.position;
                    
                    if (currentState != AIState.chasing)
                    {
                        currentState = AIState.chasing;
                        pathing.canMove = false;    // Disable patrol script when chasing
                    }
                    return;
                }
            }
        }
        // If the player was being chased and was lost, start search
        if (currentState == AIState.chasing && stateLockTimer <= 0)
        {
            player = null;
            searchTimer = searchDuration;   // Set the countdown timer
            currentState = AIState.searching;
            stateLockTimer = 0.5f;          // Prevent immediately switching back to chase state

            hasTauntedThisChase = false;    // Player escaped FOV, can taunt again if it finds the player
        }
    }

    /////   STATE BEHAVIOURS   /////
    void PerformPatrol()
    {
        pathing.moveSpeed = patrolSpeed;
        pathing.rotationSpeed = rotationSpeed;
        pathing.pauseTime = pauseTime;
        pathing.canMove = true;     // Hand patrol movement back to WaypointMover script

        if (animator != null)       // Assign proper animation based moving along path or pausing at a waypoint
        {
            float currentSpeed = (pathing.canMove && !pathing.isWaiting) ? patrolSpeed : 0f;
            animator.SetFloat(speedHash, currentSpeed);
        }
    }

    void PerformChase()
    {
        if (player == null) { return; }

        // Trigger taunt coroutine before chase starts
        if (!hasTauntedThisChase && !isTaunting)
        {
            StartCoroutine(PerformTaunt());
            return;
        }
        if (isTaunting)
        {
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.deltaTime);
            }
            return; // Skip the rest of the chase movement code below until taunt completes
        }
        // --------------------------------------------

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If player in attack range, stop chasing animation, perform attack animations
        if (distanceToPlayer <= stopDistance + 0.5f)
        {
            if (animator != null) { animator.SetFloat(speedHash, 0f); }
            else if (animation != null && !animation.IsPlaying("attack1") && !animation.IsPlaying("attack2") && !animation.IsPlaying("idle"))
            { animation.CrossFade("idle", 0.2f); } // Legacy spider animation

            // Trigger random attack animation once
            if (!isAttacking)
            {
                isAttacking = true;
                if (animator != null)
                {
                    // Choose random attack between 0 and 1
                    int randomAttack = Random.Range(0, 2);
                    animator.SetInteger(attackIndexHash, randomAttack);
                    animator.SetTrigger(attackHash);
                }
                else if (animation != null)     // Legacy spider animation
                {
                    string attackClip = (Random.Range(0, 2) == 0) ? "attack1" : "attack2";
                    animation.CrossFade(attackClip, 0.1f);
                }

                FrogStats stats = player.GetComponent<FrogStats>();
                if (stats != null)
                {
                    stats.Damage(spiderDamage);
                }
                Invoke(nameof(ResetAttackCooldown), 1.5f);
            }
            // Continue facing player while attacking
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.deltaTime);
            }
        }
        else
            if (!isAttacking)
            {
                {
                    MoveTowards(player.position, chaseSpeed, stopDistance);
                }
            }
    }

    void PerformSearch()
    {
        // Calculate distance ignoring height (avoid locked state from jittering physics)
        Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z);

        // Lost sight of player, move to players last known position
        if (Vector3.Distance(flatSelf, flatTarget) > 0.5f)
        {
            MoveTowards(lastPlayerPosition, patrolSpeed, 0f);
        }
        // At last known position but no player, start searching
        else
        {
            if (animator != null) { animator.SetFloat(speedHash, 0f); }
            else if (animation != null) { animation.CrossFade("idle"); } // Legacy spider animation

            searchTimer -= Time.deltaTime;

            // Sweep back and forth while searching
            float sweepSpeed = Mathf.Sin(Time.time * 4f) * 45f;  // Oscillates between -45 to +45 degrees
            transform.Rotate(0, sweepSpeed * Time.deltaTime, 0);

            if (searchTimer <= 0)       // End search after a time and return to patrol
            {
                currentState = AIState.returning;
            }
        }
    }

    void PerformReturn()
    {
        Vector3 targetWaypointPosition = pathing.CurrentWaypoint.position;
        MoveTowards(targetWaypointPosition, returnSpeed, 0f);

        Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(targetWaypointPosition.x, 0, targetWaypointPosition.z);

        if (Vector3.Distance(flatSelf, flatTarget) < 0.4f)
        {
            pathing.ResumePatrol();
            currentState = AIState.patrolling;
        }
    }

    void MoveTowards(Vector3 targetPosition, float moveSpeed, float offsetDistance)
    {
        // Tell animator we're moving
        if (animator != null) { animator.SetFloat(speedHash, moveSpeed); }
        else if (animation != null)     // Legacy spider animation
        { 
            string clipName = (currentState == AIState.chasing) ? "run" : "walk";
            if (!animation.IsPlaying(clipName))
            {
                animation.CrossFade(clipName);
                animation[clipName].speed = 1f;     // Adjust speed of animation
            }
        }

        // Keep calculations on a flat 2D plane to avoid floating/burying model
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        if (direction.magnitude > offsetDistance)
        {
            Vector3 destination = transform.position + direction.normalized * (direction.magnitude - offsetDistance);

            Vector3 nextPosition = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            rigidBody.MovePosition(nextPosition);

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (animator != null) { animator.SetFloat(speedHash, 0f); }
            else if (animation != null) { animation.CrossFade("idle"); } // Legacy spider animation
        }
    }

    IEnumerator PerformTaunt()
    {
        isTaunting = true;
        hasTauntedThisChase = true;

        if (animation != null && !animation.IsPlaying("taunt"))
        {
            animation.CrossFade("taunt", .15f);
        }
        yield return new WaitForSeconds(1.15f);

        isTaunting = false;
    }

    void ResetAttackCooldown()
    {
        isAttacking = false;
    }
}
