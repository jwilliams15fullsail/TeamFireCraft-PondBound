using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

// The main menu's play buttons: Offline, plus the Multiplayer panel's Quick Match, Host,
// Join by code and the Browse list. Button OnClick events call the public methods.
public class PlayMenu : MonoBehaviour
{
    [Tooltip("The level to load: drag its scene file here. It must be in the Build Profiles scene list and must not contain its own NetworkManager.")]
    [SerializeField] SceneField gameScene;
    [Tooltip("Optional: the Continue button, so it's greyed out while there's no saved game.")]
    [SerializeField] Button continueButton;
    [Tooltip("Optional: where the player types the name shown above their frog.")]
    [SerializeField] TMP_InputField playerNameInput;

    [Header("Multiplayer panel")]
    [SerializeField] CanvasGroup multiplayerPanel;  // greyed out while a request is running
    [SerializeField] TMP_InputField sessionNameInput;
    [SerializeField] TMP_InputField joinCodeInput;
    [SerializeField] Transform sessionList;         // Scroll View > Viewport > Content
    [SerializeField] SessionRow sessionRowPrefab;
    [SerializeField] TMP_Text statusText;

    [Header("List box looks (optional)")]
    [Tooltip("Shown while Refresh is searching: a spinner and \"Looking for ponds...\".")]
    [SerializeField] GameObject loadingState;
    [Tooltip("Shown when a search finds no games.")]
    [SerializeField] GameObject emptyState;
    [Tooltip("The chip in the list's header, e.g. \"3 GAMES\".")]
    [SerializeField] TMP_Text sessionCountText;
    [Tooltip("The Join Code field's FieldHighlight, so a code that fails turns it red.")]
    [SerializeField] FieldHighlight joinCodeField;

    bool busy;

    void Start()
    {
        // Coming back from a game: the frog camera may have locked the mouse.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetStatus(GameSession.LastMessage ?? "");
        GameSession.LastMessage = null;
        RefreshContinueButton();

        // Your name above your frog. It's saved as you type, so it sticks between games.
        if (playerNameInput != null)
        {
            playerNameInput.characterLimit = FrogIdentity.MaxNameLength;
            playerNameInput.text = PlayerPrefs.GetString(FrogIdentity.NameKey, "");
            playerNameInput.onValueChanged.AddListener(typed => PlayerPrefs.SetString(FrogIdentity.NameKey, typed.Trim()));
        }

        // Join codes are letters and digits: type them in capitals, skip spaces and dashes.
        if (joinCodeInput != null)
            joinCodeInput.onValidateInput = (text, index, c) => char.IsLetterOrDigit(c) ? char.ToUpperInvariant(c) : '\0';

        SetListState(false, false, null);
    }

    // Also when the menu is shown again (coming back from a game, or switching panels).
    void OnEnable() => RefreshContinueButton();

    // Greyed out until a checkpoint has been saved. The save survives quitting the game.
    void RefreshContinueButton()
    {
        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasCheckpointSave;
    }

    public async void PlayOffline()
    {
        if (busy || !LevelPicked())
            return;
        await Run("Starting...", () => GameSession.StartOfflineAsync(gameScene.SceneName));
    }

    // The Continue button: back to the level and checkpoint of the last save, offline.
    public async void ContinueOffline()
    {
        if (busy)
            return;
        SaveData save = SaveSystem.Load();
        if (save == null || save.checkpoint <= 0)
        {
            SetStatus("No saved game yet.");
            return;
        }
        SaveSystem.Continuing = save; // the level's PlayerSpawner picks it up
        if (!await Run("Starting...", () => GameSession.StartOfflineAsync(save.sceneName)))
            SaveSystem.Continuing = null;
    }

    public async void QuickMatch()
    {
        if (!LevelPicked())
            return;
        await Run("Looking for a game...", () => GameSession.QuickMatchAsync(gameScene.SceneName));
    }

    public async void HostGame()
    {
        if (!LevelPicked())
            return;
        string sessionName = sessionNameInput != null ? sessionNameInput.text : "";
        await Run("Creating game...", () => GameSession.HostAsync(sessionName, gameScene.SceneName));
    }

    bool LevelPicked()
    {
        if (gameScene != null && gameScene.IsSet)
            return true;
        SetStatus("No level picked. Drag a scene into PlayMenu's Game Scene slot.");
        return false;
    }

    public async void JoinByCode()
    {
        string code = joinCodeInput != null ? joinCodeInput.text : "";
        if (string.IsNullOrWhiteSpace(code))
        {
            SetStatus("Type a join code first.");
            return;
        }
        if (!await Run("Joining...", () => GameSession.JoinByCodeAsync(code)) && joinCodeField != null)
            joinCodeField.SetError(true);
    }

    // The PASTE button next to the join code.
    public void PasteJoinCode()
    {
        if (joinCodeInput == null)
            return;
        string pasted = GUIUtility.systemCopyBuffer ?? "";
        var code = new System.Text.StringBuilder();
        foreach (char c in pasted)
        {
            if (char.IsLetterOrDigit(c))
                code.Append(char.ToUpperInvariant(c));
        }
        joinCodeInput.text = code.ToString();
    }

    public async void RefreshSessions()
    {
        if (busy)
            return;
        SetBusy(true, "Searching...");
        SetListState(true, false, null);
        try
        {
            IList<ISessionInfo> sessions = await GameSession.BrowseAsync();
            if (this == null)
                return; // the menu closed while we waited
            ShowSessions(sessions);
            SetListState(false, sessions.Count == 0, sessions.Count == 1 ? "1 GAME" : $"{sessions.Count} GAMES");
            SetStatus(sessions.Count == 0 ? "No open games. Host one or try Quick Match." : "");
        }
        catch (Exception e)
        {
            if (this == null)
                return;
            SetListState(false, false, null);
            SetStatus(Friendly(e));
        }
        SetBusy(false, null);
    }

    // The list box's Loading and Empty looks, and the "3 GAMES" chip in its header.
    void SetListState(bool loading, bool empty, string count)
    {
        if (loadingState != null)
            loadingState.SetActive(loading);
        if (emptyState != null)
            emptyState.SetActive(empty);
        if (sessionCountText != null && count != null)
            sessionCountText.text = count;
    }

    void ShowSessions(IList<ISessionInfo> sessions)
    {
        foreach (Transform row in sessionList)
            Destroy(row.gameObject);
        foreach (ISessionInfo info in sessions)
            Instantiate(sessionRowPrefab, sessionList).Show(info, JoinSession);
    }

    async void JoinSession(string sessionId)
    {
        await Run("Joining...", () => GameSession.JoinByIdAsync(sessionId));
    }

    // Runs one online request. On success the scene changes, so the panel stays greyed out.
    // Returns false when it failed (or another request was already running).
    async Task<bool> Run(string message, Func<Task> request)
    {
        if (busy)
            return false;
        SetBusy(true, message);
        try
        {
            await request();
            return true;
        }
        catch (Exception e)
        {
            if (this == null)
                return false;
            SetStatus(Friendly(e));
            SetBusy(false, null);
            return false;
        }
    }

    void SetBusy(bool value, string message)
    {
        busy = value;
        if (multiplayerPanel != null)
            multiplayerPanel.interactable = !value;
        if (message != null)
            SetStatus(message);
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    static string Friendly(Exception e)
    {
        Debug.LogException(e);
        if (e is SessionException session)
        {
            switch (session.Error)
            {
                case SessionError.SessionNotFound: return "That game doesn't exist anymore.";
                case SessionError.NotAuthorized: return "Couldn't sign in to Unity Services.";
                case SessionError.RateLimitExceeded: return "Too many requests. Wait a moment and try again.";
            }
        }
        return $"Something went wrong: {e.Message}";
    }
}