// ============================================================
//  GameOverUI.cs
//  Attach to: GameOver Canvas in GameOver scene
//  Handles: Shows final score, high score, and retry/home buttons
//
//  HOW TO WIRE BUTTONS:
//  Same as MainMenuUI — drag this script's GameObject into
//  each Button's On Click slot and pick the method.
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Score Display")]
    public Text finalScoreText;    // Shows the score from the run that just ended
    public Text highScoreText;     // Shows all-time best score

    // ----------------------------------------------------------
    void Start()
    {
        // Read scores saved by GameManager.GameOver()
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + lastScore;

        if (highScoreText != null)
            highScoreText.text = "Best: " + highScore;
    }

    // ----------------------------------------------------------
    //  Buttons
    // ----------------------------------------------------------
    public void OnRetryButtonClicked()
    {
        SceneManager.LoadScene("Gameplay");   // Restart the game
    }

    public void OnHomeButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");   // Back to main menu
    }
}
