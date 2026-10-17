using System.Collections;
using UnityEngine;

public class WaypointMoverBoomerang : MonoBehaviour
{
    [SerializeField] private Waypoint waypoint;
    [SerializeField] private float distanceThreshold = .1f;
    [Range(1f, 5f)][SerializeField] private float moveSpeed;
    [Range(.5f, 10f)][SerializeField] private float pauseTime;
    [Range(1f, 15f)][SerializeField] private float rotationSpeed;

    // Waypoint target the object is moving towards
    private Transform currentWaypoint;
    private bool isWaiting = false;
    private Quaternion targetRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set initial position to the first waypoint
        currentWaypoint = waypoint.GetNextWaypointBoomerang(currentWaypoint);
        transform.position = currentWaypoint.position;

        // Set the next waypoint target
        currentWaypoint = waypoint.GetNextWaypointBoomerang(currentWaypoint);

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
        if (!isWaiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentWaypoint.position) < distanceThreshold)
            {
                transform.position = currentWaypoint.position;
                currentWaypoint = waypoint.GetNextWaypointBoomerang(currentWaypoint);
                StartCoroutine(WaitAtWaypoint());
            }
        }
    }

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
