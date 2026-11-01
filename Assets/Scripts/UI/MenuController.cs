using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Menu Canvases")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsMenuCanvas;
    [SerializeField] private GameObject creditsMenuCanvas;
    [SerializeField] private GameObject playOnlineMenuCanvas;

    [Header("First Selected Button")]
    [SerializeField] private GameObject mainMenuFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject creditsFirstSelected;
    [SerializeField] private GameObject playOnlineFirstSelected;
    void Start()
    {
        
    }

    public void OpenMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        settingsMenuCanvas.SetActive(false);
        creditsMenuCanvas.SetActive(false);
        playOnlineMenuCanvas.SetActive(false);

        SetSelectedUI(mainMenuFirstSelected);
    }

    public void OpenSettingsMenu()
    {
        settingsMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
        creditsMenuCanvas.SetActive(false);
        playOnlineMenuCanvas.SetActive(false);

        SetSelectedUI(settingsFirstSelected);
    }

    public void OpenCreditsMenu()
    {
        creditsMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
        settingsMenuCanvas.SetActive(false);
        playOnlineMenuCanvas.SetActive(false);

        SetSelectedUI(creditsFirstSelected);
    }

    public void OpenPlayOnlineMenu()
    {
        creditsMenuCanvas.SetActive(false);
        mainMenuCanvas.SetActive(false);
        settingsMenuCanvas.SetActive(false);
        playOnlineMenuCanvas.SetActive(true);

        SetSelectedUI(playOnlineFirstSelected);
    }

    private void SetSelectedUI(GameObject targetElement)
    {
        if (EventSystem.current != null)
        {
            //Clears current selection first so it has a clean state
            EventSystem.current.SetSelectedGameObject(null);

            //Sets the new focused element
            EventSystem.current.SetSelectedGameObject(targetElement);
        }
    }
}
