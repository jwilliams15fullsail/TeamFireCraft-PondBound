using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip lobbyMusic;
    public AudioClip gameplayMusic;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("MusicManager needs an AudioSource!");
            enabled = false;
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void Start()
    {
        UpdateMusic();
    }

    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        UpdateMusic();
    }

    void UpdateMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        Debug.Log("MusicManager detected scene: " + sceneName);

        if (sceneName == GameSession.MenuScene)
            PlayMusic(lobbyMusic);
        else
            PlayMusic(gameplayMusic);
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}