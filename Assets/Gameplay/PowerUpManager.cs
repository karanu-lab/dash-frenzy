// ============================================================
//  PowerUpManager.cs
//  Attach to: PowerUpManager GameObject under [MANAGERS]
//  Handles: All 4 power-up effects via coroutines
//           Magnet, Shield, SpeedBoost, Score Multiplier
// ============================================================

using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager instance;

    void Awake() { instance = this; }

    // ----------------------------------------------------------
    //  Called by PlayerController when player touches a power-up orb
    // ----------------------------------------------------------
    public void ActivatePowerUp(GameObject powerUpObject)
    {
        string type = powerUpObject.tag;   // Each prefab must have its correct Tag set
        Destroy(powerUpObject);            // Remove the orb from the scene

        switch (type)
        {
            case "magnet":     StartCoroutine(RunMagnet(8f));     break;
            case "shield":     StartCoroutine(RunShield(8f));     break;
            case "speedboost": StartCoroutine(RunSpeedBoost(6f)); break;
            case "Multiplier": StartCoroutine(RunMultiplier(10f)); break;
        }
    }

    // ----------------------------------------------------------
    //  MAGNET — pulls nearby coins toward the player for 8 seconds
    // ----------------------------------------------------------
    IEnumerator RunMagnet(float duration)
    {
        if (UIManager.instance != null) UIManager.instance.ShowPowerUpIndicator("MAGNET", duration);

        float timer = duration;
        while (timer > 0)
        {
            PullCoinsToPlayer();
            timer -= Time.deltaTime;
            yield return null;  // Wait one frame then loop
        }
    }

    void PullCoinsToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        GameObject[] coins = GameObject.FindGameObjectsWithTag("coin");
        foreach (GameObject coin in coins)
        {
            float dist = Vector3.Distance(coin.transform.position, player.transform.position);
            if (dist < 6f)  // Only attract coins within 6 units
            {
                coin.transform.position = Vector3.MoveTowards(
                    coin.transform.position,
                    player.transform.position,
                    8f * Time.deltaTime);
            }
        }
    }

    // ----------------------------------------------------------
    //  SHIELD — absorbs one obstacle hit, or lasts 8 seconds
    // ----------------------------------------------------------
    IEnumerator RunShield(float duration)
    {
        if (LivesManager.instance != null) LivesManager.instance.ActivateShield();
        if (UIManager.instance != null) UIManager.instance.ShowPowerUpIndicator("SHIELD", duration);
        yield return new WaitForSeconds(duration);
        // Shield may have already been consumed by a hit; safe to call either way
        if (LivesManager.instance != null) LivesManager.instance.DeactivateShield();
    }

    // ----------------------------------------------------------
    //  SPEED BOOST — temporarily increases game speed by 50%
    // ----------------------------------------------------------
    IEnumerator RunSpeedBoost(float duration)
    {
        SpeedController.currentSpeed *= 1.5f;
        if (UIManager.instance != null) UIManager.instance.ShowPowerUpIndicator("BOOST!", duration);
        yield return new WaitForSeconds(duration);
        SpeedController.currentSpeed /= 1.5f;
    }

    // ----------------------------------------------------------
    //  SCORE MULTIPLIER — doubles all points earned for 10 seconds
    // ----------------------------------------------------------
    IEnumerator RunMultiplier(float duration)
    {
        if (ScoreManager.instance != null) ScoreManager.instance.SetMultiplier(2);
        if (UIManager.instance != null) UIManager.instance.ShowPowerUpIndicator("x2 SCORE", duration);
        yield return new WaitForSeconds(duration);
        if (ScoreManager.instance != null) ScoreManager.instance.SetMultiplier(1);
    }
}
