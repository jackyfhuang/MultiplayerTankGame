using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;   // How fast the bullet moves
    public float lifetime = 3f; // How long before it disappears
    public float ignoreCollisionTime = 0.1f; // Time to ignore collisions after spawning

    private Rigidbody2D rb;
    private float spawnTime;
    private GameObject owner; // The tank that shot this bullet

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet prefab is missing Rigidbody2D!");
            return;
        }

        rb.linearVelocity = transform.up * speed;
        spawnTime = Time.time;

        // Destroy bullet after lifetime seconds
        Destroy(gameObject, lifetime);
    }

    public void SetOwner(GameObject tank)
    {
        owner = tank;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collisions for a short time after spawning (prevents immediate collision with tank)
        if (Time.time - spawnTime < ignoreCollisionTime)
            return;

        // Ignore collision with the tank that shot this bullet
        if (owner != null && collision.gameObject == owner)
            return;

        // Destroy bullet when it hits anything else
        Destroy(gameObject);
    }
}