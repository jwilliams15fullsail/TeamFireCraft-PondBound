using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireflyWander : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 1.5f;
    public float changeDirectionTime = 2f;

    [Header("Flight Smoothing")]
    public float acceleration = 3f;

    [Header("Flying Height")]
    public float minHeight = 0.2f;
    public float maxHeight = 0.8f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float timer;
    private bool caught;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        startPosition = transform.position;
        caught = false;
        PickNewTarget();
    }

    private void FixedUpdate()
    {
        if (!IsServerInitialized || !IsSpawned || caught)
            return;

        timer -= Time.fixedDeltaTime;

        if (timer <= 0f ||
            Vector3.Distance(rb.position, targetPosition) < 0.2f)
        {
            PickNewTarget();
        }

        Vector3 direction =
            (targetPosition - rb.position).normalized;

        // Gradually adjust velocity for smoother flying.
        Vector3 desiredVelocity = direction * moveSpeed;

        rb.linearVelocity = Vector3.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        // Smoothly rotate toward the flight direction.
        Vector3 flatDirection = new Vector3(
            direction.x,
            0f,
            direction.z
        );

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(flatDirection);

            Quaternion newRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                5f * Time.fixedDeltaTime
            );

            rb.MoveRotation(newRotation);
        }
    }

    private void PickNewTarget()
    {
        Vector2 randomCircle =
            Random.insideUnitCircle * wanderRadius;

        targetPosition = startPosition + new Vector3(
            randomCircle.x,
            Random.Range(minHeight, maxHeight),
            randomCircle.y
        );

        timer = changeDirectionTime;
    }

    public void SetCaught(bool isCaught)
    {
        if (!IsServerInitialized)
            return;

        caught = isCaught;

        if (caught)
            rb.linearVelocity = Vector3.zero;
        else
            PickNewTarget();
    }
}
