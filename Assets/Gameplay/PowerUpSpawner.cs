// ============================================================
//  PowerUpSpawner.cs
//  Attach to: SpawnManager GameObject under [SPAWNER]
//  Handles: Spawns power-up orbs ahead of the player
// ============================================================

using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    public GameObject[] powerUpPrefabs;      // Magnet, Shield, SpeedBoost, Multiplier prefabs

    [Header("Spawn Settings")]
    public float spawnDistance   = 40f;      // Spawn distance ahead of the player
    public float minSpawnInterval = 10f;      // Minimum seconds between power-up spawns
    public float maxSpawnInterval = 20f;      // Maximum seconds between power-up spawns

    private float[] lanePositions = { -2f, 0f, 2f }; // Align to 2-meter lanes
    private float spawnTimer = 0f;
    private float nextSpawnIn;

    // ----------------------------------------------------------
    void Start()
    {
        nextSpawnIn = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isPlaying) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnIn)
        {
            SpawnPowerUp();
            spawnTimer = 0f;
            nextSpawnIn = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    // ----------------------------------------------------------
    //  Spawns a random power-up in a random lane
    // ----------------------------------------------------------
    void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        int randomType = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[randomType] == null) return;

        int randomLane = Random.Range(0, 3);
        float xPos     = lanePositions[randomLane];

        Vector3 spawnPos = new Vector3(xPos, 0.5f, spawnDistance);
        GameObject powerUp = Instantiate(powerUpPrefabs[randomType], spawnPos, Quaternion.identity);

        // Auto-attach the mover component to drive it toward the player
        powerUp.AddComponent<PowerUpMover>();
    }
}

// ============================================================
//  PowerUpMover — auto-attached to every spawned power-up
//  Moves the power-up forward and destroys it when passed
// ============================================================
public class PowerUpMover : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.back * SpeedController.currentSpeed * Time.deltaTime, Space.World);

        if (transform.position.z < -10f)
            Destroy(gameObject);
    }
}
