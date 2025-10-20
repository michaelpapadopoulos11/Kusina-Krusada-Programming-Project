using UnityEngine;

// Small helper that destroys its GameObject when it falls behind the player by a threshold.
public class SpawnedEntity : MonoBehaviour
{
    [HideInInspector] public Transform player;
    [HideInInspector] public float destroyDistanceBehind = 20f;
    
    // Performance optimization: cache player lookup and reduce update frequency
    private float checkInterval = 0.5f; // Check every 0.5 seconds instead of every frame
    private float nextCheck = 0f;

    void Update()
    {
        // Reduce update frequency for better performance
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        // Simple Z-axis check for runner game (more efficient than full distance calculation)
        if (transform.position.z < player.position.z - destroyDistanceBehind)
        {
            // Only destroy if this is a spawned clone
            if (gameObject.name != null && gameObject.name.EndsWith("(Clone)"))
            {
                Destroy(gameObject);
            }
            else if (gameObject.GetComponent<CloneMarker>() != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
