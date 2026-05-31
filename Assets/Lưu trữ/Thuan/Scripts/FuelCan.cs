using UnityEngine;

public class FuelCan : MonoBehaviour, IInteractable
{
    private bool taken = false;

    public string GetInteractPrompt()
    {
        return "Nhan E de nhat binh xang";
    }

    public void Interact()
    {
        if (taken) return;
        taken = true;

        FuelInventory.cansHeld++;
        Debug.Log("Da nhat xang. Dang cam: " + FuelInventory.cansHeld);
        Destroy(gameObject);
    }
}