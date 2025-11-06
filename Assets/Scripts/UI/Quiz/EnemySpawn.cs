using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class EnemySpawn : MonoBehaviour
{
    public GameObject enemyPrefab; // Reference to the enemy prefab
    public float spawnPointA = -2.4f; // X position where enemies will spawn
    public float spawnPointB = 0.0f; // X position where enemies will spawn
    public float spawnPointC = 2.4f; // X position where enemies will spawn
    public float spawnInterval = 20f; // Base time interval between spawns (at low speeds)
    public float minimumSpawnInterval = 0.5f; // Minimum spawn interval at high speeds (very frequent)
    public float guaranteedSpawnSpeed = 23f; // Speed at which guaranteed frequent spawning begins
    public Transform playerTransform; // assign the main character's transform in inspector (optional)
    public float spawnOffsetZ = 20f; // how far in front of the player to spawn
    public GameObject originalEnemyModel; // optional: reference to an existing enemy model in the scene to clone

    private float timer;
    private float currentSpawnInterval;
    private float originalSpawnInterval;

    void Start()
    {
        originalSpawnInterval = spawnInterval;
        currentSpawnInterval = spawnInterval;
        timer = spawnInterval; // Initialize timer
    }

    void Update()
    {
        // Update spawn interval based on current player speed
        UpdateSpawnInterval();
        
        // Use cached deltaTime for better performance
        timer -= PerformanceHelper.CachedDeltaTime; // Decrease timer by the time passed since last frame

        if (timer <= 0f)
        {
            SpawnEnemy(); // Spawn an enemy
            timer = currentSpawnInterval; // Reset timer with current interval
        }
    }

    void SpawnEnemy()
    {
        // Ensure we have a player transform; try to find by tag if not assigned
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        if (originalEnemyModel != null && playerTransform != null)
        {
            // Create a copy of the original enemy model and place it exactly spawnOffsetZ in front of the player
            Vector3 spawnPosition = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z + spawnOffsetZ);
            Instantiate(originalEnemyModel, spawnPosition, originalEnemyModel.transform.rotation);
            return;
        }

        // Fallback: Determine spawn X (choose one of A/B/C randomly) and instantiate the prefab
        float[] xs = new float[] { spawnPointA, spawnPointB, spawnPointC };
        float spawnX = xs[UnityEngine.Random.Range(0, xs.Length)];

        float spawnZ = transform.position.z; // fallback to this spawner's z
        float spawnY = transform.position.y; // fallback to this spawner's y
        if (playerTransform != null)
        {
            spawnZ = playerTransform.position.z + spawnOffsetZ;
            spawnY = playerTransform.position.y; // spawn at player's Y level by default
        }

        Vector3 finalSpawnPosition = new Vector3(spawnX, spawnY, spawnZ);
        Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Updates the spawn interval based on player speed. 
    /// At speeds >= 23, enemies spawn at guaranteed intervals every 0.5 seconds.
    /// </summary>
    private void UpdateSpawnInterval()
    {
        float currentSpeed = GetCurrentPlayerSpeed();
        
        // If speed is at or above the guaranteed spawn speed threshold
        if (currentSpeed >= guaranteedSpawnSpeed)
        {
            // Use minimum spawn interval for guaranteed frequent spawning
            currentSpawnInterval = minimumSpawnInterval;
        }
        else
        {
            // Gradually reduce spawn interval as speed increases towards the threshold
            float speedProgress = Mathf.Clamp01(currentSpeed / guaranteedSpawnSpeed);
            currentSpawnInterval = Mathf.Lerp(originalSpawnInterval, minimumSpawnInterval, speedProgress);
        }
    }

    /// <summary>
    /// Get the current player speed from GameProgression or Movement component
    /// </summary>
    private float GetCurrentPlayerSpeed()
    {
        // Try to get speed from GameProgression first
        var gameProgression = GameProgression.Instance;
        if (gameProgression != null && gameProgression.playerMovement != null)
        {
            return gameProgression.GetCurrentBaseSpeed();
        }
        
        // Fallback: try to find player and get movement component directly
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }
        
        if (playerTransform != null)
        {
            var movement = playerTransform.GetComponent<Movement>();
            if (movement != null)
            {
                return movement.baseForwardSpeed;
            }
        }
        
        return 5f; // Default fallback speed
    }
}