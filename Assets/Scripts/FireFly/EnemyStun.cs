
using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float defaultStunDuration = 2f;

    private EnemyAI enemyAI;
    private WaypointMover waypointMover;
    private Rigidbody rb;

    private float stunTimer;

    public bool IsStunned => stunTimer > 0f;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        waypointMover = GetComponent<WaypointMover>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsStunned)
            return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            stunTimer = 0f;

            if (enemyAI != null)
                enemyAI.enabled = true;

            if (waypointMover != null)
                waypointMover.enabled = true;
        }
    }

    public void ApplyStun(float duration)
    {
        stunTimer = Mathf.Max(duration, 0.01f);

        if (waypointMover != null)
        {
            waypointMover.canMove = false;
            waypointMover.enabled = false;
        }

        if (enemyAI != null)
            enemyAI.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ApplyDefaultStun()
    {
        ApplyStun(defaultStunDuration);
    }
}
