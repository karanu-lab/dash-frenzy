using UnityEngine;
using UnityEditor;
using System.IO;

public class UltimateMaterialAndLightingFixer : MonoBehaviour
{
    [MenuItem("Dash Frenzy/Fix Grey Models & Skybox (FINAL)")]
    public static void RunUltimateFix()
    {
        EditorUtility.DisplayProgressBar("Final Fix", "Fixing FBX Material Links...", 0.1f);
        FixFBXMaterials();

        EditorUtility.DisplayProgressBar("Final Fix", "Fixing Skybox and Lighting...", 0.6f);
        FixLightingAndSkybox();

        EditorUtility.ClearProgressBar();
        Debug.Log("✅ Ultimate Fix Complete! Press Play!");
        EditorUtility.DisplayDialog("Fix Applied", "FBX materials and Skybox have been completely fixed.\n\nThe models will now be colorful, and the sky will be blue.\n\nPress Play!", "OK");
    }

    static void FixFBXMaterials()
    {
        int count = 0;
        string[] fbxGuids = AssetDatabase.FindAssets("t:Model");

        foreach (string guid in fbxGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.ToLower().EndsWith(".fbx")) continue;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                bool needsReimport = false;

                // Unity 6 API for material import
                if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
                {
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                    needsReimport = true;
                }

                if (importer.materialLocation != ModelImporterMaterialLocation.External)
                {
                    importer.materialLocation = ModelImporterMaterialLocation.External;
                    needsReimport = true;
                }

                if (importer.materialName != ModelImporterMaterialName.BasedOnMaterialName)
                {
                    importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                    needsReimport = true;
                }

                if (importer.materialSearch != ModelImporterMaterialSearch.Everywhere)
                {
                    importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
                    needsReimport = true;
                }

                if (needsReimport)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        Debug.Log("✅ Re-linked materials for " + count + " FBX models.");
    }

    static void FixLightingAndSkybox()
    {
        // 1. Force the Directional Light to point downwards for a blue daytime sky
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
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f); // High noon / mid-day angle
            dirLight.color = new Color(1f, 0.95f, 0.9f);
            dirLight.intensity = 1.3f;
            EditorUtility.SetDirty(dirLight.gameObject);
        }

        // 2. Force the Skybox settings
        string matPath = "Assets/Settings/CartoonSkybox.mat";
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (skyMat != null)
        {
            skyMat.SetColor("_SkyTint", new Color(0.3f, 0.5f, 0.85f));      // Deep Sky Blue
            skyMat.SetColor("_GroundColor", new Color(0.6f, 0.7f, 0.8f));   // Light Blue Horizon (NOT yellow/green!)
            skyMat.SetFloat("_SunSize", 0.04f);
            skyMat.SetFloat("_AtmosphereThickness", 1.0f);
            skyMat.SetFloat("_Exposure", 1.3f);
            EditorUtility.SetDirty(skyMat);

            RenderSettings.skybox = skyMat;
            RenderSettings.sun = dirLight; // Bind sun so procedural sky calculates color correctly!
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.75f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.85f, 0.85f, 0.8f);
            RenderSettings.ambientGroundColor = new Color(0.4f, 0.35f, 0.3f);
            DynamicGI.UpdateEnvironment();
        }

        // 3. Force Camera to use Skybox
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            EditorUtility.SetDirty(Camera.main.gameObject);
        }

        // 4. Tone down Post Processing Saturation which might be turning things green!
        UnityEngine.Rendering.Volume volume = Object.FindAnyObjectByType<UnityEngine.Rendering.Volume>();
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet(out UnityEngine.Rendering.Universal.ColorAdjustments colorAdj))
            {
                colorAdj.saturation.Override(5f); // 20 was too high, causing radioactive greens
                colorAdj.contrast.Override(10f);
                EditorUtility.SetDirty(volume.profile);
            }
        }
    }
}
