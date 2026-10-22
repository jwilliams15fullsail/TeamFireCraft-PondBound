using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(NetworkObject))]
public class WaterDrop : NetworkBehaviour
{
    float hydrationAmount;
    float maxTravelDistance;
    Vector3 spawnPosition;
    float maxTravelDistanceSqr;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        spawnPosition = transform.position;
        maxTravelDistanceSqr = maxTravelDistance * maxTravelDistance;
    }
    private void Update()
    {
        if (!IsServer) return;
        float distanceSqr = (transform.position - spawnPosition).sqrMagnitude;

        if (distanceSqr > maxTravelDistanceSqr)
        {
            DestroyDrop();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        FrogStats frog = other.GetComponentInParent<FrogStats>();
        if (frog != null)
        {
            frog.Restore(0f, 0f, hydrationAmount);
        }
        DestroyDrop();
    }
    void DestroyDrop()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
        else
            Destroy(gameObject);
    }
    public void Initialize(float hydroAmount, float distance)
    {
        hydrationAmount = hydroAmount;
        maxTravelDistance = distance;
    }
}
