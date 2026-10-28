using System.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;

// Lets Unity Multiplayer Services (Lobby + Relay) start and stop FishNet instead of
// Netcode for GameObjects. GameSession hands one to every session it creates or joins.
public class FishNetSessionHandler : INetworkHandler
{
    const float ConnectTimeout = 15f; // seconds

    public async Task StartAsync(NetworkConfiguration configuration)
    {
        if (configuration.Type != NetworkType.Relay)
            throw new SessionException($"Only Relay games are set up for FishNet, not {configuration.Type}.", SessionError.NetworkSetupFailed, null);

        NetworkManager manager = InstanceFinder.NetworkManager;
        UnityTransport transport = GameSession.TransportOf(manager);
        if (transport == null)
            throw new SessionException("No FishNet NetworkManager in this scene.", SessionError.NetworkManagerNotInitialized, null);

        transport.SetRelayServerData(configuration.RelayServerData);
        if (configuration.Role == NetworkRole.Client)
        {
            manager.ClientManager.StartConnection();
            await WaitUntilStarted(transport, server: false);
            return;
        }

        // Relay games always have a host player: start the server, then our own client.
        transport.SetMaximumClients(GameSession.MaxPlayers);
        manager.ServerManager.StartConnection();
        await WaitUntilStarted(transport, server: true);
        manager.ClientManager.StartConnection();
        await WaitUntilStarted(transport, server: false);
    }

    public Task StopAsync()
    {
        GameSession.StopNetwork(InstanceFinder.NetworkManager);
        return Task.CompletedTask;
    }

    // The session service waits on this, so a failed connection shows up as a normal join error.
    static async Task WaitUntilStarted(Transport transport, bool server)
    {
        float giveUpAt = Time.realtimeSinceStartup + ConnectTimeout;
        while (true)
        {
            LocalConnectionState state = transport.GetConnectionState(server);
            if (state == LocalConnectionState.Started)
                return;
            if (state == LocalConnectionState.Stopped || Time.realtimeSinceStartup > giveUpAt)
            {
                string message = server ? "Couldn't start the game on the Relay server." : "Couldn't connect to the host.";
                throw new SessionException(message, SessionError.NetworkSetupFailed, null);
            }
            await Task.Yield(); // check again next frame
        }
    }
}