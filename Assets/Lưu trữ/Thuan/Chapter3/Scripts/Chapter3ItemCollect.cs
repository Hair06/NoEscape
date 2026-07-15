using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gan vao Trai Tim / Giot Mau (vat pham chuong 3).
// Nguoi choi toi gan, hien chu, nhan E de nhat.
public class Chapter3ItemCollect : MonoBehaviour
{
    [Header("Cau hinh vat pham")]
    [Tooltip("Ten item tren hotbar: TraiTim hoac GiotMau")]
    [SerializeField] private string itemName = "TraiTim";

    [Header("UI hướng dẫn (TextMeshPro)")]
    [Tooltip("Keo Canvas prompt (cai dung o chuong 2) vao day")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Trái Tim Giáo Phái";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    private bool isPlayerInside = false;
    private bool taken = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && !taken
            && Keyboard.current != null
            && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (taken) return;
        taken = true;

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Them vao hotbar
        PlayerInventory.Add(itemName);

        // Bao ve ban tho phong an (se lam o buoc sau)
        // if (AltarSeal.Instance != null)
        //     AltarSeal.Instance.CollectItem(itemName);

        Debug.Log("Đã nhặt: " + itemName);

        if (promptText != null) promptText.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !taken)
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