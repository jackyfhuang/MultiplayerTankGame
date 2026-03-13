using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private HubConnection connection;

    public string playerId;

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

    private int movementSequence = 0;
    private int shootSequence = 0;

    private Dictionary<string, int> lastMovementSeq = new Dictionary<string, int>();
    private Dictionary<string, int> lastShootSeq = new Dictionary<string, int>();

    [Range(0f, 1f)]
    public float simulatedPacketLossRate = 0.1f;

    // pending playerId set by background thread, applied in Update()
    private string _pendingPlayerId = null;

    // instead of calling Unity APIs directly from the background thread
    private ConcurrentQueue<System.Action> _mainThreadQueue = new ConcurrentQueue<System.Action>();

    void Awake()
    {
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

    // drains the main-thread queue and applies pending playerId
    void Update()
    {
        // Apply playerId on the main thread so TankController polling sees it
        if (_pendingPlayerId != null)
        {
            playerId = _pendingPlayerId;
            _pendingPlayerId = null;
        }

        // Drain all queued callbacks onto the main thread
        while (_mainThreadQueue.TryDequeue(out System.Action action))
            action();
    }

    async void Start()
    {
        await ConnectToServer();
    }

    async Task ConnectToServer()
    {
        connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5190/tankgame", options => {
                options.SkipNegotiation = true;
                options.Transports = HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("AssignedPlayerId", (id) =>
        {
            _pendingPlayerId = id;
            Debug.Log($"Assigned as {id}");
        });

        connection.On<string>("PlayerConnected", (id) =>
        {
            _mainThreadQueue.Enqueue(() =>
            {
                Debug.Log($"{id} connected");
                OnPlayerConnected?.Invoke(id);
            });
        });

        connection.On<string>("PlayerDisconnected", (id) =>
        {
            _mainThreadQueue.Enqueue(() =>
            {
                Debug.Log($"{id} disconnected");
                OnPlayerDisconnected?.Invoke(id);
            });
        });

        connection.On<string, int, float, float, float>("ReceiveMovement",
            (id, sequenceNumber, x, y, rotation) =>
            {
                _mainThreadQueue.Enqueue(() =>
                {
                    if (Random.value < simulatedPacketLossRate)
                    {
                        Debug.Log($"[UDP Sim] Dropped movement packet #{sequenceNumber} from {id}");
                        return;
                    }

                    if (lastMovementSeq.TryGetValue(id, out int lastSeq) && sequenceNumber <= lastSeq)
                    {
                        Debug.Log($"[UDP Sim] Duplicate movement packet #{sequenceNumber} from {id}, discarding");
                        return;
                    }

                    if (lastMovementSeq.TryGetValue(id, out int prev) && sequenceNumber > prev + 1)
                    {
                        Debug.Log($"[UDP Sim] Lost {sequenceNumber - prev - 1} movement packet(s) from {id} " +
                                  $"(last: {prev}, got: {sequenceNumber}) — using last known position");
                    }

                    lastMovementSeq[id] = sequenceNumber;
                    OnMovementReceived?.Invoke(id, x, y, rotation);
                });
            });

        connection.On<string, int, float, float, float>("ReceiveShoot",
            (id, sequenceNumber, x, y, rotation) =>
            {
                _mainThreadQueue.Enqueue(() =>
                {
                    if (Random.value < simulatedPacketLossRate)
                    {
                        Debug.Log($"[UDP Sim] Dropped shoot packet #{sequenceNumber} from {id}");
                        return;
                    }

                    if (lastShootSeq.TryGetValue(id, out int lastSeq) && sequenceNumber <= lastSeq)
                    {
                        Debug.Log($"[UDP Sim] Duplicate shoot packet #{sequenceNumber} from {id}, discarding");
                        return;
                    }

                    if (lastShootSeq.TryGetValue(id, out int prev) && sequenceNumber > prev + 1)
                    {
                        Debug.Log($"[UDP Sim] Lost {sequenceNumber - prev - 1} shoot packet(s) from {id}");
                    }

                    lastShootSeq[id] = sequenceNumber;
                    OnShootReceived?.Invoke(id, x, y, rotation);
                });
            });

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

    public async Task SendMovement(float x, float y, float rotation)
    {
        if (connection.State == HubConnectionState.Connected)
        {
            movementSequence++;
            await connection.InvokeAsync("SendMovement", movementSequence, x, y, rotation);
        }
    }

    public async Task SendShoot(float x, float y, float rotation)
    {
        if (connection.State == HubConnectionState.Connected)
        {
            // counter, corrupting sequence tracking for both message types
            shootSequence++;
            await connection.InvokeAsync("SendShoot", shootSequence, x, y, rotation);
        }
    }

    async void OnDestroy()
    {
        if (connection != null)
            await connection.StopAsync();
    }
}