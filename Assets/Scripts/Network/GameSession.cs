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

public static class GameSession
{
    public const int MaxPlayers = 4;
    public const string MenuScene = "MainMenuScene";

    public static ISession Current { get; private set; }
    public static bool IsOffline { get; private set; }
    public static bool IsLeaving { get; private set; }
    public static string LastMessage { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForNewRun()
    {
        Current = null;
        IsOffline = false;
        IsLeaving = false;
        LastMessage = null;
    }

    public static async Task SignInAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Call with the intro/lobby scene name.
    public static async Task HostAsync(string sessionName, string introScene)
    {
        await SignInAsync();

        Current = await MultiplayerService.Instance.CreateSessionAsync(
            NewSessionOptions(sessionName));

        LoadLevel(introScene);
    }

    // Call with the intro/lobby scene name.
    public static async Task QuickMatchAsync(string introScene)
    {
        await SignInAsync();

        var quickJoin = new QuickJoinOptions
        {
            CreateSession = true,
            Timeout = TimeSpan.FromSeconds(5)
        };

        Current = await MultiplayerService.Instance.MatchmakeSessionAsync(
            quickJoin,
            NewSessionOptions(DefaultSessionName()));

        // If we became host, load the intro scene.
        // If we joined somebody, the host controls the scene.
        if (Current.IsHost)
            LoadLevel(introScene);
    }

    public static async Task<IList<ISessionInfo>> BrowseAsync()
    {
        await SignInAsync();

        var query = new QuerySessionsOptions
        {
            Count = 20,
            FilterOptions = new List<FilterOption>
            {
                new(FilterField.AvailableSlots, "0", FilterOperation.Greater)
            },
            SortOptions = new List<SortOption>
            {
                new(SortOrder.Descending, SortField.CreationTime)
            },
        };

        QuerySessionsResults results =
            await MultiplayerService.Instance.QuerySessionsAsync(query);

        return results.Sessions;
    }

    public static async Task JoinByIdAsync(string sessionId)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.JoinSessionByIdAsync(
            sessionId, JoinOptions());
    }

    public static async Task JoinByCodeAsync(string code)
    {
        await SignInAsync();
        Current = await MultiplayerService.Instance.JoinSessionByCodeAsync(
            code.Trim().ToUpperInvariant(), JoinOptions());
    }

    // Call with the intro/lobby scene name.
    public static bool StartOffline(string introScene)
    {
        NetworkManager manager = InstanceFinder.NetworkManager;
        UnityTransport transport = TransportOf(manager);

        if (transport == null)
            return false;

        // Local-only connection; this also resets any old Relay address.
        transport.SetConnectionData("127.0.0.1", 7777, "127.0.0.1");
        transport.SetMaximumClients(1);

        IsOffline = true;

        if (!manager.ServerManager.StartConnection())
        {
            IsOffline = false;
            return false;
        }

        manager.ClientManager.StartConnection();
        LoadLevel(introScene);
        return true;
    }

    // Called by the intro controller when the host chooses to start the game.
    // Only the server/host should make the shared scene transition.
    public static void StartGameplay(string gameScene)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            Debug.Log("Waiting for the host to start gameplay.");
            return;
        }

        LoadLevel(gameScene);
    }

    // Host loads a scene for everyone. ReplaceScenes All unloads the old scene.
    public static void LoadLevel(string sceneName)
    {
        var level = new SceneLoadData(sceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };

        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(level);
    }

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

    public static void StopNetwork(NetworkManager manager)
    {
        if (manager == null)
            return;

        manager.ClientManager.StopConnection();
        manager.ServerManager.StopConnection(true);
    }

    public static UnityTransport TransportOf(NetworkManager manager)
    {
        UnityTransport transport = manager != null
            ? manager.TransportManager.GetTransport<UnityTransport>()
            : null;

        if (transport == null)
        {
            Debug.LogError(
                "The scene needs a FishNet NetworkManager with FishyUnityTransport's Unity Transport component.");
        }

        return transport;
    }

    private static async Task LeaveSessionAsync()
    {
        ISession session = Current;
        Current = null;
        IsOffline = false;

        if (session == null)
            return;

        try
        {
            if (session.IsHost)
                await session.AsHost().DeleteAsync();
            else
                await session.LeaveAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Leaving the session failed: {e.Message}");
        }
    }

    private static SessionOptions NewSessionOptions(string sessionName)
    {
        return new SessionOptions
        {
            Name = string.IsNullOrWhiteSpace(sessionName)
                ? DefaultSessionName()
                : sessionName.Trim(),
            MaxPlayers = MaxPlayers,
        }
        .WithRelayNetwork()
        .WithNetworkHandler(new FishNetSessionHandler());
    }

    private static JoinSessionOptions JoinOptions()
    {
        return new JoinSessionOptions()
            .WithNetworkHandler(new FishNetSessionHandler());
    }

    private static string DefaultSessionName()
    {
        return $"Pond {UnityEngine.Random.Range(1000, 10000)}";
    }
}