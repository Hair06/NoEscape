using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gắn vào Object đĩa hoàn chỉnh 3D (hiện ra sau khi ghép xong mini game).
// Người chơi tới gần nhấn E để nhặt -> đĩa biến mất -> báo hộp nhạc.
public class DiscPartCollect : MonoBehaviour
{
    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Đĩa Nhạc";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    private bool isPlayerInside = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside
            && Keyboard.current != null
            && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectPart();
        }
    }

    private void CollectPart()
    {
        // Phát âm thanh nhặt tại vị trí đĩa
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Báo về hộp nhạc: đã có Đĩa Nhạc
        if (MusicBoxRestore.Instance != null)
            MusicBoxRestore.Instance.CollectPart(MusicBoxRestore.MusicBoxPart.Disc);

        Debug.Log("Đã nhặt Đĩa Nhạc!");

        // Ẩn chữ và hủy đĩa
        if (promptText != null) promptText.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = interactMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}