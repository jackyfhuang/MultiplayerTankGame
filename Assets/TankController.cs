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

    void Update()
    {
        // Get input
        float moveInput = Input.GetAxis("Vertical");   // W/S or Up/Down
        float rotateInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // Move forward/backward
        transform.Translate(Vector2.up * moveInput * moveSpeed * Time.deltaTime);

        // Rotate
        transform.Rotate(Vector3.forward * -rotateInput * rotateSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space) && Time.time > nextFire)
        {
            Shoot();
            nextFire = Time.time + fireRate;
        }
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
    }
}