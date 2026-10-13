using UnityEngine;

// Smoothly follows the local player from behind and above.
public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -8f);
    [SerializeField] float smoothTime = 0.15f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (Target == null)
            return;

        Vector3 desired = Target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(Target.position + Vector3.up * 1f);
    }
}
