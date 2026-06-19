using UnityEngine;
using UnityEngine.Events;

public class Generator : MonoBehaviour, IInteractable
{
    [Header("Cau hinh nhien lieu")]
    public int requiredCans = 2;
    public int currentFuel = 0;

    [Header("Trang thai")]
    public bool isPowered = false;

    [Header("Hieu ung (co the de trong)")]
    public AudioSource generatorAudio;
    public ParticleSystem exhaustSmoke;

    [Header("Su kien khi du xang")]
    public UnityEvent onPowerOn;

    public string GetInteractPrompt()
    {
        if (isPowered) return "May phat da duoc kich hoat";
        return "Nhan E de do xang (" + FuelInventory.cansHeld + "/" + requiredCans + ")";
    }

    public void Interact()
    {
        if (isPowered)
        {
            Debug.Log("May phat da duoc kich hoat tu truoc.");
            return;
        }

        if (!FuelInventory.HasCans(requiredCans))
        {
            Debug.Log($"Chua du xang de kich hoat may phat. Dang co {FuelInventory.cansHeld}/{requiredCans} can xang.");
            return;
        }

        if (!FuelInventory.TryConsumeCans(requiredCans))
        {
            Debug.LogWarning("Khong the tru can xang mac du da kiem tra du so luong.");
            return;
        }

        currentFuel = requiredCans;
        Debug.Log("Da do du xang: " + currentFuel + "/" + requiredCans);
        PowerOn();
    }

    private void PowerOn()
    {
        if (isPowered) return;

        isPowered = true;
        if (generatorAudio != null) generatorAudio.Play();
        if (exhaustSmoke != null) exhaustSmoke.Play();
        onPowerOn?.Invoke();
        Debug.Log("Máy phát đã được kích hoạt");
    }
}
