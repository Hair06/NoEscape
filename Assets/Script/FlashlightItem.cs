using UnityEngine;

public class FlashlightItem : MonoBehaviour, IInteractable
{
    public string promptMessage = "Nhấn E để nhặt Đèn Pin";

    public string GetInteractPrompt()
    {
        return promptMessage;
    }

    public void Interact()
    {
        // Tìm script điều khiển đèn trên người Player
        PlayerFlashlightController playerFlashlight = FindFirstObjectByType<PlayerFlashlightController>();

        if (playerFlashlight != null)
        {
            playerFlashlight.EquipFlashlight(); // Kích hoạt đèn trên tay
            Debug.Log("Đã nhặt và trang bị đèn pin lên tay!");
            Destroy(gameObject); // Xóa cây đèn dưới đất
        }
    }
}