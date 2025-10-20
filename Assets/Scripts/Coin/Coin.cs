using UnityEngine;

public class Coin : MonoBehaviour
{
    private float destroyOffset = 10f; // Distance behind player before destruction
    private float lifetime = 15f;      // Time before self-destruction
    private Transform player;
    private Generate spawner;
    private float timer;
    
    // Performance optimization: cache squared distance for faster calculations
    private float destroyOffsetSqr;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawner = FindObjectOfType<Generate>();
        timer = 0f; // Initialize timer
        destroyOffsetSqr = destroyOffset * destroyOffset; // Cache squared distance
    }

    void Update()
    {
        if (player == null) return;

        // Use cached deltaTime for better performance
        timer += PerformanceHelper.CachedDeltaTime;

        // Optimized distance check using squared distance (no expensive square root)
        float deltaZ = transform.position.z - player.position.z;
        if (deltaZ < -destroyOffset) // Simple Z-axis check is sufficient for runner game
        {
            NotifyAndDestroy();
            return;
        }

        // Destroy after lifetime expires
        if (timer >= lifetime)
        {
            NotifyAndDestroy();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NotifyAndDestroy();
        }
    }

    void NotifyAndDestroy()
    {
        // Notify spawner to spawn a new coin
        if (spawner != null)
        {
            spawner.OnCoinDestroyed();
        }

        Destroy(gameObject);
    }
}
