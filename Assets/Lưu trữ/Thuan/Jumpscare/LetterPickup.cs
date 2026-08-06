using UnityEngine;

// Lá thư đặt trong scene, nhấn E để nhặt vào túi đồ.
// Gắn vào object lá thư dưới đất (cần Collider tick Is Trigger).
public class LetterPickup : MonoBehaviour, IInteractable
{
    [Header("Tên vật phẩm lưu vào túi")]
    [Tooltip("Phải trùng với ô Item Name trong PhotoInspect")]
    [SerializeField] private string itemName = "LaThu";

    [Header("Chữ gợi ý khi đến gần")]
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt lá thư";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip pickupSound;

    private bool taken = false;

    public string GetInteractPrompt()
    {
        return taken ? "" : interactMessage;
    }

    public void Interact()
    {
        if (taken) return;
        taken = true;

        // Bỏ vào túi đồ
        PlayerInventory.Add(itemName);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Debug.Log("[LetterPickup] Đã nhặt lá thư. Nhấn 1 để xem.");

        Destroy(gameObject);
    }
}
