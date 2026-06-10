using UnityEngine;

/// <summary>
/// Quản lý âm thanh mở/đóng cho cửa và ngăn kéo.
/// Gắn vào cùng GameObject với DoubleDoorInteractable hoặc DrawerInteract,
/// sau đó gọi PlayOpen() / PlayClose() từ script tương tác.
/// </summary>
public class DoorAudioController : MonoBehaviour
{
    [Header("Âm thanh")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Cài đặt âm lượng & 3D")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.8f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
    }

    public void PlayOpen() => Play(openSound);
    public void PlayClose() => Play(closeSound);

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.Stop();
        audioSource.PlayOneShot(clip, volume);
    }
}