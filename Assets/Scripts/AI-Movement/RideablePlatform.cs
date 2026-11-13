using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class RideablePlatform : MonoBehaviour
{
    [Header("Platform & Rider Settings")]
    [Tooltip("Assign the animated 'body' transform so the frog follows walk bobbing")]
    [SerializeField] private Transform platformTarget;
    [SerializeField] private LayerMask riderMask;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private readonly HashSet<FrogMotor> activeRiders = new HashSet<FrogMotor>();

    private void Start()
    {
        if (platformTarget == null) platformTarget = transform;

        lastPosition = platformTarget.position;
        lastRotation = platformTarget.rotation;
    }

    private void FixedUpdate()
    {
        if (platformTarget == null) return;

        Vector3 positionDelta = platformTarget.position - lastPosition;
        Quaternion rotationDelta = platformTarget.rotation * Quaternion.Inverse(lastRotation);
        float yawDelta = Mathf.DeltaAngle(0f, rotationDelta.eulerAngles.y);
        Quaternion flatYawRotation = Quaternion.Euler(0f, yawDelta, 0f);

        foreach (FrogMotor motor in activeRiders)
        {
            if (motor == null) continue;

            NetworkObject netObj = motor.GetComponent<NetworkObject>();
            bool isNetworked = (InstanceFinder.NetworkManager != null && (InstanceFinder.IsServerStarted || InstanceFinder.IsClientStarted));
            if (isNetworked && netObj != null && netObj.IsSpawned && !netObj.IsOwner) continue;

            // Calculate linear + orbital movement for this physics step
            Vector3 offsetFromPivot = motor.transform.position - lastPosition;
            Vector3 rotatedOffset = flatYawRotation * offsetFromPivot;
            Vector3 totalMove = positionDelta + (rotatedOffset - offsetFromPivot);

            // Tell FrogMotor the exact velocity of the moving sphere so it doesn't brake against it!
            motor.ExternalSupportVelocity = totalMove / Time.fixedDeltaTime;
        }

        lastPosition = platformTarget.position;
        lastRotation = platformTarget.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & riderMask) == 0) return;

        FrogMotor motor = other.GetComponentInParent<FrogMotor>();
        if (motor != null)
        {
            activeRiders.Add(motor);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & riderMask) == 0) return;

        FrogMotor motor = other.GetComponentInParent<FrogMotor>();
        if (motor != null)
        {
            motor.ExternalSupportVelocity = Vector3.zero;
            activeRiders.Remove(motor);
        }
    }

    private void OnDisable()
    {
        // Clean up velocity if the robot is disabled or destroyed while a frog is riding it
        foreach (FrogMotor motor in activeRiders)
        {
            if (motor != null) motor.ExternalSupportVelocity = Vector3.zero;
        }
        activeRiders.Clear();
    }
}