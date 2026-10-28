using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { patrolling, chasing, searching, returning }

    [Header("State Machine")]
    [SerializeField] AIState currentState = AIState.patrolling;

    [Header("FOV Settings")]
    [SerializeField] float viewRadius = 10f;
    [SerializeField] float viewAngle = 90f;
    [SerializeField] LayerMask targetMask;      // What the AI targets/responds to
    [SerializeField] LayerMask obstacleMask;    // Set to environment/obstacle layer

    [Header("Search Settings")]
    [SerializeField] float searchDuration = 8f; // How long the AI searches
    private Vector3 lastPlayerPosition;
    private float searchTimer;

    [Header("AI Movement")]
    [SerializeField] float patrolSpeed = 3f;
    [SerializeField] float chaseSpeed = 5.5f;
    [SerializeField] float returnSpeed = 3f;
    [SerializeField] float rotationSpeed = 10f;

    private Transform player;
    private WaypointMover pathing;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathing = GetComponent<WaypointMover>();
    }

    // Update is called once per frame
    void Update()
    {
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
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (targetsInRadius.Length > 0)
        {
            Transform target = targetsInRadius[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Check if player is within our FOV
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle/2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // Check for obstacles with Raycast
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
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
        if (currentState == AIState.chasing)
        {
            player = null;
            searchTimer = searchDuration;   // Set the countdown timer
            currentState = AIState.searching;
        }
    }

    /////   STATE BEHAVIOURS   /////
    void PerformPatrol()
    {
        pathing.moveSpeed = patrolSpeed;
        pathing.canMove = true;     // Hand patrol movement back to WaypointMover script
    }

    void PerformChase()
    {
        if (player == null) { return; }
        MoveTowards(player.position, chaseSpeed);
    }

    void PerformSearch()
    {
        // Lost sight of player, move to players last known position
        if (Vector3.Distance(transform.position, lastPlayerPosition) > 0.4f)
        {
            MoveTowards(lastPlayerPosition, patrolSpeed);
        }
        // At last known position but no player, start searching
        else
        {
            searchTimer -= Time.deltaTime;

            // Rotate back and forth while searching
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
        MoveTowards(targetWaypointPosition, returnSpeed);

        // Once back on patrol path, reset mover script and resume
        if (Vector3.Distance(transform.position, targetWaypointPosition) < 0.2f)
        {
            pathing.ResumePatrol();
            currentState = AIState.patrolling;
        }
    }

    void MoveTowards(Vector3 targetPosition, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
