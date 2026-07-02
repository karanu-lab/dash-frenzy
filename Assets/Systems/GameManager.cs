// ============================================================
//  GameManager.cs
//  Attach to: GameManager GameObject under [MANAGERS]
//  Handles: Overall game state — playing, paused, game over
//           All other scripts check GameManager.instance.isPlaying
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton — any script accesses this as GameManager.instance
    public static GameManager instance;

    [HideInInspector]
    public bool isPlaying = false;

    // ----------------------------------------------------------
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartGame();
    }

    // ----------------------------------------------------------
    //  GAME STATE CONTROL
    // ----------------------------------------------------------
    public void StartGame()
    {
        isPlaying        = true;
        Time.timeScale   = 1;           // Ensure game is not frozen
        SpeedController.ResetSpeed();   // Always start at base speed
    }

    public void PauseGame()
    {
        isPlaying      = false;
        Time.timeScale = 0;             // Freeze all physics and updates
    }

    public void ResumeGame()
    {
        isPlaying      = true;
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        isPlaying = false;

        // Play game over sound
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGameOverSFX();

        // Save this run's score for the Game Over screen to display
        int score = ScoreManager.instance.score;
        PlayerPrefs.SetInt("LastScore", score);

        // Update high score if beaten
        if (score > PlayerPrefs.GetInt("HighScore", 0))
            PlayerPrefs.SetInt("HighScore", score);

        // Short delay so the player sees the crash before scene changes
        Invoke("LoadGameOverScene", 1.5f);
    }

    // ----------------------------------------------------------
    //  SCENE NAVIGATION
    // ----------------------------------------------------------
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
