using UnityEngine;

public class TankController : MonoBehaviour
{

    // Speed variables
    public float moveSpeed = 5f;     // forward/backward
    public float rotateSpeed = 200f; // turning speed

    public GameObject bulletPrefab;  // The bullet prefab
    public Transform firePoint;      // Where bullets spawn
    public float fireRate = 0.3f;
    private float nextFire = 0f;
    public float boundaryPadding = 0.5f;    // for boundary checking

    private Camera cam;

    // Assign these per tank instance in the Inspector
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode rotateLeftKey;
    public KeyCode rotateRightKey;
    public KeyCode fireKey;

    
    void Start()
    {
        cam = Camera.main;
    }

    // Network synchronization
    private bool isInitialized = false;
    public string ownerPlayerId;

    void Update()
    {
        if (!isInitialized)
        {
            if (NetworkManager.Instance != null &&
                !string.IsNullOrEmpty(NetworkManager.Instance.playerId))
            {
                // This tank doesn't belong to us — disable it
                if (NetworkManager.Instance.playerId != ownerPlayerId)
                {
                    enabled = false;
                    return;
                }
                isInitialized = true;
            }
            return; // Skip this frame until initialized
        }

        // Get input
        float moveInput = 0f;
        if (Input.GetKey(moveForwardKey)) moveInput = 1f;
        if (Input.GetKey(moveBackwardKey)) moveInput = -1f;

        float rotateInput = 0f;
        if (Input.GetKey(rotateLeftKey)) rotateInput = 1f;
        if (Input.GetKey(rotateRightKey)) rotateInput = -1f;

        // Move forward/backward
        transform.Translate(Vector2.up * moveInput * moveSpeed * Time.deltaTime);

        // Rotate
        transform.Rotate(Vector3.forward * -rotateInput * rotateSpeed * Time.deltaTime);

        ClampToScreen(); //prevent tank from going off-screen

        if (Input.GetKey(fireKey) && Time.time > nextFire)
        {
            Shoot();
            nextFire = Time.time + fireRate;
        }

        // Send our position to the server every frame
        _ = NetworkManager.Instance.SendMovement(
            transform.position.x,
            transform.position.y,
            transform.rotation.eulerAngles.z
        );
    }

    void Shoot()
    {
        // Check if references are assigned
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned in TankController!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Fire Point is not assigned in TankController!");
            return;
        }

        // Spawn a bullet at the FirePoint's position and rotation
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Set this tank as the owner so the bullet won't collide with us
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetOwner(gameObject);
        }

        // After Instantiate, also tell the server we fired
        _ = NetworkManager.Instance.SendShoot(
            firePoint.position.x,
            firePoint.position.y,
            firePoint.rotation.eulerAngles.z
        );
    }

    void ClampToScreen()
    {
        // Get world positions of the screen's bottom-left and top-right corners
        Vector3 minBounds = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxBounds = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Lock the tank's X and Y position within those bounds
        float clampedX = Mathf.Clamp(transform.position.x,
            minBounds.x + boundaryPadding,
            maxBounds.x - boundaryPadding);

        float clampedY = Mathf.Clamp(transform.position.y,
            minBounds.y + boundaryPadding,
            maxBounds.y - boundaryPadding);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}