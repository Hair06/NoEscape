using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Cau hinh item")]
    public string itemName = "Xang";              // ten item (vd: Xang, BoPhan1...)
    public string prompt = "Nhan E de nhat";      // chu hien khi den gan

    private bool taken = false;

    public string GetInteractPrompt()
    {
        return prompt;
    }

    public void Interact()
    {
        if (taken) return;
        taken = true;

        PlayerInventory.Add(itemName);
        Debug.Log("Da nhat: " + itemName);

        // Nhiệm vụ mở đầu (chapter 0): tìm đủ 2 can xăng.
        if (itemName == "Xang" && QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportProgressForChapter(0, 0, 1, 2);
        }

        Destroy(gameObject);
    }
}