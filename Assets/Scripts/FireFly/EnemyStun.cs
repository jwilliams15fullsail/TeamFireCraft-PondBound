
using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class EnemyStun : NetworkBehaviour
{
    public event Action OnStunStart;
    public event Action OnStunEnd;

    [Header("Stun Settings")]
    [SerializeField] private float defaultStunDuration = 2f;

    // Synced across all clients so EnemyStunAura and animations work in multiplayer
    private readonly SyncVar<bool> syncedIsStunned = new();

    private EnemyAI enemyAI;
    private WaypointMover waypointMover;
    private Rigidbody rb;

    private float stunTimer;
    private bool wasWaypointMoverEnabled;

    public bool IsStunned => syncedIsStunned.Value || stunTimer > 0f;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        waypointMover = GetComponent<WaypointMover>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Only the Host/Server (or Offline player) counts down the stun timer
        bool isRemoteClient = (InstanceFinder.NetworkManager != null && InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted);
        if (isRemoteClient) return;

        if (stunTimer <= 0f) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            stunTimer = 0f;
            syncedIsStunned.Value = false;

            if (enemyAI != null)
                enemyAI.enabled = true;

            // Only re-enable WaypointMover if it was actually enabled before the stun!
            if (waypointMover != null && wasWaypointMoverEnabled)
                waypointMover.enabled = true;

            OnStunEnd?.Invoke();
        }
    }

    public void ApplyStun(float duration)
    {
        bool isRemoteClient = (InstanceFinder.NetworkManager != null && InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted);
        if (isRemoteClient) return;

        bool alreadyStunned = IsStunned;
        stunTimer = Mathf.Max(duration, 0.01f);
        syncedIsStunned.Value = true;

        if (!alreadyStunned)
        {
            // Remember whether this enemy uses WaypointMover or Random Wander
            wasWaypointMoverEnabled = (waypointMover != null && waypointMover.enabled);

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

            OnStunStart?.Invoke();
        }
    }

    public void ApplyDefaultStun()
    {
        ApplyStun(defaultStunDuration);
    }
}