// ============================================================
//  LivesManager.cs
//  Attach to: LivesManager GameObject under [MANAGERS]
//  Handles: 3-life system, shield absorption, game over trigger
// ============================================================

using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager instance;

    [Header("Lives")]
    public int maxLives = 3;
    private int currentLives;

    // Shield state — set by PowerUpManager
    private bool shieldActive = false;

    // ----------------------------------------------------------
    void Awake()
    {
        instance     = this;
        currentLives = maxLives;
    }

    // ----------------------------------------------------------
    //  Called by PlayerController when player hits an obstacle
    // ----------------------------------------------------------
    public void LoseLife()
    {
        // If shield is active, absorb the hit instead of losing a life
        if (shieldActive)
        {
            DeactivateShield();
            return;
        }

        currentLives--;
        if (UIManager.instance != null)
            UIManager.instance.UpdateLives(currentLives);   // Update heart icons on HUD

        if (currentLives <= 0 && GameManager.instance != null)
            GameManager.instance.GameOver();
    }

    // ----------------------------------------------------------
    //  Shield control — called by PowerUpManager
    // ----------------------------------------------------------
    public void ActivateShield()
    {
        shieldActive = true;
    }

    public void DeactivateShield()
    {
        shieldActive = false;
        if (UIManager.instance != null)
            UIManager.instance.HidePowerUpIndicator();
    }

    public bool IsShieldActive()
    {
        return shieldActive;
    }
}
