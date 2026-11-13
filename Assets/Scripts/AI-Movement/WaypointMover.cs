using FishNet;
using System;
using System.Collections;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    // --- MODULAR PATROL EVENTS ---
    public event Action OnWalk;
    public event Action OnIdle;

    [SerializeField] private Waypoint waypoint;
    [SerializeField] private bool isBoomerang = false;
    [SerializeField] private float distanceThreshold = .5f;
    [SerializeField] public float moveSpeed = 3;
    [SerializeField] public float pauseTime = 2;
    [SerializeField] public float rotationSpeed = 5;

    // Waypoint target the object is moving towards
    private Transform currentWaypoint;
    private bool isWaiting = false;
    private Quaternion targetRotation;
    private Rigidbody rigidBody;
    private Coroutine patrolRoutine;
    private EnemyStun enemyStun;
    public bool canMove { get; set; } = true;
    public bool IsWaiting => isWaiting;
    public Transform CurrentWaypoint => currentWaypoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If we are connected to a network and are NOT the Host/Server, disable local patrol math
        if (InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted)
        {
            this.enabled = false;
            return;
        }

        // Get rigidBody component
        rigidBody = GetComponent<Rigidbody>();
        enemyStun = GetComponent<EnemyStun>();
        
        // Set initial position to the first waypoint
        currentWaypoint = waypoint.GetNextWaypoint(currentWaypoint);
        transform.position = currentWaypoint.position;

        // Set the next waypoint target
        currentWaypoint = waypoint.GetNextWaypoint(currentWaypoint);

        // Face first target on start
        Vector3 moveDirection = currentWaypoint.position - transform.position;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        patrolRoutine = StartCoroutine(PatrolLoop());
    }

    IEnumerator PatrolLoop()
    {
        while (true)
        {
            // If EnemyAI is taking over or the enemy is stunned, stop this loop
            if (!canMove || isWaiting || (enemyStun != null && enemyStun.IsStunned))
            {
                yield return null;
                continue;
            }

            // Broadcast that patrol movement has started
            OnWalk?.Invoke();

            // Flattened plane distance check to avoid distance threshold issues and stuck enemy patrols
            Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatTarget = new Vector3(currentWaypoint.position.x, 0, currentWaypoint.position.z);

            while (canMove && !isWaiting && Vector3.Distance(flatSelf, flatTarget) >= distanceThreshold && (enemyStun == null || !enemyStun.IsStunned))
            {
                Vector3 nextPosition = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

                if (rigidBody != null)      // If we have a rigidBody use rigidBody movement
                {
                    rigidBody.MovePosition(nextPosition);
                }
                else                // No rigidBody, use basic transform.position update
                {
                    transform.position = nextPosition;
                }

                // Recalculate flat coordinates every frame for accurate tracking
                flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
                flatTarget = new Vector3(currentWaypoint.position.x, 0, currentWaypoint.position.z);

                yield return null;
            }

            // If we reach the next waypoint
            if (canMove && !isWaiting)
            {
                // transform.position = currentWaypoint.position;
                if (isBoomerang) { currentWaypoint = waypoint.GetNextWaypointBoomerang(currentWaypoint); }
                else { currentWaypoint = waypoint.GetNextWaypoint(currentWaypoint); }

                yield return StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    public void ResumePatrol()
    {
        canMove = true;
        isWaiting = false;
        if (patrolRoutine != null) { StopCoroutine(patrolRoutine); }
        StopAllCoroutines();

        // Rotate back to our next waypoint
        Vector3 lookDirection = currentWaypoint.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(lookDirection);
        }

        patrolRoutine = StartCoroutine(PatrolLoop());
    }
    // -------------------------------

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        float elapsedTime = 0;

        // Broadcast that the enemy has stopped to wait
        OnIdle?.Invoke();

        // Perform a searching sweep at waypoint
        while (elapsedTime < pauseTime)
        {
            // Pause the waypoint timer and rotation while stunned.
            if (enemyStun != null && enemyStun.IsStunned)
            {
                yield return null;
                continue;
            }

            float sweepSpeed = Mathf.Sin(Time.time * 4f) * 45f;  // Oscillates between -45 to +45 degrees
            transform.Rotate(0, sweepSpeed * Time.deltaTime, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }


        Vector3 lookDirection = currentWaypoint.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(lookDirection);
        }

        elapsedTime = 0;
        bool isRotationFinished = false;

        while (elapsedTime < pauseTime && !isRotationFinished)
        {
            // Stop waypoint rotation while stunned.
            if (enemyStun != null && enemyStun.IsStunned)
            {
                yield return null;
                continue;
            }

            if (!canMove) { yield break; }      // Stop rotating if player is found and chase starts

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
            {
                isRotationFinished = true;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isWaiting = false;
    }
}
