using UnityEngine;
using System.Collections.Generic;

public class Generate : MonoBehaviour
{
    public GameObject coinPrefab;
    public Transform player;
    public float coinY = 2f;
    [Tooltip("Seconds between automatic spawns")]
    public float spawnInterval = 5f;
    [Tooltip("Maximum clones allowed in scene from this spawner")]
    public int maxCoins = 5;

    private readonly float[] xPositions = { -2.5f, 0.12f, 2.3f };
    private List<GameObject> coins = new List<GameObject>();
    private float timer = 0f;
    // How far behind the player a clone must be before it's destroyed
    public float destroyDistanceBehind = 20f;
    // Prevent rapid respawn storms when many coins are destroyed at once
    [Tooltip("Minimum seconds between spawns triggered by OnCoinDestroyed to avoid flooding")]
    public float onDestroyedSpawnCooldown = 0.2f;
    private float lastOnDestroyedSpawnTime = -Mathf.Infinity;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Spawn coins at interval if not already at max
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (coins.Count < maxCoins)
                SpawnCoin();
        }

        // Cleanup null coins (destroyed via collision or self-destroy)
        // Remove destroyed (null) entries and explicit out-of-range coins tracked by this spawner
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            var c = coins[i];
            if (c == null)
            {
                coins.RemoveAt(i);
                continue;
            }

            if (player != null)
            {
                float behindZ = player.position.z - destroyDistanceBehind;
                if (c.transform.position.z < behindZ)
                {
                    Destroy(c);
                    coins.RemoveAt(i);
                }
            }
        }

        // Additional safety sweep: destroy any coin clones in the scene that weren't tracked
        // This covers clones created elsewhere or duplicated unexpectedly.
        if (coinPrefab != null && player != null)
        {
            string cloneName = coinPrefab.name + "(Clone)";
            float behindZAll = player.position.z - destroyDistanceBehind;

            // Find all transforms in the scene (includes all GameObjects)
            var allTransforms = FindObjectsOfType<Transform>();
            for (int i = 0; i < allTransforms.Length; i++)
            {
                var go = allTransforms[i].gameObject;
                if (go == null || !go.activeInHierarchy) continue;

                // Skip the player itself
                if (player != null && go.transform == player) continue;

                // Skip UI elements (they use RectTransform)
                if (go.GetComponent<UnityEngine.RectTransform>() != null) continue;

                bool isCoinCloneByName = string.Equals(go.name, cloneName, System.StringComparison.Ordinal);
                bool isCoinByComponent = go.GetComponent<Coin>() != null;

                if (isCoinCloneByName || isCoinByComponent)
                {
                    // If it's already tracked, skip here — tracked ones were already processed above.
                    if (coins.Contains(go)) continue;

                    // If it's behind the player past threshold, destroy it
                    if (go.transform.position.z < behindZAll)
                    {
                        Destroy(go);
                    }
                }
            }
        }
    }
    public void OnCoinDestroyed()
{
    // Early sanity checks
    if (coinPrefab == null)
    {
        Debug.LogWarning("Generate.OnCoinDestroyed: coinPrefab not assigned");
        return;
    }
    if (player == null)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Generate.OnCoinDestroyed: player not assigned and not found by tag");
            return;
        }
    }

    // prune any null entries first to get an accurate count
    for (int i = coins.Count - 1; i >= 0; i--)
    {
        if (coins[i] == null)
            coins.RemoveAt(i);
    }

    // Respect maxCoins so we don't spawn more than allowed
    if (coins.Count >= maxCoins)
        return;

    // Enforce a tiny cooldown to avoid many simultaneous spawn calls
    if (Time.time - lastOnDestroyedSpawnTime < onDestroyedSpawnCooldown)
        return;
    lastOnDestroyedSpawnTime = Time.time;

    // Spawn a replacement coin ahead of the player (use same distance as SpawnCoin)
    int lane = Random.Range(0, xPositions.Length);
    Vector3 spawnPos = new Vector3(
        xPositions[lane],
        coinY,
        player.position.z + 150f
    );

    GameObject prefabToUse = coinPrefab != null ? coinPrefab : this.gameObject;
    GameObject coin = Instantiate(prefabToUse, spawnPos, prefabToUse.transform.rotation);

    // If we instantiated this spawner's own GameObject, remove Generate from the clone to prevent recursive spawning
    if (prefabToUse == this.gameObject)
    {
        var gen = coin.GetComponent<Generate>();
        if (gen != null) Destroy(gen);
    }

    // Ensure the clone has a SpawnedEntity to self-manage lifetime
    if (coin.GetComponent<SpawnedEntity>() == null)
    {
        var se = coin.AddComponent<SpawnedEntity>();
        se.player = this.player;
        se.destroyDistanceBehind = this.destroyDistanceBehind;
    }

    coins.Add(coin);
}

    void SpawnCoin()
    {
        int lane = Random.Range(0, xPositions.Length);
        Vector3 spawnPos = new Vector3(
            xPositions[lane],
            coinY,
            player.position.z + 150f
        );

        // Use the prefab's original rotation
        GameObject prefabToUse = coinPrefab != null ? coinPrefab : this.gameObject;
        GameObject coin = Instantiate(prefabToUse, spawnPos, prefabToUse.transform.rotation);

        if (prefabToUse == this.gameObject)
        {
            var gen = coin.GetComponent<Generate>();
            if (gen != null) Destroy(gen);
        }

        if (coin.GetComponent<SpawnedEntity>() == null)
        {
            var se = coin.AddComponent<SpawnedEntity>();
            se.player = this.player;
            se.destroyDistanceBehind = this.destroyDistanceBehind;
        }

        coins.Add(coin);
    }

}