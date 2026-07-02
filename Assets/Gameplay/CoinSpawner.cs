// ============================================================
//  CoinSpawner.cs
//  Attach to: SpawnManager GameObject in Gameplay scene
//  Handles: Spawns rows of coins in a lane ahead of the player
// ============================================================

using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject coinPrefab;          // Assign your coin prefab in Inspector

    [Header("Spawn Settings")]
    public float spawnDistance   = 35f;    // How far ahead coins appear
    public float spawnInterval   = 2.5f;  // Seconds between each coin row
    public int   coinsPerRow     = 5;      // How many coins in a line
    public float coinSpacing     = 1.5f;  // Gap between each coin

    private float[] lanePositions = { -2f, 0f, 2f }; // Align to 2-meter lanes
    private float spawnTimer = 0f;

    // ----------------------------------------------------------
    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isPlaying) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnCoinRow();
            spawnTimer = 0f;
        }
    }

    // ----------------------------------------------------------
    //  Spawns a straight row of coins in a random lane
    // ----------------------------------------------------------
    void SpawnCoinRow()
    {
        if (coinPrefab == null) return;

        int   lane = Random.Range(0, 3);
        float xPos = lanePositions[lane];

        for (int i = 0; i < coinsPerRow; i++)
        {
            Vector3 pos = new Vector3(xPos, 0.5f, spawnDistance + (i * coinSpacing));
            GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);

            // Auto-attach the mover (same speed system as obstacles)
            coin.AddComponent<CoinMover>();
        }
    }
}

// ============================================================
//  CoinMover — auto-attached to every spawned coin
//  Moves the coin forward and destroys it when passed
// ============================================================
public class CoinMover : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.back * SpeedController.currentSpeed * Time.deltaTime, Space.World);

        if (transform.position.z < -10f)
            Destroy(gameObject);
    }
}
