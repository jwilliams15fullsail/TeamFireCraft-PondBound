using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Range(0f, 3f)] [SerializeField] private float waypointSize;
    private bool isMovingForward = true;

    private void OnDrawGizmos()
    {
        foreach (Transform waypoint in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(waypoint.position, 1f);
        }

        Gizmos.color = Color.red;
        for (int child = 0; child < transform.childCount - 1; child++)
        {
            Gizmos.DrawLine(transform.GetChild(child).position, transform.GetChild(child + 1).position);
        }
    }

    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        if (currentWaypoint == null)
        {
            return transform.GetChild(0);
        }
        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1)
        {
            return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
        else
        {
            return transform.GetChild(0);
        }
    }

    public Transform GetNextWaypointBoomerang(Transform currentWaypoint)
    {
        if (currentWaypoint == null)
        {
            isMovingForward = true;
            return transform.GetChild(0);
        }

        int currentIndex = currentWaypoint.GetSiblingIndex();
        if (isMovingForward)
        {
            if (currentIndex < transform.childCount - 1)
            {
                return transform.GetChild(currentIndex + 1);
            }
            else
            {
                isMovingForward = false;
                return transform.GetChild(currentIndex - 1);
            }
        }
        else
        {
            if (currentIndex > 0)
            {
                return transform.GetChild(currentIndex - 1);
            }
            else
            {
                isMovingForward = true;
                return transform.GetChild(currentIndex + 1);
            }
        }
    }
}
