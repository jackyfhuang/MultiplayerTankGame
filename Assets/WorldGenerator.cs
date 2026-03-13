using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public float worldWidth = 20f;
    public float worldHeight = 15f;
    public float wallThickness = 0.5f;
    
    [Header("Wall Settings")]
    public GameObject wallPrefab; // Optional: assign a wall prefab, or it will create basic walls
    
    [Header("Tank Settings")]
    public GameObject tankPrefab; // Assign your PlayerTank prefab here
    public Vector2 tankSpawnPosition = Vector2.zero; // Center of the world by default
    
    [Header("Camera Settings")]
    public bool setupCameraFollow = true;
    
    [Header("Generation Settings")]
    public bool generateOnStart = true;
    public bool clearExistingWalls = true;
    
    private GameObject wallParent;
    private GameObject spawnedTank;
    private const string WALL_TAG = "Wall";
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateWorld();
        }
    }
    
    public void GenerateWorld()
    {
        if (clearExistingWalls)
        {
            ClearExistingWalls();
        }
        
        wallParent = new GameObject("Walls");
        
        CreateWall("TopWall", new Vector2(0, worldHeight / 2), new Vector2(worldWidth, wallThickness));
        CreateWall("BottomWall", new Vector2(0, -worldHeight / 2), new Vector2(worldWidth, wallThickness));
        CreateWall("LeftWall", new Vector2(-worldWidth / 2, 0), new Vector2(wallThickness, worldHeight));
        CreateWall("RightWall", new Vector2(worldWidth / 2, 0), new Vector2(wallThickness, worldHeight));
        
        spawnedTank = SpawnTank();
        
        if (setupCameraFollow)
        {
            SetupCameraFollow();
        }
        
        Debug.Log($"World generated: {worldWidth}x{worldHeight} with tank at {tankSpawnPosition}");
    }
    
    void CreateWall(string name, Vector2 position, Vector2 size)
    {
        GameObject wall;
        
        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, position, Quaternion.identity);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.rotation = Quaternion.identity;
            
            BoxCollider2D prefabCollider = wallPrefab.GetComponent<BoxCollider2D>();
            SpriteRenderer prefabRenderer = wallPrefab.GetComponent<SpriteRenderer>();
            
            Vector2 originalSize = Vector2.one;
            if (prefabCollider != null && prefabCollider.size.magnitude > 0)
            {
                originalSize = prefabCollider.size;
            }
            else if (prefabRenderer != null && prefabRenderer.sprite != null)
            {
                originalSize = prefabRenderer.sprite.bounds.size;
            }
            
            if (originalSize.x > 0 && originalSize.y > 0)
            {
                wall.transform.localScale = new Vector3(size.x / originalSize.x, size.y / originalSize.y, 1f);
            }
            else
            {
                wall.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
        }
        else
        {
            wall = new GameObject(name);
            wall.transform.position = position;
            
            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
            sr.sprite = CreateWallSprite(size);
            sr.sortingOrder = 0;
            
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
            
            Rigidbody2D rb = wall.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        BoxCollider2D collider2D = wall.GetComponent<BoxCollider2D>();
        if (collider2D == null)
        {
            collider2D = wall.AddComponent<BoxCollider2D>();
        }
        
        if (wallPrefab != null)
        {
            Vector3 scale = wall.transform.localScale;
            collider2D.size = new Vector2(size.x / scale.x, size.y / scale.y);
        }
        else
        {
            collider2D.size = size;
        }
        collider2D.isTrigger = false;
        
        Rigidbody2D rb2D = wall.GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            rb2D = wall.AddComponent<Rigidbody2D>();
        }
        rb2D.bodyType = RigidbodyType2D.Static;
        
        wall.tag = WALL_TAG;
        wall.transform.SetParent(wallParent.transform);
    }
    
    Sprite CreateWallSprite(Vector2 size)
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(size.x * 100));
        int height = Mathf.Max(1, Mathf.RoundToInt(size.y * 100));
        
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.gray;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
    
    GameObject SpawnTank()
    {
        if (tankPrefab == null)
        {
            Debug.LogError("WorldGenerator: Tank Prefab is not assigned!");
            return null;
        }
        
        float padding = 1f;
        float clampedX = Mathf.Clamp(tankSpawnPosition.x, -worldWidth / 2 + padding, worldWidth / 2 - padding);
        float clampedY = Mathf.Clamp(tankSpawnPosition.y, -worldHeight / 2 + padding, worldHeight / 2 - padding);
        Vector2 safeSpawnPosition = new Vector2(clampedX, clampedY);
        
        GameObject tank = Instantiate(tankPrefab, safeSpawnPosition, Quaternion.identity);
        tank.name = "PlayerTank";
        
        Debug.Log($"Tank spawned at {safeSpawnPosition}");
        return tank;
    }
    
    void SetupCameraFollow()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("WorldGenerator: Main Camera not found. Camera follow not set up.");
            return;
        }
        
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        if (spawnedTank != null)
        {
            cameraFollow.target = spawnedTank.transform;
            Debug.Log("Camera follow set up for spawned tank.");
        }
        else
        {
            Debug.LogWarning("WorldGenerator: No tank spawned. Camera follow target not set.");
        }
    }
    
    void ClearExistingWalls()
    {
        GameObject[] existingWalls = GameObject.FindGameObjectsWithTag(WALL_TAG);
        foreach (GameObject wall in existingWalls)
        {
            DestroyImmediate(wall);
        }
        
        GameObject wallsParent = GameObject.Find("Walls");
        if (wallsParent != null)
        {
            DestroyImmediate(wallsParent);
        }
    }
}
