using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gắn vào Object Chìa Vặn (hiện ra sau khi mở hộp khóa ký hiệu).
// Người chơi tới gần nhấn E để nhặt -> chìa biến mất -> báo hộp nhạc.
public class WindKeyCollect : MonoBehaviour
{
    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Chìa Vặn";

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
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Báo về hộp nhạc: đã có Chìa Vặn
        if (MusicBoxRestore.Instance != null)
            MusicBoxRestore.Instance.CollectPart(MusicBoxRestore.MusicBoxPart.WindKey);

        Debug.Log("Đã nhặt Chìa Vặn!");

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