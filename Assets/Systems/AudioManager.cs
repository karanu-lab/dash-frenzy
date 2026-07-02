// ============================================================
//  AudioManager.cs
//  Attach to: AudioManager GameObject
//  Handles: Background music loops and sound effects (SFX)
//           with null-safety and persistence across scenes.
// ============================================================

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Background Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip gameOverMusic;

    [Header("Sound Effect Clips")]
    public AudioClip coinCollectSFX;
    public AudioClip obstacleHitSFX;
    public AudioClip powerUpSFX;

    // ----------------------------------------------------------
    void Awake()
    {
        // Singleton pattern to ensure music persists across scene loads
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------------
    //  Music Controllers
    // ----------------------------------------------------------
    public void PlayMainMenuMusic()
    {
        if (musicSource == null || mainMenuMusic == null) return;
        if (musicSource.clip == mainMenuMusic && musicSource.isPlaying) return; // Already playing

        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (musicSource == null || gameplayMusic == null) return;
        if (musicSource.clip == gameplayMusic && musicSource.isPlaying) return; // Already playing

        musicSource.clip = gameplayMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // ----------------------------------------------------------
    //  SFX Player
    // ----------------------------------------------------------
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // Shortcuts for gameplay triggers:
    public void PlayCoinSFX()
    {
        PlaySFX(coinCollectSFX);
    }

    public void PlayObstacleSFX()
    {
        PlaySFX(obstacleHitSFX);
    }

    public void PlayPowerUpSFX()
    {
        PlaySFX(powerUpSFX);
    }

    public void PlayGameOverSFX()
    {
        StopMusic();
        if (gameOverMusic != null)
        {
            PlaySFX(gameOverMusic);
        }
    }
}
