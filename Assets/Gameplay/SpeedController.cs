// ============================================================
//  SpeedController.cs
//  Attach to: SpeedController GameObject in Gameplay scene
//  Handles: Increases game speed every 30 seconds
//           Speed is a public static — all scripts read it from here
// ============================================================

using UnityEngine;

public class SpeedController : MonoBehaviour
{
    // Static so any script can read: SpeedController.currentSpeed
    public static float currentSpeed = 8f;

    [Header("Speed Settings")]
    public float baseSpeed        = 8f;    // Starting speed
    public float maxSpeed         = 20f;   // Cap — game won't go faster than this
    public float speedIncrement   = 0.5f;  // How much to add each interval
    public float incrementInterval = 30f;  // Seconds between each speed increase

    // ----------------------------------------------------------
    void Start()
    {
        // Reset speed each time the Gameplay scene loads
        currentSpeed = baseSpeed;
        InvokeRepeating("IncreaseSpeed", incrementInterval, incrementInterval);
    }

    void IncreaseSpeed()
    {
        if (!GameManager.instance.isPlaying) return;

        if (currentSpeed < maxSpeed)
        {
            currentSpeed += speedIncrement;
            Debug.Log("Speed increased to: " + currentSpeed);
        }
    }

    // ----------------------------------------------------------
    //  Called by GameManager when restarting — resets to base
    // ----------------------------------------------------------
    public static void ResetSpeed()
    {
        currentSpeed = 8f;
    }
}
