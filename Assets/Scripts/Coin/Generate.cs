using UnityEngine;
using System.Collections.Generic;

public class Generate : MonoBehaviour
{
    public GameObject coinPrefab;
    public Transform player;
    public float coinY = 2f;
    public float spawnInterval = 9f;
    public int maxCoins = 2;

    private readonly float[] xPositions = { -2.5f, 0.12f, 2.3f };
    private List<GameObject> coins = new List<GameObject>();
    private float timer = 0f;

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

        // Cleanup null coins (destroyed via collision)
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i] == null)
                coins.RemoveAt(i);
        }
    }
    public void OnCoinDestroyed()
{
    // Always spawn 100f ahead of the player
    int lane = Random.Range(0, xPositions.Length);
    Vector3 spawnPos = new Vector3(
        xPositions[lane],
        coinY,
        player.position.z + 100f
    );

    GameObject coin = Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation);
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
        GameObject coin = Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation);
        coins.Add(coin);
    }

}