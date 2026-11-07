using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveMarkers : MonoBehaviour
{
    public Image markerImage;
    public ObjectiveSequence sequence;
    public TMP_Text meter;
    public Vector3 offset;

    [Tooltip("Subtract this amount from the camera-to-objective distance.")]
    public float distanceAdjustment = 3f;

    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        Transform targetObject = sequence != null ? sequence.CurrentTarget : null;

        if (targetObject == null)
        {
            if (markerImage != null)
                markerImage.gameObject.SetActive(false);

            if (meter != null)
                meter.text = "";

            return;
        }

        if (markerImage != null)
            markerImage.gameObject.SetActive(true);

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return;
        }

        float minX = markerImage.GetPixelAdjustedRect().width / 2f;
        float maxX = Screen.width - minX;
        float minY = markerImage.GetPixelAdjustedRect().height / 2f;
        float maxY = Screen.height - minY;

        Vector3 targetPosition = targetObject.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        if (Vector3.Dot(
                targetPosition - mainCamera.transform.position,
                mainCamera.transform.forward) < 0)
        {
            // Target is behind the camera.
            screenPosition.x = screenPosition.x < Screen.width / 2f
                ? maxX
                : minX;
        }

        screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
        screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);

        markerImage.transform.position = screenPosition;

        if (meter != null)
        {
            float actualDistance = Vector3.Distance(
                targetPosition, mainCamera.transform.position);

            int displayedDistance = Mathf.Max(
                0,
                Mathf.RoundToInt(actualDistance - distanceAdjustment));

            meter.text = displayedDistance + "m";
        }
    }
}