using UnityEngine;

// Attach this to pickup objects named "Point" to give points on collision.
public class PlusPoints : MonoBehaviour
{

    AudioManager audioManager;

    [Tooltip("Points to add when collected")]
    public int points = 50;

    // Optional: tag of the object that collects points (usually Player)
    public string collectorTag = "Player";

    private void Awake() {
    audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // If collectorTag is set, check tag first. If empty, accept any collision.
        if (!string.IsNullOrEmpty(collectorTag))
        {
            if (!other.CompareTag(collectorTag)) return;
        }

        audioManager.setSFXPitch(UnityEngine.Random.Range(1f, 2f));
        audioManager.playSFX(audioManager.fruit_collected); 
        ScoreManager.AddPoints(points);

        // Destroy the pickup on collection so clones are removed immediately, but only if this is a clone
        if (gameObject.name != null && gameObject.name.EndsWith("(Clone)"))
            Destroy(gameObject);
        else if (gameObject.GetComponent<CloneMarker>() != null)
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

        audioManager.playSFX(audioManager.fruit_collected); // Play sound after tag check
        ScoreManager.AddPoints(points);

        // Destroy the pickup on collection so clones are removed immediately, but only if this is a clone
        if (gameObject.name != null && gameObject.name.EndsWith("(Clone)"))
            Destroy(gameObject);
        else if (gameObject.GetComponent<CloneMarker>() != null)
            Destroy(gameObject);
    }
}
