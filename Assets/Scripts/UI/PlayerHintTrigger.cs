using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHintTrigger : MonoBehaviour
{
    [SerializeField] private Image hintImage;
    [SerializeField] private Sprite areaHintSprite;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hintImage != null && areaHintSprite != null)
            {
                hintImage.sprite = areaHintSprite;
                hintImage.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hintImage != null)
            {
                hintImage.gameObject.SetActive(false);
            }
        }
    }
}
