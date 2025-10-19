using UnityEngine;

// Attach this to pickup objects named "Point" to give points on collision.
public class PlusPoints : MonoBehaviour
{
    [Tooltip("Points to add when collected")]
    public int points = 50;

    // Optional: tag of the object that collects points (usually Player)
    public string collectorTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // If collectorTag is set, check tag first. If empty, accept any collision.
        if (!string.IsNullOrEmpty(collectorTag))
        {
            if (!other.CompareTag(collectorTag)) return;
        }

        ScoreManager.AddPoints(points);

        // Destroy the pickup on collection so clones are removed immediately
        Destroy(gameObject);
    }

    // Support physics collisions (2D if used) - optional
    void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        if (!string.IsNullOrEmpty(collectorTag))
        {
            if (!collision.gameObject.CompareTag(collectorTag)) return;
        }

        ScoreManager.AddPoints(points);

        // Destroy the pickup on collection so clones are removed immediately
        Destroy(gameObject);
    }
}
