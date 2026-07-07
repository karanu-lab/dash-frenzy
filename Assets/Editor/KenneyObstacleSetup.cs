// ============================================================
//  KenneyObstacleSetup.cs
//  Location: Assets/Editor/
//  Access: Dash Frenzy > Wire Kenney Obstacles
//
//  Builds tagged prefabs from Kenney FBX files already in
//  Assets/Environment/Obstacles/ and assigns them to the
//  ObstacleSpawner in the active Gameplay scene.
// ============================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class KenneyObstacleSetup
{
    [MenuItem("Dash Frenzy/Wire Kenney Obstacles")]
    public static void WireKenneyObstacles()
    {
        // Must be in Gameplay scene
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            EditorUtility.DisplayDialog("Wrong Scene!",
                "Open the 'Gameplay' scene first, then run this command.\n\nCurrent: " + activeScene,
                "Got it!");
            return;
        }

        // Ensure prefab folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Obstacles"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Obstacles");

        // ---- FBX files to turn into obstacle prefabs ----
        string[] fbxPaths = {
            "Assets/Environment/Obstacles/construction-barrier.fbx",
            "Assets/Environment/Obstacles/road-straight-barrier.fbx",
            "Assets/Environment/Obstacles/road-side-barrier.fbx",
        };

        var prefabList = new List<GameObject>();

        foreach (string fbxPath in fbxPaths)
        {
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogWarning("KenneySetup: FBX not found at " + fbxPath +
                                 " — run 'Dash Frenzy > Setup Character & Assets' first.");
                continue;
            }

            string modelName = Path.GetFileNameWithoutExtension(fbxPath);
            string prefabPath = "Assets/Prefabs/Obstacles/" + modelName + ".prefab";

            // Instantiate into scene temporarily so we can configure it
            GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
            temp.name = modelName;

            // Add a BoxCollider set to trigger — PlayerController reads this
            BoxCollider box = temp.GetComponent<BoxCollider>();
            if (box == null) box = temp.AddComponent<BoxCollider>();
            box.isTrigger = true;

            // Tag must match what PlayerController.OnTriggerEnter checks
            try { temp.tag = "obstacles"; }
            catch { Debug.LogWarning("Tag 'obstacles' not found. Add it in Project Settings > Tags."); }

            // Save as a proper prefab asset
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            Object.DestroyImmediate(temp);

            if (saved != null)
            {
                prefabList.Add(saved);
                Debug.Log("Obstacle prefab saved: " + prefabPath);
            }
        }

        if (prefabList.Count == 0)
        {
            EditorUtility.DisplayDialog("No Prefabs Created",
                "No FBX files were found in Assets/Environment/Obstacles/\n\n" +
                "Run 'Dash Frenzy > Setup Character & Assets' first to import the Kenney models.",
                "OK");
            return;
        }

        // ---- Find ObstacleSpawner in the scene and assign the new prefabs ----
        ObstacleSpawner spawner = Object.FindAnyObjectByType<ObstacleSpawner>();
        if (spawner != null)
        {
            spawner.obstaclePrefabs = prefabList.ToArray();
            EditorUtility.SetDirty(spawner);
            Debug.Log("ObstacleSpawner updated with " + prefabList.Count + " Kenney prefabs.");
        }
        else
        {
            Debug.LogWarning("No ObstacleSpawner found in scene. " +
                             "Run 'Dash Frenzy > Setup Gameplay Scene' first.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Done!",
            prefabList.Count + " Kenney obstacle prefabs created and wired to ObstacleSpawner.\n\n" +
            "Press Play — road barriers will now spawn instead of red cubes!",
            "Let's go!");
    }
}
