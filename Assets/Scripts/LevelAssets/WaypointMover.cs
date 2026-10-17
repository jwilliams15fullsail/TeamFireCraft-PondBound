using System.Collections;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [SerializeField] private Waypoint waypoint;
    [SerializeField] bool isBoomerang = false;
    [SerializeField] private float distanceThreshold = .1f;
    [Range(1f, 5f)] [SerializeField] private float moveSpeed;
    [Range(.5f, 10f)] [SerializeField] private float pauseTime;
    [Range(1f, 15f)] [SerializeField] private float rotationSpeed;

    // Waypoint target the object is moving towards
    private Transform currentWaypoint;
    private bool isWaiting = false;
    private Quaternion targetRotation;

    /////   FOR AI INTEGRATION   /////
    public bool canMove { get; set; } = true;
    public Transform CurrentWaypoint => currentWaypoint;
    // -------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && !isWaiting)     // <- Modifid for AI INTEGRATION //
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentWaypoint.position) < distanceThreshold)
            {
                transform.position = currentWaypoint.position;
                if (isBoomerang) { currentWaypoint = waypoint.GetNextWaypointBoomerang(currentWaypoint); }
                else { currentWaypoint = waypoint.GetNextWaypoint(currentWaypoint); }
                StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    /////   FOR AI INTEGRATION   /////
    public void ResumePatrol()
    {
        canMove = true;
        isWaiting = false;
        StopAllCoroutines();

        // Rotate back to our next waypoint
        Vector3 lookDirection = currentWaypoint.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(lookDirection);
        }
    }
    // -------------------------------

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        Vector3 lookDirection = currentWaypoint.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(lookDirection);
        }

        float elapsedTime = 0;
        bool isRotationFinished = false;
        while (elapsedTime < pauseTime || !isRotationFinished)
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
