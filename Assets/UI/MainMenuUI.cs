// ============================================================
//  MainMenuUI.cs
//  Attach to: MainMenu Canvas in MainMenu scene
//  Handles: Button logic for the main menu screen
//
//  HOW TO WIRE BUTTONS IN INSPECTOR:
//  1. Select each Button in the Hierarchy
//  2. In the Inspector → Button → On Click (+)
//  3. Drag this script's GameObject into the slot
//  4. Select the matching method from the dropdown
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Optional - High Score Display on Menu")]
    public Text highScoreText;   // Wire a Text element in Inspector to show best score

    // ----------------------------------------------------------
    void Start()
    {
        // Play Main Menu background music
        if (AudioManager.instance != null)
            AudioManager.instance.PlayMainMenuMusic();

        // Show the best score on the main menu if a display is wired up
        if (highScoreText != null)
        {
            int best = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "Best: " + best;
        }
    }

    // ----------------------------------------------------------
    //  Wire these to your buttons in the Inspector (On Click)
    // ----------------------------------------------------------
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void OnSettingsButtonClicked()
    {
        // Settings panel — expand this later if needed
        Debug.Log("Settings clicked — not yet implemented");
    }

    public void OnLeaderboardButtonClicked()
    {
        int best = PlayerPrefs.GetInt("HighScore", 0);
        Debug.Log("Leaderboard — Local High Score: " + best);
        // You can show a UI panel with the high score here
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
        Debug.Log("Quit (only works in built APK, not in Editor)");
    }
}
