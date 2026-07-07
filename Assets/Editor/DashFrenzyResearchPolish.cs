// ============================================================
//  DashFrenzyResearchPolish.cs
//  Location: Assets/Editor/
//  Access: Dash Frenzy > Apply Research Improvements
//
//  Automates all research recommendations in one click:
//    1. Creates/applies stylized gradient procedural skybox.
//    2. Optimizes Directional Light (Mixed, Hard Shadows, Shadow Distance).
//    3. Spawns Kenney props (barrels, bags, fruit) as sidewalk clutter.
//    4. Creates/adds URP Post-Processing Volume (Bloom, Color Adjustments).
//    5. Fixes the Coin material to shiny gold.
//    6. Fixes the Road tiles with dark asphalt material.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.IO;

public class DashFrenzyResearchPolish
{
    [MenuItem("Dash Frenzy/Apply Research Improvements")]
    public static void ApplyAllImprovements()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            EditorUtility.DisplayDialog("Wrong Scene!",
                "Open the 'Gameplay' scene first to apply these environment polish steps.",
                "Got it!");
            return;
        }

        EditorUtility.DisplayProgressBar("Polishing Dash Frenzy", "Configuring Skybox & Lighting...", 0.1f);
        SetupProceduralSkybox();
        OptimizeDirectionalLight();

        EditorUtility.DisplayProgressBar("Polishing Dash Frenzy", "Creating Materials...", 0.3f);
        Material goldMat = CreateGoldMaterial();
        Material roadMat = CreateRoadMaterial();

        EditorUtility.DisplayProgressBar("Polishing Dash Frenzy", "Decorating Road Tiles with Market Props...", 0.5f);
        DecorateTrackWithProps(roadMat);

        EditorUtility.DisplayProgressBar("Polishing Dash Frenzy", "Configuring URP Post-Processing...", 0.8f);
        SetupPostProcessing();

        // Update coin prefab
        UpdateCoinPrefab(goldMat);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Research Polish Complete!",
            "✅ Stylized Skybox applied\n" +
            "✅ Mixed lighting and shadows configured\n" +
            "✅ Road tiles decorated with market stalls/props\n" +
            "✅ URP Post-Processing Volume active (Bloom + Saturation)\n" +
            "✅ Coins set to gold & road set to asphalt\n\n" +
            "Press Play to test the visually upgraded game!",
            "Awesome!");
    }

    // ============================================================
    //  1. SKYBOX CONFIGURATION
    // ============================================================
    static void SetupProceduralSkybox()
    {
        string matPath = "Assets/Settings/CartoonSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (skyMat == null)
        {
            skyMat = new Material(Shader.Find("Skybox/Procedural"));
            skyMat.SetColor("_SkyTint", new Color(0.2f, 0.6f, 0.9f));    // Sky blue
            skyMat.SetColor("_GroundTint", new Color(0.9f, 0.6f, 0.4f)); // Warm peach horizon
            skyMat.SetFloat("_SunSize", 0.04f);
            skyMat.SetFloat("_AtmosphereThickness", 1.2f);
            skyMat.SetFloat("_Exposure", 1.1f);
            AssetDatabase.CreateAsset(skyMat, matPath);
        }

        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = AmbientMode.Skybox;
        DynamicGI.UpdateEnvironment();
        Debug.Log("✅ Skybox applied.");
    }

    // ============================================================
    //  2. DIRECTIONAL LIGHT OPTIMIZATION
    // ============================================================
    static void OptimizeDirectionalLight()
    {
        Light dirLight = null;
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                dirLight = l;
                break;
            }
        }

        if (dirLight != null)
        {
            dirLight.lightmapBakeType = LightmapBakeType.Mixed;
            dirLight.shadows = LightShadows.Hard;
            dirLight.shadowStrength = 0.8f;
            dirLight.color = new Color(1f, 0.95f, 0.85f); // Warm sunlight
            dirLight.intensity = 1.2f;
            EditorUtility.SetDirty(dirLight);
            Debug.Log("✅ Directional Light optimized.");
        }
    }

    // ============================================================
    //  3. MATERIALS CREATION
    // ============================================================
    static Material CreateGoldMaterial()
    {
        string matPath = "Assets/Settings/GoldCoin.mat";
        Material goldMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (goldMat == null)
        {
            goldMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            goldMat.SetColor("_BaseColor", new Color(1f, 0.78f, 0f));
            goldMat.SetFloat("_Metallic", 0.9f);
            goldMat.SetFloat("_Smoothness", 0.85f);
            AssetDatabase.CreateAsset(goldMat, matPath);
        }
        return goldMat;
    }

    static Material CreateRoadMaterial()
    {
        string matPath = "Assets/Settings/RoadAsphalt.mat";
        Material roadMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (roadMat == null)
        {
            roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMat.SetColor("_BaseColor", new Color(0.2f, 0.2f, 0.22f)); // Dark asphalt
            roadMat.SetFloat("_Metallic", 0.1f);
            roadMat.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(roadMat, matPath);
        }
        return roadMat;
    }

    // ============================================================
    //  4. ENVIRONMENT DECORATION (Kenney Props)
    // ============================================================
    static void DecorateTrackWithProps(Material roadMat)
    {
        string[] tiles = { "Tile_001", "Tile_002", "Tile_003" };
        string[] propFBXs = {
            "Assets/Environment/Props/barrel.fbx",
            "Assets/Environment/Props/bag.fbx",
            "Assets/Environment/Props/coconut.fbx",
            "Assets/Environment/Props/watermelon.fbx"
        };

        foreach (string tileName in tiles)
        {
            GameObject tile = GameObject.Find(tileName);
            if (tile == null) continue;

            // Apply road material to the plane
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileRenderer.sharedMaterial = roadMat;
            }

            // Remove any old props to prevent duplicates
            List<GameObject> oldProps = new List<GameObject>();
            for (int i = 0; i < tile.transform.childCount; i++)
            {
                GameObject child = tile.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Prop_"))
                {
                    oldProps.Add(child);
                }
            }
            foreach (GameObject op in oldProps) Object.DestroyImmediate(op);

            // Spawn props along the left and right sides of each tile
            // Track is width 6m (-3 to +3), so sides are around x = -3.5f and x = 3.5f
            float[] xCoords = { -3.5f, 3.5f };
            float[] zCoords = { 5f, 15f, 25f };

            int propIndex = 0;
            foreach (float z in zCoords)
            {
                foreach (float x in xCoords)
                {
                    string fbxPath = propFBXs[propIndex % propFBXs.Length];
                    propIndex++;

                    GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (fbxAsset == null) continue;

                    GameObject prop = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                    prop.name = "Prop_" + fbxAsset.name;
                    prop.transform.SetParent(tile.transform);

                    // Place relative to parent tile
                    prop.transform.localPosition = new Vector3(x, 0f, z);
                    prop.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    prop.transform.localScale    = new Vector3(6f, 6f, 6f); // Scale up appropriately
                }
            }
            EditorUtility.SetDirty(tile);
        }
    }

    // ============================================================
    //  5. POST PROCESSING VOLUME CONFIGURATION
    // ============================================================
    static void SetupPostProcessing()
    {
        GameObject volumeObj = GameObject.Find("Global Volume");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global Volume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;

            // Create URP Profile
            string profilePath = "Assets/Settings/GameplayPostProfile.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);

                // Add Bloom (for gold/glow)
                Bloom bloom = profile.Add<Bloom>(true);
                bloom.threshold.Override(0.9f);
                bloom.intensity.Override(0.5f);

                // Add Color Adjustments (for saturation and contrast)
                ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
                colorAdj.contrast.Override(15f);
                colorAdj.saturation.Override(20f);
            }

            volume.sharedProfile = profile;
            EditorUtility.SetDirty(volumeObj);
            Debug.Log("✅ URP Post-Processing Volume configured.");
        }
    }

    // ============================================================
    //  6. COIN PREFAB POLISH
    // ============================================================
    static void UpdateCoinPrefab(Material goldMat)
    {
        string coinPath = "Assets/Prefabs/Coins/Coin.prefab";
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coinPath);

        if (coinPrefab != null)
        {
            Renderer r = coinPrefab.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = goldMat;
                EditorUtility.SetDirty(coinPrefab);
                Debug.Log("✅ Coin prefab set to Gold.");
            }
        }
    }
}
