using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.wholeNumbers = false;

            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;

            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        }
    }

    public void ChangeMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void ChangeSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }
}