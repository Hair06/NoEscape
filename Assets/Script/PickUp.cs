using UnityEngine;
using TMPro;

public class PickUp : MonoBehaviour
{
    [Header("Cấu hình nhặt")]
    [SerializeField] private GameObject itemInHand;
    [SerializeField] private float pickupDistance = 3.0f;

    [Header("Tên item trên hotbar")]
    [Tooltip("Tên phải khớp với AltarSeal và Icon Library, ví dụ: ConMat")]
    [SerializeField] private string itemName = "ConMat";

    [Header("UI hướng dẫn (TextMeshPro)")]
    [Tooltip("Kéo Canvas prompt dùng chung vào đây")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Con Mắt Giáo Phái";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;
    [Range(0f, 1f)]
    [SerializeField] private float collectVolume = 1f;

    private Transform playerTransform;
    private bool isPlayerNearby = false;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= pickupDistance)
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;

                // Hien chu goi y
                if (promptText != null)
                {
                    promptText.text = interactMessage;
                    promptText.gameObject.SetActive(true);
                }

                Debug.Log("👉 Đến gần vật phẩm. Nhấn E để nhặt!");
            }

            if (CheckKeyE())
            {
                PickUpItem();
            }
        }
        else
        {
            if (isPlayerNearby)
            {
                isPlayerNearby = false;

                // An chu goi y khi di xa
                if (promptText != null) promptText.gameObject.SetActive(false);
            }
        }
    }

    private bool CheckKeyE()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            return UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame;
        }
        return false;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void PickUpItem()
    {
        if (itemInHand != null)
        {
            itemInHand.SetActive(true);
        }

        // Phat tieng nhat
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);

        // Them vao hotbar
        PlayerInventory.Add(itemName);

        // An chu goi y
        if (promptText != null) promptText.gameObject.SetActive(false);

        Debug.Log("🎒 Đã nhặt vật phẩm thành công: " + itemName);
        Destroy(gameObject);
    }
}