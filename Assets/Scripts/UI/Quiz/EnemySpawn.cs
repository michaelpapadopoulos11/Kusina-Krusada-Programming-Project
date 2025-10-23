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
    public float spawnInterval = 20f; // Time interval between spawns
    public Transform playerTransform; // assign the main character's transform in inspector (optional)
    public float spawnOffsetZ = 20f; // how far in front of the player to spawn
    public GameObject originalEnemyModel; // optional: reference to an existing enemy model in the scene to clone

    private float timer;

    void Start()
    {
        timer = spawnInterval; // Initialize timer
    }

    void Update()
    {
        // Use cached deltaTime for better performance
        timer -= PerformanceHelper.CachedDeltaTime; // Decrease timer by the time passed since last frame

        if (timer <= 0f)
        {
            SpawnEnemy(); // Spawn an enemy
            timer = spawnInterval; // Reset timer
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
            
            // Check if there's already an object within 20f radius in the same lane
            if (IsObjectTooClose(spawnPosition, 20f))
            {
                // Don't spawn if there's an object too close
                return;
            }
            
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
        
        // Check if there's already an object within 20f radius in the same lane
        if (IsObjectTooClose(finalSpawnPosition, 20f))
        {
            // Don't spawn if there's an object too close
            return;
        }
        
        Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity);
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

        // Find all active GameObjects in the scene
        var allTransforms = FindObjectsOfType<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            var go = allTransforms[i].gameObject;
            if (go == null || !go.activeInHierarchy) continue;

            // Skip the player
            if (playerTransform != null && go.transform == playerTransform) continue;

            // Skip UI elements
            if (go.GetComponent<UnityEngine.RectTransform>() != null) continue;

            // Skip this spawner
            if (go.transform == this.transform) continue;

            // Check if this is a relevant object (enemies, coins, power-ups, etc.)
            bool isRelevantObject = false;
            
            // Check for enemy-related components or names
            if (enemyPrefab != null && go.name != null && go.name.Contains(enemyPrefab.name))
                isRelevantObject = true;
            
            // Check for other game objects that should be considered for spacing
            if (go.GetComponent<PlusPoints>() != null || 
                go.GetComponent<Coin>() != null || 
                go.GetComponent<CloneMarker>() != null ||
                go.name != null && go.name.EndsWith("(Clone)"))
                isRelevantObject = true;

            // Check for power-ups
            if (go.GetComponent<Invincibility_Powerup>() != null)
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

        return false; // No objects too close
    }
}