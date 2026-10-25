using GameKit.Dependencies.Utilities.Types;
using UnityEngine;

public class MoveingPlatform : MonoBehaviour
{
    [SerializeField] public float speed = 2f;
    [SerializeField] Vector3 travelDistance;
    Vector3 startPos;
    Vector3 endPos;
    Vector3 target;
    public Vector3 DeltaMovement { get; private set; }
    private Vector3 lastPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        endPos = startPos + travelDistance;
        target = endPos;

        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = (target == startPos) ? endPos : startPos;
        }
        DeltaMovement = transform.position - lastPos;
        lastPos = transform.position;

    }
}
