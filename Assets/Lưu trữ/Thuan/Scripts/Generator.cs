using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;   // de doc input bang Input System moi

public class Generator : MonoBehaviour, IInteractable
{
    [Header("Cau hinh nhien lieu")]
    public int requiredCans = 2;   // so binh can cam de bat dau do

    [Header("Trang thai")]
    public bool isPowered = false;

    [Header("Thanh xang (bam chuot trai)")]
    public Slider fuelBar;                 // keo FuelFillBar vao day
    public float fillPerPress = 0.06f;     // moi lan bam chuot tang bao nhieu
    public float drainPerSecond = 0.25f;   // ngung bam thi tut bao nhieu/giay

    [Header("Hieu ung (co the de trong)")]
    public AudioSource generatorAudio;
    public ParticleSystem exhaustSmoke;

    [Header("Su kien khi du xang")]
    public UnityEvent onPowerOn;

    private bool isFilling = false;   // dang trong qua trinh do xang chua
    private float fillAmount = 0f;    // do day hien tai (0 -> 1)

    public string GetInteractPrompt()
    {
        if (isPowered) return "";
        if (isFilling) return "Bam chuot trai lien tuc de do xang!";
        return "Nhan E de bat dau do xang";
    }

    // Bam E de bat dau qua trinh do
    public void Interact()
    {
        if (isPowered || isFilling) return;

        if (FuelInventory.cansHeld < requiredCans)
        {
            Debug.Log("Chua du binh xang! Can " + requiredCans + " binh.");
            return;
        }

        isFilling = true;
        fillAmount = 0f;
        if (fuelBar != null)
        {
            fuelBar.value = 0f;
            fuelBar.gameObject.SetActive(true);   // hien thanh
        }
        Debug.Log("Bat dau do xang. Bam chuot trai lien tuc!");
    }

    void Update()
    {
        if (!isFilling || isPowered) return;

        // Bam chuot trai (bam-nha lien tuc) -> tang; khong bam -> tut dan
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            fillAmount += fillPerPress;
        }
        else
        {
            fillAmount -= drainPerSecond * Time.deltaTime;
        }

        fillAmount = Mathf.Clamp01(fillAmount);
        if (fuelBar != null) fuelBar.value = fillAmount;

        // Day thanh -> bat dien
        if (fillAmount >= 1f)
            PowerOn();
    }

    void PowerOn()
    {
        isPowered = true;
        isFilling = false;

        if (fuelBar != null) fuelBar.gameObject.SetActive(false);   // an thanh
        FuelInventory.cansHeld -= requiredCans;   // tru so binh da dung

        if (generatorAudio != null) generatorAudio.Play();
        if (exhaustSmoke != null) exhaustSmoke.Play();
        onPowerOn.Invoke();
        Debug.Log("Du xang! May phat chay, den bat.");
    }
}