using System.Collections;
using Unity.Physics;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [SerializeField] private Waypoint waypoint;
    [SerializeField] bool isBoomerang = false;
    [SerializeField] private float distanceThreshold = .5f;
    [SerializeField] public float moveSpeed = 2;
    [SerializeField] public float pauseTime = 2;
    [SerializeField] public float rotationSpeed = 5;

    // Waypoint target the object is moving towards
    private Transform currentWaypoint;
    public bool isWaiting = false;
    private Quaternion targetRotation;
    private Rigidbody rigidBody;
    private Coroutine patrolRoutine;

    /////   FOR ANIMATION INTEGRATION   /////
    private Animator animator;
    private Animation animation;    // Legacy spider animation
    private static readonly int speedHash = Animator.StringToHash("speed");
    // -------------------------------

    /////   FOR AI INTEGRATION   /////
    public bool canMove { get; set; } = true;
    public Transform CurrentWaypoint => currentWaypoint;
    // -------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get rigidBody component
        rigidBody = GetComponent<Rigidbody>();
        // Get the animator component
        animator = GetComponent<Animator>();
        animation = GetComponent<Animation>();  // Legacy spider animation

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

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator PatrolLoop()
    {
        while (true)
        {
            // If EnemyAI is taking over, stop this loop
            if (!canMove || isWaiting)
            {
                yield return null;
                continue;
            }

            // Flattened plane distance check to avoid distance threshold issues and stuck enemy patrols
            Vector3 flatSelf = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatTarget = new Vector3(currentWaypoint.position.x, 0, currentWaypoint.position.z);

            // Trigger walking animation at the start of the coroutine
            if (animator != null) { animator.SetFloat(speedHash, moveSpeed); }
            else if (animation != null && !animation.IsPlaying("walk"))  // Legacy spider animation
            {
                animation.CrossFade("walk");
                animation["walk"].speed = 1f;   // Adjust walk animation speed
            }

            while (canMove && !isWaiting && Vector3.Distance(flatSelf, flatTarget) >= distanceThreshold)
            {
                Vector3 nextPosition = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);
                if (rigidBody != null)      // If we have a rigidBody use rigidBody movement
                {
                    rigidBody.MovePosition(nextPosition);
                }
                else    // No rigidBody, use basic transform.position update
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
            else    // Waiting or can't move, stop the animation
            {
                if (animator != null && !isWaiting) { animator.SetFloat(speedHash, 0f); }
                else if (animation != null) { animation.CrossFade("idle"); } // Legacy spider animation
            }   
        }
    }

    /////   FOR AI INTEGRATION   /////
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
        float elapsedTime;

        // Tell animator we're waiting (idle)
        if (animator != null) { animator.SetFloat(speedHash, 0f); }
        else if (animation != null) { animation.CrossFade("idle", 0.2f); } // Legacy spider animation

        // Perform a searching sweep at waypoint
        elapsedTime = 0;
        while (elapsedTime < pauseTime)
        {
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
            if (!canMove) { yield break; }      // AI INTEGRATION: Stop rotating if player is found and chase starts

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
