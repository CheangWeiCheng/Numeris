/*
* Author: Kwek Sin En
* Date: 10/02/2026
* Description: Manages background music and sound effects for VR game.
*/
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float musicVolume = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip openInventorySound;
    [SerializeField] private AudioClip useItemSound;
    [SerializeField] private AudioClip addItemSound;
    [SerializeField] private AudioClip loginSuccessful;
    [SerializeField] private AudioClip correctAnswer;
    [SerializeField] private AudioClip wrongAnswer;
    [SerializeField] private AudioClip laserBeam;
    [SerializeField] private float sfxVolume = 0.7f;

    private void Awake()
    {
        // Singleton pattern - ensure only one AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes

        InitializeAudioSources();
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }
    /// <summary>
    /// Initializes and configures the music and sound effect audio sources for the game object.
    /// </summary>
    private void InitializeAudioSources()
    {
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure music source
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        // Configure SFX source
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// Starts playing background music if not already playing
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stops background music
    /// </summary>
    public void StopBackgroundMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Plays login successful sound effect
    /// </summary>
    public void PlayLoginSuccessful(float duration = 0f)
    {
        PlaySoundEffect(loginSuccessful, duration);
    }

    /// <summary>
    /// Plays the open inventory sound effect
    /// </summary>
    /// <param name="duration">Duration in seconds (0 = full clip length)</param>
    public void PlayOpenInventory(float duration = 0f)
    {
        PlaySoundEffect(openInventorySound, duration);
    }

    /// <summary>
    /// Plays the use item sound effect
    /// </summary>
    /// <param name="duration">Duration in seconds (0 = full clip length)</param>
    public void PlayUseItem(float duration = 0f)
    {
        PlaySoundEffect(useItemSound, duration);
    }

    /// <summary>
    /// Plays the add item sound effect
    /// </summary>
    /// <param name="duration">Duration in seconds (0 = full clip length)</param>
    public void PlayAddItem(float duration = 3f)
    {
        PlaySoundEffect(addItemSound, duration);
    }

    /// <summary>
    /// Plays correct sound effect
    /// </summary>
    public void PlayCorrectAnswer(float duration = 0f)
    {
        PlaySoundEffect(correctAnswer, duration);
    }

    /// <summary>
    /// Plays wrong sound effect
    /// </summary>
    public void PlayWrongAnswer(float duration = 0f)
    {
        PlaySoundEffect(wrongAnswer, duration);
    }

    /// <summary>
    /// Plays laser sound effect
    /// </summary>
    public void PlayLaserBeam(float duration = 0f)
    {
        PlaySoundEffect(laserBeam, duration);
    }

    /// <summary>
    /// Plays a generic sound effect
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="duration">Duration in seconds (0 = full clip length)</param>
    public void PlaySoundEffect(AudioClip clip, float duration = 0f)
    {
        if (clip != null)
        {
            if (duration > 0f)
            {
                StartCoroutine(PlaySoundForDuration(clip, duration));
            }
            else
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// Coroutine to play a sound effect for a specific duration
    /// </summary>
    private System.Collections.IEnumerator PlaySoundForDuration(AudioClip clip, float duration)
    {
        sfxSource.clip = clip;
        sfxSource.Play();

        yield return new WaitForSeconds(duration);

        sfxSource.Stop();
        sfxSource.clip = null;
    }

    /// <summary>
    /// Sets the music volume (0.0 to 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Sets the sound effects volume (0.0 to 1.0)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// Toggles music on/off
    /// </summary>
    public void ToggleMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.UnPause();
        }
    }
}