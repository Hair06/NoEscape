using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gan vao object Ky Tu Giao Phai (hiddenSignObject).
// Chi nhat duoc khi dang soi bang Con Mat (giu chuot phai).
public class CultSignCollect : MonoBehaviour
{
    [Header("Cấu hình vật phẩm")]
    [Tooltip("Tên item trên hotbar - phải khớp AltarSeal")]
    [SerializeField] private string itemName = "KiTu";
    [SerializeField] private float pickupDistance = 3f;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Ký Tự Giáo Phái";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    private Transform playerTransform;
    private bool taken = false;
    private bool promptShowing = false;

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (taken || playerTransform == null) return;

        // Chi hoat dong khi object dang hien (tuc dang soi bang Con Mat)
        if (!gameObject.activeInHierarchy) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= pickupDistance)
        {
            ShowPrompt(true);

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                CollectItem();
            }
        }
        else
        {
            ShowPrompt(false);
        }
    }

    private void ShowPrompt(bool state)
    {
        if (promptShowing == state) return;
        promptShowing = state;

        if (promptText != null)
        {
            if (state) promptText.text = interactMessage;
            promptText.gameObject.SetActive(state);
        }
    }

    private void OnDisable()
    {
        // Khi tha chuot phai, ky tu an di -> an luon prompt
        ShowPrompt(false);
    }

    private void CollectItem()
    {
        if (taken) return;
        taken = true;

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        PlayerInventory.Add(itemName);
        Debug.Log("Đã nhặt: " + itemName);

        ShowPrompt(false);
        Destroy(gameObject);
    }
}