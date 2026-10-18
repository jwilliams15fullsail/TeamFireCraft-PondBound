using UnityEngine;
using System;

public class CheckpointManager : MonoBehaviour
{
    // A static event that passes the specific checkpoint that was just activated
    public static event Action<CheckpointManager> OnCheckpointActivated;

    private bool isActivated = false;
    private Color originalColor;
    private Renderer flagRenderer;

    private void Awake()
    {
        flagRenderer = GetComponent<Renderer>();
        if (flagRenderer != null)
        {
            originalColor = flagRenderer.material.color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Frog") && !isActivated)
        {
            isActivated = true;

            // Optional color change of checkpoint
            if (flagRenderer != null)
            {
                flagRenderer.material.color = Color.green;
            }

            // Fire the event. The ?. ensures it only runs if something is listening
            OnCheckpointActivated?.Invoke(this);
        }
    }

    public void DeactivateCheckpoint()
    {
        isActivated = false;
        if (flagRenderer != null)
        {
            flagRenderer.material.color = originalColor;
        }
    }
}
