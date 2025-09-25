using UnityEngine;

public class Coin : MonoBehaviour
{
    private float destroyOffset = 10f; // Distance behind player before destruction
    private float lifetime = 15f;      // Time before self-destruction
    private Transform player;
    private Generate spawner;
    private float timer;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawner = FindObjectOfType<Generate>();
        timer = 0f; // Initialize timer
    }

    void Update()
    {
        if (player == null) return;

        // Increment timer
        timer += Time.deltaTime;

        // Destroy if far behind player
        if (transform.position.z < player.position.z - destroyOffset)
        {
            NotifyAndDestroy();
        }

        // Destroy after lifetime expires
        if (timer >= lifetime)
        {
            NotifyAndDestroy();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NotifyAndDestroy();
        }
    }

    void NotifyAndDestroy()
    {
        // Notify spawner to spawn a new coin
        if (spawner != null)
        {
            spawner.OnCoinDestroyed();
        }

        Destroy(gameObject);
    }
}
