using UnityEngine;

// Put this on any button or object that shouldn't exist in the web build, such as the Exit /
// Quit buttons (a web page can't close itself). It hides the object as soon as the scene
// loads in a web build. In the Editor and desktop builds it does nothing.
public class HideOnWeb : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}