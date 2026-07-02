# Dash Frenzy — UI & Game Polish Handoff Guide
**BSD 2307: Mobile Gaming Programming | Project Hand-Off**

---

## 🎮 Current Project Status (Where We Are)

The core game logic, physics engine, and endless runner mechanics are **100% complete and fully functional**:
* **Compilation**: `0` errors, `0` warnings under Unity 6 (Universal 3D / URP).
* **Endless Track**: Infinite road scrolling and cycling is implemented.
* **Movement**: Lane switching (arrows/swipes), jumping, and sliding work smoothly.
* **Physics & Collisions**: Frictionless collision solver applied. Max stays at `Z = 0` and is not pushed or launched.
* **Gameplay Loop**: Collecting coins, taking damage from obstacles, power-ups timing, and lives counting all function.
* **Automated Setup Tool**: An editor utility window is included to set up scenes instantly.

---

## 👥 UI & Art Tasks (What Your Team Needs to Finish)

The game currently uses colored 3D primitives (grey capsule for Max, red cubes for obstacles, gold cylinders for coins, colored spheres for power-up orbs). Your team is responsible for styling the UI and replacing these placeholders with final art assets.

### 1. UI Styling & Polish (UI Team)
* **Design & Typography**: Style the Canvas layouts in all 3 scenes (`MainMenu`, `Gameplay`, `GameOver`) to match the colorful cartoon aesthetic.
* **TextMeshPro (Optional but Recommended)**: The scripts currently use Unity's legacy `Text` class to ensure compatibility. If your UI team uses **TextMeshPro**, simply open [UIManager.cs](file:///C:/Users/user/Desktop/Dash%20frenzy/Dashfrenzy2.0/Assets/UI/UIManager.cs), [MainMenuUI.cs](file:///C:/Users/user/Desktop/Dash%20frenzy/Dashfrenzy2.0/Assets/UI/MainMenuUI.cs), and [GameOverUI.cs](file:///C:/Users/user/Desktop/Dash%20frenzy/Dashfrenzy2.0/Assets/UI/GameOverUI.cs) and change:
  - `using UnityEngine.UI;` ➔ `using TMPro;`
  - `public Text scoreText;` ➔ `public TextMeshProUGUI scoreText;`
* **Hearts Display**: Style the `HeartsPanel` in the `Gameplay` HUD. Drag your custom heart sprites (Images) into the `Heart Images` array on `UIManager`.
* **Power-Up Indicator**: Customize the `PowerUpPanel` at the bottom of the screen to display active buffs (e.g. Magnet, Shield, SpeedBoost, Multiplier) with a circular fill or slider.
* **Pause Panel**: Design a nice menu popup that pauses the game when the pause button is clicked.

### 2. Replace 3D Assets (Art Team)
To replace the placeholder objects, simply edit the prefabs in the `Assets/Prefabs/` folder or swap models in the scene:
* **Max (Player)**: Replace the Capsule mesh on the `Max` GameObject with your 3D Max character model. Keep the parent `Max` object's scripts ([PlayerController.cs](file:///C:/Users/user/Desktop/Dash%20frenzy/Dashfrenzy2.0/Assets/Player/PlayerController.cs), [SwipeDetector.cs](file:///C:/Users/user/Desktop/Dash%20frenzy/Dashfrenzy2.0/Assets/Player/SwipeDetector.cs), and `Rigidbody`).
* **Obstacles**: Replace the red cube prefab in `Assets/Prefabs/Obstacles/Obstacle.prefab` with your 3D models (market stalls, barricades, crates, traffic signs).
* **Coins**: Replace `Assets/Prefabs/Coins/Coin.prefab` with an animated, rotating 3D gold coin.
* **Power-Ups**: Replace spheres in `Assets/Prefabs/PowerUps/` with themed orbs:
  - **Magnet**: Magnet icon/model (tag: `magnet`)
  - **Shield**: Shield bubble icon/model (tag: `shield`)
  - **SpeedBoost**: Lightning bolt icon/model (tag: `speedboost`)
  - **Multiplier**: 2X multiplier icon/model (tag: `Multiplier`)

---

## 🛠️ How to Use the Setup Helper

If you make new prefabs or reset the `Gameplay` scene, you can set up everything instantly:
1. In the top Unity menu, click **`Dash Frenzy` ➔ `Setup Gameplay Scene`**.
2. This editor utility script will instantly:
   - Create and organize folder paths.
   - Generate all necessary prefabs if missing.
   - Auto-tag the tiles as `ground` and the player as `Player`.
   - Set up the Spawners (`ObstacleSpawner`, `CoinSpawner`, `PowerUpSpawner`) and wire their prefab references automatically.
   - Wire your `ScoreText` and `CoinText` components directly to `UIManager`.

---

## 🔗 Code Architecture Quick-Reference

* **`PlayerController.cs`**: Handles inputs, lane movement, jumps, slides, and collision triggers.
* **`TrackManager.cs`**: Cycles 3 road tiles endlessly as the player stands still.
* **`PowerUpManager.cs`**: Manages active coroutines for Magnet (pulling coins), Shield (absorbing hit), Speed Boost (increasing speed multiplier), and Score Multiplier.
* **`SpeedController.cs`**: Slowly scales the game speed up over time (every 30s) to increase difficulty.
* **`GameManager.cs`**: Tracks play/pause/game-over states and scene loading.
