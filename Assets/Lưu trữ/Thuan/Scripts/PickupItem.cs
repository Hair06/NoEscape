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
        Destroy(gameObject);
    }
}