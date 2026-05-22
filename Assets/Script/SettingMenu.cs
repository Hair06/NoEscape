using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Load value
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Add listener
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
    }

    public void ChangeMusicVolume(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void ChangeSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}