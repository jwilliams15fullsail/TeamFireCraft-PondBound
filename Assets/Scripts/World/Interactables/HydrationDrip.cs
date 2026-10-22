using GameKit.Dependencies.Utilities.Types;
using Unity.Netcode;
using UnityEngine;

public class HydrationDrip : NetworkBehaviour
{
    [SerializeField] WaterDrop dropPrefab;
    [SerializeField] float hydrationAmount = 5f;
    [SerializeField] float maxTravelDistance = 20f;
    [SerializeField] float dropRate = 0.5f;

    float timer;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        timer += Time.deltaTime;

        if(timer >= dropRate)
        {
            timer = 0f;
            SpawnDrop();
        }
    }
    void SpawnDrop()
    {
        WaterDrop drop = Instantiate( dropPrefab, transform.position, Quaternion.identity);
        drop.Initialize(hydrationAmount, maxTravelDistance);

        drop.GetComponent<NetworkObject>().Spawn();
    }
}
