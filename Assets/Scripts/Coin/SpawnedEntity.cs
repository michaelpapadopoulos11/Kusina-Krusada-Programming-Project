using UnityEngine;

// Small helper that destroys its GameObject when it falls behind the player by a threshold.
public class SpawnedEntity : MonoBehaviour
{
    [HideInInspector] public Transform player;
    [HideInInspector] public float destroyDistanceBehind = 20f;

    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null) return;

        if (transform.position.z < player.position.z - destroyDistanceBehind)
        {
            // Notify spawners (so they can choose to replace)
            var spawners = GameObject.FindObjectsOfType<Generate>();
            foreach (var s in spawners)
            {
                s.OnCoinDestroyed();
            }

            Destroy(gameObject);
        }
    }
}
