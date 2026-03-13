using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;   // How fast the bullet moves
    public float lifetime = 3f; // How long before it disappears
    public float ignoreCollisionTime = 0.1f; // Time to ignore collisions after spawning
    public int maxBounces = 5;  // How many times the bullet can bounce off walls
    public int damage = 100;    // Damage dealt to tanks (one-shot kill by default)

    private Rigidbody2D rb;
    private float spawnTime;
    private GameObject owner; // The tank that shot this bullet
    private int bounceCount = 0;
    private float lastBounceTime = 0f; // Track when we last bounced to prevent multiple bounces in one frame
    private const float BOUNCE_COOLDOWN = 0.05f; // Minimum time between bounces

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet prefab is missing Rigidbody2D!");
            return;
        }

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.gravityScale = 0f;
        
        rb.linearVelocity = transform.up * speed;
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    public void SetOwner(GameObject tank)
    {
        owner = tank;
    }

    bool ShouldIgnoreCollision(Collision2D collision)
    {
        if (Time.time - spawnTime < ignoreCollisionTime)
            return true;
        
        if (owner != null && collision.gameObject == owner)
            return true;
        
        return false;
    }

    void HandleWallBounce(Collision2D collision)
    {
        if (rb == null || collision.contacts.Length == 0)
            return;

        if (Time.time - lastBounceTime < BOUNCE_COOLDOWN)
            return;

        Vector2 normal = collision.contacts[0].normal;
        Vector2 reflectedDirection = Vector2.Reflect(rb.linearVelocity.normalized, normal);
        Vector2 reflected = reflectedDirection * speed;
        
        rb.linearVelocity = reflected;
        
        float angle = Mathf.Atan2(reflected.y, reflected.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.position += (Vector3)(normal * 0.1f);

        lastBounceTime = Time.time;
        bounceCount++;
        
        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (ShouldIgnoreCollision(collision))
            return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            HandleWallBounce(collision);
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            TankHealth tankHealth = collision.gameObject.GetComponent<TankHealth>();
            if (tankHealth != null && !tankHealth.IsDead())
            {
                tankHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !ShouldIgnoreCollision(collision))
        {
            HandleWallBounce(collision);
        }
    }
}