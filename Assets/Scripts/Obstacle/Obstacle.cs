using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

    // Cached colliders on this obstacle
    private Collider[] _colliders;

    void Start() {
        // Cache colliders
        _colliders = GetComponentsInChildren<Collider>(true);

        // Set all colliders as triggers for reliable collision detection
        foreach (var c in _colliders)
        {
            c.isTrigger = true;
        }
        Debug.Log($"Obstacle Start: set {_colliders.Length} collider(s) to trigger");
    }

    // Removed the Update method that was dynamically changing collider types
    // This was causing collision detection issues

    // Handles trigger-based collisions - simplified approach
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter called with: {other.name}, tag: {other.tag}");
        Movement player = other.GetComponent<Movement>();
        if (player != null)
        {
            Debug.Log($"Player found! Invincible: {player.isInvincible}, GameOver: {player.isGameOver}");
            
            // Simply check invincibility state at collision time
            if (player.isInvincible)
            {
                Debug.Log("Player is invincible - destroying obstacle");
                Destroy(gameObject);
            }
            else if (!player.isGameOver) // Only trigger if game isn't already over
            {
                Debug.Log("Player is NOT invincible - calling TriggerGameOver");
                player.TriggerGameOver();
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("No Movement component found on colliding object");
        }
    }
}
