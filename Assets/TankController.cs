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
        if (IsDead())
            return;
        
        if (Input.GetKey(KeyCode.Space) && Time.time > nextFire)
        {
            Shoot();
            nextFire = Time.time + fireRate;
        }
    }

    void FixedUpdate()
    {
        if (IsDead())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        transform.Rotate(Vector3.forward * -rotateInput * rotateSpeed * Time.fixedDeltaTime);
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
        }
        
        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null && boxCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, boxCollider, true);
        }
    }
}