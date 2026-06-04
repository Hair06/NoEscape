using UnityEngine;
using TMPro;

public class FuelCounterUI : MonoBehaviour
{
    public TextMeshProUGUI fuelText;
    public Generator generator;        // keo object Generator vao day
    public GameObject uiToHide;        // object UI se an di khi xong

    void Start()
    {
        FuelInventory.cansHeld = 0;
    }

    void Update()
    {
        // Khi may phat du xang -> an UI, xem nhu hoan thanh
        if (generator != null && generator.isPowered)
        {
            if (uiToHide != null) uiToHide.SetActive(false);
            return;
        }

        if (fuelText != null)
            fuelText.text = " " + FuelInventory.cansHeld;
    }
}