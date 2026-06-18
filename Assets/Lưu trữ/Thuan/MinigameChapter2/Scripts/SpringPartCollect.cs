using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Bản đơn giản để test: tới gần nhấn E nhặt Lò Xo Nhạc -> báo hộp nhạc.
// (Sau này sẽ nâng cấp thêm soi đèn pin gầm giường + đọc thư H.N.)
public class SpringPartCollect : MonoBehaviour
{
    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Lò Xo Nhạc";

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

        // Báo về hộp nhạc: đã có Lò Xo Nhạc
        if (MusicBoxRestore.Instance != null)
            MusicBoxRestore.Instance.CollectPart(MusicBoxRestore.MusicBoxPart.Spring);

        Debug.Log("Đã nhặt Lò Xo Nhạc!");

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