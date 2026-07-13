using TMPro;
using UnityEngine;

public class FuelCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private GameObject uiToHide;

    [Header("Máy phát")]
    [SerializeField] private Generator generator;

    [Header("Cấu hình")]
    [SerializeField] private string fuelItemName = "Xang";
    [SerializeField] private int requiredCans = 2;

    private int lastFuelCount = -1;
    private bool uiHidden;

    private void Start()
    {
        // Không đặt số can về 0 tại đây vì PlayerInventory tự quản lý.
        UpdateFuelText();
    }

    private void Update()
    {
        if (generator != null && generator.IsPowered)
        {
            HideFuelUI();
            return;
        }

        int currentFuelCount =
            PlayerInventory.Count(fuelItemName);

        // Chỉ cập nhật UI khi số lượng thay đổi.
        if (currentFuelCount != lastFuelCount)
        {
            UpdateFuelText();
        }
    }

    private void UpdateFuelText()
    {
        int currentFuelCount =
            PlayerInventory.Count(fuelItemName);

        lastFuelCount = currentFuelCount;

        if (fuelText != null)
        {
            fuelText.text =
                $"Can xăng: {currentFuelCount}/{requiredCans}";
        }
    }

    private void HideFuelUI()
    {
        if (uiHidden)
        {
            return;
        }

        uiHidden = true;

        if (uiToHide != null)
        {
            uiToHide.SetActive(false);
        }
        else if (fuelText != null)
        {
            fuelText.gameObject.SetActive(false);
        }
    }
}