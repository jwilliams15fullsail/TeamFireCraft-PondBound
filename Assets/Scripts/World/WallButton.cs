using UnityEngine;

public class WallButton : MonoBehaviour
{
    public DoorController door;
    //public float resetDelay; //optional for automatic reset
    bool isActive = false;

    public void Activate()
    {
        isActive = !isActive;
        door.SetOpen(isActive);
    }
    //void ResetButton()
    //{
    //    isActive = false;
    //    door.SetOpen(false);
    //}
}
