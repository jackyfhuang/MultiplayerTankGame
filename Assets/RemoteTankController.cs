using System.Collections.Concurrent;
using UnityEngine;
using System;

public class RemoteTankController : MonoBehaviour
{
	// The player ID this tank represents e.g. "Player2"
	public string remotePlayerId;
    private ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();


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
            transform.position = new Vector3(x, y, 0);
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        });
    }

	void HandleShoot(string playerId, float x, float y, float rotation)
	{
		// Only shoot if the message is for our remote player
		if (playerId != remotePlayerId) return;

        // Shooting logic will go here in Stage 3
        _mainThreadActions.Enqueue(() =>
        {
            Debug.Log($"{playerId} fired at {x}, {y}");
            // Stage 3 shooting logic here
        });
    }
}