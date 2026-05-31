using UnityEngine;
using UnityEngine.Events;

public class Generator : MonoBehaviour, IInteractable
{
    [Header("Cau hinh nhien lieu")]
    public int requiredCans = 4;
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
        if (isPowered) return "";
        return "Nhan E de do xang (" + currentFuel + "/" + requiredCans + ")";
    }

    public void Interact()
    {
        if (isPowered) return;

        if (FuelInventory.cansHeld <= 0)
        {
            Debug.Log("Ban khong cam binh xang nao.");
            return;
        }

        FuelInventory.cansHeld--;
        currentFuel++;
        Debug.Log("Da do xang: " + currentFuel + "/" + requiredCans);

        if (currentFuel >= requiredCans)
            PowerOn();
    }

    void PowerOn()
    {
        isPowered = true;
        if (generatorAudio != null) generatorAudio.Play();
        if (exhaustSmoke != null) exhaustSmoke.Play();
        onPowerOn.Invoke();
        Debug.Log("Du xang! May phat chay, den bat.");
    }
}