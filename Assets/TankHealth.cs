using UnityEngine;

public class TankHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Death Settings")]
    public bool respawnOnDeath = false;
    public float respawnDelay = 2f;
    public Vector3 respawnPosition = Vector3.zero;
    
    private bool isDead = false;
    private TankController controller;
    private SpriteRenderer renderer;
    private Collider2D collider;
    
    void OnEnable()
    {
        NetworkManager.OnPlayerHitReceived += HandleSyncedHit;
    }

    void OnDisable()
    {
        NetworkManager.OnPlayerHitReceived -= HandleSyncedHit;
    }

    void HandleSyncedHit(string victimPlayerId, int damageAmount)
    {
        string myId = GetNetworkPlayerId();
        if (string.IsNullOrEmpty(myId) || myId != victimPlayerId)
            return;
        TakeDamage(damageAmount);
    }

    string GetNetworkPlayerId()
    {
        TankController tc = GetComponent<TankController>();
        if (tc != null && !string.IsNullOrEmpty(tc.ownerPlayerId))
            return tc.ownerPlayerId;
        RemoteTankController rtc = GetComponent<RemoteTankController>();
        if (rtc != null && !string.IsNullOrEmpty(rtc.remotePlayerId))
            return rtc.remotePlayerId;
        return null;
    }

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<TankController>();
        renderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;
            
        currentHealth -= damage;
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead)
            return;
            
        isDead = true;
        currentHealth = 0;
        
        Debug.Log($"{gameObject.name} has been destroyed!");
        
        if (controller != null)
            controller.enabled = false;
        
        if (renderer != null)
            renderer.enabled = false;
        
        if (collider != null)
            collider.enabled = false;
        
        // Handle respawn if enabled
        if (respawnOnDeath)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            // Destroy the tank after a delay
            Destroy(gameObject, 1f);
        }
    }
    
    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        
        transform.position = respawnPosition;
        
        if (controller != null)
            controller.enabled = true;
        
        if (renderer != null)
            renderer.enabled = true;
        
        if (collider != null)
            collider.enabled = true;
        
        Debug.Log($"{gameObject.name} has respawned!");
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}
