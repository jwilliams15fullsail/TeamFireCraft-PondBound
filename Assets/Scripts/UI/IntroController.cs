using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IntroController : MonoBehaviour
{
    [Header("Assign pages in order")]
    [SerializeField] private GameObject[] pages;

#if UNITY_EDITOR
    [Header("Scene to load after the intro")]
    [SerializeField] private SceneAsset gameSceneAsset;
#endif

    [SerializeField, HideInInspector] private string gameSceneName;

    private int currentPage;

    private void Start()
    {
        if (pages == null || pages.Length == 0)
        {
            Debug.LogError("IntroController has no pages assigned.", this);
            return;
        }

        ShowPage(0);
    }

    public void NextPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPage < pages.Length - 1)
            ShowPage(currentPage + 1);
        else
            FinishIntro();
    }

    public void SkipIntro()
    {
        FinishIntro();
    }

    private void ShowPage(int pageIndex)
    {
        currentPage = pageIndex;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == currentPage);
        }
    }

    private void FinishIntro()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("Assign a gameplay scene to Intro Controller.", this);
            return;
        }

        GameSession.StartGameplay(gameSceneName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (gameSceneAsset != null)
            gameSceneName = gameSceneAsset.name;
    }
#endif
}