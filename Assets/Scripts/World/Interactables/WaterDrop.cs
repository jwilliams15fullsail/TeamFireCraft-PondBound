using UnityEngine;
using FishNet.Object;
[RequireComponent(typeof(NetworkObject))]
public class WaterDrop : NetworkBehaviour
{
    float hydrationAmount;
    float maxTravelDistance;
    Vector3 spawnPosition;
    float maxTravelDistanceSqr;
    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnPosition = transform.position;
        maxTravelDistanceSqr = maxTravelDistance * maxTravelDistance;
    }
    private void Update()
    {
        if (!IsServerInitialized) return;
        float distanceSqr = (transform.position - spawnPosition).sqrMagnitude;

        if (distanceSqr > maxTravelDistanceSqr)
        {
            DestroyDrop();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        FrogStats frog = other.GetComponentInParent<FrogStats>();
        if (frog != null)
        {
            frog.Restore(0f, 0f, hydrationAmount);
        }
        DestroyDrop();
    }
    void DestroyDrop()
    {
        if (IsSpawned)
            Despawn();
        else
            Destroy(gameObject);
    }
    public void Initialize(float hydroAmount, float distance)
    {
        hydrationAmount = hydroAmount;
        maxTravelDistance = distance;
    }
}