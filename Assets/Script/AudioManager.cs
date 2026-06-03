using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI Sounds")]
    public AudioClip buttonClick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (musicSource != null)
            musicSource.volume = volume;

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (sfxSource != null)
            sfxSource.volume = volume;

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClick != null)
        {
            sfxSource.PlayOneShot(buttonClick);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    private void LoadVolume()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }
}