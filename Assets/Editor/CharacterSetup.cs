// ============================================================
//  CharacterSetup.cs
//  Location: Assets/Editor/
//  Access: Dash Frenzy > Setup Character & Assets
//
//  Automates the full import of:
//    - X Bot Mixamo character + 4 animations
//    - Kenney city road obstacles
//    - Kenney food items as market props
//  Then builds the AnimatorController and wires it to the player.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class CharacterSetup : MonoBehaviour
{
    // ---- Source paths on Desktop ----
    static readonly string SRC_CHAR   = @"C:\Users\user\Desktop\MaxCharacter";
    static readonly string SRC_ROADS  = @"C:\Users\user\Desktop\KennyAssests\kenney_city-kit-roads\Models\FBX format";
    static readonly string SRC_FOOD   = @"C:\Users\user\Desktop\KennyAssests\kenney_food-kit\Models\FBX format";

    // ---- Destination folders inside Unity Assets ----
    static readonly string DEST_CHAR      = "Assets/Characters/Max";
    static readonly string DEST_OBSTACLES = "Assets/Environment/Obstacles";
    static readonly string DEST_PROPS     = "Assets/Environment/Props";
    static readonly string DEST_ANIM      = "Assets/Characters/Max/Animations";
    static readonly string DEST_CTRL      = "Assets/Characters/Max/MaxAnimator.controller";

    // ============================================================
    [MenuItem("Dash Frenzy/Setup Character & Assets")]
    public static void RunSetup()
    {
        EditorUtility.DisplayProgressBar("Dash Frenzy Setup", "Creating folders...", 0.05f);

        CreateFolders();
        CopyCharacterFBX();
        CopyObstacleFBX();
        CopyFoodProps();
        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar("Dash Frenzy Setup", "Building Animator Controller...", 0.7f);
        AnimatorController ctrl = BuildAnimatorController();

        EditorUtility.DisplayProgressBar("Dash Frenzy Setup", "Setting up Player prefab...", 0.85f);
        SetupPlayerPrefab(ctrl);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "✅ X Bot character imported\n" +
            "✅ Animations configured (Run / Jump / Slide / FallOver)\n" +
            "✅ AnimatorController created at Assets/Characters/Max/\n" +
            "✅ Kenney road & food models imported\n" +
            "✅ Player prefab updated\n\n" +
            "Next: Open Gameplay scene and run\n" +
            "Dash Frenzy > Setup Gameplay Scene\n" +
            "to replace obstacle prefabs with Kenney models.",
            "Let's go!");
    }

    // ============================================================
    //  FOLDERS
    // ============================================================
    static void CreateFolders()
    {
        EnsureFolder("Assets/Characters");
        EnsureFolder("Assets/Characters/Max");
        EnsureFolder("Assets/Characters/Max/Animations");
        EnsureFolder("Assets/Environment");
        EnsureFolder("Assets/Environment/Obstacles");
        EnsureFolder("Assets/Environment/Props");
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    // ============================================================
    //  COPY CHARACTER FBX FILES
    // ============================================================
    static void CopyCharacterFBX()
    {
        // Map: source filename -> destination filename
        var files = new System.Collections.Generic.Dictionary<string, string>
        {
            { "X Bot@Running.fbx",       "Max_Run.fbx"      },
            { "X Bot@Jumping.fbx",       "Max_Jump.fbx"     },
            { "X Bot@Running Slide.fbx", "Max_Slide.fbx"    },
            { "X Bot@Fall Over.fbx",     "Max_FallOver.fbx" },
        };

        foreach (var kv in files)
        {
            string src  = Path.Combine(SRC_CHAR, kv.Key);
            string dest = $"{DEST_CHAR}/{kv.Value}";

            if (!File.Exists(src))
            {
                Debug.LogWarning($"⚠️ Source not found: {src}");
                continue;
            }

            File.Copy(src, Application.dataPath + dest.Replace("Assets", ""), true);
            Debug.Log($"✅ Copied: {kv.Value}");
        }
    }

    // ============================================================
    //  COPY KENNEY OBSTACLE MODELS (road barriers + construction)
    // ============================================================
    static void CopyObstacleFBX()
    {
        // Select road barriers — look like natural market street obstacles
        string[] picks = {
            "construction-barrier.fbx",
            "road-straight-barrier.fbx",
            "road-side-barrier.fbx",
        };

        foreach (string file in picks)
        {
            string src  = Path.Combine(SRC_ROADS, file);
            string dest = Application.dataPath + DEST_OBSTACLES.Replace("Assets", "") + "/" + file;

            if (!File.Exists(src)) { Debug.LogWarning($"⚠️ Not found: {src}"); continue; }
            File.Copy(src, dest, true);
            Debug.Log($"✅ Obstacle copied: {file}");
        }
    }

    // ============================================================
    //  COPY KENNEY FOOD PROPS (market stall scatter props)
    // ============================================================
    static void CopyFoodProps()
    {
        // Market-themed items that fit a Nairobi street market
        string[] picks = {
            "banana.fbx",
            "watermelon.fbx",
            "pineapple.fbx",
            "coconut.fbx",
            "barrel.fbx",
            "bag.fbx",
            "tomato.fbx",
            "corn.fbx",
        };

        foreach (string file in picks)
        {
            string src  = Path.Combine(SRC_FOOD, file);
            string dest = Application.dataPath + DEST_PROPS.Replace("Assets", "") + "/" + file;

            if (!File.Exists(src)) { Debug.LogWarning($"⚠️ Not found: {src}"); continue; }
            File.Copy(src, dest, true);
            Debug.Log($"✅ Prop copied: {file}");
        }
    }

    // ============================================================
    //  BUILD ANIMATOR CONTROLLER
    //  States: Run (default) -> Jump -> Slide -> FallOver
    // ============================================================
    static AnimatorController BuildAnimatorController()
    {
        // Create or load existing controller
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(DEST_CTRL);
        if (ctrl == null)
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(DEST_CTRL);

        ctrl.parameters = System.Array.Empty<AnimatorControllerParameter>();

        // ---- Parameters ----
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsSliding",  AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hit",        AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);

        var root = ctrl.layers[0].stateMachine;
        root.states = System.Array.Empty<ChildAnimatorState>();
        root.anyStateTransitions = System.Array.Empty<AnimatorStateTransition>();
        root.entryTransitions = System.Array.Empty<AnimatorTransition>();

        // ---- Load animation clips ----
        AssetDatabase.Refresh();
        AnimationClip clipRun      = LoadClip($"{DEST_CHAR}/Max_Run.fbx");
        AnimationClip clipJump     = LoadClip($"{DEST_CHAR}/Max_Jump.fbx");
        AnimationClip clipSlide    = LoadClip($"{DEST_CHAR}/Max_Slide.fbx");
        AnimationClip clipFallOver = LoadClip($"{DEST_CHAR}/Max_FallOver.fbx");

        // ---- States ----
        AnimatorState stateRun      = root.AddState("Run",      new Vector3(250, 0));
        AnimatorState stateAir      = root.AddState("InAir",    new Vector3(250, 100));
        AnimatorState stateSlide    = root.AddState("Slide",    new Vector3(250, 200));
        AnimatorState stateFallOver = root.AddState("FallOver", new Vector3(250, 300));

        if (clipRun      != null) stateRun.motion      = clipRun;
        if (clipJump     != null) stateAir.motion      = clipJump;
        if (clipSlide    != null) stateSlide.motion    = clipSlide;
        if (clipFallOver != null) stateFallOver.motion = clipFallOver;

        root.defaultState = stateRun;

        // ---- Transitions ----
        // Run -> InAir (on Jump trigger)
        AddTransition(stateRun, stateAir, "Jump", isTrigger: true);

        // InAir -> Run (when IsGrounded = true)
        AddBoolTransition(stateAir, stateRun, "IsGrounded", true);

        // Run -> Slide (when IsSliding = true)
        AddBoolTransition(stateRun, stateSlide, "IsSliding", true);

        // Slide -> Run (when IsSliding = false)
        AddBoolTransition(stateSlide, stateRun, "IsSliding", false);

        // Any -> FallOver (on Hit trigger)
        AnimatorStateTransition hitTrans = root.AddAnyStateTransition(stateFallOver);
        hitTrans.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        hitTrans.hasExitTime   = false;
        hitTrans.duration      = 0.1f;
        hitTrans.canTransitionToSelf = false;

        // FallOver -> Run (after animation finishes)
        AnimatorStateTransition recoverTrans = stateFallOver.AddTransition(stateRun);
        recoverTrans.hasExitTime = true;
        recoverTrans.exitTime    = 1f; // Play full fall animation
        recoverTrans.duration    = 0.3f;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ AnimatorController built: " + DEST_CTRL);
        return ctrl;
    }

    // ============================================================
    //  WIRE CONTROLLER TO PLAYER PREFAB
    // ============================================================
    static void SetupPlayerPrefab(AnimatorController ctrl)
    {
        // Find existing Player object in scene or prefab
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("⚠️ No GameObject tagged 'Player' found in scene. " +
                             "Open the Gameplay scene and re-run this setup.");
            return;
        }

        // Find or add Animator on the player root (or its first child mesh)
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim == null)
            anim = player.AddComponent<Animator>();

        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion           = false; // Let PlayerController handle movement
        anim.updateMode                = AnimatorUpdateMode.Normal;

        Debug.Log($"✅ Animator wired to: {player.name}");
    }

    // ============================================================
    //  HELPERS
    // ============================================================
    static AnimationClip LoadClip(string fbxPath)
    {
        // FBX files contain sub-assets; we need the AnimationClip sub-asset
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (Object a in assets)
        {
            if (a is AnimationClip clip && !clip.name.Contains("__preview__"))
                return clip;
        }
        Debug.LogWarning($"⚠️ No AnimationClip found in: {fbxPath}");
        return null;
    }

    static AnimatorStateTransition AddTransition(AnimatorState from, AnimatorState to,
                                                  string param, bool isTrigger = false,
                                                  bool boolVal = true)
    {
        AnimatorStateTransition t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = 0.1f;
        if (isTrigger)
            t.AddCondition(AnimatorConditionMode.If, 0, param);
        return t;
    }

    static AnimatorStateTransition AddBoolTransition(AnimatorState from, AnimatorState to,
                                                      string param, bool val)
    {
        AnimatorStateTransition t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = 0.1f;
        t.AddCondition(val ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
        return t;
    }
}
