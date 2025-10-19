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
            Destroy(gameObject);
        }
    }
}
