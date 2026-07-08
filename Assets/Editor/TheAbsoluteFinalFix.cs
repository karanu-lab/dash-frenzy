using UnityEngine;
using UnityEditor;
using System.IO;

public class TheAbsoluteFinalFix : MonoBehaviour
{
    [MenuItem("Dash Frenzy/APPLY FINAL FIX (DO THIS)")]
    public static void RunFix()
    {
        EditorUtility.DisplayProgressBar("Final Fix", "Fixing Sky and Fog...", 0.1f);
        FixSkyAndFog();

        EditorUtility.DisplayProgressBar("Final Fix", "Force-Applying Materials to Prefabs...", 0.4f);
        ForceMaterialsOnPrefabs();

        EditorUtility.DisplayProgressBar("Final Fix", "Force-Applying Materials to Scene...", 0.8f);
        ForceMaterialsOnScene();

        EditorUtility.ClearProgressBar();
        Debug.Log("✅ ALL FIXED! Press Play!");
        EditorUtility.DisplayDialog("Fix Applied", "Fog removed, Skybox enforced, and Materials physically applied to all prefabs and scene objects.\n\nPress Play!", "OK");
    }

    static void FixSkyAndFog()
    {
        // 1. Turn off Fog (this was causing the green/yellow tint!)
        RenderSettings.fog = false;

        // 2. Fix the Skybox material
        string matPath = "Assets/Settings/CartoonSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (skyMat != null)
        {
            skyMat.SetColor("_SkyTint", new Color(0.2f, 0.5f, 0.9f));      // True Blue
            skyMat.SetColor("_GroundColor", new Color(0.6f, 0.7f, 0.8f));   // Light Blue Horizon
            EditorUtility.SetDirty(skyMat);
            RenderSettings.skybox = skyMat;
        }

        // 3. Remove post-processing volumes that might shift colors
        UnityEngine.Rendering.Volume[] volumes = Object.FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None);
        foreach (var v in volumes)
        {
            Object.DestroyImmediate(v.gameObject);
        }

        // 4. Ensure camera clears to Skybox
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            Camera.main.backgroundColor = new Color(0.2f, 0.5f, 0.9f); // Fallback blue
            EditorUtility.SetDirty(Camera.main.gameObject);
        }

        DynamicGI.UpdateEnvironment();
    }

    static void ForceMaterialsOnPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Obstacles", "Assets/Environment" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool modified = false;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                modified |= TryMapRendererMaterials(r);
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
    }

    static void ForceMaterialsOnScene()
    {
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Renderer r in renderers)
        {
            if (TryMapRendererMaterials(r))
            {
                EditorUtility.SetDirty(r.gameObject);
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    static bool TryMapRendererMaterials(Renderer r)
    {
        bool modified = false;
        Material[] mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == null) continue;

            string matName = mats[i].name.Replace(" (Instance)", "").Trim();
            
            // Clean up Trash Dash specific naming conventions
            if (matName.Contains("DumpsterGreen")) matName = "Dumpster";
            if (matName.Contains("DumpsterBlue")) matName = "Dumpster";
            if (matName.Contains("RoadWorksBarrier")) matName = "Construction";
            if (matName.Contains("RoadWorksCone")) matName = "Construction";
            if (matName.Contains("Wall")) matName = "BrickWall";
            if (matName.Contains("Wood")) matName = "WoodSlats";

            // Find the proper URP material we fixed earlier in Assets/Materials
            string[] found = AssetDatabase.FindAssets(matName + " t:Material", new[] { "Assets/Materials" });
            if (found.Length > 0)
            {
                Material properMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(found[0]));
                if (properMat != null && mats[i] != properMat)
                {
                    mats[i] = properMat;
                    modified = true;
                }
            }
        }

        if (modified)
        {
            r.sharedMaterials = mats;
        }
        return modified;
    }
}
