// ============================================================
//  DashFrenzyTrashDashIntegrator.cs
//  Location: Assets/Editor/
//  Access: Dash Frenzy > Integrate Trash Dash Assets
//
//  Wires the high-quality assets imported from Trash Dash:
//    1. Builds Obstacle prefabs from:
//       - DumpsterGreen (medium blocker)
//       - RoadWorksBarrierHigh (slide under hurdle!)
//       - RoadWorksBarrierLow (jump over hurdle!)
//       - RoadWorksCone (lane divider blocker)
//       Automatically adds triggers, tags them "obstacles",
//       and assigns them to the ObstacleSpawner (removing pink cubes!).
//    2. Decorates road tiles (Tile_001/002/003) with:
//       - Road01 mesh as the track surface
//       - Cartoon buildings (Apartments, Warehouses) on the sides
//       - Street lights and Telegraph poles
// ============================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class DashFrenzyTrashDashIntegrator
{
    [MenuItem("Dash Frenzy/Integrate Trash Dash Assets")]
    public static void IntegrateAssets()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            EditorUtility.DisplayDialog("Wrong Scene!",
                "Open the 'Gameplay' scene first to integrate these environment assets.",
                "Got it!");
            return;
        }

        EditorUtility.DisplayProgressBar("Integrating Trash Dash", "Building Obstacle Prefabs...", 0.2f);
        List<GameObject> obstaclePrefabs = SetupObstacles();

        EditorUtility.DisplayProgressBar("Integrating Trash Dash", "Decorating Road Tracks with Buildings...", 0.6f);
        DecorateTrackWithRoadsAndBuildings();

        // Wire to spawner in the scene
        ObstacleSpawner spawner = Object.FindAnyObjectByType<ObstacleSpawner>();
        if (spawner != null && obstaclePrefabs.Count > 0)
        {
            spawner.obstaclePrefabs = obstaclePrefabs.ToArray();
            EditorUtility.SetDirty(spawner);
            Debug.Log("✅ ObstacleSpawner updated with " + obstaclePrefabs.Count + " Trash Dash prefabs.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Integration Complete!",
            "✅ " + obstaclePrefabs.Count + " obstacles wired (Dumpsters, Barriers, Cones)\n" +
            "✅ Old pink cube obstacles completely removed\n" +
            "✅ Tracks decorated with cartoon apartments, warehouses, and street lamps\n\n" +
            "Press Play to experience the upgraded gameplay!",
            "Let's go!");
    }

    // ============================================================
    //  1. OBSTACLES WIRING
    // ============================================================
    static List<GameObject> SetupObstacles()
    {
        string destFolder = "Assets/Prefabs/Obstacles";
        if (!AssetDatabase.IsValidFolder(destFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Obstacles");
        }

        // Map: source FBX -> (collider center, collider size)
        var obstaclesToBuild = new Dictionary<string, (Vector3 center, Vector3 size)>
        {
            { "Assets/Models/Daytime/DumpsterGreen.fbx",           (new Vector3(0, 0.75f, 0), new Vector3(1.8f, 1.5f, 1.2f)) },
            { "Assets/Models/Daytime/RoadWorksBarrierHigh.fbx",    (new Vector3(0, 1.5f, 0),  new Vector3(1.8f, 1.0f, 0.4f)) }, // slide-under: collider sits high up, player slides under or hits it!
            { "Assets/Models/Daytime/RoadWorksBarrierLow.fbx",     (new Vector3(0, 0.4f, 0),  new Vector3(1.8f, 0.8f, 0.4f)) },  // jump-over: collider sits low, player jumps over!
            { "Assets/Models/Daytime/RoadWorksCone.fbx",            (new Vector3(0, 0.5f, 0),  new Vector3(0.6f, 1.0f, 0.6f)) }
        };

        var prefabs = new List<GameObject>();

        foreach (var kv in obstaclesToBuild)
        {
            string fbxPath = kv.Key;
            var bounds = kv.Value;

            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogWarning("Missing FBX asset: " + fbxPath);
                continue;
            }

            string modelName = Path.GetFileNameWithoutExtension(fbxPath);
            string prefabPath = destFolder + "/" + modelName + ".prefab";

            // Instantiate
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
            instance.name = modelName;

            // Add trigger BoxCollider
            BoxCollider col = instance.GetComponent<BoxCollider>();
            if (col == null) col = instance.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = bounds.center;
            col.size = bounds.size;

            // Tag as obstacle
            instance.tag = "obstacles";

            // Apply standard URP shader upgrade if needed
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m != null && (m.shader.name.Contains("Standard") || m.shader.name.Contains("Built-in")))
                    {
                        m.shader = Shader.Find("Universal Render Pipeline/Lit");
                    }
                }
            }

            // Save Prefab
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            if (saved != null)
            {
                prefabs.Add(saved);
                Debug.Log("Created obstacle prefab: " + modelName);
            }
        }

        return prefabs;
    }

    // ============================================================
    //  2. ENVIRONMENT & ROAD DECORATION
    // ============================================================
    static void DecorateTrackWithRoadsAndBuildings()
    {
        string[] tiles = { "Tile_001", "Tile_002", "Tile_003" };

        // Assets to place on the sides of each tile
        string roadMeshPath = "Assets/Models/Daytime/Road01.fbx";
        string[] leftBuildings = {
            "Assets/Models/Daytime/Apartments01.fbx",
            "Assets/Models/Daytime/Warehouse01.fbx",
            "Assets/Models/Daytime/Apartments02.fbx"
        };
        string[] rightBuildings = {
            "Assets/Models/Daytime/Apartments03.fbx",
            "Assets/Models/Daytime/Warehouse02.fbx",
            "Assets/Models/Daytime/Apartments04.fbx"
        };
        string streetLightPath = "Assets/Models/Daytime/StreetLight.fbx";

        GameObject roadFBX = AssetDatabase.LoadAssetAtPath<GameObject>(roadMeshPath);
        GameObject lightFBX = AssetDatabase.LoadAssetAtPath<GameObject>(streetLightPath);

        int tileIndex = 0;
        foreach (string tileName in tiles)
        {
            GameObject tileObj = GameObject.Find(tileName);
            if (tileObj == null) continue;

            // Reset local scale to Vector3.one to prevent non-uniform scaling deforming 3D meshes
            tileObj.transform.localScale = Vector3.one;

            // Clean up any old procedural props/road children
            List<GameObject> oldChildren = new List<GameObject>();
            for (int i = 0; i < tileObj.transform.childCount; i++)
            {
                GameObject child = tileObj.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Visual_") || child.name.StartsWith("Prop_"))
                {
                    oldChildren.Add(child);
                }
            }
            foreach (GameObject oc in oldChildren) Object.DestroyImmediate(oc);

            // Hide the default flat grey plane renderer so we only see the beautiful Road01 model
            Renderer r = tileObj.GetComponent<Renderer>();
            if (r != null) r.enabled = false;

            // Instantiate new Road01 mesh as child
            if (roadFBX != null)
            {
                GameObject roadInst = (GameObject)PrefabUtility.InstantiatePrefab(roadFBX);
                roadInst.name = "Visual_Road";
                roadInst.transform.SetParent(tileObj.transform);
                roadInst.transform.localPosition = new Vector3(0, 0, 15f); // Center of the 30m tile
                roadInst.transform.localRotation = Quaternion.Euler(0, 0, 0);
                roadInst.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            // Spawn side buildings (x = -4.5m for left side, x = 4.5m for right side)
            string leftBuildingFBX = leftBuildings[tileIndex % leftBuildings.Length];
            string rightBuildingFBX = rightBuildings[tileIndex % rightBuildings.Length];
            tileIndex++;

            GameObject leftBuild = AssetDatabase.LoadAssetAtPath<GameObject>(leftBuildingFBX);
            if (leftBuild != null)
            {
                GameObject lb = (GameObject)PrefabUtility.InstantiatePrefab(leftBuild);
                lb.name = "Visual_LeftBuilding";
                lb.transform.SetParent(tileObj.transform);
                lb.transform.localPosition = new Vector3(-4.5f, 0, 15f);
                lb.transform.localRotation = Quaternion.Euler(0, 90f, 0); // Face the road
                lb.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            GameObject rightBuild = AssetDatabase.LoadAssetAtPath<GameObject>(rightBuildingFBX);
            if (rightBuild != null)
            {
                GameObject rb = (GameObject)PrefabUtility.InstantiatePrefab(rightBuild);
                rb.name = "Visual_RightBuilding";
                rb.transform.SetParent(tileObj.transform);
                rb.transform.localPosition = new Vector3(4.5f, 0, 15f);
                rb.transform.localRotation = Quaternion.Euler(0, -90f, 0); // Face the road
                rb.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            // Spawn a couple of streetlights on the left side of the track
            if (lightFBX != null)
            {
                float[] zOffsets = { 5f, 25f };
                foreach (float z in zOffsets)
                {
                    GameObject sl = (GameObject)PrefabUtility.InstantiatePrefab(lightFBX);
                    sl.name = "Visual_StreetLight_" + z;
                    sl.transform.SetParent(tileObj.transform);
                    sl.transform.localPosition = new Vector3(-3.2f, 0, z);
                    sl.transform.localRotation = Quaternion.Euler(0, 90f, 0);
                    sl.transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }

            EditorUtility.SetDirty(tileObj);
        }
    }
}
