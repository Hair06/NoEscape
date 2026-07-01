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
        PlayerFlashlightController playerFlashlight = FindFirstObjectByType<PlayerFlashlightController>();

        if (playerFlashlight != null)
        {
            playerFlashlight.EquipFlashlight();
            PlayerInventory.hasFlashlight = true;   // danh dau da co den pin
            Debug.Log("Đã nhặt và trang bị đèn pin lên tay!");
            Destroy(gameObject);
        }
    }
}