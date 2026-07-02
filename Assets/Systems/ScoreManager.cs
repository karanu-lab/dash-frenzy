// ============================================================
//  ScoreManager.cs
//  Attach to: ScoreManager GameObject under [MANAGERS]
//  Handles: Score (distance-based), coins, score multiplier
//           High score is stored in PlayerPrefs (persists between sessions)
// ============================================================

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [HideInInspector] public int score      = 0;
    [HideInInspector] public int coins      = 0;
    [HideInInspector] public int multiplier = 1;

    private float distanceScore = 0f;   // Raw float score from distance travelled

    // ----------------------------------------------------------
    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isPlaying) return;

        // Distance score increases each frame based on speed and multiplier
        distanceScore += Time.deltaTime * 10f * multiplier;

        // Total score = distance + coin bonus
        score = (int)distanceScore + (coins * 10);

        // Push to HUD
        if (UIManager.instance != null)
            UIManager.instance.UpdateScore(score);
    }

    // ----------------------------------------------------------
    //  Called by PlayerController when player collects a coin
    // ----------------------------------------------------------
    public void AddCoin()
    {
        coins++;
        if (UIManager.instance != null)
            UIManager.instance.UpdateCoins(coins);
    }

    // ----------------------------------------------------------
    //  Called by PowerUpManager to apply/remove x2 multiplier
    // ----------------------------------------------------------
    public void SetMultiplier(int value)
    {
        multiplier = value;
    }
}
