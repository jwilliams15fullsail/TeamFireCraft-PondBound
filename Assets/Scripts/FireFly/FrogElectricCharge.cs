using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class FrogElectricCharge : NetworkBehaviour
{
    [Header("Electric Tongue")]
    [SerializeField] private float defaultStunDuration = 2f;

    // Synchronize the charge so clients can display electricity.
    public readonly SyncVar<bool> HasCharge = new();

    private float chargedStunDuration;

    private void OnEnable()
    {
        TongueTarget.Eaten += OnFoodEaten;
    }

    private void OnDisable()
    {
        TongueTarget.Eaten -= OnFoodEaten;
    }

    private void OnFoodEaten(FrogTongue eater, TongueTarget food)
    {
        // Only the server grants electric charges.
        if (!IsServerInitialized || !IsSpawned)
            return;

        // Only grant electricity to the frog that ate the food.
        if (eater.gameObject != gameObject)
            return;

        // Normal food does not grant electricity.
        if (!food.TryGetComponent(out ElectricFirefly electricFirefly))
            return;

        GrantCharge(electricFirefly.stunDuration);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        HasCharge.Value = false;
        chargedStunDuration = defaultStunDuration;
    }

    [Server]
    public void GrantCharge(float stunDuration)
    {
        HasCharge.Value = true;
        chargedStunDuration = Mathf.Max(0.1f, stunDuration);

        Debug.Log($"{name} gained an electric tongue charge!");
    }

    [Server]
    public bool TryConsumeCharge(out float stunDuration)
    {
        stunDuration = 0f;

        if (!HasCharge.Value)
            return false;

        stunDuration = chargedStunDuration;
        HasCharge.Value = false;

        Debug.Log($"{name} used an electric tongue charge!");

        return true;
    }
}
