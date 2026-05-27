using UnityEngine;

public class PlayerFlashlightController : MonoBehaviour
{
    [Header("Flashlight Reference")]
    [SerializeField] private GameObject handFlashlight; // Kéo Object Đèn trên tay vào đây
    [SerializeField] private Light flashlightLight;    // Kéo thành phần Spotlight vào đây

    private bool hasFlashlight = false; // Check xem đã nhặt đèn chưa
    private bool isLightOn = false;     // Check xem đèn đang bật hay tắt

    void Start()
    {
        // Đảm bảo lúc đầu game đèn trên tay phải ẩn đi
        if (handFlashlight != null) handFlashlight.SetActive(false);
        if (flashlightLight != null) flashlightLight.enabled = false;
    }

    void Update()
    {
        // Nếu chưa nhặt được đèn thì không cho làm gì cả
        if (!hasFlashlight) return;

        // Nhấn phím F (hoặc đổi thành KeyCode.Mouse0 nếu muốn dùng chuột trái) để bật/tắt
        if (GameInputBridge.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }

    // Hàm này được gọi từ script FlashlightItem dưới đất sau khi nhấn E
    public void EquipFlashlight()
    {
        hasFlashlight = true;
        if (handFlashlight != null)
        {
            handFlashlight.SetActive(true); // Hiện đèn pin trên tay lên
        }
        
        // Mặc định sau khi nhặt sẽ tự động bật đèn lên luôn
        isLightOn = true;
        if (flashlightLight != null) flashlightLight.enabled = true;
    }

    // Logic Bật / Tắt ánh sáng đèn
    private void ToggleFlashlight()
    {
        isLightOn = !isLightOn;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isLightOn;
            
            // Bạn có thể thêm âm thanh click tại đây nếu có:
            // AudioSource.PlayClipAtPoint(clickSound, transform.position);
            Debug.Log(isLightOn ? "Đã bật đèn pin" : "Đã tắt đèn pin");
        }
    }
}
