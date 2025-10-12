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
        timer -= Time.deltaTime; // Decrease timer by the time passed since last frame

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
}