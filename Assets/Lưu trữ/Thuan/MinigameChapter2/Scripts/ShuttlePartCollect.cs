using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gắn vào Object Con Thoi Nhạc (bộ phận 1 của hộp nhạc).
// Object này tắt sẵn, chỉ bật khi gỡ hết băng keo (TapePeelPuzzle bật lên).
public class ShuttlePartCollect : MonoBehaviour
{
    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Con Thoi Nhạc";

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
        // Phát âm thanh nhặt tại vị trí Con Thoi
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Báo về hộp nhạc: đã có bộ phận Con Thoi
        if (MusicBoxRestore.Instance != null)
            MusicBoxRestore.Instance.CollectPart(MusicBoxRestore.MusicBoxPart.Shuttle);

        Debug.Log("Đã nhặt Con Thoi Nhạc! (Bộ phận 1/4)");

        // Ẩn chữ và hủy Object
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