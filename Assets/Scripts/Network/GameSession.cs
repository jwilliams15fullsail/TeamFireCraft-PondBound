using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

// Starting and leaving games, shared by the main menu and the in-game panel.
// Online games are Unity Multiplayer Services sessions over Relay, so they work across
// the internet with no port forwarding. Offline is a host that only listens on this
// computer. Either way the frog scripts run exactly as they do in a hosted game.
public static class GameSession
{
    public const int MaxPlayers = 4;
    public const string MenuScene = "MainMenuScene";

    public static ISession Current { get; private set; }
    public static bool IsOffline { get; private set; }
    public static string LastMessage { get; set; } // shown by the menu after we get sent back to it

    static bool leaving;

    // Enter Play Mode Options skip the domain reload, so these values would survive from
    // the last Play session. Clear them at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun()
    {
        Current = null;
        IsOffline = false;
        LastMessage = null;
        leaving = false;
    }

    // Online games need a Unity Services player ID. Anonymous sign-in, done once.
    public static async Task SignInAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Host a public game that others can find with Browse or Quick Match.
    public static async Task HostAsync(string sessionName, string gameScene)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.CreateSessionAsync(NewSessionOptions(sessionName));
        LoadGameScene(gameScene);
    }

    // Join any game with a free slot. If none turns up in a few seconds, host a new one.
    public static async Task QuickMatchAsync(string gameScene)
    {
        await SignInAsync();
        var quickJoin = new QuickJoinOptions { CreateSession = true, Timeout = TimeSpan.FromSeconds(5) };
        Current = await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoin, NewSessionOptions(DefaultSessionName()));
        if (Current.IsHost)
            LoadGameScene(gameScene); // nobody to join, so we became the host
    }

    // Public games with a free slot, newest first.
    public static async Task<IList<ISessionInfo>> BrowseAsync()
    {
        await SignInAsync();
        var query = new QuerySessionsOptions
        {
            Count = 20,
            FilterOptions = new List<FilterOption> { new(FilterField.AvailableSlots, "0", FilterOperation.Greater) },
            SortOptions = new List<SortOption> { new(SortOrder.Descending, SortField.CreationTime) },
        };
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(query);
        return results.Sessions;
    }

    // Joiners don't load the level themselves: Netcode moves them into the host's scene.
    public static async Task JoinByIdAsync(string sessionId)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
    }

    public static async Task JoinByCodeAsync(string code)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant());
    }

    // Single player: a normal host, but it only listens on this computer and the
    // NetworkManager's approval turns everyone else away. No internet or sign-in needed.
    public static bool StartOffline(string gameScene)
    {
        NetworkManager manager = NetworkManager.Singleton;
        manager.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777, "127.0.0.1"); // also undoes any Relay setup from an earlier online game
        IsOffline = true;
        if (!manager.StartHost())
        {
            IsOffline = false;
            return false;
        }
        LoadGameScene(gameScene);
        return true;
    }

    // Back to the main menu from anywhere. The NetworkManager we carried into the level
    // is destroyed, because MainMenuScene brings a fresh one.
    public static async void LeaveToMenu()
    {
        if (leaving)
            return;
        leaving = true;

        await LeaveSessionAsync();

        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null)
        {
            manager.Shutdown();
            UnityEngine.Object.Destroy(manager.gameObject);
        }
        SceneManager.LoadScene(MenuScene);
        leaving = false;
    }

    // Quit to the desktop. Online, tell the session service first, so the game doesn't
    // linger in other players' Browse list after we're gone.
    public static async void QuitGame()
    {
        if (leaving)
            return;
        leaving = true;

        await LeaveSessionAsync();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static async Task LeaveSessionAsync()
    {
        ISession session = Current;
        Current = null;
        IsOffline = false;
        if (session == null)
            return;
        try
        {
            if (session.IsHost)
                await session.AsHost().DeleteAsync(); // the game is over for everyone
            else
                await session.LeaveAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Leaving the session failed: {e.Message}");
        }
    }


    static SessionOptions NewSessionOptions(string sessionName)
    {
        return new SessionOptions
        {
            Name = string.IsNullOrWhiteSpace(sessionName) ? DefaultSessionName() : sessionName.Trim(),
            MaxPlayers = MaxPlayers,
        }.WithRelayNetwork();
    }

    static string DefaultSessionName() => $"Pond {UnityEngine.Random.Range(1000, 10000)}";

    static void LoadGameScene(string gameScene)
    {
        SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
            Debug.LogError($"Couldn't load '{gameScene}' ({status}). Is it in the Build Profiles scene list?");
    }
}