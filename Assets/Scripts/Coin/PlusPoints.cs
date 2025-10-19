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

        // If there is a Generate component in the scene that wants to know, call its OnCoinDestroyed.
        // We'll try to find a Generate on the root spawner(s) to notify.
        var spawners = GameObject.FindObjectsOfType<Generate>();
        foreach (var s in spawners)
        {
            s.OnCoinDestroyed();
        }

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
        var spawners = GameObject.FindObjectsOfType<Generate>();
        foreach (var s in spawners)
        {
            s.OnCoinDestroyed();
        }

        Destroy(gameObject);
    }
}
