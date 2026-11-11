using System;
using UnityEngine;
using System.Collections;
using FishNet.Object;
using FishNet;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : NetworkBehaviour
{
    public enum AIState { patrolling, chasing, searching, returning }

    // --- MODULAR AI EVENTS ---
    public event Action OnIdle;
    public event Action OnWalk;
    public event Action OnRun;
    public event Action OnTaunt;
    public event Action OnAttack;

    [Header("Enemy Stats")]
    [SerializeField] float spiderDamage = 10f;
    [SerializeField] float spiderAttackCooldown = 1.3f;

    [Header("State Machine")]
    [SerializeField] private AIState currentState = AIState.patrolling;

    [Header("Random Wander Settings (Fallback if no WaypointMover)")]
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float wanderPauseTime = 3f;
    private Vector3 startOrigin;
    private Vector3 currentWanderTarget;
    private float wanderWaitTimer = 0f;
    private bool isWaitingAtWanderPoint = false;

    [Header("Ledge Detection Settings")]
    [SerializeField] private LayerMask groundMask;          // Set to your floor/terrain layers
    [SerializeField] private float ledgeLookAhead = 1.2f;   // How far in front of the spider to check
    [SerializeField] private float maxDropHeight = 2.0f;    // How deep a drop counts as a ledge

    [Header("FOV Settings")]
    [SerializeField] private float viewRadius = 13f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;      // What the AI targets/responds to
    [SerializeField] private LayerMask obstacleMask;    // Set to environment/obstacle layer

    [Header("Search & Taunt Settings")]
    [SerializeField] private float searchDuration = 8f; // How long the AI searches
    [SerializeField] private float stopDistance = 0.7f;    // How far away from the target it stops moving
    [SerializeField] private float tauntDuration = 1.5f;
    [SerializeField] private bool hasTauntAnimation = true; // Toggle off for future assets without taunts!

    private Vector3 lastPlayerPosition;
    private float searchTimer;

    [Header("AI Movement")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float returnSpeed = 2.8f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float pauseTime = 2f;

    private Transform player;
    private WaypointMover pathing;
    private Rigidbody rigidBody;
    private EnemyStun enemyStun;

    /////   ANIMATION INTEGRATION   /////
    private bool isAttacking = false;
    private float stateLockTimer = 0f;  // Stops state flickering for animations
    private bool isTaunting = false;
    private bool hasTauntedThisChase = false; // Ensure spider only taunts once when it spots the player
    // -------------------------------

    // Helper property to check if we have an active WaypointMover
    private bool HasValidPathing => pathing != null && pathing.enabled;

    void Start()
    {
        // Only the Host/Server runs AI vision, state switching, and damage checks
        if (InstanceFinder.NetworkManager != null && InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted) return;

        pathing = GetComponent<WaypointMover>();
        rigidBody = GetComponent<Rigidbody>();
        enemyStun = GetComponent<EnemyStun>();

        if (rigidBody != null)
        {
            rigidBody.freezeRotation = true;
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
        }
        // Record spawn position for sphere wandering and returning
        startOrigin = transform.position;
        PickNewWanderPoint();

        isTaunting = false;
        hasTauntedThisChase = false;
    }

    // Update is called once per frame
    void Update()
    {
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

            if (currentState == AIState.chasing) isInFOVCone = true;
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
                    
                    if (currentState != AIState.chasing && stateLockTimer <= 0)
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
            isTaunting = false;
            hasTauntedThisChase = false;    // Player escaped FOV, can taunt again if it finds the player
        }
    }

    /////   STATE BEHAVIOURS   /////
    void PerformPatrol()
    {
        // Use WaypointMover script if attached
        if (HasValidPathing)
        {
            pathing.moveSpeed = patrolSpeed;
            pathing.rotationSpeed = rotationSpeed;
            pathing.pauseTime = pauseTime;
            pathing.canMove = true;     // Hand patrol movement back to WaypointMover script
        }
        // Wander in a sphereical radius randomly
        if (isWaitingAtWanderPoint)
        {
            OnIdle?.Invoke();
            wanderWaitTimer -= Time.deltaTime;

            // Gentle look-around sweep while pausing at random point
            float sweepSpeed = Mathf.Sin(Time.time * 3f) * 35f;
            transform.Rotate(0, sweepSpeed * Time.deltaTime, 0);

            if (wanderWaitTimer <= 0f)
            {
                isWaitingAtWanderPoint = false;
                PickNewWanderPoint();
            }
            return;
        }
        Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(currentWanderTarget.x, 0, currentWanderTarget.z);
        Vector3 moveDirection = (flatTarget - flatSelf).normalized;

        // LEDGE CHECK ON WANDER: Pick a new point immediately if approaching a cliff
        if (IsLedgeAhead(moveDirection))
        {
            PickNewWanderPoint();
            return;
        }

        if (Vector3.Distance(flatSelf, flatTarget) > 0.5f)
        {
            MoveTowards(currentWanderTarget, patrolSpeed, 0f, isRunning: false);
        }
        else
        {
            isWaitingAtWanderPoint = true;
            wanderWaitTimer = wanderPauseTime;
        }
    }

    void PickNewWanderPoint()
    {
        // Try up to 5 times to find a random point inside the sphere that isn't blocked by a wall
        for (int i = 0; i < 5; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * wanderRadius;
            Vector3 candidatePoint = new Vector3(
                startOrigin.x + randomCircle.x,
                transform.position.y,
                startOrigin.z + randomCircle.y
            );

            Vector3 dirToCandidate = candidatePoint - transform.position;
            float distToCandidate = dirToCandidate.magnitude;

            // Raycast check ensures the spider doesn't pick a wander spot behind a solid wall
            if (distToCandidate > 0.5f && !Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToCandidate.normalized, distToCandidate, obstacleMask))
            {
                currentWanderTarget = candidatePoint;
                return;
            }
        }

        // Fallback to returning directly to spawn origin if surrounded by obstacles
        currentWanderTarget = startOrigin;
    }

    void PerformChase()
    {
        if (player == null) { return; }

        // Trigger taunt coroutine before chase starts
        if (hasTauntAnimation && !hasTauntedThisChase && !isTaunting)
        {
            StartCoroutine(PerformTaunt());
            return;
        }
        if (isTaunting)
        {
            LookAtTarget(player.position);
            return;
        }

        Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatPlayer = new Vector3(player.position.x, 0, player.position.z);
        float distanceToPlayer = Vector3.Distance(flatSelf, flatPlayer);

        // If player in attack range, stop chasing animation, perform attack animations
        if (distanceToPlayer <= stopDistance + 0.5f)
        {
            // Trigger random attack animation once
            if (!isAttacking)
            {
                OnIdle?.Invoke();   // Broadcast idle stance between attacks
                isAttacking = true;
                OnAttack?.Invoke(); // Broadcast attack trigger
                Invoke(nameof(ResetAttackCooldown), 1.2f);

                FrogStats stats = player.GetComponent<FrogStats>();
                if (stats != null)
                {
                    stats.Damage(spiderDamage);
                }
                Invoke(nameof(ResetAttackCooldown), spiderAttackCooldown);
            }
            // Continue facing player while attacking
            LookAtTarget(player.position);
        }
        else if (!isAttacking)
        {
            MoveTowards(player.position, chaseSpeed, stopDistance, isRunning: true);
        }
    }

    void PerformSearch()
    {
        // Calculate distance ignoring height (avoid locked state from jittering physics)
        Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z);

        // Lost sight of player, move to players last known position
        if (Vector3.Distance(flatSelf, flatTarget) > 0.6f)
        {
            MoveTowards(lastPlayerPosition, patrolSpeed, 0f, isRunning: false);
        }
        // At last known position but no player, start searching
        else
        {
            OnIdle?.Invoke();
            searchTimer -= Time.deltaTime;

            // Sweep back and forth while searching
            float sweepSpeed = Mathf.Sin(Time.time * 4f) * 45f;  // Oscillates between -45 to +45 degrees
            transform.Rotate(0, sweepSpeed * Time.deltaTime, 0);

            if (searchTimer <= 0)       // End search after a time and return to patrol
            {
                currentState = AIState.returning;
                stateLockTimer = 0.5f;
            }
        }
    }

    void PerformReturn()
    {
        Vector3 returnDestination = HasValidPathing ? pathing.CurrentWaypoint.position : startOrigin;
        MoveTowards(returnDestination, returnSpeed, 0f, isRunning: false);

        Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(returnDestination.x, 0, returnDestination.z);

        if (Vector3.Distance(flatSelf, flatTarget) < 0.5f)
        {
            if (HasValidPathing)
            {
                pathing.ResumePatrol();
            }
            else
            {
                isWaitingAtWanderPoint = false;
                PickNewWanderPoint();
            }
            currentState = AIState.patrolling;
        }
    }

    void MoveTowards(Vector3 targetPosition, float moveSpeed, float offsetDistance, bool isRunning)
    {
        // Keep calculations on a flat 2D plane to avoid floating/burying model
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        // LEDGE CHECK ON CHASE/SEARCH/RETURN: Stop at the edge instead of falling
        if (IsLedgeAhead(direction.normalized))
        {
            OnIdle?.Invoke();
            LookAtTarget(targetPosition);
            return;
        }

        if (isRunning) OnRun?.Invoke();
        else OnWalk?.Invoke();

        if (direction.magnitude > offsetDistance)
        {
            Vector3 destination = transform.position + direction.normalized * (direction.magnitude - offsetDistance);
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            if (rigidBody != null) { rigidBody.MovePosition(nextPosition); }
            else transform.position = nextPosition;

            LookAtTarget(targetPosition);
        }
    }

    void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 lookDir = (targetPosition - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * Time.deltaTime);
        }
    }

    bool IsLedgeAhead(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero) return false;

        // Start slightly above the spider's pivot and ahead in the direction of movement
        Vector3 checkOrigin = transform.position + (Vector3.up * 0.5f) + (moveDirection.normalized * ledgeLookAhead);

        // If the downward ray does NOT hit ground within maxDropHeight, there is a ledge ahead
        bool hasGround = Physics.Raycast(checkOrigin, Vector3.down, maxDropHeight + 0.5f, groundMask);
        return !hasGround;
    }

    void ResetAttackCooldown()
    {
        isAttacking = false;
    }

    IEnumerator PerformTaunt()
    {
        isTaunting = true;
        hasTauntedThisChase = true;
        OnTaunt?.Invoke(); // Broadcast taunt trigger

        yield return new WaitForSeconds(tauntDuration);

        isTaunting = false;
    }

    // Visualize aspects of the selected enemies movements/vision
    private void OnDrawGizmosSelected()
    {
        // Search FOV
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        // Wandering radius
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? startOrigin : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
        // Ledge check
        Vector3 rayStart = transform.position + (Vector3.up * 0.5f) + (transform.forward * ledgeLookAhead);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * (maxDropHeight + 0.5f));
    }
}
