using UnityEngine;
using UnityEngine.SceneManagement;
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
    // (Removed unused OnDestroyed spawn cooldown fields — spawning now only occurs on the timer)

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

                bool isCoinCloneByName = string.Equals(go.name, cloneName, System.StringComparison.Ordinal) || go.name.StartsWith(coinPrefab.name);
                // Consider PlusPoints as the marker component for point-like pickups
                bool isPointByComponent = go.GetComponent<PlusPoints>() != null;
                bool isCoinByComponent = go.GetComponent<Coin>() != null;

                if (isCoinCloneByName || isCoinByComponent || isPointByComponent)
                {
                    // If it's already tracked, skip here — tracked ones were already processed above.
                    if (coins.Contains(go)) continue;

                    // Don't destroy the original scene object that is explicitly named "Point"
                    if (go.name == coinPrefab.name)
                        continue;

                    // If it's behind the player past threshold OR it's not in the active scene (e.g. under DontDestroyOnLoad), destroy it
                    if (go.transform.position.z < behindZAll || go.scene != SceneManager.GetActiveScene())
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
            Debug.LogWarning("Generate.OnCoinDestroyed: coinPrefab not assigned - cannot spawn replacement.");
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

    // We do not spawn replacements here. Spawning is handled only by the timer in Update (SpawnCoin).
}

    void SpawnCoin()
    {
        int lane = Random.Range(0, xPositions.Length);
        Vector3 spawnPos = new Vector3(
            xPositions[lane],
            coinY,
            player.position.z + 150f
        );

        if (coinPrefab == null)
        {
            Debug.LogWarning("Generate.SpawnCoin: coinPrefab not assigned — cannot spawn.");
            return;
        }

        // Use the prefab's original rotation
    GameObject coin = Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation);
    // Move the clone into the active scene so it behaves the same as VIRUS clones
    SceneManager.MoveGameObjectToScene(coin, SceneManager.GetActiveScene());
    coin.transform.SetParent(null);

        if (coin.GetComponent<SpawnedEntity>() == null)
        {
            var se = coin.AddComponent<SpawnedEntity>();
            se.player = this.player;
            se.destroyDistanceBehind = this.destroyDistanceBehind;
        }

        coins.Add(coin);
    }

}