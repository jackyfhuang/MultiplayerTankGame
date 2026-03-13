using Microsoft.AspNetCore.SignalR;

public class TankHub : Hub
{
    // Tracks connectionId -> playerId (e.g. "Player1" or "Player2")
    private static Dictionary<string, string> connectedPlayers = new Dictionary<string, string>();

    // ---- CONNECTION ----

    public override async Task OnConnectedAsync()
    {
        string playerId;
        List<string> existingPlayers;

        lock (connectedPlayers)
        {
            // Reject if 2 players already connected
            if (connectedPlayers.Count >= 2)
            {
                Context.Abort();
                return;
            }
            // Snapshot who's already connected BEFORE adding the new player
            existingPlayers = new List<string>(connectedPlayers.Values);

            // Assign Player1 or Player2 based on who connected first
            playerId = connectedPlayers.Count == 0 ? "Player1" : "Player2";
            connectedPlayers[Context.ConnectionId] = playerId;
        }

        // Tell this client which player they are
        await Clients.Caller.SendAsync("AssignedPlayerId", playerId);

        // Tell this client about everyone already in the game
        foreach (var existingId in existingPlayers)
        {
            await Clients.Caller.SendAsync("PlayerConnected", existingId);
        }

        // Tell everyone a new player joined
        await Clients.Others.SendAsync("PlayerConnected", playerId);

        Console.WriteLine($"{playerId} connected. ConnectionId: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string playerId;

        lock (connectedPlayers)
        {
            if (!connectedPlayers.TryGetValue(Context.ConnectionId, out playerId))
                return;

            connectedPlayers.Remove(Context.ConnectionId);
        }

        // Tell remaining players someone left
        await Clients.All.SendAsync("PlayerDisconnected", playerId);

        Console.WriteLine($"{playerId} disconnected.");

        await base.OnDisconnectedAsync(exception);
    }

    // ---- MOVEMENT ----

    // Client calls this when their tank moves
    // Server broadcasts it to all OTHER clients
    public async Task SendMovement(float x, float y, float rotation)
    {
        string playerId = connectedPlayers[Context.ConnectionId];

        await Clients.Others.SendAsync("ReceiveMovement", playerId, x, y, rotation);
    }

    // ---- SHOOTING ----

    // Client calls this when they fire
    // Server broadcasts it to all OTHER clients
    public async Task SendShoot(float x, float y, float rotation)
    {
        string playerId = connectedPlayers[Context.ConnectionId];

        await Clients.Others.SendAsync("ReceiveShoot", playerId, x, y, rotation);
    }
}