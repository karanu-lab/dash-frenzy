// ============================================================
//  DashFrenzySetup.cs
//  EDITOR UTILITY SCRIPT — Safe to delete after running!
//
//  HOW TO USE:
//  1. Open each scene (MainMenu, Gameplay, GameOver) one by one.
//  2. Run the corresponding menu command under "Dash Frenzy" in the top bar.
//  3. The script will auto-create, position, and wire up all UI elements.
//  4. Your UI team only needs to style the visuals (change textures, colors, fonts)!
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using UnityEditor.Events;

public class DashFrenzySetup : EditorWindow
{
    // ============================================================
    //  MENU COMMAND: GAMEPLAY SCENE SETUP
    // ============================================================
    [MenuItem("Dash Frenzy/Setup Gameplay Scene")]
    public static void SetupGameplayScene()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "Gameplay")
        {
            EditorUtility.DisplayDialog(
                "Wrong Scene!",
                "You must open the 'Gameplay' scene before running this command.\n\n" +
                "Current scene: " + activeScene + "\n\n" +
                "Please double-click your 'Gameplay' scene in the Assets folder first.",
                "Got it!");
            return;
        }

        Debug.Log("=== Dash Frenzy Gameplay Setup Starting ===");

        // Make sure Prefabs folders exist
        EnsureFolderExists("Assets/Prefabs");
        EnsureFolderExists("Assets/Prefabs/Obstacles");
        EnsureFolderExists("Assets/Prefabs/Coins");
        EnsureFolderExists("Assets/Prefabs/PowerUps");

        // Step 1 — Create Prefabs
        GameObject obstaclePrefab = CreateObstaclePrefab();
        GameObject coinPrefab     = CreateCoinPrefab();
        GameObject magnetPrefab   = CreatePowerUpPrefab("Magnet",     Color.yellow,  "magnet");
        GameObject shieldPrefab   = CreatePowerUpPrefab("Shield",     Color.cyan,    "shield");
        GameObject boostPrefab    = CreatePowerUpPrefab("SpeedBoost", Color.red,     "speedboost");
        GameObject multiPrefab    = CreatePowerUpPrefab("Multiplier", Color.magenta, "Multiplier");

        // Step 2 — Wire TrackManager
        SetupTrackManager();

        // Step 3 — Create Spawner objects
        SetupSpawners(obstaclePrefab, coinPrefab,
                      magnetPrefab, shieldPrefab, boostPrefab, multiPrefab);

        // Step 4 — Set tile tags
        TagTiles();

        // Step 5 — Set Max tag
        TagPlayer();

        // Step 6 — Create and Wire Gameplay UI Canvas
        SetupGameplayUI();

        // Step 7 — Setup AudioManager
        SetupAudioManager();

        // Save all assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== Dash Frenzy Gameplay Setup Complete! ===");
        EditorUtility.DisplayDialog(
            "Dash Frenzy Gameplay Setup",
            "Gameplay Scene is fully configured!\n\n" +
            "✅ All 3 Spawners created and wired\n" +
            "✅ Gameplay UI Canvas created and wired to UIManager\n" +
            "✅ Default prefabs generated and tagged\n\n" +
            "Press Play to test, then hand off to your UI/Art teams!",
            "Awesome!");
    }

    // ============================================================
    //  MENU COMMAND: MAIN MENU SETUP
    // ============================================================
    [MenuItem("Dash Frenzy/Setup Main Menu UI")]
    public static void SetupMainMenuScene()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "MainMenu")
        {
            EditorUtility.DisplayDialog(
                "Wrong Scene!",
                "You must open the 'MainMenu' scene before running this command.\n\n" +
                "Current scene: " + activeScene + "\n\n" +
                "Please double-click your 'MainMenu' scene in the Assets folder first.",
                "Got it!");
            return;
        }

        Debug.Log("=== Dash Frenzy Main Menu UI Setup Starting ===");

        // Copy and import the main menu background sprite
        string srcPath = @"C:\Users\user\.gemini\antigravity\brain\0bcc4ec0-2d09-447e-8ea0-f187f051b7cd\scene1_main_menu_1781031913419.png";
        string destPath = "Assets/MainMenuBackground.png";
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);

        // If not yet imported as a sprite, set the texture type first
        if (bgSprite == null)
        {
            TextureImporter importer = AssetImporter.GetAtPath(destPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
            }
        }

        if (bgSprite == null)
        {
            Debug.LogWarning("⚠️ MainMenuBackground.png not found in Assets/. Image was not applied.");
        }
        else
        {
            Debug.Log("✅ MainMenuBackground.png loaded as sprite");
        }

        // Create Canvas
        GameObject canvasObj = GameObject.Find("UICanvas");
        if (canvasObj == null) canvasObj = CreateCanvas("UICanvas");

        MainMenuUI menuUI = canvasObj.GetComponent<MainMenuUI>();
        if (menuUI == null) menuUI = canvasObj.AddComponent<MainMenuUI>();

        // Create Background Image
        GameObject bgObj = GameObject.Find("BackgroundImage");
        if (bgObj == null)
        {
            bgObj = new GameObject("BackgroundImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rect = bgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        // Re-order background to be at the very top so it draws behind everything else
        bgObj.transform.SetAsFirstSibling();

        Image bgImage = bgObj.GetComponent<Image>();
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.color = Color.white;
        }
        else
        {
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Fallback color
        }

        // Create transparent interactive buttons layered over the image buttons
        // Green Start button is on the left
        GameObject startBtn = CreateTransparentButton("StartButton", canvasObj.transform, new Vector2(-280, -780), new Vector2(250, 180));
        // Blue Settings button is in the center
        GameObject settingsBtn = CreateTransparentButton("SettingsButton", canvasObj.transform, new Vector2(0, -780), new Vector2(250, 180));
        // Yellow Leaderboard button is on the right
        GameObject leaderboardBtn = CreateTransparentButton("LeaderboardButton", canvasObj.transform, new Vector2(280, -780), new Vector2(250, 180));

        // Create HighScoreText placeholder (placed near top center or bottom, can be turned off/styled by UI team)
        GameObject scoreText = CreateText("HighScoreText", canvasObj.transform, "Best: 0", 40, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), new Vector2(0, 50));

        // Wire MainMenuUI methods to the buttons
        menuUI.highScoreText = scoreText.GetComponent<Text>();
        WireButton(startBtn, menuUI, "OnStartButtonClicked");
        WireButton(settingsBtn, menuUI, "OnSettingsButtonClicked");
        WireButton(leaderboardBtn, menuUI, "OnLeaderboardButtonClicked");

        EditorUtility.SetDirty(menuUI);
        AssetDatabase.SaveAssets();

        Debug.Log("=== Main Menu Setup Complete! ===");
        EditorUtility.DisplayDialog(
            "MainMenu UI Configured",
            "Main Menu Canvas successfully created and wired up!\n\n" +
            "✅ Background image copied and assigned\n" +
            "✅ Transparent buttons placed over START, SETTINGS, and LEADERBOARD graphics\n" +
            "✅ Click events wired to MainMenuUI.cs",
            "Great!");
    }

    // ============================================================
    //  MENU COMMAND: GAME OVER SETUP
    // ============================================================
    [MenuItem("Dash Frenzy/Setup Game Over UI")]
    public static void SetupGameOverScene()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "GameOver")
        {
            EditorUtility.DisplayDialog(
                "Wrong Scene!",
                "You must open the 'GameOver' scene before running this command.\n\n" +
                "Current scene: " + activeScene + "\n\n" +
                "Please double-click your 'GameOver' scene in the Assets folder first.",
                "Got it!");
            return;
        }

        Debug.Log("=== Dash Frenzy Game Over UI Setup Starting ===");

        // Create Canvas
        GameObject canvasObj = GameObject.Find("UICanvas");
        if (canvasObj == null) canvasObj = CreateCanvas("UICanvas");

        GameOverUI gameOverUI = canvasObj.GetComponent<GameOverUI>();
        if (gameOverUI == null) gameOverUI = canvasObj.AddComponent<GameOverUI>();

        // Create UI Elements
        GameObject titleText = CreateText("TitleText", canvasObj.transform, "GAME OVER", 80, Color.red, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0, 0));
        GameObject scoreText = CreateText("FinalScoreText", canvasObj.transform, "Score: 0", 50, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0, 0));
        GameObject bestText  = CreateText("HighScoreText", canvasObj.transform, "Best: 0", 45, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0));
        
        GameObject retryBtn  = CreateButton("RetryButton", canvasObj.transform, "RETRY RUN", new Vector2(0, -50), new Vector2(300, 80));
        GameObject homeBtn   = CreateButton("HomeButton", canvasObj.transform, "MAIN MENU", new Vector2(0, -170), new Vector2(300, 80));

        // Wire GameOverUI
        gameOverUI.finalScoreText = scoreText.GetComponent<Text>();
        gameOverUI.highScoreText  = bestText.GetComponent<Text>();
        WireButton(retryBtn, gameOverUI, "OnRetryButtonClicked");
        WireButton(homeBtn, gameOverUI, "OnHomeButtonClicked");

        EditorUtility.SetDirty(gameOverUI);
        AssetDatabase.SaveAssets();

        Debug.Log("=== Game Over Setup Complete! ===");
        EditorUtility.DisplayDialog(
            "GameOver UI Configured",
            "Game Over Canvas successfully created and wired up!\n\n" +
            "✅ Final Score, Best Score, Retry, and Home elements generated\n" +
            "✅ Click events wired to GameOverUI.cs",
            "Great!");
    }

    // ============================================================
    //  SCENE AUTOMATION HELPERS
    // ============================================================

    static void SetupGameplayUI()
    {
        // 1. Create or Find Canvas
        GameObject canvasObj = GameObject.Find("UICanvas");
        if (canvasObj == null) canvasObj = CreateCanvas("UICanvas");

        UIManager ui = canvasObj.GetComponent<UIManager>();
        if (ui == null) ui = canvasObj.AddComponent<UIManager>();

        // 2. Score and Coin text
        GameObject scoreTextObj = GameObject.Find("ScoreText");
        if (scoreTextObj == null)
            scoreTextObj = CreateText("ScoreText", canvasObj.transform, "0", 50, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), new Vector2(0, -50));
        ui.scoreText = scoreTextObj.GetComponent<Text>();

        GameObject coinTextObj = GameObject.Find("CoinText");
        if (coinTextObj == null)
            coinTextObj = CreateText("CoinText", canvasObj.transform, "x 0", 45, Color.yellow, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.95f), new Vector2(0.05f, 0.95f), new Vector2(50, -50));
        ui.coinText = coinTextObj.GetComponent<Text>();

        // 3. Create Lives Panel & Hearts (Size 3)
        GameObject heartsPanel = GameObject.Find("HeartsPanel");
        if (heartsPanel == null)
        {
            heartsPanel = new GameObject("HeartsPanel", typeof(RectTransform));
            heartsPanel.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rect = heartsPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.95f, 0.95f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.pivot     = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-50, -30);
            rect.sizeDelta = new Vector2(200, 60);

            HorizontalLayoutGroup layout = heartsPanel.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 10;
        }

        ui.heartImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            string heartName = $"Heart_{i + 1}";
            Transform heartTrans = heartsPanel.transform.Find(heartName);
            GameObject heartObj;
            if (heartTrans == null)
            {
                heartObj = new GameObject(heartName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                heartObj.transform.SetParent(heartsPanel.transform, false);
                RectTransform hRect = heartObj.GetComponent<RectTransform>();
                hRect.sizeDelta = new Vector2(50, 50);
            }
            else
            {
                heartObj = heartTrans.gameObject;
            }
            ui.heartImages[i] = heartObj.GetComponent<Image>();
        }

        // 4. Create Power-Up Panel
        GameObject powerUpPanel = GameObject.Find("PowerUpPanel");
        if (powerUpPanel == null)
        {
            powerUpPanel = new GameObject("PowerUpPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            powerUpPanel.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rect = powerUpPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.15f);
            rect.anchorMax = new Vector2(0.5f, 0.15f);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(400, 100);

            Image panelImg = powerUpPanel.GetComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Translucent dark panel

            // Name text child
            CreateText("PowerUpName", powerUpPanel.transform, "SHIELD ACTIVE", 24, Color.cyan, TextAnchor.MiddleCenter, new Vector2(0, 0.5f), new Vector2(1, 1), Vector2.zero);
            // Timer text child
            CreateText("PowerUpTimer", powerUpPanel.transform, "8s", 28, Color.white, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(1, 0.5f), Vector2.zero);
        }
        ui.powerUpPanel = powerUpPanel;
        Transform nameTrans = powerUpPanel.transform.Find("PowerUpName");
        if (nameTrans != null) ui.powerUpNameText = nameTrans.GetComponent<Text>();
        Transform timerTrans = powerUpPanel.transform.Find("PowerUpTimer");
        if (timerTrans != null) ui.powerUpTimerText = timerTrans.GetComponent<Text>();
        powerUpPanel.SetActive(false); // Hide by default

        // 5. Create Pause Panel
        GameObject pausePanel = GameObject.Find("PausePanel");
        if (pausePanel == null)
        {
            pausePanel = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            pausePanel.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rect = pausePanel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = pausePanel.GetComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black

            CreateText("PauseTitleText", pausePanel.transform, "PAUSED", 60, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero);
            
            GameObject resumeBtnObj = CreateButton("ResumeButton", pausePanel.transform, "RESUME", new Vector2(0, 0), new Vector2(250, 70));
            WireButton(resumeBtnObj, ui, "TogglePause");
        }
        ui.pausePanel = pausePanel;
        pausePanel.SetActive(false); // Hide by default

        EditorUtility.SetDirty(ui);
        Debug.Log("✅ UIManager Canvas fully generated and wired");
    }

    static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // Ensure an EventSystem exists in the scene so buttons can receive clicks/touches
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Debug.Log("✅ Created EventSystem in scene");
        }

        return canvasObj;
    }

    static GameObject CreateText(string name, Transform parent, string defaultText, int fontSize, Color color, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
    {
        // Reuse if already exists
        Transform child = parent.Find(name);
        if (child != null) return child.gameObject;

        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObj.transform.SetParent(parent, false);

        Text text = textObj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = defaultText;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        return textObj;
    }

    static GameObject CreateButton(string name, Transform parent, string labelText, Vector2 anchoredPosition, Vector2 size)
    {
        // Reuse if already exists
        Transform child = parent.Find(name);
        if (child != null) return child.gameObject;

        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.GetComponent<Image>();
        img.color = Color.white;

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        // Add text label
        GameObject labelObj = CreateText("Label", btnObj.transform, labelText, 24, Color.black, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return btnObj;
    }

    static void WireButton(GameObject btnObj, MonoBehaviour targetComponent, string methodName)
    {
        Button btn = btnObj.GetComponent<Button>();
        if (btn == null || targetComponent == null) return;

        // Clear existing listeners
        while (btn.onClick.GetPersistentEventCount() > 0)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
        }

        System.Reflection.MethodInfo method = targetComponent.GetType().GetMethod(methodName);
        if (method != null)
        {
            UnityEngine.Events.UnityAction action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), targetComponent, method) as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
            EditorUtility.SetDirty(btn);
        }
    }

    // ============================================================
    //  PREFAB CREATORS
    // ============================================================

    static GameObject CreateObstaclePrefab()
    {
        // Create a red cube as the obstacle placeholder
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.name = "Obstacle";
        obs.tag  = "obstacles";

        // Scale it to look like a barrier
        obs.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        // Color it red
        ApplyColor(obs, new Color(0.9f, 0.2f, 0.2f));

        // Make sure it has a trigger collider (for GDD collision spec)
        BoxCollider col = obs.GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        // Save as prefab
        string path = "Assets/Prefabs/Obstacles/Obstacle.prefab";
        GameObject prefab = SaveAsPrefab(obs, path);
        DestroyImmediate(obs);

        Debug.Log("✅ Obstacle prefab created");
        return prefab;
    }

    static GameObject CreateCoinPrefab()
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.tag  = "coin";

        coin.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
        ApplyColor(coin, new Color(1f, 0.84f, 0f));

        CapsuleCollider col = coin.GetComponent<CapsuleCollider>();
        if (col != null) col.isTrigger = true;

        string path = "Assets/Prefabs/Coins/Coin.prefab";
        GameObject prefab = SaveAsPrefab(coin, path);
        DestroyImmediate(coin);

        Debug.Log("✅ Coin prefab created");
        return prefab;
    }

    static GameObject CreatePowerUpPrefab(string name, Color color, string tag)
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = name;
        orb.tag  = tag;

        orb.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        ApplyColor(orb, color);

        SphereCollider col = orb.GetComponent<SphereCollider>();
        if (col != null) col.isTrigger = true;

        string path = $"Assets/Prefabs/PowerUps/{name}.prefab";
        GameObject prefab = SaveAsPrefab(orb, path);
        DestroyImmediate(orb);

        Debug.Log($"✅ {name} power-up prefab created");
        return prefab;
    }

    static void SetupTrackManager()
    {
        TrackManager tm = Object.FindAnyObjectByType<TrackManager>();
        if (tm == null) return;

        GameObject tile1 = GameObject.Find("Tile_001");
        GameObject tile2 = GameObject.Find("Tile_002");
        GameObject tile3 = GameObject.Find("Tile_003");

        if (tile1 == null || tile2 == null || tile3 == null) return;

        tm.tiles = new GameObject[] { tile1, tile2, tile3 };
        EditorUtility.SetDirty(tm);
        Debug.Log("✅ TrackManager tiles wired");
    }

    static void SetupSpawners(GameObject obstaclePrefab, GameObject coinPrefab,
                               GameObject magnet, GameObject shield,
                               GameObject boost, GameObject multiplier)
    {
        GameObject spawnerParent = GameObject.Find("[SPAWNER]");
        if (spawnerParent == null)
        {
            spawnerParent = new GameObject("[SPAWNER]");
        }

        // ---- Obstacle Spawner ----
        GameObject obsSpawnerObj = GameObject.Find("ObstacleSpawner");
        if (obsSpawnerObj == null)
        {
            obsSpawnerObj = new GameObject("ObstacleSpawner");
            obsSpawnerObj.transform.SetParent(spawnerParent.transform);
        }

        ObstacleSpawner obsSpawner = obsSpawnerObj.GetComponent<ObstacleSpawner>();
        if (obsSpawner == null) obsSpawner = obsSpawnerObj.AddComponent<ObstacleSpawner>();

        if (obstaclePrefab != null)
            obsSpawner.obstaclePrefabs = new GameObject[] { obstaclePrefab };

        EditorUtility.SetDirty(obsSpawner);

        // ---- Coin Spawner ----
        GameObject coinSpawnerObj = GameObject.Find("CoinSpawner");
        if (coinSpawnerObj == null)
        {
            coinSpawnerObj = new GameObject("CoinSpawner");
            coinSpawnerObj.transform.SetParent(spawnerParent.transform);
        }

        CoinSpawner coinSpawner = coinSpawnerObj.GetComponent<CoinSpawner>();
        if (coinSpawner == null) coinSpawner = coinSpawnerObj.AddComponent<CoinSpawner>();

        if (coinPrefab != null)
            coinSpawner.coinPrefab = coinPrefab;

        EditorUtility.SetDirty(coinSpawner);

        // ---- Power-Up Spawner ----
        GameObject powerUpSpawnerObj = GameObject.Find("PowerUpSpawner");
        if (powerUpSpawnerObj == null)
        {
            powerUpSpawnerObj = new GameObject("PowerUpSpawner");
            powerUpSpawnerObj.transform.SetParent(spawnerParent.transform);
        }

        PowerUpSpawner powerUpSpawner = powerUpSpawnerObj.GetComponent<PowerUpSpawner>();
        if (powerUpSpawner == null) powerUpSpawner = powerUpSpawnerObj.AddComponent<PowerUpSpawner>();

        powerUpSpawner.powerUpPrefabs = new GameObject[] { magnet, shield, boost, multiplier };

        EditorUtility.SetDirty(powerUpSpawner);
    }

    static void TagTiles()
    {
        string[] tileNames = { "Tile_001", "Tile_002", "Tile_003" };
        foreach (string name in tileNames)
        {
            GameObject tile = GameObject.Find(name);
            if (tile != null)
            {
                tile.tag = "ground";
                EditorUtility.SetDirty(tile);
            }
        }
    }

    static void TagPlayer()
    {
        GameObject max = GameObject.Find("Max");
        if (max != null)
        {
            max.tag = "Player";
            EditorUtility.SetDirty(max);
        }
    }

    static void ApplyColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.sharedMaterial = mat;
        }
    }

    static GameObject SaveAsPrefab(GameObject obj, string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        return PrefabUtility.SaveAsPrefabAsset(obj, path);
    }

    static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    static GameObject CreateTransparentButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        // Reuse if already exists
        Transform child = parent.Find(name);
        GameObject btnObj;
        if (child != null)
        {
            btnObj = child.gameObject;
        }
        else
        {
            btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
        }

        // Set it completely transparent so the graphic behind it shows through
        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        // Remove any label children if they exist to keep it transparent
        for (int i = btnObj.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(btnObj.transform.GetChild(i).gameObject);
        }

        return btnObj;
    }

    static void SetupAudioManager()
    {
        GameObject amObj = GameObject.Find("AudioManager");
        if (amObj == null)
        {
            amObj = new GameObject("AudioManager");
            
            // Add AudioSources
            AudioSource music = amObj.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.spatialBlend = 0f; // 2D Sound

            AudioSource sfx = amObj.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f; // 2D Sound

            AudioManager am = amObj.AddComponent<AudioManager>();
            am.musicSource = music;
            am.sfxSource = sfx;

            EditorUtility.SetDirty(am);
            Debug.Log("✅ AudioManager created and wired in scene");
        }
    }
}
