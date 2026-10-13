using System;
using Unity.Netcode;
using UnityEngine;

public enum TongueTargetKind { Object, Food, Frog }

// Put this on anything a tongue may grab, on the same GameObject as its NetworkObject.
[RequireComponent(typeof(NetworkObject))]
public class TongueTarget : MonoBehaviour
{
    public TongueTargetKind kind = TongueTargetKind.Object;
    [Tooltip("Food only: hunger restored when eaten (SurvivalState will use this later).")]
    public float nutrition = 10f;

    // Host only: raised once when a frog swallows this food.
    public static event Action<FrogTongue, TongueTarget> Eaten;

    public NetworkObject NetObject { get; private set; }
    public Rigidbody Body { get; private set; }
    public FrogClimb Climb { get; private set; } // set when the target is a frog

    bool eaten;

    void Awake()
    {
        NetObject = GetComponent<NetworkObject>();
        Body = GetComponent<Rigidbody>();
        Climb = GetComponent<FrogClimb>();
    }

    public void Consume(FrogTongue eater)
    {
        if (eaten || !NetObject.IsSpawned)
            return; // can never be eaten twice
        eaten = true;
        Debug.Log($"{name} eaten by player {eater.OwnerClientId}");
        Eaten?.Invoke(eater, this);
        NetObject.Despawn();
    }
}
