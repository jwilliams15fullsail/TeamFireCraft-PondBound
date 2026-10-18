using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public DoorController door;
    int objectsOnPlate = 0;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weight"))
        {
            objectsOnPlate++;
            door.SetOpen(true);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weight"))
        {
            objectsOnPlate--;
            if (objectsOnPlate <= 0)
                door.SetOpen(false);
        }
    }
}
