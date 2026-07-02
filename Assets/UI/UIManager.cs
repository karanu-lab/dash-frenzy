// ============================================================
//  UIManager.cs
//  Attach to: UICanvas in Gameplay scene
//  Handles: All HUD updates — score, coins, hearts, power-up indicator
//
//  NOTE ON TEXT COMPONENTS:
//  This script uses legacy UnityEngine.UI.Text.
//  If your project uses TextMeshPro (default in Unity 6), change:
//    - "using UnityEngine.UI;" → "using TMPro;"
//    - "public Text scoreText;"  → "public TextMeshProUGUI scoreText;"
//  Everything else stays the same.
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HUD - Score & Coins")]
    public Text scoreText;         // Top center
    public Text coinText;          // Coin counter

    [Header("HUD - Lives")]
    public Image[] heartImages;   // Drag 3 heart UI Images here in Inspector

    [Header("Power-Up Indicator")]
    public GameObject powerUpPanel;      // Panel that shows/hides
    public Text       powerUpNameText;   // e.g. "SHIELD"
    public Text       powerUpTimerText;  // e.g. "5s"

    [Header("Pause Panel")]
    public GameObject pausePanel;        // Panel shown when paused

    // Internal timer for power-up countdown
    private float powerUpTimer   = 0f;
    private bool  powerUpRunning = false;

    // ----------------------------------------------------------
    void Awake() { instance = this; }

    void Start()
    {
        // Hide panels at start
        if (powerUpPanel != null) powerUpPanel.SetActive(false);
        if (pausePanel   != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // Count down and hide the power-up indicator when it expires
        if (powerUpRunning && powerUpTimer > 0)
        {
            powerUpTimer -= Time.deltaTime;

            if (powerUpTimerText != null)
                powerUpTimerText.text = Mathf.CeilToInt(powerUpTimer) + "s";

            if (powerUpTimer <= 0)
                HidePowerUpIndicator();
        }
    }

    // ----------------------------------------------------------
    //  SCORE & COINS
    // ----------------------------------------------------------
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void UpdateCoins(int coins)
    {
        if (coinText != null)
            coinText.text = "x " + coins;
    }

    // ----------------------------------------------------------
    //  HEARTS / LIVES
    // ----------------------------------------------------------
    public void UpdateLives(int lives)
    {
        if (heartImages == null) return;

        // Enable hearts up to 'lives', disable the rest
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
                heartImages[i].enabled = (i < lives);
        }
    }

    // ----------------------------------------------------------
    //  POWER-UP INDICATOR
    // ----------------------------------------------------------
    public void ShowPowerUpIndicator(string name, float duration)
    {
        if (powerUpPanel    != null) powerUpPanel.SetActive(true);
        if (powerUpNameText != null) powerUpNameText.text = name;

        if (duration > 0)
        {
            powerUpTimer   = duration;
            powerUpRunning = true;
        }
        else
        {
            // Timed display — just show name with no countdown (for Shield which has no duration)
            powerUpRunning = false;
            if (powerUpTimerText != null) powerUpTimerText.text = "";
        }
    }

    public void HidePowerUpIndicator()
    {
        powerUpRunning = false;
        if (powerUpPanel != null) powerUpPanel.SetActive(false);
    }

    // ----------------------------------------------------------
    //  PAUSE BUTTON (wire OnClick to this in Inspector)
    // ----------------------------------------------------------
    public void TogglePause()
    {
        if (GameManager.instance.isPlaying)
        {
            GameManager.instance.PauseGame();
            if (pausePanel != null) pausePanel.SetActive(true);
        }
        else
        {
            GameManager.instance.ResumeGame();
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }
}
