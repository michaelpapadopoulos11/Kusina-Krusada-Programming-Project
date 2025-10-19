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
