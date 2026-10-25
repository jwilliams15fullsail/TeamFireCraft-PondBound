using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

// The main menu's play buttons: Offline, plus the Multiplayer panel's Quick Match, Host,
// Join by code and the Browse list. Button OnClick events call the public methods.
public class PlayMenu : MonoBehaviour
{
    [Tooltip("The level to load: drag its scene file here. It must be in the Build Profiles scene list and must not contain its own NetworkManager.")]
    [SerializeField] SceneField gameScene;

    [Header("Multiplayer panel")]
    [SerializeField] CanvasGroup multiplayerPanel;  // greyed out while a request is running
    [SerializeField] TMP_InputField sessionNameInput;
    [SerializeField] TMP_InputField joinCodeInput;
    [SerializeField] Transform sessionList;         // Scroll View > Viewport > Content
    [SerializeField] SessionRow sessionRowPrefab;
    [SerializeField] TMP_Text statusText;

    bool busy;

    void Start()
    {
        // Coming back from a game: the frog camera may have locked the mouse.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetStatus(GameSession.LastMessage ?? "");
        GameSession.LastMessage = null;
    }

    public void PlayOffline()
    {
        if (busy || !LevelPicked())
            return;
        if (!GameSession.StartOffline(gameScene.SceneName))
            SetStatus("Couldn't start the offline game.");
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
        await Run("Joining...", () => GameSession.JoinByCodeAsync(code));
    }

    public async void RefreshSessions()
    {
        if (busy)
            return;
        SetBusy(true, "Searching...");
        try
        {
            IList<ISessionInfo> sessions = await GameSession.BrowseAsync();
            if (this == null)
                return; // the menu closed while we waited
            ShowSessions(sessions);
            SetStatus(sessions.Count == 0 ? "No open games. Host one or try Quick Match." : "");
        }
        catch (Exception e)
        {
            if (this != null)
                SetStatus(Friendly(e));
        }
        if (this != null)
            SetBusy(false, null);
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
    async Task Run(string message, Func<Task> request)
    {
        if (busy)
            return;
        SetBusy(true, message);
        try
        {
            await request();
        }
        catch (Exception e)
        {
            if (this == null)
                return;
            SetStatus(Friendly(e));
            SetBusy(false, null);
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