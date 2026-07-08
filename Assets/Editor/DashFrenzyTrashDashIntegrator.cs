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
//       - Visual_RoadSurface (procedural visual road surface)
//       - Cartoon buildings (Apartments, Warehouses) on the sides
//       - Street lights and Telegraph poles
// ============================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

public class DashFrenzyTrashDashIntegrator
{
    const float TileLength = 30f;
    const float PlayableRoadWidth = 6.2f;
    const float WallX = 5.8f;
    const float BuildingX = 10.5f;
    const float SidePropX = 5.6f; // Placed outside x = +/-5.5f

    [MenuItem("Dash Frenzy/Integrate Trash Dash Assets")]
    public static void IntegrateAssets()
    {
        RunIntegration(true);
    }

    public static void IntegrateGameplaySceneBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        RunIntegration(false);
        EditorSceneManager.SaveOpenScenes();
    }

    static void RunIntegration(bool showDialog)
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Wrong Scene!",
                    "Open the 'Gameplay' scene first to integrate these environment assets.",
                    "Got it!");
            }
            else
            {
                Debug.LogError("DashFrenzyTrashDashIntegrator: Gameplay scene is not active.");
            }
            return;
        }

        EditorUtility.DisplayProgressBar("Integrating Trash Dash", "Building Obstacle Prefabs...", 0.15f);
        List<GameObject> obstaclePrefabs = SetupObstacles();

        EditorUtility.DisplayProgressBar("Integrating Trash Dash", "Decorating Road Tracks with Buildings...", 0.4f);
        DecorateTrackWithRoadsAndBuildings();

        // Wire to spawner in the scene
        ObstacleSpawner spawner = Object.FindAnyObjectByType<ObstacleSpawner>();
        if (spawner != null && obstaclePrefabs.Count > 0)
        {
            spawner.obstaclePrefabs = obstaclePrefabs.ToArray();
            EditorUtility.SetDirty(spawner);
            Debug.Log("✅ ObstacleSpawner updated with " + obstaclePrefabs.Count + " Trash Dash prefabs.");
        }

        // Align Player (Max) start position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.transform.rotation = Quaternion.identity;
            EditorUtility.SetDirty(player);
            Debug.Log("✅ Player (Max) aligned to start position (0, 1, 0).");
        }

        EditorUtility.DisplayProgressBar("Integrating Trash Dash", "Upgrading materials to URP & fixing skybox...", 0.75f);
        UpgradeAllSceneMaterialsToURP();
        FixSkybox();
        CreateHeartSprite();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes(); // Save Gameplay.unity after running integration
        EditorUtility.ClearProgressBar();

        if (!showDialog)
        {
            Debug.Log("DashFrenzyTrashDashIntegrator: Integration complete.");
            return;
        }

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

        string[] roadMeshes = {
            "Assets/Models/Daytime/Road01.fbx",
            "Assets/Models/Daytime/Road02.fbx",
            "Assets/Models/Daytime/Road03.fbx",
            "Assets/Models/Daytime/Road04.fbx"
        };
        string[] wallMeshes = {
            "Assets/Models/Daytime/WoodFence01.fbx",
            "Assets/Models/Daytime/WoodFence02.fbx",
            "Assets/Models/Daytime/TelegraphWires.fbx"
        };
        string[] leftBuildings = {
            "Assets/Models/Daytime/Apartments01.fbx",
            "Assets/Models/Daytime/Warehouse01.fbx",
            "Assets/Models/Daytime/Garage01.fbx"
        };
        string[] rightBuildings = {
            "Assets/Models/Daytime/Apartments03.fbx",
            "Assets/Models/Daytime/Warehouse02.fbx",
            "Assets/Models/Daytime/Apartments04.fbx"
        };
        string[] sideProps = {
            "Assets/Models/Daytime/DumpsterRed.fbx",
            "Assets/Models/Daytime/BinBagClosed.fbx",
            "Assets/Models/Daytime/Bin01.fbx",
            "Assets/Models/Daytime/GrassClump01.fbx",
            "Assets/Models/Daytime/WheelyBinBlue.fbx",
            "Assets/Models/Daytime/StreetLight.fbx",
            "Assets/Models/Daytime/WoodFence01.fbx"
        };

        int tileIndex = 0;
        foreach (string tileName in tiles)
        {
            GameObject tileObj = GameObject.Find(tileName);
            if (tileObj == null) continue;

            ResetTileForRunner(tileObj, tileIndex);
            BuildRoad(tileObj, roadMeshes, tileIndex);
            BuildSideEdges(tileObj, wallMeshes, tileIndex);
            BuildBuildings(tileObj, leftBuildings, rightBuildings, tileIndex);
            BuildSideProps(tileObj, sideProps, tileIndex);

            EditorUtility.SetDirty(tileObj);
            tileIndex++;
        }

        UpdateTrackManager(tiles);
        UpdateCameraForReferenceAlley();
    }

    static void ResetTileForRunner(GameObject tileObj, int tileIndex)
    {
        tileObj.tag = "ground";
        tileObj.transform.position = new Vector3(0f, 0f, tileIndex * TileLength);
        tileObj.transform.rotation = Quaternion.identity;
        tileObj.transform.localScale = Vector3.one;

        CleanGeneratedChildren(tileObj);

        // Hide default flat mesh renderer
        Renderer renderer = tileObj.GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        // Destroy any fragile mesh colliders
        MeshCollider meshCollider = tileObj.GetComponent<MeshCollider>();
        if (meshCollider != null) Object.DestroyImmediate(meshCollider);

        // Create / overwrite a stable ground BoxCollider at y = 0
        BoxCollider groundCollider = tileObj.GetComponent<BoxCollider>();
        if (groundCollider == null) groundCollider = tileObj.AddComponent<BoxCollider>();
        groundCollider.isTrigger = false;
        groundCollider.center = new Vector3(0f, -0.06f, TileLength * 0.5f);
        groundCollider.size = new Vector3(PlayableRoadWidth, 0.12f, TileLength);
    }

    static void CleanGeneratedChildren(GameObject tileObj)
    {
        List<GameObject> oldChildren = new List<GameObject>();
        for (int i = 0; i < tileObj.transform.childCount; i++)
        {
            GameObject child = tileObj.transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Visual_") || child.name.StartsWith("Prop_"))
            {
                oldChildren.Add(child);
            }
        }

        foreach (GameObject child in oldChildren)
        {
            Object.DestroyImmediate(child);
        }
    }

    static void BuildRoad(GameObject tileObj, string[] roadMeshes, int tileIndex)
    {
        // 1. Create a stable, procedural Road surface cube (Y = 0 top surface)
        GameObject roadCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roadCube.name = "Visual_RoadSurface";
        roadCube.transform.SetParent(tileObj.transform, false);
        roadCube.transform.localPosition = new Vector3(0f, -0.05f, TileLength * 0.5f);
        roadCube.transform.localScale = new Vector3(PlayableRoadWidth, 0.1f, TileLength);

        // Destroy the default primitive collider to keep physics on the parent tile ground collider
        Collider cubeCol = roadCube.GetComponent<Collider>();
        if (cubeCol != null) Object.DestroyImmediate(cubeCol);

        // Apply asphalt material
        Material roadMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/RoadAsphalt.mat");
        if (roadMat == null) roadMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/RoadTile.mat");
        Renderer cubeRenderer = roadCube.GetComponent<Renderer>();
        if (cubeRenderer != null && roadMat != null)
        {
            cubeRenderer.sharedMaterial = roadMat;
        }

        // Road decoration meshes removed — they were misaligned and overlapping.
        // The procedural cube road surface above is the sole visual ground.
    }

    static void BuildSideEdges(GameObject tileObj, string[] wallMeshes, int tileIndex)
    {
        string leftWallPath = FirstExistingAsset(wallMeshes, tileIndex);
        string rightWallPath = FirstExistingAsset(wallMeshes, tileIndex + 1);

        for (int i = 0; i < 4; i++)
        {
            float z = 3f + (i * 8f);
            GameObject leftEdge = InstantiateAssetChild(leftWallPath, tileObj.transform, "Visual_LeftEdge_" + i);
            ConfigureSideObject(leftEdge, new Vector3(-WallX, 0f, z), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.75f, 0.75f, 0.75f));

            GameObject rightEdge = InstantiateAssetChild(rightWallPath, tileObj.transform, "Visual_RightEdge_" + i);
            ConfigureSideObject(rightEdge, new Vector3(WallX, 0f, z), Quaternion.Euler(0f, -90f, 0f), new Vector3(0.75f, 0.75f, 0.75f));
        }
    }

    static void BuildBuildings(GameObject tileObj, string[] leftBuildings, string[] rightBuildings, int tileIndex)
    {
        string leftPath = FirstExistingAsset(leftBuildings, tileIndex);
        string rightPath = FirstExistingAsset(rightBuildings, tileIndex);

        GameObject leftBuilding = InstantiateAssetChild(leftPath, tileObj.transform, "Visual_LeftBuilding");
        ConfigureSideObject(leftBuilding, new Vector3(-BuildingX, 0f, 18f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.55f, 0.55f, 0.55f));

        GameObject rightBuilding = InstantiateAssetChild(rightPath, tileObj.transform, "Visual_RightBuilding");
        ConfigureSideObject(rightBuilding, new Vector3(BuildingX, 0f, 18f), Quaternion.Euler(0f, -90f, 0f), new Vector3(0.55f, 0.55f, 0.55f));
    }

    static void BuildSideProps(GameObject tileObj, string[] sideProps, int tileIndex)
    {
        float[] zOffsets = { 4f, 12f, 20f, 27f };
        for (int i = 0; i < zOffsets.Length; i++)
        {
            string propPath = FirstExistingAsset(sideProps, tileIndex + i);
            float side = ((tileIndex + i) % 2 == 0) ? -1f : 1f;
            float x = side * SidePropX;
            float yRotation = side < 0 ? 90f : -90f;

            GameObject prop = InstantiateAssetChild(propPath, tileObj.transform, "Prop_Side_" + i);
            ConfigureSideObject(prop, new Vector3(x, 0f, zOffsets[i]), Quaternion.Euler(0f, yRotation, 0f), Vector3.one);
        }

        string lightPath = "Assets/Models/Daytime/StreetLight.fbx";
        GameObject frontLight = InstantiateAssetChild(lightPath, tileObj.transform, "Prop_LeftStreetLight");
        ConfigureSideObject(frontLight, new Vector3(-5f, 0f, 7f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.8f, 0.8f, 0.8f));

        GameObject rearLight = InstantiateAssetChild(lightPath, tileObj.transform, "Prop_RightStreetLight");
        ConfigureSideObject(rearLight, new Vector3(5f, 0f, 23f), Quaternion.Euler(0f, -90f, 0f), new Vector3(0.8f, 0.8f, 0.8f));
    }

    static void ConfigureSideObject(GameObject obj, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        if (obj == null) return;

        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;
        DisableChildColliders(obj);
        PushOutsidePlayableCorridor(obj);
    }

    static void PushOutsidePlayableCorridor(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float minAllowedX = 5.5f; // Target outer safety zone
        float moveX = 0f;

        if (obj.transform.position.x < 0f && bounds.max.x > -minAllowedX)
        {
            moveX = (-minAllowedX) - bounds.max.x;
        }
        else if (obj.transform.position.x > 0f && bounds.min.x < minAllowedX)
        {
            moveX = minAllowedX - bounds.min.x;
        }

        if (Mathf.Abs(moveX) > 0.001f)
        {
            obj.transform.position += new Vector3(moveX, 0f, 0f);
        }
    }

    static GameObject InstantiateAssetChild(string assetPath, Transform parent, string name)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            Debug.LogWarning("Missing environment asset: " + assetPath);
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        if (instance == null) return null;

        instance.name = name;
        instance.transform.SetParent(parent, false);
        return instance;
    }

    static string FirstExistingAsset(string[] assetPaths, int startIndex)
    {
        if (assetPaths == null || assetPaths.Length == 0) return null;

        for (int i = 0; i < assetPaths.Length; i++)
        {
            string path = assetPaths[(startIndex + i) % assetPaths.Length];
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return path;
        }

        return null;
    }

    static void DisableChildColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    static void UpdateTrackManager(string[] tileNames)
    {
        TrackManager trackManager = Object.FindAnyObjectByType<TrackManager>();
        if (trackManager == null) return;

        List<GameObject> tiles = new List<GameObject>();
        foreach (string tileName in tileNames)
        {
            GameObject tile = GameObject.Find(tileName);
            if (tile != null) tiles.Add(tile);
        }

        if (tiles.Count == tileNames.Length)
        {
            trackManager.tiles = tiles.ToArray();
            trackManager.tileLength = TileLength;
            EditorUtility.SetDirty(trackManager);
        }
    }

    static void UpdateCameraForReferenceAlley()
    {
        CameraFollow cameraFollow = Object.FindAnyObjectByType<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.offset = new Vector3(0f, 3.4f, -6.4f);
            cameraFollow.smoothSpeed = 12f;
            EditorUtility.SetDirty(cameraFollow);
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = 58f;
            mainCamera.backgroundColor = new Color(0.45f, 0.68f, 0.92f, 1f);
            EditorUtility.SetDirty(mainCamera);
        }
    }

    // ============================================================
    //  VALIDATION METHOD
    // ============================================================
    [MenuItem("Dash Frenzy/Validate Gameplay Corridor")]
    public static void ValidateGameplayCorridor()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            EditorUtility.DisplayDialog("Wrong Scene!",
                "Open the 'Gameplay' scene first to run corridor validation.", "Got it!");
            return;
        }

        Debug.Log("=== Validating Gameplay Corridor ===");

        // Check Max Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log(string.Format("Max Player Position: {0} | Rigidbody: {1}",
                player.transform.position, player.GetComponent<Rigidbody>() != null));
            if (Mathf.Abs(player.transform.position.x) > 0.1f || Mathf.Abs(player.transform.position.z) > 0.1f)
            {
                Debug.LogWarning("⚠️ Player is skewed from center at start. Position: " + player.transform.position);
            }
        }
        else
        {
            Debug.LogError("❌ No GameObject found with tag 'Player'!");
        }

        // Check Camera Follow offsets
        CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>();
        if (follow != null)
        {
            Debug.Log(string.Format("Camera Follow Target: {0} | Offset: {1} | SmoothSpeed: {2}",
                follow.player != null ? follow.player.name : "None", follow.offset, follow.smoothSpeed));
            if (Mathf.Abs(follow.offset.x) > 0.1f)
            {
                Debug.LogWarning("⚠️ Camera offset is skewed sideways! offset.x: " + follow.offset.x);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No CameraFollow script found in scene!");
        }

        // Check ground and corridor positioning
        string[] tileNames = { "Tile_001", "Tile_002", "Tile_003" };
        foreach (string name in tileNames)
        {
            GameObject tile = GameObject.Find(name);
            if (tile != null)
            {
                BoxCollider col = tile.GetComponent<BoxCollider>();
                string colStr = col != null ? string.Format("Center: {0}, Size: {1}", col.center, col.size) : "No BoxCollider";

                MeshCollider mesh = tile.GetComponent<MeshCollider>();
                string meshStr = mesh != null && mesh.enabled ? "⚠️ Active MeshCollider" : "None";

                Transform visuals = tile.transform.Find("Visual_RoadSurface");
                string roadStr = visuals != null ? "Yes" : "❌ Missing Visual_RoadSurface";

                Debug.Log(string.Format("Tile: {0} | Scale: {1} | Pos: {2} | GroundCollider: {3} | MeshCollider: {4} | VisualRoad: {5}",
                    tile.name, tile.transform.localScale, tile.transform.position, colStr, meshStr, roadStr));

                if (tile.transform.localScale != Vector3.one)
                {
                    Debug.LogError("❌ Tile scale is deformed! Scale: " + tile.transform.localScale);
                }

                // Check bounds of decorative items to make sure they are pushed out
                Renderer[] childRenderers = tile.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in childRenderers)
                {
                    if (r.gameObject.name.StartsWith("Visual_") && !r.gameObject.name.Contains("Road"))
                    {
                        Bounds b = r.bounds;
                        if (b.min.x < 5.5f && b.max.x > -5.5f)
                        {
                            Debug.LogWarning(string.Format("⚠️ Decorative object '{0}' bounds {1} encroach into playable road corridor!",
                                r.gameObject.name, b));
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("❌ Missing track segment: " + name);
            }
        }

        Debug.Log("=== Validation Complete ===");
        EditorUtility.DisplayDialog("Validation Complete",
            "Corridor validation checks run. Inspect the Console tab for detail logs, warnings, or errors.",
            "OK");
    }

    // ============================================================
    //  UPGRADE ALL SCENE MATERIALS TO URP
    // ============================================================
    static void UpgradeAllSceneMaterialsToURP()
    {
        int upgraded = 0;
        Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLit == null)
        {
            Debug.LogWarning("URP/Lit shader not found. Material upgrade skipped.");
            return;
        }

        foreach (Renderer r in allRenderers)
        {
            foreach (Material m in r.sharedMaterials)
            {
                if (m != null && !m.shader.name.Contains("Universal") &&
                    !m.shader.name.Contains("Skybox") &&
                    !m.shader.name.Contains("UI") &&
                    !m.shader.name.Contains("Sprites") &&
                    !m.shader.name.Contains("Particle"))
                {
                    // Preserve the base color/texture before switching
                    Color baseColor = Color.white;
                    Texture mainTex = null;

                    if (m.HasProperty("_Color"))
                        baseColor = m.GetColor("_Color");
                    if (m.HasProperty("_MainTex"))
                        mainTex = m.GetTexture("_MainTex");

                    m.shader = urpLit;

                    if (m.HasProperty("_BaseColor"))
                        m.SetColor("_BaseColor", baseColor);
                    if (m.HasProperty("_BaseMap") && mainTex != null)
                        m.SetTexture("_BaseMap", mainTex);

                    upgraded++;
                }
            }
        }

        Debug.Log("✅ Upgraded " + upgraded + " materials to URP/Lit.");
    }

    // ============================================================
    //  FIX SKYBOX — Clean blue procedural sky
    // ============================================================
    static void FixSkybox()
    {
        string matPath = "Assets/Settings/CartoonSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader == null)
        {
            Debug.LogWarning("Skybox/Procedural shader not found.");
            return;
        }

        if (skyMat == null)
        {
            skyMat = new Material(skyShader);
            AssetDatabase.CreateAsset(skyMat, matPath);
        }

        skyMat.shader = skyShader;  // Force correct shader even if it was corrupted
        skyMat.SetColor("_SkyTint", new Color(0.35f, 0.55f, 0.9f));     // Clean sky blue
        skyMat.SetColor("_GroundColor", new Color(0.65f, 0.75f, 0.85f)); // Soft horizon
        skyMat.SetFloat("_SunSize", 0.04f);
        skyMat.SetFloat("_AtmosphereThickness", 1.0f);
        skyMat.SetFloat("_Exposure", 1.3f);
        EditorUtility.SetDirty(skyMat);

        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.6f, 0.75f, 0.95f);      // Cool blue from above
        RenderSettings.ambientEquatorColor = new Color(0.85f, 0.85f, 0.8f);  // Warm neutral at horizon
        RenderSettings.ambientGroundColor = new Color(0.4f, 0.35f, 0.3f);    // Dark brown below
        DynamicGI.UpdateEnvironment();
        Debug.Log("✅ Skybox fixed to clean blue sky.");
    }

    // ============================================================
    //  CREATE HEART SPRITE for Lives UI
    // ============================================================
    static void CreateHeartSprite()
    {
        string spritePath = "Assets/UI/HeartSprite.png";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath) != null)
        {
            Debug.Log("Heart sprite already exists.");
            return;
        }

        // Generate a simple 64x64 red heart texture
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color heartRed = new Color(0.9f, 0.15f, 0.2f, 1f);

        // Fill transparent
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Draw heart shape using math
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Normalize to -1..1
                float nx = (x - size * 0.5f) / (size * 0.5f);
                float ny = (y - size * 0.5f) / (size * 0.5f);
                ny -= 0.1f; // shift down slightly

                // Heart implicit equation: (x^2 + y^2 - 1)^3 - x^2 * y^3 < 0
                float x2 = nx * nx;
                float y2 = ny * ny;
                float eq = (x2 + y2 - 1f);
                eq = eq * eq * eq - x2 * ny * ny * ny;

                if (eq < 0f)
                {
                    tex.SetPixel(x, y, heartRed);
                }
            }
        }

        tex.Apply();

        // Save as PNG
        byte[] pngBytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath, "UI/HeartSprite.png");
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(fullPath, pngBytes);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        // Configure as Sprite
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        Debug.Log("✅ Heart sprite created at " + spritePath);
    }
}
