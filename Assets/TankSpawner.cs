using UnityEngine;
using System.Collections.Generic;

public class TankSpawner : MonoBehaviour
{
    [Header("Tank Prefabs")]
    public GameObject localTankPrefab;
    public GameObject remoteTankPrefab;
    
    [Header("Spawn Settings")]
    public Vector2[] spawnPositions = new Vector2[]
    {
        new Vector2(-5f, 0f),  // Player 1 spawn
        new Vector2(5f, 0f)    // Player 2 spawn
    };
    
    private Dictionary<string, GameObject> spawnedTanks = new Dictionary<string, GameObject>();
    private NetworkManager networkManager;

    /// <summary>Peers we heard about before NetworkManager.playerId was ready (ordering race).</summary>
    private List<string> pendingRemotePlayerIds = new List<string>();
    
    void OnEnable()
    {
        NetworkManager.OnPlayerConnected += OnPlayerConnected;
        NetworkManager.OnPlayerDisconnected += OnPlayerDisconnected;
        NetworkManager.OnPlayerAssigned += OnPlayerAssigned;
        NetworkManager.OnMovementReceived += OnMovementReceived;
    }
    
    void OnDisable()
    {
        NetworkManager.OnPlayerConnected -= OnPlayerConnected;
        NetworkManager.OnPlayerDisconnected -= OnPlayerDisconnected;
        NetworkManager.OnPlayerAssigned -= OnPlayerAssigned;
        NetworkManager.OnMovementReceived -= OnMovementReceived;
    }
    
    void Start()
    {
        networkManager = NetworkManager.Instance;
    }
    
    void OnPlayerAssigned(string playerId)
    {
        Debug.Log($"TankSpawner: Player assigned as {playerId}");
        if (spawnedTanks.ContainsKey(playerId))
        {
            FlushPendingRemotePlayers();
            return;
        }

        GameObject local = FindExistingLocalTank();
        if (local != null)
        {
            TankController controller = local.GetComponent<TankController>();
            if (controller != null)
                controller.ownerPlayerId = playerId;
            spawnedTanks[playerId] = local;
            SetupCameraFollow(local.transform);
            Debug.Log($"TankSpawner: Bound server id {playerId} to existing local tank.");
        }
        else
            SpawnLocalTank(playerId);

        FlushPendingRemotePlayers();
    }

    void FlushPendingRemotePlayers()
    {
        if (networkManager == null || string.IsNullOrEmpty(networkManager.playerId))
            return;

        for (int i = pendingRemotePlayerIds.Count - 1; i >= 0; i--)
        {
            string pid = pendingRemotePlayerIds[i];
            pendingRemotePlayerIds.RemoveAt(i);
            if (pid != networkManager.playerId)
                TrySpawnRemotePlayer(pid);
        }
    }

    void TrySpawnRemotePlayer(string playerId)
    {
        if (spawnedTanks.ContainsKey(playerId))
            return;
        Debug.Log($"TankSpawner: Spawning remote tank for {playerId}");
        SpawnRemoteTank(playerId);
    }

    static GameObject FindExistingLocalTank()
    {
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        if (wg != null && wg.SpawnedLocalTank != null)
            return wg.SpawnedLocalTank;

        foreach (TankController tc in Object.FindObjectsByType<TankController>(FindObjectsSortMode.None))
        {
            if (tc != null && tc.enabled && string.IsNullOrEmpty(tc.ownerPlayerId))
                return tc.gameObject;
        }

        return null;
    }
    
    void OnPlayerConnected(string playerId)
    {
        Debug.Log($"TankSpawner: Peer event — '{playerId}' (our id: '{networkManager?.playerId ?? "(none yet)"}')");
        if (networkManager != null && !string.IsNullOrEmpty(networkManager.playerId))
        {
            if (networkManager.playerId != playerId)
                TrySpawnRemotePlayer(playerId);
            return;
        }

        if (!pendingRemotePlayerIds.Contains(playerId))
            pendingRemotePlayerIds.Add(playerId);
        Debug.Log($"TankSpawner: Waiting for our AssignedPlayerId before spawning remote '{playerId}'");
    }
    
    void OnPlayerDisconnected(string playerId)
    {
        if (spawnedTanks.ContainsKey(playerId))
        {
            Destroy(spawnedTanks[playerId]);
            spawnedTanks.Remove(playerId);
            Debug.Log($"Removed tank for {playerId}");
        }
    }
    
    void SpawnLocalTank(string playerId)
    {
        if (localTankPrefab == null)
        {
            Debug.LogError("TankSpawner: Local Tank Prefab is not assigned!");
            return;
        }
        
        int playerIndex = playerId == "Player1" ? 0 : 1;
        Vector2 spawnPos = playerIndex < spawnPositions.Length 
            ? spawnPositions[playerIndex] 
            : Vector2.zero;
        
        GameObject tank = Instantiate(localTankPrefab, spawnPos, Quaternion.identity);
        tank.name = $"LocalTank_{playerId}";
        
        // Set the owner player ID
        TankController controller = tank.GetComponent<TankController>();
        if (controller != null)
        {
            controller.ownerPlayerId = playerId;
        }
        
        spawnedTanks[playerId] = tank;
        Debug.Log($"Spawned local tank for {playerId} at {spawnPos}");
        
        // Set up camera to follow the local tank
        SetupCameraFollow(tank.transform);
    }
    
    void SetupCameraFollow(Transform target)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("TankSpawner: Main Camera not found. Camera follow not set up.");
            return;
        }
        
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        cameraFollow.target = target;
        Debug.Log("TankSpawner: Camera follow set up for local tank.");
    }
    
    void SpawnRemoteTank(string playerId)
    {
        if (remoteTankPrefab == null)
        {
            // Fallback to local prefab if remote prefab not set
            if (localTankPrefab == null)
            {
                Debug.LogError("TankSpawner: No tank prefab assigned!");
                return;
            }
            remoteTankPrefab = localTankPrefab;
        }
        
        int playerIndex = playerId == "Player1" ? 0 : 1;
        Vector2 spawnPos = playerIndex < spawnPositions.Length 
            ? spawnPositions[playerIndex] 
            : Vector2.zero;
        
        GameObject tank = Instantiate(remoteTankPrefab, spawnPos, Quaternion.identity);
        tank.name = $"RemoteTank_{playerId}";
        
        // Remove TankController (local control) and add RemoteTankController
        TankController localController = tank.GetComponent<TankController>();
        if (localController != null)
        {
            DestroyImmediate(localController);
        }
        
        RemoteTankController remoteController = tank.GetComponent<RemoteTankController>();
        if (remoteController == null)
        {
            remoteController = tank.AddComponent<RemoteTankController>();
        }
        remoteController.remotePlayerId = playerId;
        
        // Set up bullet prefab and firePoint from the prefab
        TankController originalController = remoteTankPrefab.GetComponent<TankController>();
        if (originalController != null)
        {
            remoteController.bulletPrefab = originalController.bulletPrefab;
            // Find FirePoint child
            Transform firePoint = tank.transform.Find("FirePoint");
            if (firePoint != null)
            {
                remoteController.firePoint = firePoint;
            }
        }
        
        spawnedTanks[playerId] = tank;
        Debug.Log($"Spawned remote tank for {playerId} at {spawnPos}");
    }
    
    void OnMovementReceived(string playerId, float x, float y, float rotation)
    {
        // Fallback: If we receive movement from a player but don't have their tank spawned, spawn it
        if (networkManager != null && 
            !string.IsNullOrEmpty(networkManager.playerId) && 
            networkManager.playerId != playerId &&
            !spawnedTanks.ContainsKey(playerId))
        {
            Debug.Log($"TankSpawner: Received movement from {playerId} but no tank exists, spawning remote tank");
            SpawnRemoteTank(playerId);
        }
    }
}
