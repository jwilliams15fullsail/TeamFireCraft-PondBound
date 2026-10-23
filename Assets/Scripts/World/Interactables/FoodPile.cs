using FishNet.Object;
using UnityEngine;

public class FoodPile : NetworkBehaviour
{
    [SerializeField] NetworkObject foodPrefab;
    [SerializeField] int foodRemaining = 20;
    public void OnTongueHit(FrogTongue tongue, Vector3 hitpoint)
    {
        Debug.Log("FoodPile hit!");
        if (!IsServerInitialized) return;
        if (foodRemaining <= 0) return;
        foodRemaining--;

        NetworkObject food = Instantiate(foodPrefab, hitpoint, Quaternion.identity);

        Spawn(food, null, gameObject.scene);
        TongueTarget target = food.GetComponent<TongueTarget>();
        tongue.ForceAttach(target, hitpoint);
    }

}