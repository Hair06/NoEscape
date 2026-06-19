using UnityEngine;

public class FuelCan : MonoBehaviour, IInteractable
{
    private bool taken = false;

    public string GetInteractPrompt()
    {
        return "Nhan E de nhat can xang";
    }

    public void Interact()
    {
        if (taken) return;
        taken = true;

        FuelInventory.AddCan();
        Debug.Log($"Da nhat can xang '{name}'. Dang cam: {FuelInventory.cansHeld}");
        Destroy(gameObject);
    }
}
