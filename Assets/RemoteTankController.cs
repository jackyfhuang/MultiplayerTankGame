using System.Collections.Concurrent;
using UnityEngine;
using System;

public class RemoteTankController : MonoBehaviour
{
	// The player ID this tank represents e.g. "Player2"
	public string remotePlayerId;
    private ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();
    
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;


    void OnEnable()
	{
		// Listen for movement and shoot events from NetworkManager
		NetworkManager.OnMovementReceived += HandleMovement;
		NetworkManager.OnShootReceived += HandleShoot;
	}

	void OnDisable()
	{
		// Always unsubscribe when disabled to avoid memory leaks
		NetworkManager.OnMovementReceived -= HandleMovement;
		NetworkManager.OnShootReceived -= HandleShoot;
	}

    void Update()
    {
        // Drain the queue on the main thread each frame
        while (_mainThreadActions.TryDequeue(out Action action))
            action();
    }

    void HandleMovement(string playerId, float x, float y, float rotation)
	{
		// Only move this tank if the message is for our remote player
		if (playerId != remotePlayerId) return;

        _mainThreadActions.Enqueue(() =>
        {
            // If this GameObject was destroyed or doesn't exist, the movement handler shouldn't be called
            // But just in case, check if we're still valid
            if (this == null || gameObject == null) return;
            
            // Use Rigidbody2D if available for smoother movement
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.MovePosition(new Vector2(x, y));
                rb.MoveRotation(rotation);
            }
            else
            {
                transform.position = new Vector3(x, y, 0);
                transform.rotation = Quaternion.Euler(0, 0, rotation);
            }
        });
    }

	void HandleShoot(string playerId, float x, float y, float rotation)
	{
		// Only shoot if the message is for our remote player
		if (playerId != remotePlayerId) return;

        _mainThreadActions.Enqueue(() =>
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning($"RemoteTankController: Bullet prefab not assigned for {playerId}");
                return;
            }
            
            // Spawn bullet at the remote player's fire point position
            Vector3 spawnPos = firePoint != null ? firePoint.position : new Vector3(x, y, 0);
            Quaternion spawnRot = firePoint != null ? firePoint.rotation : Quaternion.Euler(0, 0, rotation);
            
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);
            
            // Set the remote tank as the owner so bullets don't collide with it
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetOwner(gameObject);
                bulletScript.SetShooterPlayerId(remotePlayerId);
                bulletScript.authoritativeDamage = false;
            }
            
            Debug.Log($"{playerId} fired at {spawnPos}");
        });
    }
}