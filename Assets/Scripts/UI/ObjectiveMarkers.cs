using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveMarkers : MonoBehaviour
{
    public Image markerImage;
    public ObjectiveSequence sequence;
    public TMP_Text meter;
    public Vector3 offset;

    void Update()
    {
        Transform targetObject = sequence != null ? sequence.CurrentTarget : null;

        if (targetObject == null)
        {
            markerImage.gameObject.SetActive(false);
            if (meter != null)
                meter.text = "";
            return;
        }

        markerImage.gameObject.SetActive(true);

        float minX = markerImage.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;
        float minY = markerImage.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(targetObject.position + offset);

        if (Vector3.Dot(
                targetObject.position - transform.position,
                transform.forward) < 0)
        {
            if (screenPosition.x < Screen.width / 2)
                screenPosition.x = maxX;
            else
                screenPosition.x = minX;
        }

        screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
        screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);

        markerImage.transform.position = screenPosition;

        if (meter != null)
        {
            int distance = (int)Vector3.Distance(
                targetObject.position, transform.position);
            meter.text = distance + "m";
        }
    }
}