using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class ObjectiveSequence : NetworkBehaviour
{
    [SerializeField] Transform[] objectives;
    readonly SyncVar<int> currentIndex = new(-1);

    public Transform CurrentTarget
    {
        get
        {
            int index = currentIndex.Value;
            return objectives != null && index >= 0 && index < objectives.Length
                ? objectives[index]
                : null;
        }
    }

    public bool IsComplete =>
        objectives == null || currentIndex.Value >= objectives.Length;

    public override void OnStartServer()
    {
        base.OnStartServer();

        currentIndex.Value = objectives != null && objectives.Length > 0
            ? 0
            : -1;
    }

    [Server]
    public void CompleteCurrentObjective()
    {
        if (IsComplete)
            return;

        currentIndex.Value++;
    }
}
