using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance; // Global access from other scripts

    private HubConnection connection;

    public string playerId;  // Will be "Player1" or "Player2", assigned by server

    // Events that other scripts can listen to
    public delegate void MovementReceived(string playerId, float x, float y, float rotation);
    public static event MovementReceived OnMovementReceived;

    public delegate void ShootReceived(string playerId, float x, float y, float rotation);
    public static event ShootReceived OnShootReceived;

    public delegate void PlayerConnected(string playerId);
    public static event PlayerConnected OnPlayerConnected;

    public delegate void PlayerDisconnected(string playerId);
    public static event PlayerDisconnected OnPlayerDisconnected;

    public delegate void PlayerAssigned(string playerId);
    public static event PlayerAssigned OnPlayerAssigned;

    void Awake()
    {
        // Singleton pattern so any script can access NetworkManager.Instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await ConnectToServer();
    }

    async Task ConnectToServer()
    {
        // Build the connection to your SignalR hub
        connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5190/tankgame")
            .WithAutomaticReconnect()
            .Build();

        // Listen for server events

        // Server tells us which player we are
        connection.On<string>("AssignedPlayerId", (id) =>
        {
            playerId = id;
            Debug.Log($"Assigned as {playerId}");
        });

        // Server tells us another player connected
        connection.On<string>("PlayerConnected", (id) =>
        {
            Debug.Log($"{id} connected");
            OnPlayerConnected?.Invoke(id);
        });

        // Server tells us a player disconnected
        connection.On<string>("PlayerDisconnected", (id) =>
        {
            Debug.Log($"{id} disconnected");
            OnPlayerDisconnected?.Invoke(id);
        });

        // Server sends us the opponent's movement
        connection.On<string, float, float, float>("ReceiveMovement", (id, x, y, rotation) =>
        {
            OnMovementReceived?.Invoke(id, x, y, rotation);
        });

        // Server sends us the opponent's shot
        connection.On<string, float, float, float>("ReceiveShoot", (id, x, y, rotation) =>
        {
            OnShootReceived?.Invoke(id, x, y, rotation);
        });

        // Start the connection
        try
        {
            await connection.StartAsync();
            Debug.Log("Connected to SignalR server!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Connection failed: {e.Message}");
        }
    }

    // Called by TankController to send movement to server
    public async Task SendMovement(float x, float y, float rotation)
    {
        if (connection.State == HubConnectionState.Connected)
        {
            await connection.InvokeAsync("SendMovement", x, y, rotation);
        }
    }

    // Called by TankController to send a shot to server
    public async Task SendShoot(float x, float y, float rotation)
    {
        if (connection.State == HubConnectionState.Connected)
        {
            await connection.InvokeAsync("SendShoot", x, y, rotation);
        }
    }

    // Clean up connection when game closes
    async void OnDestroy()
    {
        if (connection != null)
        {
            await connection.StopAsync();
        }
    }
}