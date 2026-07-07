// ============================================================
//  VisualPolishSetup.cs
//  Location: Assets/Editor/
//  Access: Dash Frenzy > Polish Visuals
//
//  Fixes: gold coin material, road tile material, obstacle scale
// ============================================================

using UnityEngine;
using UnityEditor;

public class VisualPolishSetup
{
    [MenuItem("Dash Frenzy/Polish Visuals")]
    public static void PolishVisuals()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            EditorUtility.DisplayDialog("Wrong Scene!",
                "Open the 'Gameplay' scene first.", "Got it!");
            return;
        }

        FixCoinMaterial();
        FixRoadTiles();
        FixObstacleScale();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Visuals Polished!",
            "✅ Coins are now gold\n" +
            "✅ Road tiles have a dark road colour\n" +
            "✅ Obstacle scale corrected\n\n" +
            "Press Play to see the difference!",
            "Nice!");
    }

    // ---- COINS: Replace magenta with shiny gold ----
    static void FixCoinMaterial()
    {
        // Create a gold material if it doesn't exist
        string matPath = "Assets/Prefabs/Coins/GoldCoin.mat";
        Material goldMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (goldMat == null)
        {
            goldMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            goldMat.SetColor("_BaseColor", new Color(1f, 0.78f, 0f));       // Gold colour
            goldMat.SetFloat("_Metallic", 0.9f);
            goldMat.SetFloat("_Smoothness", 0.8f);
            AssetDatabase.CreateAsset(goldMat, matPath);
            Debug.Log("Gold material created.");
        }

        // Apply to the Coin prefab
        string coinPrefabPath = "Assets/Prefabs/Coins/Coin.prefab";
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coinPrefabPath);
        if (coinPrefab != null)
        {
            Renderer r = coinPrefab.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = goldMat;
                EditorUtility.SetDirty(coinPrefab);
                Debug.Log("Coin prefab updated with gold material.");
            }
        }
    }

    // ---- ROAD: Give tiles a proper dark road colour ----
    static void FixRoadTiles()
    {
        string matPath = "Assets/Prefabs/RoadTile.mat";
        Material roadMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (roadMat == null)
        {
            roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMat.SetColor("_BaseColor", new Color(0.25f, 0.25f, 0.28f)); // Dark asphalt grey
            roadMat.SetFloat("_Smoothness", 0.1f);
            roadMat.SetFloat("_Metallic", 0f);
            AssetDatabase.CreateAsset(roadMat, matPath);
            Debug.Log("Road material created.");
        }

        // Apply to all Tile objects in the scene
        GameObject[] tiles = {
            GameObject.Find("Tile_001"),
            GameObject.Find("Tile_002"),
            GameObject.Find("Tile_003"),
        };

        foreach (GameObject tile in tiles)
        {
            if (tile == null) continue;
            Renderer r = tile.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = roadMat;
                EditorUtility.SetDirty(tile);
                Debug.Log("Road material applied to: " + tile.name);
            }
        }
    }

    // ---- OBSTACLES: Scale Kenney barriers to a playable size ----
    static void FixObstacleScale()
    {
        // Kenney models import at 1cm = 0.01m so they appear tiny.
        // Scale them up so they are about 1.5m tall — visible but jumpable/slideable.
        string[] obstaclePrefabPaths = {
            "Assets/Prefabs/Obstacles/construction-barrier.prefab",
            "Assets/Prefabs/Obstacles/road-straight-barrier.prefab",
            "Assets/Prefabs/Obstacles/road-side-barrier.prefab",
        };

        foreach (string path in obstaclePrefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Check current scale — if it's tiny, fix it
            if (prefab.transform.localScale.x < 0.5f)
            {
                prefab.transform.localScale = new Vector3(8f, 8f, 8f);
                EditorUtility.SetDirty(prefab);
                Debug.Log("Scale fixed for: " + path);
            }
        }
    }
}
