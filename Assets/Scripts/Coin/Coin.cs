using UnityEngine;

public class Coin : MonoBehaviour
{
    public float destroyOffset = 10f; // destroy if too far behind
    private Transform player;
    private Generate spawner;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawner = FindObjectOfType<Generate>(); // reference the spawner
    }

    void Update()
    {
        // Destroy if far behind the player
        if (player != null && transform.position.z < player.position.z - destroyOffset)
        {
            NotifyAndDestroy();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Destroy if collided with the player
            NotifyAndDestroy();
            
        }
    }

    void NotifyAndDestroy()
    {
        // Notify spawner to spawn a new coin
        if (spawner != null)
            spawner.OnCoinDestroyed();

        Destroy(gameObject);
    }
}
