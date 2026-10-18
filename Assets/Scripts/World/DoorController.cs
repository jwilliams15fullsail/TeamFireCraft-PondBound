using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0, -3, 0);
    public float openSpeed = 1.5f;

    Vector3 closedPos;
    Vector3 openPos;
    bool isOpen;
    void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;
    }


    void Update()
    {
        Vector3 target = isOpen ? openPos : closedPos;
        transform.position = Vector3.Lerp(transform.position, target, openSpeed * Time.deltaTime);
    }
    public void SetOpen(bool open)
    {
        isOpen = open;
    }
}
