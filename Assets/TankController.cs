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

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private TankHealth health;

    // Network synchronization
    private bool isInitialized = false;
    public string ownerPlayerId;

    // Assign these per tank instance in the Inspector
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode rotateLeftKey;
    public KeyCode rotateRightKey;
    public KeyCode fireKey;

    // Input values stored for physics-based movement
    private float moveInput = 0f;
    private float rotateInput = 0f;

    [Header("Debug")]
    public bool offlineMode = false;
    
    void Start()
    {

        // Get or add Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Get BoxCollider2D for collision checking
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("TankController: BoxCollider2D is missing! Tank needs a collider to detect walls.");
        }
        else
        {
            boxCollider.isTrigger = false;
        }
        
        // Cache health component
        health = GetComponent<TankHealth>();
        
        // Configure Rigidbody2D
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 1000f;
        rb.gravityScale = 0f;
        rb.angularDamping = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    bool IsDead()
    {
        return health != null && health.IsDead();
    }

    void Update()
    {
        NetworkManager nm = NetworkManager.Instance;
        if (nm != null && string.IsNullOrEmpty(ownerPlayerId) && !string.IsNullOrEmpty(nm.playerId))
            ownerPlayerId = nm.playerId;

        // Network initialization: local tank may spawn before server assigns playerId
        if (!isInitialized)
        {
            if (offlineMode)
            {
                isInitialized = true;
                return;
            }

            if (nm != null && !string.IsNullOrEmpty(nm.playerId))
            {
                if (string.IsNullOrEmpty(ownerPlayerId))
                    ownerPlayerId = nm.playerId;

                if (ownerPlayerId != nm.playerId)
                {
                    enabled = false;
                    return;
                }
            }

            isInitialized = true;
        }

        if (IsDead())
            return;

        // Read input using custom KeyCode system (for multiplayer compatibility)
        moveInput = 0f;
        if (Input.GetKey(moveForwardKey)) moveInput = 1f;
        if (Input.GetKey(moveBackwardKey)) moveInput = -1f;

        rotateInput = 0f;
        if (Input.GetKey(rotateLeftKey)) rotateInput = 1f;
        if (Input.GetKey(rotateRightKey)) rotateInput = -1f;

        // Also support standard input for local play (fallback)
        if (moveInput == 0f) moveInput = Input.GetAxis("Vertical");
        if (rotateInput == 0f) rotateInput = Input.GetAxis("Horizontal");

        // Check for firing - use custom key if set, otherwise fallback to Spacebar
        bool shouldFire = (fireKey != KeyCode.None && Input.GetKey(fireKey)) || 
                          (fireKey == KeyCode.None && Input.GetKey(KeyCode.Space));
        
        if (shouldFire && Time.time > nextFire)
        {
            Shoot();
            nextFire = Time.time + fireRate;
        }

        // Send our position to the server every frame (if networked)
        if (!offlineMode && NetworkManager.Instance != null && NetworkManager.Instance.playerId == ownerPlayerId)
        {
            NetworkManager.Instance.SendMovement(
                transform.position.x,
                transform.position.y,
                transform.rotation.eulerAngles.z
            );
        }
    }

    void FixedUpdate()
    {
        if (!isInitialized || IsDead())
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        // Apply rotation using physics-safe transform rotation
        transform.Rotate(Vector3.forward * -rotateInput * rotateSpeed * Time.fixedDeltaTime);
        
        // Apply movement using Rigidbody2D physics (for wall collision)
        rb.linearVelocity = (Vector2)transform.up * moveInput * moveSpeed;
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

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetOwner(gameObject);
            bulletScript.SetShooterPlayerId(ownerPlayerId);
            bulletScript.authoritativeDamage = true;
        }

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null && boxCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, boxCollider, true);
        }

        // After Instantiate, also tell the server we fired (if networked)
        if (!offlineMode && NetworkManager.Instance != null && NetworkManager.Instance.playerId == ownerPlayerId)
        {
            NetworkManager.Instance.SendShoot(
                firePoint.position.x,
                firePoint.position.y,
                firePoint.rotation.eulerAngles.z
            );
        }
    }
}
