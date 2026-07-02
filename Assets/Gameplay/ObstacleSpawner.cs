// ============================================================
//  ObstacleSpawner.cs
//  Attach to: SpawnManager GameObject in Gameplay scene
//  Handles: Randomly spawns obstacles ahead of the player
//           Each obstacle moves itself and destroys when passed
// ============================================================

using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    public GameObject[] obstaclePrefabs;       // Assign obstacle prefabs in Inspector

    [Header("Spawn Settings")]
    public float spawnDistance   = 40f;        // How far ahead obstacles spawn
    public float minSpawnInterval = 1.0f;      // Shortest possible gap between spawns (increased frequency for GDD spec)
    public float maxSpawnInterval = 2.5f;      // Longest possible gap between spawns

    private float[] lanePositions = { -2f, 0f, 2f }; // Align to 2-meter lanes
    private float spawnTimer   = 0f;
    private float nextSpawnIn  = 2f;           // Time until next spawn

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
            SpawnObstacle();
            spawnTimer  = 0f;
            nextSpawnIn = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    // ----------------------------------------------------------
    //  Picks a random prefab and a random lane, spawns ahead
    // ----------------------------------------------------------
    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        int randomLane  = Random.Range(0, 3);
        float xPos      = lanePositions[randomLane];

        Vector3 spawnPos = new Vector3(xPos, 0.5f, spawnDistance);

        // Instantiate and attach the mover script at runtime
        GameObject obs = Instantiate(obstaclePrefabs[randomIndex], spawnPos, Quaternion.identity);

        // Attach a self-moving + self-destroying component
        obs.AddComponent<ObstacleMover>();
    }
}

// ============================================================
//  ObstacleMover — auto-attached to every spawned obstacle
//  Moves the obstacle forward and destroys it when passed
// ============================================================
public class ObstacleMover : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.back * SpeedController.currentSpeed * Time.deltaTime, Space.World);

        // Destroy when it has passed the player
        if (transform.position.z < -10f)
            Destroy(gameObject);
    }
}
