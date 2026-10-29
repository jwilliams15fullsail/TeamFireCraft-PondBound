using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonAudio : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Effects")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound, volume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("BUTTON CLICK DETECTED!");

        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(
                clickSound,
                Vector3.zero,
                volume
            );
        }
    }
}