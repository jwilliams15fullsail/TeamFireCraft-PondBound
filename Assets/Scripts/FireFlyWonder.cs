using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireflyWander : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 3f;
    public float changeDirectionTime = 2f;

    [Header("Flying Height")]
    public float minHeight = 0.5f;
    public float maxHeight = 2.5f;

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

        rb.linearVelocity = direction * moveSpeed;
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