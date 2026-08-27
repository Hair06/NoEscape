using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderBinder : MonoBehaviour
{
    public enum VolumeType { Music, SFX }
    [Header("Chọn loại âm thanh cho Slider này")]
    public VolumeType volumeType;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (slider == null) slider = GetComponent<Slider>();

        // Load lại giá trị slider theo PlayerPrefs đã lưu
        if (volumeType == VolumeType.Music)
        {
            slider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }
        else if (volumeType == VolumeType.SFX)
        {
            slider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        // Lắng nghe sự kiện thay đổi Slider
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        if (AudioManager.Instance == null) return;

        if (volumeType == VolumeType.Music)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else if (volumeType == VolumeType.SFX)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
}