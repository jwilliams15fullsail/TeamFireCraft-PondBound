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
        {
            var options = new InitializationOptions();
            string profile = TestPlayerProfile();
            if (profile != null)
                options.SetProfile(profile);
            await UnityServices.InitializeAsync(options);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Multiplayer Play Mode opens extra Editor windows (Player2, Player3, Player4) that share this
    // project, and with it the saved anonymous sign-in. Every window would sign in as the same
    // player, and a player can't join a session they're already in, so player 2 could never join
    // player 1. Each Editor window gets its own sign-in profile instead, named after its player.
    // Builds return null and keep the normal sign-in.
    private static string TestPlayerProfile()
    {
#if UNITY_EDITOR
        string[] args = Environment.GetCommandLineArgs();
        int at = Array.IndexOf(args, "-name");
        string name = at >= 0 && at + 1 < args.Length ? args[at + 1] : "Player1"; // the main Editor has no -name

        // Profile names allow letters, digits, - and _ only, up to 30 characters.
        var profile = new System.Text.StringBuilder();
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                profile.Append(c);
        }
        if (profile.Length == 0)
            return "Player1";
        return profile.Length > 30 ? profile.ToString(0, 30) : profile.ToString();
#else
        return null;
#endif
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

    // Call with the intro/lobby scene name. Throws if the game couldn't start.
    // Desktop and the Editor: a local-only server on this machine, no internet needed.
    // Web builds: a browser can't run a server, so Unity Transport refuses one there. The web
    // build hosts a private Relay game with room for one player instead (needs internet,
    // which a web build always has). Nobody can find or join it.
    public static Task StartOfflineAsync(string introScene)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return StartOfflineOnRelayAsync(introScene);
#else
        if (!StartOfflineLocal(introScene))
            throw new Exception("Couldn't start the offline game.");
        return Task.CompletedTask;
#endif
    }

    static async Task StartOfflineOnRelayAsync(string introScene)
    {
        await SignInAsync();

        var options = new SessionOptions
        {
            Name = $"Offline {UnityEngine.Random.Range(1000, 10000)}",
            MaxPlayers = 1,
            IsPrivate = true,
        }
        .WithRelayNetwork()
        .WithNetworkHandler(new FishNetSessionHandler());

        IsOffline = true;
        try
        {
            Current = await MultiplayerService.Instance.CreateSessionAsync(options);
        }
        catch
        {
            IsOffline = false;
            throw;
        }

        LoadLevel(introScene);
    }

    static bool StartOfflineLocal(string introScene)
    {
        NetworkManager manager = InstanceFinder.NetworkManager;
        UnityTransport transport = TransportOf(manager);

        if (manager == null || transport == null)
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

    public static void QuitGame()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // A web page can't close itself: Quit does nothing in a browser. Go back to the menu.
        LeaveToMenu();
#else
        QuitAppAsync();
#endif
    }

    static async void QuitAppAsync()
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
        if (manager == null)
            manager = InstanceFinder.NetworkManager;

        if (manager == null || manager.TransportManager == null)
        {
            Debug.LogError("TransportOf: NetworkManager or TransportManager is null.");
            return null;
        }

        return manager.TransportManager.GetTransport<UnityTransport>();
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