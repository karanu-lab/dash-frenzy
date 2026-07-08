// ============================================================
//  TrackManager.cs
//  Attach to: TrackManager GameObject in Gameplay scene
//  Handles: Cycles 3 road tiles to create an infinite road illusion
//           (The player stays still — the world moves toward them)
// ============================================================

using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Tile Setup")]
    public GameObject[] tiles;          // Assign exactly 3 road tile prefabs in Inspector
    public float tileLength = 30f;      // Length of each tile (must match actual prefab size)

    // ----------------------------------------------------------
    void Start()
    {
        // Ensure tiles are correctly aligned at start to prevent gaps
        if (tiles != null && tiles.Length == 3)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null)
                {
                    tiles[i].transform.position = new Vector3(0f, 0f, i * tileLength);
                }
            }
        }
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isPlaying) return;

        float speed = SpeedController.currentSpeed;

        foreach (GameObject tile in tiles)
        {
            // Move every tile toward the player in World space
            tile.transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

            // If a tile has moved fully behind the player, send it to the front
            if (tile.transform.position.z < -tileLength)
                RepositionTile(tile);
        }
    }

    // ----------------------------------------------------------
    //  Moves the given tile just ahead of the furthest tile
    // ----------------------------------------------------------
    void RepositionTile(GameObject tile)
    {
        float furthestZ = GetFurthestTileZ();
        tile.transform.position = new Vector3(0, 0, furthestZ + tileLength);
    }

    // ----------------------------------------------------------
    //  Finds the Z position of the tile that is furthest ahead
    // ----------------------------------------------------------
    float GetFurthestTileZ()
    {
        float maxZ = float.MinValue;
        foreach (GameObject t in tiles)
            if (t.transform.position.z > maxZ) maxZ = t.transform.position.z;
        return maxZ;
    }
}
