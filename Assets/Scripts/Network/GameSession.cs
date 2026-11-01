using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

// Starting and leaving games, shared by the main menu and the in-game panel.
// Online games are Unity Multiplayer Services sessions over Relay, so they work across
// the internet with no port forwarding; FishNetSessionHandler starts FishNet for them.
// Offline is a host that only listens on this computer. Either way the frog scripts run
// exactly as they do in a hosted game.
public static class GameSession
{
    public const int MaxPlayers = 4;
    public const string MenuScene = "MainMenuScene";

    public static ISession Current { get; private set; }
    public static bool IsOffline { get; private set; }
    public static bool IsLeaving { get; private set; } // leaving on purpose, so the disconnect isn't an error
    public static string LastMessage { get; set; } // shown by the menu after we get sent back to it

    // Enter Play Mode Options skip the domain reload, so these values would survive from
    // the last Play session. Clear them at the start of every run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun()
    {
        Current = null;
        IsOffline = false;
        IsLeaving = false;
        LastMessage = null;
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
        LoadLevel(gameScene);
    }

    // Join any game with a free slot. If none turns up in a few seconds, host a new one.
    public static async Task QuickMatchAsync(string gameScene)
    {
        await SignInAsync();
        var quickJoin = new QuickJoinOptions { CreateSession = true, Timeout = TimeSpan.FromSeconds(5) };
        Current = await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoin, NewSessionOptions(DefaultSessionName()));
        if (Current.IsHost)
            LoadLevel(gameScene); // nobody to join, so we became the host
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

    // Joiners don't load the level themselves: FishNet moves them into the host's level.
    public static async Task JoinByIdAsync(string sessionId)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, JoinOptions());
    }

    public static async Task JoinByCodeAsync(string code)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant(), JoinOptions());
    }

    // Single player: a normal host, but it only listens on this computer and lets
    // one player in (us). No internet or sign-in needed.
    public static bool StartOffline(string gameScene)
    {
        NetworkManager manager = InstanceFinder.NetworkManager;
        UnityTransport transport = TransportOf(manager);
        if (transport == null)
            return false;
        transport.SetConnectionData("127.0.0.1", 7777, "127.0.0.1"); // also undoes any Relay setup from an earlier online game
        transport.SetMaximumClients(1);
        IsOffline = true;
        if (!manager.ServerManager.StartConnection())
        {
            IsOffline = false;
            return false;
        }
        manager.ClientManager.StartConnection();
        LoadLevel(gameScene);
        return true;
    }

    // Host only: moves everyone to a level. Global = players who join later load it too.
    // ReplaceScenes All = the menu or the previous level is unloaded.
    public static void LoadLevel(string sceneName)
    {
        var level = new SceneLoadData(sceneName) { ReplaceScenes = ReplaceOption.All };
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(level);
    }

    // Back to the main menu from anywhere. The NetworkManager we carried into the level
    // is destroyed, because MainMenuScene brings a fresh one.
    public static async void LeaveToMenu()
    {
        if (IsLeaving)
            return;
        IsLeaving = true;
        Debug.Log("[Leave] 1: leaving");

        await LeaveSessionAsync();
        Debug.Log("[Leave] 2: session left, stopping the network");

        NetworkManager manager = InstanceFinder.NetworkManager;
        if (manager != null)
        {
            StopNetwork(manager);
            Debug.Log("[Leave] 3: network stopped, destroying the manager");
            UnityEngine.Object.Destroy(manager.gameObject);
        }
        UnitySceneManager.LoadScene(MenuScene);
        Debug.Log("[Leave] 4: loading the menu");
        IsLeaving = false;
    }

    // Quit to the desktop. Online, tell the session service first, so the game doesn't
    // linger in other players' Browse list after we're gone.
    public static async void QuitGame()
    {
        if (IsLeaving)
            return;
        IsLeaving = true;

        await LeaveSessionAsync();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Stops hosting and/or playing. Safe to call when nothing is running.
    public static void StopNetwork(NetworkManager manager)
    {
        if (manager == null)
            return;
        manager.ClientManager.StopConnection();
        manager.ServerManager.StopConnection(true);
    }

    // The FishyUnityTransport "Unity Transport" component on the NetworkManager.
    public static UnityTransport TransportOf(NetworkManager manager)
    {
        UnityTransport transport = manager != null ? manager.TransportManager.GetTransport<UnityTransport>() : null;
        if (transport == null)
            Debug.LogError("The scene needs a FishNet NetworkManager with FishyUnityTransport's Unity Transport component.");
        return transport;
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
        }.WithRelayNetwork().WithNetworkHandler(new FishNetSessionHandler());
    }

    static JoinSessionOptions JoinOptions() => new JoinSessionOptions().WithNetworkHandler(new FishNetSessionHandler());

    static string DefaultSessionName() => $"Pond {UnityEngine.Random.Range(1000, 10000)}";
}