using UnityEngine;

public class FuelCan : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt()
    {
        return "Nhan E de nhat binh xang";
    }

    public void Interact()
    {
        FuelInventory.cansHeld++;
        Debug.Log("Da nhat xang. Dang cam: " + FuelInventory.cansHeld);
        Destroy(gameObject);   // huy binh xang sau khi nhat
    }
}