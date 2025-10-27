using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Generate : MonoBehaviour
{
    public GameObject coinPrefab;
    public Transform player;
    public float coinY = 2f;
    [Tooltip("Seconds between automatic spawns (starting value)")]
    public float spawnInterval = 5f;
    [Tooltip("Minimum spawn interval - reached when player reaches max speed")]
    public float minSpawnInterval = 1f;
    [Tooltip("Maximum clones allowed in scene from this spawner")]
    public int maxCoins = 5;
    
    [Header("Consecutive Spawning")]
    [Tooltip("Allow spawning 3 consecutive items in the same lane 10f apart when at max speed")]
    public bool enableConsecutiveSpawning = false;

    private readonly float[] xPositions = { -2.5f, 0.12f, 2.3f };
    // possible Y positions for spawned coins
    public float[] yPositions = new float[] { 2f, 4f };
    private List<GameObject> coins = new List<GameObject>();
    private float timer = 0f;
    private float originalSpawnInterval; // Store the original spawn interval
    private float currentSpawnInterval; // Current calculated spawn interval
    
    // Consecutive spawning variables
    private bool isConsecutiveSpawning = false;
    private int consecutiveCount = 0;
    private int selectedLane = 0;
    private float consecutiveSpacing = 10f;
    // How far behind the player a clone must be before it's destroyed
    public float destroyDistanceBehind = 20f;
    // (Removed unused OnDestroyed spawn cooldown fields — spawning now only occurs on the timer)

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Store the original spawn interval for calculations
        originalSpawnInterval = spawnInterval;
        currentSpawnInterval = spawnInterval;

        // Warn if the assigned coinPrefab is a scene instance rather than a project prefab asset.
        #if UNITY_EDITOR
        if (coinPrefab != null && coinPrefab.scene.IsValid())
        {
            Debug.LogWarning("Generate: coinPrefab appears to be a scene instance. Assign the prefab asset from the Project window to avoid duplicating scene objects.");
        }
        #endif
    }

    void Update()
    {
        // Update spawn interval based on player speed
        UpdateSpawnInterval();

        // Use cached deltaTime for better performance
        timer += PerformanceHelper.CachedDeltaTime;

        // Check if we're at max speed for guaranteed spawning
        bool isAtMaxSpeed = IsPlayerAtMaxSpeed();
        
        // Spawn coins at current interval
        if (timer >= currentSpawnInterval)
        {
            timer = 0f;
            
            // At max speed, guarantee spawn regardless of maxCoins limit
            if (isAtMaxSpeed)
            {
                SpawnCoin();
            }
            // Normal spawning with maxCoins limit
            else if (coins.Count < maxCoins)
            {
                SpawnCoin();
            }
        }

        // Optimized cleanup: iterate backwards and remove null entries efficiently
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            var c = coins[i];
            if (c == null)
            {
                coins.RemoveAt(i);
                continue;
            }

            if (player != null)
            {
                float behindZ = player.position.z - destroyDistanceBehind;
                if (c.transform.position.z < behindZ)
                {
                    Destroy(c);
                    coins.RemoveAt(i);
                }
            }
        }

        // Additional safety sweep: destroy any coin clones in the scene that weren't tracked
        // This covers clones created elsewhere or duplicated unexpectedly.
        if (coinPrefab != null && player != null)
        {
            string cloneName = coinPrefab.name + "(Clone)";
            float behindZAll = player.position.z - destroyDistanceBehind;

            // Find all transforms in the scene (includes all GameObjects)
            var allTransforms = FindObjectsOfType<Transform>();
            for (int i = 0; i < allTransforms.Length; i++)
            {
                var go = allTransforms[i].gameObject;
                if (go == null || !go.activeInHierarchy) continue;

                // Skip the player itself
                if (player != null && go.transform == player) continue;

                // Skip UI elements (they use RectTransform)
                if (go.GetComponent<UnityEngine.RectTransform>() != null) continue;

                // Only consider objects that look like Unity clones (name ends with '(Clone)') or that were explicitly marked by us
                bool nameLooksLikeClone = go.name != null && go.name.EndsWith("(Clone)");
                bool isCoinCloneByName = nameLooksLikeClone && go.name.StartsWith(coinPrefab.name);
                // Consider PlusPoints or Coin components only when the object is actually a clone (avoid removing original scene objects)
                bool isPointByComponent = nameLooksLikeClone && go.GetComponent<PlusPoints>() != null;
                bool isCoinByComponent = nameLooksLikeClone && go.GetComponent<Coin>() != null;
                bool hasCloneMarker = go.GetComponent<CloneMarker>() != null;

                if (isCoinCloneByName || isCoinByComponent || isPointByComponent || hasCloneMarker)
                {
                    // If it's already tracked, skip here — tracked ones were already processed above.
                    if (coins.Contains(go)) continue;

                    // If it's behind the player past threshold OR it's not in the active scene (e.g. under DontDestroyOnLoad), destroy it
                    if (go.transform.position.z < behindZAll || go.scene != SceneManager.GetActiveScene())
                    {
                        Destroy(go);
                    }
                }
            }
        }
    }
    public void OnCoinDestroyed()
    {
        // Early sanity checks
        if (coinPrefab == null)
        {
            Debug.LogWarning("Generate.OnCoinDestroyed: coinPrefab not assigned - cannot spawn replacement.");
            return;
        }
    if (player == null)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Generate.OnCoinDestroyed: player not assigned and not found by tag");
            return;
        }
    }
    // prune any null entries first to get an accurate count
    for (int i = coins.Count - 1; i >= 0; i--)
    {
        if (coins[i] == null)
            coins.RemoveAt(i);
    }

    // We do not spawn replacements here. Spawning is handled only by the timer in Update (SpawnCoin).
}

    void SpawnCoin()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("Generate.SpawnCoin: coinPrefab not assigned — cannot spawn.");
            return;
        }

        bool isAtMaxSpeed = IsPlayerAtMaxSpeed();
        
        // Handle consecutive spawning logic with 25% chance
        if (enableConsecutiveSpawning && isAtMaxSpeed && !isConsecutiveSpawning)
        {
            // 25% chance to start consecutive spawning sequence
            if (Random.Range(0f, 1f) <= 0.25f)
            {
                isConsecutiveSpawning = true;
                consecutiveCount = 0;
                selectedLane = Random.Range(0, xPositions.Length);
                Debug.Log("Starting consecutive spawning sequence (25% chance triggered)");
            }
        }
        
        // Determine spawn parameters
        int lane;
        float baseSpawnDistance = 150f;
        
        if (isConsecutiveSpawning && consecutiveCount < 3)
        {
            // Use selected lane for consecutive spawning
            lane = selectedLane;
            // Spawn items 10f apart in the same lane
            baseSpawnDistance = 150f + (consecutiveCount * consecutiveSpacing);
        }
        else
        {
            // Normal random lane selection
            lane = Random.Range(0, xPositions.Length);
            // Reset consecutive spawning if we've completed the sequence
            if (isConsecutiveSpawning)
            {
                isConsecutiveSpawning = false;
                consecutiveCount = 0;
            }
        }

        float y = coinY;
        if (yPositions != null && yPositions.Length > 0)
            y = yPositions[Random.Range(0, yPositions.Length)];

        Vector3 spawnPos = new Vector3(
            xPositions[lane],
            y,
            player.position.z + baseSpawnDistance
        );

        // For consecutive spawning, skip collision detection to guarantee spawning
        if (!isConsecutiveSpawning || consecutiveCount >= 3)
        {
            // Check if there's already an object within 20f radius in the same lane
            if (IsObjectTooClose(spawnPos, 20f))
            {
                // Don't spawn if there's an object too close (only for normal spawning)
                return;
            }
        }

        // Create the coin
        GameObject coin = CreateCoinInstance(spawnPos);
        if (coin != null)
        {
            coins.Add(coin);
            
            // Update consecutive spawning counter
            if (isConsecutiveSpawning)
            {
                consecutiveCount++;
                Debug.Log($"Consecutive spawn {consecutiveCount}/3 in lane {lane}");
            }
        }
    }

    /// <summary>
    /// Creates a coin instance with all necessary components and setup
    /// </summary>
    private GameObject CreateCoinInstance(Vector3 spawnPos)
    {
        // Use the prefab's original rotation
        GameObject coin = Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation);
        
        // Move the clone into the active scene so it behaves the same as VIRUS clones
        SceneManager.MoveGameObjectToScene(coin, SceneManager.GetActiveScene());
        coin.transform.SetParent(null);

        // Normalize name: Unity can sometimes append multiple (Clone)(Clone). Keep a single (Clone)
        if (coin.name.Contains("(Clone)"))
        {
            var baseName = coin.name.Replace("(Clone)", "");
            coin.name = baseName + "(Clone)";
        }

        // Remove any Generate components on the clone or its children to avoid recursive spawning
        var gens = coin.GetComponentsInChildren<Generate>(true);
        foreach (var g in gens)
        {
            if (g != this)
                Destroy(g);
        }

        // Attach marker so this object is recognized as a spawned clone
        if (coin.GetComponent<CloneMarker>() == null)
            coin.AddComponent<CloneMarker>();

        if (coin.GetComponent<SpawnedEntity>() == null)
        {
            var se = coin.AddComponent<SpawnedEntity>();
            se.player = this.player;
            se.destroyDistanceBehind = this.destroyDistanceBehind;
        }

        return coin;
    }

    /// <summary>
    /// Checks if there's already an object within the specified radius of the spawn position.
    /// Focuses on the same lane (X position) and checks Z-axis distance.
    /// </summary>
    /// <param name="spawnPos">The intended spawn position</param>
    /// <param name="checkRadius">The radius to check for existing objects</param>
    /// <returns>True if an object is too close, false otherwise</returns>
    private bool IsObjectTooClose(Vector3 spawnPos, float checkRadius)
    {
        // Define lane tolerance - objects within this X range are considered in the same lane
        float laneWidth = 1.0f;

        // Check all tracked coins first
        foreach (var coin in coins)
        {
            if (coin == null) continue;

            Vector3 coinPos = coin.transform.position;

            // Check if in the same lane (similar X position)
            if (Mathf.Abs(coinPos.x - spawnPos.x) <= laneWidth)
            {
                // Check Z-axis distance
                if (Mathf.Abs(coinPos.z - spawnPos.z) <= checkRadius)
                {
                    return true; // Object too close
                }
            }
        }

        // Also check for any other spawned objects in the scene that might not be tracked
        if (coinPrefab != null)
        {
            // Find all objects with similar components or clone names
            var allTransforms = FindObjectsOfType<Transform>();
            for (int i = 0; i < allTransforms.Length; i++)
            {
                var go = allTransforms[i].gameObject;
                if (go == null || !go.activeInHierarchy) continue;

                // Skip the player
                if (player != null && go.transform == player) continue;

                // Skip UI elements
                if (go.GetComponent<UnityEngine.RectTransform>() != null) continue;

                // Check if this is a spawned object we care about
                bool isRelevantObject = false;
                
                // Check if it's a coin clone
                if (go.name != null && go.name.EndsWith("(Clone)") && go.name.StartsWith(coinPrefab.name))
                    isRelevantObject = true;
                
                // Check if it has relevant components
                if (go.GetComponent<PlusPoints>() != null || go.GetComponent<Coin>() != null || go.GetComponent<CloneMarker>() != null)
                    isRelevantObject = true;

                if (isRelevantObject)
                {
                    Vector3 objPos = go.transform.position;

                    // Check if in the same lane
                    if (Mathf.Abs(objPos.x - spawnPos.x) <= laneWidth)
                    {
                        // Check Z-axis distance
                        if (Mathf.Abs(objPos.z - spawnPos.z) <= checkRadius)
                        {
                            return true; // Object too close
                        }
                    }
                }
            }
        }

        return false; // No objects too close
    }

    /// <summary>
    /// Updates the current spawn interval based on the player's movement speed.
    /// Spawn interval decreases as speed increases, reaching minSpawnInterval when max speed is reached.
    /// </summary>
    private void UpdateSpawnInterval()
    {
        // Try to get the GameProgression instance to access speed information
        var gameProgression = GameProgression.Instance;
        if (gameProgression != null && gameProgression.playerMovement != null)
        {
            // Get current, original, and max speeds from GameProgression
            float currentSpeed = gameProgression.GetCurrentBaseSpeed();
            float originalSpeed = gameProgression.GetOriginalSpeed();
            float maxSpeed = gameProgression.maxSpeed;
            
            // Calculate progress from 0 to 1 based on speed increase
            float speedProgress = 0f;
            if (maxSpeed > originalSpeed)
            {
                speedProgress = Mathf.Clamp01((currentSpeed - originalSpeed) / (maxSpeed - originalSpeed));
            }

            // Interpolate between original spawn interval and minimum spawn interval
            currentSpawnInterval = Mathf.Lerp(originalSpawnInterval, minSpawnInterval, speedProgress);
            
            // Debug log for testing (can be removed later)
            // Debug.Log($"Speed: {currentSpeed:F1}/{maxSpeed} | Spawn Interval: {currentSpawnInterval:F2}s");
        }
        else
        {
            // Fallback: try to get player movement directly
            if (player != null)
            {
                var movement = player.GetComponent<Movement>();
                if (movement != null)
                {
                    // Assume starting speed is 5f and max speed is 20f if GameProgression isn't available
                    float assumedOriginalSpeed = 5f;
                    float assumedMaxSpeed = 20f;
                    float currentSpeed = movement.baseForwardSpeed;
                    
                    float speedProgress = Mathf.Clamp01((currentSpeed - assumedOriginalSpeed) / (assumedMaxSpeed - assumedOriginalSpeed));
                    currentSpawnInterval = Mathf.Lerp(originalSpawnInterval, minSpawnInterval, speedProgress);
                }
                else
                {
                    // No movement component found, use original interval
                    currentSpawnInterval = originalSpawnInterval;
                }
            }
            else
            {
                // No player found, use original interval
                currentSpawnInterval = originalSpawnInterval;
            }
        }
    }

    /// <summary>
    /// Get the current spawn interval for UI display or other systems
    /// </summary>
    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }

    /// <summary>
    /// Check if the player has reached the guaranteed spawn speed threshold (40)
    /// </summary>
    private bool IsPlayerAtMaxSpeed()
    {
        const float guaranteedSpawnSpeed = 40f;
        
        var gameProgression = GameProgression.Instance;
        if (gameProgression != null && gameProgression.playerMovement != null)
        {
            float currentSpeed = gameProgression.GetCurrentBaseSpeed();
            return currentSpeed >= guaranteedSpawnSpeed;
        }
        
        // Fallback: check player movement directly
        if (player != null)
        {
            var movement = player.GetComponent<Movement>();
            if (movement != null)
            {
                return movement.baseForwardSpeed >= guaranteedSpawnSpeed;
            }
        }
        
        return false;
    }

}