using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

    // If true, Start() will ensure all colliders are non-trigger. You can set to false
    // if you want to manage collider settings in the editor instead.
    public bool enforceNonTrigger = true;

    // Cached colliders on this obstacle
    private Collider[] _colliders;
    // Cached reference to player Movement (assumes single player)
    private Movement _player;
    // Current isTrigger state we've applied to colliders
    private bool _currentIsTrigger = false;

    void Start() {
        // Cache colliders
        _colliders = GetComponentsInChildren<Collider>(true);

        // Ensure colliders are non-trigger by default if enforcement is on
        if (enforceNonTrigger)
        {
            foreach (var c in _colliders)
            {
                c.isTrigger = false;
            }
            _currentIsTrigger = false;
            Debug.Log($"Obstacle Start: set {_colliders.Length} collider(s) to non-trigger");
        }

        // Cache player reference (finds the first Movement in scene)
        _player = FindObjectOfType<Movement>();
    }

    void Update()
    {
        if (_player == null)
        {
            // Try to find player if it wasn't present at Start
            _player = FindObjectOfType<Movement>();
            if (_player == null) return;
        }

        bool desiredIsTrigger = _player.isInvincible;
        if (desiredIsTrigger != _currentIsTrigger)
        {
            // Apply new trigger state to all colliders
            foreach (var c in _colliders)
            {
                c.isTrigger = desiredIsTrigger;
            }
            _currentIsTrigger = desiredIsTrigger;
            Debug.Log($"Obstacle: set colliders isTrigger = {desiredIsTrigger}");

            // If we just switched to trigger mode and the player is overlapping, destroy immediately
            if (desiredIsTrigger)
            {
                foreach (var c in _colliders)
                {
                    // Quick overlap check: if player's position is inside collider bounds, destroy
                    if (c.bounds.Contains(_player.transform.position))
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
    }

    // Handles trigger-based collisions (when colliders are triggers)
    void OnTriggerEnter(Collider other)
    {
        Movement player = other.GetComponent<Movement>();
        if (player != null)
        {
            if (player.isInvincible)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Player hit obstacle while vulnerable (trigger)");
            }
        }
    }

    // Handles normal physics collisions (when colliders are not triggers)
    void OnCollisionEnter(Collision collision)
    {
        Movement player = collision.gameObject.GetComponent<Movement>();
        if (player != null)
        {
            if (player.isInvincible)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Player hit obstacle while vulnerable (collision)");
            }
        }
    }
}
